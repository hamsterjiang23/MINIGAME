using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    [CustomEditor(typeof(MiniGameAudioProfile))]
    public sealed class MiniGameAudioProfileEditor : UnityEditor.Editor
    {
        private string error;
        private bool showRouting;
        private void OnEnable()
        {
            Undo.undoRedoPerformed += Repaint;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            MiniGameAudioSettingsWindow.StopPreview();
        }
        private void OnPlayModeChanged(PlayModeStateChange state) => MiniGameAudioSettingsWindow.StopPreview();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawSourceFolder();
            Draw("MasterVolume", "总音量");
            Draw("MusicVolume", "音乐音量");
            Draw("AmbienceVolume", "环境音量");
            Draw("SfxVolume", "场景音效音量");
            Draw("UIVolume", "界面音效音量");
            Draw("FadeSeconds", "淡入淡出时长（秒）");
            DrawSounds();
            showRouting = EditorGUILayout.Foldout(showRouting, "高级：混音路由", true);
            if (showRouting)
            {
                Draw("Mixer", "混音器");
                Draw("MusicGroup", "音乐通道");
                Draw("AmbienceGroup", "环境通道");
                Draw("SfxGroup", "场景音效通道");
                Draw("UIGroup", "界面音效通道");
            }
            if (serializedObject.ApplyModifiedProperties()) EditorApplication.QueuePlayerLoopUpdate();
            EditorGUILayout.HelpBox("这里直接编辑音频配置资产。支持撤销和重做；播放中调整并保存后，退出播放仍会保留。片段留空即不播放。", MessageType.Info);
            EditorGUILayout.HelpBox("音频留空时不会自动借用其他曲目；终局音乐可在此选择你准备好的文件。", MessageType.Info);
            EditorGUILayout.HelpBox("试听播放片段原声，不经过游戏混音。播放模式请在游戏中听实时音量，编辑器试听会自动停止。", MessageType.Info);
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Warning);
            if (GUILayout.Button("保存音频配置"))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssetIfDirty(target);
            }
        }

        private void Draw(string property, string label) =>
            EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(label));

        private void DrawSourceFolder()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("从本机文件夹选择音频", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(MiniGameAudioFileBrowser.Folder)
                ? "尚未选择文件夹" : MiniGameAudioFileBrowser.Folder, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("选择音频文件夹"))
                {
                    string folder = EditorUtility.OpenFolderPanel("选择音频文件夹", MiniGameAudioFileBrowser.Folder, "");
                    if (!string.IsNullOrEmpty(folder)) SelectFolder(folder, MiniGameAudioFileBrowser.Recursive);
                }
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(MiniGameAudioFileBrowser.Folder)))
                    if (GUILayout.Button("刷新文件列表", GUILayout.Width(100)))
                        SelectFolder(MiniGameAudioFileBrowser.Folder, MiniGameAudioFileBrowser.Recursive);
            }
            bool recursive = EditorGUILayout.Toggle("包含子文件夹", MiniGameAudioFileBrowser.Recursive);
            if (recursive != MiniGameAudioFileBrowser.Recursive) SelectFolder(MiniGameAudioFileBrowser.Folder, recursive);
            EditorGUILayout.HelpBox("支持 WAV、MP3、OGG、AIF、AIFF。选择文件夹只列出文件；下方选中某个用途的文件后才导入。也可直接拖入 Project 中的 AudioClip。", MessageType.Info);
            if (!string.IsNullOrEmpty(MiniGameAudioFileBrowser.Warning)) EditorGUILayout.HelpBox(MiniGameAudioFileBrowser.Warning, MessageType.Warning);
        }

        private void SelectFolder(string folder, bool recursive)
        {
            try { MiniGameAudioSettingsWindow.SelectSourceFolder(folder, recursive); error = null; }
            catch (Exception exception) { error = exception.Message; }
        }

        private void DrawSounds()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("声音条目", EditorStyles.boldLabel);
            var sounds = serializedObject.FindProperty("Sounds");
            var present = new HashSet<int>();
            for (int index = 0; index < sounds.arraySize; index++)
            {
                var entry = sounds.GetArrayElementAtIndex(index);
                var id = entry.FindPropertyRelative("Id");
                var clip = entry.FindPropertyRelative("Clip");
                var volume = entry.FindPropertyRelative("Volume");
                if (id == null || clip == null || volume == null) continue;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var role = (MiniGameSound)id.intValue;
                    EditorGUILayout.LabelField(MiniGameAudioSettingsWindow.SoundLabel(role), EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(MiniGameAudioSettingsWindow.SoundTrigger(role), EditorStyles.wordWrappedMiniLabel);
                    var choices = MiniGameAudioFileBrowser.Choices;
                    using (new EditorGUI.DisabledScope(choices.Length < 2))
                    {
                        int selected = EditorGUILayout.Popup("文件夹音频", 0, choices);
                        if (selected > 0 && AssignSource(role, MiniGameAudioFileBrowser.Candidates[selected - 1])) return;
                    }
                    EditorGUILayout.PropertyField(clip, new GUIContent("音频片段"));
                    EditorGUILayout.PropertyField(volume, new GUIContent("片段音量"));
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("选文件"))
                        {
                            string file = EditorUtility.OpenFilePanelWithFilters("选择" + MiniGameAudioSettingsWindow.SoundLabel(role),
                                MiniGameAudioFileBrowser.Folder, new[] { "音频", "wav,mp3,ogg,aif,aiff" });
                            if (!string.IsNullOrEmpty(file) && AssignSource(role, file)) return;
                        }
                        using (new EditorGUI.DisabledScope(clip.objectReferenceValue == null || EditorApplication.isPlayingOrWillChangePlaymode))
                            if (GUILayout.Button("试听")) MiniGameAudioSettingsWindow.PreviewClip((AudioClip)clip.objectReferenceValue);
                        if (GUILayout.Button("停止")) MiniGameAudioSettingsWindow.StopPreview();
                    }
                    if (!present.Add(id.intValue)) EditorGUILayout.HelpBox("用途编号重复：运行时使用第一个条目。", MessageType.Warning);
                }
            }
            if (Enum.GetValues(typeof(MiniGameSound)).Cast<MiniGameSound>().Any(id => !present.Contains((int)id))
                && GUILayout.Button("补齐缺少的用途（保留现有配置）"))
                foreach (MiniGameSound id in Enum.GetValues(typeof(MiniGameSound)))
                {
                    if (present.Contains((int)id)) continue;
                    int index = sounds.arraySize;
                    sounds.InsertArrayElementAtIndex(index);
                    var entry = sounds.GetArrayElementAtIndex(index);
                    entry.FindPropertyRelative("Id").intValue = (int)id;
                    entry.FindPropertyRelative("Clip").objectReferenceValue = null;
                    entry.FindPropertyRelative("Volume").floatValue = 1;
                }
        }

        private bool AssignSource(MiniGameSound role, string file)
        {
            try
            {
                serializedObject.ApplyModifiedProperties();
                MiniGameAudioSettingsWindow.AssignCandidate((MiniGameAudioProfile)target, role, file);
                serializedObject.Update();
                error = null;
                return true;
            }
            catch (Exception exception) { error = exception.Message; return false; }
        }
    }

    public sealed class MiniGameAudioSettingsWindow : EditorWindow
    {
        private MiniGameAudioProfile profile;
        private UnityEditor.Editor profileEditor;
        private Vector2 scroll;

        [MenuItem("MiniGame/音频设置")]
        public static void Open() => GetWindow<MiniGameAudioSettingsWindow>("音频设置").Show();

        public static string[] SelectSourceFolder(string folder, bool recursive = false) => MiniGameAudioFileBrowser.Select(folder, recursive);

        public static AudioClip AssignCandidate(MiniGameAudioProfile profile, MiniGameSound role, string file)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var clip = MiniGameAudioSetup.ImportSelectedFile(file, role);
            Undo.RecordObject(profile, "选择" + SoundLabel(role));
            var entry = profile.GetEntry(role);
            if (entry == null)
            {
                entry = new MiniGameSoundEntry { Id = role };
                profile.Sounds = (profile.Sounds ?? Array.Empty<MiniGameSoundEntry>()).Concat(new[] { entry }).ToArray();
            }
            entry.Clip = clip;
            EditorUtility.SetDirty(profile);
            EditorApplication.QueuePlayerLoopUpdate();
            return clip;
        }

        public static void PreviewClip(AudioClip clip) => MiniGameAudioFileBrowser.Play(clip);
        public static void StopPreview() => MiniGameAudioFileBrowser.Stop();

        public static string SoundLabel(MiniGameSound role)
        {
            switch (role)
            {
                case MiniGameSound.StartMenu: return "主菜单音乐";
                case MiniGameSound.Gameplay: return "房间音乐";
                case MiniGameSound.Dream0: return "Day1 鲸尸海滩";
                case MiniGameSound.Dream1: return "Day2 无尽长路";
                case MiniGameSound.Dream2: return "备用：枯花田";
                case MiniGameSound.Dream3: return "Day3 受缚待审";
                case MiniGameSound.EndingMusic: return "海滩终局音乐";
                case MiniGameSound.OceanWaves: return "海浪环境声";
                case MiniGameSound.TerminalPowerOn: return "电脑开机";
                case MiniGameSound.TerminalPowerOff: return "电脑关机";
                case MiniGameSound.PCNoise: return "电脑底噪";
                case MiniGameSound.Projector: return "放映机";
                case MiniGameSound.Scan: return "扫描";
                case MiniGameSound.LightBeep: return "检定仪亮灯";
                case MiniGameSound.WorldInteraction: return "场景交互";
                case MiniGameSound.UIInteraction: return "界面交互";
                case MiniGameSound.DialoguePop: return "对话弹出";
                default: return "未定义用途（" + (int)role + "）";
            }
        }

        public static string SoundTrigger(MiniGameSound role)
        {
            switch (role)
            {
                case MiniGameSound.StartMenu: return "主菜单使用；当前房间场景没有主菜单，此处仅预留配置。";
                case MiniGameSound.Gameplay: return "房间内循环播放，梦境和结局期间停止。";
                case MiniGameSound.Dream0: return "正式流程 Day1 鲸尸海滩投影期间播放。";
                case MiniGameSound.Dream1: return "正式流程 Day2 无尽长路投影期间播放。";
                case MiniGameSound.Dream2: return "旧版枯花田备用曲目，正式梦境不使用。";
                case MiniGameSound.Dream3: return "正式流程 Day3 受缚待审投影期间播放。";
                case MiniGameSound.EndingMusic: return "海滩终局循环音乐；留空时只播放海浪。";
                case MiniGameSound.OceanWaves: return "海滩终局循环环境声。";
                case MiniGameSound.PCNoise: return "当天开机后持续循环；退出操作界面保持底噪，全部会谈完成关机后淡出。";
                case MiniGameSound.TerminalPowerOn: return "当天第一次开机时播放一次；再次打开操作界面不重播。";
                case MiniGameSound.TerminalPowerOff: return "当天全部会谈完成后，关闭会话并熄屏时播放一次。";
                case MiniGameSound.Projector: return "梦境期间叠加放映机循环声。";
                case MiniGameSound.Scan: return "进入审查或打开检查时播放。";
                case MiniGameSound.LightBeep: return "检定仪的可处置指示灯亮起时播放。";
                case MiniGameSound.WorldInteraction: return "有效的场景交互发生时播放。";
                case MiniGameSound.UIInteraction: return "按钮点击或键盘确认触发时播放。";
                case MiniGameSound.DialoguePop: return "新对白出现时播放。";
                default: return "此用途尚未定义触发行为。";
            }
        }

        private void OnEnable()
        {
            minSize = new Vector2(400, 320);
            Undo.undoRedoPerformed += Repaint;
            EditorApplication.projectChanged += Repaint;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("MiniGame · 音频设置", EditorStyles.boldLabel);
            profile = AssetDatabase.LoadAssetAtPath<MiniGameAudioProfile>(MiniGameAudioSetup.ProfilePath);
            if (profile == null)
            {
                EditorGUILayout.HelpBox("尚未导入原创音频与默认配置。首次导入需要退出播放。", MessageType.Info);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("导入音频并创建默认配置")) profile = MiniGameAudioSetup.EnsureAssets();
                return;
            }
            using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField("当前配置", profile, typeof(MiniGameAudioProfile), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("保存音频配置"))
                {
                    EditorUtility.SetDirty(profile);
                    AssetDatabase.SaveAssetIfDirty(profile);
                }
                if (GUILayout.Button("定位音频配置"))
                {
                    Selection.activeObject = profile;
                    EditorGUIUtility.PingObject(profile);
                }
            }
            using (var view = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = view.scrollPosition;
                UnityEditor.Editor.CreateCachedEditor(profile, typeof(MiniGameAudioProfileEditor), ref profileEditor);
                profileEditor.OnInspectorGUI();
            }
        }

        private void OnDisable()
        {
            StopPreview();
            Undo.undoRedoPerformed -= Repaint;
            EditorApplication.projectChanged -= Repaint;
            if (profileEditor != null) DestroyImmediate(profileEditor);
        }
    }

    internal static class MiniGameAudioFileBrowser
    {
        private const string FolderKey = "MiniGame.Audio.SourceFolder";
        private const string RecursiveKey = "MiniGame.Audio.SourceFolderRecursive";
        private static string folder;
        private static bool recursive;
        private static string[] candidates;
        private static string[] choices;
        private static bool previewOwned;
        public static string Warning { get; private set; }
        public static string Folder { get { Initialize(); return folder; } }
        public static bool Recursive { get { Initialize(); return recursive; } }
        public static string[] Candidates { get { Initialize(); return candidates; } }
        public static string[] Choices { get { Initialize(); return choices; } }

        static MiniGameAudioFileBrowser()
        {
            EditorApplication.playModeStateChanged += _ => Stop();
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
        }

        private static void Initialize()
        {
            if (candidates != null) return;
            folder = EditorPrefs.GetString(FolderKey, "");
            recursive = EditorPrefs.GetBool(RecursiveKey, false);
            candidates = Array.Empty<string>();
            choices = new[] { "选择文件…" };
            if (Directory.Exists(folder)) Select(folder, recursive);
        }

        public static string[] Select(string sourceFolder, bool includeSubfolders)
        {
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("请选择存在的音频文件夹。");
            folder = Path.GetFullPath(sourceFolder);
            recursive = includeSubfolders;
            EditorPrefs.SetString(FolderKey, folder);
            EditorPrefs.SetBool(RecursiveKey, recursive);
            Warning = null;
            var found = new List<string>();
            var pending = new Stack<string>();
            pending.Push(folder);
            while (pending.Count > 0)
            {
                string directory = pending.Pop();
                try
                {
                    foreach (string file in Directory.EnumerateFiles(directory))
                        if ((File.GetAttributes(file) & FileAttributes.Hidden) == 0 && MiniGameAudioSetup.IsSupportedAudioFile(file)) found.Add(file);
                    if (!recursive) continue;
                    foreach (string child in Directory.EnumerateDirectories(directory))
                    {
                        var attributes = File.GetAttributes(child);
                        string name = Path.GetFileName(child);
                        if ((attributes & (FileAttributes.Hidden | FileAttributes.ReparsePoint)) != 0 || name.StartsWith(".", StringComparison.Ordinal)
                            || new[] { "Library", "Temp", "node_modules", "obj", "bin" }.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
                        pending.Push(child);
                    }
                }
                catch (UnauthorizedAccessException) { Warning = "部分子文件夹无法读取，已跳过。"; }
                catch (IOException) { Warning = "部分文件已移动或无法读取，请刷新列表。"; }
            }
            candidates = found.OrderBy(file => file, StringComparer.OrdinalIgnoreCase).ToArray();
            choices = new[] { "选择文件…" }.Concat(candidates.Select(file => Path.GetRelativePath(folder, file))).ToArray();
            return candidates.ToArray();
        }

        public static void Play(AudioClip clip)
        {
            if (clip == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            Stop();
            AudioUtilMethod("PlayPreviewClip").Invoke(null, new object[] { clip, 0, false });
            previewOwned = true;
        }

        public static void Stop()
        {
            if (!previewOwned) return;
            AudioUtilMethod("StopAllPreviewClips").Invoke(null, null);
            previewOwned = false;
        }

        private static MethodInfo AudioUtilMethod(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("UnityEditor.AudioUtil")).FirstOrDefault(type => type != null)
            ?.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException("UnityEditor.AudioUtil", name);
    }
}

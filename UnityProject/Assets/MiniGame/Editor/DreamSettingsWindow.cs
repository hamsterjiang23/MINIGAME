using System;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public sealed class DreamSettingsWindow : EditorWindow
    {
        private const string PendingBeachOpenKey = "MiniGame.DreamSettings.OpenBeachAfterPlay";

        [InitializeOnLoadMethod]
        private static void ResumePendingBeachOpen()
        {
            EditorApplication.playModeStateChanged -= OnBeachOpenPlayModeChanged;
            EditorApplication.playModeStateChanged += OnBeachOpenPlayModeChanged;
            if (SessionState.GetBool(PendingBeachOpenKey, false) && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.delayCall += CompletePendingBeachOpen;
        }

        private static void OnBeachOpenPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += CompletePendingBeachOpen;
        }

        private static void CompletePendingBeachOpen()
        {
            if (!SessionState.GetBool(PendingBeachOpenKey, false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.EraseBool(PendingBeachOpenKey);
            var window = GetWindow<DreamSettingsWindow>("梦境与结局");
            window.Run(window.OpenBeach);
            window.Repaint();
        }

        private sealed class ChoiceRow
        {
            public int Day;
            public string PatientId, PatientName, OptionId, Text;
            public bool Special;
        }

        private DreamSequenceProfile profile;
        private PlayerRebellionProfile rebellion;
        private SerializedObject settings, rules;
        private readonly List<ChoiceRow> choices = new List<ChoiceRow>();
        private string[] patients = { "全部人物" }, dayLabels = { "全部日期" };
        private int[] days = Array.Empty<int>();
        private string[] patientIds = Array.Empty<string>();
        private int tab, selectedPatient, selectedDay;
        private string search = "", error, choiceError, savedMessage;
        private Vector2 scroll;
        private bool showRawRules;
        private DreamSequenceDirector preview;

        [MenuItem("MiniGame/梦境与结局")]
        public static void Open()
        {
            var window = GetWindow<DreamSettingsWindow>("梦境与结局");
            window.minSize = new Vector2(540, 440);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(540, 440);
            Undo.undoRedoPerformed += OnUndo;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.projectChanged += ReloadChoices;
            try { ResolveAssets(); ReloadChoices(); }
            catch (Exception exception) { error = exception.Message; }
        }

        private void OnDisable()
        {
            StopPreview();
            Undo.undoRedoPerformed -= OnUndo;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorApplication.projectChanged -= ReloadChoices;
            settings?.Dispose(); rules?.Dispose();
        }

        private void ResolveAssets()
        {
            if (profile == null) profile = DreamSequenceSetup.EnsureProfile();
            if (rebellion == null) rebellion = DreamSequenceSetup.EnsureRebellionProfile();
            if (settings == null && profile != null) settings = new SerializedObject(profile);
            if (rules == null && rebellion != null) rules = new SerializedObject(rebellion);
        }

        private void OnGUI()
        {
            if (profile == null || rebellion == null || settings == null || rules == null)
            {
                EditorGUILayout.HelpBox(error ?? "梦境配置尚未创建。", MessageType.Warning);
                if (GUILayout.Button("加载主场景配置"))
                    try { ResolveAssets(); ReloadChoices(); error = null; } catch (Exception exception) { error = exception.Message; }
                return;
            }
            settings.Update(); rules.Update();
            DrawToolbar();
            tab = GUILayout.Toolbar(tab, new[] { "每日梦境", "海滩终局", "玩家反叛值" }, GUILayout.Height(27));
            EditorGUILayout.HelpBox(tab == 2
                ? "反叛值属于玩家全局状态。修改配置并保存后，从下一次新游戏生效；不会改写当前患者信任或已结算的选择。"
                : "直接编辑配置资产，支持撤销/重做。播放中保存后退出仍保留。预览仅展示画面，不推进正式剧情。", MessageType.Info);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (tab == 0) DrawDailyDreams();
            else if (tab == 1) DrawEnding();
            else DrawRebellion();
            EditorGUILayout.EndScrollView();
            bool modified = settings.ApplyModifiedProperties() | rules.ApplyModifiedProperties();
            if (modified) { savedMessage = null; EditorApplication.QueuePlayerLoopUpdate(); }
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
            if (!string.IsNullOrEmpty(savedMessage)) EditorGUILayout.HelpBox(savedMessage, MessageType.Info);
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("保存配置", EditorStyles.toolbarButton, GUILayout.Width(80))) Save();
                if (GUILayout.Button("定位配置", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    EditorGUIUtility.PingObject(tab == 2 ? (UnityEngine.Object)rebellion : profile);
                GUILayout.FlexibleSpace();
                if (tab == 1 && GUILayout.Button(BeachOpenLabel, EditorStyles.toolbarButton, GUILayout.Width(160)))
                    Run(RequestOpenBeach);
                if (GUILayout.Button("音频设置", EditorStyles.toolbarButton, GUILayout.Width(80))) MiniGameAudioSettingsWindow.Open();
                if (preview != null && preview.IsPreview)
                    preview.PreviewPaused = GUILayout.Toggle(preview.PreviewPaused, "暂停预览", EditorStyles.toolbarButton, GUILayout.Width(80));
                using (new EditorGUI.DisabledScope(preview == null))
                    if (GUILayout.Button("停止预览", EditorStyles.toolbarButton, GUILayout.Width(80))) StopPreview();
            }
        }

        private void DrawDailyDreams()
        {
            EditorGUILayout.LabelField("墙面放映", EditorStyles.boldLabel);
            Draw(settings, "EnableDreamFlow");
            Draw(settings, "AlignSeconds"); Draw(settings, "ReturnSeconds"); Draw(settings, "ProjectionInset");
            Draw(settings, "Audio");
            EditorGUILayout.HelpBox("Day0 完成教学后只开启床引导。Day1–3 工作结束、退出电脑后自动播放对应梦境，播放完成再引导床或特殊分支电脑。", MessageType.None);
            var entries = settings.FindProperty("Days");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var day = entries.GetArrayElementAtIndex(i);
                int number = day.FindPropertyRelative("Day").intValue;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string title = number == 1 ? "鲸尸海滩" : number == 2 ? "无尽长路" : number == 3 ? "受缚待审" : "梦境";
                    EditorGUILayout.LabelField("Day" + number + " · " + title, EditorStyles.boldLabel);
                    Relative(day, "Duration", "播放时长（秒）");
                    Relative(day, "Artwork", "画面资源");
                    Relative(day, "Music", "音乐用途");
                    Relative(day, "MotionIntensity", "运动强度");
                    if (GUILayout.Button("预览 Day" + number + " 梦境")) StartPreview(number);
                }
            }
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("更新 Combined 梦境接入与墙面定位"))
                    Run(() => { StopPreview(); settings.ApplyModifiedProperties(); DreamSequenceSetup.ApplyToCombined(); });
            DrawSceneBindings();
        }

        private void DrawEnding()
        {
            EditorGUILayout.LabelField("移动与躺下", EditorStyles.boldLabel);
            Draw(settings, "WalkSpeed"); Draw(settings, "BedInteractionDistance"); Draw(settings, "LieDownSeconds");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("漂流与天空", EditorStyles.boldLabel);
            Draw(settings, "FloatAmplitude"); Draw(settings, "FloatPeriod"); Draw(settings, "FloatTiltDegrees");
            Draw(settings, "DriftSpeed"); Draw(settings, "CloudSpeed");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("海浪与岸边", EditorStyles.boldLabel);
            Draw(settings, "SeaWaveHeight"); Draw(settings, "SeaWavePeriod"); Draw(settings, "SeaWaveLength");
            Draw(settings, "ShoreWashDistance"); Draw(settings, "SeaFoamAmount");
            EditorGUILayout.HelpBox("海面和湿沙共用同一演出时钟；暂停会停止波浪，读档恢复当时的浪形。播放中调整参数会实时更新。", MessageType.None);
            EditorGUILayout.Space(); EditorGUILayout.LabelField("字幕与配音", EditorStyles.boldLabel);
            Draw(settings, "SubtitleDelay");
            var lines = settings.FindProperty("EndingLines");
            for (int i = 0; i < lines.arraySize; i++)
            {
                var line = lines.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("字幕 " + (i + 1), EditorStyles.boldLabel);
                    Relative(line, "Text", "字幕内容");
                    Relative(line, "Voice", "配音文件");
                    Relative(line, "Duration", "无配音显示秒数");
                    var clip = line.FindPropertyRelative("Voice").objectReferenceValue as AudioClip;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(clip == null || EditorApplication.isPlayingOrWillChangePlaymode))
                            if (GUILayout.Button("试听配音")) MiniGameAudioSettingsWindow.PreviewClip(clip);
                        if (GUILayout.Button("停止试听")) MiniGameAudioSettingsWindow.StopPreview();
                    }
                }
            }
            EditorGUILayout.HelpBox("有配音时按音频长度推进字幕；留空时使用显示秒数。终局歌曲与海浪循环在“音频设置”中选择文件。", MessageType.None);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                EditorGUILayout.HelpBox("暂停仍属于播放模式。点击下方按钮可退出播放，再打开海滩场景。", MessageType.Info);
            if (GUILayout.Button(BeachOpenLabel)) Run(RequestOpenBeach);
        }

        private void DrawSceneBindings()
        {
            var director = FindObjectsOfType<DreamSequenceDirector>().FirstOrDefault(item => item.gameObject.scene.name == "MainRoomCombined");
            if (director == null)
            {
                EditorGUILayout.HelpBox("打开 Combined 并更新梦境接入后，可调整投影墙面与观影定位点。", MessageType.None);
                return;
            }
            EditorGUILayout.Space(); EditorGUILayout.LabelField("Combined 场景定位", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || director.IsPreview))
            using (var bindings = new SerializedObject(director))
            {
                bindings.Update();
                EditorGUILayout.PropertyField(bindings.FindProperty("wall"), new GUIContent("投影墙面"));
                EditorGUILayout.PropertyField(bindings.FindProperty("projectionSurface"), new GUIContent("投影显示面"));
                var poseProperty = bindings.FindProperty("viewingPose");
                EditorGUILayout.PropertyField(poseProperty, new GUIContent("观影定位点"));
                var pose = poseProperty.objectReferenceValue as Transform;
                bindings.ApplyModifiedProperties();
                if (pose != null)
                {
                    EditorGUI.BeginChangeCheck();
                    var position = EditorGUILayout.Vector3Field("观影世界位置", pose.position);
                    var rotation = EditorGUILayout.Vector3Field("观影世界朝向", pose.eulerAngles);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(pose, "调整梦境观影定位");
                        pose.SetPositionAndRotation(position, Quaternion.Euler(rotation));
                        EditorSceneManager.MarkSceneDirty(pose.gameObject.scene);
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("在 Scene 视图定位"))
                    {
                        Selection.activeGameObject = pose != null ? pose.gameObject : director.gameObject;
                        SceneView.lastActiveSceneView?.FrameSelected();
                    }
                    if (GUILayout.Button("保存场景定位"))
                        Run(() => EditorSceneManager.SaveScene(director.gameObject.scene));
                }
            }
            EditorGUILayout.HelpBox("场景定位单独保存在 Combined；预览时临时相机会在停止后还原。播放模式下定位只读。", MessageType.None);
        }

        private void DrawRebellion()
        {
            Draw(rules, "InitialValue"); Draw(rules, "Maximum"); Draw(rules, "Threshold");
            var current = MiniGameSessionHost.Current?.Runtime?.GetView();
            EditorGUILayout.LabelField("当前玩家全局反叛值", current != null ? current.Game.Rebellion.ToString() : "未运行正式会话");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("对白选项增减", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                search = EditorGUILayout.TextField("搜索文字或 ID", search);
                if (GUILayout.Button("刷新对白", GUILayout.Width(80))) ReloadChoices();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                selectedPatient = EditorGUILayout.Popup("人物", Mathf.Clamp(selectedPatient, 0, patients.Length - 1), patients);
                selectedDay = EditorGUILayout.Popup("日期", Mathf.Clamp(selectedDay, 0, dayLabels.Length - 1), dayLabels);
            }
            if (!string.IsNullOrEmpty(choiceError)) EditorGUILayout.HelpBox(choiceError, MessageType.Warning);
            var deltas = rules.FindProperty("Choices");
            var lookup = new Dictionary<string, int>(StringComparer.Ordinal);
            bool duplicate = false;
            for (int i = 0; i < deltas.arraySize; i++)
            {
                var id = deltas.GetArrayElementAtIndex(i).FindPropertyRelative("OptionId").stringValue;
                if (lookup.ContainsKey(id)) duplicate = true; else lookup[id] = i;
            }
            int visible = 0;
            foreach (var row in choices)
            {
                if (selectedPatient > 0 && row.PatientId != patientIds[selectedPatient - 1]) continue;
                if (selectedDay > 0 && row.Day != days[selectedDay - 1]) continue;
                if (!string.IsNullOrWhiteSpace(search) && row.OptionId.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0
                    && row.Text.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0
                    && row.PatientName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                visible++;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Day" + row.Day + " · " + row.PatientName + (row.Special ? " · 特殊对白" : ""), EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(row.Text, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.SelectableLabel(row.OptionId, EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    bool exists = lookup.TryGetValue(row.OptionId, out int index);
                    int oldValue = exists ? deltas.GetArrayElementAtIndex(index).FindPropertyRelative("Delta").intValue : 0;
                    int value = EditorGUILayout.IntSlider("玩家反叛值增减", oldValue, -100, 100);
                    if (value != oldValue)
                    {
                        if (!exists)
                        {
                            index = deltas.arraySize; deltas.InsertArrayElementAtIndex(index);
                            deltas.GetArrayElementAtIndex(index).FindPropertyRelative("OptionId").stringValue = row.OptionId;
                            lookup[row.OptionId] = index;
                        }
                        deltas.GetArrayElementAtIndex(index).FindPropertyRelative("Delta").intValue = value;
                    }
                }
            }
            EditorGUILayout.LabelField("显示 " + visible + " / " + choices.Count + " 个可配置回答；未配置项为 0。", EditorStyles.wordWrappedMiniLabel);
            if (duplicate) EditorGUILayout.HelpBox("配置包含重复选项 ID，请在原始条目中修正后保存。", MessageType.Error);
            var validIds = new HashSet<string>(choices.Select(c => c.OptionId), StringComparer.Ordinal);
            int unknown = lookup.Keys.Count(id => !validIds.Contains(id));
            if (unknown > 0) EditorGUILayout.HelpBox("有 " + unknown + " 个条目不属于当前有效对白。请核对原始条目；不会自动删除旧配置。", MessageType.Warning);
            showRawRules = EditorGUILayout.Foldout(showRawRules, "原始条目（处理重复或旧 ID）", true);
            if (showRawRules) EditorGUILayout.PropertyField(deltas, true);
        }

        private void ReloadChoices()
        {
            choices.Clear(); choiceError = null;
            try
            {
                var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
                if (package == null || package.PackageSource == null) throw new InvalidOperationException("正式 Campaign 对白包尚未导入。");
                var compiled = DialoguePackageCompiler.Compile(package.PackageSource.text);
                var labels = compiled.SourceNodes.SelectMany(node => node.Options).Where(option => !option.Disposition.HasValue)
                    .GroupBy(option => option.Id, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.First().Text, StringComparer.Ordinal);
                var names = compiled.Rules.Patients.ToDictionary(patient => patient.Id, patient => patient.DisplayName, StringComparer.Ordinal);
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var day in compiled.Rules.Days)
                    foreach (var meeting in day.Meetings)
                    {
                        foreach (var round in meeting.NormalRounds) AddRound(round, day.Day, meeting.PatientId, false, labels, names, seen);
                        if (meeting.SpecialRound != null) AddRound(meeting.SpecialRound, day.Day, meeting.PatientId, true, labels, names, seen);
                    }
                patientIds = choices.Select(row => row.PatientId).Distinct().ToArray();
                patients = new[] { "全部人物" }.Concat(patientIds.Select(id => names[id])).ToArray();
                days = choices.Select(row => row.Day).Distinct().OrderBy(day => day).ToArray();
                dayLabels = new[] { "全部日期" }.Concat(days.Select(day => "Day" + day)).ToArray();
            }
            catch (Exception exception) { choiceError = exception.Message; }
            Repaint();
        }

        private void AddRound(RoundDefinition round, int day, string patient, bool special,
            Dictionary<string, string> labels, Dictionary<string, string> names, HashSet<string> seen)
        {
            foreach (var option in round.Responses)
                if (seen.Add(option.Id)) choices.Add(new ChoiceRow { Day = day, PatientId = patient,
                    PatientName = names.TryGetValue(patient, out var name) ? name : patient, Special = special,
                    OptionId = option.Id, Text = labels.TryGetValue(option.Id, out var text) ? text : option.Id });
        }

        private void StartPreview(int day)
        {
            Run(() =>
            {
                StopPreview(); settings.ApplyModifiedProperties();
                preview = FindObjectsOfType<DreamSequenceDirector>().FirstOrDefault(item => item.gameObject.scene.name == "MainRoomCombined");
                if (preview == null) throw new InvalidOperationException("请先打开 MainRoomCombined 并更新梦境接入。");
                if (!preview.PreviewDay(day))
                {
                    preview = null;
                    throw new InvalidOperationException("当前正在正式演出，或墙面、相机配置不完整，无法开始预览。");
                }
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            });
        }

        private void StopPreview()
        {
            if (preview != null) preview.StopPreview();
            preview = null;
            MiniGameAudioSettingsWindow.StopPreview();
        }

        private static string BeachOpenLabel => EditorApplication.isPlayingOrWillChangePlaymode
            ? "退出播放并打开海滩" : "打开海滩终局场景";

        private void RequestOpenBeach()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) { OpenBeach(); return; }
            StopPreview(); settings.ApplyModifiedProperties(); rules.ApplyModifiedProperties();
            SessionState.SetBool(PendingBeachOpenKey, true);
            EditorApplication.isPaused = false;
            EditorApplication.isPlaying = false;
        }

        private void OpenBeach()
        {
            StopPreview(); settings.ApplyModifiedProperties();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DreamEndingSceneBuilder.ScenePath) == null)
                DreamEndingSceneBuilder.BuildScene(profile);
            EditorSceneManager.OpenScene(DreamEndingSceneBuilder.ScenePath, OpenSceneMode.Single);
        }

        private void Save()
        {
            Run(() =>
            {
                settings.ApplyModifiedProperties(); rules.ApplyModifiedProperties();
                rebellion.CreateDeltas();
                if (choices.Count > 0)
                {
                    var valid = new HashSet<string>(choices.Select(row => row.OptionId), StringComparer.Ordinal);
                    var stale = rebellion.Choices.FirstOrDefault(item => item != null && !valid.Contains(item.OptionId));
                    if (stale != null) throw new InvalidOperationException("存在无效选项 ID：" + stale.OptionId + "。请在原始条目中核对后保存。");
                }
                EditorUtility.SetDirty(profile); EditorUtility.SetDirty(rebellion);
                AssetDatabase.SaveAssetIfDirty(profile); AssetDatabase.SaveAssetIfDirty(rebellion);
                savedMessage = "配置已保存。反叛规则从下一次新游戏生效。";
            });
        }

        private void Run(Action action)
        {
            try { action(); error = null; }
            catch (Exception exception) { error = exception.Message; Debug.LogWarning(exception.Message); }
        }

        private static void Draw(SerializedObject value, string property) => EditorGUILayout.PropertyField(value.FindProperty(property));
        private static void Relative(SerializedProperty value, string property, string label) =>
            EditorGUILayout.PropertyField(value.FindPropertyRelative(property), new GUIContent(label));
        private void OnUndo() { settings?.Update(); rules?.Update(); Repaint(); }
        private void OnPlayModeChanged(PlayModeStateChange value) => StopPreview();
        private void OnSceneClosing(Scene scene, bool removingScene) => StopPreview();
        private void OnInspectorUpdate() { if (EditorApplication.isPlaying || preview != null) Repaint(); }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace MiniGame.UnityHost.Editor
{
    public static class MiniGameAudioSetup
    {
        public const string ProfilePath = "Assets/MiniGame/Resources/MiniGame/AudioDefault.asset";
        public const string ProfileResourcePath = "MiniGame/AudioDefault";
        public const string MixerPath = "Assets/MiniGame/Audio/MiniGame.mixer";
        public const string ImportedPath = "Assets/MiniGame/Audio/Imported";
        private const BindingFlags ReflectionFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private static readonly (MiniGameSound Id, string File, bool Streaming)[] SourceClips =
        {
            (MiniGameSound.StartMenu, "Music/StartMenu.wav", true),
            (MiniGameSound.LogoGlitch, "SFX/LogoGlitchSound.wav", false),
            (MiniGameSound.Gameplay, "Music/BGMGameplay.wav", true),
            (MiniGameSound.Dream0, "Music/Dream0.wav", true),
            (MiniGameSound.Dream1, "Music/Dream1.wav", true),
            (MiniGameSound.Dream2, "Music/Dream2.wav", true),
            (MiniGameSound.PCNoise, "SFX/PCNoise.wav", true),
            (MiniGameSound.Projector, "SFX/ProjectorRunning.wav", false),
            (MiniGameSound.ProjectorOpen, "SFX/ProjectorOpen.wav", false),
            (MiniGameSound.Scan, "SFX/Scan.wav", false),
            (MiniGameSound.LightBeep, "SFX/LightBeep.wav", false),
            (MiniGameSound.WorldInteraction, "SFX/3DSceneInteraction.wav", false),
            (MiniGameSound.UIInteraction, "SFX/UIInteraction.wav", false),
            (MiniGameSound.DialoguePop, "SFX/DialoguePop.wav", false),
            (MiniGameSound.TerminalPowerOn, "SFX/TerminalPowerOn.wav", false),
            (MiniGameSound.TerminalPowerOff, "SFX/TerminalPowerOff.wav", false),
            (MiniGameSound.TerminalType1, "SFX/UIType/UIType1.wav", false),
            (MiniGameSound.TerminalType2, "SFX/UIType/UIType2.wav", false),
            (MiniGameSound.TerminalType3, "SFX/UIType/UIType3.wav", false),
            (MiniGameSound.TerminalType4, "SFX/UIType/UIType4.wav", false),
            (MiniGameSound.DoorOpen, "SFX/DoorOpen.wav", false),
            (MiniGameSound.DoorShut, "SFX/NewDoorShut.wav", false),
            (MiniGameSound.DoorKnock, "SFX/NewDoorKnock.wav", false),
            (MiniGameSound.EndNote, "SFX/EndNote.wav", false)
        };

        public static MiniGameAudioProfile EnsureAssets()
        {
            var existing = AssetDatabase.LoadAssetAtPath<MiniGameAudioProfile>(ProfilePath);
            if (existing != null) return existing;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请退出播放后首次导入音频；已有配置可在播放中调整并保存。");
            if (File.Exists(ProfilePath)) throw new InvalidOperationException("默认音频配置路径已有其他资产，未覆盖：" + ProfilePath);

            EnsureFolder("Assets/MiniGame/Audio/Music");
            EnsureFolder("Assets/MiniGame/Audio/SFX");
            EnsureFolder("Assets/MiniGame/Audio/SFX/UIType");
            EnsureFolder("Assets/MiniGame/Resources/MiniGame");
            var entries = SourceClips.Select(source => new MiniGameSoundEntry
            {
                Id = source.Id, Clip = ImportClip(source.File, source.Streaming),
                Volume = source.Id >= MiniGameSound.TerminalType1 && source.Id <= MiniGameSound.TerminalType4
                    ? .234f : source.Id == MiniGameSound.PCNoise || source.Id == MiniGameSound.EndingMusic ? .7f : source.Id == MiniGameSound.Scan ? 1.625f : 1
            }).ToArray();
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                if (File.Exists(MixerPath)) throw new InvalidOperationException("混音器路径已有其他资产，未覆盖：" + MixerPath);
                mixer = CreateMixer();
            }

            var profile = ScriptableObject.CreateInstance<MiniGameAudioProfile>();
            try
            {
                profile.Mixer = mixer;
                profile.MusicGroup = RequireGroup(mixer, "Music");
                profile.AmbienceGroup = RequireGroup(mixer, "Ambience");
                profile.SfxGroup = RequireGroup(mixer, "SFX");
                profile.UIGroup = RequireGroup(mixer, "UI");
                profile.Sounds = entries;
                AssetDatabase.CreateAsset(profile, ProfilePath);
                AssetDatabase.SaveAssetIfDirty(profile);
                return profile;
            }
            catch
            {
                if (!EditorUtility.IsPersistent(profile)) UnityEngine.Object.DestroyImmediate(profile);
                throw;
            }
        }

        [MenuItem("MiniGame/音频/应用开门与关门音效")]
        public static void ApplyDoorSounds()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请先停止播放再导入开关门音效。");
            var profile = EnsureAssets();
            var open = ImportClip("SFX/DoorOpen.wav", false);
            var shut = ImportClip("SFX/NewDoorShut.wav", false);
            var knock = ImportClip("SFX/NewDoorKnock.wav", false);
            Undo.RecordObject(profile, "绑定开关门音效");
            foreach (var pair in new[] { (Id: MiniGameSound.DoorOpen, Clip: open), (Id: MiniGameSound.DoorShut, Clip: shut),
                (Id: MiniGameSound.DoorKnock, Clip: knock) })
            {
                var entry = profile.GetEntry(pair.Id);
                if (entry == null)
                {
                    entry = new MiniGameSoundEntry { Id = pair.Id, Volume = 1 };
                    profile.Sounds = (profile.Sounds ?? Array.Empty<MiniGameSoundEntry>()).Concat(new[] { entry }).ToArray();
                }
                entry.Clip = pair.Clip;
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            Debug.Log("DOOR_AUDIO_APPLIED DoorOpen.wav / NewDoorShut.wav / NewDoorKnock.wav");
        }

        private static AudioClip ImportClip(string relativePath, bool streaming)
        {
            var repository = Directory.GetParent(Application.dataPath).Parent.FullName;
            // Source files were numbered Day1–3 in the repository; serialized sound IDs stay stable.
            var sourcePath = relativePath switch
            {
                "Music/Dream0.wav" => "Music/Dream1.wav",
                "Music/Dream1.wav" => "Music/Dream2.wav",
                "Music/Dream2.wav" => "Music/UnusedFlowerField.wav",
                _ => relativePath
            };
            var source = Path.Combine(repository, "Art", "Audio", sourcePath);
            var destination = "Assets/MiniGame/Audio/" + relativePath;
            if (!File.Exists(source)) throw new FileNotFoundException("缺少原创音频源文件。", source);
            if (!File.Exists(destination)) File.Copy(source, destination, false);
            else if (!SameBytes(source, destination))
                throw new InvalidOperationException("已有音频副本与源文件不同，已保留现有文件，请先核对：" + destination);
            return ConfigureImportedClip(destination, streaming, false);
        }

        [MenuItem("MiniGame/Audio/Apply ending note")]
        public static void ApplyEndNoteSound()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请先停止播放再导入结局音效。");
            var bank = EnsureAssets();
            var clip = ImportClip("SFX/EndNote.wav", false);
            Undo.RecordObject(bank, "绑定结局音效");
            var entry = bank.GetEntry(MiniGameSound.EndNote);
            if (entry == null)
            {
                entry = new MiniGameSoundEntry { Id = MiniGameSound.EndNote };
                bank.Sounds = bank.Sounds.Concat(new[] { entry }).ToArray();
            }
            entry.Clip = clip;
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssetIfDirty(bank);
        }

        public static bool IsSupportedAudioFile(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".wav": case ".mp3": case ".ogg": case ".aif": case ".aiff": return true;
                default: return false;
            }
        }

        public static AudioClip ImportSelectedFile(string path, MiniGameSound role)
        {
            if (!Enum.IsDefined(typeof(MiniGameSound), role)) throw new ArgumentOutOfRangeException(nameof(role));
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new FileNotFoundException("请选择存在的音频文件。", path);
            if (!IsSupportedAudioFile(path)) throw new NotSupportedException("支持 WAV、MP3、OGG、AIF 和 AIFF 音频。");
            EnsureFolder(ImportedPath);
            string digest;
            using (var hash = SHA256.Create())
            using (var stream = File.OpenRead(path))
                digest = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            string destination = Directory.EnumerateFiles(ImportedPath, "*_" + digest + ".*")
                .Where(IsSupportedAudioFile).OrderBy(file => file, StringComparer.Ordinal).FirstOrDefault();
            if (destination == null)
            {
                string basename = new string(Path.GetFileNameWithoutExtension(path).Select(character =>
                    char.IsLetterOrDigit(character) || character == '-' || character == '_' ? character : '_').Take(48).ToArray());
                if (string.IsNullOrEmpty(basename)) basename = "audio";
                destination = ImportedPath + "/" + basename + "_" + digest + Path.GetExtension(path).ToLowerInvariant();
                File.Copy(path, destination, false);
            }
            else if (!SameBytes(path, destination))
                throw new InvalidOperationException("已有导入文件内容发生变化，已保留文件，未覆盖：" + destination);
            destination = destination.Replace('\\', '/');
            bool streaming = (int)role <= (int)MiniGameSound.Dream2 || role == MiniGameSound.PCNoise
                || role == MiniGameSound.Dream3 || role == MiniGameSound.EndingMusic || role == MiniGameSound.OceanWaves;
            return ConfigureImportedClip(destination, streaming, true);
        }

        private static AudioClip ConfigureImportedClip(string destination, bool streaming, bool keepStreaming)
        {
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(destination) as AudioImporter;
            if (importer == null) throw new InvalidOperationException("Unity 未能识别音频：" + destination);
            var settings = importer.defaultSampleSettings;
            // Shared content stays streaming if any music/ambience role already required it.
            streaming |= keepStreaming && settings.loadType == AudioClipLoadType.Streaming;
            var loadType = streaming ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            var compression = streaming ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            float quality = streaming ? .8f : 1f;
            bool changed = settings.loadType != loadType || settings.compressionFormat != compression
                || settings.sampleRateSetting != AudioSampleRateSetting.PreserveSampleRate
                || !Mathf.Approximately(settings.quality, quality) || settings.preloadAudioData == streaming
                || importer.loadInBackground != streaming;
            if (changed)
            {
                settings.loadType = loadType;
                settings.compressionFormat = compression;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.quality = quality;
                settings.preloadAudioData = !streaming;
                importer.defaultSampleSettings = settings;
                importer.loadInBackground = streaming;
                importer.SaveAndReimport();
            }
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(destination);
            if (clip == null) throw new InvalidOperationException("音频导入失败：" + destination);
            return clip;
        }

        private static bool SameBytes(string first, string second)
        {
            if (new FileInfo(first).Length != new FileInfo(second).Length) return false;
            using (var hash = SHA256.Create())
            using (var firstStream = File.OpenRead(first))
            using (var secondStream = File.OpenRead(second))
                return hash.ComputeHash(firstStream).SequenceEqual(hash.ComputeHash(secondStream));
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static AudioMixer CreateMixer()
        {
            // Unity 2022.3's editor controller creates native groups, attenuation effects and snapshots.
            // These internal APIs are isolated here; runtime code uses only public AudioMixer APIs.
            var controllerType = RequireEditorType("UnityEditor.Audio.AudioMixerController");
            var mixer = (AudioMixer)RequireMethod(controllerType, "CreateMixerControllerAtPath").Invoke(null, new object[] { MixerPath });
            var master = (AudioMixerGroup)RequireProperty(controllerType, "masterGroup").GetValue(mixer);
            var snapshot = RequireProperty(controllerType, "startSnapshot").GetValue(mixer);
            var groups = new AudioMixerGroup[5];
            var names = new[] { "Master", "Music", "Ambience", "SFX", "UI" };
            var parameters = new[] { "MasterVolume", "MusicVolume", "AmbienceVolume", "SfxVolume", "UIVolume" };
            groups[0] = master;
            for (int index = 1; index < groups.Length; index++)
            {
                groups[index] = (AudioMixerGroup)RequireMethod(controllerType, "CreateNewGroup")
                    .Invoke(mixer, new object[] { names[index], false });
                RequireMethod(controllerType, "AddChildToParent").Invoke(mixer, new object[] { groups[index], master });
                var views = RequireProperty(controllerType, "views").GetValue(mixer) as Array;
                if (views != null && views.Length > 0)
                    RequireMethod(controllerType, "AddGroupToCurrentView").Invoke(mixer, new object[] { groups[index] });
            }
            var exposedType = RequireEditorType("UnityEditor.Audio.ExposedAudioParameter");
            var exposed = Array.CreateInstance(exposedType, groups.Length);
            for (int index = 0; index < groups.Length; index++)
            {
                var group = groups[index];
                var guid = RequireMethod(group.GetType(), "GetGUIDForVolume").Invoke(group, null);
                var entry = Activator.CreateInstance(exposedType);
                exposedType.GetField("guid", ReflectionFlags).SetValue(entry, guid);
                exposedType.GetField("name", ReflectionFlags).SetValue(entry, parameters[index]);
                exposed.SetValue(entry, index);
                RequireMethod(group.GetType(), "SetValueForVolume").Invoke(group, new[] { (object)mixer, snapshot, 0f });
                EditorUtility.SetDirty(group);
            }
            RequireProperty(controllerType, "exposedParameters").SetValue(mixer, exposed);
            RequireMethod(controllerType, "OnChangedExposedParameter").Invoke(mixer, null);
            EditorUtility.SetDirty((UnityEngine.Object)snapshot);
            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssetIfDirty(mixer);
            return mixer;
        }

        private static AudioMixerGroup RequireGroup(AudioMixer mixer, string name) => mixer.FindMatchingGroups(name)
            .FirstOrDefault(group => group.name == name)
            ?? throw new InvalidOperationException("现有混音器缺少通道，未覆盖配置：" + name);

        private static Type RequireEditorType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).FirstOrDefault(type => type != null)
            ?? throw new NotSupportedException("当前 Unity 缺少 2022.3 音频编辑器 API：" + name);

        private static MethodInfo RequireMethod(Type type, string name) => type.GetMethod(name, ReflectionFlags)
            ?? throw new MissingMethodException(type.FullName, name);

        private static PropertyInfo RequireProperty(Type type, string name) => type.GetProperty(name, ReflectionFlags)
            ?? throw new MissingMemberException(type.FullName, name);
    }
}

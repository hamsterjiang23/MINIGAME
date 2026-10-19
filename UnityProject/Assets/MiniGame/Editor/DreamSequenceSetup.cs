using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public static class DreamSequenceSetup
    {
        public const string ProfilePath = "Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset";
        public const string RebellionPath = "Assets/MiniGame/Resources/MiniGame/PlayerRebellion.asset";

        public static DreamSequenceProfile EnsureProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(ProfilePath);
            if (profile != null) return profile;
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("请退出播放后首次创建梦境配置。");
            var audio = MiniGameAudioSetup.EnsureAssets();
            EnsureDreamAudio(audio);
            profile = ScriptableObject.CreateInstance<DreamSequenceProfile>();
            profile.Audio = audio;
            string[] files = { "Day1_WhaleBeach", "Day2_EndlessRoad", "Day3_BoundJudgment" };
            for (int i = 0; i < profile.Days.Length; i++)
            {
                string path = "Assets/MiniGame/Art/Dreams/" + files[i] + ".png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.maxTextureSize = 2048; importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
                }
                profile.Days[i].Artwork = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (profile.Days[i].Artwork == null) throw new InvalidOperationException("尚未导入梦境画面：" + path);
            }
            AssetDatabase.CreateAsset(profile, ProfilePath);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        public static void EnsureDreamAudio(MiniGameAudioProfile audio)
        {
            var sounds = audio.Sounds.ToList();
            AddSound(sounds, MiniGameSound.Dream3, "Assets/MiniGame/Audio/Music/Day3_BoundJudgment.wav");
            AddSound(sounds, MiniGameSound.EndingMusic, null);
            AddSound(sounds, MiniGameSound.OceanWaves, "Assets/MiniGame/Art/DreamEnding/OceanWaves.wav");
            audio.Sounds = sounds.ToArray();
            EditorUtility.SetDirty(audio); AssetDatabase.SaveAssetIfDirty(audio);
        }

        private static void AddSound(List<MiniGameSoundEntry> entries, MiniGameSound id, string path)
        {
            if (entries.Any(entry => entry.Id == id)) return;
            AudioClip clip = null;
            var importer = path == null ? null : AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.Streaming; settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = .85f; settings.preloadAudioData = false;
                importer.defaultSampleSettings = settings; importer.loadInBackground = true; importer.SaveAndReimport();
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            entries.Add(new MiniGameSoundEntry { Id = id, Clip = clip });
        }

        public static PlayerRebellionProfile EnsureRebellionProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<PlayerRebellionProfile>(RebellionPath);
            if (profile != null) return profile;
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("请退出播放后首次创建反叛值配置。");
            EnsureProfile();
            profile = ScriptableObject.CreateInstance<PlayerRebellionProfile>();
            AssetDatabase.CreateAsset(profile, RebellionPath); AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        [MenuItem("MiniGame/梦境/接入当前 Combined")]
        public static void ApplyToCombined()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("请先退出播放。");
            var scene = SceneManager.GetSceneByPath(MainRoomGameplayBuilder.ScenePath);
            if (!scene.isLoaded) throw new InvalidOperationException("请先打开 MainRoomCombined 场景。");
            ConfigureOpenScene(EnsureProfile(), EnsureRebellionProfile());
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        public static void ConfigureOpenScene(DreamSequenceProfile profile, PlayerRebellionProfile rebellion)
        {
            var scene = SceneManager.GetSceneByPath(MainRoomGameplayBuilder.ScenePath);
            if (!scene.isLoaded) throw new InvalidOperationException("MainRoomCombined 未打开。");
            ReserveLayer(29, "DreamStage"); ReserveLayer(30, "SelfMonitor");
            var roots = scene.GetRootGameObjects();
            var host = roots.SelectMany(root => root.GetComponentsInChildren<MiniGameBootstrap>(true)).Single();
            var renderers = roots.SelectMany(root => root.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
            var wall = renderers.Single(renderer => renderer.name == "B_Wall__3_.001");
            var floor = renderers.Single(renderer => renderer.name == "B_Floor");
            var director = host.GetComponent<DreamSequenceDirector>();
            if (director == null) director = Undo.AddComponent<DreamSequenceDirector>(host.gameObject);
            var surfaceTransform = host.transform.Find("Dream wall projection");
            var pose = host.transform.Find("Dream viewing pose");
            Vector3 normal = wall.bounds.size.x < wall.bounds.size.z ? Vector3.right : Vector3.forward;
            if (Vector3.Dot(host.Player.ViewCamera.transform.position - wall.bounds.center, normal) < 0) normal = -normal;
            if (surfaceTransform == null)
            {
                var surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
                surface.name = "Dream wall projection";
                UnityEngine.Object.DestroyImmediate(surface.GetComponent<Collider>());
                surface.transform.SetParent(host.transform, false); surfaceTransform = surface.transform;
                float thickness = Mathf.Abs(Vector3.Dot(wall.bounds.size, normal)) * .5f;
                surfaceTransform.position = wall.bounds.center + normal * (thickness + .015f);
                surfaceTransform.rotation = Quaternion.LookRotation(-normal, Vector3.up);
                float height = Mathf.Min(2.7f, wall.bounds.size.y - profile.ProjectionInset * 2);
                surfaceTransform.localScale = new Vector3(height * 16 / 9, height, 1);
            }
            if (pose == null)
            {
                pose = new GameObject("Dream viewing pose").transform; pose.SetParent(host.transform, false);
                pose.SetPositionAndRotation(surfaceTransform.position + normal * 2.5f, Quaternion.LookRotation(-normal, Vector3.up));
            }
            const string materialPath = "Assets/MiniGame/Art/Dreams/WallProjection.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            { material = new Material(Shader.Find("MiniGame/Dream Projection Surface")); AssetDatabase.CreateAsset(material, materialPath); }
            var projection = surfaceTransform.GetComponent<Renderer>(); projection.sharedMaterial = material; projection.enabled = false;
            director.Configure(wall, pose, projection, host.Player.ViewCamera, profile);
            host.ConfigureDreams(profile, rebellion);
            var wake = roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "Bed approach");
            host.Player.ConfigureDreamTraversal(floor.bounds, wake, director);
            var reference = host.Player.ViewCamera.GetComponent<ReferenceCameraProjection>();
            if (reference != null) reference.ConfigureDreams(director);
            host.Player.ViewCamera.cullingMask &= ~((1 << 29) | (1 << 30));
            var effects = new SerializedObject(host.Effects);
            var terminal = effects.FindProperty("terminalSurface").objectReferenceValue as Renderer;
            host.GetComponent<BedtimeGuidance>().ConfigureSelfReview(terminal);
            foreach (var item in new UnityEngine.Object[] { host, director, host.Player, reference, host.GetComponent<BedtimeGuidance>() })
                if (item != null) EditorUtility.SetDirty(item);
        }

        private static void ReserveLayer(int index, string name)
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layer = settings.FindProperty("layers").GetArrayElementAtIndex(index);
            if (layer.stringValue != "" && layer.stringValue != name) throw new InvalidOperationException("图层 " + index + " 已被占用：" + layer.stringValue);
            layer.stringValue = name; settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

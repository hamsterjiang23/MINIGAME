using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniGame.UnityHost.Editor
{
    public static class PatientMonitorAssets
    {
        public const string ProfilePath = "Assets/MiniGame/Resources/MiniGame/PatientMonitorDefault.asset";
        public const string ArtPath = "Assets/MiniGame/Art/Characters/Surveillance";
        private static readonly string[] Prefixes = { "ML", "VV", "ED", "AD", "JN", "D0" };

        public static PatientMonitorProfile EnsureProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<PatientMonitorProfile>(ProfilePath);
            if (profile != null) return profile;
            EnsureFolder(Path.GetDirectoryName(ProfilePath).Replace('\\', '/'));
            profile = ScriptableObject.CreateInstance<PatientMonitorProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        public static int ImportArtwork(bool updateCatalog = true)
        {
            var replacements = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (string prefix in Prefixes)
            {
                string path = ArtPath + "/" + prefix + "-sheet.png";
                if (!File.Exists(path)) continue;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 4096;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                int count = prefix == "D0" ? 2 : 5;
                Rect crop = CharacterCrop(path, width, height, count, out var pivots);
                var sprites = new SpriteMetaData[count];
                for (int i = 0; i < count; i++)
                {
                    int left = Mathf.RoundToInt(i * width / (float)count);
                    int right = Mathf.RoundToInt((i + 1) * width / (float)count);
                    sprites[i] = new SpriteMetaData
                    {
                        name = prefix + (prefix == "D0" ? "_" : "") + (i + 1).ToString("00"),
                        rect = new Rect(left + crop.x, crop.y, Mathf.Min(crop.width, right - left - crop.x), crop.height),
                        alignment = (int)SpriteAlignment.Custom, pivot = pivots[i]
                    };
                }
                importer.spritesheet = sprites;
                var textureSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(textureSettings);
                textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(textureSettings);
                importer.SaveAndReimport();
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                    replacements[sprite.name] = FindRemadeSprite(sprite.name) ?? sprite;
            }
            if (updateCatalog && replacements.Count > 0)
            {
                var catalog = AssetDatabase.LoadAssetAtPath<PortraitCatalog>(CampaignBuilder.PortraitPath);
                if (catalog == null) throw new InvalidOperationException("请先导入正式人物目录。");
                var serialized = new SerializedObject(catalog);
                var entries = serialized.FindProperty("entries");
                var values = new List<PortraitCatalog.Entry>();
                for (int i = 0; i < entries.arraySize; i++)
                {
                    var entry = entries.GetArrayElementAtIndex(i);
                    string id = entry.FindPropertyRelative("id").stringValue;
                    var original = entry.FindPropertyRelative("sprite").objectReferenceValue as Sprite;
                    values.Add(new PortraitCatalog.Entry { id = id,
                        sprite = replacements.TryGetValue(id, out var replacement) ? replacement : original });
                }
                catalog.Configure(values.ToArray());
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssetIfDirty(catalog);
            }
            return replacements.Count;
        }

        // Slice metadata only; keep the generated source PNG unchanged. One union crop per
        // character keeps its feet, scale and framing stable across all expressions.
        internal static Rect CharacterCrop(string path, int width, int height, int count, out Vector2[] pivots)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("PNG 解码失败：" + path);
                var pixels = texture.GetPixels32();
                var centers = new float[count];
                var feet = new int[count];
                int minX = width, maxX = -1, minY = height, maxY = -1;
                for (int cell = 0; cell < count; cell++)
                {
                    int left = Mathf.RoundToInt(cell * width / (float)count);
                    int right = Mathf.RoundToInt((cell + 1) * width / (float)count);
                    int frameMin = width, frameMax = -1, frameBottom = height;
                    for (int y = 0; y < height; y++)
                        for (int x = left; x < right; x++)
                        {
                            if (pixels[y * width + x].a <= 16) continue;
                            minX = Math.Min(minX, x - left); maxX = Math.Max(maxX, x - left);
                            minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                            frameMin = Math.Min(frameMin, x - left); frameMax = Math.Max(frameMax, x - left);
                            frameBottom = Math.Min(frameBottom, y);
                        }
                    if (frameMax < frameMin) throw new InvalidOperationException("表情切片全透明：" + path + " / " + cell);
                    centers[cell] = (frameMin + frameMax) * .5f;
                    feet[cell] = frameBottom;
                }
                if (maxX < minX) throw new InvalidOperationException("立绘全透明：" + path);
                minX = Math.Max(0, minX - 2); maxX = Math.Min(Mathf.FloorToInt(width / (float)count) - 1, maxX + 2);
                minY = Math.Max(0, minY - 2); maxY = Math.Min(height - 1, maxY + 2);
                var crop = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
                pivots = new Vector2[count];
                for (int i = 0; i < count; i++) pivots[i] = new Vector2((centers[i] - minX) / crop.width, (feet[i] - minY) / crop.height);
                return crop;
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        public static Sprite FindRemadeSprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var named = AssetDatabase.LoadAllAssetsAtPath("Assets/MiniGame/Art/Characters/NamedPortraits/" + NamedPortraitImport.NamedId(id) + ".png")
                .OfType<Sprite>().FirstOrDefault();
            if (named != null) return named;
            string prefix = id.StartsWith("D0_", StringComparison.Ordinal) ? "D0" : id.Substring(0, Math.Min(2, id.Length));
            return AssetDatabase.LoadAllAssetsAtPath(ArtPath + "/" + prefix + "-sheet.png")
                .OfType<Sprite>().FirstOrDefault(sprite => sprite.name == id);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
        }
    }

    public sealed class PatientMonitorWindow : EditorWindow
    {
        private PatientMonitorProfile profile;
        private SerializedObject serialized;
        private Vector2 scroll;
        private int roomIndex, expression;
        private Scene previewScene;
        private GameObject previewOwner;
        private PatientSurveillanceFeed preview;
        private double lastTick;
        private string message = "预览仅展示画面，不改变会谈与处置。";

        [MenuItem("MiniGame/人物监控室")]
        public static void Open() => GetWindow<PatientMonitorWindow>("人物监控室");

        private void OnEnable()
        {
            profile = PatientMonitorAssets.EnsureProfile();
            serialized = new SerializedObject(profile);
            EditorApplication.update += TickPreview;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
        private void OnDisable()
        {
            EditorApplication.update -= TickPreview;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            StopPreview();
        }
        private void OnPlayModeChanged(PlayModeStateChange state) => StopPreview();

        private void OnGUI()
        {
            if (profile == null) return;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("保存配置", EditorStyles.toolbarButton)) AssetDatabase.SaveAssetIfDirty(profile);
                if (GUILayout.Button("定位配置", EditorStyles.toolbarButton)) EditorGUIUtility.PingObject(profile);
                if (GUILayout.Button("更新完整PNG目录", EditorStyles.toolbarButton))
                {
                    StopPreview();
                    NamedPortraitImport.Run();
                    message = "已更新命名PNG与表情目录；原图保持不变。";
                }
            }
            EditorGUILayout.HelpBox(message + " 参数可撤销；播放中保存也会保留。", MessageType.Info);
            var rooms = profile.Rooms ?? Array.Empty<PatientMonitorProfile.Room>();
            string[] labels = rooms.Select(room => room == null ? "缺失房间" : room.Label + " / " + room.PatientId).ToArray();
            if (rooms.Length == 0) { EditorGUILayout.HelpBox("请配置人物房间。", MessageType.Warning); return; }
            EditorGUI.BeginChangeCheck();
            roomIndex = EditorGUILayout.Popup("人物房间", Mathf.Clamp(roomIndex, 0, rooms.Length - 1), labels);
            expression = EditorGUILayout.IntSlider("表情编号", expression + 1, 1, NamedPortraitImport.ExpressionCount(rooms[roomIndex]?.PatientId)) - 1;
            if (EditorGUI.EndChangeCheck() && preview != null) ApplyPreviewPortrait();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("独立监控预览")) StartPreview();
                if (GUILayout.Button("停止预览")) StopPreview();
                if (preview != null && GUILayout.Button(preview.IsCloseUp ? "切换全景" : "查看表情近景")) preview.SetCloseUp(!preview.IsCloseUp);
            }
            if (preview != null && preview.Output != null)
            {
                Rect rect = GUILayoutUtility.GetAspectRect((float)preview.Output.width / preview.Output.height);
                GUI.DrawTexture(rect, preview.Output, ScaleMode.ScaleToFit, false);
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            serialized.Update();
            foreach (string property in new[] { "Enabled", "Backdrop", "Width", "Height", "FramesPerSecond", "Exposure", "Saturation", "Noise", "IdleMotion" })
                EditorGUILayout.PropertyField(serialized.FindProperty(property));
            EditorGUILayout.Space();
            var roomProperty = serialized.FindProperty("Rooms").GetArrayElementAtIndex(roomIndex);
            if (serialized.FindProperty("Backdrop").objectReferenceValue == null)
                EditorGUILayout.PropertyField(roomProperty, new GUIContent(labels[roomIndex]), true);
            else
            {
                foreach (string property in new[] { "PatientId", "Label", "ActorHeight", "ActorOffset" })
                    EditorGUILayout.PropertyField(roomProperty.FindPropertyRelative(property));
                EditorGUILayout.PropertyField(serialized.FindProperty("PortraitPlacements"), true);
            }
            if (serialized.ApplyModifiedProperties())
            {
                profile.MarkChanged();
                EditorUtility.SetDirty(profile);
                if (previewOwner != null) previewOwner.GetComponent<RectTransform>().sizeDelta = new Vector2(profile.Width, profile.Height);
            }
            EditorGUILayout.EndScrollView();
        }

        private void StartPreview()
        {
            StopPreview();
            previewScene = EditorSceneManager.NewPreviewScene();
            previewOwner = new GameObject("Patient monitor editor preview", typeof(RectTransform), typeof(RawImage), typeof(PatientSurveillanceFeed));
            previewOwner.hideFlags = HideFlags.HideAndDontSave;
            SceneManager.MoveGameObjectToScene(previewOwner, previewScene);
            preview = previewOwner.GetComponent<PatientSurveillanceFeed>();
            preview.UseExternalClock = true;
            preview.Initialize(previewOwner.GetComponent<RawImage>(), profile);
            previewOwner.GetComponent<RectTransform>().sizeDelta = new Vector2(profile.Width, profile.Height);
            lastTick = EditorApplication.timeSinceStartup;
            ApplyPreviewPortrait();
        }

        private void ApplyPreviewPortrait()
        {
            var room = profile.Rooms[Mathf.Clamp(roomIndex, 0, profile.Rooms.Length - 1)];
            if (room == null) return;
            string baseline = PortraitCatalog.BasePortrait(room.PatientId);
            string id = baseline.Substring(0, Math.Max(0, baseline.Length - 2)) + (expression + 1).ToString("00");
            var catalog = Resources.Load<PortraitCatalog>("MiniGame/PortraitCatalog");
            preview.Show(room.PatientId, catalog != null ? catalog.Find(id) : null, false);
        }

        private void TickPreview()
        {
            if (preview == null) return;
            previewOwner.GetComponent<RectTransform>().sizeDelta = new Vector2(profile.Width, profile.Height);
            double now = EditorApplication.timeSinceStartup;
            preview.Advance((float)(now - lastTick));
            lastTick = now;
            Repaint();
        }
        private void StopPreview()
        {
            if (previewOwner != null) DestroyImmediate(previewOwner);
            preview = null;
            previewOwner = null;
            if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
            previewScene = default;
        }
    }
}

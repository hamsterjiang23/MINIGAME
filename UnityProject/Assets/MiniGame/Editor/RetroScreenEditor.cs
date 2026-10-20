using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    [CustomEditor(typeof(RetroScreenProfile))]
    public sealed class RetroScreenProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Draw("effectEnabled", "启用效果", "关闭时直接显示原始画面。");
            Draw("strength", "整体强度", "0 为原图，1 为完整效果；混合包含调色、像素化与色阶变化。");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("分区调色", EditorStyles.boldLabel);
            Draw("colorGradingEnabled", "启用分区调色", "默认关闭。只改变画面的光色，物体遮挡仍由真实灯光和阴影决定。");
            using (new EditorGUI.DisabledScope(!serializedObject.FindProperty("colorGradingEnabled").boolValue))
            {
                Draw("postExposure", "曝光补偿（EV）", "+1 为两倍曝光；先小幅提亮中间调，避免灯泡和电脑过曝。");
                Draw("contrast", "对比度", "0 不调整。适当降低可保留暗处纹理。");
                Draw("saturation", "饱和度", "0 不调整；-100 为灰度。");
                Draw("neutralTonemapping", "Neutral 高光压缩", "可选地压缩 HDR 高光；默认关闭，不替代真实灯光。");
                Draw("shadowTint", "暗部偏色", "白色不偏色；可少量偏灰绿或橄榄绿。");
                Draw("shadowExposure", "暗部亮度（EV）");
                Draw("midtoneTint", "中间调偏色", "白色不偏色；可少量偏土黄、黄绿。");
                Draw("midtoneExposure", "中间调亮度（EV）");
                Draw("highlightTint", "高光偏色", "白色不偏色；可少量偏暖米黄。");
                Draw("highlightExposure", "高光亮度（EV）");
                Draw("shadowEnd", "暗部过渡终点", "按画面亮度划分区域，颜色会平滑过渡。");
                Draw("highlightStart", "高光过渡起点", "高于暗部过渡终点，避免区域重叠异常。");
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("可选色阶与静态抖动", EditorStyles.boldLabel);
            Draw("fullscreenQuantization", "启用全屏色阶压缩", "自然聚光灯默认关闭。开启后对最终画面亮度分档，可能加强光斑边界。");
            using (new EditorGUI.DisabledScope(!serializedObject.FindProperty("fullscreenQuantization").boolValue))
            {
                Draw("perceptualQuantization", "按感知亮度分档", "开启时按亮度保留色相比率；关闭时保持旧版最大通道分档，兼容已有配置。");
                Draw("levels", "色阶分段数", "明暗值的分段数，不是整幅画面的颜色总数；越少，分层越明显。");
                Draw("quantizationStrength", "色阶强度", "0 为连续亮度，1 为完全分档；先从较低强度试起。");
                Draw("ditherStrength", "量化前抖动强度", "0 关闭；可尝试 0.1～0.25，让边界破碎。噪声固定在像素格上，不随时间闪动。");
                Draw("gammaBefore", "暗部层次（前置 Gamma）", "改变明暗分档的位置；小于 1 时为暗部提供更多分档空间。");
                Draw("gammaAfter", "输出明暗（后置 Gamma）", "调整分档后的明暗；大于 1 通常使中间调变暗。");
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("像素化", EditorStyles.boldLabel);
            Draw("useVirtualResolution", "按虚拟分辨率像素化", "宽度根据画面比例计算；不会把小窗口放大成更多虚拟像素。");
            if (serializedObject.FindProperty("useVirtualResolution").boolValue)
            {
                Draw("virtualHeight", "虚拟画面高度", "16:9 时：540 对应 960×540；360 对应 640×360。");
            }
            else Draw("pixelSize", "像素尺寸", "单位为输出像素。1 不额外像素化；越大，颗粒越粗。");
            if (serializedObject.ApplyModifiedProperties()) RetroScreenPreview.Refresh();
            EditorGUILayout.HelpBox("Game 视图实时预览，支持撤销/重做。播放期间编辑的是配置资产，保存后退出仍保留。Overlay UI 保持清晰；梦境播放继续自动旁路。", MessageType.Info);
            var profile = (RetroScreenProfile)target;
            bool original = RetroScreenPreview.IsOriginal(profile);
            bool next = GUILayout.Toggle(original, original ? "正在查看原图 · 点击返回效果" : "临时查看原图", "Button");
            if (next != original) RetroScreenPreview.SetOriginal(profile, next);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("自然画面预设"))
                {
                    Undo.RecordObject(profile, "恢复自然画面"); profile.ResetToNaturalDefaults();
                    EditorUtility.SetDirty(profile); RetroScreenPreview.Refresh();
                }
                if (GUILayout.Button("参考调色起点"))
                {
                    Undo.RecordObject(profile, "应用参考调色起点"); profile.ResetToReferenceStyle();
                    EditorUtility.SetDirty(profile); RetroScreenPreview.Refresh();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("恢复轻度默认"))
                {
                    Undo.RecordObject(profile, "恢复复古效果默认值");
                    profile.ResetToLightDefaults();
                    EditorUtility.SetDirty(profile);
                    RetroScreenPreview.Refresh();
                }
                if (GUILayout.Button("保存配置"))
                {
                    EditorUtility.SetDirty(profile);
                    AssetDatabase.SaveAssetIfDirty(profile);
                }
            }
        }

        private void Draw(string property, string label, string tooltip = "") =>
            EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(label, tooltip));
        private void OnDisable() => RetroScreenPreview.Clear();
    }

    public sealed class RetroScreenWindow : EditorWindow
    {
        private RetroScreenProfile profile;
        private UnityEditor.Editor profileEditor;
        private Vector2 scroll;

        [MenuItem("MiniGame/画面/复古效果")]
        [MenuItem("MiniGame/画面/调色与像素风格")]
        public static void Open() => GetWindow<RetroScreenWindow>("调色与像素风格").Show();

        private void OnGUI()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var resolved = scene.path == MainRoomGameplayBuilder.ScenePath ? RetroScreenSetup.FindProfile(scene) : null;
            if (resolved == null) resolved = AssetDatabase.LoadAssetAtPath<RetroScreenProfile>(RetroScreenSetup.ProfilePath);
            if (profile != resolved)
            {
                RetroScreenPreview.Clear();
                profile = resolved;
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                if (GUILayout.Button("房间灯光", EditorStyles.toolbarButton)) RoomLightingWindow.Open();
            EditorGUILayout.LabelField("主场景 · 调色与像素风格", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("当前配置", profile, typeof(RetroScreenProfile), false);
            if (profile == null)
            {
                EditorGUILayout.HelpBox("尚未创建默认配置。", MessageType.Warning);
                if (GUILayout.Button("创建默认配置")) profile = RetroScreenSetup.EnsureProfile();
                return;
            }
            using (var view = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = view.scrollPosition;
                UnityEditor.Editor.CreateCachedEditor(profile, typeof(RetroScreenProfileEditor), ref profileEditor);
                profileEditor.OnInspectorGUI();
            }
            if (GUILayout.Button("在 Project 中定位配置")) EditorGUIUtility.PingObject(profile);
        }

        private void OnDisable()
        {
            RetroScreenPreview.Clear();
            if (profileEditor != null) DestroyImmediate(profileEditor);
        }
    }

    [InitializeOnLoad]
    internal static class RetroScreenPreview
    {
        private static RetroScreenProfile originalProfile;
        static RetroScreenPreview()
        {
            Undo.undoRedoPerformed += Refresh;
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
            EditorApplication.playModeStateChanged += _ => Clear();
        }
        public static bool IsOriginal(RetroScreenProfile profile) => originalProfile == profile;
        public static void SetOriginal(RetroScreenProfile profile, bool original)
        {
            Clear();
            originalProfile = original ? profile : null;
            foreach (var effect in Object.FindObjectsOfType<RetroScreenEffect>())
                effect.PreviewOriginal = original && effect.Profile == profile;
            Refresh();
        }
        public static void Clear()
        {
            originalProfile = null;
            foreach (var effect in Object.FindObjectsOfType<RetroScreenEffect>()) effect.PreviewOriginal = false;
            Refresh();
        }
        public static void Refresh()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }

    public static class RetroScreenSetup
    {
        public const string ProfilePath = "Assets/MiniGame/Resources/RetroScreenDefault.asset";
        public const string ShaderPath = "Assets/MiniGame/Resources/RetroScreen.shader";

        public static RetroScreenProfile EnsureProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<RetroScreenProfile>(ProfilePath);
            if (profile != null) return profile;
            profile = ScriptableObject.CreateInstance<RetroScreenProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        public static RetroScreenProfile FindProfile(UnityEngine.SceneManagement.Scene scene) =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<RetroScreenEffect>(true))
                .Select(effect => effect.Profile).FirstOrDefault(profile => profile != null);

        public static RetroScreenProfile FindSavedProfile(string scenePath)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scenePath);
            if (scene.IsValid() && scene.isLoaded) return FindProfile(scene);
            if (!System.IO.File.Exists(scenePath)) return null;
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try { return FindProfile(scene); }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            }
        }

        public static void Attach(Camera camera, RetroScreenProfile retainedProfile = null)
        {
            var effect = camera.GetComponent<RetroScreenEffect>();
            if (effect == null) effect = camera.gameObject.AddComponent<RetroScreenEffect>();
            effect.Configure(retainedProfile != null ? retainedProfile : effect.Profile != null ? effect.Profile : EnsureProfile(),
                AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath));
            EditorUtility.SetDirty(effect);
        }

        // Add only the effect. Never invoke the room generator just to install post-processing.
        [MenuItem("MiniGame/画面/为当前主场景安装复古效果")]
        public static void InstallInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("请退出播放后安装效果；调参可在播放中进行。");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != MainRoomGameplayBuilder.ScenePath)
                throw new System.InvalidOperationException("请先打开 MainRoomCombined 场景。");
            var camera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>(true)).Single();
            Undo.RegisterFullObjectHierarchyUndo(camera.gameObject, "安装复古效果");
            Attach(camera);
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}

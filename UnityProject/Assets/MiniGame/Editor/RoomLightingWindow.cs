using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    [CustomEditor(typeof(RoomLightingProfile))]
    public sealed class RoomLightingProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update(); DrawSettings(serializedObject);
            if (serializedObject.ApplyModifiedProperties()) RefreshRooms();
        }

        internal static void DrawSettings(SerializedObject settings)
        {
            EditorGUILayout.LabelField("吊灯聚光灯", EditorStyles.boldLabel);
            Draw(settings, "lampOffset", "灯罩开口偏移（米）", "Y 保持负值，防止光源进入灯罩产生椭圆投影。");
            Draw(settings, "lampColor", "灯光颜色"); Draw(settings, "lampIntensity", "灯光强度");
            Draw(settings, "lampRange", "照射距离（米）"); Draw(settings, "outerAngle", "外圈角度");
            Draw(settings, "innerAngle", "内圈角度", "使用遮罩控制 Built-in 聚光灯的过渡范围。");
            Draw(settings, "shadowStrength", "阴影强度"); Draw(settings, "shadowBias", "阴影深度偏移");
            Draw(settings, "shadowNormalBias", "阴影法线偏移"); Draw(settings, "bulbEmission", "灯泡自发光");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("环境与窗边", EditorStyles.boldLabel);
            Draw(settings, "ambientColor", "暗部环境色"); Draw(settings, "reflectionIntensity", "环境反射强度");
            Draw(settings, "windowColor", "窗边冷光颜色"); Draw(settings, "windowIntensity", "窗边冷光强度");
            Draw(settings, "windowRange", "窗边冷光范围"); Draw(settings, "monitorIntensity", "电脑微光强度");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("可选照明分层（默认关闭）", EditorStyles.boldLabel);
            Draw(settings, "lightingBandsEnabled", "启用照明分层", "仅对材质受到的灯光分档，不压缩木纹、海报等贴图颜色。");
            Draw(settings, "lightingBands", "亮度分段数");
            Draw(settings, "lightingBandsStrength", "分层强度", "0 为连续灯光，1 为完全分档；默认 0.4，保留衰减过渡。");
            Draw(settings, "lightingDither", "分界细碎程度");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("木地板", EditorStyles.boldLabel);
            Draw(settings, "floorGain", "木纹亮度"); Draw(settings, "floorContrast", "木纹对比度"); Draw(settings, "floorSmoothness", "地板光滑度");
            EditorGUILayout.Space(); EditorGUILayout.LabelField("地面地毯", EditorStyles.boldLabel);
            Draw(settings, "rugVisible", "显示地毯"); Draw(settings, "rugOffset", "地毯相对地板中心位置", "Y 为离地高度，最小 0.008 米。");
            Draw(settings, "rugSize", "地毯长宽（米）"); Draw(settings, "rugYaw", "地毯旋转角度"); Draw(settings, "rugTint", "地毯色调");
        }

        private static void Draw(SerializedObject settings, string property, string label, string tooltip = "") =>
            EditorGUILayout.PropertyField(settings.FindProperty(property), new GUIContent(label, tooltip));

        internal static void RefreshRooms()
        {
            foreach (var controller in Resources.FindObjectsOfTypeAll<RoomLightingController>()
                .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)) controller.ApplyNow();
            SceneView.RepaintAll(); EditorApplication.QueuePlayerLoopUpdate();
        }
    }

    public sealed class RoomLightingWindow : EditorWindow
    {
        private RoomLightingProfile profile;
        private Vector2 scroll;
        private bool preview;
        [MenuItem("MiniGame/画面/房间灯光")]
        public static void Open() => GetWindow<RoomLightingWindow>("房间灯光");
        private void OnEnable() { preview = false; Undo.undoRedoPerformed += OnUndo; Resolve(); }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
            foreach (var room in Resources.FindObjectsOfTypeAll<RoomLightingController>()) { room.PreviewUnbanded = false; room.ApplyNow(); }
        }
        private void OnUndo() { RoomLightingProfileEditor.RefreshRooms(); Repaint(); }
        private void Resolve()
        {
            var controller = RoomLightingSetup.FindController(SceneManager.GetActiveScene());
            profile = controller != null ? controller.Profile : AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(RoomLightingSetup.ProfilePath);
        }
        private void OnGUI()
        {
            if (profile == null) Resolve();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("保存配置", EditorStyles.toolbarButton) && profile != null)
                {
                    EditorUtility.SetDirty(profile); AssetDatabase.SaveAssetIfDirty(profile);
                    RoomLightingProfileEditor.RefreshRooms();
                }
                if (GUILayout.Button("定位配置", EditorStyles.toolbarButton) && profile != null) { Selection.activeObject = profile; EditorGUIUtility.PingObject(profile); }
                if (GUILayout.Button("调色与像素风格", EditorStyles.toolbarButton)) RetroScreenWindow.Open();
                bool next = GUILayout.Toggle(preview, "临时关闭分层", EditorStyles.toolbarButton);
                if (next != preview)
                {
                    preview = next;
                    foreach (var room in Resources.FindObjectsOfTypeAll<RoomLightingController>()) { room.PreviewUnbanded = preview; room.ApplyNow(); }
                }
            }
            EditorGUILayout.HelpBox("调整的是配置资产，支持撤销/重做。播放中保存后退出仍保留。临时关闭分层只用于对比，不改参数；夜间仍由床引导控制灯光。", MessageType.Info);
            if (profile == null)
            {
                EditorGUILayout.HelpBox("请打开 MainRoomCombined，再安装房间灯光。", MessageType.Warning);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("在当前主场景安装")) { RoomLightingSetup.InstallInOpenScene(); Resolve(); }
                return;
            }
            var settings = new SerializedObject(profile); settings.Update();
            scroll = EditorGUILayout.BeginScrollView(scroll); RoomLightingProfileEditor.DrawSettings(settings); EditorGUILayout.EndScrollView();
            if (settings.ApplyModifiedProperties()) { EditorUtility.SetDirty(profile); RoomLightingProfileEditor.RefreshRooms(); }
        }
    }
}

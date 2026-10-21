using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public sealed class TerminalSignalWindow : EditorWindow
    {
        private const string DefaultPath = "Assets/MiniGame/Resources/MiniGame/TerminalSignalDefault.asset";
        private TerminalSignalProfile profile;
        private SerializedObject serialized;
        private Vector2 scroll;
        private int group;
        private string message;
        private bool saveFailed;

        [MenuItem("MiniGame/画面/标志与监控扰动")]
        public static void Open() => GetWindow<TerminalSignalWindow>("标志与监控扰动");

        private void OnEnable()
        {
            minSize = new Vector2(360, 450);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeChanged;

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            profile = null;
            serialized = null;
            message = null;
            Repaint();
        }

        private void OnGUI()
        {
            var controller = Application.isPlaying ? FindObjectOfType<TerminalSignalController>() : null;
            var target = controller != null
                ? controller.Profile
                : AssetDatabase.LoadAssetAtPath<TerminalSignalProfile>(DefaultPath);
            if (target == null)
            {
                EditorGUILayout.HelpBox("尚未找到扰动配置：" + DefaultPath, MessageType.Info);
                return;
            }
            if (profile != target || serialized == null)
            {
                profile = target;
                serialized = new SerializedObject(profile);
            }
            bool live = controller != null;
            EditorGUILayout.HelpBox(live
                ? "正在编辑游戏中的实时参数。F6 可打开游戏内面板，保存后下次启动继续生效。"
                : "正在编辑默认配置资产。游戏内已保存的个人参数会优先使用。", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button(live ? "保存运行参数" : "保存默认资产", EditorStyles.toolbarButton))
                {
                    saveFailed = live && !controller.SaveSettings();
                    if (!live) AssetDatabase.SaveAssetIfDirty(profile);
                    message = saveFailed ? "保存失败，请查看 Console 中的提示。" : "配置已保存。";
                }
                if (live && GUILayout.Button(controller.PanelOpen ? "关闭游戏面板" : "打开游戏面板", EditorStyles.toolbarButton))
                    controller.SetPanelOpen(!controller.PanelOpen);
                if (!live && GUILayout.Button("定位资产", EditorStyles.toolbarButton))
                    EditorGUIUtility.PingObject(profile);
            }
            if (live)
                controller.FreezePreview = EditorGUILayout.Toggle("冻结扰动预览", controller.FreezePreview);
            if (!string.IsNullOrEmpty(message))
                EditorGUILayout.HelpBox(message, saveFailed ? MessageType.Error : MessageType.Info);

            group = GUILayout.Toolbar(group, new[] { "Norm 标志", "监控画面" });
            scroll = EditorGUILayout.BeginScrollView(scroll);
            serialized.Update();
            var parameters = serialized.FindProperty(group == 0 ? "Logo" : "Monitor");
            if (parameters == null)
            {
                EditorGUILayout.HelpBox("配置缺少 Logo / Monitor 参数组。", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }
            EditorGUILayout.PropertyField(parameters.FindPropertyRelative("Enabled"), new GUIContent("启用扰动"));
            DrawSlider(parameters, "Intensity", "总强度", 0, 1);
            DrawSlider(parameters, "Tear", "横向撕裂（画宽比例）", 0, .12f);
            DrawSlider(parameters, "RgbSplit", "RGB 分离（画宽比例）", 0, .03f);
            DrawSlider(parameters, "Noise", "信号噪声", 0, .25f);
            DrawSlider(parameters, "ScanlineStrength", "扫描线强度", 0, .8f);
            DrawSlider(parameters, "ScanlineDensity", "扫描线密度（条）", 20, 600);
            DrawSlider(parameters, "ScanlineSpeed", "扫描线速度（周期/秒）", 0, 10);
            DrawSlider(parameters, "RollStrength", "滚动干扰强度", 0, .15f);
            DrawSlider(parameters, "RollSpeed", "滚动干扰速度（屏/秒）", 0, 3);
            DrawSlider(parameters, "Rate", "随机刷新（次/秒）", 1, 30);
            DrawSlider(parameters, "BurstFrequency", "突发频率（次/秒）", .1f, 10);
            DrawSlider(parameters, "BurstDuration", "突发时长（秒）", .02f, .8f);
            if (serialized.ApplyModifiedProperties())
            {
                if (live)
                {
                    controller.ApplySettings();
                    controller.Panel?.Refresh();
                }
                else EditorUtility.SetDirty(profile);
                message = "参数已修改，点击保存以保留。";
                saveFailed = false;
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSlider(SerializedProperty parent, string name, string label, float min, float max)
        {
            var property = parent.FindPropertyRelative(name);
            if (property == null) return;
            EditorGUILayout.Slider(property, min, max, new GUIContent(label));
        }
    }
}

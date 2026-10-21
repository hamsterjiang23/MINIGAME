using System;
using System.Collections.Generic;
using System.IO;
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
        private RoomLightingPeriod editingPeriod;
        public override void OnInspectorGUI()
        {
            editingPeriod = (RoomLightingPeriod)GUILayout.Toolbar((int)editingPeriod, new[] { "编辑白天", "编辑夜晚" });
            serializedObject.Update(); DrawSettings(serializedObject, editingPeriod);
            if (serializedObject.ApplyModifiedProperties()) ApplyInspectorChanges((RoomLightingProfile)target);
            if (!EditorUtility.IsPersistent(target))
                EditorGUILayout.HelpBox("这是临时运行配置。修改仅影响当前实例；要退出播放后保留，请在房间灯光窗口编辑 Project 配置资产。", MessageType.Info);
        }

        public static void ApplyInspectorChanges(RoomLightingProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (EditorUtility.IsPersistent(profile)) RoomLightingAuthoring.ProfileEdited(profile);
            else { profile.NotifyChanged(); RefreshRooms(); }
        }

        internal static void DrawSettings(SerializedObject settings) => DrawSettings(settings, RoomLightingPeriod.Day);
        internal static void DrawSettings(SerializedObject settings, RoomLightingPeriod period)
        {
            if (period == RoomLightingPeriod.Night)
            {
                EditorGUILayout.LabelField("夜间环境与默认衰减", EditorStyles.boldLabel);
                var night = settings.FindProperty("night");
                EditorGUILayout.PropertyField(night.FindPropertyRelative("ambientColor"), new GUIContent("夜间环境色"));
                EditorGUILayout.PropertyField(night.FindPropertyRelative("reflectionIntensity"), new GUIContent("夜间反射强度"));
                EditorGUILayout.PropertyField(night.FindPropertyRelative("lightMultiplier"), new GUIContent("未覆盖灯光倍率", "没有单独设置的普通灯相对白天的强度倍率；床引导灯有独立基准。"));
                EditorGUILayout.PropertyField(night.FindPropertyRelative("emissionMultiplier"), new GUIContent("夜间自发光倍率"));
                EditorGUILayout.PropertyField(night.FindPropertyRelative("fadeSeconds"), new GUIContent("昼夜过渡时间（秒）"));
                EditorGUILayout.HelpBox("夜间每盏灯可在下方选择后单独调整。已单独设置的灯使用自己的强度，不再叠加默认灯光倍率。", MessageType.None);
                return;
            }
            EditorGUILayout.LabelField("吊灯聚光灯", EditorStyles.boldLabel);
            Draw(settings, "lampOffset", "灯罩开口偏移（米）", "Y 保持负值，防止光源进入灯罩产生椭圆投影。");
            Draw(settings, "lampEulerAngles", "吊灯世界旋转（度）");
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
                .Where(item => item.gameObject.scene.IsValid() && item.gameObject.scene.isLoaded)) controller.ApplyFromProfile();
            SceneView.RepaintAll(); EditorApplication.QueuePlayerLoopUpdate();
        }
    }

    public sealed class RoomLightingWindow : EditorWindow
    {
        private static readonly LightType[] LightTypes = { LightType.Point, LightType.Spot, LightType.Directional };
        private static readonly string[] TypeLabels = { "点光源", "聚光灯", "平行光" };
        private static readonly LightShadows[] Shadows = { LightShadows.None, LightShadows.Hard, LightShadows.Soft };
        private static readonly string[] ShadowLabels = { "无阴影", "硬阴影", "软阴影" };
        [SerializeField] private string selectedPath = "";
        [SerializeField] private bool showBaseSettings, sceneHandles = true;
        [SerializeField] private Vector2 scroll;
        [SerializeField] private RoomLightingPeriod editingPeriod;
        [SerializeField] private bool followSelection = true;
        [SerializeField] private RoomLightingController targetRoom;
        [SerializeField] private string presetFolder = "";
        private string presetMessage = "";
        private MessageType presetMessageType = MessageType.Info;
        private RoomLightingController controller;
        private RoomLightingProfile profile;
        private readonly Dictionary<RoomLightingController, bool> previewStates = new Dictionary<RoomLightingController, bool>();
        private bool toolsOwned, previousToolsHidden;
        private int handleUndoGroup = -1;
        public string SelectedHierarchyPath => selectedPath;
        public int SceneHandleUndoGroup => handleUndoGroup;

        [MenuItem("MiniGame/画面/房间灯光")]
        public static void Open()
        {
            var window = GetWindow<RoomLightingWindow>("房间灯光"); window.Show();
            EnableScenePreview();
        }
        private void OnEnable()
        {
            minSize = new Vector2(420, 360); autoRepaintOnSceneChange = true;
            Undo.undoRedoPerformed += RebindAndRepaint;
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.hierarchyChanged += RebindAndRepaint;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;
            SceneView.duringSceneGui += DuringSceneGUI;
            Resolve(); EnableScenePreview();
        }
        private void OnDisable()
        {
            EndSceneHandleInteraction();
            Undo.undoRedoPerformed -= RebindAndRepaint;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.hierarchyChanged -= RebindAndRepaint;
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneView.duringSceneGui -= DuringSceneGUI;
            EditorApplication.delayCall -= RebindAndRepaint;
            SetToolsHidden(false);
            ReleaseController(controller);
            foreach (var pair in previewStates)
                if (pair.Key != null) { pair.Key.PreviewUnbanded = pair.Value; pair.Key.ApplyFromProfile(); }
            previewStates.Clear(); RoomLightingAuthoring.SavePendingChanges();
        }
        private void OnInspectorUpdate() { Resolve(); Repaint(); }
        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            EndSceneHandleInteraction(); SetToolsHidden(false); ReleaseController(controller);
            controller = null; Resolve(); EditorApplication.delayCall += RebindAndRepaint;
        }
        private void OnSceneChanged(Scene previous, Scene next)
        { EndSceneHandleInteraction(); RebindAndRepaint(); }
        private void RebindAndRepaint() { if (this == null) return; Resolve(); Repaint(); }
        private void Resolve()
        {
            // A save may briefly activate a different scene to serialize its day environment; keep this window's target and previews intact.
            if (RoomLightingAuthoring.IsSavingScenePreview) return;
            var next = followSelection ? RoomLightingAuthoring.FindController() : targetRoom;
            if (next != null && (!next.gameObject.scene.IsValid() || !next.gameObject.scene.isLoaded)) next = null;
            if (controller != next)
            { EndSceneHandleInteraction(); SetToolsHidden(false); ReleaseController(controller); selectedPath = ""; }
            controller = next;
            RoomLightingAuthoring.SetEditingPeriod(controller, editingPeriod);
            profile = controller != null ? controller.Profile : RoomLightingAuthoring.GetLoadedControllers().Length == 0
                ? AssetDatabase.LoadAssetAtPath<RoomLightingProfile>(RoomLightingSetup.ProfilePath) : null;
        }
        private void ReleaseController(RoomLightingController room)
        {
            if (room == null) return;
            RoomLightingAuthoring.ClearEditingPeriod(room);
            room.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            if (previewStates.TryGetValue(room, out bool previous))
            { room.PreviewUnbanded = previous; previewStates.Remove(room); room.ApplyFromProfile(); }
        }
        private void OnSelectionChanged()
        {
            EndSceneHandleInteraction();
            Resolve();
            var light = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Light>() : null;
            if (controller != null && light != null && controller.IsEditableSceneLight(light, editingPeriod)) selectedPath = controller.GetHierarchyPath(light);
            SetToolsHidden(false); Repaint();
        }

        public static void EnableScenePreview()
        {
            foreach (SceneView view in SceneView.sceneViews)
            { view.sceneLighting = true; view.sceneViewState.showImageEffects = true; view.Repaint(); }
        }

        private void OnGUI()
        {
            Resolve();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                using (new EditorGUI.DisabledScope(profile == null || !EditorUtility.IsPersistent(profile)))
                    if (GUILayout.Button("保存配置", EditorStyles.toolbarButton)) RoomLightingAuthoring.SaveProfile(profile);
                if (GUILayout.Button("定位配置", EditorStyles.toolbarButton) && profile != null) { Selection.activeObject = profile; EditorGUIUtility.PingObject(profile); }
                if (GUILayout.Button("场景真实预览", EditorStyles.toolbarButton)) EnableScenePreview();
                if (GUILayout.Button("调色与像素风格", EditorStyles.toolbarButton)) RetroScreenWindow.Open();
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("保存与分享", EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(profile == null || !EditorUtility.IsPersistent(profile)))
                {
                    if (GUILayout.Button(new GUIContent("导出 JSON", "导出完整白天与夜晚配置，发送此文件即可分享。"), EditorStyles.toolbarButton)) ExportPreset();
                    if (GUILayout.Button(new GUIContent("导入 JSON", "读取分享文件，替换当前房间的白天与夜晚配置；支持一次撤销。"), EditorStyles.toolbarButton)) ImportPreset();
                }
            }
            if (!string.IsNullOrEmpty(presetMessage)) EditorGUILayout.HelpBox(presetMessage, presetMessageType);
            EditorGUILayout.LabelField("房间灯光 · " + (EditorApplication.isPlaying ? EditorApplication.isPaused ? "播放已暂停" : "播放中" : "编辑模式"), EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("可停靠在 Scene 或 Game 旁直接调灯，播放和暂停时都可编辑。修改写入配置资产，支持撤销/重做，退出播放前自动保存。", MessageType.Info);
            DrawTargetAndPeriod();
            if (profile == null)
            {
                if (RoomLightingAuthoring.GetLoadedControllers().Length > 0)
                { EditorGUILayout.HelpBox("请先指定需要配置的房间，避免把修改写入其他房间。", MessageType.Warning); return; }
                EditorGUILayout.HelpBox("请打开 MainRoomCombined，再安装房间灯光。", MessageType.Warning);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("在当前主场景安装")) { RoomLightingSetup.InstallInOpenScene(); Resolve(); }
                return;
            }
            using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField("实际配置资产", profile, typeof(RoomLightingProfile), false);
            if (RoomLightingAuthoring.GetLoadedControllers().Count(room => room.Profile == profile) > 1)
                EditorGUILayout.HelpBox("多个已加载房间共用这份配置，修改会同步到这些房间。需要独立效果时请为目标房间绑定独立配置资产。", MessageType.Info);
            if (!EditorUtility.IsPersistent(profile))
            { EditorGUILayout.HelpBox("当前是临时配置，请先绑定 Project 中的房间灯光配置资产。", MessageType.Warning); return; }
            using (var scrolling = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = scrolling.scrollPosition;
                DrawSceneLights();
                EditorGUILayout.Space();
                showBaseSettings = EditorGUILayout.Foldout(showBaseSettings, editingPeriod == RoomLightingPeriod.Day
                    ? "白天基础光、共用地板与地毯" : "夜间环境与过渡", true);
                if (showBaseSettings)
                {
                    var settings = new SerializedObject(profile); settings.Update(); RoomLightingProfileEditor.DrawSettings(settings, editingPeriod);
                    if (settings.ApplyModifiedProperties()) RoomLightingAuthoring.ProfileEdited(profile);
                    if (controller != null)
                    {
                        bool next = EditorGUILayout.Toggle("临时关闭照明分层", controller.PreviewUnbanded);
                        if (next != controller.PreviewUnbanded)
                        {
                            if (!previewStates.ContainsKey(controller)) previewStates.Add(controller, controller.PreviewUnbanded);
                            controller.PreviewUnbanded = next; controller.ApplyFromProfile(); SceneView.RepaintAll();
                        }
                    }
                }
            }
        }

        private void ExportPreset()
        {
            var source = profile;
            if (source == null) return;
            string path = EditorUtility.SaveFilePanel("导出白天与夜晚灯光配置", presetFolder, source.name + "-lighting.json", "json");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                RoomLightingPresetIO.SaveFile(source, path);
                presetFolder = Path.GetDirectoryName(path) ?? "";
                presetMessage = "已导出完整白天与夜晚灯光配置，可分享此 JSON 文件：\n" + path;
                presetMessageType = MessageType.Info;
            }
            catch (Exception exception)
            { presetMessage = "导出失败：" + exception.Message; presetMessageType = MessageType.Error; }
        }

        private void ImportPreset()
        {
            var destination = profile;
            if (destination == null || !EditorUtility.IsPersistent(destination)) return;
            string path = EditorUtility.OpenFilePanel("导入白天与夜晚灯光配置", presetFolder, "json");
            if (string.IsNullOrEmpty(path)) return;
            EndSceneHandleInteraction();
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("导入昼夜灯光配置");
            Undo.RecordObject(destination, "导入昼夜灯光配置");
            try
            {
                if (!RoomLightingPresetIO.TryLoadFile(path, destination, out string message))
                { presetMessage = message; presetMessageType = MessageType.Error; return; }
                // The authoring gateway marks dirty, queues saving and refreshes every room sharing this profile.
                RoomLightingAuthoring.ProfileEdited(destination);
                presetFolder = Path.GetDirectoryName(path) ?? "";
                presetMessage = message + " 可撤销；点击“保存配置”写入工程。";
                presetMessageType = MessageType.Info;
            }
            catch (Exception exception)
            { presetMessage = "导入失败：" + exception.Message; presetMessageType = MessageType.Error; }
            finally
            { Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); Undo.IncrementCurrentGroup(); Repaint(); }
        }

        private void DrawTargetAndPeriod()
        {
            EditorGUI.BeginChangeCheck();
            bool follow = EditorGUILayout.Toggle("跟随所选物件的房间", followSelection);
            var target = targetRoom;
            if (!follow) target = (RoomLightingController)EditorGUILayout.ObjectField("目标房间", targetRoom, typeof(RoomLightingController), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (!follow && target == null) target = controller;
                followSelection = follow; targetRoom = target; Resolve();
            }
            var rooms = RoomLightingAuthoring.GetLoadedControllers();
            if (controller != null)
                EditorGUILayout.LabelField("当前目标", controller.gameObject.scene.name + " / " + controller.name);
            else if (rooms.Length > 1)
                EditorGUILayout.HelpBox("已加载多个房间。选中目标房间内的灯或物件，或取消跟随后显式指定目标房间。", MessageType.Warning);
            int period = GUILayout.Toolbar((int)editingPeriod, new[] { "编辑白天", "编辑夜晚" });
            if (period != (int)editingPeriod)
            {
                EndSceneHandleInteraction(); editingPeriod = (RoomLightingPeriod)period;
                RoomLightingAuthoring.SetEditingPeriod(controller, editingPeriod);
                if (controller != null) controller.SetPreviewMode(editingPeriod == RoomLightingPeriod.Day
                    ? RoomLightingPreviewMode.Day : RoomLightingPreviewMode.Night);
                SetToolsHidden(false);
            }
            if (controller == null) return;
            var mode = (RoomLightingPreviewMode)EditorGUILayout.Popup("画面预览", (int)controller.PreviewMode,
                new[] { "跟随游戏流程", "白天", "夜晚" });
            if (mode != controller.PreviewMode) { controller.SetPreviewMode(mode); EnableScenePreview(); }
            EditorGUILayout.LabelField("当前画面时段", controller.EffectivePeriod == RoomLightingPeriod.Day ? "白天" : "夜晚");
            if (controller.EffectivePeriod != editingPeriod)
                EditorGUILayout.HelpBox("当前画面与编辑时段不同。切换画面预览可立即检查这一时段的修改。", MessageType.Info);
        }

        private void DrawSceneLights()
        {
            EditorGUILayout.LabelField(editingPeriod == RoomLightingPeriod.Day ? "白天场景补光灯" : "夜间场景灯与床引导灯", EditorStyles.boldLabel);
            if (controller == null)
            { EditorGUILayout.HelpBox("当前场景没有房间灯光控制器。打开主场景后会自动连接。", MessageType.Info); return; }
            var lights = RoomLightingAuthoring.GetEditableLights(controller, editingPeriod);
            var selected = RoomLightingAuthoring.ResolveLight(controller, selectedPath, editingPeriod);
            var names = new[] { "选择要调整的灯" }.Concat(lights.Select(light => controller.GetHierarchyPath(light))).ToArray();
            int current = selected == null ? 0 : Array.IndexOf(lights, selected) + 1;
            int next = EditorGUILayout.Popup("灯光", current, names);
            if (next != current)
            {
                EndSceneHandleInteraction();
                selected = next > 0 ? lights[next - 1] : null;
                selectedPath = selected != null ? controller.GetHierarchyPath(selected) : "";
                SetToolsHidden(false);
                if (selected != null) RoomLightingAuthoring.SelectLight(controller, selected, editingPeriod);
            }
            if (lights.Length == 0) { EditorGUILayout.HelpBox("此时段没有可编辑的场景灯。白天主吊灯等托管灯使用基础配置。", MessageType.Info); return; }
            if (selected == null) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("选中场景物件")) RoomLightingAuthoring.SelectLight(controller, selected, editingPeriod);
                if (GUILayout.Button("Scene 定位"))
                {
                    RoomLightingAuthoring.SelectLight(controller, selected, editingPeriod);
                    if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Frame(new Bounds(selected.transform.position, Vector3.one * 2), false);
                }
                sceneHandles = GUILayout.Toggle(sceneHandles, "场景手柄", "Button");
            }
            if (!sceneHandles) { EndSceneHandleInteraction(); SetToolsHidden(false); }
            var value = RoomLightingAuthoring.ReadLight(controller, selected, editingPeriod);
            EditorGUI.BeginChangeCheck();
            value.dayEnabled = EditorGUILayout.Toggle(editingPeriod == RoomLightingPeriod.Day ? "白天启用" : "夜间启用", value.dayEnabled);
            value.localPosition = EditorGUILayout.Vector3Field("局部位置（米）", value.localPosition);
            value.localEuler = EditorGUILayout.Vector3Field("局部旋转（度）", value.localEuler);
            int type = EditorGUILayout.Popup("灯光类型", Mathf.Max(0, Array.IndexOf(LightTypes, value.type)), TypeLabels);
            value.type = LightTypes[type];
            value.color = EditorGUILayout.ColorField(new GUIContent("灯光颜色"), value.color, true, false, true);
            value.intensity = EditorGUILayout.FloatField("灯光强度", value.intensity);
            using (new EditorGUI.DisabledScope(value.type == LightType.Directional)) value.range = EditorGUILayout.FloatField("照射范围（米）", value.range);
            if (value.type == LightType.Spot)
            {
                value.spotAngle = EditorGUILayout.Slider("外圈角度", value.spotAngle, 1, 179);
                value.innerSpotAngle = EditorGUILayout.Slider("内圈角度", value.innerSpotAngle, 0, value.spotAngle);
            }
            int shadows = EditorGUILayout.Popup("阴影类型", Mathf.Max(0, Array.IndexOf(Shadows, value.shadows)), ShadowLabels);
            value.shadows = Shadows[shadows];
            if (value.shadows != LightShadows.None)
            {
                value.shadowStrength = EditorGUILayout.Slider("阴影强度", value.shadowStrength, 0, 1);
                value.shadowBias = EditorGUILayout.FloatField("阴影深度偏移", value.shadowBias);
                value.shadowNormalBias = EditorGUILayout.FloatField("阴影法线偏移", value.shadowNormalBias);
            }
            if (EditorGUI.EndChangeCheck()) RoomLightingAuthoring.WriteLight(controller, selected, value, editingPeriod);
            EditorGUILayout.LabelField("Scene 手柄", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Toggle(Tools.current == Tool.Move, "移动", "Button")) Tools.current = Tool.Move;
                if (GUILayout.Toggle(Tools.current == Tool.Rotate, "旋转", "Button")) Tools.current = Tool.Rotate;
                if (GUILayout.Toggle(Tools.current == Tool.Scale, "范围", "Button")) Tools.current = Tool.Scale;
            }
            EditorGUILayout.HelpBox("Scene 中选中这盏灯后可拖动手柄；原生 Inspector／Gizmo 的人工修改也会同步到同一配置。未编辑的灯保持原场景值。", MessageType.None);
        }

        private void DuringSceneGUI(SceneView view)
        {
            Resolve();
            var light = RoomLightingAuthoring.ResolveLight(controller, selectedPath, editingPeriod);
            bool owns = sceneHandles && light != null && controller.EffectivePeriod == editingPeriod
                && Selection.activeGameObject == light.gameObject;
            SetToolsHidden(owns);
            if (!owns) { EndSceneHandleInteraction(); return; }
            var value = RoomLightingAuthoring.ReadLight(controller, light, editingPeriod);
            EventType rawType = Event.current.rawType;
            Color previous = Handles.color; Handles.color = value.color;
            try
            {
                Handles.Label(light.transform.position, light.name + (editingPeriod == RoomLightingPeriod.Day ? " · 白天强度 " : " · 夜间强度 ") + value.intensity.ToString("0.##"));
                EditorGUI.BeginChangeCheck();
                if (Tools.current == Tool.Rotate)
                {
                    var world = Handles.RotationHandle(light.transform.rotation, light.transform.position);
                    value.localEuler = (light.transform.parent != null ? Quaternion.Inverse(light.transform.parent.rotation) * world : world).eulerAngles;
                }
                else if (Tools.current == Tool.Scale && value.type != LightType.Directional)
                    value.range = Handles.RadiusHandle(light.transform.rotation, light.transform.position, value.range);
                else
                {
                    var rotation = Tools.pivotRotation == PivotRotation.Local ? light.transform.rotation : Quaternion.identity;
                    var world = Handles.PositionHandle(light.transform.position, rotation);
                    value.localPosition = light.transform.parent != null ? light.transform.parent.InverseTransformPoint(world) : world;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    BeginSceneHandleInteraction();
                    RoomLightingAuthoring.WriteLight(controller, light, value, editingPeriod); Repaint();
                }
            }
            finally { Handles.color = previous; CompleteSceneHandleEvent(rawType); }
        }
        public void BeginSceneHandleInteraction()
        {
            if (handleUndoGroup >= 0) return;
            Undo.IncrementCurrentGroup(); handleUndoGroup = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("拖动场景灯光");
        }
        public void EndSceneHandleInteraction()
        {
            if (handleUndoGroup < 0) return;
            int group = handleUndoGroup; handleUndoGroup = -1;
            Undo.CollapseUndoOperations(group); Undo.IncrementCurrentGroup();
        }
        public void CompleteSceneHandleEvent(EventType rawType)
        { if (rawType == EventType.MouseUp) EndSceneHandleInteraction(); }
        private void SetToolsHidden(bool owns)
        {
            if (owns && !toolsOwned) { previousToolsHidden = Tools.hidden; Tools.hidden = true; toolsOwned = true; }
            else if (!owns && toolsOwned) { Tools.hidden = previousToolsHidden; toolsOwned = false; }
        }
    }
}

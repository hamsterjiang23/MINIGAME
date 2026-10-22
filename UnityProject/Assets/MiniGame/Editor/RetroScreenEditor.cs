using System.Collections.Generic;
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
            EditorGUILayout.LabelField("胶片颗粒", EditorStyles.boldLabel);
            Draw("filmGrainStrength", "颗粒强度", "0 关闭；建议 0.03～0.06。单色颗粒随帧变化，暗部稍强；独立于量化抖动，仅作用于场景画面，Overlay UI 保持清晰。");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("像素化", EditorStyles.boldLabel);
            Draw("useVirtualResolution", "按虚拟分辨率像素化", "宽度根据画面比例计算；不会把小窗口放大成更多虚拟像素。");
            if (serializedObject.FindProperty("useVirtualResolution").boolValue)
            {
                Draw("virtualHeight", "虚拟画面高度", "16:9 时：540 对应 960×540；360 对应 640×360。");
            }
            else Draw("pixelSize", "像素尺寸", "单位为输出像素。1 不额外像素化；越大，颗粒越粗。");
            if (serializedObject.ApplyModifiedProperties()) RetroScreenPreview.ProfileEdited((RetroScreenProfile)target);
            EditorGUILayout.HelpBox("Scene 与 Game 视图实时预览，支持撤销/重做。播放期间编辑的是配置资产，保存后退出仍保留。Scene 请开启场景灯光与 Effects。", MessageType.Info);
            var profile = (RetroScreenProfile)target;
            bool original = RetroScreenPreview.IsOriginal(profile);
            bool next = GUILayout.Toggle(original, original ? "正在查看原图 · 点击返回效果" : "临时查看原图", "Button");
            if (next != original) RetroScreenPreview.SetOriginal(profile, next);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("自然画面预设"))
                {
                    Undo.RecordObject(profile, "恢复自然画面"); profile.ResetToNaturalDefaults();
                    RetroScreenPreview.ProfileEdited(profile);
                }
                if (GUILayout.Button("参考调色起点"))
                {
                    Undo.RecordObject(profile, "应用参考调色起点"); profile.ResetToReferenceStyle();
                    RetroScreenPreview.ProfileEdited(profile);
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("恢复轻度默认"))
                {
                    Undo.RecordObject(profile, "恢复复古效果默认值");
                    profile.ResetToLightDefaults();
                    RetroScreenPreview.ProfileEdited(profile);
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
        [SerializeField] private RoomLightingController targetRoom;
        [SerializeField] private bool followSelection = true;
        [SerializeField] private RoomLightingPeriod editingPeriod;
        private RoomLightingController room;
        private string error;
        private readonly Dictionary<RoomLightingController, RoomLightingPreviewMode> previousPreviews = new Dictionary<RoomLightingController, RoomLightingPreviewMode>();

        [MenuItem("MiniGame/画面/复古效果")]
        [MenuItem("MiniGame/画面/调色与像素风格")]
        public static void Open() => GetWindow<RetroScreenWindow>("调色与像素风格").Show();

        public static void OpenForRoom(RoomLightingController controller, RoomLightingPeriod period)
        {
            var window = GetWindow<RetroScreenWindow>("调色与像素风格");
            window.targetRoom = controller; window.followSelection = controller == null;
            window.editingPeriod = period;
            RoomLightingAuthoring.SetEditingPeriod(controller, period);
            window.Show(); window.Repaint();
        }

        private void OnEnable()
        {
            minSize = new Vector2(440, 420);
            Undo.undoRedoPerformed += Repaint;
            Selection.selectionChanged += Repaint;
        }
        private void OnInspectorUpdate() => Repaint();

        private void Preview(RoomLightingPreviewMode mode)
        {
            if (room == null) return;
            if (!previousPreviews.ContainsKey(room)) previousPreviews.Add(room, room.PreviewMode);
            room.SetPreviewMode(mode); RoomLightingWindow.EnableScenePreview();
            RetroScreenPreview.Refresh();
        }

        private void OnGUI()
        {
            if (!RoomLightingAuthoring.IsSavingScenePreview)
                room = followSelection ? RoomLightingAuthoring.FindController() : targetRoom;
            if (room != null && (!room.gameObject.scene.IsValid() || !room.gameObject.scene.isLoaded)) room = null;
            RetroScreenPreview.TargetRoom = room;
            editingPeriod = RoomLightingAuthoring.GetEditingPeriod(room, editingPeriod);
            var scene = room != null ? room.gameObject.scene : UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var resolved = room != null ? room.GetProfile(editingPeriod)?.ScreenStyle : RetroScreenSetup.FindProfile(scene);
            if (resolved == null && room == null && RoomLightingAuthoring.GetLoadedControllers().Length == 0)
                resolved = AssetDatabase.LoadAssetAtPath<RetroScreenProfile>(RetroScreenSetup.ProfilePath);
            if (profile != resolved)
            {
                RetroScreenPreview.Clear();
                profile = resolved;
            }
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                if (GUILayout.Button("房间灯光", EditorStyles.toolbarButton)) RoomLightingWindow.Open();
            EditorGUILayout.LabelField("房间 · 昼夜调色与像素风格", EditorStyles.boldLabel);
            followSelection = EditorGUILayout.Toggle("跟随所选物件的房间", followSelection);
            if (!followSelection) targetRoom = (RoomLightingController)EditorGUILayout.ObjectField("目标房间", targetRoom != null ? targetRoom : room, typeof(RoomLightingController), true);
            if (room != null)
            {
                EditorGUILayout.LabelField("当前目标", room.gameObject.scene.name + " / " + room.name);
                DrawBinding(RoomLightingPeriod.Day, "白天调色配置");
                DrawBinding(RoomLightingPeriod.Night, "夜晚调色配置");
                int period = GUILayout.Toolbar((int)editingPeriod, new[] { "编辑白天", "编辑夜晚" });
                if (period != (int)editingPeriod)
                {
                    editingPeriod = (RoomLightingPeriod)period;
                    RoomLightingAuthoring.SetEditingPeriod(room, editingPeriod);
                    Preview(editingPeriod == RoomLightingPeriod.Day ? RoomLightingPreviewMode.Day : RoomLightingPreviewMode.Night);
                    GUIUtility.ExitGUI();
                }
                var mode = (RoomLightingPreviewMode)EditorGUILayout.Popup("画面预览", (int)room.PreviewMode,
                    new[] { "跟随游戏流程", "白天", "夜晚" });
                if (mode != room.PreviewMode) Preview(mode);
                EditorGUILayout.LabelField("当前画面时段", room.EffectivePeriod == RoomLightingPeriod.Day ? "白天" : "夜晚");
                if (room.EffectivePeriod != editingPeriod)
                    EditorGUILayout.HelpBox("当前画面与编辑时段不同。切换画面预览可检查当前时段的修改。", MessageType.Info);
                if (!room.HasIndependentProfiles || room.Profile.ScreenStyle == null || room.NightProfile.ScreenStyle == null
                    || room.Profile.ScreenStyle == room.NightProfile.ScreenStyle)
                {
                    EditorGUILayout.HelpBox("昼夜使用两份独立调色资产。初始化会保留当前调色，创建夜晚副本。", MessageType.Info);
                    using (new EditorGUI.DisabledScope(!room.HasIndependentProfiles))
                        if (GUILayout.Button("建立昼夜独立调色配置"))
                        { RetroScreenSetup.EnsureRoomProfiles(room); error = null; }
                    if (!room.HasIndependentProfiles) EditorGUILayout.HelpBox("请先在房间灯光工具中绑定独立的白天与夜晚灯光配置。", MessageType.Warning);
                    if (room.HasIndependentProfiles && room.Profile.ScreenStyle != null && room.Profile.ScreenStyle == room.NightProfile.ScreenStyle)
                        return;
                }
            }
            else if (RoomLightingAuthoring.GetLoadedControllers().Length > 0)
                EditorGUILayout.HelpBox("请选择目标房间内的物件，或取消跟随后指定房间。", MessageType.Warning);
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Warning);
            EditorGUILayout.ObjectField(editingPeriod == RoomLightingPeriod.Day ? "正在编辑白天" : "正在编辑夜晚", profile, typeof(RetroScreenProfile), false);
            if (profile == null)
            {
                EditorGUILayout.HelpBox("当前时段尚未绑定调色配置。", MessageType.Warning);
                if (room == null && RoomLightingAuthoring.GetLoadedControllers().Length == 0 && GUILayout.Button("创建默认配置"))
                    profile = RetroScreenSetup.EnsureProfile();
                return;
            }
            if (room != null)
            {
                EditorGUILayout.HelpBox("曝光、色彩、颗粒、色阶和像素尺寸只写入当前时段。两套资产独立保存，预览时与房间灯光同步。", MessageType.Info);
                var other = room.GetProfile(editingPeriod == RoomLightingPeriod.Day ? RoomLightingPeriod.Night : RoomLightingPeriod.Day)?.ScreenStyle;
                using (new EditorGUI.DisabledScope(other == null || other == profile))
                    if (GUILayout.Button(editingPeriod == RoomLightingPeriod.Day ? "复制夜晚参数到白天" : "复制白天参数到夜晚"))
                    {
                        Undo.RecordObject(profile, "复制另一时段调色");
                        string name = profile.name; EditorUtility.CopySerialized(other, profile); profile.name = name;
                        RetroScreenPreview.ProfileEdited(profile);
                    }
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
            Undo.undoRedoPerformed -= Repaint;
            Selection.selectionChanged -= Repaint;
            RetroScreenPreview.Clear();
            if (profileEditor != null) DestroyImmediate(profileEditor);
            RetroScreenPreview.TargetRoom = null;
            foreach (var pair in previousPreviews) if (pair.Key != null) pair.Key.SetPreviewMode(pair.Value);
            previousPreviews.Clear();
        }

        private void DrawBinding(RoomLightingPeriod period, string label)
        {
            var owner = room.GetProfile(period);
            using (new EditorGUI.DisabledScope(owner == null || !EditorUtility.IsPersistent(owner)))
            {
                var current = owner != null ? owner.ScreenStyle : null;
                var next = (RetroScreenProfile)EditorGUILayout.ObjectField(label, current, typeof(RetroScreenProfile), false);
                if (next == current) return;
                try { RetroScreenSetup.SetPeriodProfile(room, period, next); error = null; }
                catch (System.ArgumentException exception) { error = exception.Message; }
            }
        }
    }

    [InitializeOnLoad]
    internal static class RetroScreenPreview
    {
        public static RoomLightingController TargetRoom { get; set; }
        private static RetroScreenProfile originalProfile;
        private static readonly HashSet<RetroScreenProfile> EditedProfiles = new HashSet<RetroScreenProfile>();
        static RetroScreenPreview()
        {
            Undo.undoRedoPerformed += Refresh;
            AssemblyReloadEvents.beforeAssemblyReload += SaveAndClear;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.ExitingEditMode) SaveEditedProfiles();
                Clear();
            };
            EditorApplication.quitting += SaveEditedProfiles;
            Camera.onPreCull += SynchronizeSceneCamera;
        }
        public static void ProfileEdited(RetroScreenProfile profile)
        {
            if (profile != null)
            {
                EditorUtility.SetDirty(profile);
                if (EditorUtility.IsPersistent(profile)) EditedProfiles.Add(profile);
            }
            Refresh();
        }
        private static void SaveEditedProfiles()
        {
            foreach (var profile in EditedProfiles.ToArray())
            {
                if (profile == null) EditedProfiles.Remove(profile);
                else if (EditorUtility.IsPersistent(profile)) AssetDatabase.SaveAssetIfDirty(profile);
            }
        }
        private static void SaveAndClear() { SaveEditedProfiles(); Clear(); }
        public static bool IsOriginal(RetroScreenProfile profile) => originalProfile == profile;
        public static void SetOriginal(RetroScreenProfile profile, bool original)
        {
            Clear();
            originalProfile = original ? profile : null;
            foreach (var effect in Object.FindObjectsOfType<RetroScreenEffect>())
                effect.PreviewOriginal = original && effect.ActiveProfile == profile;
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
        private static void SynchronizeSceneCamera(Camera camera)
        {
            if (camera == null) return;
            var room = camera.cameraType == CameraType.SceneView
                ? TargetRoom != null ? TargetRoom : RoomLightingAuthoring.FindController() : null;
            foreach (var effect in camera.GetComponents<RetroScreenEffect>())
            {
                if (camera.cameraType == CameraType.SceneView) effect.BindLighting(room);
                if (camera.cameraType == CameraType.SceneView || originalProfile != null)
                    effect.PreviewOriginal = originalProfile != null && effect.ActiveProfile == originalProfile;
            }
        }
    }

    public static class RetroScreenSetup
    {
        public const string ProfilePath = "Assets/MiniGame/Resources/RetroScreenDefault.asset";
        public const string ShaderPath = "Assets/MiniGame/Resources/RetroScreen.shader";

        public static void SetPeriodProfile(RoomLightingController room, RoomLightingPeriod period, RetroScreenProfile value)
        {
            var owner = room != null ? room.GetProfile(period) : null;
            if (owner == null || !EditorUtility.IsPersistent(owner)) throw new System.ArgumentException("请先绑定当前时段的 Project 灯光配置资产。");
            if (value != null && !EditorUtility.IsPersistent(value)) throw new System.ArgumentException("调色必须使用 Project 配置资产。");
            var other = room.GetProfile(period == RoomLightingPeriod.Day ? RoomLightingPeriod.Night : RoomLightingPeriod.Day);
            if (value != null && other != null && value == other.ScreenStyle)
                throw new System.ArgumentException("白天和夜晚必须使用不同的调色配置。可先建立独立副本。");
            Undo.RecordObject(owner, "绑定时段调色配置");
            owner.ScreenStyle = value; RoomLightingAuthoring.ProfileEdited(owner);
            RetroScreenPreview.Refresh();
        }

        public static void EnsureRoomProfiles(RoomLightingController room)
        {
            if (room == null || !room.HasIndependentProfiles) throw new System.ArgumentException("请先建立独立的昼夜灯光配置。");
            if (!EditorUtility.IsPersistent(room.Profile) || !EditorUtility.IsPersistent(room.NightProfile))
                throw new System.ArgumentException("昼夜灯光必须使用 Project 配置资产。");
            var day = room.Profile.ScreenStyle;
            if (day == null || !EditorUtility.IsPersistent(day))
            {
                day = FindProfile(room.gameObject.scene);
                if (day == null || !EditorUtility.IsPersistent(day) || day == room.NightProfile.ScreenStyle)
                {
                    day = UnityEngine.Object.Instantiate(day != null ? day : EnsureProfile());
                    day.name = room.Profile.name + "Retro";
                    AssetDatabase.CreateAsset(day, AssetDatabase.GenerateUniqueAssetPath(
                        System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(room.Profile)) + "/" + day.name + ".asset"));
                    Undo.RegisterCreatedObjectUndo(day, "创建白天调色配置");
                }
                SetPeriodProfile(room, RoomLightingPeriod.Day, day);
            }
            if (room.NightProfile.ScreenStyle == null || room.NightProfile.ScreenStyle == day)
            {
                var night = UnityEngine.Object.Instantiate(day); night.name = room.NightProfile.name + "Retro";
                AssetDatabase.CreateAsset(night, AssetDatabase.GenerateUniqueAssetPath(
                    System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(room.NightProfile)) + "/" + night.name + ".asset"));
                Undo.RegisterCreatedObjectUndo(night, "创建夜晚调色配置");
                SetPeriodProfile(room, RoomLightingPeriod.Night, night);
                AssetDatabase.SaveAssetIfDirty(night);
            }
            RoomLightingAuthoring.SaveProfile(room.Profile); RoomLightingAuthoring.SaveProfile(room.NightProfile);
        }

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
            effect.FollowRoomLighting = true;
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Game-view authoring of the same profile used by the Scene lighting tools.</summary>
    [DefaultExecutionOrder(-1100), DisallowMultipleComponent]
    public sealed class RoomLightingTuningPanel : MonoBehaviour
    {
        private static readonly Color Paper = new Color(.91f, .88f, .78f);
        private static readonly Color Amber = new Color(.90f, .64f, .29f);
        private static readonly Color Quiet = new Color(.58f, .59f, .55f);
        private static readonly Color Surface = new Color(.12f, .14f, .13f);
        private readonly List<Action> refreshers = new List<Action>();
        private RoomLightingController controller;
        private GameRuntime runtime;
        private TerminalSignalController signal;
        private Font font;
        private RectTransform modal, panel, content;
        private ScrollRect scroll;
        private Text status, selection, editingAsset;
        private Button dayButton, nightButton, automaticButton, previewDayButton, previewNightButton;
        private Button saveButton, exportButton, importButton;
        private Material overlayMaterial;
        private bool refreshing, previousPaused, previousCursor;
        private CursorLockMode previousLock;
        private int blockedFrame = -1, rowIndex;
        private string selectedPath = "";
        private RoomLightingProfile rowsProfile;
        private Light[] rowsLights = Array.Empty<Light>();
        private readonly HashSet<RoomLightingProfile> loadedPlayerProfiles = new HashSet<RoomLightingProfile>();

        [Serializable] private sealed class SavedSettings
        {
            public int Version = 2;
            public string Period;
            public string ProfileName;
            public string ProfileJson;
        }

        // These events carry authoring notifications only. No gameplay state is serialized here.
#if UNITY_EDITOR
        public static event Action<RoomLightingProfile> ProfileEditedInEditor;
#endif
        public RoomLightingController Controller => controller;
        public event Action VisibilityChanged;
        public RoomLightingProfile Profile => controller != null ? controller.GetProfile(SelectedPeriod) : null;
        public bool IsPanelOpen { get; private set; }
        public bool BlocksInput => IsPanelOpen || blockedFrame == Time.frameCount;
        public RoomLightingPeriod SelectedPeriod { get; private set; }
        public string SelectedLightPath => selectedPath;
        public static string SettingsPath => Path.Combine(Application.persistentDataPath, "room-lighting-settings.json");
        public static string SharedPresetPath => Path.Combine(Application.persistentDataPath, "room-lighting-preset.json");
        public static string NightSettingsPath => Path.Combine(Application.persistentDataPath, "room-lighting-night-settings.json");
        public static string NightSharedPresetPath => Path.Combine(Application.persistentDataPath, "room-lighting-night-preset.json");
        public string CurrentSettingsPath => SettingsPathFor(SelectedPeriod);
        public string CurrentSharedPresetPath => SelectedPeriod == RoomLightingPeriod.Day ? SharedPresetPath : NightSharedPresetPath;
        private string PeriodLabel => SelectedPeriod == RoomLightingPeriod.Day ? "白天" : "夜晚";
        private static string MissingProfileMessage(RoomLightingPeriod period) => period == RoomLightingPeriod.Night
            ? "夜晚需要独立配置资产，请在 Scene 灯光工具中指定。" : "白天需要配置资产，请在 Scene 灯光工具中指定。";

        private static string SettingsPathFor(RoomLightingPeriod period) => period == RoomLightingPeriod.Day ? SettingsPath : NightSettingsPath;

        public void Initialize(RectTransform parent, Font uiFont)
        {
            if (modal != null) return;
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            font = uiFont != null ? uiFont : throw new ArgumentNullException(nameof(uiFont));
            Build(parent);
        }

        public void Bind(GameRuntime game, RoomLightingController lighting, TerminalSignalController signalController = null,
            Action<string> preparationMarker = null)
        {
            bool reuseRows = controller == lighting && SelectedPeriod == RoomLightingPeriod.Day
                && rowsProfile == (lighting != null ? lighting.GetProfile(RoomLightingPeriod.Day) : null)
                && content != null && content.childCount > 0;
            if (reuseRows && lighting != null)
            {
                var lights = lighting.GetEditableSceneLights(RoomLightingPeriod.Day);
                reuseRows = rowsLights.SequenceEqual(lights) && (lights.Length == 0
                    ? string.IsNullOrEmpty(selectedPath)
                    : lights.Any(light => lighting.GetHierarchyPath(light) == selectedPath));
            }
            SetPanelOpen(false);
            if (signal != null) signal.Opening -= CloseForSignal;
            runtime = game; controller = lighting; signal = signalController;
            if (signal != null) signal.Opening += CloseForSignal;
            SelectedPeriod = RoomLightingPeriod.Day;
            if (!reuseRows) selectedPath = "";
            preparationMarker?.Invoke("install.lighting_panel_binding");
            LoadPlayerSettings();
            preparationMarker?.Invoke("install.lighting_settings");
            // Bind closes the tool. Its next open rebuilds and refreshes from the current profile;
            // updating every hidden input here adds work to the title-to-gameplay handoff.
            if (modal != null && !reuseRows) RebuildRows();
            preparationMarker?.Invoke(reuseRows ? "install.lighting_rows_reused" : "install.lighting_rows_rebuild");
        }

        public bool SetPanelOpen(bool open)
        {
            if (IsPanelOpen == open) return IsPanelOpen;
            if (open && Profile == null) { SetStatus(MissingProfileMessage(SelectedPeriod), true); return false; }
            if (open && (modal == null || runtime == null
                || MiniGameSessionHost.Current != null && MiniGameSessionHost.Current.IsTransitioning)) return false;
            blockedFrame = Time.frameCount;
            if (open)
            {
                signal?.SetPanelOpen(false);
                previousLock = Cursor.lockState; previousCursor = Cursor.visible;
                previousPaused = runtime.GetView().Paused;
                IsPanelOpen = true;
                runtime.SetPaused(true);
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
                modal.gameObject.SetActive(true);
                modal.SetAsLastSibling();
                RebuildRows(); LayoutPanel();
                SetStatus("F7 / Esc 关闭。编辑" + PeriodLabel + "独立配置；保存仅保留当前时段。", false);
            }
            else
            {
                IsPanelOpen = false;
                controller?.SetPreviewMode(RoomLightingPreviewMode.Automatic);
                if (modal != null) modal.gameObject.SetActive(false);
                runtime?.SetPaused(previousPaused);
                Cursor.lockState = previousLock; Cursor.visible = previousCursor;
            }
            EventSystem.current?.SetSelectedGameObject(null);
            VisibilityChanged?.Invoke();
            return IsPanelOpen;
        }

        private void CloseForSignal() => SetPanelOpen(false);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7)) SetPanelOpen(!IsPanelOpen);
            else if (IsPanelOpen && Input.GetKeyDown(KeyCode.Escape)) SetPanelOpen(false);
            if (IsPanelOpen) Refresh();
        }

        private void LateUpdate()
        {
            if (!IsPanelOpen) return;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        public void SelectPeriod(RoomLightingPeriod period)
        {
            if (period != RoomLightingPeriod.Day && period != RoomLightingPeriod.Night) return;
            if (controller == null || controller.GetProfile(period) == null)
            { SetStatus(MissingProfileMessage(period), true); return; }
            SelectedPeriod = period;
            selectedPath = "";
            RebuildRows();
            SetStatus("正在编辑" + PeriodLabel + "配置；预览时段由上方预览按钮独立控制。", false);
        }

        public bool SelectLight(string path)
        {
            if (Profile == null || controller.ResolveSceneLight(path, SelectedPeriod) == null) return false;
            selectedPath = path; RebuildRows(); return true;
        }

        public void SetPreview(RoomLightingPreviewMode mode)
        {
            if (mode == RoomLightingPreviewMode.Night && (controller == null || controller.GetProfile(RoomLightingPeriod.Night) == null))
            { SetStatus(MissingProfileMessage(RoomLightingPeriod.Night), true); return; }
            controller?.SetPreviewMode(mode);
            Refresh();
        }

        public void Refresh()
        {
            if (dayButton == null) return;
            if (rowsProfile != Profile) { RebuildRows(); return; }
            bool available = Profile != null;
            saveButton.interactable = exportButton.interactable = importButton.interactable = available;
            if (!available)
            {
                editingAsset.text = MissingProfileMessage(SelectedPeriod);
                SetStatus(MissingProfileMessage(SelectedPeriod), true);
                return;
            }
            LayoutPanel();
            refreshing = true;
            try
            {
                Highlight(dayButton, SelectedPeriod == RoomLightingPeriod.Day);
                Highlight(nightButton, SelectedPeriod == RoomLightingPeriod.Night);
                Highlight(automaticButton, controller.PreviewMode == RoomLightingPreviewMode.Automatic);
                Highlight(previewDayButton, controller.PreviewMode == RoomLightingPreviewMode.Day);
                Highlight(previewNightButton, controller.PreviewMode == RoomLightingPreviewMode.Night);
                editingAsset.text = "正在编辑：" + PeriodLabel + " · " + Profile.name;
#if UNITY_EDITOR
                editingAsset.text += "\n" + UnityEditor.AssetDatabase.GetAssetPath(Profile);
#else
                editingAsset.text += "\n分享文件：" + CurrentSharedPresetPath;
#endif
                saveButton.GetComponentInChildren<Text>().text = "保存" + PeriodLabel + "配置";
                exportButton.GetComponentInChildren<Text>().text = "导出" + PeriodLabel + " JSON";
                importButton.GetComponentInChildren<Text>().text = "导入" + PeriodLabel + " JSON";
                foreach (var refresh in refreshers) refresh();
            }
            finally { refreshing = false; }
        }

        public bool SaveSettings()
        {
            if (Profile == null) return false;
            try
            {
                Profile.NotifyChanged(); controller.ApplyFromProfile();
#if UNITY_EDITOR
                if (!UnityEditor.AssetDatabase.Contains(Profile))
                    throw new InvalidOperationException("当前灯光配置不是工程资产，请先在 Scene 工具中指定配置资产。");
                UnityEditor.EditorUtility.SetDirty(Profile);
                ProfileEditedInEditor?.Invoke(Profile);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(Profile);
                SetStatus("已保存" + PeriodLabel + "配置：" + UnityEditor.AssetDatabase.GetAssetPath(Profile) + "\n退出播放后仍保留。", false);
#else
                Directory.CreateDirectory(Application.persistentDataPath);
                var saved = new SavedSettings { Period = SelectedPeriod.ToString(), ProfileName = Profile.name, ProfileJson = JsonUtility.ToJson(Profile) };
                string path = CurrentSettingsPath;
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(saved, true));
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
                SetStatus("已保存" + PeriodLabel + "独立配置：" + path, false);
#endif
                return true;
            }
            catch (Exception exception)
            {
                SetStatus("保存失败：" + exception.Message, true);
                Debug.LogWarning("房间灯光配置保存失败：" + exception.Message, this);
                return false;
            }
        }

        public bool ExportPreset(string path)
        {
            if (Profile == null || string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                RoomLightingPresetIO.SaveFile(Profile, path);
                SetStatus("已导出" + PeriodLabel + "完整配置，可将此 JSON 发给他人：\n" + Path.GetFullPath(path), false);
                return true;
            }
            catch (Exception exception)
            {
                SetStatus("导出失败：" + exception.Message, true);
                Debug.LogWarning("房间灯光配置导出失败：" + exception.Message, this);
                return false;
            }
        }

        public bool ImportPreset(string path)
        {
            if (Profile == null || string.IsNullOrWhiteSpace(path)) return false;
            try
            {
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(Profile, "导入房间灯光配置");
#endif
                if (!RoomLightingPresetIO.TryLoadFile(path, Profile, out string message))
                { SetStatus("导入失败：" + message, true); return false; }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Profile);
                ProfileEditedInEditor?.Invoke(Profile);
#endif
                controller.ApplyFromProfile();
                // Keep the editing period and the selected light when its scene path still exists.
                RebuildRows();
                SetStatus("已导入" + PeriodLabel + "配置并实时应用；点击保存保留。\n" + Path.GetFullPath(path), false);
                return true;
            }
            catch (Exception exception)
            {
                SetStatus("导入失败：" + exception.Message, true);
                Debug.LogWarning("房间灯光配置导入失败：" + exception.Message, this);
                return false;
            }
        }

        private void ChooseExportPreset()
        {
#if UNITY_EDITOR
            string filename = SelectedPeriod == RoomLightingPeriod.Day ? "room-lighting-day-preset" : "room-lighting-night-preset";
            string path = UnityEditor.EditorUtility.SaveFilePanel("导出" + PeriodLabel + "灯光配置", "", filename, "json");
#else
            string path = CurrentSharedPresetPath;
#endif
            if (!string.IsNullOrWhiteSpace(path)) ExportPreset(path);
        }

        private void ChooseImportPreset()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("导入" + PeriodLabel + "灯光配置", "", "json");
#else
            string path = CurrentSharedPresetPath;
#endif
            if (!string.IsNullOrWhiteSpace(path)) ImportPreset(path);
        }

        private void LoadPlayerSettings()
        {
#if !UNITY_EDITOR
            if (controller == null) return;
            LoadPlayerSettings(RoomLightingPeriod.Day);
            // Legacy single-profile controllers must never receive two files into the same object.
            if (controller.HasIndependentProfiles) LoadPlayerSettings(RoomLightingPeriod.Night);
#endif
        }

#if !UNITY_EDITOR
        private void LoadPlayerSettings(RoomLightingPeriod period)
        {
            var target = controller.GetProfile(period);
            if (target == null || !loadedPlayerProfiles.Add(target)) return;
            string path = SettingsPathFor(period);
            if (!File.Exists(path)) return;
            RoomLightingProfile validated = null;
            try
            {
                var saved = JsonUtility.FromJson<SavedSettings>(File.ReadAllText(path));
                if (saved == null || saved.Version < 1 || saved.Version > 2 || string.IsNullOrEmpty(saved.ProfileJson))
                    throw new InvalidDataException("灯光配置版本或内容无效。");
                if (saved.Version == 1 && period != RoomLightingPeriod.Day
                    || saved.Version == 2 && saved.Period != period.ToString())
                    throw new InvalidDataException("灯光配置时段与保存位置不一致。");
                if (saved.ProfileName != target.name) return;
                validated = ScriptableObject.CreateInstance<RoomLightingProfile>();
                JsonUtility.FromJsonOverwrite(saved.ProfileJson, validated);
                validated.NotifyChanged();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(validated), target);
                target.NotifyChanged(); controller.ApplyFromProfile();
            }
            catch (Exception exception) { Debug.LogWarning("自定义房间灯光未载入：" + exception.Message, this); }
            finally { if (validated != null) Destroy(validated); }
        }
#endif

        private void BeginEdit()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(Profile, "调整房间" + (SelectedPeriod == RoomLightingPeriod.Day ? "白天" : "夜晚") + "灯光");
#endif
        }

        private void Changed()
        {
            Profile.NotifyChanged();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Profile);
            ProfileEditedInEditor?.Invoke(Profile);
#endif
            controller.ApplyFromProfile();
            SetStatus("已实时应用" + PeriodLabel + "配置；Scene 和 Game 共用此资产。点击保存保留。", false);
            Refresh();
        }

        private void RebuildRows()
        {
            if (content == null) return;
            refreshers.Clear(); rowIndex = 0;
            bool wasRefreshing = refreshing;
            refreshing = true;
            foreach (Transform child in content)
            { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            refreshing = wasRefreshing;
            rowsProfile = Profile;
            rowsLights = Array.Empty<Light>();
            if (Profile == null)
            {
                AddHeading(MissingProfileMessage(SelectedPeriod));
            }
            else
            {
                BuildDayRows();
                BuildLightRows();
            }
            content.sizeDelta = new Vector2(-14, rowIndex * 60 + 10);
            scroll.StopMovement(); scroll.verticalNormalizedPosition = 1;
            Refresh();
        }

        private void BuildDayRows()
        {
            AddHeading(PeriodLabel + " · 环境与整体控制");
            AddColor("环境光", () => Profile.ambientColor, value => Profile.ambientColor = value);
            AddNumber("环境光强度", 0, 4, () => Profile.ambientIntensity, value => Profile.ambientIntensity = value);
            AddNumber("反射强度（非环境光亮度）", 0, 1, () => Profile.reflectionIntensity, value => Profile.reflectionIntensity = value);
            AddNumber("灯光整体倍率", 0, 4, () => Profile.lightMultiplier, value => Profile.lightMultiplier = value);
            AddNumber("自发光整体倍率", 0, 4, () => Profile.emissionMultiplier, value => Profile.emissionMultiplier = value);
            AddNumber("时段渐变秒数", .01f, 10, () => Profile.fadeSeconds, value => Profile.fadeSeconds = value);
            AddHeading(PeriodLabel + " · 吊灯（位置相对灯罩开口）");
            AddColor("吊灯颜色", () => Profile.lampColor, value => Profile.lampColor = value);
            AddNumber("吊灯强度", 0, 8, () => Profile.lampIntensity, value => Profile.lampIntensity = value);
            AddNumber("吊灯范围", 1, 12, () => Profile.lampRange, value => Profile.lampRange = value);
            AddVector("吊灯偏移", -20, 20, () => Profile.lampOffset, value => Profile.lampOffset = value);
            AddVector("吊灯旋转", -180, 180, () => Profile.lampEulerAngles, value => Profile.lampEulerAngles = value);
            AddNumber("聚光外角", 20, 170, () => Profile.outerAngle, value => Profile.outerAngle = value);
            AddNumber("聚光内角", 0, 165, () => Profile.innerAngle, value => Profile.innerAngle = value);
            AddNumber("吊灯阴影强度", 0, 1, () => Profile.shadowStrength, value => Profile.shadowStrength = value);
            AddNumber("吊灯阴影偏差", 0, .1f, () => Profile.shadowBias, value => Profile.shadowBias = value);
            AddNumber("吊灯法线偏差", 0, 1, () => Profile.shadowNormalBias, value => Profile.shadowNormalBias = value);
            AddNumber("灯泡自发光", 0, 4, () => Profile.bulbEmission, value => Profile.bulbEmission = value);
            AddHeading(PeriodLabel + " · 窗边与显示器");
            AddColor("窗光颜色", () => Profile.windowColor, value => Profile.windowColor = value);
            AddNumber("窗光强度", 0, 2, () => Profile.windowIntensity, value => Profile.windowIntensity = value);
            AddNumber("窗光范围", 1, 8, () => Profile.windowRange, value => Profile.windowRange = value);
            AddNumber("显示器光强度", 0, 1, () => Profile.monitorIntensity, value => Profile.monitorIntensity = value);
            AddHeading(PeriodLabel + " · 照明分层");
            AddAction("启用照明分层", () => Profile.lightingBandsEnabled ? "已启用" : "已关闭", () =>
            { BeginEdit(); Profile.lightingBandsEnabled = !Profile.lightingBandsEnabled; Changed(); });
            AddNumber("照明分层数", 2, 32, () => Profile.lightingBands, value => Profile.lightingBands = Mathf.RoundToInt(value));
            AddNumber("照明分层强度", 0, 1, () => Profile.lightingBandsStrength, value => Profile.lightingBandsStrength = value);
            AddNumber("照明抖动", 0, 1, () => Profile.lightingDither, value => Profile.lightingDither = value);
            AddHeading(PeriodLabel + " · 地板与地毯");
            AddNumber("地板亮度", .25f, 3, () => Profile.floorGain, value => Profile.floorGain = value);
            AddNumber("地板对比度", .25f, 2, () => Profile.floorContrast, value => Profile.floorContrast = value);
            AddNumber("地板光滑度", 0, 1, () => Profile.floorSmoothness, value => Profile.floorSmoothness = value);
            AddAction("显示地毯", () => Profile.rugVisible ? "已显示" : "已隐藏", () =>
            { BeginEdit(); Profile.rugVisible = !Profile.rugVisible; Changed(); });
            AddVector("地毯偏移", -20, 20, () => Profile.rugOffset, value => Profile.rugOffset = value);
            AddNumber("地毯宽度", .2f, 8, () => Profile.rugSize.x, value => Profile.rugSize = new Vector2(value, Profile.rugSize.y));
            AddNumber("地毯长度", .2f, 6, () => Profile.rugSize.y, value => Profile.rugSize = new Vector2(Profile.rugSize.x, value));
            AddNumber("地毯旋转", -180, 180, () => Profile.rugYaw, value => Profile.rugYaw = value);
            AddColor("地毯颜色", () => Profile.rugTint, value => Profile.rugTint = value);
        }

        private void BuildLightRows()
        {
            AddHeading(PeriodLabel + " · 场景灯光（含吊灯、窗光、电脑、床灯）");
            var lights = controller.GetEditableSceneLights(SelectedPeriod);
            rowsLights = lights;
            if (lights.Length == 0) { AddHeading("此时段没有可编辑的场景灯。"); return; }
            if (lights.All(light => controller.GetHierarchyPath(light) != selectedPath))
                selectedPath = controller.GetHierarchyPath(lights[0]);
            var row = NewRow("LightSelection");
            var previous = MakeButton(row, "PreviousLight", "上一盏", () => CycleLight(-1));
            Position(previous.GetComponent<RectTransform>(), 0, 0, 0, 0, .22f, 1);
            var next = MakeButton(row, "NextLight", "下一盏", () => CycleLight(1));
            Position(next.GetComponent<RectTransform>(), .78f, 0, 0, 0, 1, 1);
            selection = MakeText(row, "SelectedLight", "", 14, Paper);
            Position(selection.rectTransform, .24f, 0, 0, 0, .76f, 1);
            selection.alignment = TextAnchor.MiddleCenter;
            refreshers.Add(() =>
            {
                var light = controller.ResolveSceneLight(selectedPath, SelectedPeriod);
                selection.text = light != null ? light.name + "\n" + (Array.IndexOf(lights, light) + 1) + " / " + lights.Length : "灯光已移除";
            });
            AddAction("灯光启用", () => ReadSelected().dayEnabled ? "已启用" : "已关闭",
                () => EditSelected(settings => settings.dayEnabled = !settings.dayEnabled));
            AddAction("灯光类型", () => ReadSelected().type.ToString(), () => EditSelected(settings =>
                settings.type = settings.type == LightType.Point ? LightType.Spot : settings.type == LightType.Spot ? LightType.Directional : LightType.Point));
            AddColor("灯光颜色", () => ReadSelected().color, value => WriteSelected(settings => settings.color = value));
            AddNumber("灯光强度", 0, 100, () => ReadSelected().intensity, value => WriteSelected(settings => settings.intensity = value));
            AddNumber("灯光范围", .01f, 100, () => ReadSelected().range, value => WriteSelected(settings => settings.range = value),
                () => ReadSelected().type != LightType.Directional);
            AddVector("局部位置", -30, 30, () => ReadSelected().localPosition, value => WriteSelected(settings => settings.localPosition = value));
            AddVector("局部旋转", -180, 180, () => ReadSelected().localEuler, value => WriteSelected(settings => settings.localEuler = value));
            AddNumber("聚光外角", 1, 179, () => ReadSelected().spotAngle, value => WriteSelected(settings => settings.spotAngle = value),
                () => ReadSelected().type == LightType.Spot);
            AddNumber("聚光内角", 0, 179, () => ReadSelected().innerSpotAngle, value => WriteSelected(settings => settings.innerSpotAngle = value),
                () => ReadSelected().type == LightType.Spot);
            AddAction("阴影类型", () => ReadSelected().shadows.ToString(), () => EditSelected(settings =>
                settings.shadows = settings.shadows == LightShadows.None ? LightShadows.Hard : settings.shadows == LightShadows.Hard ? LightShadows.Soft : LightShadows.None));
            AddNumber("阴影强度", 0, 1, () => ReadSelected().shadowStrength, value => WriteSelected(settings => settings.shadowStrength = value),
                () => ReadSelected().shadows != LightShadows.None);
            AddNumber("阴影偏差", 0, 2, () => ReadSelected().shadowBias, value => WriteSelected(settings => settings.shadowBias = value),
                () => ReadSelected().shadows != LightShadows.None);
            AddNumber("阴影法线偏差", 0, 3, () => ReadSelected().shadowNormalBias, value => WriteSelected(settings => settings.shadowNormalBias = value),
                () => ReadSelected().shadows != LightShadows.None);
            AddAction("清除当前灯光覆盖", () => "恢复当前时段默认值", () =>
            {
                BeginEdit();
                controller.GetSceneLightSettingsList(SelectedPeriod).RemoveAll(item => item != null && item.HierarchyPath == selectedPath);
                Changed();
            });
        }

        private SceneLightSettings ReadSelected()
        {
            var light = controller.ResolveSceneLight(selectedPath, SelectedPeriod);
            return light != null ? controller.CaptureSceneLight(light, SelectedPeriod) : new SceneLightSettings();
        }

        private void WriteSelected(Action<SceneLightSettings> write)
        {
            var light = controller.ResolveSceneLight(selectedPath, SelectedPeriod);
            if (light == null) return;
            var list = controller.GetSceneLightSettingsList(SelectedPeriod);
            var settings = list.FirstOrDefault(item => item != null && item.HierarchyPath == selectedPath);
            if (settings == null) { settings = controller.CaptureSceneLight(light, SelectedPeriod); list.Add(settings); }
            write(settings);
        }

        private void EditSelected(Action<SceneLightSettings> write)
        { BeginEdit(); WriteSelected(write); Changed(); }

        private void CycleLight(int delta)
        {
            var lights = controller.GetEditableSceneLights(SelectedPeriod);
            if (lights.Length == 0) return;
            int index = Array.FindIndex(lights, light => controller.GetHierarchyPath(light) == selectedPath);
            selectedPath = controller.GetHierarchyPath(lights[(index + delta + lights.Length) % lights.Length]);
            // Existing controls read the selected profile value directly; retain the scroll position.
            Refresh();
        }

        private void AddColor(string label, Func<Color> read, Action<Color> write)
        {
            AddNumber(label + " R", 0, 4, () => read().r, value => { var color = read(); color.r = value; write(color); });
            AddNumber(label + " G", 0, 4, () => read().g, value => { var color = read(); color.g = value; write(color); });
            AddNumber(label + " B", 0, 4, () => read().b, value => { var color = read(); color.b = value; write(color); });
        }

        private void AddVector(string label, float min, float max, Func<Vector3> read, Action<Vector3> write)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                int component = axis;
                AddNumber(label + " " + "XYZ"[axis], min, max, () => read()[component], value =>
                { var vector = read(); vector[component] = value; write(vector); });
            }
        }

        private void AddNumber(string label, float min, float max, Func<float> read, Action<float> write, Func<bool> applicable = null)
        {
            var row = NewRow(label);
            var text = MakeText(row, "Label", label, 15, Paper);
            Position(text.rectTransform, 0, .45f, 0, 0, .68f, 1);
            var inputRect = NewRect(row, "Value");
            Position(inputRect, .7f, .48f, 0, 0, 1, 1);
            var background = AddImage(inputRect, Surface);
            var valueText = MakeText(inputRect, "Text", "", 15, Amber);
            Stretch(valueText.rectTransform); valueText.rectTransform.offsetMin = new Vector2(6, 2);
            valueText.rectTransform.offsetMax = new Vector2(-6, -2); valueText.alignment = TextAnchor.MiddleRight;
            var input = inputRect.gameObject.AddComponent<InputField>();
            input.targetGraphic = background; input.textComponent = valueText;
            input.contentType = InputField.ContentType.DecimalNumber;
            input.navigation = new Navigation { mode = Navigation.Mode.None };
            string presentedText = "";
            var sliderRect = NewRect(row, "Slider");
            Position(sliderRect, 0, .07f, 0, 0, 1, .36f);
            AddImage(sliderRect, Surface);
            var handle = NewRect(sliderRect, "Handle");
            handle.anchorMin = handle.anchorMax = new Vector2(.5f, .5f);
            handle.sizeDelta = new Vector2(13, 20);
            var slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.handleRect = handle; slider.targetGraphic = AddImage(handle, Amber);
            slider.minValue = min; slider.maxValue = max;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            Action<float> commit = value =>
            {
                if (refreshing || Profile == null || applicable != null && !applicable() || float.IsNaN(value) || float.IsInfinity(value)) return;
                BeginEdit(); write(value); Changed();
            };
            slider.onValueChanged.AddListener(value => commit(value));
            input.onEndEdit.AddListener(value =>
            {
                // Merely leaving a focused field must not overwrite a newer Scene-tool edit.
                if (value == presentedText) { Refresh(); return; }
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)) commit(parsed);
                else Refresh();
            });
            refreshers.Add(() =>
            {
                bool available = applicable == null || applicable();
                input.interactable = slider.interactable = available;
                text.color = available ? Paper : Quiet;
                float value = read();
                slider.SetValueWithoutNotify(value);
                if (!input.isFocused)
                {
                    presentedText = value.ToString("0.###", CultureInfo.InvariantCulture);
                    input.SetTextWithoutNotify(presentedText);
                }
            });
        }

        private void AddAction(string label, Func<string> read, Action action)
        {
            var row = NewRow(label);
            var text = MakeText(row, "Label", label, 15, Paper);
            Position(text.rectTransform, 0, .1f, 0, 0, .47f, .9f);
            var button = MakeButton(row, "Change", "", () => { if (Profile != null) action(); });
            Position(button.GetComponent<RectTransform>(), .5f, .1f, 0, 0, 1, .9f);
            refreshers.Add(() => button.GetComponentInChildren<Text>().text = read());
        }

        private void AddHeading(string text)
        {
            var row = NewRow("Group");
            var label = MakeText(row, "Label", text, 17, Amber);
            Stretch(label.rectTransform); label.alignment = TextAnchor.MiddleLeft;
        }

        private RectTransform NewRow(string name)
        {
            var row = NewRect(content, name);
            row.anchorMin = new Vector2(0, 1); row.anchorMax = Vector2.one; row.pivot = new Vector2(.5f, 1);
            row.offsetMin = new Vector2(0, -rowIndex * 60 - 54);
            row.offsetMax = new Vector2(0, -rowIndex * 60);
            rowIndex++; return row;
        }

        private void Build(RectTransform parent)
        {
            modal = NewRect(parent, "RoomLightingTuningModal"); Stretch(modal);
            var canvas = modal.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            var parentCanvas = parent.GetComponentInParent<Canvas>();
            canvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 10) + 120;
            modal.gameObject.AddComponent<GraphicRaycaster>();
            AddImage(modal, Color.clear);
            panel = NewRect(modal, "LightingControls");
            panel.anchorMin = panel.anchorMax = new Vector2(1, .5f); panel.pivot = new Vector2(1, .5f);
            AddImage(panel, new Color(.045f, .057f, .052f, .99f));
            var title = MakeText(panel, "Title", "房间灯光 · 白天 / 夜晚", 23, Paper);
            Top(title.rectTransform, 20, 14, 400, 38);
            var close = MakeButton(panel, "Close", "关闭 ×", () => SetPanelOpen(false));
            close.GetComponent<RectTransform>().anchorMin = close.GetComponent<RectTransform>().anchorMax = Vector2.one;
            close.GetComponent<RectTransform>().pivot = Vector2.one;
            close.GetComponent<RectTransform>().anchoredPosition = new Vector2(-18, -18);
            close.GetComponent<RectTransform>().sizeDelta = new Vector2(72, 32);
            dayButton = MakeButton(panel, "DayPeriod", "编辑白天", () => SelectPeriod(RoomLightingPeriod.Day));
            nightButton = MakeButton(panel, "NightPeriod", "编辑夜晚", () => SelectPeriod(RoomLightingPeriod.Night));
            Top(dayButton.GetComponent<RectTransform>(), 20, 63, 230, 35);
            Top(nightButton.GetComponent<RectTransform>(), 260, 63, 230, 35);
            automaticButton = MakeButton(panel, "AutomaticPreview", "预览：随流程", () => SetPreview(RoomLightingPreviewMode.Automatic));
            previewDayButton = MakeButton(panel, "DayPreview", "预览白天", () => SetPreview(RoomLightingPreviewMode.Day));
            previewNightButton = MakeButton(panel, "NightPreview", "预览夜晚", () => SetPreview(RoomLightingPreviewMode.Night));
            Top(automaticButton.GetComponent<RectTransform>(), 20, 110, 150, 34);
            Top(previewDayButton.GetComponent<RectTransform>(), 180, 110, 150, 34);
            Top(previewNightButton.GetComponent<RectTransform>(), 340, 110, 150, 34);
            editingAsset = MakeText(panel, "EditingAsset", "", 12, Amber);
            Top(editingAsset.rectTransform, 20, 152, 470, 46);
            editingAsset.alignment = TextAnchor.MiddleLeft;
            var viewport = NewRect(panel, "Viewport"); Stretch(viewport);
            viewport.offsetMin = new Vector2(20, 196); viewport.offsetMax = new Vector2(-20, -208);
            AddImage(viewport, Color.clear); viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport;
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 42;
            content = NewRect(viewport, "Parameters");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero; scroll.content = content;
            CreateScrollbar(viewport);
            status = MakeText(panel, "Status", "F7 打开灯光配置", 13, Quiet);
            status.rectTransform.anchorMin = Vector2.zero; status.rectTransform.anchorMax = new Vector2(1, 0);
            status.rectTransform.offsetMin = new Vector2(20, 96); status.rectTransform.offsetMax = new Vector2(-20, 188);
            saveButton = MakeButton(panel, "Save", "保存白天配置", () => SaveSettings());
            saveButton.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            saveButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            saveButton.GetComponent<RectTransform>().offsetMin = new Vector2(20, 54);
            saveButton.GetComponent<RectTransform>().offsetMax = new Vector2(-20, 88);
            exportButton = MakeButton(panel, "ExportPreset", "导出白天 JSON", ChooseExportPreset);
            exportButton.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            exportButton.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, 0);
            exportButton.GetComponent<RectTransform>().offsetMin = new Vector2(20, 14);
            exportButton.GetComponent<RectTransform>().offsetMax = new Vector2(-5, 48);
            importButton = MakeButton(panel, "ImportPreset", "导入白天 JSON", ChooseImportPreset);
            importButton.GetComponent<RectTransform>().anchorMin = new Vector2(.5f, 0);
            importButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
            importButton.GetComponent<RectTransform>().offsetMin = new Vector2(5, 14);
            importButton.GetComponent<RectTransform>().offsetMax = new Vector2(-20, 48);
            overlayMaterial = new Material(Graphic.defaultGraphicMaterial)
            { name = "Room lighting tuning overlay", hideFlags = HideFlags.HideAndDontSave, renderQueue = 4100 };
            ApplyOverlayMaterials(); LayoutPanel(); modal.gameObject.SetActive(false);
        }

        private void CreateScrollbar(RectTransform viewport)
        {
            var bar = NewRect(viewport, "Scrollbar");
            bar.anchorMin = new Vector2(1, 0); bar.anchorMax = Vector2.one;
            bar.offsetMin = new Vector2(-7, 0); bar.offsetMax = Vector2.zero;
            AddImage(bar, Surface);
            var handle = NewRect(bar, "Handle"); Stretch(handle);
            var scrollbar = bar.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop; scrollbar.handleRect = handle;
            scrollbar.targetGraphic = AddImage(handle, Quiet);
            scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }

        private void ApplyOverlayMaterials()
        {
            if (modal == null || overlayMaterial == null) return;
            foreach (var graphic in modal.GetComponentsInChildren<Graphic>(true)) graphic.material = overlayMaterial;
        }

        private void LayoutPanel()
        {
            if (modal == null || panel == null) return;
            float width = modal.rect.width > 0 ? modal.rect.width : 1920;
            float height = modal.rect.height > 0 ? modal.rect.height : 1080;
            float scale = Mathf.Min(1, Mathf.Max(1, width - 32) / 530, Mathf.Max(1, height - 32) / 480);
            panel.localScale = Vector3.one * scale;
            panel.anchoredPosition = new Vector2(-16, 0);
            panel.sizeDelta = new Vector2(510, Mathf.Min(930, Mathf.Max(480, (height - 32) / scale)));
        }

        private void SetStatus(string text, bool error)
        { if (status != null) { status.text = text; status.color = error ? new Color(.92f, .43f, .34f) : Quiet; } }

        private void OnDisable() => SetPanelOpen(false);
        private void OnDestroy()
        {
            SetPanelOpen(false);
            if (signal != null) signal.Opening -= CloseForSignal;
            if (modal != null) Destroy(modal.gameObject);
            if (overlayMaterial != null) Destroy(overlayMaterial);
        }

        private Button MakeButton(Transform parent, string name, string label, Action action)
        {
            var rect = NewRect(parent, name); var image = AddImage(rect, Surface);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var text = MakeText(rect, "Label", label, 15, Paper);
            Stretch(text.rectTransform); text.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(() => action()); return button;
        }

        private Text MakeText(Transform parent, string name, string value, int size, Color color)
        {
            var rect = NewRect(parent, name); var text = rect.gameObject.AddComponent<Text>();
            text.font = font; text.fontSize = size; text.color = color; text.text = value;
            text.raycastTarget = false; text.verticalOverflow = VerticalWrapMode.Truncate;
            if (overlayMaterial != null) text.material = overlayMaterial;
            return text;
        }

        private static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false); return (RectTransform)go.transform;
        }
        private Image AddImage(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.color = color;
            if (overlayMaterial != null) image.material = overlayMaterial;
            return image;
        }
        private static void Highlight(Button button, bool active)
        { button.GetComponent<Image>().color = active ? Amber : Surface; button.GetComponentInChildren<Text>().color = active ? Surface : Paper; }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void Top(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
        private static void Position(RectTransform rect, float left, float bottom, float offsetX, float offsetY, float right, float top)
        {
            rect.anchorMin = new Vector2(left, bottom); rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = new Vector2(offsetX, offsetY); rect.offsetMax = Vector2.zero;
        }
    }
}

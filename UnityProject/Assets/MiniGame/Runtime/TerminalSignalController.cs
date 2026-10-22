using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Visual settings and preview input only; never changes the game session.</summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class TerminalSignalController : MonoBehaviour
    {
        [Serializable]
        private sealed class SavedSettings
        {
            public int Version = 1;
            public TerminalSignalParameters Logo;
            public TerminalSignalParameters Monitor;
        }

        private readonly List<TerminalSignalEffect> effects = new List<TerminalSignalEffect>();
        private Func<bool> paused;
        private int blockedFrame = -1;
        private CursorLockMode previousLock;
        private bool previousCursor;
        public TerminalSignalProfile Profile { get; private set; }
        public TerminalSignalTuningPanel Panel { get; private set; }
        public bool PanelOpen { get; private set; }
        public bool BlocksGameplayInput => PanelOpen || blockedFrame == Time.frameCount;
        public bool FreezePreview { get; set; }
        public event Action Opening;
        public event Action LogoGlitch;
        private int lastLogoBurst = -1;
        public float Clock { get; private set; }
        public static string SettingsPath => Path.Combine(Application.persistentDataPath, "terminal-signal-settings.json");

        public void Initialize(RectTransform parent, Font font, Func<bool> isPaused, bool createTuningPanel = true)
        {
            paused = isPaused;
            var defaults = Resources.Load<TerminalSignalProfile>("MiniGame/TerminalSignalDefault");
            Profile = defaults != null ? Instantiate(defaults) : ScriptableObject.CreateInstance<TerminalSignalProfile>();
            Profile.name = "标志与监控扰动（运行时）";
            Profile.hideFlags = HideFlags.DontSave;
            LoadSettings();
            Profile.Sanitize();
            if (createTuningPanel)
            {
                Panel = TerminalSignalTuningPanel.Create(parent, font, this);
                Panel.SetVisible(false);
            }
        }

        public TerminalSignalEffect Attach(RawImage image, bool logo = false)
        {
            var effect = image.gameObject.AddComponent<TerminalSignalEffect>();
            effect.Initialize(this, logo);
            effects.Add(effect);
            return effect;
        }

        private void Update()
        {
            if (Profile == null) return;
            if (Input.GetKeyDown(KeyCode.F6)) SetPanelOpen(!PanelOpen);
            else if (PanelOpen && Input.GetKeyDown(KeyCode.Escape)) SetPanelOpen(false);
        }

        private void LateUpdate()
        {
            if (Profile == null) return;
            bool advancing = !FreezePreview && !(paused?.Invoke() ?? false);
            if (advancing) Clock += Time.unscaledDeltaTime;
            ApplySettings();
            if (advancing)
            {
                int burst = Profile.Logo.LogoBurstIndex(Clock);
                if (burst >= 0 && burst != lastLogoBurst)
                {
                    lastLogoBurst = burst;
                    foreach (var effect in effects)
                        if (effect != null && effect.IsVisibleLogo) { LogoGlitch?.Invoke(); break; }
                }
            }
            if (PanelOpen) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }

        public void ApplySettings()
        {
            if (Profile == null) return;
            Profile.Sanitize();
            foreach (var effect in effects) if (effect != null) effect.Apply();
        }

        public void SetPanelOpen(bool open)
        {
            if (Panel == null || PanelOpen == open) return;
            if (open) Opening?.Invoke();
            blockedFrame = Time.frameCount;
            if (open)
            {
                previousLock = Cursor.lockState; previousCursor = Cursor.visible;
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            }
            else { Cursor.lockState = previousLock; Cursor.visible = previousCursor; }
            PanelOpen = open;
            EventSystem.current?.SetSelectedGameObject(null);
            Panel.SetVisible(open);
        }

        public void ResetGroup(bool logo)
        {
            var defaults = Resources.Load<TerminalSignalProfile>("MiniGame/TerminalSignalDefault");
            var value = defaults != null ? (logo ? defaults.Logo : defaults.Monitor) :
                logo ? TerminalSignalParameters.LogoDefaults() : new TerminalSignalParameters();
            var copy = JsonUtility.FromJson<TerminalSignalParameters>(JsonUtility.ToJson(value));
            if (logo) Profile.Logo = copy; else Profile.Monitor = copy;
            ApplySettings();
            Panel?.Refresh();
        }

        public bool SaveSettings()
        {
            try
            {
                ApplySettings();
                Directory.CreateDirectory(Application.persistentDataPath);
                var data = new SavedSettings { Logo = Profile.Logo, Monitor = Profile.Monitor };
                string temporary = SettingsPath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (File.Exists(SettingsPath)) File.Replace(temporary, SettingsPath, null);
                else File.Move(temporary, SettingsPath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("扰动配置保存失败：" + exception.Message, this);
                return false;
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath)) return;
            try
            {
                var saved = JsonUtility.FromJson<SavedSettings>(File.ReadAllText(SettingsPath));
                if (saved == null || saved.Version != 1 || saved.Logo == null || saved.Monitor == null)
                    throw new InvalidDataException("配置版本或内容无效");
                Profile.Logo = saved.Logo; Profile.Monitor = saved.Monitor;
            }
            catch (Exception exception) { Debug.LogWarning("扰动配置未载入，使用项目默认值：" + exception.Message, this); }
        }

        private void OnDisable() { if (PanelOpen) SetPanelOpen(false); }
        private void OnDestroy() { if (Profile != null) Destroy(Profile); }
    }
}

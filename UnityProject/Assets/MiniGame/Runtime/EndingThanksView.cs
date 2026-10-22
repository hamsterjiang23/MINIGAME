using System;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    public enum EndingThanksPhase { FadingOut, Visible, Completed }

    [Serializable]
    public sealed class EndingThanksCheckpoint
    {
        public EndingThanksPhase Phase;
        public float Elapsed;
        public bool IsValid() => Enum.IsDefined(typeof(EndingThanksPhase), Phase)
            && !float.IsNaN(Elapsed) && !float.IsInfinity(Elapsed) && Elapsed >= 0
            && (Phase != EndingThanksPhase.FadingOut || Elapsed <= EndingThanksView.FadeSeconds);
    }

    /// <summary>The shared final page owns its fade and requires a fresh click after becoming visible.</summary>
    public sealed class EndingThanksView : MonoBehaviour
    {
        public const float FadeSeconds = 1.2f;
        private EndingThanksCheckpoint state = new EndingThanksCheckpoint();
        private Image black;
        private Text title, prompt;
        private Font font;
        private Action completed;
        private bool inputReleased;
        private GameRuntime pauseSource;
        private bool runtimePaused;
        public bool UseExternalClock { get; set; }
        public bool IsComplete => state.Phase == EndingThanksPhase.Completed;
        public bool IsPageVisible => state.Phase == EndingThanksPhase.Visible;
        public float Opacity => black != null ? black.color.a : 0;
        public event Action Presented;

        public static EndingThanksView Show(Action completed, EndingThanksCheckpoint checkpoint = null,
            GameRuntime pauseSource = null)
        {
            var root = new GameObject("Ending thanks", typeof(EndingThanksView));
            var view = root.GetComponent<EndingThanksView>();
            view.completed = completed;
            view.Build();
            view.RestoreCheckpoint(checkpoint);
            view.BindPauseSource(pauseSource ?? MiniGameSessionHost.Current?.Runtime);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return view;
        }

        public EndingThanksCheckpoint CaptureCheckpoint() => new EndingThanksCheckpoint
            { Phase = state.Phase, Elapsed = state.Elapsed };

        public void RestoreCheckpoint(EndingThanksCheckpoint checkpoint)
        {
            state = new EndingThanksCheckpoint
            {
                Phase = checkpoint != null && Enum.IsDefined(typeof(EndingThanksPhase), checkpoint.Phase)
                    ? checkpoint.Phase : EndingThanksPhase.FadingOut,
                Elapsed = checkpoint != null && Finite(checkpoint.Elapsed) ? Mathf.Max(0, checkpoint.Elapsed) : 0
            };
            if (state.Phase == EndingThanksPhase.FadingOut) state.Elapsed = Mathf.Min(FadeSeconds, state.Elapsed);
            inputReleased = false;
            Refresh();
        }

        private void Update()
        {
            if (UseExternalClock) return;
            BindPauseSource(MiniGameSessionHost.Current?.Runtime);
            var room = FindObjectOfType<MiniGameBootstrap>(true);
            var ending = room != null && !room.enabled ? FindObjectOfType<DreamEndingController>(true) : null;
            ending?.RefreshPausePresentation();
            if (!MainMenuController.BlocksGameplayInput && Input.GetKeyDown(KeyCode.Escape))
            {
                if (room != null && !room.enabled)
                {
                    // The hidden room suspended the ordinary bootstrap; its existing pause menu remains reusable.
                    if (ending != null) ending.SetPaused(!ending.Paused);
                }
            }
            AdvanceForTests(Time.unscaledDeltaTime, Input.GetMouseButton(0), Input.GetMouseButtonDown(0),
                MainMenuController.BlocksGameplayInput || (MiniGameSessionHost.Current?.Runtime?.GetView().Paused ?? false));
        }

        public void AdvanceForTests(float dt, bool pointerHeld, bool pointerDown, bool paused = false)
        {
            if (paused)
            {
                inputReleased = false;
                return;
            }
            if (IsComplete) return;
            dt = Finite(dt) ? Mathf.Max(0, dt) : 0;
            state.Elapsed += dt;
            if (state.Phase == EndingThanksPhase.FadingOut)
            {
                if (state.Elapsed >= FadeSeconds)
                {
                    state.Phase = EndingThanksPhase.Visible;
                    state.Elapsed = 0;
                    inputReleased = false;
                    Presented?.Invoke();
                }
                Refresh();
                return; // The input that finished the fade cannot dismiss its resulting page.
            }
            if (!inputReleased)
            {
                if (!pointerHeld && !pointerDown) inputReleased = true;
                Refresh();
                return;
            }
            if (pointerDown)
            {
                state.Phase = EndingThanksPhase.Completed;
                completed?.Invoke();
                return;
            }
            Refresh();
        }

        private void BindPauseSource(GameRuntime source)
        {
            if (ReferenceEquals(pauseSource, source)) return;
            if (pauseSource != null) pauseSource.Changed -= OnRuntimeChanged;
            pauseSource = source;
            runtimePaused = source?.GetView().Paused ?? false;
            inputReleased = false;
            if (pauseSource != null) pauseSource.Changed += OnRuntimeChanged;
        }

        private void OnRuntimeChanged(RuntimeView view)
        {
            // A resume button can run before this component's Update in the same frame.
            if (runtimePaused != view.Paused) inputReleased = false;
            runtimePaused = view.Paused;
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            var panel = new GameObject("Ending black", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            black = panel.GetComponent<Image>();
            black.raycastTarget = true;
            font = GameUiTypography.CreateFont();
            title = Label("感谢游玩", new Vector2(.5f, .55f), 64);
            prompt = Label("单击返回主菜单", new Vector2(.5f, .33f), 26);
        }

        private Text Label(string text, Vector2 anchor, int size)
        {
            var root = new GameObject(text, typeof(RectTransform), typeof(Text));
            root.transform.SetParent(transform, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(1500, 140);
            var label = root.GetComponent<Text>();
            label.font = font; label.text = text; label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(.84f, .88f, .82f);
            label.raycastTarget = false;
            return label;
        }

        private void Refresh()
        {
            if (black == null) return;
            black.color = new Color(0, 0, 0, state.Phase == EndingThanksPhase.FadingOut
                ? Mathf.Clamp01(state.Elapsed / FadeSeconds) : 1);
            bool visible = state.Phase != EndingThanksPhase.FadingOut;
            title.gameObject.SetActive(visible);
            prompt.gameObject.SetActive(visible);
            var color = prompt.color;
            color.a = .18f + .72f * (.5f + .5f * Mathf.Sin(state.Elapsed * Mathf.PI * .8f));
            prompt.color = color;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDestroy()
        {
            if (pauseSource != null) pauseSource.Changed -= OnRuntimeChanged;
            if (font != null) Destroy(font);
        }
    }
}

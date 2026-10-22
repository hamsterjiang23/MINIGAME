using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Authored ending lines use the same pagination, typing and input gates as patient dialogue.</summary>
    public sealed class EndingDialoguePresenter : MonoBehaviour
    {
        private GameRuntime runtime;
        private MiniGameScreen screen;
        private SupervisorDoorwayView actor;
        private string presented = "";
        private int epoch;
        private GameObject blackout;

        public static EndingDialogueCatalog LoadCatalog()
        {
            var source = Resources.Load<TextAsset>("MiniGame/Endings/Dialogue");
            return source != null ? EndingDialogueCatalog.Parse(source.text) : null;
        }

        public void Bind(GameRuntime game, MiniGameScreen ui, SupervisorDoorwayView doorwayActor)
        {
            runtime = game; screen = ui; actor = doorwayActor; presented = ""; epoch++;
            if (blackout != null) blackout.SetActive(false);
            if (runtime.UsesEndingFlow && runtime.Endings == null)
                runtime.ConfigureEndings(LoadCatalog() ?? throw new System.InvalidOperationException("Ending dialogue resource is missing."));
        }

        private void Update()
        {
            if (runtime == null || screen == null || !runtime.UsesEndingFlow) return;
            var view = runtime.GetView();
            if (view.Paused) return;
            var line = runtime.CurrentEndingLine;
            if (line == null)
            {
                if (presented.Length != 0) { presented = ""; screen.HideDialogueActions(); }
                return;
            }
            if (presented == line.SourceId) return;
            presented = line.SourceId;
            int generation = epoch;
            if (!view.IsSelfReview)
            {
                actor?.SetPortrait(line.PortraitId);
                if (line.BlackScreen) ShowBlackout();
            }
            screen.PresentDialogueLine(runtime.CurrentEndingConversation.Speaker, line.Text,
                line.SourceId, line.PortraitId, false, () =>
                {
                    if (generation != epoch || runtime.GetView().Paused) return;
                    runtime.CompleteEndingLine(line.SourceId);
                });
        }

        private void ShowBlackout()
        {
            if (blackout == null)
            {
                blackout = new GameObject("Priest final black screen", typeof(RectTransform), typeof(Canvas));
                blackout.transform.SetParent(transform, false);
                var canvas = blackout.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 9;
                var panel = new GameObject("Black", typeof(RectTransform), typeof(Image));
                panel.transform.SetParent(blackout.transform, false);
                var rect = (RectTransform)panel.transform;
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                panel.GetComponent<Image>().color = Color.black;
                panel.GetComponent<Image>().raycastTarget = false;
            }
            blackout.SetActive(true);
        }
        private void OnDestroy() { if (blackout != null) Destroy(blackout); }
    }
}

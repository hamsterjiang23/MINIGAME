using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>The supervisor's existing subtitle styling and PCM-driven voice, owned by the ending.</summary>
    internal sealed class DreamSupervisorDialogue : MonoBehaviour
    {
        internal static readonly string[] Lines = {
            "喂，我敲了那么久你怎么不开门？", "和你说话呢，新来的。",
            "难道你傻了吗？听不到我说话啦？", "真可惜，又要招几个新人了……"
        };
        private GameObject panel;
        private Text body;
        private Font font;
        private Material border, textMaterial;
        private SupervisorVoicePlayer voice;
        private readonly DialogueTextPlayback playback = new DialogueTextPlayback();
        internal string Text => body != null ? body.text : "";

        internal void Initialize(Transform parent, MiniGameAudioService audio)
        {
            voice = gameObject.AddComponent<SupervisorVoicePlayer>();
            voice.Initialize(audio);
            font = GameUiTypography.CreateDialogueFont();
            panel = new GameObject("Dream supervisor dialogue", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(.15f, .035f); rect.anchorMax = new Vector2(.85f, .22f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = panel.GetComponent<Image>();
            image.color = new Color(.035f, .028f, .017f, .72f); image.raycastTarget = false;
            var shader = Resources.Load<Shader>("SoftDialogueBorder");
            if (shader != null) { border = new Material(shader); image.material = border; }
            var label = new GameObject("DialogueBody", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(panel.transform, false);
            body = label.GetComponent<Text>();
            body.rectTransform.anchorMin = new Vector2(.10f, .20f);
            body.rectTransform.anchorMax = new Vector2(.90f, .80f);
            body.rectTransform.offsetMin = body.rectTransform.offsetMax = Vector2.zero;
            body.font = font; body.fontSize = 34; body.alignment = TextAnchor.MiddleCenter;
            body.lineSpacing = 1.25f; body.color = new Color32(0xE4, 0xCE, 0x9A, 0xFF);
            body.raycastTarget = false;
            shader = Resources.Load<Shader>("PropNarration");
            if (shader != null)
            {
                textMaterial = new Material(shader); textMaterial.SetFloat("_PixelStep", 2f);
                body.material = textMaterial;
            }
            panel.SetActive(false);
        }

        internal void Present(int index, System.Action completed)
        {
            panel.SetActive(true);
            playback.Begin(body, Lines[index], 30, completed, false, voice);
        }
        internal void Tick(float dt) => playback.Tick(dt);
        internal void Advance() => playback.Advance();
        internal void SetPaused(bool paused) { voice.SetPaused(paused); panel.SetActive(!paused && playback.Active); }
        internal void Hide() { playback.Cancel(); panel.SetActive(false); }
        private void OnDestroy()
        {
            playback.Cancel();
            if (font != null) Destroy(font);
            if (border != null) Destroy(border);
            if (textMaterial != null) Destroy(textMaterial);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Shared supervisor dialogue styling and PCM-driven voice, owned by the ending.</summary>
    internal sealed class DreamSupervisorDialogue : MonoBehaviour
    {
        internal static readonly string[] Lines = {
            "喂，我敲了那么久你怎么不开门？", "和你说话呢，新来的。",
            "难道你傻了吗？听不到我说话啦？", "真可惜，又要招几个新人了……"
        };
        private GameObject panel;
        private Text body;
        private Font font;
        private Material textMaterial;
        private SupervisorVoicePlayer voice;
        private readonly DialogueTextPlayback playback = new DialogueTextPlayback();
        internal string Text => body != null ? body.text : "";

        internal void Initialize(Transform parent, MiniGameAudioService audio)
        {
            voice = gameObject.AddComponent<SupervisorVoicePlayer>();
            voice.Initialize(audio);
            font = GameUiTypography.CreateFont();
            Font.textureRebuilt += RefreshFontTexture;
            RefreshFontTexture(font);
            panel = new GameObject("Dream supervisor dialogue", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            var image = panel.GetComponent<Image>();
            image.color = Color.clear; image.raycastTarget = false;
            var backdrop = new GameObject("SupervisorBackdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(panel.transform, false);
            SupervisorDialogueStyle.ApplyBackdrop(backdrop.GetComponent<Image>());
            var label = new GameObject("DialogueBody", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(panel.transform, false);
            body = label.GetComponent<Text>();
            textMaterial = SupervisorDialogueStyle.CreateTextMaterial();
            SupervisorDialogueStyle.ApplyText(body, font, textMaterial);
            SupervisorDialogueStyle.Layout(rect, string.Empty);
            panel.SetActive(false);
        }

        internal void Present(int index, System.Action completed)
        {
            panel.SetActive(true);
            SupervisorDialogueStyle.Layout((RectTransform)panel.transform, Lines[index]);
            playback.Begin(body, Lines[index], 30, completed, false, voice);
        }
        private void RefreshFontTexture(Font rebuilt)
        {
            if (rebuilt == font && font != null && font.material != null && font.material.mainTexture != null)
                font.material.mainTexture.filterMode = FilterMode.Point;
        }
        internal void Tick(float dt) => playback.Tick(dt);
        internal void Advance() => playback.Advance();
        internal void SetPaused(bool paused) { voice.SetPaused(paused); panel.SetActive(!paused && playback.Active); }
        internal void Hide() { playback.Cancel(); panel.SetActive(false); }
        private void OnDestroy()
        {
            playback.Cancel();
            Font.textureRebuilt -= RefreshFontTexture;
            if (font != null) Destroy(font);
            if (textMaterial != null) Destroy(textMaterial);
        }
    }
}

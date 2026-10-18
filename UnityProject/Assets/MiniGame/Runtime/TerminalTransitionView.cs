using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only CRT startup/shutdown overlay; it never advances game state.</summary>
    internal sealed class TerminalTransitionView
    {
        private readonly CanvasGroup group;
        private readonly Image curtain;
        private readonly Image topBar;
        private readonly Image bottomBar;
        private readonly RectTransform phosphorLine;
        private readonly RectTransform progressFill;
        private readonly Text title;
        private readonly Text status;
        private readonly Text code;

        public TerminalTransitionView(RectTransform parent, Font font)
        {
            var root = new GameObject("TerminalHandoff", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            Stretch(rect, Vector2.zero, Vector2.one);
            group = root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false; group.interactable = false;
            curtain = Panel(rect, "PowerCurtain", new Color(.008f, .016f, .013f, 0), Vector2.zero, Vector2.one);
            topBar = Panel(rect, "UpperLetterbox", Color.black, new Vector2(0, .95f), Vector2.one);
            bottomBar = Panel(rect, "LowerLetterbox", Color.black, Vector2.zero, new Vector2(1, .05f));
            var content = new GameObject("BootReadout", typeof(RectTransform));
            content.transform.SetParent(rect, false);
            var contentRect = content.GetComponent<RectTransform>();
            Stretch(contentRect, new Vector2(.19f, .31f), new Vector2(.81f, .68f));
            title = Label(contentRect, font, "BootTitle", "PEACE / LOCAL TERMINAL", 32, new Vector2(0, .73f), new Vector2(1, .94f));
            status = Label(contentRect, font, "BootStatus", "", 23, new Vector2(0, .25f), new Vector2(1, .69f));
            code = Label(contentRect, font, "BootCode", "", 16, new Vector2(0, -.08f), new Vector2(1, .09f));
            Panel(contentRect, "BootRule", new Color(.22f, .42f, .31f), new Vector2(0, .70f), new Vector2(1, .703f));
            Panel(contentRect, "ProgressTrack", new Color(.025f, .06f, .04f), new Vector2(0, .12f), new Vector2(1, .132f));
            progressFill = Panel(contentRect, "ProgressFill", new Color(.54f, .84f, .63f), new Vector2(0, .12f), new Vector2(0, .132f)).rectTransform;
            phosphorLine = Panel(rect, "PhosphorLine", new Color(.73f, 1f, .82f), new Vector2(.2f, .499f), new Vector2(.8f, .501f)).rectTransform;
            SetFrame(0, false, false);
        }

        public void SetFrame(float progress, bool exiting, bool active)
        {
            float p = Mathf.Clamp01(progress);
            group.alpha = active ? 1 : 0;
            if (!active) return;
            float letterbox = Mathf.Sin(p * Mathf.PI) * .92f;
            topBar.color = bottomBar.color = new Color(0, 0, 0, letterbox);
            float black = exiting ? Window(p, .05f, .23f, .48f, .78f) : Window(p, .48f, .67f, .82f, .98f);
            curtain.color = new Color(.008f, .016f, .013f, black);
            float readout = exiting ? Window(p, .1f, .24f, .38f, .54f) : Window(p, .57f, .68f, .79f, .9f);
            title.color = new Color(.69f, .88f, .74f, readout);
            status.color = new Color(.46f, .66f, .52f, readout);
            code.color = new Color(.32f, .48f, .38f, readout);
            title.text = exiting ? "PEACE / SESSION CLOSED" : "PEACE / LOCAL TERMINAL";
            status.text = exiting ? "会谈记录已封存\n正在断开工作连接……"
                : p < .72f ? "正在建立加密连接……\n摄像头 B-12  /  信号自检"
                : "影像通道已连接\n人员档案就绪";
            code.text = exiting ? "SEC.047    /    RETURN TO QUARTERS" : "SEC.047    /    PERSONNEL MONITORING SYSTEM";
            progressFill.anchorMax = new Vector2(exiting ? 1 - p : Mathf.InverseLerp(.57f, .88f, p), .132f);
            progressFill.GetComponent<Image>().color = new Color(.54f, .84f, .63f, readout);
            var lineImage = phosphorLine.GetComponent<Image>();
            float pulse = exiting ? Window(p, .24f, .34f, .41f, .55f) : Window(p, .50f, .57f, .60f, .67f);
            lineImage.color = new Color(.73f, 1f, .82f, pulse * .9f);
            phosphorLine.localScale = new Vector3(Mathf.Lerp(.12f, 1f, pulse), 1, 1);
            foreach (var image in status.transform.parent.GetComponentsInChildren<Image>())
            {
                if (image.name == "BootRule" || image.name == "ProgressTrack")
                { var color = image.color; color.a = readout; image.color = color; }
            }
        }

        private static float Window(float p, float start, float peak, float fade, float end) =>
            Mathf.SmoothStep(0, 1, Mathf.InverseLerp(start, peak, p)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(fade, end, p)));

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }

        private static Image Panel(RectTransform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            Stretch((RectTransform)go.transform, min, max);
            return image;
        }

        private static Text Label(RectTransform parent, Font font, string name, string value, int size, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.font = font; label.text = value; label.fontSize = size; label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false; label.supportRichText = false;
            Stretch((RectTransform)go.transform, min, max);
            return label;
        }
    }
}

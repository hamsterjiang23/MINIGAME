using System;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Native-pixel industrial controls from the terminal HTML reference.</summary>
    internal static class TerminalIndustrialSkin
    {
        public static RectTransform Panel(Transform parent, string name, Color color,
            float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            Position(rect, x, y, width, height);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        public static RectTransform Bevel(Transform parent, string name,
            float x, float y, float width, float height, Color fill, bool sunken = false)
        {
            var root = Panel(parent, name, ColorOf(sunken ? "68736b" : "89968d"), x, y, width, height);
            Panel(root, "InnerRim", ColorOf(sunken ? "979f98" : "edf0e7"), 1, 1, width - 2, height - 2);
            Panel(root, "WorkingFace", fill, 2, 2, width - 4, height - 4);
            var leading = ColorOf(sunken ? "59665d" : "f4f1e9");
            var trailing = ColorOf(sunken ? "f2f3eb" : "566159");
            Panel(root, "TopRim", leading, 1, 1, width - 2, 1);
            Panel(root, "LeftRim", leading, 1, 1, 1, height - 2);
            Panel(root, "BottomRim", trailing, 1, height - 2, width - 2, 1);
            Panel(root, "RightRim", trailing, width - 2, 1, 1, height - 2);
            if (sunken && width > 10 && height > 10)
            {
                Panel(root, "InsetTop", ColorOf("b8c1b6"), 3, 3, width - 6, 1);
                Panel(root, "InsetLeft", ColorOf("b8c1b6"), 3, 3, 1, height - 6);
            }
            return root;
        }

        public static void Frame(Transform parent, Font font, string name, string title,
            float x, float y, float width, float height, bool dark = false)
        {
            var root = Bevel(parent, name, x, y, width, height, ColorOf(dark ? "384640" : "c6cbc3"));
            if (dark)
            {
                Panel(root, "HeaderHighlight", ColorOf("667c6c"), 2, 2, width - 4, 1);
                Panel(root, "HeaderShadow", ColorOf("0f241b"), 2, height - 3, width - 4, 1);
                Panel(root, "RegistrationStrip", ColorOf("94a79a"), 5, 4, 2, height - 10);
                Label(root, font, "Heading", title, 12, 3, width - 24, height - 6, 13, ColorOf("e0e3da"));
                return;
            }

            Panel(root, "FrameHeading", ColorOf("9fa9a0"), 3, 3, width - 6, 16);
            Panel(root, "HeadingHighlight", ColorOf("dee5d9"), 4, 4, width - 8, 1);
            Panel(root, "HeadingShadow", ColorOf("58665c"), 4, 18, width - 8, 1);
            Panel(root, "RegistrationStrip", ColorOf("485a4d"), 5, 6, 2, 10);
            Label(root, font, "Heading", title, 11, 3, width - 30, 16, 12, ColorOf("26312c"));
            if (width > 80)
            {
                Panel(root, "EndCap", ColorOf("8a968d"), width - 18, 7, 11, 9);
                Panel(root, "EndCapHighlight", ColorOf("dce4d9"), width - 17, 8, 9, 1);
                Panel(root, "EndCapShadow", ColorOf("57645a"), width - 17, 13, 9, 1);
            }
            Panel(root, "WorkingField", ColorOf("c9cec7"), 3, 20, width - 6, height - 24);
            Panel(root, "FieldBottom", ColorOf("78877c"), 3, height - 3, width - 6, 1);
        }

        public static Button Button(Transform parent, Font font, string name, string label,
            float x, float y, float width, float height, Action action, int size = 11, bool rowStyle = false)
        {
            var root = Panel(parent, name, ColorOf("8b988e"), x, y, width, height);
            root.GetComponent<Image>().raycastTarget = true;
            var button = root.gameObject.AddComponent<TerminalButtonVisual>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.Initialize(font, label, width, height, size, rowStyle);
            button.onClick.AddListener(() =>
            {
                if (button.IsActive() && button.IsInteractable()) action?.Invoke();
            });
            return button;
        }

        public static void SetSelected(Button button, bool selected)
        {
            if (button is TerminalButtonVisual visual) visual.SetSelected(selected);
        }

        public static void SetChoiceRow(Button button)
        {
            if (button is TerminalButtonVisual visual) visual.SetChoiceRow();
        }

        public static void RegisterLabel(Button button, Text label)
        {
            if (button is TerminalButtonVisual visual) visual.RegisterLabel(label);
        }

        public static void AddCrtOverlay(RectTransform logical)
        {
            var shader = Resources.Load<Shader>("TerminalCrtOverlay");
            if (shader == null) shader = Shader.Find("MiniGame/UI/TerminalCrtOverlay");
            if (shader == null)
            {
                Debug.LogWarning("Terminal CRT overlay shader resource is unavailable.");
                return;
            }
            var overlay = Panel(logical, "TerminalCrtOverlay", Color.white, 0, 0, 640, 360);
            var material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            overlay.GetComponent<Image>().material = material;
            overlay.gameObject.AddComponent<TerminalCrtMaterialOwner>().OwnedMaterial = material;
        }

        internal static Text Label(Transform parent, Font font, string name, string value,
            float x, float y, float width, float height, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TerminalBitmapText));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            Position((RectTransform)go.transform, x, y, width, height);
            var label = go.GetComponent<Text>();
            label.font = font;
            label.text = value;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = false;
            label.raycastTarget = false;
            return label;
        }

        internal static void Position(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(Mathf.Max(0, width), Mathf.Max(0, height));
        }

        internal static Color ColorOf(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }

    internal sealed class TerminalCrtMaterialOwner : MonoBehaviour
    {
        public Material OwnedMaterial;

        private void OnDestroy()
        {
            if (OwnedMaterial == null) return;
            if (Application.isPlaying) Destroy(OwnedMaterial);
            else DestroyImmediate(OwnedMaterial);
        }
    }
}

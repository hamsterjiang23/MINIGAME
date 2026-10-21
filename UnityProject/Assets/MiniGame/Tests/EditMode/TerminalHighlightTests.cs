using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class TerminalHighlightTests
    {
        private static readonly Type VisualType = typeof(MiniGameScreen).Assembly.GetType("MiniGame.UnityHost.TerminalButtonVisual", true);
        private static readonly Type GroupType = typeof(MiniGameScreen).Assembly.GetType("MiniGame.UnityHost.TerminalHighlightGroup", true);
        private GameObject owner;

        [SetUp]
        public void CreatePage()
        {
            owner = new GameObject("Terminal highlight fixture", typeof(RectTransform));
            owner.AddComponent(GroupType);
        }

        [TearDown]
        public void DestroyPage() => Object.DestroyImmediate(owner);

        [Test]
        public void PointerOwnsOneRowAndLeavingOrKeyboardNavigationRestoresSelection()
        {
            var first = Row("First"); var second = Row("Second"); var footer = Row("Footer");
            Invoke(first, "SetSelected", true);
            Invoke(footer, "SetAlwaysHighlighted", true);
            AssertFace(first, "eeb430"); AssertFace(second, "0b0803");

            second.OnPointerEnter(Pointer());
            AssertFace(first, "0b0803"); AssertFace(second, "eeb430"); AssertFace(footer, "eeb430");
            AssertLabels(second, "170e02"); AssertLabels(first, "efb83f");

            second.OnPointerExit(Pointer());
            AssertFace(first, "eeb430"); AssertFace(second, "0b0803");
            second.OnPointerEnter(Pointer());
            GroupType.GetMethod("ClearPointerHighlight").Invoke(null, new object[] { owner.transform });
            AssertFace(first, "eeb430"); AssertFace(second, "0b0803");

            var moved = Pointer(); moved.delta = Vector2.right;
            ((IPointerMoveHandler)second).OnPointerMove(moved);
            AssertFace(first, "0b0803"); AssertFace(second, "eeb430");

            second.OnPointerExit(Pointer()); second.OnPointerEnter(Pointer());
            second.interactable = false;
            AssertFace(first, "eeb430"); AssertFace(second, "0b0803"); AssertLabels(second, "674b19");
            footer.interactable = false;
            AssertFace(footer, "eeb430"); AssertLabels(footer, "170e02");
        }

        [Test]
        public void PressOffsetsEveryLabelOnceAndResetsOnReleaseExitDisableOrFocusLoss()
        {
            var button = Row("Press");
            var extra = new GameObject("Availability", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            extra.transform.SetParent(button.transform, false); extra.rectTransform.anchoredPosition = new Vector2(100, -5);
            Invoke(button, "RegisterLabel", extra);
            var label = button.transform.Find("Label").GetComponent<Text>();
            Vector2 labelRest = label.rectTransform.anchoredPosition, extraRest = extra.rectTransform.anchoredPosition;
            for (int repetition = 0; repetition < 3; repetition++)
            {
                button.OnPointerDown(Pointer()); button.OnPointerDown(Pointer());
                Assert.That(label.rectTransform.anchoredPosition, Is.EqualTo(labelRest + new Vector2(1, -1)));
                Assert.That(extra.rectTransform.anchoredPosition, Is.EqualTo(extraRest + new Vector2(1, -1)));
                Assert.That(button.transform.Find("PressedInset").gameObject.activeSelf, Is.True);
                button.OnPointerUp(Pointer()); AssertRestored();
            }
            button.OnPointerDown(Pointer()); button.OnPointerExit(Pointer()); AssertRestored();
            button.OnPointerDown(Pointer()); button.interactable = false; AssertRestored();
            button.interactable = true;
            button.OnPointerDown(Pointer()); button.gameObject.SetActive(false); AssertRestored();
            button.gameObject.SetActive(true);
            button.OnPointerDown(Pointer()); button.SendMessage("OnApplicationFocus", false); AssertRestored();

            void AssertRestored()
            {
                Assert.That(label.rectTransform.anchoredPosition, Is.EqualTo(labelRest));
                Assert.That(extra.rectTransform.anchoredPosition, Is.EqualTo(extraRest));
                Assert.That(button.transform.Find("PressedInset").gameObject.activeSelf, Is.False);
            }
        }

        [Test]
        public void CurrentCommandUsesOnlyAThinFrameAndVisualEventsNeverSubmitAnAction()
        {
            var selected = Row("Selected"); var field = Row("Current command");
            Invoke(selected, "SetSelected", true); Invoke(field, "SetCommandField", true);
            int activations = 0; field.onClick.AddListener(() => activations++);
            field.OnPointerEnter(Pointer()); field.OnPointerDown(Pointer()); field.OnPointerUp(Pointer());
            AssertFace(field, "0b0803"); AssertLabels(field, "ffcf68"); AssertFace(selected, "eeb430");
            Assert.That(field.transform.Find("CommandOutline").gameObject.activeSelf, Is.True);
            Assert.That(field.transform.Find("CommandOutline").GetComponentsInChildren<Image>(), Has.Length.EqualTo(4));
            field.OnSubmit(new BaseEventData(null));
            Assert.That(activations, Is.Zero, "The raw screen keyboard handler owns Enter; visual submit must not duplicate it.");
            Assert.That(((MiniGameAudioButton)field).ActivateWithAudio(), Is.True);
            Assert.That(activations, Is.EqualTo(1));
            field.OnPointerClick(Pointer());
            Assert.That(activations, Is.EqualTo(2), "Each accepted click retains the existing audio button activation path.");
            field.interactable = false;
            field.OnPointerClick(Pointer());
            Assert.That(activations, Is.EqualTo(2)); AssertLabels(field, "674b19");
        }

        private Button Row(string label)
        {
            var child = new GameObject(label, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(owner.transform, false);
            ((RectTransform)child.transform).sizeDelta = new Vector2(248, 28);
            var button = (Button)child.AddComponent(VisualType);
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Invoke(button, "Initialize", null, label, 248f, 28f, 14, true);
            return button;
        }

        private static PointerEventData Pointer() => new PointerEventData(null) { button = PointerEventData.InputButton.Left };
        private static void Invoke(Button button, string method, params object[] arguments)
            => VisualType.GetMethod(method, BindingFlags.Instance | BindingFlags.Public).Invoke(button, arguments);
        private static void AssertFace(Button button, string hex)
            => Assert.That((Color32)button.GetComponent<Image>().color, Is.EqualTo(ColorOf(hex)), button.name + " background");
        private static void AssertLabels(Button button, string hex)
        {
            foreach (var label in button.GetComponentsInChildren<Text>(true))
                Assert.That((Color32)label.color, Is.EqualTo(ColorOf(hex)), button.name + "/" + label.name + " foreground");
        }
        private static Color32 ColorOf(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color); return color;
        }
    }
}

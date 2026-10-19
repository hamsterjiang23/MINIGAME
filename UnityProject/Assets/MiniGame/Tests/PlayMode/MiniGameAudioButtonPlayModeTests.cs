using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class MiniGameAudioButtonPlayModeTests
    {
        private MiniGameAudioService service;
        private GameObject buttonObject;
        private MiniGameAudioButton button;
        private int actions;
        private int uiSounds;

        [UnitySetUp]
        public IEnumerator LoadRealAudioAndCreateScreenOwnedButton()
        {
            actions = uiSounds = 0;
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            var host = Object.FindObjectOfType<MiniGameBootstrap>();
            Assert.That(host, Is.Not.Null, "MainRoomCombined must provide the actual gameplay/audio bootstrap.");
            host.Player.enabled = false;
            Assert.That(host.Audio, Is.Not.Null, "Bootstrap must create and bind its scene-owned audio director.");
            Assert.That(host.Screen.Audio, Is.SameAs(host.Audio), "Buttons must resolve the director through their owning screen.");
            service = host.Audio.Service;
            Assert.That(service, Is.Not.Null);
            Assert.That(host.Audio.Profile, Is.Not.Null, "The real audio profile must be installed before running these tests.");
            Assert.That(service.Profile, Is.SameAs(host.Audio.Profile));
            Assert.That(service.Profile.GetClip(MiniGameSound.UIInteraction), Is.Not.Null,
                "The configured UIInteraction clip is required; synthetic audio is not substituted by this fixture.");
            Assert.That(service.GetSource(MiniGameAudioChannel.UI), Is.Not.Null);
            Assert.That(EventSystem.current, Is.Not.Null);
            service.SoundPlayed += OnSoundPlayed;

            buttonObject = new GameObject("Audio button regression", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(MiniGameAudioButton));
            buttonObject.transform.SetParent(host.Screen.Canvas.transform, false);
            ((RectTransform)buttonObject.transform).sizeDelta = new Vector2(40, 20);
            button = buttonObject.GetComponent<MiniGameAudioButton>();
            button.onClick.AddListener(() => actions++);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator RemoveOnlyTheFixtureButtonAndSubscription()
        {
            if (service != null) service.SoundPlayed -= OnSoundPlayed;
            if (buttonObject != null) Object.Destroy(buttonObject);
            yield return null;
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator NativeSubmitThenRawKeyboardInOneFramePlaysAndActsOnce()
        {
            Submit();
            AssertCounts(1);
            Assert.That(button.ActivateWithAudio(), Is.True);
            AssertCounts(1);
            Assert.That(button.ActivateWithAudio(), Is.True, "An additional deliberate manual activation must not be suppressed.");
            AssertCounts(2);
            Click(PointerEventData.InputButton.Left);
            AssertCounts(3); // Pointer activations are independent of the native/raw submit pair.
            yield return null;
            Assert.That(button.ActivateWithAudio(), Is.True);
            AssertCounts(4);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator RawKeyboardThenNativeSubmitInOneFramePlaysAndActsOnce()
        {
            Assert.That(button.ActivateWithAudio(), Is.True);
            AssertCounts(1);
            Submit();
            AssertCounts(1);
            Assert.That(button.ActivateWithAudio(), Is.True, "Deduplication must consume only the matching native/raw pair.");
            AssertCounts(2);
            Click(PointerEventData.InputButton.Left);
            AssertCounts(3);
            yield return null;
            Submit();
            AssertCounts(4);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator PointerActivationPlaysOnceButRightClickAndDisabledControlsStaySilent()
        {
            Click(PointerEventData.InputButton.Right);
            AssertCounts(0);
            Click(PointerEventData.InputButton.Left);
            AssertCounts(1);

            button.interactable = false;
            Click(PointerEventData.InputButton.Left);
            Submit();
            Assert.That(button.ActivateWithAudio(), Is.False);
            AssertCounts(1);
            button.interactable = true;
            button.enabled = false;
            Click(PointerEventData.InputButton.Left);
            Submit();
            Assert.That(button.ActivateWithAudio(), Is.False);
            AssertCounts(1);

            button.enabled = true;
            var group = buttonObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            yield return null;
            Assert.That(button.IsInteractable(), Is.False, "CanvasGroup input gating must be respected as well as Button.interactable.");
            Click(PointerEventData.InputButton.Left);
            Submit();
            Assert.That(button.ActivateWithAudio(), Is.False);
            AssertCounts(1);
            group.interactable = true;
            yield return null;
            Click(PointerEventData.InputButton.Left);
            AssertCounts(2);
        }

        [UnityTest, Timeout(45000)]
        public IEnumerator ReplacingContinueListenersPreservesSoundAcrossPointerSubmitAndKeyboard()
        {
            int originalCalls = 0;
            int replacementCalls = 0;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { originalCalls++; actions++; });
            Click(PointerEventData.InputButton.Left);
            AssertCounts(1);
            yield return null;

            button.onClick.RemoveAllListeners(); // Matches the dialogue Continue button's per-line callback replacement.
            button.onClick.AddListener(() => { replacementCalls++; actions++; });
            Click(PointerEventData.InputButton.Left);
            AssertCounts(2);
            Assert.That(originalCalls, Is.EqualTo(1));
            Assert.That(replacementCalls, Is.EqualTo(1));
            yield return null;
            Submit();
            AssertCounts(3);
            yield return null;
            Assert.That(button.ActivateWithAudio(), Is.True);
            AssertCounts(4);
            Assert.That(originalCalls, Is.EqualTo(1), "A replaced callback must not be retained by the audio hook.");
            Assert.That(replacementCalls, Is.EqualTo(3));
        }

        private void Click(PointerEventData.InputButton inputButton)
        {
            var pointer = new PointerEventData(EventSystem.current) { button = inputButton, clickCount = 1 };
            ExecuteEvents.Execute(buttonObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private void Submit()
            => ExecuteEvents.Execute(buttonObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

        private void OnSoundPlayed(MiniGameAudioChannel channel, MiniGameSound sound)
        {
            if (channel == MiniGameAudioChannel.UI && sound == MiniGameSound.UIInteraction) uiSounds++;
        }

        private void AssertCounts(int expected)
        {
            Assert.That(actions, Is.EqualTo(expected), "Incorrect button action count at frame " + Time.frameCount);
            Assert.That(uiSounds, Is.EqualTo(expected), "UIInteraction playback count must equal accepted activations at frame " + Time.frameCount);
        }
    }
}

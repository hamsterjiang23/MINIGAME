using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DayNightLightingPlayModeTests
    {
        private MiniGameBootstrap host;
        private RoomLightingController controller;
        private RoomLightingProfile source, working, sourceNight, workingNight;
        private string sourceJson, sourceNightJson;
        private string sharedFile;

        [UnitySetUp]
        public IEnumerator OpenRoom()
        {
            MiniGameSessionHost.Release();
            yield return SceneManager.LoadSceneAsync("MainRoomCombined");
            yield return null;
            host = Object.FindObjectOfType<MiniGameBootstrap>();
            host.StartTerminalDemo();
            Assert.That(host.Runtime.TryInteract(MiniGame.Core.InteractionTarget.Door).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
            Assert.That(host.Runtime.CompleteCue(host.Runtime.GetView().Cue.Token).Accepted, Is.True);
            controller = Object.FindObjectsOfType<RoomLightingController>().Single(item => item.gameObject.scene == host.gameObject.scene);
            source = controller.Profile; sourceJson = JsonUtility.ToJson(source);
            sourceNight = controller.NightProfile; sourceNightJson = JsonUtility.ToJson(sourceNight);
            working = Object.Instantiate(source); working.name = "Day night lighting isolated test";
            workingNight = Object.Instantiate(sourceNight); workingNight.name = "Independent night isolated test";
            // Only fixture wiring is replaced; the shared designer asset must remain byte-for-byte unedited.
            controller.ConfigureProfiles(working, workingNight);
            host.Screen.LightingTuning.Bind(host.Runtime, controller, host.Screen.SignalTuning);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (host != null) host.Screen.LightingTuning.SetPanelOpen(false);
            if (controller != null)
            {
                controller.SetPreviewMode(RoomLightingPreviewMode.Automatic);
                controller.ConfigureProfiles(source, sourceNight);
            }
            if (working != null) Object.Destroy(working);
            if (workingNight != null) Object.Destroy(workingNight);
            if (!string.IsNullOrEmpty(sharedFile))
            {
                Assert.That(Path.GetFileName(sharedFile), Does.StartWith("minigame-lighting-panel-"));
                if (File.Exists(sharedFile)) File.Delete(sharedFile);
            }
            MiniGameSessionHost.Release();
            yield return null;
            Assert.That(JsonUtility.ToJson(source), Is.EqualTo(sourceJson));
            Assert.That(JsonUtility.ToJson(sourceNight), Is.EqualTo(sourceNightJson));
        }

        [UnityTest, Timeout(20000)]
        public IEnumerator BothEditingPeriodsExposeTheSameCompleteControls()
        {
            var panel = host.Screen.LightingTuning;
            panel.SetPanelOpen(true); panel.SelectPeriod(RoomLightingPeriod.Day);
            yield return null;
            var dayRows = panel.GetComponentsInChildren<Slider>(true).Select(value => value.transform.parent.name).OrderBy(value => value).ToArray();
            panel.SelectPeriod(RoomLightingPeriod.Night);
            yield return null;
            var nightRows = panel.GetComponentsInChildren<Slider>(true).Select(value => value.transform.parent.name).OrderBy(value => value).ToArray();
            Assert.That(nightRows, Is.EqualTo(dayRows));
            Assert.That(panel.Profile, Is.SameAs(workingNight));
            Assert.That(panel.Profile, Is.Not.SameAs(working));
        }

        [UnityTest, Timeout(20000)]
        public IEnumerator GamePanelCanExportAndImportARealShareFileWithoutChangingTheGame()
        {
            var panel = host.Screen.LightingTuning;
            panel.SetPanelOpen(true); panel.SelectPeriod(RoomLightingPeriod.Night);
            string game = host.Runtime.Save();
            sharedFile = Path.Combine(Path.GetTempPath(), "minigame-lighting-panel-" + Guid.NewGuid().ToString("N") + ".json");
            string dayBefore = JsonUtility.ToJson(working);
            workingNight.lampIntensity = 2.1f; workingNight.lightMultiplier = .35f; workingNight.NotifyChanged();
            Assert.That(panel.ExportPreset(sharedFile), Is.True);
            workingNight.lampIntensity = 7; workingNight.lightMultiplier = 1.5f; workingNight.NotifyChanged(); controller.ApplyFromProfile();
            Assert.That(panel.ImportPreset(sharedFile), Is.True);
            yield return null;
            Assert.That(workingNight.lampIntensity, Is.EqualTo(2.1f).Within(.001f));
            Assert.That(workingNight.lightMultiplier, Is.EqualTo(.35f).Within(.001f));
            Assert.That(JsonUtility.ToJson(working), Is.EqualTo(dayBefore));
            Assert.That(panel.SelectedPeriod, Is.EqualTo(RoomLightingPeriod.Night));
            Assert.That(host.Runtime.Save(), Is.EqualTo(game));
        }

        [UnityTest, Timeout(20000)]
        public IEnumerator GamePanelOwnsInputAndRestoresPauseAndAutomaticLighting()
        {
            var panel = host.Screen.LightingTuning;
            string token = host.Runtime.GetView().Cue?.Token;
            Assert.That(panel.SetPanelOpen(true), Is.True);
            Assert.That(host.Runtime.GetView().Paused, Is.True);
            Assert.That(host.Screen.GetComponentsInChildren<PauseMenuView>(true).Single().gameObject.activeInHierarchy,
                Is.False, "The tool must not cover live lighting with a frozen ordinary pause backdrop.");
            Assert.That(host.Screen.PresentationToolsBlockInput, Is.True);
            Assert.That(Cursor.visible, Is.True);
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
            Assert.That(host.Screen.RequestDayZeroOpening(), Is.False);
            string frozen = host.Runtime.Save();
            panel.SetPreview(RoomLightingPreviewMode.Night);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(host.Runtime.Save(), Is.EqualTo(frozen), "A lighting preview must not advance or rewrite the game.");
            Assert.That(controller.PreviewMode, Is.EqualTo(RoomLightingPreviewMode.Night));
            panel.SetPreview(RoomLightingPreviewMode.Day);
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "lighting-game-panel-day");
            Assert.That(panel.SetPanelOpen(false), Is.False);
            Assert.That(host.Runtime.GetView().Paused, Is.False);
            Assert.That(controller.PreviewMode, Is.EqualTo(RoomLightingPreviewMode.Automatic));
            Assert.That(panel.BlocksInput, Is.True, "Closing input cannot activate the room in the same frame.");
            Assert.That(host.Runtime.GetView().Cue?.Token, Is.EqualTo(token));
            yield return null; yield return null;
            Assert.That(panel.BlocksInput, Is.False);
            Assert.That(host.Runtime.SetPaused(true).Accepted, Is.True);
            panel.SetPanelOpen(true); panel.SetPanelOpen(false);
            Assert.That(host.Runtime.GetView().Paused, Is.True, "Opening an already-paused game preserves pause.");
            Assert.That(host.Screen.GetComponentsInChildren<PauseMenuView>(true).Single().gameObject.activeInHierarchy, Is.True);
            panel.SetPanelOpen(true);
            host.Screen.SignalTuning.SetPanelOpen(true);
            Assert.That(panel.IsPanelOpen, Is.False);
            panel.SetPanelOpen(true);
            Assert.That(host.Screen.SignalTuning.PanelOpen, Is.False);
        }

        [UnityTest, Timeout(20000)]
        public IEnumerator GameControlsAndExternalSceneChangesShareIndependentNightValues()
        {
            var panel = host.Screen.LightingTuning;
            panel.SetPanelOpen(true); panel.SelectPeriod(RoomLightingPeriod.Night);
            var selected = controller.GetEditableSceneLights(RoomLightingPeriod.Day).First();
            string path = controller.GetHierarchyPath(selected);
            var day = controller.CaptureSceneLight(selected, RoomLightingPeriod.Day);
            Assert.That(panel.SelectLight(path), Is.True);
            var night = controller.CaptureSceneLight(selected, RoomLightingPeriod.Night);
            night.dayEnabled = true; night.intensity = 1.2f;
            workingNight.SceneLights.Add(night); workingNight.NotifyChanged(); controller.ApplyFromProfile();
            panel.SetPreview(RoomLightingPreviewMode.Night); panel.Refresh();
            yield return null; // Rebuilt uGUI rows release their previous GameObjects at end of frame.
            var intensity = panel.GetComponentsInChildren<InputField>(true).Single(field => field.transform.parent.name == "灯光强度");
            intensity.text = "2.6"; intensity.onEndEdit.Invoke(intensity.text);
            yield return null; yield return null;
            Assert.That(selected.intensity, Is.EqualTo(2.6f).Within(.001f));
            Assert.That(controller.CaptureSceneLight(selected, RoomLightingPeriod.Day).intensity, Is.EqualTo(day.intensity).Within(.001f));
            Assert.That(controller.CaptureSceneLight(selected, RoomLightingPeriod.Night).intensity, Is.EqualTo(2.6f).Within(.001f));
            var record = workingNight.SceneLights.Single(item => item.HierarchyPath == path);
            record.intensity = 3.4f; workingNight.NotifyChanged(); controller.ApplyFromProfile();
            panel.Refresh();
            Assert.That(float.Parse(intensity.text, System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(3.4f).Within(.001f),
                "A Scene-side update must refresh the same Game-side controls.");
            yield return PrototypePlayModeTests.CaptureIfRequested(host, "lighting-game-panel-night");
            panel.SetPanelOpen(false);
            Assert.That(selected.intensity, Is.EqualTo(day.intensity).Within(.001f));
        }
    }
}

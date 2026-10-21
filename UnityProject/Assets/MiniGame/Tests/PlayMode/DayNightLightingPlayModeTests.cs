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

        [UnityTest, Timeout(60000)]
        public IEnumerator EverySceneLightRespondsToAllGameControlsInBothPeriods()
        {
            var panel = host.Screen.LightingTuning;
            panel.SetPanelOpen(true);
            foreach (var period in new[] { RoomLightingPeriod.Day, RoomLightingPeriod.Night })
            {
                panel.SelectPeriod(period);
                panel.SetPreview(period == RoomLightingPeriod.Day ? RoomLightingPreviewMode.Day : RoomLightingPreviewMode.Night);
                yield return null;
                var other = period == RoomLightingPeriod.Day ? workingNight : working;
                string otherBefore = JsonUtility.ToJson(other);
                var lights = controller.GetEditableSceneLights(period);
                Assert.That(lights.Length, Is.GreaterThanOrEqualTo(10), "The actual scene must include the restored window and monitor lights.");
                foreach (var light in lights)
                {
                    string path = controller.GetHierarchyPath(light);
                    Assert.That(panel.SelectLight(path), Is.True, path);
                    SetLightInput(panel, "灯光强度", .67f);
                    if (!controller.CaptureSceneLight(light, period).dayEnabled) ClickLightAction(panel, "灯光启用");
                    for (int attempts = 0; light.type != LightType.Spot && attempts < 3; attempts++) ClickLightAction(panel, "灯光类型");
                    Assert.That(light.type, Is.EqualTo(LightType.Spot), path);
                    foreach (var type in new[] { LightType.Directional, LightType.Point, LightType.Spot })
                    {
                        ClickLightAction(panel, "灯光类型"); Assert.That(light.type, Is.EqualTo(type), path);
                        Assert.That(LightInput(panel, "灯光范围").interactable, Is.EqualTo(type != LightType.Directional), path);
                        Assert.That(LightInput(panel, "聚光外角").interactable, Is.EqualTo(type == LightType.Spot), path);
                    }
                    SetLightInput(panel, "灯光颜色 R", .31f); SetLightInput(panel, "灯光颜色 G", .55f); SetLightInput(panel, "灯光颜色 B", .79f);
                    SetLightInput(panel, "灯光范围", 4.2f);
                    float x = light.transform.localPosition.x + .11f;
                    float y = light.transform.localPosition.y + .07f, z = light.transform.localPosition.z - .09f;
                    SetLightInput(panel, "局部位置 X", x); SetLightInput(panel, "局部位置 Y", y); SetLightInput(panel, "局部位置 Z", z);
                    SetLightInput(panel, "局部旋转 X", 12); SetLightInput(panel, "局部旋转 Y", 37); SetLightInput(panel, "局部旋转 Z", 8);
                    SetLightInput(panel, "聚光外角", 97); SetLightInput(panel, "聚光内角", 31);
                    for (int attempts = 0; light.shadows != LightShadows.Soft && attempts < 3; attempts++) ClickLightAction(panel, "阴影类型");
                    foreach (var shadows in new[] { LightShadows.None, LightShadows.Hard, LightShadows.Soft })
                    {
                        ClickLightAction(panel, "阴影类型"); Assert.That(light.shadows, Is.EqualTo(shadows), path);
                        Assert.That(LightInput(panel, "阴影强度").interactable, Is.EqualTo(shadows != LightShadows.None), path);
                    }
                    SetLightInput(panel, "阴影强度", .63f); SetLightInput(panel, "阴影偏差", .09f); SetLightInput(panel, "阴影法线偏差", .17f);
                    yield return null;
                    Assert.That(light.enabled, Is.True, path);
                    Assert.That(light.intensity, Is.EqualTo(.67f).Within(.001f), path);
                    Assert.That(light.range, Is.EqualTo(4.2f).Within(.001f), path);
                    Assert.That(light.color.r, Is.EqualTo(.31f).Within(.001f), path);
                    Assert.That(light.color.g, Is.EqualTo(.55f).Within(.001f), path);
                    Assert.That(light.color.b, Is.EqualTo(.79f).Within(.001f), path);
                    Assert.That(light.transform.localPosition.x, Is.EqualTo(x).Within(.001f), path);
                    Assert.That(light.transform.localPosition.y, Is.EqualTo(y).Within(.001f), path);
                    Assert.That(light.transform.localPosition.z, Is.EqualTo(z).Within(.001f), path);
                    Assert.That(Quaternion.Angle(light.transform.localRotation, Quaternion.Euler(12, 37, 8)), Is.LessThan(.05f), path);
                    Assert.That(light.spotAngle, Is.EqualTo(97).Within(.001f), path);
                    Assert.That(light.innerSpotAngle, Is.EqualTo(31).Within(.001f), path);
                    Assert.That(light.shadows, Is.EqualTo(LightShadows.Soft), path);
                    Assert.That(light.shadowStrength, Is.EqualTo(.63f).Within(.001f), path);
                    Assert.That(light.shadowBias, Is.EqualTo(.09f).Within(.001f), path);
                    Assert.That(light.shadowNormalBias, Is.EqualTo(.17f).Within(.001f), path);
                    ClickLightAction(panel, "灯光启用"); Assert.That(light.enabled, Is.False, path);
                    ClickLightAction(panel, "灯光启用"); Assert.That(light.enabled, Is.True, path);
                    SetLightInput(panel, "灯光强度", 0); Assert.That(light.intensity, Is.Zero, path);
                    SetLightInput(panel, "灯光强度", .67f);
                }
                SetLightInput(panel, "灯光整体倍率", 0);
                Assert.That(lights.All(light => !light.enabled && light.intensity == 0), Is.True);
                SetLightInput(panel, "灯光整体倍率", 2);
                Assert.That(lights.All(light => light.enabled && Mathf.Abs(light.intensity - 1.34f) < .001f), Is.True);
                Assert.That(controller.CaptureSceneLight(lights[0], period).intensity, Is.EqualTo(.67f).Within(.001f));
                SetLightInput(panel, "灯光整体倍率", 1);
                Assert.That(JsonUtility.ToJson(other), Is.EqualTo(otherBefore));
            }
        }

        private static void SetLightInput(RoomLightingTuningPanel panel, string row, float value)
        {
            var field = LightInput(panel, row);
            field.text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            field.onEndEdit.Invoke(field.text);
        }

        private static InputField LightInput(RoomLightingTuningPanel panel, string row) =>
            panel.GetComponentsInChildren<InputField>(true).Last(item => item.transform.parent.name == row);

        private static void ClickLightAction(RoomLightingTuningPanel panel, string row) =>
            panel.GetComponentsInChildren<Button>(true).Last(item => item.transform.parent.name == row).onClick.Invoke();
    }
}

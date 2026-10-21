using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RoomLightingPresetTests
    {
        private RoomLightingProfile source, target;
        private string path;

        [SetUp]
        public void CreateProfiles()
        {
            source = ScriptableObject.CreateInstance<RoomLightingProfile>(); source.name = "Alice room";
            target = ScriptableObject.CreateInstance<RoomLightingProfile>(); target.name = "Bob room";
            source.lampIntensity = 2.3f; source.windowColor = Color.cyan;
            source.ambientColor = new Color(.05f, .1f, .2f); source.lightMultiplier = .4f;
            source.emissionMultiplier = .7f; source.fadeSeconds = 2;
            source.ambientIntensity = 2.5f;
            source.SceneLights.Add(new SceneLightSettings { HierarchyPath = "Room#0/Lamp#0", intensity = 1.7f });
            source.SceneLightDefaults.Add(new SceneLightSettings { HierarchyPath = "Room#0/Lamp#0", intensity = 3.6f,
                color = Color.red, localPosition = new Vector3(1, 2, 3), shadowStrength = .7f });
            source.NotifyChanged();
            path = Path.Combine(Path.GetTempPath(), "minigame-lighting-preset-" + Guid.NewGuid().ToString("N") + ".json");
        }

        [TearDown]
        public void Cleanup()
        {
            if (source != null) Object.DestroyImmediate(source);
            if (target != null) Object.DestroyImmediate(target);
            Assert.That(Path.GetFileName(path), Does.StartWith("minigame-lighting-preset-"));
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void ShareFileImportsACompletePeriodUnderAnotherProfilesName()
        {
            string original = JsonUtility.ToJson(source);
            RoomLightingPresetIO.SaveFile(source, path);
            Assert.That(File.Exists(path), Is.True);
            Assert.That(RoomLightingPresetIO.TryLoadFile(path, target, out string message), Is.True, message);
            Assert.That(target.name, Is.EqualTo("Bob room"));
            Assert.That(target.lampIntensity, Is.EqualTo(2.3f).Within(.0001f));
            Assert.That(target.windowColor, Is.EqualTo(Color.cyan));
            Assert.That(target.ambientColor, Is.EqualTo(source.ambientColor));
            Assert.That(target.ambientIntensity, Is.EqualTo(2.5f));
            Assert.That(target.lightMultiplier, Is.EqualTo(.4f));
            Assert.That(target.emissionMultiplier, Is.EqualTo(.7f));
            Assert.That(target.fadeSeconds, Is.EqualTo(2));
            Assert.That(target.SceneLights[0].intensity, Is.EqualTo(1.7f).Within(.0001f));
            Assert.That(target.SceneLightDefaults[0].intensity, Is.EqualTo(3.6f).Within(.0001f));
            Assert.That(target.SceneLightDefaults[0].localPosition, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(JsonUtility.ToJson(source), Is.EqualTo(original), "Export cannot alter the author's current configuration.");
            Assert.That(File.ReadAllText(path), Does.Not.Contain(Application.dataPath));
        }

        [Test]
        public void DamagedOrUnknownShareFilesLeaveTheCurrentConfigurationUnchanged()
        {
            string before = JsonUtility.ToJson(target);
            foreach (string broken in new[] { "{", "{}", "[]", "not json" })
            {
                Assert.That(RoomLightingPresetIO.TryImportJson(broken, target, out _), Is.False);
                Assert.That(JsonUtility.ToJson(target), Is.EqualTo(before));
            }
            string valid = RoomLightingPresetIO.ExportJson(source);
            string unknown = valid.Replace("\"version\": " + RoomLightingPresetIO.Version, "\"version\": 999")
                .Replace("\"version\":" + RoomLightingPresetIO.Version, "\"version\":999");
            Assert.That(unknown, Is.Not.EqualTo(valid));
            Assert.That(RoomLightingPresetIO.TryImportJson(unknown, target, out _), Is.False);
            Assert.That(JsonUtility.ToJson(target), Is.EqualTo(before));
        }

        [Test]
        public void LegacyDayOnlyFilesRetainDefaultsForNewFields()
        {
            source.SceneLights.Clear();
            string legacy = JsonUtility.ToJson(source);
            foreach (var field in new[] {
                "\"lampEulerAngles\":" + JsonUtility.ToJson(source.lampEulerAngles),
                "\"night\":" + JsonUtility.ToJson(source.night), "\"SceneLights\":[]" })
                legacy = legacy.Replace(field + ",", "").Replace("," + field, "");
            Assert.That(legacy, Does.Not.Contain("\"lampEulerAngles\""));
            Assert.That(legacy, Does.Not.Contain("\"night\""));
            Assert.That(RoomLightingPresetIO.TryImportJson(legacy, target, out string message), Is.True, message);
            Assert.That(target.lampIntensity, Is.EqualTo(source.lampIntensity));
            Assert.That(target.lampEulerAngles, Is.EqualTo(new Vector3(90, 0, 0)));
            Assert.That(target.night.lightMultiplier, Is.Zero);
            Assert.That(target.SceneLights, Is.Empty);
        }

        [Test]
        public void ADeletedPresetFileReturnsAnErrorWithoutReplacingCurrentValues()
        {
            string before = JsonUtility.ToJson(target);
            Assert.That(RoomLightingPresetIO.TryLoadFile(path, target, out string message), Is.False);
            Assert.That(message, Is.Not.Empty);
            Assert.That(JsonUtility.ToJson(target), Is.EqualTo(before));
        }

        [Serializable] private sealed class VersionTwoFile
        {
            public string format = RoomLightingPresetIO.Format;
            public int version = 2;
            public string profileName;
            public string profileJson;
        }

        [Test]
        public void VersionTwoFilesDefaultAmbientStrengthToOne()
        {
            string previous = JsonUtility.ToJson(source).Replace("\"ambientIntensity\":2.5,", "");
            Assert.That(previous, Does.Not.Contain("\"ambientIntensity\""));
            string json = JsonUtility.ToJson(new VersionTwoFile { profileName = source.name, profileJson = previous });
            target.ambientIntensity = .2f;
            Assert.That(RoomLightingPresetIO.TryImportJson(json, target, out string message), Is.True, message);
            Assert.That(target.ambientIntensity, Is.EqualTo(1));
            Assert.That(target.windowColor, Is.EqualTo(source.windowColor));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MiniGame.Core;
using MiniGame.Presentation.Authoring;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamSettingsEditorTests
    {
        private DreamSettingsWindow window;
        private DreamSequenceProfile originalProfile, profile;
        private PlayerRebellionProfile originalRebellion, rebellion;
        private CompiledDialoguePackage compiled;
        private string originalProfileJson, originalRebellionJson;
        private string token, profilePath, rebellionPath, retroPath;
        private RetroScreenProfile endingRetro;

        [SetUp]
        public void SetUp()
        {
            originalProfile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(DreamSequenceSetup.ProfilePath);
            originalRebellion = AssetDatabase.LoadAssetAtPath<PlayerRebellionProfile>(DreamSequenceSetup.RebellionPath);
            Assert.That(originalProfile, Is.Not.Null, "Install the authored dream assets first; these tests never create real defaults.");
            Assert.That(originalRebellion, Is.Not.Null);
            originalProfileJson = JsonUtility.ToJson(originalProfile);
            originalRebellionJson = JsonUtility.ToJson(originalRebellion);
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            Assert.That(package, Is.Not.Null);
            compiled = DialoguePackageCompiler.Compile(package.PackageSource.text);
            token = Guid.NewGuid().ToString("N");
            profilePath = "Assets/MiniGame/Tests/DreamSettingsTest-" + token + "-dream.asset";
            rebellionPath = "Assets/MiniGame/Tests/DreamSettingsTest-" + token + "-rebellion.asset";
            retroPath = "Assets/MiniGame/Tests/DreamSettingsTest-" + token + "-retro.asset";
            Assert.That(File.Exists(profilePath) || File.Exists(rebellionPath), Is.False);
            profile = Object.Instantiate(originalProfile);
            endingRetro = Object.Instantiate(originalProfile.EndingRetro);
            AssetDatabase.CreateAsset(endingRetro, retroPath);
            profile.EndingRetro = endingRetro;
            rebellion = Object.Instantiate(originalRebellion);
            AssetDatabase.CreateAsset(profile, profilePath);
            AssetDatabase.CreateAsset(rebellion, rebellionPath);
            OpenIsolatedWindow();
        }

        [TearDown]
        public void TearDown()
        {
            CloseWindow();
            if (profile != null) Undo.ClearUndo(profile);
            if (rebellion != null) Undo.ClearUndo(rebellion);
            if (endingRetro != null) Undo.ClearUndo(endingRetro);
            foreach (string path in new[] { profilePath, rebellionPath, retroPath })
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
                Assert.That(Guid.TryParseExact(token, "N", out _), Is.True);
                Assert.That(path, Does.StartWith("Assets/MiniGame/Tests/DreamSettingsTest-" + token + "-"));
                Assert.That(path, Is.Not.EqualTo(DreamSequenceSetup.ProfilePath).And.Not.EqualTo(DreamSequenceSetup.RebellionPath));
                Assert.That(AssetDatabase.DeleteAsset(path), Is.True);
            }
            if (profile != null && !EditorUtility.IsPersistent(profile)) Object.DestroyImmediate(profile);
            if (rebellion != null && !EditorUtility.IsPersistent(rebellion)) Object.DestroyImmediate(rebellion);
            profile = null; rebellion = null;
            if (originalProfile != null && originalProfileJson != null)
                Assert.That(JsonUtility.ToJson(originalProfile), Is.EqualTo(originalProfileJson), "The real dream profile must remain untouched.");
            if (originalRebellion != null && originalRebellionJson != null)
                Assert.That(JsonUtility.ToJson(originalRebellion), Is.EqualTo(originalRebellionJson), "The real rebellion configuration and its Undo history must remain untouched.");
        }

        [Test]
        public void DailyDefaultsAndTheEditorCatalogMatchTheActiveCampaign()
        {
            var defaults = ScriptableObject.CreateInstance<DreamSequenceProfile>();
            try
            {
                Assert.That(defaults.Days.Select(day => day.Day), Is.EqualTo(new[] { 1, 2, 3 }));
                Assert.That(defaults.Days.Select(day => day.Duration), Is.All.EqualTo(20));
                Assert.That(defaults.Days.Select(day => day.Music),
                    Is.EqualTo(new[] { MiniGameSound.Dream0, MiniGameSound.Dream1, MiniGameSound.Dream3 }));
                Assert.That(defaults.GetDay(0), Is.Null, "Day0 skips projection instead of borrowing a formal-day slot.");
                foreach (int day in new[] { 1, 2, 3 }) Assert.That(profile.GetDay(day)?.Artwork, Is.Not.Null);
            }
            finally { Object.DestroyImmediate(defaults); }

            var rows = ChoiceRows();
            var expected = compiled.Rules.Days.SelectMany(day => day.Meetings)
                .SelectMany(meeting => meeting.NormalRounds.Concat(meeting.SpecialRound == null
                    ? Array.Empty<RoundDefinition>() : new[] { meeting.SpecialRound }))
                .SelectMany(round => round.Responses).Select(response => response.Id).Distinct(StringComparer.Ordinal).ToArray();
            var actual = rows.Select(row => ReadField<string>(row, "OptionId")).ToArray();
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.That(actual.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(actual.Length), "UI rows must be keyed by stable choice IDs without duplicates.");
            var special = compiled.Rules.Days.SelectMany(day => day.Meetings).Where(meeting => meeting.SpecialRound != null)
                .SelectMany(meeting => meeting.SpecialRound.Responses).Select(response => response.Id).Distinct(StringComparer.Ordinal).ToArray();
            Assert.That(rows.Where(row => ReadField<bool>(row, "Special")).Select(row => ReadField<string>(row, "OptionId")),
                Is.EquivalentTo(special), "The active campaign may contain no special rounds; the UI must reflect the authored data exactly.");
            foreach (string id in special)
                Assert.That(rows.Any(row => ReadField<string>(row, "OptionId") == id && ReadField<bool>(row, "Special")), Is.True, id);
            var dispositionIds = compiled.SourceNodes.SelectMany(node => node.Options).Where(option => option.Disposition.HasValue)
                .Select(option => option.Id).ToArray();
            Assert.That(actual.Intersect(dispositionIds, StringComparer.Ordinal), Is.Empty, "Disposition buttons are not editable dialogue-answer deltas.");
            Assert.That(rows.All(row => !string.IsNullOrWhiteSpace(ReadField<string>(row, "Text"))), Is.True);
        }

        [Test]
        public void WindowSerializedEditsUndoRedoAndSaveReloadWithoutChangingRealDefaults()
        {
            var rows = ChoiceRows();
            var ids = rows.Select(row => ReadField<string>(row, "OptionId")).Distinct(StringComparer.Ordinal).Take(2).ToArray();
            Assert.That(ids.Length, Is.EqualTo(2));
            string firstId = ids[0], secondId = ids[1];
            var artwork = profile.GetDay(2).Artwork;
            Assert.That(profile.Audio, Is.Not.Null);
            var voice = profile.Audio.Sounds.Where(entry => entry != null).Select(entry => entry.Clip).First(clip => clip != null);
            string artworkGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(artwork));
            string voiceGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(voice));
            string beforeProfile = JsonUtility.ToJson(profile);
            string beforeRebellion = JsonUtility.ToJson(rebellion);
            var settings = ReadField<SerializedObject>(window, "settings");
            var rules = ReadField<SerializedObject>(window, "rules");
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            settings.Update(); rules.Update();
            settings.FindProperty("AlignSeconds").floatValue = 2.25f;
            settings.FindProperty("SeaWaveHeight").floatValue = .41f;
            settings.FindProperty("ShoreWashDistance").floatValue = 2.8f;
            settings.FindProperty("AnimateRoomWater").boolValue = false;
            settings.FindProperty("EnableEndingRetro").boolValue = false;
            settings.FindProperty("PanoramaSeaLevel").floatValue = -5.2f;
            var day = settings.FindProperty("Days").GetArrayElementAtIndex(0);
            day.FindPropertyRelative("Duration").floatValue = 42.5f;
            day.FindPropertyRelative("Music").intValue = (int)MiniGameSound.Dream1;
            day.FindPropertyRelative("Artwork").objectReferenceValue = artwork;
            var line = settings.FindProperty("EndingLines").GetArrayElementAtIndex(0);
            line.FindPropertyRelative("Text").stringValue = "隔离配置的字幕测试。";
            line.FindPropertyRelative("Voice").objectReferenceValue = voice;
            rules.FindProperty("InitialValue").intValue = 11;
            rules.FindProperty("Maximum").intValue = 100;
            rules.FindProperty("Threshold").intValue = 83;
            var choices = rules.FindProperty("Choices");
            choices.arraySize = 2;
            SetChoice(choices.GetArrayElementAtIndex(0), firstId, 17);
            SetChoice(choices.GetArrayElementAtIndex(1), secondId, -9);
            Assert.That(settings.ApplyModifiedProperties(), Is.True);
            Assert.That(rules.ApplyModifiedProperties(), Is.True);
            Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group);
            Assert.That(profile.GetDay(1).Duration, Is.EqualTo(42.5f));
            Assert.That(rebellion.CreateDeltas()[secondId], Is.EqualTo(-9));
            Undo.PerformUndo();
            Assert.That(JsonUtility.ToJson(profile), Is.EqualTo(beforeProfile));
            Assert.That(JsonUtility.ToJson(rebellion), Is.EqualTo(beforeRebellion));
            Undo.PerformRedo();
            Assert.That(profile.GetDay(1).Duration, Is.EqualTo(42.5f));
            Assert.That(rebellion.CreateDeltas()[firstId], Is.EqualTo(17));
            rules.Update();
            rules.FindProperty("Choices").MoveArrayElement(0, 1);
            rules.ApplyModifiedProperties();
            endingRetro.Strength = .37f;
            endingRetro.FilmGrainStrength = .04f;
            EditorUtility.SetDirty(endingRetro);
            Invoke(window, "Save");
            Assert.That(ReadField<string>(window, "error"), Is.Null);
            Assert.That(ReadField<string>(window, "savedMessage"), Does.Contain("配置已保存"));
            CloseWindow();
            Undo.ClearUndo(profile); Undo.ClearUndo(rebellion);
            Resources.UnloadAsset(profile); Resources.UnloadAsset(rebellion);
            Resources.UnloadAsset(endingRetro); endingRetro = null;
            profile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(profilePath);
            rebellion = AssetDatabase.LoadAssetAtPath<PlayerRebellionProfile>(rebellionPath);
            Assert.That(profile.AlignSeconds, Is.EqualTo(2.25f));
            Assert.That(profile.SeaWaveHeight, Is.EqualTo(.41f).Within(.0001));
            Assert.That(profile.ShoreWashDistance, Is.EqualTo(2.8f).Within(.0001));
            Assert.That(profile.AnimateRoomWater, Is.False);
            Assert.That(profile.EnableEndingRetro, Is.False);
            Assert.That(profile.PanoramaSeaLevel, Is.EqualTo(-5.2f).Within(.0001));
            Assert.That(profile.EndingRetro.Strength, Is.EqualTo(.37f).Within(.0001));
            Assert.That(profile.EndingRetro.FilmGrainStrength, Is.EqualTo(.04f).Within(.0001));
            Assert.That(profile.GetDay(1).Duration, Is.EqualTo(42.5f));
            Assert.That(profile.GetDay(1).Music, Is.EqualTo(MiniGameSound.Dream1));
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(profile.GetDay(1).Artwork)), Is.EqualTo(artworkGuid));
            Assert.That(profile.EndingLines[0].Text, Is.EqualTo("隔离配置的字幕测试。"));
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(profile.EndingLines[0].Voice)), Is.EqualTo(voiceGuid));
            Assert.That(rebellion.InitialValue, Is.EqualTo(11));
            Assert.That(rebellion.Threshold, Is.EqualTo(83));
            Assert.That(rebellion.Choices[0].OptionId, Is.EqualTo(secondId));
            var deltas = rebellion.CreateDeltas();
            Assert.That(deltas[firstId], Is.EqualTo(17));
            Assert.That(deltas[secondId], Is.EqualTo(-9), "Reordering rows must not change ID-keyed answer values.");
            Assert.DoesNotThrow(() => compiled.Rules.WithDreamFlow(rebellion.InitialValue, rebellion.Maximum, rebellion.Threshold, deltas));
            OpenIsolatedWindow();
            Assert.That(ReadField<SerializedObject>(window, "settings").FindProperty("AlignSeconds").floatValue, Is.EqualTo(2.25f));
        }

        [Test]
        public void SyntheticSpecialRoundKeepsItsLabelAndStableRebellionKey()
        {
            // The shipped campaign currently has no special rounds; exercise the same row builder
            // with an isolated rules fixture rather than adding fake content to that campaign.
            string normalId = "editor_fixture_normal_" + token;
            string specialId = "editor_fixture_special_" + token;
            const string patientId = "editor_fixture_patient";
            var normal = new RoundDefinition("editor_fixture_normal_node", new[] { new ResponseDefinition(normalId, "normal_branch", 1) });
            var special = new RoundDefinition("editor_fixture_special_node", new[] { new ResponseDefinition(specialId, "special_branch", 2) });
            var labels = new Dictionary<string,string> { [normalId] = "普通回答", [specialId] = "特殊回答" };
            var names = new Dictionary<string,string> { [patientId] = "隔离测试角色" };
            var rows = ChoiceRows();
            var seen = new HashSet<string>(rows.Select(row => ReadField<string>(row, "OptionId")), StringComparer.Ordinal);
            Invoke(window, "AddRound", normal, 1, patientId, false, labels, names, seen);
            Invoke(window, "AddRound", special, 1, patientId, true, labels, names, seen);
            Invoke(window, "AddRound", special, 1, patientId, true, labels, names, seen);
            var added = ChoiceRows();
            Assert.That(added.Length, Is.EqualTo(rows.Length + 2), "Repeated source discovery must not duplicate a stable option ID.");
            var specialRow = added.Single(row => ReadField<string>(row, "OptionId") == specialId);
            Assert.That(ReadField<bool>(specialRow, "Special"), Is.True);
            Assert.That(ReadField<int>(specialRow, "Day"), Is.EqualTo(1));
            Assert.That(ReadField<string>(specialRow, "PatientId"), Is.EqualTo(patientId));
            Assert.That(ReadField<string>(specialRow, "PatientName"), Is.EqualTo(names[patientId]));
            Assert.That(ReadField<string>(specialRow, "Text"), Is.EqualTo(labels[specialId]));
            var serialized = ReadField<SerializedObject>(window, "rules");
            serialized.Update();
            var choices = serialized.FindProperty("Choices");
            choices.arraySize = 2;
            SetChoice(choices.GetArrayElementAtIndex(0), normalId, 13);
            SetChoice(choices.GetArrayElementAtIndex(1), specialId, -21);
            choices.MoveArrayElement(0, 1);
            serialized.ApplyModifiedProperties();
            var patient = new PatientDefinition(patientId, names[patientId], 1, 0, null, PatientPresence.TreatmentEnded);
            var meeting = new MeetingDefinition(patientId, new[] { normal }, special, 1, 0,
                new Dictionary<Disposition,string> { [Disposition.Treat] = "treated" });
            var fixture = new GameRules("editor_special_fixture", new[] { patient }, new[] { new DayDefinition(1, 0, new[] { meeting }) },
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
            var configured = fixture.WithDreamFlow(0, 100, 100, rebellion.CreateDeltas());
            Assert.That(configured.RebellionOptionDeltas[normalId], Is.EqualTo(13));
            Assert.That(configured.RebellionOptionDeltas[specialId], Is.EqualTo(-21));
        }

        private void OpenIsolatedWindow()
        {
            window = ScriptableObject.CreateInstance<DreamSettingsWindow>();
            ReadField<SerializedObject>(window, "settings")?.Dispose();
            ReadField<SerializedObject>(window, "rules")?.Dispose();
            SetField(window, "profile", profile); SetField(window, "rebellion", rebellion);
            SetField(window, "settings", new SerializedObject(profile));
            SetField(window, "rules", new SerializedObject(rebellion));
            Invoke(window, "ReloadChoices");
            Assert.That(ReadField<string>(window, "choiceError"), Is.Null);
        }

        private void CloseWindow()
        {
            if (window == null) return;
            Object.DestroyImmediate(window);
            window = null;
        }

        private object[] ChoiceRows() => ((IEnumerable)ReadField<object>(window, "choices")).Cast<object>().ToArray();
        private static void SetChoice(SerializedProperty entry, string id, int delta)
        {
            entry.FindPropertyRelative("OptionId").stringValue = id;
            entry.FindPropertyRelative("Delta").intValue = delta;
        }
        private static T ReadField<T>(object owner, string name) => (T)Field(owner, name).GetValue(owner);
        private static void SetField(object owner, string name, object value) => Field(owner, name).SetValue(owner, value);
        private static FieldInfo Field(object owner, string name) => owner.GetType().GetField(name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new MissingFieldException(owner.GetType().FullName, name);
        private static void Invoke(object owner, string name, params object[] arguments) => (owner.GetType().GetMethod(name,
            BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new MissingMethodException(owner.GetType().FullName, name)).Invoke(owner, arguments);
    }
}

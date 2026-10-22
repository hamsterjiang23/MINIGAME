using System;
using System.IO;
using System.Reflection;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DayNightRetroToolsTests
    {
        private string folder;
        private Scene previous, scene;
        private RoomLightingController room;
        private RetroScreenEffect effect;
        private RetroScreenProfile day, night;
        private RoomLightingProfile dayLights, nightLights;
        private bool savedHarness;

        [SetUp]
        public void CreateRoom()
        {
            folder = "Assets/DayNightRetroTest-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            previous = SceneManager.GetActiveScene();
            savedHarness = string.IsNullOrEmpty(previous.path);
            if (savedHarness) EditorSceneManager.SaveScene(previous, folder + "/Harness.unity");
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Grading test room"); SceneManager.MoveGameObjectToScene(root, scene);
            var camera = new GameObject("Camera", typeof(Camera)); camera.transform.SetParent(root.transform);
            effect = camera.AddComponent<RetroScreenEffect>();
            day = NewStyle("Day"); night = NewStyle("Night");
            night.ColorGradingEnabled = true; night.PostExposure = -.75f; night.PixelSize = 4;
            dayLights = ScriptableObject.CreateInstance<RoomLightingProfile>(); dayLights.ScreenStyle = day;
            nightLights = ScriptableObject.CreateInstance<RoomLightingProfile>(); nightLights.ScreenStyle = night;
            AssetDatabase.CreateAsset(dayLights, folder + "/DayLights.asset");
            AssetDatabase.CreateAsset(nightLights, folder + "/NightLights.asset");
            room = root.AddComponent<RoomLightingController>(); room.ConfigureProfiles(dayLights, nightLights);
            effect.Configure(day, AssetDatabase.LoadAssetAtPath<Shader>(RetroScreenSetup.ShaderPath)); effect.BindLighting(room);
            effect.FollowRoomLighting = true;
        }

        [TearDown]
        public void Cleanup()
        {
            var preview = typeof(RetroScreenSetup).Assembly.GetType("MiniGame.UnityHost.Editor.RetroScreenPreview");
            preview.GetProperty("TargetRoom").SetValue(null, null);
            RoomLightingAuthoring.ClearEditingPeriod(room);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        private RetroScreenProfile NewStyle(string name)
        {
            var style = ScriptableObject.CreateInstance<RetroScreenProfile>(); style.ResetToNaturalDefaults();
            style.name = name; AssetDatabase.CreateAsset(style, folder + "/" + name + ".asset"); return style;
        }

        [Test]
        public void DayAndNightSelectDifferentRenderedPixelsAndEditsStayIndependent()
        {
            string dayBefore = JsonUtility.ToJson(day);
            room.SetPreviewMode(RoomLightingPreviewMode.Day); var daylight = Render();
            Assert.That(effect.ActiveProfile, Is.SameAs(day));
            room.SetPreviewMode(RoomLightingPreviewMode.Night); var nightlight = Render();
            Assert.That(effect.ActiveProfile, Is.SameAs(night));
            Assert.That(nightlight, Is.Not.EqualTo(daylight));
            night.PixelSize = 8; night.PostExposure = -1.5f;
            Assert.That(Render(), Is.Not.EqualTo(nightlight));
            room.SetPreviewMode(RoomLightingPreviewMode.Day);
            Assert.That(Render(), Is.EqualTo(daylight));
            Assert.That(JsonUtility.ToJson(day), Is.EqualTo(dayBefore));
            room.SetPreviewMode(RoomLightingPreviewMode.Automatic);
            Assert.That(effect.ActiveProfile, Is.SameAs(day));
        }

        [Test]
        public void DarkAndBrightControlsChangeTheirOwnLuminanceRanges()
        {
            room.SetPreviewMode(RoomLightingPreviewMode.Day);
            day.ColorGradingEnabled = true; day.ShadowEnd = .35f; day.HighlightStart = .65f;
            var neutral = Render();
            day.ShadowExposure = 1; var raisedShadows = Render();
            Assert.That(raisedShadows[0].r, Is.GreaterThan(neutral[0].r + .02f));
            Assert.That(raisedShadows[63].r, Is.EqualTo(neutral[63].r).Within(.0001f));
            day.ShadowExposure = 0; day.HighlightExposure = -1; var loweredHighlights = Render();
            Assert.That(loweredHighlights[0].r, Is.EqualTo(neutral[0].r).Within(.0001f));
            Assert.That(loweredHighlights[63].r, Is.LessThan(neutral[63].r - .1f));
            Assert.That(night.ShadowExposure, Is.Zero);
            Assert.That(night.HighlightExposure, Is.Zero);
        }

        [Test]
        public void BindingUndoSaveAndLightingJsonKeepTheOtherPeriodIntact()
        {
            var replacement = NewStyle("ReplacementNight");
            Assert.Throws<ArgumentException>(() => RetroScreenSetup.SetPeriodProfile(room, RoomLightingPeriod.Night, day));
            RetroScreenSetup.SetPeriodProfile(room, RoomLightingPeriod.Night, replacement);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
            Assert.That(nightLights.ScreenStyle, Is.SameAs(night));
            Undo.PerformRedo(); Assert.That(nightLights.ScreenStyle, Is.SameAs(replacement));
            Assert.That(dayLights.ScreenStyle, Is.SameAs(day));
            replacement.ShadowExposure = .75f; EditorUtility.SetDirty(replacement);
            RoomLightingAuthoring.SaveProfile(nightLights);
            Assert.That(File.ReadAllText(AssetDatabase.GetAssetPath(nightLights)),
                Does.Contain(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(replacement))));
            Assert.That(File.ReadAllText(AssetDatabase.GetAssetPath(replacement)), Does.Contain("shadowExposure: 0.75"));
            Assert.That(RoomLightingPresetIO.TryImportJson(RoomLightingPresetIO.ExportJson(dayLights), nightLights, out string message), Is.True, message);
            Assert.That(nightLights.ScreenStyle, Is.SameAs(replacement), "Lighting JSON must retain the separate grading binding.");
        }

        [Test]
        public void MigrationClonesTheCurrentStyleAndIsIdempotent()
        {
            nightLights.ScreenStyle = null;
            RetroScreenSetup.EnsureRoomProfiles(room);
            var created = nightLights.ScreenStyle;
            Assert.That(created, Is.Not.Null.And.Not.SameAs(day));
            Assert.That(created.PixelSize, Is.EqualTo(day.PixelSize));
            Assert.That(created.ShadowTint, Is.EqualTo(day.ShadowTint));
            Assert.That(created.PostExposure, Is.EqualTo(day.PostExposure));
            RetroScreenSetup.EnsureRoomProfiles(room);
            Assert.That(nightLights.ScreenStyle, Is.SameAs(created));
        }

        [Test]
        public void DisabledEndpointCannotIntroduceAnEffectOnlyDuringTheBlend()
        {
            day.ResetToNaturalDefaults(); day.NeutralTonemapping = true; // stale switch on a disabled grading block
            night.ResetToLightDefaults(); night.EffectEnabled = false;
            var blend = ScriptableObject.CreateInstance<RetroScreenProfile>();
            try
            {
                var method = typeof(RetroScreenProfile).GetMethod("BlendFrom", BindingFlags.NonPublic | BindingFlags.Instance);
                effect.FollowRoomLighting = false; effect.Profile = day; var expected = Render();
                foreach (float amount in new[] { .1f, .5f, .9f })
                {
                    method.Invoke(blend, new object[] { day, night, amount }); effect.Profile = blend;
                    Assert.That(Render(), Is.EqualTo(expected));
                }
            }
            finally { Object.DestroyImmediate(blend); }
        }

        [Test]
        public void ScenePreviewHonorsPinnedRoomAndOutlineUsesNightPixelSize()
        {
            var camera = effect.GetComponent<Camera>(); camera.cameraType = CameraType.SceneView;
            var preview = typeof(RetroScreenSetup).Assembly.GetType("MiniGame.UnityHost.Editor.RetroScreenPreview");
            preview.GetProperty("TargetRoom").SetValue(null, room);
            var other = new GameObject("Other selected room", typeof(RoomLightingController));
            Selection.activeObject = other; effect.BindLighting(null);
            preview.GetMethod("SynchronizeSceneCamera", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { camera });
            Assert.That(effect.LightingSource, Is.SameAs(room));
            room.SetPreviewMode(RoomLightingPreviewMode.Night); night.PixelSize = 8;
            var outline = typeof(RetroScreenEffect).Assembly.GetType("MiniGame.UnityHost.InteractionFocusOutline");
            var pixelSize = outline.GetMethod("ActivePixelSize", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(pixelSize.Invoke(null, new object[] { camera }), Is.EqualTo(8));
            effect.PresentationBypass = true;
            Assert.That(pixelSize.Invoke(null, new object[] { camera }), Is.EqualTo(1));
        }

        private Color[] Render()
        {
            var input = new Texture2D(64, 1, TextureFormat.RGBAFloat, false, true);
            var source = RenderTexture.GetTemporary(64, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var output = RenderTexture.GetTemporary(64, 1, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(64, 1, TextureFormat.RGBAFloat, false, true);
            var active = RenderTexture.active;
            try
            {
                var colors = new Color[64];
                for (int i = 0; i < 64; i++) { float value = .1f + .8f * i / 63; colors[i] = new Color(value, value, value, 1); }
                input.SetPixels(colors); input.Apply(); Graphics.Blit(input, source); effect.Render(source, output);
                RenderTexture.active = output; readback.ReadPixels(new Rect(0, 0, 64, 1), 0, 0); readback.Apply();
                return readback.GetPixels();
            }
            finally
            {
                RenderTexture.active = active; Object.DestroyImmediate(input); Object.DestroyImmediate(readback);
                RenderTexture.ReleaseTemporary(source); RenderTexture.ReleaseTemporary(output);
            }
        }
    }
}

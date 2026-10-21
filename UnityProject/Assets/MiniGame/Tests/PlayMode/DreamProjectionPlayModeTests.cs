using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamProjectionPlayModeTests
    {
        private readonly List<Object> cleanup = new List<Object>();
        private DreamSequenceDirector director;
        private DreamSequenceProfile profile;
        private Camera camera;
        private Renderer wall, projection;
        private Transform viewingPose;

        [TearDown]
        public void TearDown()
        {
            if (director != null) director.PrepareForRuntimeChange();
            for (int index = cleanup.Count - 1; index >= 0; index--)
                if (cleanup[index] != null) Object.DestroyImmediate(cleanup[index]);
            cleanup.Clear();
        }

        [UnityTest]
        public IEnumerator ThreeDreamsHaveDeterministicMovingPixelsAndAnOffscreenOnlyCamera()
        {
            for (int day = 1; day <= 3; day++)
            {
                using (var stage = new DreamAnimationStage(640, 360))
                {
                    Assert.That(stage.StageCamera.enabled, Is.False);
                    Assert.That(stage.StageCamera.targetTexture, Is.SameAs(stage.Texture));
                    Assert.That(stage.StageCamera.GetComponent<AudioListener>(), Is.Null);
                    Assert.That(stage.StageCamera.cullingMask, Is.EqualTo(1 << 29));
                    var artwork = Artwork(day);
                    stage.Render(day, artwork, 0, 20);
                    var first = Capture(stage.Texture);
                    stage.Render(day, artwork, 7.3f, 20);
                    var moved = Capture(stage.Texture);
                    float difference = Difference(first, moved);
                    Assert.That(difference, Is.GreaterThan(.002f), "Day " + day + " must animate rendered content, not only its playback counter.");
                    stage.Render(day, artwork, 7.3f, 20);
                    Assert.That(Capture(stage.Texture), Is.EqualTo(moved), "Explicit animation time must recreate the same frame for pause/load.");
                    stage.Render(day, artwork, 0, 20, 0);
                    var still = Capture(stage.Texture);
                    stage.Render(day, artwork, 7.3f, 20, 0);
                    Assert.That(Capture(stage.Texture), Is.EqualTo(still), "Zero motion must suppress all time-driven visual layers.");
                    Debug.Log("MINIGAME_DREAM_PIXEL_CHANGE day=" + day + " meanDifference=" + difference.ToString("F6"));
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator PreviewFramesWallPausesAndRestoresCameraAndProjection()
        {
            CreateRig();
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            float fov = camera.fieldOfView;
            var originalMaterial = projection.sharedMaterial;
            Assert.That(director.PreviewDay(1), Is.True);
            Assert.That(director.Phase, Is.EqualTo(DreamPlaybackPhase.Aligning));
            Assert.That(projection.enabled, Is.False, "Projection starts only after alignment.");
            director.Advance(profile.AlignSeconds);
            Assert.That(director.IsPlaying, Is.True);
            Assert.That(director.StageTexture.width, Is.EqualTo(1920));
            Assert.That(director.StageTexture.height, Is.EqualTo(1080));
            director.Advance(2);
            Assert.That(camera.orthographic, Is.False);
            Assert.That(Mathf.Abs(Vector3.Dot(camera.transform.forward, wall.transform.forward)), Is.GreaterThan(.999f));
            foreach (var corner in new[] { new Vector3(-.5f,-.5f,0), new Vector3(.5f,-.5f,0), new Vector3(.5f,.5f,0), new Vector3(-.5f,.5f,0) })
            {
                var viewport = camera.WorldToViewportPoint(projection.transform.TransformPoint(corner));
                Assert.That(viewport.z, Is.GreaterThan(camera.nearClipPlane));
                Assert.That(viewport.x, Is.InRange(.01f,.99f));
                Assert.That(viewport.y, Is.InRange(.01f,.99f));
            }
            director.PreviewPaused = true;
            float seconds = director.PlaybackSeconds;
            var pausedPixels = Capture(director.StageTexture);
            director.Advance(5);
            yield return null;
            Assert.That(director.PlaybackSeconds, Is.EqualTo(seconds));
            Assert.That(Capture(director.StageTexture), Is.EqualTo(pausedPixels));
            director.StopPreview();
            Assert.That(director.OwnsCamera, Is.False);
            Assert.That(Vector3.Distance(camera.transform.position, position), Is.LessThan(.00001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, rotation), Is.LessThan(.001f));
            Assert.That(camera.fieldOfView, Is.EqualTo(fov));
            Assert.That(projection.enabled, Is.False);
            Assert.That(projection.sharedMaterial, Is.SameAs(originalMaterial));
            Assert.That(director.StageTexture, Is.Null);
        }

        [UnityTest]
        public IEnumerator RuntimeCheckpointKeepsPlaybackTimeAndCompletesOnlyAfterReturn()
        {
            CreateRig();
            var rules = Rules();
            var content = new FixtureContent();
            var runtime = new GameRuntime(rules, content, true);
            ReachFirstDream(runtime);
            Vector3 originalPosition = camera.transform.position;
            Quaternion originalRotation = camera.transform.rotation;
            director.Bind(runtime, null, null, null, profile);
            director.Advance(profile.AlignSeconds);
            director.Advance(6.25f);
            Assert.That(runtime.SetPaused(true).Accepted, Is.True);
            var checkpoint = director.CaptureCheckpoint();
            Assert.That(checkpoint.Phase, Is.EqualTo(DreamPlaybackPhase.Playing));
            var frame = Capture(director.StageTexture);
            string oldToken = runtime.GetView().Cue.Token;
            var restored = GameRuntime.Restore(runtime.Save(), rules, content);
            director.Bind(restored, null, null, null, profile);
            director.RestoreCheckpoint(checkpoint);
            Assert.That(director.PlaybackSeconds, Is.EqualTo(6.25f));
            Assert.That(Capture(director.StageTexture), Is.EqualTo(frame));
            director.Advance(10);
            Assert.That(director.PlaybackSeconds, Is.EqualTo(6.25f));
            Assert.That(restored.SetPaused(false).Accepted, Is.True);
            Assert.That(restored.CompleteCue(oldToken).Accepted, Is.False);
            director.Advance(20);
            Assert.That(director.Phase, Is.EqualTo(DreamPlaybackPhase.Returning));
            Assert.That(restored.GetView().Game.Phase, Is.EqualTo(GamePhase.Dream));
            director.Advance(profile.ReturnSeconds);
            Assert.That(restored.GetView().Game.Phase, Is.EqualTo(GamePhase.AfterDream));
            Assert.That(director.OwnsCamera, Is.False);
            Assert.That(Vector3.Distance(camera.transform.position, originalPosition), Is.LessThan(.00001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation, originalRotation), Is.LessThan(.001f));
            yield return null;
        }

        private void CreateRig()
        {
            var origin = new Vector3(200, 200, 200);
            wall = Track(GameObject.CreatePrimitive(PrimitiveType.Cube)).GetComponent<Renderer>();
            wall.transform.position = origin + Vector3.up;
            wall.transform.localScale = new Vector3(10, 3.12f, .2f);
            projection = Track(GameObject.CreatePrimitive(PrimitiveType.Quad)).GetComponent<Renderer>();
            projection.transform.position = origin + new Vector3(0, 1, -.11f);
            projection.transform.localScale = new Vector3(5, 5 * 9f / 16f, 1);
            projection.enabled = false;
            var cameraObject = Track(new GameObject("Dream test camera", typeof(Camera)));
            camera = cameraObject.GetComponent<Camera>();
            camera.enabled = false;
            camera.aspect = 16f / 9f;
            camera.transform.SetPositionAndRotation(origin + new Vector3(.7f, 1.7f, -5), Quaternion.Euler(4, 8, 0));
            camera.fieldOfView = 61;
            viewingPose = Track(new GameObject("Dream test viewing pose")).transform;
            viewingPose.position = origin + new Vector3(0, 1, -4);
            profile = Track(ScriptableObject.CreateInstance<DreamSequenceProfile>());
            foreach (var day in profile.Days)
            {
                day.Artwork = Artwork(day.Day);
                var production = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault").GetDay(day.Day);
                day.Frames = production.Frames;
                day.CrossfadeSeconds = production.CrossfadeSeconds;
                day.PostIntensity = production.PostIntensity;
            }
            director = Track(new GameObject("Dream test director")).AddComponent<DreamSequenceDirector>();
            director.Configure(wall, viewingPose, projection, camera, profile);
        }

        private static Texture2D Artwork(int day)
        {
#if UNITY_EDITOR
            var names = new[] { "Day1_WhaleBeach", "Day2_EndlessRoad", "Day3_BoundJudgment" };
            var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MiniGame/Art/Dreams/" + names[day - 1] + ".png");
            Assert.That(texture, Is.Not.Null);
            return texture;
#else
            return Texture2D.grayTexture;
#endif
        }

        private static Color32[] Capture(RenderTexture target)
        {
            var previous = RenderTexture.active;
            var pixels = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); pixels.Apply();
                return pixels.GetPixels32();
            }
            finally { RenderTexture.active = previous; Object.DestroyImmediate(pixels); }
        }

        private static float Difference(Color32[] first, Color32[] second)
        {
            double sum = 0;
            for (int index = 0; index < first.Length; index++)
                sum += Mathf.Abs(first[index].r-second[index].r) + Mathf.Abs(first[index].g-second[index].g) + Mathf.Abs(first[index].b-second[index].b);
            return (float)(sum / (first.Length * 3 * 255));
        }

        private static GameRules Rules()
        {
            var patient = new PatientDefinition("patient", "测试病人", 0, 0, null, PatientPresence.TreatmentEnded);
            return new GameRules("dream-projection-fixture", new[] { patient }, Enumerable.Range(0, 4).Select(day =>
                new DayDefinition(day, 0, new[] { new MeetingDefinition(patient.Id,
                    new[] { new RoundDefinition("day" + day, new[] { new ResponseDefinition("pick", "branch", 1) }) },
                    null, 100, 0, new Dictionary<Disposition,string> { [Disposition.Treat] = "treated" }) })),
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway })
                .WithDreamFlow(0, 100, 100, new Dictionary<string,int>());
        }

        private static void ReachFirstDream(GameRuntime runtime)
        {
            for (int step = 0; step < 100; step++)
            {
                var view = runtime.GetView();
                if (view.Game.Day == 1 && view.Game.Phase == GamePhase.Dream) return;
                RuntimeResult result;
                if (view.Cue != null) result = runtime.CompleteCue(view.Cue.Token);
                else if (view.TerminalPage == TerminalPage.Archive) result = runtime.LeaveTerminal();
                else if (view.TerminalPage == TerminalPage.Directory) result = runtime.EnterSelectedPatient(view.Game.CurrentPatientId);
                else if (view.Input == InputContext.Dialogue) result = view.Game.EncounterStage == EncounterStage.AwaitingDisposition
                    ? runtime.SubmitDisposition(view.Game.CurrentEncounterId, Disposition.Treat)
                    : runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, "pick");
                else if (view.Game.Phase == GamePhase.Evening) result = runtime.BeginNightDream();
                else if (view.Game.Phase == GamePhase.MorningInspection) result = runtime.TryInteract(InteractionTarget.Door);
                else if (view.Game.Phase == GamePhase.AfterDream) result = runtime.TryInteract(InteractionTarget.Bed);
                else result = runtime.TryInteract(InteractionTarget.Computer);
                Assert.That(result.Accepted, Is.True, "Fixture could not advance from " + view.Game.Phase);
            }
            Assert.Fail("The bounded fixture did not reach Day1 Dream.");
        }

        private T Track<T>(T value) where T : Object { cleanup.Add(value); return value; }
        private sealed class FixtureContent : IGameContent
        {
            public string Revision => "dream-projection-content";
            public DialogueContent GetDialogue(PlayerView view) => new DialogueContent(view.CurrentNodeId, "测试病人", "测试对白",
                view.AvailableResponseIds.Select(id => new ResponseContent(id, "回答")));
            public string GetPresentation(PresentationKind kind, PlayerView view) => kind.ToString();
            public InspectionContent GetInspection(string patientId, int day, InspectionKind kind) => new InspectionContent(kind,"观察","记录");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class PatientSurveillancePlayModeTests
    {
        private GameObject fixture;
        private PatientSurveillanceFeed feed;
        private PatientMonitorProfile profile;
        private RawImage image;
        private Sprite firstSprite, secondSprite;
        private Texture2D firstTexture, secondTexture;
        private float previousTimeScale;

        [UnitySetUp]
        public IEnumerator CreateIsolatedMonitor()
        {
            previousTimeScale = Time.timeScale;
            fixture = new GameObject("Patient surveillance test fixture");
            var canvasObject = new GameObject("Isolated monitor canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(fixture.transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var imageObject = new GameObject("Monitor display", typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(canvasObject.transform, false);
            image = imageObject.GetComponent<RawImage>();
            profile = ScriptableObject.CreateInstance<PatientMonitorProfile>();
            Assert.That(profile.Rooms.Length, Is.EqualTo(6), "The campaign profile must include all five patients and the tutorial.");
            profile.Rooms = profile.Rooms.Take(2).ToArray();
            profile.Width = 346;
            profile.Height = 140;
            profile.FramesPerSecond = 10;
            firstSprite = MakeSprite(new Color32(133, 159, 144, 255), out firstTexture);
            secondSprite = MakeSprite(new Color32(157, 125, 103, 255), out secondTexture);
            feed = fixture.AddComponent<PatientSurveillanceFeed>();
            feed.UseExternalClock = true;
            feed.Initialize(image, profile);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator DestroyOnlyFixtureOwnedObjects()
        {
            Time.timeScale = previousTimeScale;
            if (fixture != null) Object.Destroy(fixture);
            yield return null;
            yield return null;
            if (firstSprite != null) Object.Destroy(firstSprite);
            if (secondSprite != null) Object.Destroy(secondSprite);
            if (firstTexture != null) Object.Destroy(firstTexture);
            if (secondTexture != null) Object.Destroy(secondTexture);
            if (profile != null) Object.Destroy(profile);
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator SwitchingPatientsUsesDistinctOccupiedRoomsAndRendersRealPixels()
        {
            feed.Show(profile.Rooms[0].PatientId, firstSprite, false);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            var firstRoom = feed.ActiveRoom;
            Transform stage = firstRoom.transform.parent;
            Assert.That(stage.GetComponentsInChildren<PatientMonitorRoom>(true).Count(room => room.gameObject.activeInHierarchy), Is.EqualTo(1));
            foreach (Transform child in firstRoom.GetComponentsInChildren<Transform>(true))
                Assert.That(child.gameObject.layer, Is.EqualTo(PatientSurveillanceFeed.StageLayer), child.name);
            AssertVariedNonBlackPixels(feed.Output);

            Vector3 wideCameraPosition = feed.ViewCamera.transform.position;
            feed.SetCloseUp(true);
            Assert.That(feed.IsCloseUp, Is.True);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            Assert.That(feed.ActiveRoom, Is.SameAs(firstRoom));
            Assert.That(Vector3.Distance(feed.ViewCamera.transform.position, wideCameraPosition), Is.GreaterThan(.1f),
                "The inspection view must move the room camera closer while preserving the selected patient.");
            AssertVariedNonBlackPixels(feed.Output);

            feed.Show(profile.Rooms[1].PatientId, secondSprite, false);
            AssertLiveFeed(secondSprite, profile.Rooms[1].PatientId);
            Assert.That(feed.IsCloseUp, Is.False, "Selecting another patient must restore the room overview.");
            Assert.That(feed.ActiveRoom, Is.Not.SameAs(firstRoom));
            Assert.That(firstRoom.gameObject.activeInHierarchy, Is.False);
            Assert.That(Vector3.Distance(firstRoom.transform.position, feed.ActiveRoom.transform.position), Is.GreaterThan(10f));
            Assert.That(stage.GetComponentsInChildren<PatientMonitorRoom>(true).Count(room => room.gameObject.activeInHierarchy), Is.EqualTo(1));
            Assert.That(stage.GetComponentInChildren<SpriteRenderer>(true).sprite, Is.SameAs(secondSprite));
            AssertVariedNonBlackPixels(feed.Output);
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator ExternalClockLimitsFramesAndPauseAndDisableSuspendPresentation()
        {
            feed.Show(profile.Rooms[0].PatientId, firstSprite, false);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            int initialFrames = feed.RenderedFrames;
            var actor = feed.ActiveRoom.transform.parent.GetComponentInChildren<SpriteRenderer>(true);
            Vector3 plantedFeet = actor.transform.position;
            feed.Advance(.04f);
            feed.Advance(.04f);
            Assert.That(feed.RenderedFrames, Is.EqualTo(initialFrames), "An 80 ms interval must not render another 10 Hz frame.");
            feed.Advance(.021f);
            Assert.That(feed.RenderedFrames, Is.EqualTo(initialFrames + 1));
            Assert.That(feed.PresentationTime, Is.EqualTo(.101f).Within(.0001f));
            feed.Advance(.31f);
            Assert.That(Vector3.Distance(actor.transform.position, plantedFeet), Is.LessThan(.0001f),
                "Breathing must keep the foot registration planted instead of moving the cutout through the floor.");
            Assert.That(feed.RenderedFrames, Is.EqualTo(initialFrames + 2), "A delayed update should render one current frame, without a catch-up render burst.");

            Time.timeScale = 0;
            feed.Show(profile.Rooms[0].PatientId, firstSprite, true);
            float frozenTime = feed.PresentationTime;
            int frozenFrames = feed.RenderedFrames;
            ulong frozenPixels = FrameChecksum(feed.Output);
            float frozenRoomTime = feed.ActiveRoom.AnimationTime;
            feed.Advance(1f);
            yield return null;
            Assert.That(feed.PresentationTime, Is.EqualTo(frozenTime));
            Assert.That(feed.ActiveRoom.AnimationTime, Is.EqualTo(frozenRoomTime));
            Assert.That(feed.RenderedFrames, Is.EqualTo(frozenFrames));
            Assert.That(FrameChecksum(feed.Output), Is.EqualTo(frozenPixels), "Paused footage must remain pixel-identical.");

            RenderTexture frozenOutput = feed.Output;
            profile.Noise = .06f;
            profile.Rooms[0].Grime = .6f;
            profile.MarkChanged();
            feed.Advance(0f);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            Assert.That(feed.Output, Is.Not.SameAs(frozenOutput), "A changed profile must replace its off-screen render target.");
            Assert.That(feed.RenderedFrames, Is.EqualTo(frozenFrames + 1),
                "A paused configuration rebuild must populate its new target exactly once.");
            Assert.That(feed.PresentationTime, Is.EqualTo(frozenTime));
            Assert.That(feed.ActiveRoom.AnimationTime, Is.EqualTo(frozenRoomTime));
            AssertVariedNonBlackPixels(feed.Output);
            frozenFrames = feed.RenderedFrames;
            frozenPixels = FrameChecksum(feed.Output);
            feed.Advance(0f);
            feed.Advance(1f);
            Assert.That(feed.RenderedFrames, Is.EqualTo(frozenFrames), "A rebuilt paused feed must return to its frozen state.");
            Assert.That(feed.PresentationTime, Is.EqualTo(frozenTime));
            Assert.That(feed.ActiveRoom.AnimationTime, Is.EqualTo(frozenRoomTime));
            Assert.That(FrameChecksum(feed.Output), Is.EqualTo(frozenPixels));

            feed.enabled = false;
            Assert.That(image.enabled, Is.False, "A disabled feed must not keep displaying the previous patient's frame.");
            Assert.That(feed.ActiveRoom.gameObject.activeInHierarchy, Is.False);
            feed.Advance(1f);
            Assert.That(feed.PresentationTime, Is.EqualTo(frozenTime));
            Assert.That(feed.RenderedFrames, Is.EqualTo(frozenFrames));
            feed.enabled = true;
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            Assert.That(feed.PresentationTime, Is.EqualTo(frozenTime));
            Assert.That(feed.ActiveRoom.gameObject.activeInHierarchy, Is.True);
            feed.Show(profile.Rooms[0].PatientId, firstSprite, false);
            feed.Advance(.11f);
            Assert.That(feed.PresentationTime, Is.GreaterThan(frozenTime));
            Assert.That(feed.RenderedFrames, Is.GreaterThan(frozenFrames));
            Assert.That(Time.timeScale, Is.Zero, "The monitor must not change the game's pause state.");
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator SaturationControlChangesWarmGrayTintWithoutChangingPatient()
        {
            profile.Saturation=0;profile.MarkChanged();feed.Show(profile.Rooms[0].PatientId,firstSprite,false);
            ulong neutral=FrameChecksum(feed.Output);
            profile.Saturation=1;profile.MarkChanged();feed.Advance(0);
            Assert.That(FrameChecksum(feed.Output),Is.Not.EqualTo(neutral),"The existing artist color control must affect the new warm-gray output.");
            Assert.That(feed.PatientId,Is.EqualTo(profile.Rooms[0].PatientId));
            Assert.That(feed.CurrentSprite,Is.SameAs(firstSprite));
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator MissingPortraitUnknownPatientAndDisabledProfileNeverExposeStaleFootage()
        {
            feed.Show(profile.Rooms[0].PatientId, firstSprite, false);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            feed.Show(profile.Rooms[0].PatientId, null, false);
            AssertHiddenWithoutRendering();
            Assert.That(feed.CurrentSprite, Is.Null);
            feed.Show(profile.Rooms[1].PatientId, secondSprite, false);
            AssertLiveFeed(secondSprite, profile.Rooms[1].PatientId);
            feed.Show("no-such-patient", firstSprite, false);
            AssertHiddenWithoutRendering();
            feed.Show(null, null, false);
            AssertHiddenWithoutRendering();

            feed.Show(profile.Rooms[0].PatientId, firstSprite, false);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            profile.Enabled = false;
            feed.Advance(.2f);
            AssertHiddenWithoutRendering();
            profile.Enabled = true;
            feed.Show(profile.Rooms[1].PatientId, secondSprite, false);
            AssertLiveFeed(secondSprite, profile.Rooms[1].PatientId);
            yield return null;
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator DestroyReleasesStageTexturesMeshesAndMaterialsWithoutChangingHostRendering()
        {
            Camera hostCamera = Camera.main;
            int hostMask = hostCamera != null ? hostCamera.cullingMask : 0;
            RenderTexture hostTarget = hostCamera != null ? hostCamera.targetTexture : null;
            CameraClearFlags hostClear = hostCamera != null ? hostCamera.clearFlags : CameraClearFlags.Skybox;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientLight = RenderSettings.ambientLight;
            bool fog = RenderSettings.fog;
            Material skybox = RenderSettings.skybox;
            RenderTexture previouslyActive = RenderTexture.active;
            var previousMaterialIds = new HashSet<int>(Resources.FindObjectsOfTypeAll<Material>().Select(value => value.GetInstanceID()));

            feed.Show(profile.Rooms[0].PatientId, firstSprite, false);
            AssertLiveFeed(firstSprite, profile.Rooms[0].PatientId);
            GameObject stage = feed.ActiveRoom.transform.parent.gameObject;
            Camera ownedCamera = feed.ViewCamera;
            RenderTexture ownedSource = ownedCamera.targetTexture;
            RenderTexture ownedOutput = feed.Output;
            Mesh[] ownedMeshes = stage.GetComponentsInChildren<MeshFilter>(true).Select(value => value.sharedMesh).Distinct().ToArray();
            Material[] ownedMaterials = Resources.FindObjectsOfTypeAll<Material>()
                .Where(value => !previousMaterialIds.Contains(value.GetInstanceID()) && value.shader != null
                    && value.shader.name.Contains("Patient Monitor")).ToArray();
            Assert.That(ownedMaterials.Any(value => value.shader.name.EndsWith("Patient Monitor Signal")), Is.True,
                "The post-process material must be included in the resource-lifetime check.");
            Assert.That(stage.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(stage.GetComponentsInChildren<AudioListener>(true), Is.Empty);

            Object.Destroy(fixture);
            yield return null;
            yield return null;
            Assert.That(stage == null, Is.True, "The unparented off-screen stage must not survive its controller.");
            Assert.That(ownedCamera == null, Is.True);
            Assert.That(ownedSource == null, Is.True);
            Assert.That(ownedOutput == null, Is.True);
            foreach (Mesh mesh in ownedMeshes) Assert.That(mesh == null, Is.True, "An owned room mesh leaked.");
            foreach (Material material in ownedMaterials) Assert.That(material == null, Is.True, "An owned room/actor/signal material leaked.");
            Assert.That(RenderTexture.active, Is.SameAs(previouslyActive));
            Assert.That(RenderSettings.ambientMode, Is.EqualTo(ambientMode));
            Assert.That(RenderSettings.ambientLight, Is.EqualTo(ambientLight));
            Assert.That(RenderSettings.fog, Is.EqualTo(fog));
            Assert.That(RenderSettings.skybox, Is.SameAs(skybox));
            if (hostCamera != null)
            {
                Assert.That(Camera.main, Is.SameAs(hostCamera));
                Assert.That(hostCamera.cullingMask, Is.EqualTo(hostMask));
                Assert.That(hostCamera.targetTexture, Is.SameAs(hostTarget));
                Assert.That(hostCamera.clearFlags, Is.EqualTo(hostClear));
            }
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator WardBackdropSurvivesExpressionChangesAndReleasesItsOwnedSprite()
        {
            var plate = new Texture2D(2, 2);
            plate.SetPixels(new[] { Color.gray, Color.gray, Color.gray, Color.gray });
            plate.Apply();
            profile.Backdrop = plate;
            profile.Noise = 0; profile.IdleMotion = 0; profile.Exposure = 1; profile.Saturation = 1;
            profile.Rooms[0].ActorHeight = 1.2f;
            profile.Rooms[0].ActorOffset = new Vector3(.2f, .1f, 0);
            try
            {
                feed.Show(profile.Rooms[0].PatientId, firstSprite, true);
                Assert.That(feed.ViewCamera.orthographic, Is.True);
                Assert.That(feed.ActiveRoom.gameObject.activeSelf, Is.False, "Room geometry cannot occlude flat artwork.");
                var background = feed.ActiveRoom.transform.parent.Find("Original white ward backdrop").GetComponent<SpriteRenderer>();
                var ownedSprite = background.sprite;
                var actor = feed.ActiveRoom.transform.parent.Find("Seated patient cutout").GetComponent<SpriteRenderer>();
                Assert.That(actor.bounds.size.y, Is.EqualTo(1.2f).Within(.002f));
                Assert.That(actor.bounds.min.y - feed.ActiveRoom.transform.parent.position.y, Is.EqualTo(-.88f).Within(.002f));
                Assert.That(ownedSprite.texture, Is.SameAs(plate));
                var before = ReadPixels(feed.Output);
                feed.Show(profile.Rooms[0].PatientId, secondSprite, true);
                var after = ReadPixels(feed.Output);
                int corner = feed.Output.width * (feed.Output.height / 2) + feed.Output.width * 9 / 10;
                Assert.That(after[corner], Is.EqualTo(before[corner]), "Changing expression must retain the ward background.");
                Assert.That(after[corner].r, Is.GreaterThan(40), "The background must render as visible pixels.");
                Assert.That(after[corner].r, Is.EqualTo(after[corner].b).Within(2), "The white plate must keep neutral color.");
                Assert.That(after.SequenceEqual(before), Is.False, "The selected expression must change rendered pixels.");
                profile.Rooms[0].ActorOffset = Vector3.zero;
                feed.SetCloseUp(true);
                float top = feed.ViewCamera.WorldToViewportPoint(actor.bounds.max).y;
                Assert.That(top, Is.InRange(.95f, 1f), "A close-up must keep the top of the head inside the frame.");
                Object.Destroy(fixture);
                yield return null;
                yield return null;
                Assert.That(ownedSprite == null, Is.True);
                Assert.That(plate != null, Is.True, "The controller owns its sprite, not the shared texture asset.");
            }
            finally { Object.Destroy(plate); }
        }

        [UnityTest, Timeout(30000)]
        public IEnumerator SeatedPosePlacementDoesNotMoveOtherExpressions()
        {
            profile.Backdrop = firstTexture;
            profile.IdleMotion = 0;
            firstSprite.name = "VV04";
            secondSprite.name = "Standing";
            profile.PortraitPlacements = new[] { new PatientMonitorProfile.PortraitPlacement
                { PortraitId = "VV04", HeightMultiplier = .74f, BottomCenter = new Vector2(.625f, .15f) } };
            feed.Show(profile.Rooms[0].PatientId, firstSprite, true);
            var actor = feed.ActiveRoom.transform.parent.Find("Seated patient cutout").GetComponent<SpriteRenderer>();
            var bottom = feed.ViewCamera.WorldToViewportPoint(new Vector3(actor.bounds.center.x, actor.bounds.min.y, actor.bounds.center.z));
            Assert.That(bottom.x, Is.EqualTo(.625f).Within(.002f));
            Assert.That(bottom.y, Is.EqualTo(.15f).Within(.002f));
            Assert.That(actor.bounds.size.y, Is.EqualTo(profile.Rooms[0].ActorHeight * .74f).Within(.002f));
            feed.Show(profile.Rooms[0].PatientId, secondSprite, true);
            bottom = feed.ViewCamera.WorldToViewportPoint(new Vector3(actor.bounds.center.x, actor.bounds.min.y, actor.bounds.center.z));
            Assert.That(bottom.x, Is.EqualTo(.42f).Within(.002f));
            Assert.That(bottom.y, Is.EqualTo(.01f).Within(.002f));
            yield return null;
        }

        private void AssertLiveFeed(Sprite expectedSprite, string expectedId)
        {
            Assert.That(feed.CurrentSprite, Is.SameAs(expectedSprite));
            Assert.That(feed.PatientId, Is.EqualTo(expectedId));
            Assert.That(feed.ActiveRoom, Is.Not.Null);
            Assert.That(feed.ActiveRoom.gameObject.activeInHierarchy, Is.True);
            Assert.That(feed.ViewCamera, Is.Not.Null);
            Assert.That(feed.ViewCamera.enabled, Is.False, "The dedicated camera is manually rendered only.");
            Assert.That(feed.ViewCamera.cullingMask, Is.EqualTo(1 << PatientSurveillanceFeed.StageLayer));
            Assert.That(feed.Output, Is.Not.Null);
            Assert.That(feed.Output.IsCreated(), Is.True);
            Assert.That(feed.Output.width, Is.EqualTo(346));
            Assert.That(feed.Output.height, Is.EqualTo(140));
            Assert.That(image.enabled, Is.True);
            Assert.That(image.texture, Is.SameAs(feed.Output));
            Assert.That(image.raycastTarget, Is.False);
            Assert.That(feed.RenderedFrames, Is.GreaterThan(0));
        }

        private void AssertHiddenWithoutRendering()
        {
            int frames = feed.RenderedFrames;
            Assert.That(image.enabled, Is.False);
            Assert.That(feed.ActiveRoom == null || !feed.ActiveRoom.gameObject.activeInHierarchy, Is.True,
                "A rejected or cleared portrait must also suspend the previous room.");
            feed.Advance(.5f);
            Assert.That(image.enabled, Is.False);
            Assert.That(feed.RenderedFrames, Is.EqualTo(frames));
        }

        private static Sprite MakeSprite(Color32 body, out Texture2D texture)
        {
            const int width = 32, height = 64;
            texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "Synthetic surveillance patient" };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    bool head = (x - 16) * (x - 16) + (y - 51) * (y - 51) < 49;
                    bool torso = x >= 9 && x <= 23 && y >= 20 && y <= 44;
                    bool legs = ((x >= 9 && x <= 14) || (x >= 19 && x <= 23)) && y >= 3 && y < 20;
                    pixels[y * width + x] = head ? new Color32(186, 173, 144, 255)
                        : torso || legs ? body : new Color32(0, 0, 0, 0);
                }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, 0), 64);
        }

        private static Color32[] ReadPixels(RenderTexture source)
        {
            RenderTexture previous = RenderTexture.active;
            var sample = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                sample.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                return sample.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(sample);
            }
        }

        private static void AssertVariedNonBlackPixels(RenderTexture source)
        {
            Color32[] pixels = ReadPixels(source);
            var colors = new HashSet<int>();
            int lit = 0;
            for (int i = 0; i < pixels.Length; i += 13)
            {
                Color32 pixel = pixels[i];
                if (pixel.r + pixel.g + pixel.b > 60) lit++;
                colors.Add((pixel.r / 16) << 8 | (pixel.g / 16) << 4 | pixel.b / 16);
            }
            Assert.That(lit, Is.GreaterThan(200), "The render target must contain a visible room, not black output.");
            Assert.That(colors.Count, Is.GreaterThan(12), "The camera must render varied scene colors, not only its clear color.");
        }

        private static ulong FrameChecksum(RenderTexture source)
        {
            ulong hash = 14695981039346656037UL;
            unchecked
            {
                foreach (Color32 pixel in ReadPixels(source))
                {
                    hash = (hash ^ pixel.r) * 1099511628211UL;
                    hash = (hash ^ pixel.g) * 1099511628211UL;
                    hash = (hash ^ pixel.b) * 1099511628211UL;
                    hash = (hash ^ pixel.a) * 1099511628211UL;
                }
            }
            return hash;
        }
    }
}

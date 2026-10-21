using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DreamImageSequencePlayModeTests
    {
        [UnityTest]
        public IEnumerator FramesAdvanceInOrderCrossfadeAndHoldTheLastImageDeterministically()
        {
            var frames = new[] { Solid(Color.red), Solid(Color.green), Solid(Color.blue) };
            try
            {
                var settings = new DreamDaySettings { Day = 2, Frames = frames, Duration = 6,
                    CrossfadeSeconds = .5f, MotionIntensity = 0, PostIntensity = 0 };
                using (var stage = new DreamAnimationStage(128, 72))
                {
                    stage.Render(settings, 0);
                    var red = Read(stage.Texture);
                    Assert.That(red[36 * 128 + 64].r, Is.GreaterThan(240));
                    stage.Render(settings, 1.75f);
                    Assert.That(stage.CurrentFrameIndex, Is.EqualTo(0));
                    Assert.That(stage.NextFrameIndex, Is.EqualTo(1));
                    Assert.That(stage.FrameBlend, Is.EqualTo(.5f).Within(.001f));
                    var blended = Read(stage.Texture);
                    var sample = blended[36 * 128 + 64];
                    Assert.That(sample.r, Is.GreaterThan(90));
                    Assert.That(sample.g, Is.GreaterThan(90));
                    Assert.That(sample.b, Is.LessThan(10));
                    stage.Render(settings, 2);
                    Assert.That(stage.CurrentFrameIndex, Is.EqualTo(1));
                    Assert.That(Read(stage.Texture)[36 * 128 + 64].g, Is.GreaterThan(240));
                    stage.Render(settings, 60);
                    Assert.That(stage.CurrentFrameIndex, Is.EqualTo(2));
                    Assert.That(stage.NextFrameIndex, Is.EqualTo(2));
                    Assert.That(stage.FrameBlend, Is.Zero);
                    Assert.That(Read(stage.Texture)[36 * 128 + 64].b, Is.GreaterThan(240));
                    stage.Render(settings, 1.75f);
                    Assert.That(Read(stage.Texture), Is.EqualTo(blended), "Restoring time must restore the same transition pixels.");
                }
            }
            finally { foreach (var frame in frames) Object.Destroy(frame); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllThreeDaysUseTenOrderedImagesAndReproduciblePostProcessing()
        {
            var profile = Resources.Load<DreamSequenceProfile>("MiniGame/DreamSequenceDefault");
            Assert.That(profile, Is.Not.Null);
            for (int day = 1; day <= 3; day++)
            {
                var settings = JsonUtility.FromJson<DreamDaySettings>(JsonUtility.ToJson(profile.GetDay(day)));
                settings.Frames = profile.GetDay(day).Frames;
                settings.Artwork = profile.GetDay(day).Artwork;
                Assert.That(settings.Frames.Length, Is.EqualTo(10));
                Assert.That(settings.Duration / settings.Frames.Length, Is.EqualTo(1f).Within(.0001f),
                    "The production sequence must advance one image per second.");
                Assert.That(settings.CrossfadeSeconds, Is.LessThan(1f));
                Assert.That(settings.Frames.Select(frame => frame.name),
                    Is.EqualTo(Enumerable.Range(1, 10).Select(index => "Frame" + index.ToString("00"))));
                Assert.That(settings.Frames.All(frame => frame.width >= 1600 && frame.height >= 900), Is.True);
                using (var stage = new DreamAnimationStage(320, 180))
                {
                    for (int index = 0; index < 10; index++)
                    {
                        stage.Render(settings, index);
                        Assert.That(stage.CurrentFrameIndex, Is.EqualTo(index));
                        stage.Render(settings, index + .999f);
                        Assert.That(stage.CurrentFrameIndex, Is.EqualTo(index));
                    }
                    float time = settings.Duration * .66f;
                    stage.Render(settings, time);
                    var processed = Read(stage.Texture);
                    stage.Render(settings, time);
                    Assert.That(Read(stage.Texture), Is.EqualTo(processed));
                    settings.PostIntensity = 0;
                    stage.Render(settings, time);
                    var plain = Read(stage.Texture);
                    long difference = 0;
                    for (int i = 0; i < plain.Length; i++)
                        difference += Mathf.Abs(plain[i].r - processed[i].r) + Mathf.Abs(plain[i].g - processed[i].g)
                            + Mathf.Abs(plain[i].b - processed[i].b);
                    Assert.That(difference / (plain.Length * 3f * 255), Is.GreaterThan(.003f), "Post processing must affect actual pixels for Day " + day);
                }
                yield return null;
            }
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixels(Enumerable.Repeat(color, 16).ToArray()); texture.Apply(); return texture;
        }

        private static Color32[] Read(RenderTexture target)
        {
            var previous = RenderTexture.active;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                return texture.GetPixels32();
            }
            finally { RenderTexture.active = previous; Object.DestroyImmediate(texture); }
        }
    }
}

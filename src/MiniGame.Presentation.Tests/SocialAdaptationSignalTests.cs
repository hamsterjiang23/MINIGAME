using System;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class SocialAdaptationSignalTests
    {
        [TestCase(0d)]
        [TestCase(.15)]
        [TestCase(.5)]
        [TestCase(.64)]
        [TestCase(.65)]
        [TestCase(.8)]
        [TestCase(1d)]
        public void DecorationCannotMoveTheSignalOutsideItsTrustEnvelope(double coordinate)
        {
            var parameters = SocialAdaptationSignal.Parameters(coordinate);
            double maximum = 0;
            for (int index = 0; index < 12000; index++)
            {
                double signal = SocialAdaptationSignal.Sample(index * .023, index * .371 - 200, parameters);
                Assert.That(Math.Abs(signal), Is.LessThanOrEqualTo(1.00000001));
                maximum = Math.Max(maximum, Math.Abs(signal * parameters.Amplitude));
            }
            if (coordinate >= SocialAdaptationSignal.ReleaseCoordinate)
                Assert.That(maximum, Is.LessThanOrEqualTo(1.00000001), "Passed readings remain inside the fixed band during every burst.");
            else Assert.That(maximum, Is.GreaterThan(1), "A failed reading must visibly cross the band.");
        }

        [Test]
        public void LowerTrustIncreasesAmplitudeFrequencyAndAgitationMonotonically()
        {
            var previous = SocialAdaptationSignal.Parameters(0);
            for (int index = 1; index <= 100; index++)
            {
                var next = SocialAdaptationSignal.Parameters(index / 100d);
                Assert.That(next.Amplitude, Is.LessThan(previous.Amplitude));
                Assert.That(next.Frequency, Is.LessThan(previous.Frequency));
                Assert.That(next.NoiseSpeed, Is.LessThan(previous.NoiseSpeed));
                Assert.That(next.Disturbance, Is.LessThan(previous.Disturbance));
                Assert.That(next.Agitation, Is.LessThanOrEqualTo(previous.Agitation));
                previous = next;
            }
            Assert.That(SocialAdaptationSignal.Parameters(.65).Amplitude, Is.EqualTo(1).Within(.000001));
        }
    }
}

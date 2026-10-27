#nullable enable
using System;
using MiniGame.Core;

namespace MiniGame.Presentation
{
    /// <summary>Visual-only signal parameters. Never participates in release adjudication.</summary>
    public readonly struct AdaptationSignalParameters
    {
        public double Amplitude { get; }
        public double Frequency { get; }
        public double Disturbance { get; }
        public double NoiseSpeed { get; }
        public double Agitation { get; }
        internal AdaptationSignalParameters(double amplitude, double frequency, double disturbance, double noiseSpeed, double agitation)
        { Amplitude = amplitude; Frequency = frequency; Disturbance = disturbance; NoiseSpeed = noiseSpeed; Agitation = agitation; }
    }

    public static class SocialAdaptationSignal
    {
        public const double ReleaseCoordinate = .65;
        public static double CoordinateFor(SocialAdaptationReading reading)
            => Clamp(ReleaseCoordinate + reading.RelativeReleaseMargin * .45, 0, 1);

        public static AdaptationSignalParameters Parameters(double coordinate)
        {
            double q = Clamp(coordinate, 0, 1), low = 1 - q;
            double amplitude = q < ReleaseCoordinate ? 1 + 1.45 * (1 - q / ReleaseCoordinate)
                : 1 - .52 * (q - ReleaseCoordinate) / (1 - ReleaseCoordinate);
            return new AdaptationSignalParameters(amplitude, .75 + 4.65 * Math.Pow(low, 1.35),
                .025 + .92 * Math.Pow(low, 1.45), 3 + 61 * Math.Pow(low, 1.4),
                Math.Pow(Clamp((ReleaseCoordinate - q) / ReleaseCoordinate, 0, 1), .85));
        }

        public static double IrregularNoise(double x)
        {
            double cell = Math.Floor(x), fraction = x - cell, blend = fraction * fraction * (3 - 2 * fraction);
            return Random(cell) * (1 - blend) + Random(cell + 1) * blend;
        }
        private static double Random(double value)
        {
            double hash = Math.Sin(value * 127.1 + 311.7) * 43758.5453;
            return (hash - Math.Floor(hash)) * 2 - 1;
        }

        // Bounded by [-1, 1], even during bursts: decoration cannot invalidate a passed reading.
        public static double Sample(double phase, double clock, AdaptationSignalParameters parameters)
        {
            double cluster = .5 + .5 * IrregularNoise(clock * .19 + phase * .27);
            double burst = Math.Pow(cluster, .7);
            double warp = parameters.Agitation * (.6 * Math.Sin(phase * 1.83 + clock * .37)
                + .24 * IrregularNoise(phase * 4.7 - clock * .28));
            double carrier = Math.Sin(phase + warp);
            double pointed = Math.Sign(carrier) * Math.Pow(Math.Abs(carrier), 1 + parameters.Agitation * (1.4 + 4.8 * burst));
            double grain = .5 * IrregularNoise(phase * 9.3 + clock * 1.13)
                + .3 * IrregularNoise(phase * 23.7 - clock * 1.67) + .2 * Math.Sin(phase * 31.1 + clock * 2.17);
            double kickBase = Math.Sin(phase * 3.37 - clock * .61);
            double kick = Math.Sign(kickBase) * Math.Pow(Math.Abs(kickBase), 18);
            double chaos = Clamp(grain * (.65 + 1.3 * burst) + parameters.Agitation * .52 * kick, -1, 1);
            return pointed + parameters.Disturbance * (1 - Math.Abs(pointed)) * chaos;
        }
        private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));
    }
}

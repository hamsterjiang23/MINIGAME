using System;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Native-pixel instrument, composited by the existing terminal CRT camera.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TerminalAdaptationWaveform : MaskableGraphic
    {
        private string encounterId;
        private bool hasReading, paused, meetsThreshold;
        private double target, coordinate, phase, noiseClock;
        public bool HasReading => hasReading;
        public double AnimationClock => noiseClock;
        public double DisplayCoordinate => coordinate;
        public AdaptationSignalParameters SignalParameters => SocialAdaptationSignal.Parameters(coordinate);

        public void Initialize()
        {
            raycastTarget = false;
            color = TerminalView.Ink;
        }

        public void SetReading(SocialAdaptationReading reading, string identity, bool freeze)
        {
            paused = freeze;
            if (reading == null)
            {
                hasReading = false; encounterId = null;
                SetVerticesDirty(); return;
            }
            double next = SocialAdaptationSignal.CoordinateFor(reading);
            bool changedEncounter = !hasReading || encounterId != identity;
            // Snap across the boundary so the graph never disagrees with current core eligibility.
            if (changedEncounter || meetsThreshold != reading.MeetsReleaseThreshold) coordinate = next;
            if (changedEncounter) phase = noiseClock = 0;
            target = next; encounterId = identity; hasReading = true;
            meetsThreshold = reading.MeetsReleaseThreshold;
            SetVerticesDirty();
        }

        private void Update()
        {
            if (!hasReading || paused) return;
            double dt = Math.Min(Time.unscaledDeltaTime, .05);
            coordinate += (target - coordinate) * (1 - Math.Exp(-dt * 5));
            if (Math.Abs(target - coordinate) < .00005) coordinate = target;
            var p = SignalParameters;
            double uneven = 1 + p.Agitation * (.3 * SocialAdaptationSignal.IrregularNoise(noiseClock * .12) + .12 * Math.Sin(noiseClock * .4));
            phase += dt * p.Frequency * Math.PI * 2 * uneven;
            noiseClock += dt * p.NoiseSpeed;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            Rect r = rectTransform.rect;
            if (r.width < 2 || r.height < 2) return;
            float middle = r.center.y, band = r.height * .18f;
            Quad(vertices, new Rect(r.xMin, middle - band, r.width, 2 * band), Tint(.1f));
            Color grid = Tint(.13f), limit = Tint(.6f);
            for (float x = r.xMin; x <= r.xMax; x += 16) Line(vertices, new Vector2(x, r.yMin), new Vector2(x, r.yMax), .45f, grid);
            for (float y = r.yMin + 3; y < r.yMax; y += 8) Line(vertices, new Vector2(r.xMin, y), new Vector2(r.xMax, y), .45f, grid);
            foreach (float y in new[] { middle - band, middle + band })
                for (float x = r.xMin; x < r.xMax; x += 6) Line(vertices, new Vector2(x, y), new Vector2(Mathf.Min(x + 3, r.xMax), y), 1f, limit);
            Line(vertices, new Vector2(r.xMin, middle), new Vector2(r.xMax, middle), .4f, Tint(.22f));
            if (!hasReading) return;
            var p = SignalParameters;
            int count = Mathf.CeilToInt(r.width * 2);
            Vector2 previous = default;
            for (int index = 0; index <= count; index++)
            {
                float x = r.xMin + r.width * index / count;
                double t = (double)index / count * 3.2 - 3.2;
                double sample = SocialAdaptationSignal.Sample(phase + t * p.Frequency * 2 * Math.PI,
                    noiseClock + t * p.NoiseSpeed, p);
                var point = new Vector2(x, middle + (float)(sample * p.Amplitude) * band);
                if (index > 0)
                {
                    Line(vertices, previous, point, 2.2f, Tint(.09f));
                    Line(vertices, previous, point, 1f, color);
                }
                previous = point;
            }
        }

        private Color Tint(float alpha) => new Color(color.r, color.g, color.b, color.a * alpha);
        private static void Quad(VertexHelper vertices, Rect rect, Color tint)
        {
            AddQuad(vertices, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), tint);
        }
        private static void Line(VertexHelper vertices, Vector2 from, Vector2 to, float width, Color tint)
        {
            var direction = to - from;
            if (direction.sqrMagnitude < .000001f) return;
            var normal = new Vector2(-direction.y, direction.x).normalized * (width * .5f);
            AddQuad(vertices, from - normal, from + normal, to + normal, to - normal, tint);
        }
        private static void AddQuad(VertexHelper vertices, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            int start = vertices.currentVertCount;
            vertices.AddVert(a, tint, Vector2.zero); vertices.AddVert(b, tint, Vector2.zero);
            vertices.AddVert(c, tint, Vector2.zero); vertices.AddVert(d, tint, Vector2.zero);
            vertices.AddTriangle(start, start + 1, start + 2); vertices.AddTriangle(start, start + 2, start + 3);
        }
    }
}

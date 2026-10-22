using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Sparse glyph-only offsets. Never changes text, layout, transcript data or Unity's random state.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(TerminalBitmapText))]
    public sealed class TerminalDialogueTextGlitch : BaseMeshEffect
    {
        private TerminalSignalController clockSource;
        private TerminalDialogueTextSettings settings;
        private int lastState = int.MinValue, lastRevision = -1;
        public int Seed { get; set; }
        public float? PreviewClock { get; set; }
        public TerminalDialogueTextSettings Settings => settings != null ? settings
            : settings = Resources.Load<TerminalDialogueTextSettings>("MiniGame/TerminalDialogueTextDefault");
        private float Clock => PreviewClock ?? (clockSource != null ? clockSource.Clock : Time.unscaledTime);

        public void Initialize(TerminalSignalController owner)
        { clockSource = owner; Seed = GetInstanceID(); graphic.SetVerticesDirty(); }
        public void Configure(TerminalDialogueTextSettings value)
        { settings = value; lastState = int.MinValue; graphic.SetVerticesDirty(); }

        private int State()
        {
            var p = Settings;
            if (p == null || !p.Enabled || !(p.Intensity > 0)) return 0;
            float time = Mathf.Max(0, Clock) * Mathf.Clamp(p.Rate, 1, 20);
            int step = Mathf.FloorToInt(time);
            float phase = time - step;
            if (phase > Mathf.Min(.95f, Mathf.Clamp(p.BurstSeconds, .01f, .2f) * Mathf.Clamp(p.Rate, 1, 20))) return 0;
            return Unit(unchecked((uint)(step ^ Seed))) < Mathf.Clamp01(p.BurstChance) ? step + 1 : 0;
        }

        private void LateUpdate()
        {
            int state = State(), revision = Settings != null ? Settings.Revision : 0;
            if (state == lastState && revision == lastRevision) return;
            lastState = state; lastRevision = revision; graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive() || State() == 0) return;
            var p = Settings;
            int step = State();
            float offset = Mathf.Round(Mathf.Clamp(p.HorizontalPixels, 0, 3) * Mathf.Clamp01(p.Intensity));
            float vertical = Mathf.Round(Mathf.Clamp(p.VerticalPixels, 0, 4) * Mathf.Clamp01(p.Intensity));
            for (int glyph = 0; glyph < vertices.currentVertCount / 4; glyph++)
            {
                uint key = unchecked((uint)(Seed ^ step * 397 ^ glyph * 7919));
                if (Unit(key) >= Mathf.Clamp01(p.GlyphFraction)) continue;
                float delta = (Unit(key + 17) < .5f ? -1 : 1) * offset;
                float deltaY = (Unit(key + 47) < .5f ? -1 : 1) * vertical;
                float brightness = 1 - Mathf.Clamp(p.BrightnessDip, 0, .3f) * Mathf.Clamp01(p.Intensity) * Unit(key + 31);
                for (int corner = 0; corner < 4; corner++)
                {
                    int index = glyph * 4 + corner;
                    var vertex = new UIVertex(); vertices.PopulateUIVertex(ref vertex, index);
                    vertex.position.x += delta;
                    vertex.position.y += deltaY;
                    var color = vertex.color;
                    color.r = (byte)Mathf.RoundToInt(color.r * brightness);
                    color.g = (byte)Mathf.RoundToInt(color.g * brightness);
                    color.b = (byte)Mathf.RoundToInt(color.b * brightness);
                    vertex.color = color; vertices.SetUIVertex(vertex, index);
                }
            }
        }

        private static float Unit(uint value)
        {
            unchecked { value ^= value >> 16; value *= 0x7feb352d; value ^= value >> 15; value *= 0x846ca68b; value ^= value >> 16; }
            return (value & 0x00ffffff) / 16777216f;
        }
    }
}

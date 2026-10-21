using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only beach source. It contains no player, bed, UI or session.</summary>
    [DisallowMultipleComponent]
    public sealed class DreamCubemapSource : MonoBehaviour
    {
        public Material SkyMaterial;
        public Transform StandingPose;
        // The parent is a meshless copy of the original bed pivot, preserving its pillow offset.
        public Transform PillowPose;
        public Renderer[] WaterSurfaces = Array.Empty<Renderer>();
        public Light[] Lights = Array.Empty<Light>();
        public Color AmbientSkyColor = new Color(.48f, .61f, .72f);
        public Color AmbientEquatorColor = new Color(.44f, .5f, .52f);
        public Color AmbientGroundColor = new Color(.34f, .31f, .25f);
        public float AmbientIntensity = .75f;
    }
}

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only ceiling haze and insects, advanced by the room controller's clock.</summary>
    public sealed class BedtimeAtmosphere : MonoBehaviour
    {
        private const int SwarmCount = 30;
        private const int QuadsPerInsect = 9;
        private const int ConeSegments = 48;
        private static readonly int StrengthId = Shader.PropertyToID("_Strength");
        private static readonly int EmitterId = Shader.PropertyToID("_EmitterWS");
        private static readonly int AxisId = Shader.PropertyToID("_AxisWS");
        private static readonly int HeightId = Shader.PropertyToID("_Height");
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int VisualTimeId = Shader.PropertyToID("_VisualTime");

        private Transform emitter;
        private Vector3 bedCenter;
        private float radius;
        private float height;
        private Vector3 origin;
        private Vector3 axis;
        private Vector3 radialRight;
        private Vector3 radialForward;
        private Camera viewingCamera;
        private Mesh beamMesh;
        private Mesh insectsMesh;
        private Material beamMaterial;
        private Material insectsMaterial;
        private MeshRenderer beamRenderer;
        private MeshRenderer insectsRenderer;
        private GameObject beamObject;
        private GameObject insectsObject;
        private Vector3[] insectVertices;
        private readonly Vector3[] insectPositions = new Vector3[SwarmCount];
        private bool initialized;

        public float VisualTime { get; private set; }
        public int InsectCount => initialized ? SwarmCount : 0;
        public Renderer BeamRenderer => beamRenderer;

        /// <summary>World-space bed surface and ceiling emitter. Radius is the light footprint in metres.</summary>
        public void Initialize(Transform ceilingEmitter, Vector3 bedSurfaceCenter, float footprintRadius)
        {
            if (ceilingEmitter == null) throw new ArgumentNullException(nameof(ceilingEmitter));
            ReleaseOwnedResources();
            emitter = ceilingEmitter;
            bedCenter = bedSurfaceCenter;
            radius = Mathf.Max(.1f, footprintRadius);
            VisualTime = 0;
            UpdateFrame();

            var beamShader = Resources.Load<Shader>("BedtimeBeam");
            var insectShader = Resources.Load<Shader>("BedtimeInsect");
            if (beamShader == null || insectShader == null)
                throw new InvalidOperationException("Bedtime atmosphere shaders are missing from MiniGame/Resources.");
            beamMaterial = new Material(beamShader) { name = "Bedtime Beam (Runtime)" };
            insectsMaterial = new Material(insectShader) { name = "Bedtime Insects (Runtime)" };
            beamObject = CreateRenderer("BedtimeCeilingHaze", beamMaterial, out beamRenderer);
            insectsObject = CreateRenderer("BedtimeFlyingInsects", insectsMaterial, out insectsRenderer);
            BuildCone();
            BuildInsects();
            beamObject.GetComponent<MeshFilter>().sharedMesh = beamMesh;
            insectsObject.GetComponent<MeshFilter>().sharedMesh = insectsMesh;
            initialized = true;
            SetVisualState(0, 0, true);
        }

        /// <summary>Strength [0,1], elapsed seconds from the owner, and explicit pause. No Unity clock is read.</summary>
        public void SetVisualState(float strength, float deltaSeconds, bool paused)
        {
            if (!initialized) return;
            float amount = Mathf.Clamp01(strength);
            bool visible = amount > .0001f;
            beamRenderer.enabled = visible;
            insectsRenderer.enabled = visible;
            beamMaterial.SetFloat(StrengthId, amount);
            insectsMaterial.SetFloat(StrengthId, amount);
            if (!visible) return;

            if (!paused && deltaSeconds > 0 && !float.IsInfinity(deltaSeconds) && !float.IsNaN(deltaSeconds))
                VisualTime += deltaSeconds;
            // The fixture is normally static; rebuild the cone if its ceiling position is adjusted.
            if ((emitter.position - origin).sqrMagnitude > .000001f)
            {
                UpdateFrame();
                BuildCone();
                beamObject.GetComponent<MeshFilter>().sharedMesh = beamMesh;
            }
            beamMaterial.SetVector(EmitterId, origin);
            beamMaterial.SetVector(AxisId, axis);
            beamMaterial.SetFloat(HeightId, height);
            beamMaterial.SetFloat(RadiusId, radius);
            beamMaterial.SetFloat(VisualTimeId, VisualTime);
            UpdateInsects();
        }

        public Vector3 GetInsectPosition(int index)
        {
            if (index < 0 || index >= InsectCount) throw new ArgumentOutOfRangeException(nameof(index));
            return insectPositions[index];
        }

        private void UpdateFrame()
        {
            origin = emitter.position;
            var direction = bedCenter - origin;
            height = Mathf.Max(.2f, direction.magnitude);
            axis = direction.sqrMagnitude < .0001f ? Vector3.down : direction.normalized;
            radialRight = Vector3.Cross(axis, Vector3.forward).normalized;
            if (radialRight.sqrMagnitude < .01f) radialRight = Vector3.right;
            radialForward = Vector3.Cross(radialRight, axis).normalized;
        }

        private GameObject CreateRenderer(string label, Material material, out MeshRenderer meshRenderer)
        {
            var child = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            child.transform.SetParent(transform, false);
            child.layer = gameObject.layer;
            meshRenderer = child.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return child;
        }

        private void BuildCone()
        {
            DestroyOwned(beamMesh);
            beamMesh = new Mesh { name = "Bedtime Soft Cone Volume" };
            var vertices = new Vector3[ConeSegments + 2];
            var triangles = new int[ConeSegments * 6];
            vertices[0] = transform.InverseTransformPoint(origin);
            vertices[1] = transform.InverseTransformPoint(origin + axis * height);
            for (int i = 0; i < ConeSegments; i++)
            {
                float angle = i * Mathf.PI * 2 / ConeSegments;
                var radial = radialRight * Mathf.Cos(angle) + radialForward * Mathf.Sin(angle);
                vertices[i + 2] = transform.InverseTransformPoint(origin + axis * height + radial * radius);
                int current = i + 2;
                int next = (i + 1) % ConeSegments + 2;
                int triangle = i * 6;
                triangles[triangle] = 0;
                triangles[triangle + 1] = current;
                triangles[triangle + 2] = next;
                triangles[triangle + 3] = 1;
                triangles[triangle + 4] = next;
                triangles[triangle + 5] = current;
            }
            beamMesh.vertices = vertices;
            beamMesh.triangles = triangles;
            beamMesh.RecalculateBounds();
        }

        private void BuildInsects()
        {
            insectsMesh = new Mesh { name = "Bedtime Winged Swarm" };
            insectsMesh.MarkDynamic();
            int quadCount = SwarmCount * QuadsPerInsect;
            insectVertices = new Vector3[quadCount * 4];
            var uv = new Vector2[quadCount * 4];
            var colors = new Color[quadCount * 4];
            var triangles = new int[quadCount * 6];
            for (int q = 0; q < quadCount; q++)
            {
                int v = q * 4;
                uv[v] = new Vector2(-1, -1); uv[v + 1] = new Vector2(1, -1);
                uv[v + 2] = new Vector2(1, 1); uv[v + 3] = new Vector2(-1, 1);
                bool wing = q % QuadsPerInsect == 1 || q % QuadsPerInsect == 2;
                var tint = wing ? new Color(.48f, .46f, .37f, .72f) : new Color(.17f, .15f, .10f, .92f);
                for (int c = 0; c < 4; c++) colors[v + c] = tint;
                int t = q * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
            insectsMesh.vertices = insectVertices;
            insectsMesh.uv = uv;
            insectsMesh.colors = colors;
            insectsMesh.triangles = triangles;
            UpdateInsects();
        }

        private void UpdateInsects()
        {
            if (viewingCamera == null || !viewingCamera.isActiveAndEnabled) viewingCamera = Camera.main;
            Vector3 cameraForward = viewingCamera != null ? viewingCamera.transform.forward : Vector3.forward;
            Vector3 cameraUp = viewingCamera != null ? viewingCamera.transform.up : Vector3.up;
            for (int i = 0; i < SwarmCount; i++)
            {
                Vector3 p = FlightPosition(i, VisualTime);
                insectPositions[i] = p;
                Vector3 velocity = FlightPosition(i, VisualTime + .015f) - p;
                Vector3 bodyAxis = Vector3.ProjectOnPlane(velocity, cameraForward).normalized;
                if (bodyAxis.sqrMagnitude < .1f) bodyAxis = cameraUp;
                Vector3 side = Vector3.Cross(cameraForward, bodyAxis).normalized;
                float size = Mathf.Lerp(.014f, .035f, Seed(i, 7));
                float wingSpread = .28f + .72f * Mathf.Abs(Mathf.Sin(VisualTime * (31 + 13 * Seed(i, 3)) + i * 2.31f));
                int quad = i * QuadsPerInsect;
                SetQuad(quad, p, side * size * .075f, bodyAxis * size * .26f);
                for (int wing = 0; wing < 2; wing++)
                {
                    float sign = wing == 0 ? -1 : 1;
                    Vector3 direction = (side * sign * wingSpread + bodyAxis * .30f).normalized;
                    Vector3 cross = Vector3.Cross(cameraForward, direction).normalized;
                    Vector3 center = p + direction * size * .22f;
                    SetQuad(quad + 1 + wing, center, cross * size * .11f, direction * size * .23f);
                }
                // Three pairs of fine legs keep close silhouettes insect-like, without particle streaks.
                for (int leg = 0; leg < 6; leg++)
                {
                    float sign = leg % 2 == 0 ? -1 : 1;
                    float offset = (leg / 2 - 1) * .15f;
                    Vector3 from = p + bodyAxis * size * offset;
                    Vector3 to = from + side * sign * size * .23f - bodyAxis * size * (.10f - offset);
                    Vector3 legAxis = (to - from) * .5f;
                    Vector3 legWidth = Vector3.Cross(cameraForward, legAxis).normalized * size * .009f;
                    SetQuad(quad + 3 + leg, (from + to) * .5f, legWidth, legAxis);
                }
            }
            insectsMesh.vertices = insectVertices;
            insectsMesh.RecalculateBounds();
        }

        private Vector3 FlightPosition(int index, float seconds)
        {
            float phase = Seed(index, 1) * Mathf.PI * 2;
            float t = seconds * Mathf.Lerp(.62f, 1.42f, Seed(index, 2));
            float h = Mathf.Clamp(.38f + .36f * Seed(index, 4) + .09f * Mathf.Sin(t * .79f + phase)
                + .028f * Mathf.Sin(t * 3.7f + phase * 1.3f), .24f, .86f);
            float x = .51f * Mathf.Sin(t * 1.13f + phase) + .20f * Mathf.Sin(t * 3.9f + phase * 2.7f);
            float z = .49f * Mathf.Cos(t * .91f + phase * 1.6f) + .19f * Mathf.Sin(t * 4.7f + phase * .7f);
            float orbit = radius * h * Mathf.Lerp(.52f, .91f, Seed(index, 5));
            return origin + axis * (height * h) + radialRight * (x * orbit) + radialForward * (z * orbit);
        }

        private void SetQuad(int quad, Vector3 center, Vector3 halfWidth, Vector3 halfHeight)
        {
            int first = quad * 4;
            insectVertices[first] = transform.InverseTransformPoint(center - halfWidth - halfHeight);
            insectVertices[first + 1] = transform.InverseTransformPoint(center + halfWidth - halfHeight);
            insectVertices[first + 2] = transform.InverseTransformPoint(center + halfWidth + halfHeight);
            insectVertices[first + 3] = transform.InverseTransformPoint(center - halfWidth + halfHeight);
        }

        private static float Seed(int index, int channel)
        {
            uint value = (uint)(index * 747796405 + channel * 2891336453L);
            value = ((value >> (int)((value >> 28) + 4)) ^ value) * 277803737u;
            value = (value >> 22) ^ value;
            return (value & 0x00ffffffu) / 16777215f;
        }

        private void OnDestroy() => ReleaseOwnedResources();

        private void ReleaseOwnedResources()
        {
            initialized = false;
            if (beamRenderer != null) beamRenderer.enabled = false;
            if (insectsRenderer != null) insectsRenderer.enabled = false;
            DestroyOwned(beamObject); DestroyOwned(insectsObject);
            DestroyOwned(beamMesh); DestroyOwned(insectsMesh);
            DestroyOwned(beamMaterial); DestroyOwned(insectsMaterial);
            beamObject = null; insectsObject = null;
            beamMesh = null; insectsMesh = null;
            beamMaterial = null; insectsMaterial = null;
        }

        private static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}

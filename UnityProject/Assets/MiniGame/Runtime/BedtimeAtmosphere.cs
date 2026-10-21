using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Presentation-only ceiling haze and floating glow particles, advanced by the room controller's clock.</summary>
    public sealed class BedtimeAtmosphere : MonoBehaviour
    {
        private const int ParticleTotal = 36;
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
        private Mesh particleMesh;
        private Material beamMaterial;
        private Material particleMaterial;
        private MeshRenderer beamRenderer;
        private MeshRenderer particleRenderer;
        private GameObject beamObject;
        private GameObject particleObject;
        private Vector3[] particleVertices;
        private readonly Vector3[] particlePositions = new Vector3[ParticleTotal];
        private bool initialized;

        public float VisualTime { get; private set; }
        public int ParticleCount => initialized ? ParticleTotal : 0;
        public Renderer BeamRenderer => beamRenderer;
        public Renderer ParticleRenderer => particleRenderer;

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
            // Keep the legacy resource filename and asset GUID so existing builds resolve the same shader asset.
            var particleShader = Resources.Load<Shader>("BedtimeInsect");
            if (beamShader == null || particleShader == null)
                throw new InvalidOperationException("Bedtime atmosphere shaders are missing from MiniGame/Resources.");
            beamMaterial = new Material(beamShader) { name = "Bedtime Beam (Runtime)" };
            particleMaterial = new Material(particleShader) { name = "Bedtime GlowParticles (Runtime)" };
            beamObject = CreateRenderer("BedtimeCeilingHaze", beamMaterial, out beamRenderer);
            particleObject = CreateRenderer("BedtimeGlowParticles", particleMaterial, out particleRenderer);
            BuildCone();
            BuildParticles();
            beamObject.GetComponent<MeshFilter>().sharedMesh = beamMesh;
            particleObject.GetComponent<MeshFilter>().sharedMesh = particleMesh;
            initialized = true;
            BindRenderingCamera();
            SetVisualState(0, 0, true);
        }

        /// <summary>Strength [0,1], elapsed seconds from the owner, and explicit pause. No Unity clock is read.</summary>
        public void SetVisualState(float strength, float deltaSeconds, bool paused)
        {
            if (!initialized) return;
            float amount = Mathf.Clamp01(strength);
            bool visible = amount > .0001f;
            beamRenderer.enabled = visible;
            particleRenderer.enabled = visible;
            beamMaterial.SetFloat(StrengthId, amount);
            particleMaterial.SetFloat(StrengthId, amount);
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
            UpdateParticles();
        }

        public Vector3 GetParticlePosition(int index)
        {
            if (index < 0 || index >= ParticleCount) throw new ArgumentOutOfRangeException(nameof(index));
            return particlePositions[index];
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

        private void BuildParticles()
        {
            particleMesh = new Mesh { name = "Bedtime GlowParticles" };
            particleMesh.MarkDynamic();
            particleVertices = new Vector3[ParticleTotal * 4];
            var uv = new Vector2[ParticleTotal * 4];
            var colors = new Color[ParticleTotal * 4];
            var triangles = new int[ParticleTotal * 6];
            for (int q = 0; q < ParticleTotal; q++)
            {
                int v = q * 4;
                uv[v] = new Vector2(-1, -1); uv[v + 1] = new Vector2(1, -1);
                uv[v + 2] = new Vector2(1, 1); uv[v + 3] = new Vector2(-1, 1);
                var tint = Color.Lerp(new Color(1, .95f, .84f), new Color(1, .98f, .91f), Seed(q, 6));
                tint.a = Mathf.Lerp(.25f, .44f, Seed(q, 8));
                for (int c = 0; c < 4; c++) colors[v + c] = tint;
                int t = q * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
            particleMesh.vertices = particleVertices;
            particleMesh.uv = uv;
            particleMesh.colors = colors;
            particleMesh.triangles = triangles;
            UpdateParticles();
        }

        private void UpdateParticles(Camera camera = null)
        {
            if (viewingCamera == null || !viewingCamera.isActiveAndEnabled) viewingCamera = Camera.main;
            var facingCamera = camera != null ? camera : viewingCamera;
            Vector3 cameraRight = facingCamera != null ? facingCamera.transform.right : Vector3.right;
            Vector3 cameraUp = facingCamera != null ? facingCamera.transform.up : Vector3.up;
            for (int i = 0; i < ParticleTotal; i++)
            {
                Vector3 position = DriftPosition(i, VisualTime);
                particlePositions[i] = position;
                float halfSize = Mathf.Lerp(.0075f, .015f, Seed(i, 7));
                SetQuad(i, position, cameraRight * halfSize, cameraUp * halfSize);
            }
            particleMesh.vertices = particleVertices;
            particleMesh.RecalculateBounds();
        }

        private Vector3 DriftPosition(int index, float seconds)
        {
            float phase = Seed(index, 1) * Mathf.PI * 2;
            float t = seconds * Mathf.Lerp(.16f, .28f, Seed(index, 2));
            float h = Mathf.Clamp(.31f + .44f * Seed(index, 4) + .035f * Mathf.Sin(t * .58f + phase), .24f, .86f);
            float x = .48f * Mathf.Sin(t * .64f + phase) + .14f * Mathf.Sin(t * .29f + phase * 2.7f);
            float z = .44f * Mathf.Cos(t * .53f + phase * 1.6f) + .15f * Mathf.Sin(t * .33f + phase * .7f);
            float orbit = radius * h * Mathf.Lerp(.62f, .95f, Seed(index, 5));
            return origin + axis * (height * h) + radialRight * (x * orbit) + radialForward * (z * orbit);
        }

        private void SetQuad(int quad, Vector3 center, Vector3 halfWidth, Vector3 halfHeight)
        {
            int first = quad * 4;
            particleVertices[first] = transform.InverseTransformPoint(center - halfWidth - halfHeight);
            particleVertices[first + 1] = transform.InverseTransformPoint(center + halfWidth - halfHeight);
            particleVertices[first + 2] = transform.InverseTransformPoint(center + halfWidth + halfHeight);
            particleVertices[first + 3] = transform.InverseTransformPoint(center - halfWidth + halfHeight);
        }

        private static float Seed(int index, int channel)
        {
            uint value = (uint)(index * 747796405 + channel * 2891336453L);
            value = ((value >> (int)((value >> 28) + 4)) ^ value) * 277803737u;
            value = (value >> 22) ^ value;
            return (value & 0x00ffffffu) / 16777215f;
        }

        private void OnEnable() => BindRenderingCamera();
        private void OnDisable() => Camera.onPreCull -= FaceRenderingCamera;
        private void BindRenderingCamera()
        {
            Camera.onPreCull -= FaceRenderingCamera;
            if (initialized && isActiveAndEnabled) Camera.onPreCull += FaceRenderingCamera;
        }
        private void FaceRenderingCamera(Camera camera)
        {
            if (!initialized || !isActiveAndEnabled || particleRenderer == null || !particleRenderer.enabled || camera == null
                || (camera.cullingMask & (1 << particleObject.layer)) == 0) return;
            // Orient for the actual render camera, including Scene/capture cameras. Rendering never advances VisualTime.
            UpdateParticles(camera);
        }
        private void OnDestroy() { Camera.onPreCull -= FaceRenderingCamera; ReleaseOwnedResources(); }

        private void ReleaseOwnedResources()
        {
            Camera.onPreCull -= FaceRenderingCamera;
            initialized = false;
            if (beamRenderer != null) beamRenderer.enabled = false;
            if (particleRenderer != null) particleRenderer.enabled = false;
            DestroyOwned(beamObject); DestroyOwned(particleObject);
            DestroyOwned(beamMesh); DestroyOwned(particleMesh);
            DestroyOwned(beamMaterial); DestroyOwned(particleMaterial);
            beamObject = null; particleObject = null;
            beamMesh = null; particleMesh = null;
            beamMaterial = null; particleMaterial = null;
            viewingCamera = null;
        }

        private static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}

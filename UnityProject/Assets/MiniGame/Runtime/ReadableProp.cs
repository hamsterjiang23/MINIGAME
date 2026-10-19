using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Configurable room text and a focus outline; input and subtitle lifetime belong to the player host.</summary>
    public sealed class ReadableProp : MonoBehaviour
    {
        [SerializeField] private string id = "";
        [SerializeField, TextArea] private string caption = "";
        [SerializeField] private Renderer surface;
        [SerializeField, Min(.1f), InspectorName("交互距离（米）")]
        [Tooltip("从玩家镜头到鼠标命中的海报表面的最大距离；距离内才会显示白色描边并允许阅读。")]
        private float interactionDistance = 2f;
        [SerializeField, Min(.1f)] private float captionSeconds = 4f;
        private LineRenderer outline;
        private Material outlineMaterial;
        private bool preparingCameras;

        public string Id => id;
        public string Caption => caption;
        public Texture Texture => surface != null && surface.sharedMaterial != null ? surface.sharedMaterial.mainTexture : null;
        public Renderer Surface => surface;
        public float InteractionDistance
        {
            get => ValidPositive(interactionDistance, 2f);
            set => interactionDistance = ValidPositive(value, 2f);
        }
        public float CaptionSeconds => captionSeconds;
        public bool IsHighlighted => outline != null && outline.enabled;

        public void Configure(string stableId, string text, Renderer posterSurface, float distance = 2f, float seconds = 4f)
        {
            id = stableId ?? "";
            caption = text ?? "";
            surface = posterSurface;
            InteractionDistance = distance;
            captionSeconds = ValidPositive(seconds, 4f);
            if (Application.isPlaying) RebuildOutline();
        }

        private void Awake()
        {
            if (surface == null) surface = GetComponent<Renderer>();
            RebuildOutline();
        }

        public void Highlight(bool enabled)
        {
            if (outline == null && enabled) RebuildOutline();
            if (outline != null) outline.enabled = enabled && isActiveAndEnabled && surface != null && surface.enabled;
            bool prepare = IsHighlighted;
            if (prepare == preparingCameras) return;
            if (prepare) Camera.onPreCull += PrepareCamera;
            else Camera.onPreCull -= PrepareCamera;
            preparingCameras = prepare;
        }

        private void PrepareCamera(Camera camera)
        {
            if (!IsHighlighted || (camera.cullingMask & (1 << outline.gameObject.layer)) == 0) return;
            float width = .006f;
            int pixelSize = InteractionFocusOutline.ActivePixelSize(camera);
            if (pixelSize > 1)
            {
                float depth = camera.nearClipPlane;
                for (int index = 0; index < outline.positionCount; index++)
                    depth = Mathf.Max(depth, Vector3.Dot(outline.transform.TransformPoint(outline.GetPosition(index))
                        - camera.transform.position, camera.transform.forward));
                float worldHeight = camera.orthographic ? 2 * camera.orthographicSize
                    : 2 * depth * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                width = Mathf.Max(width, worldHeight / Mathf.Max(1, camera.pixelHeight) * pixelSize * 2);
            }
            outline.startWidth = outline.endWidth = width;
        }

        private void RebuildOutline()
        {
            ClearOutline();
            if (surface == null) return;
            var meshFilter = surface.GetComponent<MeshFilter>();
            var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (mesh == null) return;
            var shader = Resources.Load<Shader>("ReadablePropOutline");
            if (shader == null || !shader.isSupported) return;
            outlineMaterial = new Material(shader) { name = "Readable poster focus outline", hideFlags = HideFlags.HideAndDontSave };
            outlineMaterial.SetColor("_Color", Color.white);
            var root = new GameObject("Poster focus outline");
            root.layer = surface.gameObject.layer;
            root.transform.SetParent(surface.transform, false);
            outline = root.AddComponent<LineRenderer>();
            outline.useWorldSpace = false;
            outline.loop = true;
            outline.positionCount = 4;
            outline.startWidth = outline.endWidth = .006f;
            outline.numCornerVertices = 2;
            outline.sharedMaterial = outlineMaterial;
            outline.shadowCastingMode = ShadowCastingMode.Off;
            outline.receiveShadows = false;
            outline.lightProbeUsage = LightProbeUsage.Off;
            outline.reflectionProbeUsage = ReflectionProbeUsage.Off;
            var bounds = mesh.bounds;
            var normals = mesh.isReadable ? mesh.normals : null;
            var normal = normals != null && normals.Length > 0 ? normals[0] : Vector3.back;
            var offset = normal.normalized * .003f;
            outline.SetPosition(0, new Vector3(bounds.min.x, bounds.min.y, bounds.center.z) + offset);
            outline.SetPosition(1, new Vector3(bounds.max.x, bounds.min.y, bounds.center.z) + offset);
            outline.SetPosition(2, new Vector3(bounds.max.x, bounds.max.y, bounds.center.z) + offset);
            outline.SetPosition(3, new Vector3(bounds.min.x, bounds.max.y, bounds.center.z) + offset);
            outline.enabled = false;
        }

        private static float ValidPositive(float value, float fallback) => float.IsNaN(value) || float.IsInfinity(value)
            ? fallback : Mathf.Max(.1f, value);

        private void OnValidate()
        {
            interactionDistance = ValidPositive(interactionDistance, 2f);
            captionSeconds = ValidPositive(captionSeconds, 4f);
        }

        private void OnDisable() => Highlight(false);

        private void OnDestroy()
        {
            ClearOutline();
        }

        private void ClearOutline()
        {
            if (preparingCameras) Camera.onPreCull -= PrepareCamera;
            preparingCameras = false;
            if (outline != null)
            {
                outline.enabled = false;
                if (Application.isPlaying) Destroy(outline.gameObject);
                else DestroyImmediate(outline.gameObject);
            }
            if (outlineMaterial != null)
            {
                if (Application.isPlaying) Destroy(outlineMaterial);
                else DestroyImmediate(outlineMaterial);
            }
            outline = null;
            outlineMaterial = null;
        }
    }
}

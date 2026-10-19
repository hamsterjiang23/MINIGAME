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
        [SerializeField, Min(.1f)] private float interactionDistance = 2f;
        [SerializeField, Min(.1f)] private float captionSeconds = 4f;
        private LineRenderer outline;
        private Material outlineMaterial;

        public string Id => id;
        public string Caption => caption;
        public Texture Texture => surface != null && surface.sharedMaterial != null ? surface.sharedMaterial.mainTexture : null;
        public Renderer Surface => surface;
        public float InteractionDistance => interactionDistance;
        public float CaptionSeconds => captionSeconds;
        public bool IsHighlighted => outline != null && outline.enabled;

        public void Configure(string stableId, string text, Renderer posterSurface, float distance = 2f, float seconds = 4f)
        {
            id = stableId ?? "";
            caption = text ?? "";
            surface = posterSurface;
            interactionDistance = Mathf.Max(.1f, distance);
            captionSeconds = Mathf.Max(.1f, seconds);
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
            if (outline != null) outline.enabled = enabled && isActiveAndEnabled;
        }

        private void RebuildOutline()
        {
            if (surface == null) return;
            if (outline != null) Destroy(outline.gameObject);
            if (outlineMaterial != null) Destroy(outlineMaterial);
            var meshFilter = surface.GetComponent<MeshFilter>();
            var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (mesh == null) return;
            var shader = Resources.Load<Shader>("ReadablePropOutline");
            if (shader == null) return;
            outlineMaterial = new Material(shader) { name = "Readable poster focus outline" };
            outlineMaterial.SetColor("_Color", new Color(.76f, .85f, .68f, .95f));
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
            var normal = mesh.normals.Length > 0 ? mesh.normals[0] : Vector3.back;
            var offset = normal.normalized * .003f;
            outline.SetPosition(0, new Vector3(bounds.min.x, bounds.min.y, bounds.center.z) + offset);
            outline.SetPosition(1, new Vector3(bounds.max.x, bounds.min.y, bounds.center.z) + offset);
            outline.SetPosition(2, new Vector3(bounds.max.x, bounds.max.y, bounds.center.z) + offset);
            outline.SetPosition(3, new Vector3(bounds.min.x, bounds.max.y, bounds.center.z) + offset);
            outline.enabled = false;
        }

        private void OnDisable() => Highlight(false);

        private void OnDestroy()
        {
            if (outlineMaterial != null) Destroy(outlineMaterial);
        }
    }
}

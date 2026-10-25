using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGame.UnityHost
{
    /// <summary>Fixed scenery calibrated to one doorway eye, revealing its room surfaces when viewed off-axis.</summary>
    public sealed class DreamCubemapRoomSurface : IDisposable
    {
        public GameObject Root { get; }
        public Renderer[] Faces { get; }
        public Bounds Bounds { get; }
        public Material Material { get; }
        public Vector3 ProjectionOrigin { get; private set; }
        public float Opacity { get; private set; } = 1;
        private readonly Mesh[] meshes = new Mesh[6];
        private bool disposed;

        public DreamCubemapRoomSurface(Bounds bounds, Transform parent, int layer, Vector3? projectionOrigin = null)
        {
            var shader = Resources.Load<Shader>("DreamCubemapRoomSurface");
            if (shader == null || !shader.isSupported) throw new InvalidOperationException("房间六面贴图 Shader 不可用。");
            Bounds = bounds;
            ProjectionOrigin = projectionOrigin ?? bounds.center;
            Root = new GameObject("Dream room six mapped surfaces");
            Root.transform.SetParent(parent, false); Root.transform.position = bounds.center;
            Root.layer = layer;
            Material = new Material(shader) { name = "Cubemap on room walls floor ceiling", hideFlags = HideFlags.DontSave };
            Material.SetVector("_RoomHalfSize", bounds.extents);
            Material.SetVector("_RoomCenter", bounds.center);
            Material.SetVector("_ProjectionOrigin", ProjectionOrigin);
            Faces = new Renderer[6];
            float x=bounds.extents.x, y=bounds.extents.y, z=bounds.extents.z;
            Make(0,"Front wall (+Z)",new[]{new Vector3(-x,-y,z),new Vector3(x,-y,z),new Vector3(x,y,z),new Vector3(-x,y,z)},layer);
            Make(1,"Back wall (-Z)",new[]{new Vector3(x,-y,-z),new Vector3(-x,-y,-z),new Vector3(-x,y,-z),new Vector3(x,y,-z)},layer);
            Make(2,"Left wall (-X)",new[]{new Vector3(-x,-y,-z),new Vector3(-x,-y,z),new Vector3(-x,y,z),new Vector3(-x,y,-z)},layer);
            Make(3,"Right wall (+X)",new[]{new Vector3(x,-y,z),new Vector3(x,-y,-z),new Vector3(x,y,-z),new Vector3(x,y,z)},layer);
            Make(4,"Ceiling (+Y)",new[]{new Vector3(-x,y,z),new Vector3(x,y,z),new Vector3(x,y,-z),new Vector3(-x,y,-z)},layer);
            Make(5,"Floor (-Y)",new[]{new Vector3(-x,-y,-z),new Vector3(x,-y,-z),new Vector3(x,-y,z),new Vector3(-x,-y,z)},layer);
        }
        private void Make(int index,string name,Vector3[] vertices,int layer)
        {
            var face=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            face.transform.SetParent(Root.transform,false);face.layer=layer;
            meshes[index]=new Mesh{name="Room interior "+name,hideFlags=HideFlags.DontSave};
            meshes[index].vertices=vertices;meshes[index].triangles=new[]{0,2,1,0,3,2};
            meshes[index].RecalculateNormals();meshes[index].RecalculateBounds();
            face.GetComponent<MeshFilter>().sharedMesh=meshes[index];
            var renderer=face.GetComponent<MeshRenderer>();renderer.sharedMaterial=Material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            Faces[index]=renderer;
        }
        public void Bind(Texture texture,float yaw)
        {
            if(disposed) return;
            Material.SetTexture("_Tex",texture);
            Material.SetFloat("_Yaw",float.IsNaN(yaw)||float.IsInfinity(yaw)?0:yaw);
        }
        public void SetProjectionOrigin(Vector3 origin)
        {
            if (disposed) return;
            ProjectionOrigin = origin;
            Material.SetVector("_ProjectionOrigin", origin);
        }
        public void BindHead(Texture texture, Vector3 origin)
        {
            if (disposed) return;
            Material.SetTexture("_HeadTex", texture);
            Material.SetVector("_HeadOrigin", origin);
            Material.SetFloat("_HeadEnabled", texture != null ? 1 : 0);
        }
        public void SetOutline(bool visible)
        {
            if (!disposed) Material.SetFloat("_EdgeWidth", visible ? .015f : 0);
        }
        public void SetOpacity(float value)
        {
            if (disposed) return;
            value = float.IsNaN(value) || float.IsInfinity(value) ? 1 : Mathf.Clamp01(value);
            if (Opacity != value)
            {
                bool transparent = value < 1;
                Material.SetFloat("_Opacity", value);
                Material.SetInt("_SrcBlend", (int)(transparent ? BlendMode.SrcAlpha : BlendMode.One));
                Material.SetInt("_DstBlend", (int)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
                Material.SetInt("_ZWrite", transparent ? 0 : 1);
                // During the dissolve the print is an image overlay; even exterior props in front
                // of a wall must enter gradually instead of popping through it at full brightness.
                Material.SetInt("_ZTest", (int)(transparent ? CompareFunction.Always : CompareFunction.LessEqual));
                Material.SetOverrideTag("RenderType", transparent ? "Transparent" : "Opaque");
                Material.renderQueue = transparent ? (int)RenderQueue.Overlay : (int)RenderQueue.Geometry;
                Opacity = value;
            }
            SetVisible(value > 0);
        }

        public void SetVisible(bool visible)
        {
            if (!disposed && Root.activeSelf != visible) Root.SetActive(visible);
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            Root.SetActive(false);Destroy(Root);Destroy(Material);
            foreach(var mesh in meshes)Destroy(mesh);
        }
        private static void Destroy(UnityEngine.Object value)
        {
            if(value==null)return;
            if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}

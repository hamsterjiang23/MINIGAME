using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Installs the single-mesh floor rug without rebuilding the room or adding collision.</summary>
    public static class RoomRugSetup
    {
        public const string RootName = "Reference floor rug";
        public const string ArtPath = "Assets/MiniGame/Art/MainRoom/RoomLighting/";
        public const string TexturePath = ArtPath + "WornTraditionalRugAlbedo.png";
        public const string MeshPath = ArtPath + "WornReferenceRug.obj";
        public const string MaterialPath = ArtPath + "WornTraditionalRug.mat";
        public static readonly Vector2 DefaultSize = new Vector2(3.4f, 2f);

        public static Renderer Ensure(Scene scene, Transform parent, Material roomBase)
        {
            if (!scene.IsValid() || !scene.isLoaded) throw new ArgumentException("地毯需要已加载的场景。", nameof(scene));
            if (parent != null && parent.gameObject.scene != scene)
                throw new ArgumentException("地毯父物体必须属于目标场景。", nameof(parent));
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请退出播放后首次接入地毯；播放中的大小与位置由房间灯光配置调整。");

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath);
            var sourceFilter = model == null ? null : model.GetComponentInChildren<MeshFilter>(true);
            if (sourceFilter == null || sourceFilter.sharedMesh == null)
                throw new InvalidOperationException("地毯模型尚未导入：" + MeshPath);
            var root = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(item => item.name == RootName);
            if (root == null)
            {
                var rug = new GameObject(RootName, typeof(MeshFilter), typeof(MeshRenderer));
                Undo.RegisterCreatedObjectUndo(rug, "添加房间地毯");
                SceneManager.MoveGameObjectToScene(rug, scene);
                root = rug.transform;
                if (parent != null) root.SetParent(parent, false);
                var floor = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Renderer>(true))
                    .FirstOrDefault(item => item.name == "B_Floor");
                float floorHeight = floor != null ? floor.bounds.max.y : -.72788f;
                root.SetPositionAndRotation(new Vector3(-1.7f, floorHeight + .008f, -1.75f), Quaternion.identity);
                root.localScale = new Vector3(DefaultSize.x, 1f, DefaultSize.y);
            }

            var filter = root.GetComponent<MeshFilter>();
            if (filter == null) filter = Undo.AddComponent<MeshFilter>(root.gameObject);
            var renderer = root.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = Undo.AddComponent<MeshRenderer>(root.gameObject);
            Undo.RecordObjects(new UnityEngine.Object[] { filter, renderer }, "更新房间地毯");
            filter.sharedMesh = sourceFilter.sharedMesh;
            renderer.sharedMaterial = EnsureMaterial(roomBase);
            renderer.receiveShadows = true;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            foreach (var collider in root.GetComponentsInChildren<Collider>(true)) Undo.DestroyObjectImmediate(collider);
            EditorUtility.SetDirty(filter);
            EditorUtility.SetDirty(renderer);
            return renderer;
        }

        private static Material EnsureMaterial(Material roomBase)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;
            var shader = roomBase != null ? roomBase.shader : Shader.Find("MiniGame/Room Art Standard");
            if (shader == null) throw new InvalidOperationException("房间材质 Shader 尚未导入。");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (texture == null) throw new InvalidOperationException("地毯贴图尚未导入：" + TexturePath);
            material = new Material(shader) { name = "WornTraditionalRug", mainTexture = texture, color = Color.white };
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_Glossiness", .06f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetColor("_EmissionColor", Color.black);
            AssetDatabase.CreateAsset(material, MaterialPath);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }
    }
}

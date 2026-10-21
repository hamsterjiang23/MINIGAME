using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Bakes one fixed beach panorama for the room's doorway-calibrated wallpaper.</summary>
    public static class DreamDoorwayPanoramaBaker
    {
        public const string CubemapPath = "Assets/MiniGame/Art/DreamEnding/DoorwayBeachPanorama.asset";
        public const string MaterialPath = "Assets/MiniGame/Art/DreamEnding/DoorwayBeachSkybox.mat";
        private static readonly CubemapFace[] Faces =
        {
            CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY,
            CubemapFace.NegativeY, CubemapFace.PositiveZ, CubemapFace.NegativeZ
        };

        [MenuItem("MiniGame/梦境/烘焙房门固定海滩天空盒")]
        public static void Bake()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请先退出播放模式再烘焙固定海滩图。");
            var profile = AssetDatabase.LoadAssetAtPath<DreamSequenceProfile>(DreamSequenceSetup.ProfilePath);
            if (profile == null || profile.CubemapSourcePrefab == null)
                throw new InvalidOperationException("请先生成海滩全景捕获源，再烘焙固定天空盒。");
            var skyboxShader = Shader.Find("Skybox/Cubemap");
            if (skyboxShader == null || !skyboxShader.isSupported)
                throw new InvalidOperationException("当前图形设备无法使用 Skybox/Cubemap Shader。");

            var previousScene = SceneManager.GetActiveScene();
            var previousTarget = RenderTexture.active;
            bool previousSrgbWrite = GL.sRGBWrite;
            Scene staging = default;
            DreamCubemapRenderer capture = null;
            Cubemap baked = null;
            try
            {
                // A preview scene also works when the user's current scene is untitled/unsaved.
                // The capture owns its isolated hidden source; no user scene is saved or reloaded.
                staging = EditorSceneManager.NewPreviewScene();
                var cameraObject = new GameObject("Fixed panorama bake viewer", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, staging);
                var camera = cameraObject.GetComponent<Camera>();
                camera.enabled = false;
                capture = new DreamCubemapRenderer(profile, camera, false);
                capture.Render(0, BeachEndingPhase.Walking, 0, 0, force: true);
                if (!capture.IsReady) throw new InvalidOperationException("海滩全景首幅捕获未完成。");
                baked = ReadCubemap(capture.Texture);
                float worstError = VerifySixViews(camera, capture.Texture, baked);

                var cubeAsset = AssetDatabase.LoadAssetAtPath<Cubemap>(CubemapPath);
                if (cubeAsset == null)
                {
                    AssetDatabase.CreateAsset(baked, CubemapPath);
                    cubeAsset = baked;
                    baked = null;
                }
                else
                {
                    EditorUtility.CopySerialized(baked, cubeAsset);
                    EditorUtility.SetDirty(cubeAsset);
                }
                AssetDatabase.SaveAssetIfDirty(cubeAsset);

                var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if (material == null)
                {
                    material = new Material(skyboxShader) { name = "Doorway Beach Skybox" };
                    AssetDatabase.CreateAsset(material, MaterialPath);
                }
                else material.shader = skyboxShader;
                material.SetTexture("_Tex", cubeAsset);
                material.SetColor("_Tint", new Color(.5f, .5f, .5f, 1));
                material.SetFloat("_Exposure", 1);
                material.SetFloat("_Rotation", 0);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
                Undo.RecordObject(profile, "绑定房门固定海滩天空盒");
                profile.RoomPanoramaSkybox = material;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssetIfDirty(profile);
                Debug.Log("固定海滩天空盒已烘焙：" + MaterialPath + "; 六向读回校验最大平均误差="
                    + worstError.ToString("F6") + "; 每面=" + cubeAsset.width + "; 海浪时钟=0。");
            }
            finally
            {
                RenderTexture.active = previousTarget;
                GL.sRGBWrite = previousSrgbWrite;
                capture?.Dispose();
                if (baked != null) UnityEngine.Object.DestroyImmediate(baked);
                if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
                if (staging.IsValid()) EditorSceneManager.ClosePreviewScene(staging);
            }
        }

        private static Cubemap ReadCubemap(RenderTexture source)
        {
            if (source == null || source.dimension != TextureDimension.Cube)
                throw new InvalidOperationException("固定全景烘焙需要完整 Cubemap 渲染纹理。");
            var previousTarget = RenderTexture.active;
            Texture2D reader = null;
            Cubemap result = null;
            try
            {
                reader = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, !source.sRGB)
                { hideFlags = HideFlags.HideAndDontSave };
                result = new Cubemap(source.width, TextureFormat.RGBA32, false)
                { name = "Doorway Beach Panorama", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                foreach (var face in Faces)
                {
                    Graphics.SetRenderTarget(source, 0, face);
                    reader.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                    // ReadPixels and SetPixels both use bottom-left row order. Preserve each named face directly.
                    result.SetPixels(reader.GetPixels(), face, 0);
                }
                result.Apply(false, false);
                return result;
            }
            catch
            {
                if (result != null) UnityEngine.Object.DestroyImmediate(result);
                throw;
            }
            finally
            {
                RenderTexture.active = previousTarget;
                if (reader != null) UnityEngine.Object.DestroyImmediate(reader);
            }
        }

        private static float VerifySixViews(Camera camera, RenderTexture original, Cubemap baked)
        {
            var shader = Resources.Load<Shader>("DreamDynamicCubemap");
            if (shader == null || !shader.isSupported)
                throw new InvalidOperationException("固定全景读回校验缺少 Cubemap 采样 Shader。");
            var previousTarget = RenderTexture.active;
            Material material = null;
            RenderTexture target = null;
            Texture2D reader = null;
            try
            {
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                material.SetFloat("_Yaw", 0);
                target = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGB32)
                { hideFlags = HideFlags.HideAndDontSave, antiAliasing = 1 };
                if (!target.Create()) throw new InvalidOperationException("无法创建固定全景读回校验纹理。");
                reader = new Texture2D(64, 64, TextureFormat.RGBA32, false, !target.sRGB)
                { hideFlags = HideFlags.HideAndDontSave };
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.fieldOfView = 90;
                camera.aspect = 1;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                var skybox = camera.gameObject.AddComponent<Skybox>();
                skybox.material = material;
                Vector3[] angles = { new Vector3(0, 90, 0), new Vector3(0, -90, 0), new Vector3(-90, 0, 0),
                    new Vector3(90, 0, 0), Vector3.zero, new Vector3(0, 180, 0) };
                float worst = 0;
                for (int face = 0; face < Faces.Length; face++)
                {
                    camera.transform.rotation = Quaternion.Euler(angles[face]);
                    var expected = SampleView(camera, material, original, target, reader);
                    var actual = SampleView(camera, material, baked, target, reader);
                    double difference = 0;
                    for (int i = 0; i < expected.Length; i++)
                        difference += Mathf.Abs(expected[i].r - actual[i].r) + Mathf.Abs(expected[i].g - actual[i].g)
                            + Mathf.Abs(expected[i].b - actual[i].b);
                    float error = (float)(difference / (expected.Length * 3 * 255.0));
                    worst = Mathf.Max(worst, error);
                    if (error > .005f)
                        throw new InvalidOperationException("固定全景的 " + Faces[face]
                            + " 面读回方向或色彩不匹配，平均误差=" + error.ToString("F6") + "；未覆盖天空盒资产。");
                }
                return worst;
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previousTarget;
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (reader != null) UnityEngine.Object.DestroyImmediate(reader);
                if (material != null) UnityEngine.Object.DestroyImmediate(material);
            }
        }

        private static Color32[] SampleView(Camera camera, Material material, Texture texture, RenderTexture target, Texture2D reader)
        {
            material.SetTexture("_Tex", texture);
            camera.Render();
            RenderTexture.active = target;
            reader.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0, false);
            return reader.GetPixels32();
        }
    }
}

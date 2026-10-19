using System.Collections.Generic;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>A separate room camera sees a seated player proxy that the first-person camera never renders.</summary>
    [DisallowMultipleComponent]
    public sealed class SelfReviewMonitor : MonoBehaviour
    {
        public const int ProxyLayer = 30;
        private ApartmentPlayer player;
        private GameRuntime runtime;
        private Camera roomCamera;
        private Camera monitorCamera;
        private Transform proxyRoot;
        private RenderTexture output;
        private Shader monitorShader;
        private bool restoreProxyLayer;
        private readonly List<Material> ownedMaterials = new List<Material>();

        public RenderTexture Output => output;
        public Camera ViewCamera => monitorCamera;
        public Transform ProxyRoot => proxyRoot;
        public bool IsActive => proxyRoot != null && proxyRoot.gameObject.activeSelf;
        public int RenderedFrames { get; private set; }

        public void Configure(ApartmentPlayer owner)
        {
            if (player != owner) RestoreMainCameraMask();
            player = owner;
            if (player == null) return;
            roomCamera = player.ViewCamera;
            if (output == null)
            {
                output = new RenderTexture(512, 288, 24, RenderTextureFormat.ARGB32)
                { name = "Self-review live room", filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave };
                output.Create();
            }
            monitorShader = Resources.Load<Shader>("SelfReviewMonitor");
            if (monitorShader == null || !monitorShader.isSupported)
            {
                Debug.LogError("SelfReviewMonitor shader is unavailable.", this);
                return;
            }
            if (monitorCamera == null)
            {
                var cameraObject = new GameObject("Self-review room camera", typeof(Camera));
                cameraObject.transform.SetParent(transform, false);
                cameraObject.hideFlags = HideFlags.DontSave;
                monitorCamera = cameraObject.GetComponent<Camera>();
                monitorCamera.enabled = false;
                monitorCamera.targetTexture = output;
                monitorCamera.clearFlags = CameraClearFlags.SolidColor;
                monitorCamera.backgroundColor = new Color(.07f, .1f, .075f);
                monitorCamera.nearClipPlane = .04f;
                monitorCamera.farClipPlane = 30;
                monitorCamera.fieldOfView = 58;
                monitorCamera.allowHDR = false;
                monitorCamera.allowMSAA = false;
                cameraObject.SetActive(false);
            }
            if (proxyRoot == null) CreatePlayerProxy();
            ExcludeProxyFromMainCamera();
            ApplyView();
        }

        public void Bind(GameRuntime game)
        {
            if (runtime != null) runtime.Changed -= OnViewChanged;
            runtime = game;
            if (runtime != null && isActiveAndEnabled) runtime.Changed += OnViewChanged;
            ApplyView();
        }

        private void OnViewChanged(RuntimeView view) => ApplyView();

        private void ApplyView()
        {
            var view = runtime?.GetView();
            bool active = isActiveAndEnabled && player != null && view != null && view.UsesDreamFlow
                && view.Game.Phase == GamePhase.SelfReview && view.TerminalOpen && view.TerminalPage == TerminalPage.SelfReview;
            if (proxyRoot != null) proxyRoot.gameObject.SetActive(active);
            if (monitorCamera != null) monitorCamera.gameObject.SetActive(active);
        }

        private void LateUpdate()
        {
            if (!IsActive || runtime == null || runtime.GetView().Paused || monitorShader == null || monitorCamera == null) return;
            ExcludeProxyFromMainCamera();
            proxyRoot.SetPositionAndRotation(player.transform.position, player.transform.rotation);
            var actor = player.transform;
            var target = actor.position + Vector3.up * .95f;
            var position = actor.position - actor.forward * .95f + actor.right * .9f + Vector3.up * 1.65f;
            monitorCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position, Vector3.up));
            monitorCamera.cullingMask = roomCamera.cullingMask | (1 << ProxyLayer);
            monitorCamera.RenderWithShader(monitorShader, "");
            RenderedFrames++;
        }

        private void ExcludeProxyFromMainCamera()
        {
            if (roomCamera == null) return;
            if ((roomCamera.cullingMask & (1 << ProxyLayer)) != 0) restoreProxyLayer = true;
            roomCamera.cullingMask &= ~(1 << ProxyLayer);
        }

        private void RestoreMainCameraMask()
        {
            if (roomCamera != null && restoreProxyLayer) roomCamera.cullingMask |= 1 << ProxyLayer;
            restoreProxyLayer = false;
        }

        private void CreatePlayerProxy()
        {
            var root = new GameObject("Self-review seated player") { layer = ProxyLayer, hideFlags = HideFlags.DontSave };
            proxyRoot = root.transform;
            proxyRoot.SetParent(transform, false);
            root.SetActive(false);
            var clothes = MakeMaterial("Review uniform", new Color(.43f, .49f, .45f));
            var trousers = MakeMaterial("Review trousers", new Color(.16f, .2f, .18f));
            var skin = MakeMaterial("Review skin", new Color(.61f, .55f, .44f));
            var dark = MakeMaterial("Review hair and shoes", new Color(.08f, .085f, .075f));
            Part("Torso", PrimitiveType.Capsule, new Vector3(0, .99f, 0), new Vector3(.4f, .31f, .25f), clothes);
            Part("Pelvis", PrimitiveType.Sphere, new Vector3(0, .68f, 0), new Vector3(.37f, .25f, .28f), trousers);
            Part("Neck", PrimitiveType.Cylinder, new Vector3(0, 1.27f, .015f), new Vector3(.11f, .07f, .11f), skin);
            Part("Head", PrimitiveType.Sphere, new Vector3(0, 1.43f, .025f), new Vector3(.23f, .29f, .24f), skin);
            Part("Hair", PrimitiveType.Sphere, new Vector3(0, 1.51f, 0), new Vector3(.24f, .16f, .25f), dark);
            Part("Nose", PrimitiveType.Sphere, new Vector3(0, 1.42f, .15f), new Vector3(.055f, .07f, .055f), skin);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side;
                Part("Eye " + side, PrimitiveType.Sphere, new Vector3(x * .045f, 1.46f, .134f), new Vector3(.022f, .015f, .012f), dark);
                Limb("Sleeve " + side, new Vector3(x * .22f, 1.16f, .015f), new Vector3(x * .29f, .92f, .15f), .085f, clothes);
                Limb("Forearm " + side, new Vector3(x * .29f, .92f, .15f), new Vector3(x * .23f, .94f, .43f), .06f, clothes);
                Part("Hand " + side, PrimitiveType.Sphere, new Vector3(x * .23f, .94f, .46f), new Vector3(.095f, .055f, .14f), skin);
                Limb("Thigh " + side, new Vector3(x * .12f, .67f, .02f), new Vector3(x * .13f, .65f, .4f), .09f, trousers);
                Limb("Calf " + side, new Vector3(x * .13f, .65f, .4f), new Vector3(x * .13f, .18f, .44f), .075f, trousers);
                Part("Shoe " + side, PrimitiveType.Cube, new Vector3(x * .13f, .12f, .52f), new Vector3(.16f, .11f, .29f), dark);
            }
        }

        private Material MakeMaterial(string label, Color color)
        {
            var material = new Material(monitorShader) { name = label, color = color, hideFlags = HideFlags.DontSave };
            ownedMaterials.Add(material);
            return material;
        }

        private Transform Part(string label, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = label; part.layer = ProxyLayer; part.hideFlags = HideFlags.DontSave;
            part.transform.SetParent(proxyRoot, false);
            part.transform.localPosition = position; part.transform.localScale = scale;
            var collider = part.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; Destroy(collider); }
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
        }

        private void Limb(string label, Vector3 start, Vector3 end, float radius, Material material)
        {
            var part = Part(label, PrimitiveType.Cylinder, (start + end) * .5f,
                new Vector3(radius * 2, Vector3.Distance(start, end) * .5f, radius * 2), material);
            part.localRotation = Quaternion.FromToRotation(Vector3.up, end - start);
        }

        private void OnEnable()
        {
            if (runtime != null) runtime.Changed += OnViewChanged;
            ExcludeProxyFromMainCamera();
            ApplyView();
        }

        private void OnDisable()
        {
            if (runtime != null) runtime.Changed -= OnViewChanged;
            ApplyView();
            RestoreMainCameraMask();
        }

        private void OnDestroy()
        {
            RestoreMainCameraMask();
            if (monitorCamera != null) Destroy(monitorCamera.gameObject);
            if (proxyRoot != null) Destroy(proxyRoot.gameObject);
            if (output != null) { output.Release(); Destroy(output); }
            foreach (var material in ownedMaterials) if (material != null) Destroy(material);
        }
    }
}

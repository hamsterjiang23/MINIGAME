#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    /// <summary>Opt-in Player verification of the real fresh campaign, without a separate demo runtime.</summary>
    public sealed class DayZeroOpeningPlayerSmoke : MonoBehaviour
    {
        private MiniGameBootstrap host;
        private string folder;
        private float began, nextAction, pullbackStart = -1;
        private int lines;
        private bool submitted, slept, done, success;
        private Button pendingButton;
        private int pendingFrame;
        private readonly HashSet<string> captures = new HashSet<string>();

        [Serializable] private sealed class Report
        {
            public string result, detail, unity;
            public string captureMode = "Native UI camera RenderTexture and the displayed CRT material; room capture uses the actual room camera";
            public int sourceLines, day;
            public bool singlePurification, bedOnly, furnitureRestored;
            public float seconds;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Environment.GetCommandLineArgs().Contains("--day0-opening-smoke"))
                new GameObject("Day0 opening Player verification").AddComponent<DayZeroOpeningPlayerSmoke>();
        }

        private void Start()
        {
            began = Time.realtimeSinceStartup;
            var root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            folder = Path.Combine(string.IsNullOrEmpty(root) ? Application.persistentDataPath : root, "Artifacts", "Opening", "Player");
            Directory.CreateDirectory(folder);
            host = FindObjectOfType<MiniGameBootstrap>();
            if (host == null) { Finish(false, "Missing game host"); return; }
            host.Screen.DialogueCharactersPerSecond = 0;
        }

        private void Update()
        {
            if (done) { if (Time.realtimeSinceStartup >= nextAction) Application.Quit(success ? 0 : 1); return; }
            try { Tick(); }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void Tick()
        {
            if (Time.realtimeSinceStartup - began > 80) throw new TimeoutException("Day0 Player operation timed out");
            var view = host.Runtime.GetView();
            if (host.Screen.DayZeroOpeningWaiting)
            {
                Require(view.Game.Day == 0 && view.Cue?.Kind == PresentationKind.EnterTerminal, "Fresh opening checkpoint");
                Capture("monitor");
                if (Time.realtimeSinceStartup - began < 1) return;
                Require(host.Screen.RequestDayZeroOpening(), "Opening screen click");
                Require(!host.Screen.RequestDayZeroOpening(), "Repeated opening click");
                pullbackStart = Time.realtimeSinceStartup;
                return;
            }
            if (host.Effects.EntryPhase == TerminalEntryPhase.OpeningPullback)
            {
                Require(host.Screen.TransitionLocked && view.Transcript.Count == 0, "Pullback must lock tutorial input");
                return;
            }
            if (!host.Screen.TransitionLocked && view.TerminalOpen && view.TerminalPage == TerminalPage.Review)
            {
                Capture("terminal");
                if (Time.realtimeSinceStartup - pullbackStart < 4.2f || Time.realtimeSinceStartup < nextAction) return;
                var buttons = host.Screen.TerminalDisplay.GetComponentInParent<MiniGameScreen>().GetComponentsInChildren<Button>(false)
                    .Where(button => button.isActiveAndEnabled && button.interactable && button.GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace).ToArray();
                Require(buttons.Length <= 1, "Only one actionable Day0 terminal button");
                if (buttons.Length == 0) return;
                var button = buttons[0];
                Require(button.name == "Continue" || button.name == "Disposition-Purify", "Only tutorial Continue or Purify");
                if (pendingButton != button) { pendingButton = button; pendingFrame = Time.frameCount; return; }
                if (pendingFrame == Time.frameCount) return;
                if (button.name == "Disposition-Purify") { Capture("purify"); submitted = true; }
                Click(button); pendingButton = null; nextAction = Time.realtimeSinceStartup + .09f;
                return;
            }
            if (!view.TerminalOpen && BedtimeGuidance.IsSleepOnly(view) && !slept)
            {
                Require(submitted, "Tutorial must finish before sleep");
                Require(view.AllowedInteractions.SequenceEqual(new[] { InteractionTarget.Bed }), "Bed-only interactions");
                lines = view.Transcript.Count(line => !line.IsPlayer && line.Completed);
                Require(lines == 7 && view.Transcript.Count(line => line.SourceNodeId == "disposition:Purify") == 1, "Five intro, two reaction, one purification");
                var guidance = host.GetComponent<BedtimeGuidance>();
                if (guidance.BeamStrength < .98f) return;
                CaptureRoom("bed-only");
                Require(host.GetComponent<BedtimeCollisionGate>().Suppressing, "Furniture collision suppression");
                Require(host.Player.GetComponent<CharacterController>().enabled, "Player collision retained");
                slept = host.Runtime.TryInteract(InteractionTarget.Bed).Accepted;
                Require(slept && host.Runtime.GetView().Cue?.Kind == PresentationKind.Sleep, "Single sleep action");
            }
            if (slept && view.Game.Day == 1 && view.Cue == null)
            {
                Require(view.Game.Phase == GamePhase.MorningInspection, "Day1 morning");
                Require(!host.GetComponent<BedtimeCollisionGate>().Suppressing, "Furniture restored");
                CaptureRoom("day1"); Finish(true, "Fresh campaign opening, native UI tutorial and one sleep completed");
            }
        }

        private void Click(Button button)
        {
            var pointer = new PointerEventData(EventSystem.current) { position = host.Screen.TerminalDisplay.ScreenPointFor(button.GetComponent<RectTransform>()), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button, "Visible button must be the first pointer hit: " + button.name);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private void Capture(string name)
        {
            if (!captures.Add(name)) return;
            var rect = host.Screen.TerminalDisplay.OutputRect.rect;
            CaptureFrame(Path.Combine(folder, name + ".png"), 1280, Mathf.RoundToInt(1280 * rect.height / rect.width));
        }

        private void CaptureFrame(string path, int width, int height)
        {
            var display = host.Screen.TerminalDisplay; display.RenderFrame();
            var target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = target; GL.Clear(true, true, Color.black);
                float aspect = display.OutputRect.rect.width / display.OutputRect.rect.height;
                float drawWidth = Mathf.Min(width, height * aspect), drawHeight = drawWidth / aspect;
                GL.PushMatrix();
                try
                {
                    GL.LoadPixelMatrix(0, width, height, 0);
                    Graphics.DrawTexture(new Rect((width - drawWidth) / 2, (height - drawHeight) / 2, drawWidth, drawHeight),
                        display.Texture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.white, display.ScreenMaterial);
                }
                finally { GL.PopMatrix(); }
                WritePixels(target, path);
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); }
        }

        private void CaptureRoom(string name)
        {
            var camera = host.Player.ViewCamera;
            var previous = camera.targetTexture;
            var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            try { camera.targetTexture = target; camera.Render(); WritePixels(target, Path.Combine(folder, name + ".png")); }
            finally { camera.targetTexture = previous; RenderTexture.ReleaseTemporary(target); }
        }

        private static void WritePixels(RenderTexture target, string path)
        {
            var previous = RenderTexture.active;
            var pixels = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            try { RenderTexture.active = target; pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); pixels.Apply(); File.WriteAllBytes(path, pixels.EncodeToPNG()); }
            finally { RenderTexture.active = previous; Destroy(pixels); }
        }

        private void Finish(bool passed, string detail)
        {
            success = passed; done = true; nextAction = Time.realtimeSinceStartup + .3f;
            var view = host != null ? host.Runtime.GetView() : null;
            var report = new Report { result = passed ? "passed" : "failed", detail = detail, unity = Application.unityVersion,
                sourceLines = lines, day = view?.Game.Day ?? -1, singlePurification = submitted, bedOnly = slept,
                furnitureRestored = passed, seconds = Time.realtimeSinceStartup - began };
            File.WriteAllText(Path.Combine(folder, "verification.json"), JsonUtility.ToJson(report, true));
            Debug.Log("MINIGAME_OPENING_PLAYER_" + (passed ? "PASSED " : "FAILED ") + detail);
        }

        private static void Require(bool condition, string detail) { if (!condition) throw new InvalidOperationException(detail); }
    }
}
#endif

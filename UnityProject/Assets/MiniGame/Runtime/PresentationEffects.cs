using System.Collections;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Visual completion is acknowledged with the captured runtime cue token.</summary>
    public sealed class PresentationEffects : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform terminalCameraPose;
        [SerializeField] private Light apartmentLight;
        [SerializeField] private Light bedGuide;
        private GameRuntime runtime;
        private MiniGameScreen screen;
        private Coroutine playing;
        private string activeToken;
        private Vector3 roomPosition;
        private Quaternion roomRotation;
        private bool hasRoomPose;
        public void Configure(Camera camera, Transform terminalPose, Light ceilingLight, Light sleepGuide)
        { viewCamera = camera; terminalCameraPose = terminalPose; apartmentLight = ceilingLight; bedGuide = sleepGuide; }
        public void Bind(GameRuntime game, MiniGameScreen ui)
        {
            if (runtime != null) runtime.Changed -= Present;
            if (playing != null) StopCoroutine(playing);
            playing = null;
            runtime = game; screen = ui; activeToken = null;
            screen.SetTransitionLocked(false); screen.SetFade(0f); screen.SetGlitch(0f);
            if (hasRoomPose && !game.GetView().TerminalOpen)
            { viewCamera.transform.SetPositionAndRotation(roomPosition, roomRotation); hasRoomPose = false; }
            if (game.GetView().TerminalOpen)
            {
                if (!hasRoomPose)
                { roomPosition = viewCamera.transform.position; roomRotation = viewCamera.transform.rotation; hasRoomPose = true; }
                var kind = game.GetView().Cue?.Kind;
                if (kind != PresentationKind.EnterTerminal && kind != PresentationKind.ExitTerminal)
                    viewCamera.transform.SetPositionAndRotation(terminalCameraPose.position, terminalCameraPose.rotation);
            }
            runtime.Changed += Present;
            Present(runtime.GetView());
        }
        private void OnDestroy() { if (runtime != null) runtime.Changed -= Present; }

        private void Present(RuntimeView view)
        {
            bool night = view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.Sleeping
                || view.Game.Phase == GamePhase.Dream || view.Game.Phase == GamePhase.AfterDream;
            if (apartmentLight != null)
            {
                apartmentLight.intensity = night ? .6f : 1.8f;
                apartmentLight.color = night ? new Color(.43f, .54f, .67f) : new Color(.89f, .91f, .74f);
            }
            if (bedGuide != null) bedGuide.enabled = view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream;
            var cue = view.Cue;
            string token = cue?.Token;
            if (activeToken == token) return;
            activeToken = token;
            if (playing != null) { StopCoroutine(playing); playing = null; }
            screen.SetTransitionLocked(false);
            screen.SetFade(0f); screen.SetGlitch(0f);
            if (cue == null) return;
            switch (cue.Kind)
            {
                case PresentationKind.EnterTerminal:
                    if (!hasRoomPose)
                    { roomPosition = viewCamera.transform.position; roomRotation = viewCamera.transform.rotation; hasRoomPose = true; }
                    playing = StartCoroutine(CameraTransition(cue.Token, terminalCameraPose.position, terminalCameraPose.rotation, false));
                    break;
                case PresentationKind.ExitTerminal:
                    playing = view.UsesTerminalDirectory && !view.LeavingTerminal
                        ? StartCoroutine(TerminalSignalLoss(cue.Token))
                        : StartCoroutine(CameraTransition(cue.Token, hasRoomPose ? roomPosition : viewCamera.transform.position, hasRoomPose ? roomRotation : viewCamera.transform.rotation, true));
                    break;
                case PresentationKind.Sleep: playing = StartCoroutine(SleepTransition(cue.Token)); break;
                case PresentationKind.Reaction: playing = StartCoroutine(ReactionSignal(cue.Token)); break;
            }
        }

        private IEnumerator CameraTransition(string token, Vector3 destination, Quaternion rotation, bool exiting)
        {
            var source = viewCamera.transform.position;
            var sourceRotation = viewCamera.transform.rotation;
            screen.SetTransitionLocked(true);
            float elapsed = 0f;
            while (elapsed < .65f && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / .65f));
                viewCamera.transform.SetPositionAndRotation(Vector3.Lerp(source, destination, progress), Quaternion.Slerp(sourceRotation, rotation, progress));
                screen.SetGlitch(exiting ? 1f - progress : Mathf.Sin(progress * Mathf.PI) * .45f);
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            viewCamera.transform.SetPositionAndRotation(destination, rotation);
            if (exiting) hasRoomPose = false;
            screen.SetTransitionLocked(false); screen.SetGlitch(0f);
            playing = null;
            runtime.CompleteCue(token);
        }

        private IEnumerator SleepTransition(string token)
        {
            screen.SetTransitionLocked(true);
            float elapsed = 0f;
            while (elapsed < .8f && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                screen.SetFade(Mathf.Clamp01(elapsed / .8f));
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.SetTransitionLocked(false); screen.SetFade(0f);
            playing = null;
            runtime.CompleteCue(token);
        }

        private IEnumerator ReactionSignal(string token)
        {
            float elapsed = 0f;
            while (activeToken == token)
            {
                if (!runtime.GetView().Paused)
                {
                    elapsed += Time.unscaledDeltaTime;
                    screen.SetGlitch(elapsed < .45f ? .9f * (1f - elapsed / .45f) : .035f);
                }
                yield return null;
            }
        }

        private IEnumerator TerminalSignalLoss(string token)
        {
            screen.SetTransitionLocked(true);
            float elapsed = 0f;
            while (elapsed < .68f && activeToken == token)
            {
                if (runtime.GetView().Paused) { yield return null; continue; }
                elapsed += Time.unscaledDeltaTime;
                screen.SetGlitch(.85f);
                yield return null;
            }
            while (activeToken == token && runtime.GetView().Paused) yield return null;
            if (activeToken != token) yield break;
            screen.SetGlitch(0f); screen.SetTransitionLocked(false);
            playing = null;
            runtime.CompleteCue(token);
        }
    }
}

using System;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using MiniGame.Presentation.Authoring;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Reproducible visual preview using legal tutorial commands; never writes a player's save.</summary>
    [InitializeOnLoad]
    public static class BedtimePreview
    {
        private const string PendingKey = "MiniGame.BedtimePreview.Pending";
        static BedtimePreview() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        [MenuItem("MiniGame/Main Room/Preview bedtime lighting")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before opening the bedtime preview.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(MainRoomGameplayBuilder.ScenePath, OpenSceneMode.Single);
            SessionState.SetBool(PendingKey, true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
            SessionState.SetBool(PendingKey, false);
            var host = UnityEngine.Object.FindObjectOfType<MiniGameBootstrap>();
            var package = Resources.Load<DialoguePackageAsset>("MiniGame/ActiveCampaignPackage");
            if (host == null || package == null || SceneManager.GetActiveScene().path != MainRoomGameplayBuilder.ScenePath)
                throw new InvalidOperationException("The combined campaign room is required for this preview.");
            var compiled = DialoguePackageCompiler.Compile(package.PackageSource.text);
            var preview = new GameRuntime(compiled.Rules, compiled.Content, true);
            Require(preview.TryInteract(InteractionTarget.Computer));
            Require(preview.CompleteCue(preview.GetView().Cue.Token));
            Require(preview.EnterSelectedPatient(preview.GetView().Game.CurrentPatientId));
            Require(preview.CompleteCue(preview.GetView().Cue.Token));
            Require(preview.SubmitDisposition(preview.GetView().Game.CurrentEncounterId, Disposition.Purify));
            Require(preview.CompleteCue(preview.GetView().Cue.Token));
            Require(preview.CompleteCue(preview.GetView().Cue.Token));
            Require(preview.ReturnToDirectory());
            Require(preview.LeaveTerminal());
            Require(preview.CompleteCue(preview.GetView().Cue.Token));
            host.RestoreCheckpoint(preview.Save());
            var marker = GameObject.Find("Computer approach");
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = marker.transform.position;
            controller.enabled = true;
            Physics.SyncTransforms();
            var bed = UnityEngine.Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Bed);
            host.Player.FacePoint(bed.GetComponent<Collider>().bounds.center + Vector3.up * .9f);
            host.Player.RefreshInteractionFocus();
            Debug.Log("MINIGAME_BEDTIME_PREVIEW_READY phase=" + host.Runtime.GetView().Game.Phase + " no player save written");
        }

        private static void Require(RuntimeResult result)
        {
            if (!result.Accepted) throw new InvalidOperationException("Bedtime preview command rejected: " + result.Reason);
        }
    }
}

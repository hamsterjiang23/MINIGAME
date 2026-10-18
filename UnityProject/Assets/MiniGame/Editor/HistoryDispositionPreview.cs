using System;
using System.Collections;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Plays the actual Day0 introduction and leaves its history ready for a disposition.</summary>
    [InitializeOnLoad]
    public static class HistoryDispositionPreview
    {
        private const string PendingKey = "MiniGame.HistoryDispositionPreview.Pending";
        static HistoryDispositionPreview() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        [MenuItem("MiniGame/Main Room/Preview Day0 history disposition")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before opening the Day0 history preview.");
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
            if (host == null) throw new InvalidOperationException("The combined campaign room is required for this preview.");
            host.StartCoroutine(Prepare(host));
        }

        private static IEnumerator Prepare(MiniGameBootstrap host)
        {
            yield return null;
            host.StartCampaign();
            float typingSpeed = host.Screen.DialogueCharactersPerSecond;
            host.Screen.DialogueCharactersPerSecond = 0;
            try
            {
                Require(host.Runtime.TryInteract(InteractionTarget.Computer));
                float deadline = Time.realtimeSinceStartup + 20;
                while (Time.realtimeSinceStartup < deadline)
                {
                    var view = host.Runtime.GetView();
                    if (view.Input == InputContext.TerminalDirectory && !host.Screen.TransitionLocked)
                        Require(host.Runtime.EnterSelectedPatient("tutorial"));
                    else if (view.Input == InputContext.Dialogue && view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                    {
                        Button(host, "HistoryTab").onClick.Invoke();
                        Debug.Log("MINIGAME_HISTORY_DISPOSITION_PREVIEW_READY day=0 stage=AwaitingDisposition history="
                            + host.Screen.HistoryOpen + " available=" + string.Join(",", view.Game.AvailableDispositions)
                            + " no player save written");
                        yield break;
                    }
                    else
                    {
                        var next = Button(host, "Continue");
                        if (next != null) next.onClick.Invoke();
                    }
                    yield return null;
                }
                throw new InvalidOperationException("Day0 history preview did not finish its native Yarn introduction.");
            }
            finally { host.Screen.DialogueCharactersPerSecond = typingSpeed; }
        }

        private static Button Button(MiniGameBootstrap host, string name)
            => host.Screen.GetComponentsInChildren<Button>(false).FirstOrDefault(item => item.name == name && item.interactable);

        private static void Require(RuntimeResult result)
        {
            if (!result.Accepted) throw new InvalidOperationException("Day0 preview command rejected: " + result.Reason);
        }
    }
}

using System;
using System.Collections;
using System.IO;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class SupervisorSubtitlePreview
    {
        [MenuItem("MiniGame/Main Room/Preview supervisor subtitles %#F8")]
        public static void Open()
        {
            if (!EditorApplication.isPlaying)
                throw new InvalidOperationException("Enter Play Mode before previewing subtitles. Stop Play Mode to restore the scene.");
            var host = UnityEngine.Object.FindObjectOfType<MiniGameBootstrap>();
            if (host == null) throw new InvalidOperationException("Open MainRoomCombined first.");
            host.StartTerminalDemo();
            host.MainMenu.TryDismissForEditorDebug();
            host.Screen.SetMainMenuCovered(false);
            host.Screen.DialogueCharactersPerSecond = 0;
            host.StartCoroutine(Prepare(host));
        }

        private static IEnumerator Prepare(MiniGameBootstrap host)
        {
            yield return null;
            var door = UnityEngine.Object.FindObjectsOfType<WorldInteraction>().Single(item => item.Target == InteractionTarget.Door);
            var controller = host.Player.GetComponent<CharacterController>();
            controller.enabled = false;
            host.Player.transform.position = GameObject.Find("Door approach").transform.position;
            controller.enabled = true;
            host.Player.FacePoint(door.GetComponent<Collider>().bounds.center);
            Physics.SyncTransforms();
            host.Player.RefreshInteractionFocus();
            host.Player.InteractFocused();
            float deadline = Time.realtimeSinceStartup + 12;
            while (host.Runtime.GetView().Cue?.Kind != PresentationKind.MorningInspection && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (host.Runtime.GetView().Cue?.Kind != PresentationKind.MorningInspection)
                throw new InvalidOperationException("The doorway preview did not reach morning dialogue.");
            host.Player.enabled = false;
            host.Screen.SetDialogueText("", "你的电脑上有三个选项：\u201c释放\u201d、\u201c治疗\u201d和\u201c净化\u201d。");
            yield return new WaitForSecondsRealtime(.8f);
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Artifacts/DialogueUI"));
            Directory.CreateDirectory(folder);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "supervisor-subtitles.png"));
            Debug.Log("SUPERVISOR_SUBTITLE_PREVIEW_READY " + folder);
        }
    }
}

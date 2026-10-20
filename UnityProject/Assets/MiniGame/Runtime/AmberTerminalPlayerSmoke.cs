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
    /// <summary>Opt-in standalone-player validation. Absent unless --terminal-amber-smoke is supplied.</summary>
    public sealed class AmberTerminalPlayerSmoke : MonoBehaviour
    {
        private MiniGameBootstrap host;
        private float began, nextClick, quitAt;
        private int cycle;
        private bool submitted, finished, removedKeysVerified, directoryPixels, reviewPixels;
        private bool initiallyOff, middayLit, reopenedWithoutBoot, finallyDark;
        private int powerOnSounds, powerOffSounds;
        private Renderer physicalScreen;
        private MiniGameAudioService observedAudio;
        private Button pendingButton;
        private int pendingFrame;
        private string folder;
        private readonly HashSet<string> captures=new HashSet<string>();
        [Serializable] private sealed class Report {public string result,detail,unity,captureMode;public int replies,quota,powerOnSounds,powerOffSounds;public bool manualReopen,automaticShutdown,removedFunctionKeys,directoryPixels,reviewPixels,initiallyOff,middayLit,reopenedWithoutBoot,finallyDark;public float seconds;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if(Environment.GetCommandLineArgs().Contains("--terminal-amber-smoke"))new GameObject("Amber standalone smoke").AddComponent<AmberTerminalPlayerSmoke>();
        }
        private void Start()
        {
            began=Time.realtimeSinceStartup;
            string root=Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            folder=Path.Combine(string.IsNullOrEmpty(root)?Application.persistentDataPath:root,"Artifacts","TerminalAmber","Player");
            Directory.CreateDirectory(folder);
            host=FindObjectOfType<MiniGameBootstrap>();
            if(host==null){Finish(false,"No game host in the built main scene.");return;}
            host.StartTerminalDemo();
            physicalScreen=FindObjectsOfType<MeshRenderer>().SingleOrDefault(item=>item.name.StartsWith("CRT screen emission",StringComparison.Ordinal));
            initiallyOff=PhysicalPower(false);Require(initiallyOff,"The working day must begin with the physical CRT off");
            observedAudio=host.Audio.Service;observedAudio.SoundPlayed+=ObserveSound;
        }
        private void Update()
        {
            if(finished){if(Time.realtimeSinceStartup>=quitAt)Application.Quit(cycle==3?0:1);return;}
            try {Tick();}catch(Exception error){Finish(false,error.ToString());}
        }
        private void Tick()
        {
            if(Time.realtimeSinceStartup-began>100){Finish(false,"Player operation timeout.");return;}
            var runtime=host.Runtime;var view=runtime.GetView();
            if(host.Screen.TerminalReveal>=.999f)
            {
                var phase=host.Screen.PowerPhase;
                if(phase==TerminalPowerPhase.Boot)Capture("boot");
                if(phase==TerminalPowerPhase.Loading)Capture("loading");
                if(phase==TerminalPowerPhase.Safe)Capture("shutdown");
                if(phase==TerminalPowerPhase.Collapse)Capture("collapse");
            }
            if(Time.realtimeSinceStartup<nextClick)return;
            nextClick=Time.realtimeSinceStartup+.045f;
            if(view.Input==InputContext.World)
            {
                if(submitted&&view.Game.Phase==GamePhase.Evening&&!view.TerminalOpen)
                {
                    Require(view.Transcript.Count(t=>t.IsPlayer&&!t.SourceNodeId.StartsWith("disposition:",StringComparison.Ordinal))==6,"Six actual replies must be retained");
                    Require(view.Game.RemainingPurificationQuota==5,"Treat must not consume purification quota");
                    Require(view.Patients.Single(p=>p.Id=="child").HasMetToday,"The child must be archived");
                    Require(directoryPixels&&reviewPixels,"Player GPU output must contain the actual terminal directory and monitor UI");
                    Require(view.TerminalPower==TerminalPowerState.Off,"Final daily shutdown must persist Off");
                    finallyDark=PhysicalPower(false);Require(finallyDark,"Final daily shutdown must darken the physical CRT");
                    Require(powerOnSounds==1&&powerOffSounds==1,"Exactly one boot sound and one shutdown sound must play in the day");
                    Require(middayLit&&reopenedWithoutBoot,"Midday UI exit/reentry must preserve the powered session");
                    cycle=3;Capture("completed-world");Finish(true,"Mapped UI clicks completed midday UI exit/reentry with the physical CRT lit, then the six-round child encounter, reaction and final daily shutdown; power sounds played once each.");return;
                }
                if(cycle==1)
                {
                    middayLit=view.TerminalPowered&&PhysicalPower(true);Require(middayLit,"Midday UI exit must keep the physical CRT lit");
                    Require(powerOnSounds==1&&powerOffSounds==0,"Midday UI exit must not replay power sounds");
                    cycle=2;Require(runtime.TryInteract(InteractionTarget.Computer).Accepted,"Reopen during the same work day");
                    reopenedWithoutBoot=!runtime.GetView().TerminalBootRequired;Require(reopenedWithoutBoot,"Reentry must resume the powered session without boot");return;
                }
                if(view.Game.Phase==GamePhase.MorningInspection)Require(runtime.TryInteract(InteractionTarget.Door).Accepted,"Morning door");
                else if(view.AllowedInteractions.Contains(InteractionTarget.Computer))Require(runtime.TryInteract(InteractionTarget.Computer).Accepted,"Computer entry");
                return;
            }
            if(view.Input==InputContext.TerminalDirectory&&view.Cue==null)
            {
                Capture("directory");
                Require(!host.Screen.HandleTerminalKey(KeyCode.F2)&&!host.Screen.HandleTerminalKey(KeyCode.F3),"F2/F3 must remain removed");
                removedKeysVerified=true;
                if(cycle==0){if(Click(Find("LeaveTerminal")))cycle=1;return;}
                Require(host.Screen.PowerPhase==TerminalPowerPhase.Running&&powerOnSounds==1&&powerOffSounds==0,"Reopened UI must remain Running without another power sequence");
                Click(Find("EnterReview"));return;
            }
            if(view.Input==InputContext.Dialogue&&view.Game.EncounterStage==EncounterStage.AwaitingDisposition)
            {Capture("measure");if(Click(Find("Disposition-Treat")))submitted=true;return;}
            var next=Find("Continue");
            if(next!=null){Click(next);return;}
            var reply=host.Screen.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name.StartsWith("Reply-",StringComparison.Ordinal)&&b.IsActive()&&b.IsInteractable());
            if(reply!=null){Capture("review");Click(reply);}
        }
        private Button Find(string name)=>host.Screen.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name==name&&b.IsActive()&&b.IsInteractable());
        private bool Click(Button button)
        {
            Require(button!=null,"Expected active UI command");
            if(pendingButton!=button){pendingButton=button;pendingFrame=Time.frameCount;return false;}
            if(Time.frameCount<=pendingFrame)return false;
            pendingButton=null;Canvas.ForceUpdateCanvases();
            var rectangle=(RectTransform)button.transform;
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,clickCount=1,position=host.Screen.TerminalDisplay.ScreenPointFor(rectangle)};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"Top raycast must target "+button.name+" position="+pointer.position+" depth="+button.GetComponent<Image>().depth+" hits="+string.Join(";",hits.Select(h=>h.gameObject.name+":"+h.depth)));
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,pointer,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,pointer,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,pointer,ExecuteEvents.pointerClickHandler);
            return true;
        }
        private void Capture(string name)
        {
            if(!captures.Add(name)||host==null||!host.Runtime.GetView().TerminalOpen)return;
            var display=host.Screen.TerminalDisplay;display.RenderFrame();
            var target=new RenderTexture(1600,1200,0,RenderTextureFormat.ARGB32);target.Create();
            var image=new Texture2D(1600,1200,TextureFormat.RGBA32,false);
            var previous=RenderTexture.active;
            try
            {
                Graphics.Blit(display.Texture,target,display.ScreenMaterial);RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1600,1200),0,0);image.Apply();
                int lit=image.GetPixels32().Count(p=>p.r>80&&p.r>p.b);
                if(name=="directory"||name=="review")Require(lit>1000,"Empty Player GPU terminal output: "+name+" lit="+lit);
                if(name=="directory")directoryPixels=true;if(name=="review")reviewPixels=true;
                File.WriteAllBytes(Path.Combine(folder,name+"-terminal.png"),image.EncodeToPNG());
            }
            finally {RenderTexture.active=previous;target.Release();Destroy(target);Destroy(image);}
        }
        private static void Require(bool accepted,string message){if(!accepted)throw new InvalidOperationException(message);}
        private void ObserveSound(MiniGameAudioChannel channel,MiniGameSound sound)
        {
            if(sound==MiniGameSound.TerminalPowerOn)powerOnSounds++;
            if(sound==MiniGameSound.TerminalPowerOff)powerOffSounds++;
        }
        private bool PhysicalPower(bool lit)
        {
            if(physicalScreen==null)return false;
            var properties=new MaterialPropertyBlock();physicalScreen.GetPropertyBlock(properties,0);
            if(!properties.HasProperty(Shader.PropertyToID("_CrtPower")))return false;
            float power=properties.GetFloat("_CrtPower");
            float glow=properties.GetColor("_EmissionColor").maxColorComponent;
            return lit?power>.99f&&glow>.1f:power<.001f&&glow<.001f;
        }
        private void OnDestroy(){if(observedAudio!=null)observedAudio.SoundPlayed-=ObserveSound;}
        private void Finish(bool success,string detail)
        {
            var view=host!=null?host.Runtime.GetView():null;
            var report=new Report{result=success?"Passed":"Failed",detail=detail,unity=Application.unityVersion,captureMode="Standalone GPU native UI RenderTexture + actual CRT material; physical CRT power property blocks and real audio service events; hidden-window backbuffer is not used",directoryPixels=directoryPixels,reviewPixels=reviewPixels,manualReopen=cycle>=2,automaticShutdown=success,removedFunctionKeys=removedKeysVerified,replies=view?.Transcript.Count(t=>t.IsPlayer&&!t.SourceNodeId.StartsWith("disposition:",StringComparison.Ordinal))??0,quota=view?.Game.RemainingPurificationQuota??-1,powerOnSounds=powerOnSounds,powerOffSounds=powerOffSounds,initiallyOff=initiallyOff,middayLit=middayLit,reopenedWithoutBoot=reopenedWithoutBoot,finallyDark=finallyDark,seconds=Time.realtimeSinceStartup-began};
            File.WriteAllText(Path.Combine(folder,"verification.json"),JsonUtility.ToJson(report,true));
            Debug.Log("MINIGAME_AMBER_PLAYER_"+report.result.ToUpperInvariant()+" "+detail);
            if(!success)cycle=-1;finished=true;quitAt=Time.realtimeSinceStartup+.4f;
        }
    }
}
#endif

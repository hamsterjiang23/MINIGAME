#if DEVELOPMENT_BUILD || UNITY_EDITOR || MINIGAME_FEEDBACK_PROBE
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
        private bool middayLiveUi, middaySourceUpdating, middayPixels, middayWorldPickingBlocked;
        private bool liveAfterPause, liveAfterRestore, sourceStoppedAfterShutdown;
        private bool orientationAfterExit, orientationAfterRestore;
        private int middayStage, middayFrameCount, shutdownFrameCount;
        private float middayStageStarted, middayPersistenceSeconds, shutdownStarted = -1;
        private int powerOnSounds, powerOffSounds;
        private int bootTypingSounds, shutdownTypingSounds, invalidTypingSounds;
        private bool typingSourceObservedPlaying;
        private readonly HashSet<MiniGameSound> typingVariants = new HashSet<MiniGameSound>();
        private Renderer physicalScreen;
        private MiniGameAudioService observedAudio;
        private Button pendingButton;
        private int pendingFrame;
        private string folder;
        private readonly HashSet<string> captures=new HashSet<string>();
        [Serializable] private sealed class Report {public string result,detail,unity,captureMode;public int replies,quota,powerOnSounds,powerOffSounds,bootTypingSounds,shutdownTypingSounds,invalidTypingSounds,typingVariants;public bool manualReopen,automaticShutdown,removedFunctionKeys,directoryPixels,reviewPixels,initiallyOff,middayLit,reopenedWithoutBoot,finallyDark,middayLiveUi,middaySourceUpdating,middayPixels,middayWorldPickingBlocked,liveAfterPause,liveAfterRestore,sourceStoppedAfterShutdown,orientationAfterExit,orientationAfterRestore,typingSourceObservedPlaying;public float seconds,middayPersistenceSeconds;}
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
            CapturePhysical("physical-off");
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
            if(cycle==1&&!view.TerminalOpen){VerifyMiddayPersistence();return;}
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
                    if(shutdownStarted<0){shutdownStarted=Time.realtimeSinceStartup;shutdownFrameCount=host.Screen.TerminalDisplay.RenderedFrameCount;return;}
                    if(Time.realtimeSinceStartup-shutdownStarted<.3f)return;
                    sourceStoppedAfterShutdown=!SourceIsActive()&&host.Screen.TerminalDisplay.RenderedFrameCount==shutdownFrameCount;
                    Require(sourceStoppedAfterShutdown,"Final shutdown must disable the source and stop rendering new terminal frames");
                    Require(powerOnSounds==1&&powerOffSounds==1,"Exactly one boot sound and one shutdown sound must play in the day");
                    Require(bootTypingSounds>250&&shutdownTypingSounds==47&&typingVariants.Count==4&&invalidTypingSounds==0,
                        "Both power consoles must type through the four uploaded variants without paused or unrelated sounds");
                    Require(middayLit&&reopenedWithoutBoot,"Midday UI exit/reentry must preserve the powered session");
                    cycle=3;Capture("completed-world");Finish(true,"Mapped UI clicks verified live nonuniform UI for at least two seconds after world exit, pause/resume and checkpoint restore, with hidden UI picking blocked; then completed six replies and final shutdown with source rendering stopped and one sound per power sequence.");return;
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
        private void VerifyMiddayPersistence()
        {
            var display=host.Screen.TerminalDisplay;
            float elapsed=Time.realtimeSinceStartup-middayStageStarted;
            if(middayStage==0)
            {
                middayStageStarted=Time.realtimeSinceStartup;middayFrameCount=display.RenderedFrameCount;middayStage=1;return;
            }
            if(middayStage==2)
            {
                if(elapsed<.3f)return;
                Require(LivePhysicalUi()&&SourceIsActive(),"Pausing in world must preserve the live physical UI source");
                Require(host.Runtime.SetPaused(false).Accepted,"Resume the world");
                middayStageStarted=Time.realtimeSinceStartup;middayFrameCount=display.RenderedFrameCount;middayStage=3;return;
            }
            if(elapsed<2)return;
            Require(display.RenderedFrameCount>middayFrameCount+2,"Powered world CRT source must continue rendering over two seconds");
            Require(LivePhysicalUi()&&SourceIsActive(),"Powered world CRT must retain the live UI texture and active source");
            Require(HasUiPixels(),"Powered CRT texture must contain nonuniform amber UI pixels, not a flat emissive color");
            Require(WorldPickingBlocked(),"World mode must not pick hidden terminal UI");
            if(middayStage==1)
            {
                middayPersistenceSeconds=elapsed;
                middayLit=host.Runtime.GetView().TerminalPowered&&PhysicalPower(true);Require(middayLit,"Midday UI exit must keep the physical CRT lit");
                middayLiveUi=middaySourceUpdating=middayPixels=middayWorldPickingBlocked=true;
                CapturePhysical("physical-midday-on");
                orientationAfterExit=CrtUiOrientationProbe.CaptureAndVerify(host,physicalScreen,Path.Combine(folder,"physical-orientation-midday.png"));
                Require(orientationAfterExit,"Physical CRT must preserve top/bottom and left/right UI orientation after exit");
                Require(host.Runtime.SetPaused(true).Accepted,"Pause in world after terminal exit");
                middayStageStarted=Time.realtimeSinceStartup;middayStage=2;return;
            }
            if(middayStage==3)
            {
                liveAfterPause=true;
                host.RestoreCheckpoint(host.Runtime.Save());
                middayStageStarted=Time.realtimeSinceStartup;middayFrameCount=display.RenderedFrameCount;middayStage=4;return;
            }
            liveAfterRestore=true;
            CapturePhysical("physical-midday-restored");
            orientationAfterRestore=CrtUiOrientationProbe.CaptureAndVerify(host,physicalScreen,Path.Combine(folder,"physical-orientation-restored.png"));
            Require(orientationAfterRestore,"Physical CRT must preserve top/bottom and left/right UI orientation after restore");
            Require(powerOnSounds==1&&powerOffSounds==0,"Midday exit, pause and restore must not replay power sounds");
            cycle=2;Require(host.Runtime.TryInteract(InteractionTarget.Computer).Accepted,"Reopen during the same work day");
            reopenedWithoutBoot=!host.Runtime.GetView().TerminalBootRequired;Require(reopenedWithoutBoot,"Reentry must resume the powered session without boot");
        }
        private bool SourceIsActive()
        {
            var camera=host.Screen.GetComponentsInChildren<Camera>(true).Single(item=>item.name=="Terminal UI camera");
            return camera.gameObject.activeInHierarchy&&camera.targetTexture==host.Screen.TerminalDisplay.Texture;
        }
        private bool LivePhysicalUi()
        {
            var properties=new MaterialPropertyBlock();physicalScreen.GetPropertyBlock(properties,0);
            return properties.GetFloat("_LiveDisplay")>.99f&&properties.GetTexture("_LiveDisplayTex")==host.Screen.TerminalDisplay.Texture
                &&!host.Screen.TerminalDisplay.OutputRect.gameObject.activeInHierarchy
                &&host.Screen.GetComponentsInChildren<Transform>(true).Single(item=>item.name=="ObjectDirectory").gameObject.activeInHierarchy;
        }
        private bool HasUiPixels()
        {
            var texture=host.Screen.TerminalDisplay.Texture;
            var image=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            var previous=RenderTexture.active;
            try
            {
                RenderTexture.active=texture;image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);image.Apply();
                var pixels=image.GetPixels32();
                return pixels.Count(p=>p.r>80&&p.r>p.b+20)>1000&&pixels.Count(p=>p.r<40&&p.g<40&&p.b<40)>1000;
            }
            finally{RenderTexture.active=previous;Destroy(image);}
        }
        private bool WorldPickingBlocked()
        {
            var button=host.Screen.GetComponentsInChildren<Button>(true).Single(item=>item.name=="EnterReview");
            var pointer=new PointerEventData(EventSystem.current){position=host.Screen.TerminalDisplay.ScreenPointFor((RectTransform)button.transform)};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            var source=host.Screen.GetComponentsInChildren<Camera>(true).Single(item=>item.name=="Terminal UI camera").transform.parent;
            return !hits.Any(hit=>hit.gameObject.transform.IsChildOf(source));
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
        private void CapturePhysical(string name)
        {
            var markers=physicalScreen.transform.root.GetComponentsInChildren<Transform>(true);
            var origin=markers.Single(t=>t.name=="Unity_ReferenceOrigin").position;
            var front=(markers.Single(t=>t.name=="Unity_ReferenceDepth").position-origin).normalized;
            var right=(markers.Single(t=>t.name=="Unity_ReferenceRight").position-origin).normalized;
            var cameraObject=new GameObject("Physical CRT smoke capture");
            var camera=cameraObject.AddComponent<Camera>();camera.CopyFrom(host.Player.ViewCamera);camera.enabled=false;
            camera.fieldOfView=45;camera.nearClipPlane=.01f;
            camera.transform.position=physicalScreen.bounds.center+front*.58f+right*.5f+Vector3.up*.18f;
            camera.transform.LookAt(physicalScreen.bounds.center);
            var target=RenderTexture.GetTemporary(1280,960,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;var image=new Texture2D(1280,960,TextureFormat.RGBA32,false);
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
            }
            finally {camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Destroy(image);Destroy(cameraObject);}
        }
        private void ObserveSound(MiniGameAudioChannel channel,MiniGameSound sound)
        {
            if(sound==MiniGameSound.TerminalPowerOn)powerOnSounds++;
            if(sound==MiniGameSound.TerminalPowerOff)powerOffSounds++;
            if(channel!=MiniGameAudioChannel.TerminalTyping)return;
            typingVariants.Add(sound);
            if(host.Runtime.GetView().Paused||sound<MiniGameSound.TerminalType1||sound>MiniGameSound.TerminalType4)invalidTypingSounds++;
            if(host.Screen.PowerPhase==TerminalPowerPhase.Boot)bootTypingSounds++;
            else if(host.Screen.PowerPhase==TerminalPowerPhase.Shutdown)shutdownTypingSounds++;
            else invalidTypingSounds++;
            typingSourceObservedPlaying|=observedAudio.GetSource(MiniGameAudioChannel.TerminalTyping).isPlaying;
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
            var report=new Report{result=success?"Passed":"Failed",detail=detail,unity=Application.unityVersion,captureMode="Standalone GPU native UI RenderTexture + actual CRT material; persistent source frames and UI pixels, asymmetric source-canvas markers through physical CRT shader, pause/restore, raycast isolation, power blocks and audio events",directoryPixels=directoryPixels,reviewPixels=reviewPixels,manualReopen=cycle>=2,automaticShutdown=success,removedFunctionKeys=removedKeysVerified,replies=view?.Transcript.Count(t=>t.IsPlayer&&!t.SourceNodeId.StartsWith("disposition:",StringComparison.Ordinal))??0,quota=view?.Game.RemainingPurificationQuota??-1,powerOnSounds=powerOnSounds,powerOffSounds=powerOffSounds,bootTypingSounds=bootTypingSounds,shutdownTypingSounds=shutdownTypingSounds,invalidTypingSounds=invalidTypingSounds,typingVariants=typingVariants.Count,typingSourceObservedPlaying=typingSourceObservedPlaying,initiallyOff=initiallyOff,middayLit=middayLit,reopenedWithoutBoot=reopenedWithoutBoot,finallyDark=finallyDark,middayLiveUi=middayLiveUi,middaySourceUpdating=middaySourceUpdating,middayPixels=middayPixels,middayWorldPickingBlocked=middayWorldPickingBlocked,liveAfterPause=liveAfterPause,liveAfterRestore=liveAfterRestore,sourceStoppedAfterShutdown=sourceStoppedAfterShutdown,orientationAfterExit=orientationAfterExit,orientationAfterRestore=orientationAfterRestore,middayPersistenceSeconds=middayPersistenceSeconds,seconds=Time.realtimeSinceStartup-began};
            File.WriteAllText(Path.Combine(folder,"verification.json"),JsonUtility.ToJson(report,true));
            Debug.Log("MINIGAME_AMBER_PLAYER_"+report.result.ToUpperInvariant()+" "+detail);
            if(!success)cycle=-1;finished=true;quitAt=Time.realtimeSinceStartup+
                (Environment.GetCommandLineArgs().Contains("--wall-lighting-smoke")?2f:.4f);
        }
    }
}
#endif

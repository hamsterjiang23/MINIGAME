using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PMSAmber.Editor
{
    [InitializeOnLoad]
    public static class PMSPackagePlayValidation
    {
        private const string Flag="PMS.PackageValidation.Active";
        private static double deadline,shutdownAt;
        private static PMSTerminalView view;
        private static string failure;
        static PMSPackagePlayValidation()
        {if(SessionState.GetBool(Flag,false)){deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/PMSAmberTerminal/Scenes/PMSAmberDemo.unity");
            SessionState.SetBool(Flag,true);SessionState.SetInt(Flag+".Stage",0);deadline=EditorApplication.timeSinceStartup+90;
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;EditorApplication.isPlaying=true;
        }
        private static void Log(string message,string trace,LogType type)
        {if(type==LogType.Exception||type==LogType.Error)failure=message;}
        private static void Tick()
        {
            if(!SessionState.GetBool(Flag,false))return;
            try
            {
                if(failure!=null)throw new Exception(failure);
                if(!EditorApplication.isPlaying)return;
                if(!view)view=UnityEngine.Object.FindObjectOfType<PMSTerminalView>();if(!view)return;view.ReadKeyboardAndMouse=false;
                double now=EditorApplication.timeSinceStartup;int stage=SessionState.GetInt(Flag+".Stage",0);
                if(stage==0)
                {
                    if(now>deadline)throw new Exception("Boot/load timeout.");
                    if(view.Phase==PMSTerminalView.PowerPhase.Running){SessionState.SetInt(Flag+".Stage",1);deadline=now+.7;}
                }
                else if(stage==1&&now>deadline)
                {
                    Capture("Unity_Directory");view.HandleKey("Enter");
                    if(view.Phase!=PMSTerminalView.PowerPhase.Running)throw new Exception("Normal page switch started a power transition.");
                    SessionState.SetInt(Flag+".Stage",2);deadline=now+.7;
                }
                else if(stage==2&&now>deadline)
                {
                    Capture("Unity_Review");
                    if(!view.Glyphs.Covers('对',14))throw new Exception("Chinese glyph did not load.");
                    view.HandleKey("Enter");view.HandleKey("Enter");view.HandleKey("1");view.HandleKey("Enter");
                    if(view.Phase!=PMSTerminalView.PowerPhase.Shutdown)throw new Exception("Demo disposition did not request shutdown.");
                    shutdownAt=now;SessionState.SetInt(Flag+".Stage",3);deadline=now+15;
                }
                else if(stage==3)
                {
                    if(now-shutdownAt>3.35&&now-shutdownAt<3.8&&!File.Exists(Path.Combine(Application.dataPath,"../Unity_PowerOff.png")))Capture("Unity_PowerOff");
                    if(now>deadline)throw new Exception("Shutdown timeout.");
                    if(view.Phase==PMSTerminalView.PowerPhase.Off)
                    {
                        view.enabled=false;view.enabled=true;
                        if(view.Phase!=PMSTerminalView.PowerPhase.Boot)throw new Exception("Reopening did not replay boot.");
                        SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;
                        File.WriteAllText(Path.Combine(Application.dataPath,"../play-validation.json"),"{\"unity\":\""+Application.unityVersion+"\",\"playMode\":true,\"bootAndRegionalLoading\":true,\"normalSwitchNoLoading\":true,\"nativeChineseText\":true,\"demoEvents\":true,\"shutdown\":true,\"reopenBoot\":true,\"runtimeErrors\":0}");
                        Debug.Log("PMS_PACKAGE_PLAY_PASS");EditorApplication.Exit(0);
                    }
                }
            }
            catch(Exception error)
            {SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Debug.LogException(error);EditorApplication.Exit(1);}
        }
        private static void Capture(string name)
        {
            var temporary=RenderTexture.GetTemporary(800,600,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(view.ScreenTexture,temporary,view.ScreenMaterial);
            var old=RenderTexture.active;RenderTexture.active=temporary;var image=new Texture2D(800,600,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,800,600),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Application.dataPath,"../"+name+".png"),image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=old;RenderTexture.ReleaseTemporary(temporary);
        }
    }
}

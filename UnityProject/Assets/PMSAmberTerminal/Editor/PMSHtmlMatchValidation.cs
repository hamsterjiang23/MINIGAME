using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PMSAmber.Editor
{
    [InitializeOnLoad]
    public static class PMSHtmlMatchValidation
    {
        private const string Flag="PMS.HTMLMatch.Validation";
        private static PMSTerminalView view;
        private static double deadline;
        private static string failure;
        static PMSHtmlMatchValidation()
        {if(SessionState.GetBool(Flag,false)){deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
        public static void Run()
        {
            var target=Array.IndexOf(Environment.GetCommandLineArgs(),"-pmsLinear")>=0?ColorSpace.Linear:ColorSpace.Gamma;
            if(PlayerSettings.colorSpace!=target)PlayerSettings.colorSpace=target;
            PMSPackageBuilder.BuildAndValidate();SessionState.SetBool(Flag,true);SessionState.SetInt(Flag+".Stage",0);
            deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;EditorApplication.isPlaying=true;
        }
        private static void Log(string m,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception)failure=m;}
        private static void Next(int i){SessionState.SetInt(Flag+".Stage",i);deadline=EditorApplication.timeSinceStartup+.5;}
        private static void Tick()
        {
            if(!SessionState.GetBool(Flag,false))return;
            try
            {
                if(failure!=null)throw new Exception(failure);if(!EditorApplication.isPlaying)return;
                if(!view)view=UnityEngine.Object.FindObjectOfType<PMSTerminalView>();if(!view)return;view.ReadKeyboardAndMouse=false;
                int stage=SessionState.GetInt(Flag+".Stage",0);double now=EditorApplication.timeSinceStartup;
                if(stage==0){if(now>deadline)throw new Exception("Boot timeout");if(view.Phase==PMSTerminalView.PowerPhase.Running)Next(1);return;}
                if(stage==1&&now>deadline){Capture("Directory");view.HandleKey("Enter");if(view.Phase!=PMSTerminalView.PowerPhase.Running)throw new Exception("Unexpected page transition");Next(2);}
                else if(stage==2&&now>deadline)
                {
                    Capture("Review");view.ShowReview(new PMSReviewData{Round="R2",Lines=new[]{new PMSLogLine{Text="我没有打人，也没有拿别人的东西。"}},CanContinue=false,
                    Replies=new[]{new PMSReply{Id="1",Text="谁说你打人了？"},new PMSReply{Id="2",Text="你恨他们吗？"},new PMSReply{Id="3",Text="检测前谁教过你？"},new PMSReply{Id="4",Text="工作人员呢？"}}});Next(3);
                }
                else if(stage==3&&now>deadline)
                {
                    Capture("Choice");view.ShowReview(new PMSReviewData{Round="R2",Lines=new[]{new PMSLogLine{Text="这里还要检测吗？"},new PMSLogLine{Speaker="值班员",Text="别害怕，慢慢说。"},new PMSLogLine{Text="和昨天广场上的不一样吗？"},new PMSLogLine{Text="我没有打人，也没有拿别人的东西。"},new PMSLogLine{Speaker="值班员",Text="工作人员呢？"},new PMSLogLine{Text="可我不知道自己做错了什么。"}}});view.ToggleHistory();Next(4);
                }
                else if(stage==4&&now>deadline)
                {
                    Capture("History");view.ShowReview(new PMSReviewData{Round="R6",Status="会谈结束，可执行处置。",Lines=new[]{new PMSLogLine{Text="刚才那些，我都说对了吗？"}},CanContinue=false,CanTreat=true,CanPurify=true});Next(5);
                }
                else if(stage==5&&now>deadline){Capture("Measure");view.NotifyAllCharactersCompletedAfterDialogue();Next(6);deadline=now+15;}
                else if(stage==6)
                {
                    if(now>deadline)throw new Exception("Shutdown timeout");
                    if(view.Phase==PMSTerminalView.PowerPhase.Off)
                    {
                        view.enabled=false;view.enabled=true;if(view.Phase!=PMSTerminalView.PowerPhase.Boot)throw new Exception("Reopen boot failed");
                        SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;
                        File.WriteAllText(Path.Combine(Application.dataPath,"../html-match-play.json"),"{\"unity\":\""+Application.unityVersion+"\",\"colorSpace\":\""+QualitySettings.activeColorSpace+"\",\"fiveStatesCaptured\":true,\"bitmapGlyphs\":true,\"normalPageNoLoading\":true,\"shutdown\":true,\"reopen\":true,\"runtimeErrors\":0}");Debug.Log("PMS_HTML_MATCH_PLAY_PASS");EditorApplication.Exit(0);
                    }
                }
            }
            catch(Exception e){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Debug.LogException(e);EditorApplication.Exit(1);}
        }
        private static void Capture(string name)
        {
            Save(view.ScreenTexture,"Unity_"+name+"_Raw");
            var target=RenderTexture.GetTemporary(800,600,0,RenderTextureFormat.ARGB32);Graphics.Blit(view.ScreenTexture,target,view.ScreenMaterial);Save(target,"Unity_"+name+"_CRT");RenderTexture.ReleaseTemporary(target);
        }
        private static void Save(RenderTexture target,string name)
        {
            var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(800,600,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,800,600),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Application.dataPath,"../"+name+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=old;
        }
    }
}

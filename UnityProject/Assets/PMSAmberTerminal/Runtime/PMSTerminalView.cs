using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PMSAmber
{
    /// <summary>Presentation adapter only. GameSession/Yarn remains the authority for all game rules.</summary>
    public sealed class PMSTerminalView : MonoBehaviour
    {
        public enum PowerPhase { Boot, Loading, Running, Shutdown, Off }
        [Header("Exported assets")]
        public PMSBitmapFont Glyphs;
        public Texture2D Ward, GlowLine, GlowDot;
        public Shader DisplayShader;
        [Header("Display")]
        public bool AutoBoot = true, ReadKeyboardAndMouse = true, ShowOverlay = true;
        public string QuotaDisplay = "05";
        [Range(0,2)] public int CrtMode = 1;
        [Range(0,31)] public int SourceLayer = 31;
        [Header("Forward these requests to the existing game runtime")]
        public UnityEvent<string> EnterRequested = new UnityEvent<string>();
        public UnityEvent ContinueRequested = new UnityEvent();
        public UnityEvent<string> ReplyRequested = new UnityEvent<string>();
        public UnityEvent<string> DispositionRequested = new UnityEvent<string>();
        public UnityEvent ShutdownCompleted = new UnityEvent();
        public PowerPhase Phase { get; private set; }
        public RenderTexture ScreenTexture { get; private set; }
        public Material ScreenMaterial { get; private set; }

        private static Color Hex(string value) { Color c; ColorUtility.TryParseHtmlString("#" + value, out c); return c; }
        private static readonly Color Desk=Hex("0b0803"), Ink=Hex("efb83f"), Bright=Hex("ffcf68"), Dim=Hex("a77d2d"), Faint=Hex("674b19"), Edge=Hex("d9a339"), Select=Hex("eeb430"), Inverse=Hex("170e02");
        private RectTransform sourceRoot, frame, display;
        private Camera sourceCamera;
        private GameObject surface;
        private bool built, reviewPage, history;
        private int selectedRoster, selectedReply, historyOffset;
        private int selectedDisposition=1;
        private PMSRosterEntry[] roster = { new PMSRosterEntry() };
        private PMSReviewData review = new PMSReviewData();
        private readonly List<Hit> hits = new List<Hit>();
        private string pressed, hovered, lastRosterId;
        private float lastRosterClick=-10;
        private GameObject consoleCursor;
        private float cursorClock;
        private sealed class Hit { public string Id; public Rect Area; public Action Action; }
        private readonly List<GameObject> startupParts = new List<GameObject>();
        private static readonly int[] LoadAt = {120,280,440,500,560,620,680,800,960,1020,1080,1140,1200,1360};

        private void Start()
        {
            if(!Glyphs)Glyphs=Resources.Load<PMSBitmapFont>("PMSAmberTerminal/HTMLBitmapFont");
            if (!Glyphs || !Glyphs.Atlas || !DisplayShader) { Debug.LogError("PMS: assign the package font and CRT shader, or use the supplied prefab.",this); enabled=false; return; }
            BuildSurface(); built=true;
            if (AutoBoot) Boot(); else { Phase=PowerPhase.Running; DrawApplication(); }
        }
        private void OnEnable() { if (built && AutoBoot) Boot(); }
        private void OnDisable() { StopAllCoroutines(); pressed=hovered=null; if(surface) surface.SetActive(false); }
        private void OnDestroy()
        {
            if(ScreenTexture) { ScreenTexture.Release(); Destroy(ScreenTexture); }
            if(ScreenMaterial) Destroy(ScreenMaterial);
        }
        private void BuildSurface()
        {
            surface=new GameObject("PMS Display Surface");surface.transform.SetParent(transform,false);
            var cameraObject=new GameObject("Source Camera");cameraObject.transform.SetParent(surface.transform,false);
            sourceCamera=cameraObject.AddComponent<Camera>();sourceCamera.transform.position=new Vector3(50000,50000,-10);
            sourceCamera.orthographic=true;sourceCamera.orthographicSize=300;sourceCamera.nearClipPlane=.1f;sourceCamera.farClipPlane=20;
            sourceCamera.clearFlags=CameraClearFlags.SolidColor;sourceCamera.backgroundColor=Desk;sourceCamera.cullingMask=1<<SourceLayer;sourceCamera.depth=-100;
            ScreenTexture=new RenderTexture(800,600,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear) {name="PMS 800x600",filterMode=FilterMode.Point,antiAliasing=1};ScreenTexture.Create();sourceCamera.targetTexture=ScreenTexture;
            var source=new GameObject("Native UI",typeof(RectTransform),typeof(Canvas));source.transform.SetParent(surface.transform,false);source.layer=SourceLayer;
            var sourceCanvas=source.GetComponent<Canvas>();sourceCanvas.renderMode=RenderMode.ScreenSpaceCamera;sourceCanvas.worldCamera=sourceCamera;sourceCanvas.planeDistance=1;sourceCanvas.pixelPerfect=true;
            sourceRoot=source.GetComponent<RectTransform>();
            Box(sourceRoot,"Dark glass backdrop",0,0,800,600,Hex("030201"));
            var output=new GameObject("Output",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));output.transform.SetParent(surface.transform,false);
            var canvas=output.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30000;canvas.enabled=ShowOverlay;
            var scaler=output.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(800,600);scaler.matchWidthOrHeight=.5f;
            display=Rect(output.transform,"CRT 4x3",0,0,800,600);display.anchorMin=Vector2.zero;display.anchorMax=Vector2.one;display.offsetMin=display.offsetMax=Vector2.zero;
            var aspect=display.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=4f/3f;
            var image=display.gameObject.AddComponent<RawImage>();image.texture=ScreenTexture;image.raycastTarget=false;
            ScreenMaterial=new Material(DisplayShader);ScreenMaterial.name="PMS runtime CRT";image.material=ScreenMaterial;
        }
        private RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var o=new GameObject(name,typeof(RectTransform));o.layer=SourceLayer;o.transform.SetParent(parent,false);
            var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        private Image Box(Transform p,string n,float x,float y,float w,float h,Color color)
        {
            var r=Rect(p,n,x,y,w,h);var i=r.gameObject.AddComponent<Image>();
            // Canvas converts Image vertex colors to linear in a Linear project. The source buffer
            // intentionally stores the HTML's gamma-encoded RGB values, matching its Canvas pixels.
            i.color=QualitySettings.activeColorSpace==ColorSpace.Linear?new Color(Mathf.LinearToGammaSpace(color.r),Mathf.LinearToGammaSpace(color.g),Mathf.LinearToGammaSpace(color.b),color.a):color;
            i.raycastTarget=false;return i;
        }
        private void Rule(Transform p,float x,float y,float w,Color c)
        {
            var half=new Color(c.r,c.g,c.b,c.a*.5f);
            Box(p,"Rule start",x,y,1,1,half);if(w>2)Box(p,"Rule",x+1,y,w-2,1,c);if(w>1)Box(p,"Rule end",x+w-1,y,1,1,half);
        }
        private void Vertical(Transform p,float x,float y,float h,Color c)
        {
            var half=new Color(c.r,c.g,c.b,c.a*.5f);
            Box(p,"Rule start",x,y,1,1,half);if(h>2)Box(p,"Rule",x,y+1,1,h-2,c);if(h>1)Box(p,"Rule end",x,y+h-1,1,1,half);
        }
        private void Outline(Transform p,float x,float y,float w,float h,Color c)
        {Rule(p,x,y,w,c);Vertical(p,x,y,h,c);Vertical(p,x+w-1,y,h,c);Rule(p,x,y+h-1,w,c);}
        private PMSFixedText Text(Transform p,string value,float x,float y,int size=14,Color? color=null,float width=760,bool wrap=false)
        {
            var r=Rect(p,"Text "+(value??""),Mathf.Floor(x+.5f),Mathf.Floor(y+.5f),width,Mathf.Max(30,wrap?130:30));var t=r.gameObject.AddComponent<PMSFixedText>();
            t.Glyphs=Glyphs;t.Content=value??"";t.PixelSize=size;t.LinePitch=20;t.Wrap=wrap;t.color=color??Ink;t.raycastTarget=false;t.SetAllDirty();return t;
        }
        private void Right(Transform p,string value,float right,float y,int size=13,Color? color=null) { Text(p,value,right-PMSFixedText.Width(value,size),y,size,color); }
        private void Center(Transform p,string value,float y,int size=13,Color? color=null) { Text(p,value,Mathf.Floor((800-PMSFixedText.Width(value,size))/2),y,size,color); }
        private void Clear()
        {
            hits.Clear();startupParts.Clear();consoleCursor=null;
            if(frame){frame.gameObject.SetActive(false);Destroy(frame.gameObject);}
            frame=Rect(sourceRoot,"Logical 800x600",0,0,800,600);
            frame.anchorMin=frame.anchorMax=frame.pivot=new Vector2(.5f,.5f);frame.anchoredPosition=Vector2.zero;
        }
        private Transform Part(string name) {var r=Rect(frame,name,0,0,800,600);startupParts.Add(r.gameObject);return r;}
        private void Shell()
        {
            Box(frame,"Background",0,0,800,600,Desk);Outline(frame,8,8,784,584,Edge);Rule(frame,8,36,784,Edge);if(!reviewPage)Rule(frame,8,393,784,Edge);
        }
        private void Header(Transform p)
        {Text(p,"1994-11-08",22,16,13,Bright);Center(p,"PMS / 3.14",16);Right(p,"T-06 / 047",778,16,13,Bright);}
        private void Footer(Transform p,string first,string second)
        {Box(p,"Command strip",9,548,782,43,Select);Center(p,first,554,14,Inverse);Center(p,second,574,12,Inverse);}
        private void Row(Transform p,string id,float x,float y,float w,string number,string label,string status,bool enabled,bool selected,Action action,int height=28)
        {
            bool hot=enabled&&(hovered==id||hovered==null&&selected),down=enabled&&pressed==id;
            if(hot)Box(p,"Selected "+id,x,y,w,height,Select);if(down)Outline(p,x+1,y+1,w-2,height-2,Inverse);
            var c=hot?Inverse:enabled?Ink:Faint;int offset=down?1:0;
            Text(p,number,x+8+offset,y+5+offset,12,c,28);
            float labelX=x+42+offset,rightW=string.IsNullOrEmpty(status)?0:PMSFixedText.Width(status,12)+18;
            Text(p,label,labelX,y+4+offset,14,c,w-(labelX-x)-rightW-6);
            if(!string.IsNullOrEmpty(status))Right(p,status,x+w-8+offset,y+5+offset,12,c);
            if(enabled)hits.Add(new Hit{Id=id,Area=new Rect(x,y,w,height),Action=action});
        }
        private void Field(Transform p,string label,string value,int y,bool focus=false)
        {Text(p,label,24,y);if(focus)Outline(p,131,y-4,633,24,Bright);Text(p,value,138,y,14,focus?Bright:Ink,620);}
        private void Directory()
        {
            Shell();Header(Part("01 Header"));var heading=Part("02 Directory heading");
            Text(heading,"第三安置所 / B区 / 观察对象",23,51,14,Bright);Right(heading,"人员复核 / DAY 01",777,51);
            Text(heading,"对象 / 登记编号",29,82,13,Dim);Right(heading,"单元       状态",772,82,13,Dim);
            for(int i=0;i<5;i++)
            {
                int slot=i;var p=Part("03 Directory row "+i);bool valid=i<roster.Length;var e=valid?roster[i]:null;
                Row(p,"roster"+i,22,108+i*30,756,(i+1).ToString("00"),valid?e.Name+"    "+e.Id:"未配置对象 "+(i+1).ToString("00"),valid?e.Unit+"    "+e.Status:"--      未接入",true,selectedRoster==i,()=>{selectedRoster=slot;DrawApplication();},27);
            }
            var summary=Part("04 Directory summary");int available=0;foreach(var e in roster)if(e.CanEnter)available++;
            Text(summary,"当日登记 "+roster.Length.ToString("00")+" / 待审 "+available.ToString("00")+" / 已处置 "+(roster.Length-available).ToString("00"),29,351,13,Dim);
            var selected=selectedRoster<roster.Length?roster[selectedRoster]:null;
            Field(Part("05 Terminal"),"终端","第三安置所 / B区",410);
            Field(Part("06 Object"),"对象",selected==null?"未配置对象 "+(selectedRoster+1).ToString("00"):selected.Name.Split('/')[0].Trim()+" / "+selected.Id,436);
            Field(Part("07 Command"),"当前命令",selected==null?"尚未接入":selected.CanEnter?"进入人员复核":"档案已关闭",462,true);
            Field(Part("08 Status"),"档案状态",selected==null?"无登记数据":selected.CanEnter?selected.Status+" / 尚未开始":"已归档 / "+selected.Status,488);
            var last=Part("09 Quota and unit");Text(last,"净化配额",24,514);Text(last,QuotaDisplay,138,514);Text(last,"单元",482,514);Text(last,selected?.Unit??"--",596,514);
            Footer(Part("10 Commands"),selected!=null&&selected.CanEnter?"选择对象并确认，进入人员复核":selected!=null?"当前对象已处置，档案不可再次打开":"当前条目未配置","↑↓ 选择对象   双击 / Enter 打开   F2 终端设置");
        }
        private void Review()
        {
            Shell();Header(frame);Text(frame,review.Id,26,51,13);Text(frame,review.Location,532,51,13,Bright);
            Vertical(frame,516,48,487,Faint);Text(frame,"监控 / "+review.Unit,26,69,13,Bright);Right(frame,"CH 01",498,69,12,Dim);
            Outline(frame,26,88,472,269,Faint);
            var image=Rect(frame,"Ward 16x9",30,92,464,261).gameObject.AddComponent<RawImage>();image.texture=Ward;image.raycastTarget=false;
            Text(frame,"CAM B12-01",37,100,10,Dim);
            Text(frame,history?"对话记录 / 查阅":"对话 / "+review.Round,26,368,13,Dim);Right(frame,history?"F3 返回":"F3 查阅",498,368,11,Dim);Rule(frame,22,388,480,Faint);
            hits.Add(new Hit{Id="history",Area=new Rect(22,360,480,26),Action=ToggleHistory});
            var clip=Rect(frame,"Dialogue clip",23,395,478,131);clip.gameObject.AddComponent<RectMask2D>();
            int rows=history?6:5,total=review.Lines?.Length??0,end=Mathf.Max(0,total-historyOffset),start=Mathf.Max(0,end-rows),yy=4;
            for(int i=start;i<end;i++){var e=review.Lines[i];Text(clip,e.Speaker,3,yy,11,Dim);var body=Text(clip,e.Text,56,yy,14,i==total-1?Bright:Ink,415,true);body.MaxHeight=32;yy+=(history?22:25);}
            Text(frame,"人员复核 / "+review.Round,534,83,13,Dim);var status=Text(frame,history?"查阅记录中，处置暂时锁定。":review.Status,534,112,12,Dim,240,true);status.MaxHeight=46;status.LinePitch=19;
            Text(frame,"处置命令",534,173,13,Dim);Rule(frame,530,194,248,Faint);
            string[] commands={"释放","治疗","净化"};bool[] allowed={review.CanRelease,review.CanTreat,review.CanPurify};
            for(int i=0;i<3;i++){string type=commands[i];bool can=allowed[i]&&!history;Row(frame,"command"+i,530,204+i*37,248,(i+1).ToString("00"),type,can?"可执行":"锁定",can,selectedDisposition==i,()=>DispositionRequested.Invoke(type));}
            Rule(frame,530,334,248,Faint);Text(frame,"应答命令",534,346,13,Dim);
            if(!history)
            {
                if(review.Replies!=null&&review.Replies.Length>0)for(int i=0;i<Mathf.Min(4,review.Replies.Length);i++)
                {var reply=review.Replies[i];Row(frame,"reply"+i,530,374+i*35,248,(i+1).ToString("00"),reply.Text,"",reply.Enabled,i==selectedReply,()=>ReplyRequested.Invoke(reply.Id));}
                else if(review.CanContinue)Row(frame,"continue",530,374,248,">","下一句","Enter",true,true,()=>ContinueRequested.Invoke());
            }
            if(history)
            {
                Text(frame,"记录 "+(total>0?start+1:0)+"–"+end+" / "+total,26,532,10,Dim);
                Footer(frame,"查阅对话记录 / 不执行应答和处置","↑↓ / 滚轮 翻阅   F3 / Esc 返回   F2 终端设置");
            }
            else if(review.Replies!=null&&review.Replies.Length>0)Footer(frame,"选择应答并确认","↑↓ 选择   Enter / 单击 确认   1–4 应答   F3 查阅记录");
            else if(!review.CanContinue)
            {
                var hint=Text(frame,"会谈结束。请在上方选择处置命令。",536,382,14,Dim,236,true);hint.LinePitch=22;hint.MaxHeight=76;
                Footer(frame,"会谈结束 / 选择处置命令并确认","↑↓ 选择处置   Enter / 单击 确认   F3 查阅记录");
            }
            else Footer(frame,"按 Enter 或单击反白命令条继续","F3 查阅对话记录   F2 终端设置");
        }
        private void DrawApplication() {if(!built||Phase!=PowerPhase.Running)return;Clear();if(reviewPage)Review();else Directory();}
        public void SetRoster(PMSRosterEntry[] entries) {roster=entries??new PMSRosterEntry[0];selectedRoster=Mathf.Clamp(selectedRoster,0,Mathf.Max(0,roster.Length-1));DrawApplication();}
        public void ShowDirectory() {reviewPage=false;history=false;DrawApplication();}
        public void ShowReview(PMSReviewData data) {review=data??new PMSReviewData();reviewPage=true;history=false;historyOffset=0;selectedReply=0;DrawApplication();}
        public void SetReview(PMSReviewData data) {review=data??new PMSReviewData();selectedReply=Mathf.Clamp(selectedReply,0,Mathf.Max(0,(review.Replies?.Length??0)-1));DrawApplication();}
        public void ToggleHistory() {if(Phase!=PowerPhase.Running||!reviewPage)return;history=!history;historyOffset=0;DrawApplication();}
        [ContextMenu("Preview/Boot")]
        public void Boot() {if(!built)return;surface.SetActive(true);StopAllCoroutines();reviewPage=false;history=false;pressed=hovered=null;StartCoroutine(BootSequence());}
        public void NotifyAllCharactersCompletedAfterDialogue() {if(Phase==PowerPhase.Running){StopAllCoroutines();StartCoroutine(ShutdownSequence());}}
        [ContextMenu("Preview/Shutdown")]
        private void PreviewShutdown() {NotifyAllCharactersCompletedAfterDialogue();}
        private void ConsoleFrame(params string[] lines)
        {
            Clear();Box(frame,"Dark glass",0,0,800,600,Hex("080603"));
            for(int i=0;i<lines.Length;i++)
            {
                bool cursor=i==lines.Length-1&&lines[i].EndsWith("_");
                string value=cursor?lines[i].Substring(0,lines[i].Length-1).TrimEnd():lines[i];Text(frame,value,28,24+i*20,14,Ink);
                if(cursor)consoleCursor=Box(frame,"Cursor",28+PMSFixedText.Width(value,14),24+i*20+14,8,2,Ink).gameObject;
            }
        }
        private IEnumerator Wait(float seconds) {yield return new WaitForSecondsRealtime(seconds);}
        private IEnumerator BootSequence()
        {
            Phase=PowerPhase.Boot;cursorClock=Time.unscaledTime;ConsoleFrame("_");yield return Wait(.625f);ConsoleFrame("PMS console 3.14","","Self-test");yield return Wait(.5f);
            int[] counts={4,10,16,21,28,32,36,43,48,50,56,64};float[] waits={.125f,.125f,.25f,.125f,.625f,.125f,.125f,.125f,.375f,.125f,.125f,.5f};
            for(int k=0;k<counts.Length;k++){var lines=new List<string>{"PMS console 3.14","","Self-test",""};for(int i=0;i<counts[k];i++){int row=4+i/16;if(lines.Count<=row)lines.Add("");lines[row]+=(66-i).ToString("00")+"..";}ConsoleFrame(lines.ToArray());yield return Wait(waits[k]);}
            var completed=new List<string>{"PMS console 3.14","","Self-test",""};for(int row=0;row<4;row++){string line="";for(int i=row*16;i<(row+1)*16;i++)line+=(66-i).ToString("00")+"..";completed.Add(line);}completed.Add("");completed.Add("Tests completed.");completed.Add("");completed.Add(">>> _");ConsoleFrame(completed.ToArray());yield return Wait(1.25f);
            completed[completed.Count-1]=">>> BOOT";ConsoleFrame(completed.ToArray());yield return Wait(.75f);ConsoleFrame();yield return Wait(.375f);ConsoleFrame("PMS / 3.14","","Console ready.","_");yield return Wait(1.25f);
            yield return LoadDirectory();
        }
        private IEnumerator LoadDirectory()
        {
            Phase=PowerPhase.Loading;Clear();reviewPage=false;Directory();hits.Clear();foreach(var p in startupParts)p.SetActive(false);
            int last=0;for(int i=0;i<startupParts.Count;i++){yield return Wait((LoadAt[i]-last)/1000f);last=LoadAt[i];startupParts[i].SetActive(true);}
            yield return Wait(.12f);Phase=PowerPhase.Running;DrawApplication();
        }
        private IEnumerator ShutdownSequence()
        {
            Phase=PowerPhase.Shutdown;cursorClock=Time.unscaledTime;ConsoleFrame();yield return Wait(.375f);ConsoleFrame("Closing session...");yield return Wait(.875f);ConsoleFrame("Session closed.");yield return Wait(.5f);ConsoleFrame("Session closed.","","System halted.","",">>> _");yield return Wait(.75f);ConsoleFrame("System halted.","",">>> _");yield return Wait(.625f);
            var content=frame;var dim=content.gameObject.AddComponent<CanvasGroup>();
            var optics=Rect(sourceRoot,"Power optics",0,0,800,600);optics.anchorMin=optics.anchorMax=optics.pivot=new Vector2(.5f,.5f);optics.anchoredPosition=Vector2.zero;
            var beam=Glow(optics,"Fine beam",GlowLine);var bloom=Glow(optics,"Beam bloom",GlowLine);var dot=Glow(optics,"Afterglow",GlowDot);
            float began=Time.unscaledTime;
            while(Time.unscaledTime-began<.76f)
            {
                float ms=(Time.unscaledTime-began)*1000,half=0,energy=0,dotEnergy=0,h=600,w=800,picture=0;
                if(ms<100)picture=1-.7f*ms/100;
                else if(ms<240){float p=(ms-100)/140;h=Mathf.Max(1.1f,600*Mathf.Pow(1-p,3));w=800*(1-.035f*p);picture=.3f*(1-p);energy=.20f*p*p;half=356;}
                else if(ms<390){float p=(ms-240)/150;energy=.24f*Mathf.Exp(-2.7f*p);half=356*Mathf.Exp(-6.2f*p);dotEnergy=.085f*p;}
                else dotEnergy=.09f*Mathf.Exp(-8*(ms-390)/370);
                content.localScale=new Vector3(w/800,h/600,1);dim.alpha=picture;
                Light(beam,half*2,1.1f,energy*1.8f);Light(bloom,half*2,11,energy*.30f);Light(dot,18,14,dotEnergy*1.4f);yield return null;
            }
            optics.gameObject.SetActive(false);Destroy(optics.gameObject);Clear();Box(frame,"Off",0,0,800,600,Hex("030201"));Phase=PowerPhase.Off;ShutdownCompleted.Invoke();
        }
        private RawImage Glow(Transform parent,string name,Texture texture)
        {var r=Rect(parent,name,400,300,0,0);r.pivot=new Vector2(.5f,.5f);var i=r.gameObject.AddComponent<RawImage>();i.texture=texture;i.raycastTarget=false;return i;}
        private static void Light(RawImage image,float w,float h,float alpha) {image.rectTransform.sizeDelta=new Vector2(Mathf.Max(.1f,w),Mathf.Max(.1f,h));image.color=new Color(1,1,1,Mathf.Clamp01(alpha));}
        public void HandleKey(string key)
        {
            if(Phase==PowerPhase.Off){if(key=="Enter")Boot();return;}
            if(Phase!=PowerPhase.Running)return;
            if(!reviewPage){if(key=="Up"||key=="Down"){selectedRoster=Mathf.Clamp(selectedRoster+(key=="Up"?-1:1),0,4);DrawApplication();}else if(key=="Enter"&&selectedRoster<roster.Length&&roster[selectedRoster].CanEnter)EnterRequested.Invoke(roster[selectedRoster].Id);return;}
            if(key=="F3"||key=="Escape"&&history){ToggleHistory();return;}
            if(history){if(key=="Up"||key=="Down"){historyOffset=Mathf.Clamp(historyOffset+(key=="Up"?1:-1),0,Mathf.Max(0,(review.Lines?.Length??0)-6));DrawApplication();}return;}
            int count=review.Replies?.Length??0;
            if(count>0)
            {
                if(key=="Up"||key=="Down"){selectedReply=(selectedReply+(key=="Up"?-1:1)+count)%count;DrawApplication();}
                int index;if(int.TryParse(key,out index)&&index>=1&&index<=Mathf.Min(4,count)&&review.Replies[index-1].Enabled)ReplyRequested.Invoke(review.Replies[index-1].Id);
                if(key=="Enter"&&review.Replies[selectedReply].Enabled)ReplyRequested.Invoke(review.Replies[selectedReply].Id);
            }
            else if(review.CanContinue&&(key=="Enter"||key=="Space"))ContinueRequested.Invoke();
            else if(key=="Up"||key=="Down"){selectedDisposition=(selectedDisposition+(key=="Up"?-1:1)+3)%3;DrawApplication();}
            else if(key=="Enter") {bool allowed=selectedDisposition==0?review.CanRelease:selectedDisposition==1?review.CanTreat:review.CanPurify;if(allowed)DispositionRequested.Invoke(new[]{"释放","治疗","净化"}[selectedDisposition]);}
        }
        /// <summary>Use this from a 3D screen raycast. UV is bottom-left origin, matching a normal Unity texture.</summary>
        public void PointerUV(Vector2 uv,bool down,bool up)
        {
            if(Phase==PowerPhase.Off){if(up)Boot();return;}if(Phase!=PowerPhase.Running)return;
            Vector2 q=uv*2-Vector2.one;float curve=CrtMode==2?.018f:.008f,zoom=CrtMode==2?1.012f:1.006f,warp=CrtMode==0?1:zoom*(1+curve*q.sqrMagnitude);var p=(q*warp+Vector2.one)*.5f;var logical=new Vector2(p.x*800,(1-p.y)*600);
            Hit hit=null;foreach(var candidate in hits)if(candidate.Area.Contains(logical)){hit=candidate;break;}
            if(down)pressed=hit?.Id;
            if(up){string was=pressed;pressed=null;if(hit!=null&&was==hit.Id){bool rosterDouble=hit.Id.StartsWith("roster")&&lastRosterId==hit.Id&&Time.unscaledTime-lastRosterClick<.32f;hit.Action();if(hit.Id.StartsWith("roster")){lastRosterId=hit.Id;lastRosterClick=Time.unscaledTime;if(rosterDouble&&selectedRoster<roster.Length&&roster[selectedRoster].CanEnter)EnterRequested.Invoke(roster[selectedRoster].Id);}return;}}
            if(hovered!=hit?.Id){hovered=hit?.Id;DrawApplication();}
        }
        private void Update()
        {
            if(consoleCursor)consoleCursor.SetActive(Mathf.FloorToInt((Time.unscaledTime-cursorClock)*2)%2==0);
            if(ScreenMaterial)ScreenMaterial.SetFloat("_Mode",CrtMode);
            if(!built||!ReadKeyboardAndMouse)return;
#if ENABLE_INPUT_SYSTEM
            var keyboard=UnityEngine.InputSystem.Keyboard.current;
            if(keyboard!=null){if(keyboard.enterKey.wasPressedThisFrame)HandleKey("Enter");if(keyboard.spaceKey.wasPressedThisFrame)HandleKey("Space");if(keyboard.upArrowKey.wasPressedThisFrame)HandleKey("Up");if(keyboard.downArrowKey.wasPressedThisFrame)HandleKey("Down");if(keyboard.f3Key.wasPressedThisFrame)HandleKey("F3");if(keyboard.escapeKey.wasPressedThisFrame)HandleKey("Escape");if(keyboard.digit1Key.wasPressedThisFrame)HandleKey("1");if(keyboard.digit2Key.wasPressedThisFrame)HandleKey("2");if(keyboard.digit3Key.wasPressedThisFrame)HandleKey("3");if(keyboard.digit4Key.wasPressedThisFrame)HandleKey("4");}
            var mouse=UnityEngine.InputSystem.Mouse.current;if(mouse!=null)Mouse(mouse.position.ReadValue(),mouse.leftButton.wasPressedThisFrame,mouse.leftButton.wasReleasedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(KeyCode.Return))HandleKey("Enter");if(Input.GetKeyDown(KeyCode.Space))HandleKey("Space");if(Input.GetKeyDown(KeyCode.UpArrow))HandleKey("Up");if(Input.GetKeyDown(KeyCode.DownArrow))HandleKey("Down");if(Input.GetKeyDown(KeyCode.F3))HandleKey("F3");if(Input.GetKeyDown(KeyCode.Escape))HandleKey("Escape");
            for(int i=1;i<=4;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0+i)))HandleKey(i.ToString());Mouse(Input.mousePosition,Input.GetMouseButtonDown(0),Input.GetMouseButtonUp(0));
#endif
        }
        private void Mouse(Vector2 point,bool down,bool up)
        {Vector2 local;if(!display||!RectTransformUtility.ScreenPointToLocalPointInRectangle(display,point,null,out local))return;var r=display.rect;var uv=new Vector2((local.x-r.x)/r.width,(local.y-r.y)/r.height);if(uv.x<0||uv.x>1||uv.y<0||uv.y>1){pressed=null;return;}PointerUV(uv,down,up);}
    }
}

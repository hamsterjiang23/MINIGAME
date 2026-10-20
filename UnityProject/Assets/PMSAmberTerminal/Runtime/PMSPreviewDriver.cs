using System.Collections.Generic;
using UnityEngine;

namespace PMSAmber
{
    /// <summary>Visual demonstration only. Do not attach this component to the production GameSession.</summary>
    public sealed class PMSPreviewDriver : MonoBehaviour
    {
        public PMSTerminalView View;
        private PMSReviewData data;
        private readonly List<PMSLogLine> lines=new List<PMSLogLine>();
        private int step;
        private void Awake()
        {
            if(!View)View=GetComponent<PMSTerminalView>();
            View.EnterRequested.AddListener(Enter);View.ContinueRequested.AddListener(Continue);
            View.ReplyRequested.AddListener(Reply);View.DispositionRequested.AddListener(Disposition);
        }
        private void Enter(string id)
        {step=0;lines.Clear();lines.Add(new PMSLogLine());data=new PMSReviewData{Id=id,Lines=lines.ToArray()};View.ShowReview(data);}
        private void Continue()
        {
            if(data==null)return;
            step++;
            if(step==1){lines.Add(new PMSLogLine{Text="和昨天广场上的不一样吗？"});data.Lines=lines.ToArray();}
            else{data.CanContinue=false;data.Replies=new[]{new PMSReply{Id="01",Text="嗯，不一样。"},new PMSReply{Id="02",Text="别害怕，慢慢说。"},new PMSReply{Id="03",Text="和你说的一样。"}};}
            View.SetReview(data);
        }
        private void Reply(string id)
        {
            lines.Add(new PMSLogLine{Speaker="值班员",Text="【资源预览】应答 "+id});data.Lines=lines.ToArray();data.Replies=new PMSReply[0];data.CanContinue=false;
            data.Status="资源预览：选择任一处置可查看关机效果。";data.CanRelease=data.CanTreat=data.CanPurify=true;View.SetReview(data);
        }
        private void Disposition(string type)
        {
            data.CanRelease=data.CanTreat=data.CanPurify=false;data.Status="资源预览 / "+type;View.SetReview(data);
            // The demo previews presentation only. Production must wait for the final reaction dialogue.
            View.NotifyAllCharactersCompletedAfterDialogue();
        }
        private void OnDestroy()
        {if(!View)return;View.EnterRequested.RemoveListener(Enter);View.ContinueRequested.RemoveListener(Continue);View.ReplyRequested.RemoveListener(Reply);View.DispositionRequested.RemoveListener(Disposition);}
    }
}

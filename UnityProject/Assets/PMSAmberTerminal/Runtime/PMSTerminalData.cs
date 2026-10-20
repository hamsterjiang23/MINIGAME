using System;
namespace PMSAmber
{
    [Serializable] public sealed class PMSRosterEntry
    {
        public string Id = "R-0317-042", Name = "小孩 / 初见", Unit = "B-12", Status = "待审";
        public bool CanEnter = true;
    }
    [Serializable] public sealed class PMSReply
    {
        public string Id, Text;
        public bool Enabled = true;
    }
    [Serializable] public sealed class PMSLogLine
    {
        public string Speaker = "对象", Text = "这里还要检测吗？";
    }
    [Serializable] public sealed class PMSReviewData
    {
        public string Id = "R-0317-042", Unit = "B-12", Round = "R1";
        public string Location = "第三安置所 / B区", Status = "会谈进行中，处置暂时锁定。";
        public PMSLogLine[] Lines = { new PMSLogLine() };
        public PMSReply[] Replies = new PMSReply[0];
        public bool CanContinue = true, CanRelease, CanTreat, CanPurify;
    }
}

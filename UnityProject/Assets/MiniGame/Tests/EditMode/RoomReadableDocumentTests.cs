using NUnit.Framework;
using UnityEngine;

namespace MiniGame.UnityHost.Tests
{
    public sealed class RoomReadableDocumentTests
    {
        private const string FamilyLetter = "你能考上国家干部，我们全家都很高兴。咱们家总算出了个有出息的人！听说和平部的工作很体面，坐办公室，吃穿都有国家照顾，还能帮助那些危险的人重新过上正常的生活，功德无量。能做这样的工作是你的福气。\n\n"
            + "现在国家太平，百姓安居乐业，全靠领袖这些年的辛劳。你既然进了和平部，就更要懂得感恩，服从领导安排，凡事以国家的利益为先。记得把衣服收拾整齐，见了领导要主动问好。好好工作，别给家里丢脸。\n\n"
            + "等领到制服，记得寄张照片回来，你爸想拿给亲戚们看看。\n\n爱你的\n\n父母";

        [TestCase("FamilyLetter", "family-letter", "阅读信件")]
        [TestCase("DailyNewspaper", "daily-newspaper", "阅读报纸")]
        [TestCase("EmploymentNotice", "employment-notice", "查看录用通知书")]
        public void AuthoredResourcesExposeStableIdentityAndInteraction(string resource, string id, string label)
        {
            var document = Load(resource);
            Assert.That(document.Id, Is.EqualTo(id));
            Assert.That(document.Title, Is.Not.Empty);
            Assert.That(document.InteractionLabel, Is.EqualTo(label));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(99)]
        public void StaticDocumentsPreserveTheCompleteSourceTextOnEveryDay(int day)
        {
            Assert.That(Load("FamilyLetter").GetText(day), Is.EqualTo(FamilyLetter));
            Assert.That(Load("EmploymentNotice").GetText(day),
                Is.EqualTo("我的录用通知书，那是我人生中最开心的一天。"));
        }

        [TestCase(1, "本市治安水平再创历史新高\n\n据和平部最新公布的统计数据，本市犯罪案件数量已连续六个月下降，社会治安状况持续向好。\n\n和平部发言人表示，这一成果得益于全民健康监测制度的全面落实，以及广大市民对预防性治疗工作的积极配合。\n\n“和平是每一位公民共同努力的成果。”")]
        [TestCase(2, "东三区集中健康检查圆满结束\n\n昨日，东三区顺利完成本月第二次集中健康检查。\n\n检查期间，共有14名市民被建议前往和平院接受进一步治疗。相关部门表示，所有人员均已得到妥善安置。\n\n值得一提的是，本次检查期间，辖区内未发生任何犯罪案件。")]
        [TestCase(3, "和平部回应东三区居民去向问题\n\n近日，部分市民对东三区集中健康检查后的人员安置情况表示关切。\n\n和平部今日回应称，相关人员均已依照规定完成统一安置，不存在所谓的“居民失踪”情况。\n\n有关部门同时提醒，未经许可传播其他公民的个人信息，可能对社会秩序造成不良影响。")]
        public void NewspaperUsesOnlyTheExactArticleForTheRequestedDay(int day, string text)
        {
            Assert.That(Load("DailyNewspaper").GetText(day), Is.EqualTo(text));
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(4)]
        [TestCase(99)]
        public void NewspaperDoesNotLeakOtherDaysWhenNoArticleIsDefined(int day)
        {
            var root = new GameObject("Newspaper resource test");
            try
            {
                var prop = root.AddComponent<ReadableProp>();
                prop.ConfigureDocument(Load("DailyNewspaper"));
                Assert.That(prop.CaptionForDay(day), Is.Empty);
                Assert.That(prop.IsReadableOnDay(day), Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ReadablePropBindsDocumentAndRetainsLegacyConfigureBehavior()
        {
            var root = new GameObject("Readable resource test");
            try
            {
                var prop = root.AddComponent<ReadableProp>();
                var letter = Load("FamilyLetter");
                prop.ConfigureDocument(letter);
                Assert.That(prop.Document, Is.SameAs(letter));
                Assert.That(prop.Id, Is.EqualTo("family-letter"));
                Assert.That(prop.Caption, Is.EqualTo(FamilyLetter));
                Assert.That(prop.InteractionLabel, Is.EqualTo("阅读信件"));
                Assert.That(prop.IsReadableOnDay(0), Is.True);

                prop.Configure("legacy-poster", "海报文案", null);
                Assert.That(prop.Document, Is.Null);
                Assert.That(prop.Id, Is.EqualTo("legacy-poster"));
                Assert.That(prop.CaptionForDay(3), Is.EqualTo("海报文案"));
                Assert.That(prop.InteractionLabel, Is.EqualTo("查看海报"));
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static RoomReadableDocument Load(string resource)
        {
            var document = Resources.Load<RoomReadableDocument>("MiniGame/Readables/" + resource);
            Assert.That(document, Is.Not.Null, resource);
            return document;
        }
    }
}

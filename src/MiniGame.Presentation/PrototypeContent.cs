#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MiniGame.Core;

namespace MiniGame.Presentation
{
    /// <summary>Original Chinese development dialogue. The supplied rules own every node and choice.</summary>
    public sealed class PrototypeContent : IGameContent
    {
        private readonly GameRules _rules;
        private readonly Dictionary<string, DialogueContent> _nodes = new Dictionary<string, DialogueContent>(StringComparer.Ordinal);
        public string Revision => "original-zh-placeholder-v1:" + _rules.Fingerprint;
        public IReadOnlyList<DialogueContent> Nodes { get; }

        public PrototypeContent(GameRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            foreach (var day in rules.Days)
            foreach (var meeting in day.Meetings)
            {
                var patient = rules.Patients.Single(p => p.Id == meeting.PatientId);
                for (var index = 0; index < meeting.NormalRounds.Count; index++)
                    AddRound(meeting.NormalRounds[index], patient, day.Day, index, false);
                if (meeting.SpecialRound != null) AddRound(meeting.SpecialRound, patient, day.Day, 0, true);
                foreach (var reaction in meeting.ReactionNodes)
                    Add(new DialogueContent(reaction.Value, patient.DisplayName,
                        ReactionText(patient.Id, reaction.Key), Array.Empty<ResponseContent>()));
            }
            Nodes = new ReadOnlyCollection<DialogueContent>(_nodes.Values.OrderBy(n => n.NodeId, StringComparer.Ordinal).ToList());
        }

        public DialogueContent GetDialogue(PlayerView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (view.CurrentNodeId == null || !_nodes.TryGetValue(view.CurrentNodeId, out var node))
                throw new InvalidOperationException("The current core node has no configured development content.");
            return new DialogueContent(node.NodeId, node.Speaker, node.Text,
                node.Responses.Where(r => view.AvailableResponseIds.Contains(r.Id)));
        }

        public string GetPresentation(PresentationKind kind, PlayerView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            switch (kind)
            {
                case PresentationKind.ResponseBranch:
                case PresentationKind.Reaction: return GetDialogue(view).Text;
                case PresentationKind.MorningInspection:
                    return view.Day == 1 ? "【开发占位】门外的扫描灯亮起。监督终端要求你保持安静，等待今天的员工晨检结束。"
                        : view.Day == 2 ? "【开发占位】扫描灯停留得比昨天更久。终端只问：昨晚是否休息充足？"
                        : "【开发占位】晨检表多了一栏工作感受。你的手仍放在昨天相同的位置，扫描灯再次亮起。";
                case PresentationKind.Briefing:
                    return $"【开发占位】第{view.Day}天工作单已送达。今日尚需净化{view.RemainingPurificationQuota}人。请分别核对陈述、观察和工作记录，再提交处置。";
                case PresentationKind.EnterTerminal: return "【开发占位】终端连接监控。病人的会谈与检查入口正在载入。";
                case PresentationKind.ExitTerminal: return "【开发占位】处置记录已保存。信号短暂失真，终端退回房间。";
                case PresentationKind.Sleep: return "【开发占位】房间的灯逐一熄灭。你躺下后，终端的低鸣仍从墙里传来。";
                case PresentationKind.Dream:
                    return view.Day == 1 ? "【开发占位】梦里，每扇门后都放着一台监控。你走到自己的门前，屏幕却刚刚关闭。"
                        : view.Day == 2 ? "【开发占位】一叠没有姓名的工作单从桌上滑落。纸张不断重复同一句话：请等待下一次检定。"
                        : "【开发占位】你在空房间里听见扫描声。玻璃另一边的椅子慢慢转过来，座位上没有人。";
                case PresentationKind.Ending:
                    switch (view.Ending)
                    {
                        case EndingType.PriestAttack: return "【开发占位】来访者没有携带晨检设备。乔纳站在门外，重复着他对净化的理解。画面在你后退时中断。";
                        case EndingType.LockedInRoom: return "【开发占位】未完成的配额留在工作单上。门锁转动，终端通知你等待复检。房间从此没有新的开门提示。";
                        case EndingType.TakenAway: return "【开发占位】三天工作记录已归档。门外要求你接受下一轮审查。你离开座位，监控仍对准空椅子。";
                        case EndingType.DreamSea: return ""; // The dream-ending scene supplies its own presentation.
                        default: throw new InvalidOperationException("An ending presentation requires a resolved ending.");
                    }
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        public InspectionContent GetInspection(string patientId, int day, InspectionKind kind)
        {
            var patient = _rules.Patients.SingleOrDefault(p => p.Id == patientId)
                ?? throw new ArgumentException("Unknown configured patient.", nameof(patientId));
            if (!_rules.Days.Any(d => d.Day == day && d.Meetings.Any(m => m.PatientId == patientId)))
                throw new ArgumentOutOfRangeException(nameof(day), "This patient has no configured meeting on that day.");
            var title = kind switch
            {
                InspectionKind.Eyes => "眼部观察", InspectionKind.Hands => "手部观察",
                InspectionKind.Teeth => "口部观察", InspectionKind.Photo => "监控照片",
                InspectionKind.DailySigns => "今日检查提示", InspectionKind.Records => "工作档案",
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
            var description = kind switch
            {
                InspectionKind.Eyes => Observation(patientId, day),
                InspectionKind.Hands => Hands(patientId),
                InspectionKind.Teeth => "监控只能记录短暂张口的画面；照明与压缩会影响细节，这一观察不能单独决定处置。",
                InspectionKind.Photo => $"第{day}天的监控静帧。这里只保留当下可见的姿势；照片无法说明其经历或危险性。",
                InspectionKind.DailySigns => day == 1 ? "今日提示：先观察，再询问。把看见的细节与病人的陈述分开记录。"
                    : day == 2 ? "今日提示：相似表现可能有不同原因。核对昨天与今天的陈述，勿把单项外观当作结论。"
                    : "今日提示：记录中的缺席也是处置后果。没有新的观察时，不补写答案。",
                InspectionKind.Records => "此页是观察用开发占位，不含隐藏信任或真假标签。实际回答、处置和治疗次数由会话记录保存；此页不推定未发生的选择。",
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
            return new InspectionContent(kind, patient.DisplayName + " · " + title, "【开发占位】" + description);
        }

        private void AddRound(RoundDefinition round, PatientDefinition patient, int day, int index, bool special)
        {
            Add(new DialogueContent(round.NodeId, patient.DisplayName,
                "【开发占位】" + DialogueText(patient.Id, day, index, special),
                round.Responses.Select(r => new ResponseContent(r.Id, ResponseText(r.Id)))));
            foreach (var response in round.Responses)
                Add(new DialogueContent(response.BranchNodeId, patient.DisplayName,
                    "【开发占位】" + BranchText(patient.Id, response.Id), Array.Empty<ResponseContent>()));
        }

        private void Add(DialogueContent node)
        {
            if (_nodes.TryGetValue(node.NodeId, out var existing))
            {
                if (existing.Speaker != node.Speaker || existing.Text != node.Text ||
                    !existing.Responses.Select(r => r.Id).SequenceEqual(node.Responses.Select(r => r.Id)))
                    throw new ArgumentException("Conflicting content for the same core node: " + node.NodeId, nameof(_rules));
                return;
            }
            _nodes.Add(node.NodeId, node);
        }

        private static string ResponseText(string id) => id switch
        {
            "empathize" => "倾听：我听见了，请继续说。",
            "neutral" => "核对：请把你能确认的经过再说一遍。",
            "pressure" => "施压：请直接回答眼前的问题。",
            _ => "核对：继续说明这一点。"
        };

        private static string DialogueText(string patientId, int day, int index, bool special)
        {
            if (special) return patientId switch
            {
                PrototypeRules.Child => "我可以把想念说出来吗？我一直怕，这也会被写成不合格。",
                PrototypeRules.FormerRuler => "如果申辩只属于曾经掌权的人，那我所说的程序又有什么意义？",
                PrototypeRules.Doctor => "我想停止疼痛，可我也怕有一天连那些没能救下的人都忘记。",
                PrototypeRules.Diplomat => "你正在阅读我的记录。也请记得，你提交的每一句话都留下了记录。",
                PrototypeRules.Priest => "我相信净化有意义。你愿意听我说，并不表示我们相信同一件事。",
                _ => "有些事我还没有说清楚。你是否愿意再给我一次说明的机会？"
            };
            var lines = patientId switch
            {
                PrototypeRules.Child => day == 1
                    ? new[] { "我叫米洛。这里的问题有正确答案吗？", "我只是睡不着。我是不是已经说错了？", "他们让我把手放好，可我越想不动就越害怕。", "如果今天听话，我能知道家里有没有回信吗？" }
                    : day == 2 ? new[] { "我记住了昨天坐的位置。我今天会努力听话。", "纸上写要保持平静。我一直看着那行字。", "我想问一件事，又怕打断你的工作。", "如果我不哭，是不是就可以早点结束？" }
                    : new[] { "我今天想说，我很想家。", "我没有忘记这里的规矩，只是不想再装作不想念。", "你问经过的时候，可以让我先把这一句说完吗？", "我希望有人知道，我在这里等过。" },
                PrototypeRules.FormerRuler => day == 1
                    ? new[] { "我希望这次会谈也有可查阅的程序。", "我曾要求别人遵守流程。现在我也要求知道自己面对什么。", "我的职务已经结束。文件上的签名仍然是我的。", "请把我的申辩记录下来，不必先认同它。" }
                    : day == 2 ? new[] { "昨天提到的文件，我不能把签名推给别人。", "那时我认为每一项命令都有理由。理由并没有抹去后果。", "承担责任，与不许解释，并不是同一件事。", "你可以继续核对。我会区分我知道的与我推测的。" }
                    : new[] { "今天我不再只为自己要求申辩。", "若程序只能保护熟悉程序的人，它还欠别人很多。", "我曾经选择忽略的声音，如今听起来并不遥远。", "请记录我的责任，也记录其他人能否说话。" },
                PrototypeRules.Doctor => day == 1
                    ? new[] { "我是埃德温。这里把疼痛也归入需要治疗的表现吗？", "我听到仪器的声响时，会先寻找出口。", "我以前处理伤口，现在却不清楚这里的治疗要改变什么。", "你可以问我，但不要把停顿当成没有回答。" }
                    : day == 2 ? new[] { "昨天我没有说完救援时的取舍。", "当时资源不够。我作出的选择让另一些人没能等到救助。", "我记得他们的名字。理解当时的困难不能替我解除责任。", "请问具体的事，我不想用创伤掩盖每一件事。" }
                    : new[] { "我想让疼痛停止，又怕记忆也跟着消失。", "离开这里不代表我已经恢复。", "我仍会因为声音僵住，但我愿意把能确认的部分说明白。", "如果还有下一次会谈，请保留我今天说过的话。" },
                PrototypeRules.Diplomat => day == 1
                    ? new[] { "阿德里安。你可以按工作单的顺序问，我会配合。", "这样的会谈需要清楚的记录。我很理解你的处境。", "有些措辞看起来相同，写进记录后却会留下不同印象。", "请照常完成工作。你的问题本身也值得被准确记录。" }
                    : day == 2 ? new[] { "我们又见面了。你现在应该已经熟悉终端的记录方式。", "你选择观察什么，也会改变一份会谈记录的形状。", "我不会替你补写昨天的选择。终端已经保留了实际提交的内容。", "当你核对我的陈述时，也有人会核对你的工作。" }
                    : new[] { "今天的会谈里，你似乎更在意记录会被谁阅读。", "我知道的也有边界。没有显示的信息，我不会替它下结论。", "一份报告能描述你的选择，却未必能描述你为什么犹豫。", "现在，请想想你希望下一位阅读者看见什么。" },
                PrototypeRules.Priest => day <= 2
                    ? new[] { "我是乔纳。我等待净化，不是等待你替我找借口。", "信仰让我确信一些事，也让我难以听见反对的声音。", "你们的仪器有自己的说法。我的理解未必与它一致。", "若你只问我是否平静，我能回答；但那并不能说明我会做什么。" }
                    : new[] { "又一次治疗之后，我仍相信自己的判断。", "你让我解释，我却越来越急着要求你接受。", "我知道自己在重复同样的话。重复并没有消除它带来的危险。", "你不必把耐心误认成认同。我也不应这样理解。" },
                _ => new[] { "我想说明自己的经过。", "请区分观察到的事和对它的解释。", "还有一部分我不能确认。", "请保留我的实际陈述。" }
            };
            return index < lines.Length ? lines[index] : "在你继续核对以前，我想补充刚才没有说清的部分。";
        }

        private static string BranchText(string patientId, string responseId)
        {
            if (responseId == "pressure") return patientId == PrototypeRules.Child
                ? "米洛看了一眼屏幕，试着缩短自己的回答。" : "对方停顿了一会儿，重新组织陈述；这次停顿本身没有给出结论。";
            if (responseId == "empathize") return patientId == PrototypeRules.Diplomat
                ? "阿德里安点头，提醒你把刚才实际说过的话留在记录里。" : "对方继续说明刚才的经历。愿意继续说话，并不替任何处置提供标准答案。";
            return "对方把能确认的经过重新说明，并把不确定的部分留了下来。";
        }

        private static string ReactionText(string patientId, Disposition disposition)
        {
            var text = disposition switch
            {
                Disposition.Treat => patientId == PrototypeRules.Child ? "米洛问下次是否仍在这里见面。工作人员进入画面，信号随即失真。"
                    : patientId == PrototypeRules.Doctor ? "埃德温把手放回桌面，询问治疗是否会影响记忆。画面随后中断。"
                    : "对方听见治疗通知。工作人员接替监控，后续状态由实际治疗记录决定。",
                Disposition.Purify => patientId == PrototypeRules.Priest ? "乔纳低声重复净化这个词。屏幕关闭，没有展示他的去向。"
                    : "净化通知已送达。对方的会谈信号关闭，屏幕没有展示其去向。",
                Disposition.Release => patientId == PrototypeRules.Doctor ? "埃德温站起来，说离开并不代表疼痛已经结束。会谈信号关闭。"
                    : patientId == PrototypeRules.Priest ? "乔纳注视镜头，把离开理解为一次新的行动机会。会谈信号关闭。"
                    : "释放通知已送达。对方离开镜头；这次选择不会被写成真假或善恶判定。",
                _ => throw new ArgumentOutOfRangeException(nameof(disposition))
            };
            return "【开发占位】" + text;
        }

        private static string Observation(string id, int day) => id switch
        {
            PrototypeRules.Child => day == 1 ? "米洛频繁看向屏幕边缘，像在等待提示。" : "米洛试着直视镜头，随后又把目光移开。",
            PrototypeRules.FormerRuler => "薇拉在提到文件时看向镜头，停顿后才继续。",
            PrototypeRules.Doctor => "埃德温在机械声响起时闭眼片刻，随后寻找声源。",
            PrototypeRules.Diplomat => "阿德里安的视线在镜头与终端提示之间移动。",
            PrototypeRules.Priest => "乔纳持续注视镜头；画面无法说明这种专注的原因。",
            _ => "监控记录到一次目光移动，其原因仍需询问。"
        };

        private static string Hands(string id) => id switch
        {
            PrototypeRules.Child => "米洛反复把双手放回桌面的标记内。",
            PrototypeRules.FormerRuler => "薇拉将手掌压在桌面上，提到签名时松开。",
            PrototypeRules.Doctor => "埃德温握紧又松开双手，没有新的伤口信息可供确认。",
            PrototypeRules.Diplomat => "阿德里安把双手保持在镜头范围内。",
            PrototypeRules.Priest => "乔纳双手交握，伴随自己的陈述缓慢移动。",
            _ => "双手位于监控范围内，单项姿势不能决定处置。"
        };
    }
}

# 正式 Campaign 文本通顺度检查清单（2026-10-28）

本清单的 **28 个固定 ID 已全部处置：24 项已修复（32处文案），3 项按用户要求保留，1 项已在前次导入时被新版替换**。明确保留 `p2.12`、`p2.14`、`p1.7` 的原文；`p2.15` 不复用编号。P0/P1/P2 为原审计优先级，当前以各项状态和处理结果为准。[verified: text-fluency-repairs-2026-10-28.json fixedIssueIds/preservedIssueIds/edits][opinion]

下方“原文”和问题解释保留修改前的审计记录，当前实际文本见“本次处理”及[逐处修订记录](text-fluency-repairs-2026-10-28.json)。本次使用[已批准措辞适配清单](../../Tools/DialogueCampaign/approved-text-edits.json)生成内容，源工作簿保持原始字节，规则、跳转、数值、立绘和结局句数经修改前后比较均未改变。[verified: text-fluency-repairs-2026-10-28.json checks]

## 检查范围与证据口径

- 检查正式 Campaign 的 13 场候选会谈，含教学、全部可选应答及处置反应；另读监督员、三组结局对白、房间阅读物、档案摘要及主要菜单和操作提示源码。旧演示、第三方与参考游戏不纳入问题清单。[verified: C campaign.days；S conversations；E conversations；UnityProject/Assets/MiniGame/Resources/MiniGame/Readables；UnityProject/Assets/MiniGame/Runtime]
- 活动绑定为 `ActiveCampaignPackage.asset` 的 `packageSource` GUID `11a3a98431b46414fa69fc3565c97d64`，解析到 `ImportedDialogue/inspection-campaign/228214ade069d83c1c296b9049d1a58d/Package.minigame.json`；其 `contentVersion` 为 `1.2.2-development`。本次解析比较，活动包顶层 `yarnSource` 与当前生成包对应字段完全相同。[verified: UnityProject/Assets/Resources/MiniGame/ActiveCampaignPackage.asset packageSource；上述 Package.minigame.json.meta guid；当前执行的 JSON 字符串比较]
- 日常患者“表格位置”来自现有来源审计 `provenance.json` 的 `books[*].rows`；新版监督员位置来自本次直接读取的工作簿“对话流程”，结局位置来自 `Dialogue.json` 的 `sourceCell`。本次未在 Excel 界面人工逐格验收。[verified: C 的同目录 provenance.json；supervisor-dialogue-import-2026-10-28.json sourceCoverage；E 的同目录 Dialogue.json]
- 首轮结论来自静态文本与调用/跳转核对，p1.1、p1.7当时属于叙事或路线风险判断。用户现已批准修订p1.1，并决定保留p1.7。本次在已打开的Unity Editor中导入并运行相关自动化回归；这些结果与人工完整游玩、视觉验收分别记录。[opinion]

位置缩写（行号以本轮文件为准）：

| 缩写 | 文件 |
| --- | --- |
| C | [正式 Campaign Yarn](../../UnityProject/Assets/MiniGame/Content/Campaign/campaign.yarn) |
| S | [监督员 Yarn](../../UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor/Flow.yarn) |
| E | [结局 Yarn](../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Flow.yarn) |
| R | [录用通知书阅读文案](../../UnityProject/Assets/MiniGame/Resources/MiniGame/Readables/EmploymentNotice.asset) |

## P0：2 项已修复

### p0.1 【已修复】医生对白漏掉核心词语

- **本次处理**：“我曾经见过太多人了……他们因为敌人而愤怒，因为家乡遭难而悲伤，也因自己可能丧命而害怕……”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits]

- **位置**：游戏 Day1，`P04_D1_R4_CLOSE_002`；医生表“对话流程”F33。[verified: C:1323；provenance.json books[battlefield-doctor].rows[row=33]]
- **原文**：“他们因为敌人而愤怒，**因为家乡被而悲伤**，也因自己可能丧命而害怕……”
- **问题**：“被”后缺少谓语，句子明显残缺。[opinion]
- **建议**：补齐家乡遭遇的事件；不补充具体剧情时可写“因为家乡遭难而悲伤”。若要使用“被毁”等具体事件，应由文案确认。[opinion]

### p0.2 【已修复】薇拉日期问答直接错位

- **本次处理**：“我刚才在门口还念过……等一下，让我想想。”；“你问日期，是觉得我已经精神错乱了吗？我只是……一时记不清。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits]

- **位置**：游戏 Day2，`P03_D2_R2_OPTION_001 → P03_D2_R2_CLOSE_001`；薇拉表 E104、F106。[verified: C:3666–3667、C:2864–2865]
- **原文**：玩家“**你还记得今天的日期吗？**” → 薇拉“**我和那些人不一样，我不是精神错乱的异常体。**”
- **问题**：实际跳转直接连接上述两句；回答既不回应日期，也没有表现她把提问理解成质疑的过渡，读起来像接错台词。[opinion]
- **建议**：连同后续 `CLOSE_003/004` 一起调整，让她先回应日期或想不起日期，再表达被质疑的不满。例如先说“我刚才在门口还念过……等一下，让我想想”，随后再接防御性的回答。保留现有节点 ID 与数值。[opinion]

## P1：8 项已修复，1 项按要求保留

| 编号 | 状态 | 修改前位置与原文 | 原问题记录 | 本次处理 |
| --- | --- | --- | --- | --- |
| p1.1 | **已修复** | 医生游戏 Day3：“你还记得昨天说了什么吗？”→“记得一个选择，和一个没等到我的人。”；随后“如果我忘了那个人……”与“我当年替别人排序”。医生 E143/F144/D186/D199；节点 `P04_D3_R1_OPTION_003`、`CLOSE_007`、`COMMON_005/006`。[verified: C:7558、C:6768、C:7127、C:7237；医生来源审计对应节点] | 当前 Day1–2 主要谈情绪检测、战友、隔壁哀嚎及战争，并未明确交代这个人的身份和救援排序事件；Day3 却像在回顾玩家已经听过的一段往事。这是上下文缺口判断，需要文案确认。[opinion] | “你现在想起了什么？”；“我当年决定过先救谁。有个人没能等到我。”；“有个人没等到我的救助。如果我把他忘了，我会变得更安全，还是只是更安静？”；“我当年决定过先救谁。今天轮到你决定怎样处置我。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.2 | **已修复** | 医生游戏 Day3：“至少你不会再被触发。”→“触发需要治疗，不是抹除。”；医生 E191、F193，`OPTION_014/CLOSE_032`。[verified: C:7612、C:7182] | “人被触发”“触发需要治疗”都省去了具体症状；后一句尤其像抽象术语直译。[opinion] | “至少那些回忆不会再让你失控。”；“需要治疗的是我的症状，不是抹去我的记忆。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.3 | **已修复** | 医生游戏 Day3 释放：“打开门，让我保留后果。”；医生 H238，`ACTION_008`。[verified: C:7544] | “保留后果”搭配不成立，也不清楚是承担责任还是保留自主权。[opinion] | “打开门，让我自己承担后果。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.4 | **已修复** | 医生游戏 Day3：“别替我把最后一句说掉。”／“谢谢你没有替我把最后一句说掉。”；医生 D212、H236，`COMMON_007/ACTION_006`。[verified: C:7347、C:7522] | “说掉”含义不清，容易在代说、删掉、打断之间产生歧义。[opinion] | “无论你选什么，别替我说完最后一句。”；“你听完了。谢谢你让我自己把最后一句说完。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.5 | **已修复** | 薇拉游戏 Day1：“有本事问我为什么发火，别上来就替我认罪。”；薇拉 F11，`P03_D1_R2_CLOSE_002`。[verified: C:183] | “替我认罪”通常表示代替她承认罪行；此处她在反抗审查者预设自己有罪，施事关系不准确。[opinion] | “有本事问我为什么发火，别一上来就认定我有罪。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.6 | **已修复** | 神父游戏 Day2：“他们沉默并不空洞。”；神父表 D33，`P01_D1_R5_CLOSE_003`。[verified: C:5353] | 若“空洞”评价沉默本身，应有“的”；当前读起来像“他们沉默，但他们不空洞”，与后句“洁净后的语言”衔接不稳。[opinion] | “他们的沉默并不空洞。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.7 | **按要求保留** | 自审：“他们靠着自己的意志走出去了，外面的世界会欢迎他们吗？”；自审快照“对话流程”D11，`CUSTOM001_D1_R1_COMMON_010`。[verified: E:24；Endings/Dialogue.json self-review.lines sourceCell=D11] | 固定文案将“那些人已走出去”写作既成事实，但隐藏判定核对的是四名角色历史信任之和，不检查这些人是否释放。“他们”指谁以及是否与实际处置吻合，需要按路线确认；本轮没有做无释放路线复现。[verified: src/MiniGame.Core/Runtime/GameSession.cs:199–209；src/MiniGame.Presentation/EndingDialogueCatalog.cs:15–32][opinion] | 用户明确决定保留原文，本轮不修改；不再列为待改项。[opinion] |
| p1.8 | **已修复** | 监督员游戏 Day2：“最后被仪器检测不通过。”；监督员表 D25，`P02_D2_R1_COMMON_006`。[verified: S:34；Supervisor/Dialogue.json sourceRow=25] | 混用了“被仪器检测出……”和“未通过检测”两种句式。[opinion] | “之前有个审查员听信了里面那些人的话，最后没能通过仪器检测。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p1.9 | **已修复** | **新增：每日净化配额的教学说明缺失**。旧 Day1 句“每天最起码要净化一个人”已被替换；新 D12只说“绩效考核里可没有对于释放的指标”。[verified: 新监督员表“对话流程”D12–D14；S:13–15；supervisor-dialogue-import-2026-10-28.json changes] | 正式流程的每日净化目标仍为1，未达目标仍会触发后续结局。新版口头说明讲了释放不计指标，却没有解释什么计指标、每天需完成多少。界面仍显示配额，因此这是教学信息缺口，不是规则未实现。[verified: Tools/DialogueCampaign/conversion-config.json dailyQuota；src/MiniGame.Core/Runtime/GameSession.cs FinishDay；UnityProject/Assets/MiniGame/Runtime/TerminalView.cs directoryRequirement/dailyWorkRemaining][opinion] | “但是释放可不是什么好事呀。每天至少净化一个人，才算完成指标。释放可不算，记住了。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |

## P2：14 项已修复，2 项按要求保留，1 项前次已替换

| 编号 | 状态 | 修改前位置与原文 | 原问题记录 | 本次处理 |
| --- | --- | --- | --- | --- |
| p2.1 | **已修复** | 教学：“这玩意把我绑的严严实实呢。”；教学 D5，`P00_D0_R1_COMMON_004`。[verified: C:37] | 补语前“的”误用。[opinion] | “这玩意把我绑得严严实实呢。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.2 | **已修复** | 医生游戏 Day1 玩家选项：“但他确实让犯罪率降低了。”；医生 E22，`P04_D1_R3_OPTION_002`。[verified: C:1784] | 前文谈仪器/制度，使用“他”会误指男性人物。[opinion] | “我也不明白其中的原理，但它确实让犯罪率降低了。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.3 | **已修复** | 医生：“被炸的只剩半截身子”“你可以做的更好的”“被炸的粉碎”；监督员：“你的活干的还不错嘛。”[verified: C:1366、C:1388、C:1431；S:30] | 同类“的/得”问题，按一个批次处理。[opinion] | “我闭上眼睛依然能想起那个被炸得只剩半截身子的战友……”；“你可以做得更好，不是吗？！”；“好多的人…都死了…被炸得粉碎！！！我要去救他们！！”；“据我所知，你的活干得还不错嘛。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.4 | **已修复** | 医生游戏 Day2：“传言的发生是有依据的，对么？”；医生 F102，`P04_D2_R3_CLOSE_001`。[verified: C:3884] | “传言发生”搭配生硬。[opinion] | “这些传言总有些依据，对么？”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.5 | **已修复** | 薇拉游戏 Day1：“请你自己回去亲自翻”；薇拉 F30，`P03_D1_R3_CLOSE_007`。[verified: C:348] | “自己/亲自”重复。[opinion] | “请你回去亲自翻看原件，别直接把我的话当结论。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.6 | **已修复** | 薇拉游戏 Day1 释放：“是你开的吗。”；薇拉 H93，`P03_D1_R6_ACTION_003`。[verified: C:887] | 疑问句用了句号；“开”的对象不明确。[opinion] | “是你决定放我走的吗？”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.7 | **已修复** | 薇拉游戏 Day3：“你有没有把我当作一个人听完。”；薇拉 F256，`P03_D3_R6_CLOSE_004`。[verified: C:6210] | “听完一个人”搭配生硬，“听完”的对象应是她的话。[opinion] | “现在...我更想知道，你有没有把我当作一个人，认真听我把话说完，而不是只等我认罪或者服从。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.8 | **已修复** | 薇拉游戏 Day3：“再给我说完这一句，好吗？”；薇拉 F255，`P03_D3_R6_CLOSE_003`。[verified: C:6199] | “给我说完”可能被读成要求对方说，而她是在请求继续发言。[opinion] | “...听见了。再让我把这一句说完，好吗？”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.9 | **已修复** | 外交官游戏 Day3：“我把手续办得快，当成接待工作的周到。”；外交官 F37，`P05_D1_R4_CLOSE_004`。[verified: C:9368] | 前半句容易先读成“我把手续办得快”；“当成……的周到”也不顺。[opinion] | “没有。我当时觉得，手续办得快，是接待人员照顾周到。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.10 | **已修复** | 外交官游戏 Day3：“就比较相信后面的‘同意’出于自己。”；外交官 F66，`P05_D1_R6_SPECIAL_002`。[verified: C:9632] | “比较相信”不及“更容易相信”自然；“出于自己”缺少明确意愿。[opinion] | “谈判时，我常先提一个很容易被拒绝的小要求。对方说过一次“不”，就更容易相信，后面的“同意”是自己真心作出的决定。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.11 | **已修复** | 神父结局：“门叩响的声音。”；神父结局表 D4，`CUSTOM001_D1_R1_COMMON_003`。[verified: E:5] | “门叩响”像门主动叩响；名词短语略拗口。[opinion] | “叩门的声音。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.12 | **按要求保留** | 录用通知书阅读旁白：“我的录用通知书，那是我人生中最开心的一天。”[verified: R:18] | 先指物件，再用“那”指一天，缺少时间衔接。[opinion] | 用户明确决定保留原文，本轮不修改；不再列为待改项。[opinion] |
| p2.13 | **已修复** | 薇拉游戏 Day2：“按完两套规定，最后没饭吃”；薇拉 F119，`P03_D2_R3_CLOSE_002`。[verified: C:2963] | “规定”通常遵守或执行，不说“按完规定”。[opinion] | “可一个有工作的人，两套规定都照办了，最后却没饭吃...那规定就该改。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.14 | **按要求保留** | 自审：“我的工作……我是一名公务员，我的工作是按下按键，决定他们的命运。”；自审快照 D4，`CUSTOM001_D1_R1_COMMON_003`。[verified: E:17] | “我的工作”短距离重复；“按下按键”可简化。开头迟疑本身可保留。[opinion] | 用户明确决定保留原文，本轮不修改；不再列为待改项。[opinion] |
| p2.15 | 前次已替换 | **已被新版替换，不再待改**。旧句“只有你的教育取得被仪器认可的结果之后，释放按钮才会亮起”已不在当前内容。 | 历史问题保留编号；原有长定语问题已随源文替换消失，不将新句再次计为旧问题。[verified: 本次监督员新旧源差异；S:11–12][opinion] | 保留历史编号，本次没有新增修改。[verified: supervisor-dialogue-import-2026-10-28.json] |
| p2.16 | **已修复** | **新增：治疗引号方向反写**。监督员 Day1：`在”治疗“的时候`；新监督员表 D8，`P02_D1_R1_COMMON_007`。[verified: S:9] | 左右引号颠倒，属于新增正文中的明确标点问题。[opinion] | “你可以继续治疗，把他们留到第二天继续教育，这样我们兄弟几个在“治疗”的时候也能找点乐子。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |
| p2.17 | **已修复** | **新增：释放指标措辞拗口**。监督员 Day1：“绩效考核里可没有对于释放的指标”；新监督员表 D12，`P02_D1_R1_COMMON_011`。[verified: S:13] | “对于释放的指标”搭配不自然，也不如直接说明释放与考核的关系清楚。[opinion] | “但是释放可不是什么好事呀。每天至少净化一个人，才算完成指标。释放可不算，记住了。”。[verified: approved-text-edits.json；text-fluency-repairs-2026-10-28.json edits] |

## 本次修订与验收

- 24项修订通过32处精确文本适配生效：每处绑定源工作簿哈希、原节点ID、字段、修改前后全文与问题ID。任何源哈希或原文变化都会停止生成；不作模糊替换。[verified: approved-text-edits.json；Tools/DialogueCampaign/approved_text.py]
- `p0.2`在现有薇拉日期应答链内补足回应；`p1.1`不再假定昨天讲过救援排序，并让共享段落明确指代未获救助的人；`p1.9`将每日净化指标补回监督员现有句子，监督员仍为48句。[verified: approved-text-edits.json 对应issueIds；text-fluency-repairs-2026-10-28.json edits/checks]
- `p2.12`阅读物文件逐字节不变，`p2.14`与`p1.7`所在自审16句全部不变；八份源Excel也逐字节不变。比较确认患者、日程、阈值、信任和处置增量、节点/连线、立绘、监督员晨检/简报分段及结局句数均保持原样。[verified: text-fluency-repairs-2026-10-28.json checks/baselineProtectedHashes]
- 生成、活动绑定和最终自动化验证结果见[本次验证记录](text-fluency-repairs-validation-2026-10-28.json)。没有进行人工完整游玩或Player构建。[verified: text-fluency-repairs-validation-2026-10-28.json][opinion]
- 最终结果：对白工具39项、固定.NET SDK 8.0.424的474项测试、Unity EditMode布局5项、PlayMode监督员8项和结局12项全部通过；Release构建0警告、0错误，`git diff --check`通过。[verified: text-fluency-repairs-validation-2026-10-28.json python/dotnet/unity；本次git diff --check]

## 前次监督员导入记录（历史）

- 监督员源文件由用户提供的新版逐字节复制到原绑定路径，SHA-256 为 `71a1bff89ffcfee40d96a3543611e87d8eb361c3fa550ab4854fb63d0f44119e`；48句按 Day1–4 分为18/12/11/7句，Day1–3 的最后两句仍在检测后的简报阶段播放，Day4七句正文未变。[verified: [导入来源差异](supervisor-dialogue-import-2026-10-28.json)；S；Supervisor/Dialogue.json conversations]
- 对新旧正文、节点、连线、立绘及工作表行数逐项比较后，更新来源锁定、监督员48句校验和相关测试。未因新增内容重建主场景。[verified: supervisor-dialogue-import-2026-10-28.json changes/sourceCoverage/adaptations；Tools/DialogueCampaign/build_supervisor.py；UnityProject/Assets/MiniGame/Runtime/SupervisorDialogueContent.cs]
- 自审快照本地曾为CRLF；仅恢复LF后，原锁定哈希 `ff782d70cd011511e84301e5bdfcdf123839ea3f0e7aae493c537d1cef14e2b1` 精确匹配。没有改自审正文或放宽哈希校验。[verified: docs/ending-content/sources/source-refresh.json self-review.sha256；本次文件字节比较]
- 内容生成与检查、Unity导入和回归结果见本次最终验证记录。[verified: [验证结果](supervisor-dialogue-validation-2026-10-28.json)]
- 本次对白工具最终33项测试、Unity EditMode布局5项、PlayMode监督员播放8项均通过；PlayMode逐句覆盖48句及扫描音、立绘和检查点恢复。现有Editor实际为2022.3.23f1，工程版本文件记录2022.3.23f1c1；本次使用已打开的Editor，未重启、升级或改写版本文件。[verified: supervisor-dialogue-validation-2026-10-28.json contentCommands/editMode/playMode/editorVersion]

## 执行边界与复核建议

- 本表按“修改事项”计数，同类多处“的/得”归为 p2.3 一项；不等于28个单句，其中 p2.15 已关闭。省略号、连续问叹号、故意断句、神父的信仰表述、监督员的讽刺和被遮挡档案，未仅因不标准或不友善而列为问题。[opinion]
- 日常对白应回到 `Art/chat_excel` 的对应源表，或使用明确且经审阅的适配规则；医生 Day3 同时受 `Tools/DialogueCampaign/source-repairs.json` 的文本前置条件约束。改词后必须复核修复条件，不能只换哈希。[verified: Tools/DialogueCampaign/conversion-config.json patients；Tools/DialogueCampaign/source-repairs.json books.battlefield-doctor；AGENTS.md「内容配表与导入」]
- 自审源为 [revision18 完整快照](../ending-content/sources/self-review-revision18/snapshot.json)，并非当前六份日常患者表之一；监督员和神父结局使用独立工作簿。应走来源更新、审计、生成和现有 Unity 导入链，保持自审16句及最终净化动作等契约。[verified: Tools/DialogueCampaign/build_endings.py verified_sources/read_snapshot/linear_conversation；docs/ending-content/sources/source-refresh.json sources]
- 阅读物后续修改前应核对资源的配置与重建入口，避免只改副本后被重建覆盖。[opinion]
- 人工复核时可逐条走涉事应答链，重点检查 p0.2 的连续四句、p1.1 的新旧回忆关系和 p1.7 的不同处置历史。自动化修订完成与人工完整游玩验收应分别记录；p1.7按用户要求保留，不再要求改词。[opinion]

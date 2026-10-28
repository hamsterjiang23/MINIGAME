# 对白信任与处置情绪分离、选项反馈平衡

2026-10-28按用户要求实施：长期信任仅累计对白选项增减；治疗、净化和释放的增减只表现当前回应的情绪波动，不进入次日和任何条件判断。正式活动包已通过既有导入菜单更新；隐藏条件仍为Day3梦结束时四位患者历史信任总和≥10。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs SocialAdaptationTrust/Dispose/HasHiddenEndingTrust；trust-rebalance-2026-10-28.json activePackage]`

## 规则与存档

- 回答继续写入患者自己的有符号 `Trust`；退场不清零。释放、特殊对白、隐藏条件均只读该值。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs Select/CanRelease/StartDialogueOrDisposition/HasHiddenEndingTrust]`
- 处置提交仍一次性更新人物去留、治疗次数、配额和次日袭击事件。回应期间的社会适应度读数为“对白信任＋本次处置情绪增减”，使用Int64计算，回应结束即不再叠加。没有第二份权威信任或UI缓存分数。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs Dispose/SocialAdaptationTrust/FinishReaction；../../src/MiniGame.Core/Contracts/GameResult.cs SocialAdaptationReading]`
- 存档保存原有接受命令和处置记录，重放可恢复同样的临时波形；重复处置或过期回调不会叠加。含处置增减的规则指纹加入策略版本2，旧累计语义的存档被拒绝；无扩展旧原型的既有指纹不变。`[verified: ../../src/MiniGame.Core/Rules/GameRules.cs ComputeFingerprint；../../src/MiniGame.Core.Tests/DispositionEmotionTests.cs；CampaignRulesTests.LegacyFingerprintStaysCompatibleWhileIntroductionAndDispositionDeltasAreFingerprintInputs]`

## 选项方案

采用“承接与诚实沟通+1，具体倾听、帮助、安全感和自主权支持+2；轻度否定−1，强烈羞辱／威胁保留−2或−3”的幅度。回应中生气不自动等于掉信任，沉默或服从也不自动等于安抚成功；按玩家表达、人物处境和后续回应共同判断。程序性开场、单纯追查和识破操纵仍可为0。医生已明确指定的−10和神父固定开场+100保留。`[opinion: 本轮配表原则；verified: ../../Tools/DialogueCampaign/trust-adjustments.json policy/adjustments]`

本轮相对开始时的工作区共改变91个正式排期选项：薇拉29、医生24、小孩30、外交官8。每项在显式适配清单保留稳定ID、原文、源表未适配净值、最终值和理由。当前清单合计136项，包括此前修订和14项未排期内容。`[verified: trust-rebalance-2026-10-28.json adjustments；../../Tools/DialogueCampaign/trust-adjustments.json]`

| 选项 | 修改前→后 | 反馈依据 |
| --- | --- | --- |
| 医生Day1：“你的问题都会被监控记录下来。” | 0→+1 | 给疑问留下记录，即使尚未解除他的不满，也有有限的正反馈。 |
| 医生Day2：“说来听听看，你都听见什么了？” | 0→+2 | 接住他对隔壁病人安危的担忧，允许讲述，而不是直接称其幻听。 |
| 医生Day3：“我理解你的拒绝。” | 0→+2 | 承认拒绝权；对方继续追问责任，不抹掉这份承认。 |
| 小孩Day1：“放在膝盖上就可以。” | 0→+2 | 给紧张的孩子一个具体、可执行且无需持续迎合审查的动作许可。 |
| 小孩Day2：“那就先到这里。” | 0→+1 | 允许暂停，不把所有正反馈都集中在追问更多内容上。 |
| 医生Day1：“看到了吗，你刚才情绪很激动。” | 0→−1 | 用激动反证危险、引出自我怀疑；服从与沉默不能替代信任。源表−5与回应+5先合并为0，再显式适配为−1。 |
| 医生Day3：“忘记也许会让你轻松。” | 0→−1 | 表面安慰，却忽视他最担心的人格和记忆丧失。 |
| 医生Day3：“我必须遵守流程。” | 0→−1 | 在已明确表达拒绝时以流程压回，缺少承担选择责任的回应。 |
| 小孩Day3：“你知道自己做了什么？” | 0→−1 | 对记忆受损者预设责任，形成责备感。 |

`[verified: ../../Tools/DialogueCampaign/trust-adjustments.json 对应sourceId/expectedEffectiveDelta/trustDelta/reason；../../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json mappings/adaptations]`

## 随机数值验算

口径为每个实际提供的对白选项等概率、每次合法处置等概率。保留释放门槛、特殊对白门槛、净化配额、人物退场、神父释放后自然次日袭击和提前结局。只合计薇拉、医生、小孩、外交官的历史信任；神父仍参与合法处置和配额，但不加入总和。DayN数据以实际到达DayN的开局为条件，统计该日工作结束时的累计值；不要求当天配额达标才纳入，因为Day3隐藏判定先于次日失败。`[verified: ../../Artifacts/TrustRebalance20261028/evaluate.py；../../src/MiniGame.Core/Runtime/GameSession.cs ResolveDreamReturnPhase/ResolveEnding/AdvanceDay]`

| 日期 | 期望 | 中位数 | 最低 | 最高 | 全部开局到达率 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Day1 | 8.3611 | 9 | −14 | 26 | 100% |
| Day2 | 10.3034 | 11 | −28 | 48 | 80.5686% |
| Day3 | 18.1817 | 18 | −41 | 83 | 36.7678% |

精确有理数枚举：Day3到达者中82.2584%达到≥10；乘以到达率，全部开局的隐藏入口达成率为30.2446%。不是全部开局都能到Day3，也不保证每条随机路线成功。若仅分离处置情绪、不修改选项数值，同口径Day3期望为4.8975、条件达成率27.8237%、全部开局11.6943%。`[verified: trust-rebalance-2026-10-28.json analysis.final/analysis.reactionOnlyBeforeBalance]`

独立使用真实 `DialoguePackageCompiler`、`GameSession` 和合法命令运行2万局（种子20261028），前100局在处置回应时实际保存／重放恢复。观察到Day3到达率37.545%、条件隐藏达成率81.9284%、全部开局30.76%；各日均值和达成率均在精确结果六倍标准误范围内。该样本用于交叉核对，理论极值来自精确枚举，不使用样本最值。`[verified: ../../Artifacts/TrustRebalance20261028/Probe/Program.cs；trust-rebalance-2026-10-28.json analysis.coreSimulation/analysis.validation]`

## 本轮验证与边界

生成前先执行smoke，随后生成和check、监督员check、结局check及39项对白工具测试。固定SDK8.0.424按顺序locked restore、Release build、Release test：0警告／0错误，Core160项与Presentation322项通过，共482项。`[verified: trust-rebalance-2026-10-28.json checks.python/checks.dotnet]`

Unity实际Editor为2022.3.23f1，工程文件保留本轮开始时的2022.3.23f1c1版本记录，没有改动Editor或ProjectSettings。正式Campaign原生导入、映射节点EditMode检查、3项社会适应度PlayMode和2项处置PlayMode通过。扩展的5项导入器EditMode中2项通过、3项V1旧单日包一致性检查失败，错误为旧包Runtime.yarn不可变来源不同／ActiveDialoguePackage来源与原生Yarn不匹配；未修复或重新生成旧单日演示，不把该扩展集报告为全通过。`[verified: trust-rebalance-2026-10-28.json checks.unity]`

源Excel、主场景和全部ProjectSettings相对本轮开始时逐文件SHA-256保持不变。按去除 `#trust` 数值后的JSON逐项比较，角色、日程、门槛、文案和图结构不变；处置情绪配置也保留。活动资产指向的PackageSource与本次生成包逐字节一致。未做Player构建、完整人工游玩或视觉验收。`[verified: trust-rebalance-2026-10-28.json invariants/activePackage/limits]`

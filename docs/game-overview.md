# 我们要完成什么游戏

> 本文保留飞书设计资料的综合解读；当前已建立Unity开发原型，完成情况与待补项见[研发状态](project-status.md)，验证范围见[验证汇总](gameplay-migration-verification.json)。来源逐篇保存在 [sources](source-index.md)，互相矛盾的设计列在[待定问题](open-questions.md)，参考代码版本见[仓库记录](source-lock.md)。[verified: gameplay-migration-verification.json unity.sceneGeneration/playMode/windowsMonoBuild]

2026-10-15已同步主Wiki608版及其主角、梦场景和小孩对白表；变动、原型差距与访问缺口见[本次同步记录](feishu-sync-2026-10-15.md)。[verified: .source-data/game-wiki.json revision_id；.source-data/sync-2026-10-15.json changes]

## 1. 一句话定位

一款**第一人称、2D/3D 混合的心理恐怖叙事游戏**：玩家成为一名刚入职的检定员，在国家分配的员工公寓内通过电脑监控审查五位病人，在净化配额、病人的陈述和自己的判断之间，选择治疗、净化或释放；日复一日的工作、晨检与梦境逐渐揭露制度的真实作用，以及玩家自己也在被审查的事实。

工作中的主要乐趣来自观察、对话、信息对照和有后果的选择。恐怖来自熟悉日常的异化、系统对信息的屏蔽、善意选择可能带来的危险，以及对自身处境的重新理解。

依据：[主 Wiki](sources/game-wiki.md)、[程序需求 v0.1](sources/program-requirements-v0-1.md)、[游戏梳理](sources/game-concept-outline.md)、[叙事白皮书](sources/narrative-whitepaper.md)。

## 2. 这次要交付的范围

| 项目 | 当前口径 | 解释及边界 |
| --- | --- | --- |
| 体验 | 正式全流程相同玩家体验 | 主 Wiki 原文；需要做出完整日循环、选择及后果，但这句话的具体验收标准仍待确认 |
| 内容长度 | 3 天 | 主体为 Day1–Day3；主 Wiki另写了 Day0教学和 Day4失败收束，是否都在3天预算之外尚未确认 |
| 病人 | 5 位 | 神父、小孩、女性执政者、战场医生、外交官；教学病人0是否另外制作待确认 |
| 结局 | 交付标准3种，正文已有4条描述 | 主Wiki新增反叛值触发的自我净化分支；是否纳入Mini、冲突顺序与阈值仍需裁决。[verified: .source-data/game-wiki.json content「Mini Game交付标准」「结局」] |
| 空间 | 有限可探索的公寓/工作空间 | 3D场景承载移动与道具交互，2D界面承载监控审判与文书 |

白皮书的5日结构、提案中的5–6名病人/2–3结局、额外章节与商业化蓝图都单独保留为背景或扩展方向，不扩大上述Mini Game目标。依据：[主 Wiki](sources/game-wiki.md)、[早期方案15](sources/brainstorm-proposal-15.md)。

## 3. 玩家是谁，世界发生了什么

主角的共同设定是一名通过严格选拔、追求稳定与上升机会的底层青年，初期相信自己正在维护和平。最新梳理称其为 **Steve**，隶属**和平保障总署**；其他来源使用健康部、和平部、审查员等名称。已读[世界观名词表](sources/world-terminology.md)使用和平部、和平仪与评估员，另列候选名称；它与最新梳理的名称不同，最终用词仍待统一。

一场名为“大狂乱”或旧世界大战的灾难之后，社会用定期检测来预防危险。官方把某种“战争倾向因子”包装成可检测、可治疗的问题；人被仪器标成异常后，会被收容并接受处置。

资料明确给出的真相是：仪器检测的是**情绪波动**，不能可靠地预测犯罪或区分善恶。恐惧、悲伤、愤怒和创伤可能产生异常读数，冷静的危险人物却可能获得正常结果。玩家必须从人的陈述和经历中形成判断，同时承受制度对其自身行为的记录与评估。

对白、官方文件与环境证据需要呈现两层信息：机构如何解释治疗和净化，以及病人的具体经历如何使这些解释产生裂缝。新增[主角专篇](sources/player-character.md)将举报妹妹、选择性记忆断层与公务员奋斗目标串为同一主角经历，姓名仍未定；与梳理中的Steve称呼和机构命名仍需统一。主角Day4–5仅保留为完整主线方向，不能自动扩大Mini范围。[verified: .source-data/player-character.json content「人物概况」「背景」「设计约束与待定项」] [opinion]

依据：[游戏梳理](sources/game-concept-outline.md)、[叙事白皮书](sources/narrative-whitepaper.md)、[灵感墙](sources/inspiration-wall.md)。

## 4. 玩家的一天

**醒来/敲门 → 监督员与晨检 → 接收任务/指标 → 电脑工作 → 病人对话与处置 → 夜间收束 → 睡眠与梦境 → 次日反馈。**

| 环节 | 玩家行为 | 应传达的体验 | 来源状态 |
| --- | --- | --- | --- |
| 公寓交互 | 第一人称移动，按E调查物件、开门、操作电脑和床 | 工作与生活被压缩进有限空间 | 程序需求明确无玩家肢体、无跳跃 |
| 晨检 | 接受检测，面对逐渐触及工作感受的问题 | 维护自身合格身份的压力 | 主 Wiki明确仪器检测；白皮书提出紧贴太阳穴按住3秒 |
| 监督员/任务 | 接收每日任务或配额 | 制度进入私人空间 | 主 Wiki写监督员对话；白皮书写监督员不发声，需统一 |
| 工作终端 | 从3D房间进入可独立交互的2D审判界面 | 仿佛能控制一切的工作位置 | 摄像机移动、开机遮挡加载是程序提案，过渡实现尚未验证 |
| 病人审判 | 对话、建立信任，之后决定处置 | 了解人的经历后承担选择责任 | 多轮结构和三键规则见下一节 |
| 夜间收束 | 回顾白天，睡眠、做梦 | 白天被屏蔽的信息与主角心境浮现 | 日记在早期提案/白皮书中；梦境在主 Wiki日流程中 |
| 次日 | 根据昨日处置看到病人变化、缺席或特殊访客 | 选择的后果进入日常 | 神父释放后替代监督员是明确特例 |

最新主流程画板在每日开始前新增结局检查，并要求普通梦后压暗场景、给床打舞台聚光灯；Day3反叛值分支则由主Wiki要求改为电脑亮、床不亮。两个流程分支需分别设计，不能将普通梦后床交互规则无条件套用到新增分支。[verified: .source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14/o1:12；.source-data/game-wiki.json content「结局」] [opinion]

Day0原文流程为电脑开机、审判病人0、净化病人0、睡觉与做梦。主流程画板进一步说明：从系统加载进入审判，与特殊NPC对话、净化、关机回房，使用舞台聚光灯引导玩家按E上床。画板还注明梦播完后恢复自由移动，但只允许与床交互；再与床交互进入黑屏敲门、按E醒来的下一日循环。Day1–3每天审判仍可出现的病人；角色若被释放、净化或达到终止条件，不能机械地继续按“每天五人”安排返场。该调度关系尚需统一配置。

依据：[主 Wiki](sources/game-wiki.md)、[程序需求](sources/program-requirements-v0-1.md)、[方案15](sources/brainstorm-proposal-15.md)、[角色设计](source-index.md)。

## 5. 审判、对话与代价

程序需求给出三种处置，操作只能在对话结束之后进行：

| 按键 | 意义 | 已描述的后果 | 未确定部分 |
| --- | --- | --- | --- |
| 隐藏绿键：释放 | 玩家愿意让病人离开 | 人物线结束；神父特例会在次日袭击玩家 | 解锁和灯色阈值、释放代价、对玩家评估的计算方式 |
| 黄键：治疗 | 延后终局，继续了解病人，同时施加痛苦 | 普通病人次日返场并逐渐迟缓；小孩/执政者/医生第三次治疗死亡 | 神父与外交官存在特殊终止或保护机制问题 |
| 红键：净化 | 终极处置 | 人物从审判队列消失；当日净化配额数字减一 | 净化后命运在不同来源有不同说法；不应据此增加未确认场景 |

玩家面临的压力由三个层面构成：当日必须净化多少人的**可见配额**；每个病人独立维护的**隐藏信任**；对玩家偏离制度、共情或探问行为的**隐藏评估**。程序需求提到疯狂值，梳理提出偏离指数，名词表另有狂热值，主Wiki608版与流程画板新增反叛值。四种名称的语义和计算关系尚未统一，不能直接合并成一个变量。[verified: sources/program-requirements-v0-1.md「疯狂值」；sources/game-concept-outline.md「偏离指数」；sources/world-terminology.md「狂热值」；.source-data/game-wiki.json content「结局」] [opinion]

### 对话结构

- 每个角色每日普通对话计划4–5轮，允许角色差异。
- 每轮角色说1–4句话，玩家最多从4个回复中选择一句。
- 回复导致不同角色对白，再收束到共用下一套回复。
- 满足该角色、该日的信任阈值，普通对话结束后进入一轮独有特殊对话。
- 处置后播放对应反应文本，并以花屏结束本次审判。
- 对话结束的红绿灯决定能否释放；灯色暂定与信任相关。

新增[小孩v1.1完整对白表](sources/dialogue-child-v1-1.md)已给出Day1–3的R1–R6对话与R7措施轮：捧哏、半分支的信任变化为0，真分支为−3/0/+3。它与程序需求的4–5轮及原型默认4轮、±10并不一致；保留角色来源口径，接入前需确认轮次模型与数值。[verified: .source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第10–11、20、30–33行；../src/README.md「开发默认规则」] [opinion]

“信任0–100”“阈值50”和特殊对白更大的信任变化均为原文示例或拟议规则。项目尚无最终数值表。台词只能引用玩家实际说过、做过和已经揭露的信息。

依据：[程序需求](sources/program-requirements-v0-1.md)、[游戏梳理](sources/game-concept-outline.md)、[外交官](sources/patient-diplomat.md)。

## 6. 五位病人分别承担什么体验

| 病人 | 主要叙事作用 | 跨日推进 | 处置特例/界限 |
| --- | --- | --- | --- |
| [神父乔纳](sources/patient-priest.md) | 让真诚信仰与危险狂热并存；主动渴望净化 | 角色设计写Day2出现；治疗使其愈加偏执 | 释放后次日杀死主角；持续治疗最终自我手术死亡；背景与最新梳理不同 |
| [小孩米洛](sources/patient-child.md) | 呈现情绪正常但被制度收容的无辜者 | Day1怕答错，Day2努力听话，Day3敢表达思念 | 无危险计划或身份反转；第三次治疗死亡；避免直接儿童伤害画面 |
| [女性执政者薇拉](sources/patient-former-ruler.md) | 责任、替罪与申辩权之间的复杂判断 | 恢复程序 → 承认自己的签名 → 申辩权是否属于所有人 | 并非纯粹无辜；不预设改良后的公正执政者或政治后续 |
| [战场医生埃德温](sources/patient-battlefield-doctor.md) | 分开理解创伤、危险与真实责任 | 质疑治疗 → 坦白救援取舍 → 渴望止痛但害怕遗忘 | 释放不等于康复；不增加战争闪回；第三次治疗死亡 |
| [外交官阿德里安](sources/patient-diplomat.md) | 让“审查者也被审查”的反转成为具体人物体验 | 最舒服的对象 → 知道太多 → 审查方向反转 | 只读取实际工作记录；实际治疗/净化风险、保护机制与报告影响未定 |

这五份当前角色设计与梳理中的病例A–F、白皮书中的人物原型是不同层级的材料。病例样例不能直接变成新增出场角色或覆盖现有人设。

## 7. 结局与可感知后果

主Wiki608版的交付标准仍写3种结局，正文已列四条：[verified: .source-data/game-wiki.json content「Mini Game交付标准」「结局」]

1. **正常流程，Day4起床被带走**，原文条件为“无”。[verified: .source-data/game-wiki.json content「结局」第1条]
2. **释放神父后遭袭**，角色文档仍明确为次日替代监督员出现。[verified: .source-data/game-wiki.json content「结局」第2条；sources/patient-priest.md「释放」]
3. **任意一天KPI未完成，跳转Day4**；流程正文写检测失败后永久锁房，画板却将前一天KPI失败归为“常规结局”。[verified: .source-data/game-wiki.json content「全流程」「结局」第3条；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14]
4. **反叛值触发自我净化**：Day3梦后床不亮、电脑亮，监控中出现自己，唯一选项“净化”，执行后进入超自然场景。正文条件为反叛值max；画板写天数≥4且反叛值大于阈值。[verified: .source-data/game-wiki.json content「结局」第4条；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14]

第四条已不再是占位“r”。需要确认其是否替代某个既有结局、是否纳入Mini，以及KPI去向、阈值和同时满足条件时的顺序。画板中的列举顺序不能直接当作已确认的程序优先级；当前原型的三种结局和日终检查也不能当作新来源已经接入。[verified: .source-data/history/2026-10-12/game-wiki.json content「结局」；../src/README.md「开发默认规则」] [opinion]

## 8. 表现与制作内容

| 类别 | 要准备的内容 | 控制范围 |
| --- | --- | --- |
| 3D | 小型公寓/走廊、门、电脑、床与交互物件 | 有限布局与重复探索；没有大规模场景承诺 |
| 2D | 五位病人、表情/状态差分、文书、任务单、监控UI、梦境画面 | 按角色实际日程和已确定事件拆分 |
| 演出 | 晨检、电脑进入/退出、红绿灯、花屏、敲门、结局和梦境 | 3D与2D切换必须顺滑；具体方案待验证 |
| 音乐 | 开始菜单、主玩法、梦境、分支结局 | 按状态切换；来源链接是参考，授权与正式使用尚待确认 |
| 音效 | 电脑噪音、仪器扫描/亮灯、对话弹出、放映机、3D与监控按键 | 主 Wiki列出的具体需求 |

白皮书明确要求虚构政治符号、避免直接血腥处刑与儿童伤害画面，并通过阳光、关怀与秩序外观下的裂缝制造恐怖。角色文档也强调优先用停顿、表情和动作变化表现治疗结果，限制额外资产。

引擎版本、对话工具、存档格式与AI管线尚未由这些Wiki材料确定。既有[源码与Unity研究报告](../Dependency/寻找伪人_相似玩法源码与Unity开发研究报告_2026-10.md)是技术选型参考；其中的身份生成、资源模型和Unity6建议不自动成为本游戏需求。

新增[梦场景专篇](sources/dream-scenes.md)已经描述Day0鲸尸海滩、Day1无尽长路、Day2枯花田、Day3受缚待审，附四条音乐参考和12张图片；主Wiki另补病房与主角场景概念图及六条音效参考。均为来源素材，不能当作已接入原型或已获成品使用授权。[verified: .source-data/dream-scenes.json content；.source-data/game-wiki.json content「美术」] [opinion]

另有用户提供的[Day1终端交互原型v0.5.2](sources/inspection-terminal-day1-prototype.md)：原生640×360、对象目录/审查页、复古控件皮肤、应答与实际历史、核验灯、处置反应及返回目录。仅小孩Day1已配置，内嵌数据与Excel主表一致；初始配额5、释放阈值3都是测试设置，不替代正式规则或当前Unity配置。[verified: .source-data/inspection-terminal-day1-v0.5.2.json native_canvas/configured_patients/data_matches_xlsx_day1/default_test_quota/default_test_threshold] [opinion]

## 9. 开发前还缺什么

第一优先级是统一3天内的角色调度、每日KPI、信任与释放门槛、交付3结局与正文四条分支的范围、条件及优先级，以及Day0/Day4的制作范围。随后接入已提供的小孩对白与梦场景方向，补齐其余正式内容、资产清单与输入/界面切换规则。[verified: open-questions.md Q12/Q14/Q25/Q26] [opinion]

2026-10-15已复查13篇正文、3份可读工作簿的12个子表与6块画板169节点。新增主角、梦场景、小孩对白；用户Excel与Day1 HTML也已归档核对。排期明确排除，已知引用链与本轮附件完成同步；空间目录及另两份工作簿原始Excel的获取限制仍单独记录。灵感树中的19天以上结构与六类结局继续保留早期设想身份。完整证据见[来源索引](source-index.md)和[同步记录](feishu-sync-2026-10-15.md)，冲突见[待定问题](open-questions.md)。[verified: .source-data/sync-2026-10-15.json body_sources_read/sheets_read/whiteboard_nodes/completion_scope/local_deliveries] [opinion]

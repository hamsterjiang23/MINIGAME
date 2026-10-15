# 设计冲突、待定项与访问缺口

> 本清单记录原文之间的差异与缺失，不替策划裁决。当前Mini Game范围采用主Wiki交付标准，正文逐篇保留原口径。

## 1. 开发范围与流程

| 编号 | 问题 | 来源与差异 | 对开发的影响 |
| --- | --- | --- | --- |
| Q01 | 3天与5天 | [主Wiki](sources/game-wiki.md)仍为3天；[白皮书](sources/narrative-whitepaper.md)与[主角专篇](sources/player-character.md)有Day1–5，后者明确Demo重点Day0–3。[verified: .source-data/player-character.json content「设计约束与待定项」] | 需要决定长线揭露哪些压缩入Mini Game |
| Q02 | Day0/Day4是否计入预算 | 主Wiki有教学病人0和失败Day4 | 影响角色、美术、对白与结局制作量 |
| Q03 | 五人如何每日出场 | 主Wiki写每天审判12345；[神父](sources/patient-priest.md)Day2出现；已释放/净化者线结束 | 要补逐日队列和退出条件；不能每天固定召回终止角色 |
| Q04 | 正式全流程相同体验如何验收 | 已有[Day1终端HTML](sources/inspection-terminal-day1-prototype.md)展示一位病人的目录、会谈、处置和返回流程；没有覆盖房间与跨日。[verified: .source-data/inspection-terminal-day1-v0.5.2.json configured_patients/runtime_browser_test；sources/inspection-terminal-day1-prototype.md「会谈与处置流程」] | 参考终端交互，仍需定义完整日循环、选择后果及正式验收。[opinion] |
| Q05 | 三键与四类处置 | [程序需求](sources/program-requirements-v0-1.md)释放/治疗/净化；[梳理](sources/game-concept-outline.md)另有继续观察 | UI与调度目前按三键解读，观察是否独立操作待裁决 |
| Q06 | 绿键含义 | 程序需求是隐藏释放；白皮书员工守则是继续观察 | 影响文案、配额与患者是否离队 |
| Q07 | 每日KPI数值、检查时点与去向 | 主Wiki608版写任意一天未完成跳Day4；画板新增每日开始前检查前一天KPI，归为“常规结局”；正文Day4写永久锁房。正式每日配额仍未填；HTML测试剩余5不能覆盖原型目标1或当作正式配额。[verified: .source-data/inspection-terminal-day1-v0.5.2.json default_test_quota；../src/README.md「开发默认规则」] [opinion][verified: .source-data/game-wiki.json content「全流程」「结局」；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14] | 需统一失败去向与检查时点；当前原型仍等三天结束，不当作新需求已实现。[opinion] |

## 2. 数值、信息与叙事

| 编号 | 问题 | 来源与差异 | 对开发的影响 |
| --- | --- | --- | --- |
| Q08 | 信任与特殊对白阈值 | 程序中的0–100、50为示例；角色/天数可以不同 | 需逐角色、逐日配置；不固化示例数字 |
| Q09 | 信任如何决定释放 | HTML用信任≥可调测试阈值亮绿灯，默认3，只有处置阶段且绿灯允许释放；原型门槛30，正式值未定。[verified: sources/inspection-terminal-day1-prototype.md「测试数值与实现边界」；../src/README.md「开发默认规则」] | 确认正式阈值、灯色/按键表现与代价，不直接采用测试值。[opinion] |
| Q10 | 疯狂、偏离、狂热与反叛值 | 前三种仍来自程序需求、梳理与名词表；主Wiki608版新增反叛值max，画板写大于阈值。[verified: .source-data/game-wiki.json content「结局」；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14] | 需定义是否相关、取值范围、累积规则及max/阈值关系，不自行合并。[opinion] |
| Q11 | 档案、新闻与信息屏蔽的Mini范围 | 梳理提供多种档案与证据；程序未给这些模块完整规格 | 需要确认界面、资产量与哪些信息可查询 |
| Q12 | 对话表与现有轮次模型 | [小孩v1.1](sources/dialogue-child-v1-1.md)已给三天R1–R6对话与R7措施轮、实际ID及信任−3/0/+3；与程序需求4–5轮、原型4轮及±10不同。[verified: .source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第10–11、20、30–33行] | 已有来源表，尚未自动接入；确认轮次、跳转与数值兼容方式。[opinion] |
| Q13 | 日记是否为必需系统 | 方案15与白皮书有日记；主Wiki日流程未写 | 要确认是否纳入3天与文本生成方法 |
| Q14 | 梦境素材与记忆叙事如何统一 | [梦场景](sources/dream-scenes.md)已有Day0–3四种场景及图/音乐；主角专篇要求记忆回流且非超自然线索；主Wiki第四结局另写超自然场景。[verified: .source-data/dream-scenes.json content；.source-data/player-character.json content「梦境与情绪表现」；.source-data/game-wiki.json content「结局」] | 需完成交互、揭露顺序及分支边界；不能再写全部梦境未提供。[opinion] |
| Q15 | 随机性实现范围 | 早期方案强调叙事约束随机；正式事件表未填 | 不能将随机病人、事件、物件全部计为已承诺功能 |
| Q16 | 监督员是否说话 | 主Wiki每日监督员对话；白皮书不发声且禁止搭话 | 影响对白、声音和交互形式 |

## 3. 世界观、角色与结局

| 编号 | 问题 | 来源与差异 | 对开发的影响 |
| --- | --- | --- | --- |
| Q17 | 名称统一 | 健康部、和平部、和平保障总署；战争消除仪、战争倾向检定仪等 | 名词表已读，使用和平部/和平仪/评估员等较早口径，仍不统一改写来源 |
| Q18 | 主角名称与记忆揭露 | 新[主角专篇](sources/player-character.md)已把举报妹妹、选择性记忆断层和体制奋斗串为同一背景，姓名仍未定；妹妹当前命运及结局未定。[verified: .source-data/player-character.json content「背景」「未知与待定」「设计约束与待定项」] | 更新共同背景；Steve称呼、机构命名和Mini内揭露程度仍需统一。[opinion] |
| Q19 | 净化后的命运 | 梳理表层为解除收容回归社会；白皮书为无忧劳工/地下工厂 | 需区分官方说法与真实去向，仍有口径需要确认 |
| Q20 | 筛查频率与社会设定 | 梳理年度检定；白皮书每日广场筛查 | 可并存但目前未明确制度层级；不要自行解释成已定事实 |
| Q21 | 神父背景 | [角色文档](sources/patient-priest.md)狂热杀人、求净化；梳理神父反对制度、被消音 | 两者叙事作用与对白显著不同，需指定最终版 |
| Q22 | 神父出现与死亡时间 | Day2初见、两次治疗后再治疗、下一日死亡 | 需和3天Mini及提前结局时间统一 |
| Q23 | 普通角色第三次治疗 | 小孩、执政者、医生文档写死亡；表现或原因待定 | 保留明确终止结果，具体演出不自行补写 |
| Q24 | 外交官实际处置 | [外交官](sources/patient-diplomat.md)第三次治疗死亡/撤离与净化拦截未定 | 需确认保护特例、表层反馈、报告对玩家结局的作用 |
| Q25 | 结局条件与优先级 | 主Wiki已补正常Day4被带走、释放神父触发袭击、任意一天KPI失败跳Day4；画板另列每日开始前条件检查。[verified: .source-data/game-wiki.json content「结局」；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14] | 条件已有补充，KPI实际结局、同时命中顺序和演出时点仍需统一。[opinion] |
| Q26 | 3种交付与第四条结局 | 原占位r已改为反叛值触发自我净化；交付标准仍写3种。[verified: .source-data/game-wiki.json content「Mini Game交付标准」「结局」] | 需确认新增、替代或扩展身份；先同步来源，不自动增加原型结局。[opinion] |
| Q27 | 病例/原型与当前五人名单 | 梳理A–F、白皮书七类原型、主Wiki五位病人 | 原型与样例不新增到当前出场名单 |

## 4. 工程与制作缺口

### 场景灯光、开场与梦境待办（2026-10-15聊天补充）

来源：[用户提供的聊天截图](assets/room-lighting-and-dream-chat-2026-10-15.png)。以下记录已讨论的表现方向与待制作事项，不代表现有Unity原型已经完成。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 灰色回复及下方梦境讨论] [opinion]

- [ ] **补齐开场介绍**：开场介绍已被提问，但截图未给出具体内容与播放安排，继续作为待定项。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 顶部「游戏开始的开场介绍」提问] [opinion]
- [ ] **确认场景物件交互信息**：逐物件确认可交互内容、提示及反馈；截图明确表示此项还未确认。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png「场景物体交互信息这个还没确认」] [opinion]
- [ ] **制作并切换下列五种房间灯光状态**：聊天原话称“四种”，实际列了1–5；此处保留全部五项，不擅自合并。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 灰色回复的编号1–5] [opinion]

| 房间状态 | 截图中的表现与用途 |
| --- | --- |
| 日常 | 日常情况下的房间。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 第1项] |
| 工作结束后的夜晚 | 工作结束后进入晚上，窗户外侧光源熄灭。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 第2项] |
| 全黑 | 所有地方都压黑，用于播放梦场景。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 第3项] |
| 仅床被聚光灯照亮 | 房间几乎不可见，仅高强度舞台聚光灯照床，可从窗外射入；用于梦场景及游戏开始的Day0场景。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 第4项] |
| 仅电脑屏幕亮起 | 其他地方全部压黑，仅电脑屏幕亮起，用于特殊结局。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png 第5项] |

- [ ] **制作闭眼到2D梦境的演出**：先将主场景压黑，模拟闭眼，再播放2D梦境动画；不将聊天补充解读为新增可探索3D梦境关卡。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png「闭上眼睛然后2D做梦」「然后就做梦动画」] [opinion]
- [ ] **落实房间内的梦境投影与输入锁定**：按聊天描述，将电脑另一侧的墙面用作屏幕，锁住玩家视角和操作，呈现如同播放2D动画的效果。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png「把电脑另一侧的墙变成屏幕」「视角和操作都锁住」] [opinion]
- [ ] **完成梦醒后的房间揭示**：动画结束后亮灯，让玩家发现自己一直在同一个房间；恢复阶段允许的视角与操作，并按当前流程开放交互。[verified: assets/room-lighting-and-dream-chat-2026-10-15.png「播完灯亮让玩家发现自己其实还是在这个房间内」] [opinion]
- [ ] **核对普通梦境、Day0与特殊结局的触发和衔接**：把上述灯光状态与既有梦后床交互、反叛值结局电脑引导分别对应，补齐转换时机及验收路线；截图未决定完整状态转换表。[verified: .source-data/game-wiki.json content「结局」；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:12/o43:14；assets/room-lighting-and-dream-chat-2026-10-15.png 第3–5项] [opinion]

### 其他工程与制作缺口

- 原文没有确定摄像机与输入的完整方案；当前开发原型已实现电脑镜头进出、输入切换和反应后回房，正式演出仍需制作与验收。[verified: ../UnityProject/README.md「运行与操作」；gameplay-migration-verification.json unity.playMode.tests]
- 原文没有确定技术选型；当前开发基线为Unity2022.3.23f1c1、Built-in、Yarn3.2.8及JSON存档。本机正常UPM导入已于2026-10-15修复；新机器环境、AI制作管线与最终发布配置仍需验证或裁决。[verified: gameplay-migration-verification.json unity.version/rendering/normalUpmRecovery、yarnCompiler.version；../src/README.md「保存与恢复」] [opinion]
- [梦场景](sources/dream-scenes.md)已提供Day0–3的12张图片与四条音乐参考，主Wiki已有病房及主角场景概念图、六条音效链接；来源已提供不等于成品资产已制作或接入。[verified: .source-data/dream-scenes.json content；.source-data/game-wiki.json content「美术」] [opinion]
- 正式资产清单尚缺：监督员、教学病人、角色差分、三天梦境、日记/任务单、结局演出与实际UI数量。
- 音乐与外部媒体是参考链接；能否用于成品及最终素材替换仍需确认。
- 发布平台、目标设备、性能预算与工期未明确承诺。早期Steam发行与DLC方向留作长线规划。

## 5. 当前资料与原文缺口

2026-10-15复查：已读取13篇正文、3份工作簿的12个子表及6块画板169个节点；排期按用户要求排除；已知引用链和两份本地附件已同步。小孩Excel原件已取得，HTML已做静态对应检查；空间目录及另两份工作簿原件导出仍有限制，不等同于整个空间盘点。详细证据见[同步记录](feishu-sync-2026-10-15.md)。[verified: .source-data/sync-2026-10-15.json body_sources_read/sheets_read/whiteboard_nodes/scope_exclusions/local_deliveries/remaining_limitations]

下列2026-10-12的数量与访问记录保留历史身份，本次新增和变化以上述同步记录为准。[verified: .source-data/history/2026-10-12/validation.json checked_at]

- 11篇正文和两份工作簿均已读取并独立整理；脑暴收集池仅整理方案15。
- 六块画板已读取节点、文字、连线与图片节点，并保存和检查完整画板图像。节点共167个，没有发现新的飞书文档链接。
- 神父工作簿六个子表全部读取：基础信息已填；状态差分和表演要求保留空白；人物关系表只有表头；另有空白子表和空白Sheet1。这些是原文未填，不是访问失败。
- 世界观名词表共有17条已填术语。候选词未全部采纳，原表F列表头为空但有一个候选值；来源文档保留这一细节。
- 流程画板补充了教学系统加载、特殊NPC、净化、关机与聚光灯引导上床，以及梦后仅能与床交互的规则。
- 灵感墙画板另含正常期第1–7天、畸变期第8–18天、崩坏期第19天+，以及六类结局；这些属于早期设想，未覆盖Mini Game的3天、3结局标准。
- 主Wiki知识空间全目录此前返回131006资源访问不足；已知单篇文档与直接引用都可读取。本次已覆盖三个入口的本游戏相关引用链，没有宣称盘点整个企业知识库。
- 12张正文参考图片、6张画板图像均已保存并检查。画板图像中的参考作品保持素材身份，不宣称为本项目资产或实机。
- 无权限、原文未填和范围外是不同状态；不把任何一种缺口自行补成已定设计。

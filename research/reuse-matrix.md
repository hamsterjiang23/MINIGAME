# 游戏需求与源码借鉴矩阵

> 研究日期：2026-10-12。Unity2022.3 LTS为研究基线；URP14为主要视觉候选。下列“已有”表示参考仓库的静态实现，不表示本游戏已实现。逐库文档包含完整SHA、本地源码和上游行号证据。

游戏范围依据[主Wiki](../docs/sources/game-wiki.md)、[程序需求](../docs/sources/program-requirements-v0-1.md)与[游戏总览](../docs/game-overview.md)。研究分类：直接引入候选、改写结构、表现参考、自研缺口。优先级P0表示先解决核心闭环，P1表示在闭环上补内容，P2表示表现或辅助功能；不是制作工期。

## 1. 公寓、终端与每日流程

| 编号／优先级 | 我们的需求与出处 | 已核查来源与代码入口 | 怎样借鉴 | 自研部分与验证场景 |
| --- | --- | --- | --- | --- |
| R01／P0 | 第一人称、无肢体、无跳跃，见程序需求 | [The Guest](repositories/the-guest.md)的PlayerController；[Moral Matrix](repositories/moral-matrix.md)的ClickMovement | 改写结构：前者参考CharacterController与交互责任分离；后者仅参考交互暂停 | 两者均未提供匹配的第一人称完整控制。需做视角旋转、碰撞、无跳跃和无身体显示；进入UI时不继续移动 |
| R02／P0 | E调查物件、门、电脑和床，见程序需求 | The Guest的IInteractable、PlayerController触发器与E键入口 | 改写结构：统一交互接口及提示显示 | 不沿用服装、开门信任或原游戏角色类型；重叠对象有确定目标，退出范围／对象销毁及时清提示 |
| R03／P0 | 公寓进入独立2D电脑界面并顺滑退出，见程序需求 | Moral Matrix的DoorClickDetector摄像机切换；The Guest的输入冻结 | 改写结构＋自研：划分过渡控制、世界交互和终端UI | 连续靠近电脑、开机遮挡、鼠标解锁、退出还原均需开发；过渡期间E和处置键不能重复触发 |
| R04／P0 | 敲门、晨检、任务、工作、睡眠、梦境、次日，见主Wiki | [Do-You-Trust-Them](repositories/do-you-trust-them.md)的DayContentSO、DaysManager、ProgressController | 改写结构：每日静态内容与运行进度分离，活动完成才开放下一环节 | 原仓库只提供部分脚本。需自己的流程状态及合法切换；晨检结束前工作不可开始 |
| R05／P0 | 工作结束可回房，睡眠推动次日；梦后仅床可交互，见流程画板 | Do-You-Trust-Them的活动完成与切日动画；Moral Matrix的CutsceneManager | 改写结构：活动门控和演出结束回调；表现参考：淡入淡出 | 自行区分自由移动、允许交互对象与允许切日。梦后门和电脑不可交互，床只结算一次 |
| R06／P1 | 各阶段切换音乐、敲门和扫描声，见主Wiki／程序需求 | [Human, Please](repositories/human-please.md)的work_day；Moral Matrix的AudioManager | 改写结构／表现参考：阶段驱动音轨和环境声 | 不复制原游戏断电事件或素材；工作、梦境、结局切换无残留叠音，音量设置生效 |

## 2. 病人会面与处置

| 编号／优先级 | 我们的需求与出处 | 已核查来源与代码入口 | 怎样借鉴 | 自研部分与验证场景 |
| --- | --- | --- | --- | --- |
| R07／P0 | 角色／日期对应会谈，普通4–5轮、每轮1–4句话，见程序需求 | [Yarn](repositories/yarn-spinner-unity.md)的DialogueRunner与Presenter；[Ink](repositories/ink-unity-integration.md)的Story／Continue | 直接引入候选：Yarn为主，Ink备选；按病人、日期和轮次组织剧情 | 数量是内容规则而非框架自动保证。检查台词和节点引用完整，逐句推进不一次倾倒全部文本 |
| R08／P0 | 玩家最多4个回复，回答分支再回公共回复，见程序需求 | Yarn的OptionsPresenter与jump示例；Ink的choice与gather示例 | 直接引入候选：使用条件选项和分支回流 | 我们限制四选项，校验没有不可达公共节点；不同回应写入正确的病人状态 |
| R09／P0 | 每位病人的独立隐藏信任，见程序需求 | [The Guest](repositories/the-guest.md)的NPCProfile／NPCMemory | 改写结构：静态人物资料与运行状态分离 | 原实现按costume划分信任且依赖LLM。改为稳定病人ID，跨日保存；不同病人不串值，UI不泄露隐藏量 |
| R10／P1 | 信任满足角色／日期阈值，普通结束后进入独有特殊轮，见程序需求 | Yarn命令／函数注册与条件选项；Ink外部函数／变量 | 直接引入候选＋自研：业务层提供资格判断，对话层负责呈现 | 阈值50只是示例，不能固化。每场特殊轮最多一次，不在每个普通节点结束时提前触发 |
| R11／P0 | 所有对话结束后才允许治疗／净化／释放，见程序需求 | Yarn的onDialogueComplete；Human, Please的camera_area控件门控 | 改写结构：会谈阶段驱动处置控件与业务资格 | 普通结束不等于特殊轮也结束。会谈未结束、转场中或已经提交时，三键均不能成功 |
| R12／P0 | 灯色决定释放资格，隐藏绿键、黄色治疗、红色净化，见程序需求 | Human, Please的操作控件；[Papers-Please](repositories/papers-please-unity.md)的DeskButton→GameManager.ButtonCall(bool isRed) | 表现参考：按钮显示、禁用和清理。资格由我们独立判定 | 原库都是二元处置，需三动作模型。释放门槛、灯色与信任关系待定；红灯点击不能改变状态 |
| R13／P0 | 处置后反应对白、花屏、结束当前病人，见程序需求 | Human, Please的病人交接遮挡；Yarn独立反应节点；[Retro CRT](repositories/urp-retro-crt-shader.md)材质 | 改写结构＋直接引入候选：提交后果→反应→故障演出→清空终端 | 反应期不允许二次处置；演出结束不再次扣配额。反应缺失应作为内容错误暴露 |

## 3. 跨日后果、结局与存档

| 编号／优先级 | 我们的需求与出处 | 已核查来源与代码入口 | 怎样借鉴 | 自研部分与验证场景 |
| --- | --- | --- | --- | --- |
| R14／P0 | 每次净化使当日剩余配额减一，见程序需求 | [DeathAndTaxes](repositories/death-and-taxes.md)的DesktopManager任务／处置后果；[Gatekeeper](repositories/gatekeeper-insight.md)的game_stats | 改写结构：将处置结果与配额显示分开，业务状态为唯一来源 | 原游戏统计不等于我们的净化配额。需一次性提交、零边界与日终判断；治疗／释放不误扣净化配额 |
| R15／P0 | 治疗后返场，释放／净化后退出，见五份角色文档 | DeathAndTaxes的PlayerState按角色ID记录处置；Do-You-Trust-Them每日配置 | 改写结构：静态出场计划先过滤运行状态，再生成当日队列 | 出场日与退出条件待统一Q03。退出角色不机械召回；同一病人不重复进队 |
| R16／P1 | 小孩、执政者、医生第三次治疗终止，神父／外交官特例，见角色文档 | The Guest的配置／记忆拆分；DeathAndTaxes跨日后果结构 | 改写结构＋自研：每角色治疗阶段和终止规则 | 无整套匹配实现；阶段、差分与对白要一起配置。外交官保护等未定，不默认按普通角色处理 |
| R17／P1 | 对玩家的隐藏审查影响后续，外交官仅知道真实工作记录，见游戏梳理／外交官 | DeathAndTaxes任务偏差统计；Yarn变量与函数桥接 | 改写结构：从实际会面和处置历史提供可用事实 | 疯狂值／偏离指数／狂热值语义未统一，不合并。尚未发生的动作不得出现在外交官台词中 |
| R18／P1 | 释放神父后次日袭击；正常带走／KPI失败锁房，见主Wiki／神父 | Do-You-Trust-Them结局分发；DeathAndTaxes跨日叙事状态 | 改写结构：条件选择与结局演出分离 | 原库没有我们三个结局判定。触发时点、优先级及正常结局条件见Q25；占位r不计第四结局 |
| R19／P0 | 保存日期、病人状态、会谈位置和一次性处置 | Yarn变量JSON；Ink StoryState JSON；DeathAndTaxes SaveManager | 借鉴状态边界与保存时机；建立自己的业务快照 | Yarn只存变量，Ink也不含我们的Unity业务状态。需业务状态与对话检查点一致，重载不重扣配额、不重播一次性事件 |
| R20／P2 | 快速检查病人、日期与结局路径 | [NINAHSaveEditor](repositories/ninah-save-editor.md)的Types／Editor字段编辑 | 改写结构：自建开发期状态查看和受控跳转工具 | 不使用原作类型名、密钥、存档副本或Windows Forms工具作为游戏模块；修改后重新校验状态约束 |

## 4. 视觉、输入与内容组织

| 编号／优先级 | 我们的需求与出处 | 已核查来源与代码入口 | 怎样借鉴 | 自研部分与验证场景 |
| --- | --- | --- | --- | --- |
| R21／P1 | 3D场景中的2D人物／物件，见程序需求 | [Moral Matrix](repositories/moral-matrix.md)的Billboard／DirectionalSpriteUtility | 代码复用候选或改写结构：摄像机朝向与方向差分 | 监控中的病人用UI图像即可；不是所有2D资产都需要billboard。检查遮挡、透明材质及近距离视角 |
| R22／P1 | 电脑CRT、扫描线、反应花屏，见程序需求／主Wiki | Retro CRT的FullScreenPass示例；[CrowFX](repositories/crowfx-unity-image-effects.md)的RenderStack和预设 | 直接引入候选：URP14优先Retro；Built-in可评估CrowFX | 效果先限定电脑；不同时叠加两整套CRT。中文可读、实体屏幕路线正确、打包后Shader未被剥离 |
| R23／P0 | 终端、弹窗、暂停、梦境时避免E误触世界 | [无障碍MOD](repositories/im-a-blind-human.md)的InputModeGate／InputContext；The Guest输入隔离 | 改写结构：统一输入上下文和覆盖层占用 | 自研且不搬原游戏反射类型。嵌套弹窗关闭后正确恢复，过渡被打断也能清理输入占用 |
| R24／P1 | 仪器检测情绪，不等于善恶；台词来自真实经历，见游戏梳理 | [NINAH-Docs](repositories/ninah-docs.md)的机制条件／表征成因表达；Gatekeeper检查信息分层 | 表现／内容组织参考：建立“读数、成因、已知证据、人物陈述”表 | 不抄攻略正文和原作素材；不沿用SAFE／DANGEROUS标准答案。核对同一异常读数可由不同情绪成因产生 |
| R25／P1 | 教学病人／制度通知／任务单，见主Wiki和程序需求 | [DeadLetterOffice](repositories/dead-letter-office-day1.md)的特殊内容插入、票据反馈与日终Timeline；Papers资料清理 | 改写结构／表现参考：特殊会面插入和行政反馈 | Day0是否额外计入预算见Q02；档案／拖放规模见Q11。无确认需求时不新增邮件生成器、纸面校验或大量资产 |

## 5. 借鉴组合怎样工作

实际需要连接的是以下链路，而不是把多个游戏工程合并：

**当天配置 → 当前可出场病人 → 普通与特殊会谈 → 处置资格 → 一次性提交 → 反应演出 → 日终与跨日事件 → 完整状态保存。**

Yarn／Ink只承担会谈执行和文本选项；人物状态、配额、角色去留、事件与结局由我们的业务层统一决定。电脑UI读取资格与显示信息，不能直接改写病人、日期或配额。世界交互与终端共用输入门控。

| 借鉴分类 | 本轮可采用的来源 | 执行边界 |
| --- | --- | --- |
| 直接引入候选 | Yarn或Ink之一；Retro CRT或所选管线的少数效果；明确许可的小型脚本 | 固定版本、保留对应许可，完成2022.3导入／编译／打包验证后才记录为已采用 |
| 改写结构 | DeathAndTaxes、Do-You-Trust-Them、Gatekeeper、Human, Please、The Guest、MOD／存档工具 | 使用自己的角色ID、三键处置与日流程；不复制无明确许可的本体代码 |
| 表现参考 | 病人交接遮挡、灯光／音乐变化、行政反馈、机制表格表达 | 用本游戏文本、素材和事件重做，不照搬原作身份／角色／剧情 |
| 自研缺口 | 第一人称到终端完整过渡、五人治疗／信任、释放门槛、事件优先级、完整业务存档 | 所有尚未确定的策划规则保持待定，研究建议和正式内容配置分开 |

## 6. 不带入当前范围的机制

- Gatekeeper和Papers的随机访客、客观真伪／危险标准答案；我们的对象是五位固定病人，仪器读情绪。
- DeadLetterOffice的邮票与收发件人字段校验；没有确认的纸质档案需求时，拖放只保留参考。
- The Guest的服装伪装、自由提问、LLM信任裁决和API依赖。
- 旧研究的食品、精力、安保、生存资源、八人生成器、Monte Carlo生成平衡任务。
- NINAH原作十四天和额外结局、灵感墙十九天以上结构；不扩大三天／三结局。

当前完整性依据是[三入口设计来源](../docs/source-index.md)与[15仓库版本记录](../docs/source-lock.md)，不宣称获得原作全部内部代码或企业知识库全目录。

## 7. 下一步如何验收

[路线图](unity-2022-roadmap.md)将R01–R25分配到验证阶段；[风险与缺口](risks-and-gaps.md)记录依赖和未定规则。逐库文档给出可复核的源码入口。验证时分别记录“本地源码确有实现”“包声明支持2022.3”“我们的场景运行成功”，这三个结论不能相互替代。

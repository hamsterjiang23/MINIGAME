# 当前完成情况与待补项

状态更新日期：**2026-10-15，Asia/Shanghai**。最新为小孩Day1终端、Yarn内容包、离线策划编辑器与Unity导入；默认演示采用该包，旧三天原型保留。见[使用指南](terminal-ui-guide.md)和[本轮报告](terminal-ui-verification.json)，历史结果仍在[原汇总](gameplay-migration-verification.json)。[verified: terminal-ui-verification.json scope；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L126]

**当前交付是可以运行流程的开发原型。** 已有“三天、五病人、三结局”的规则、存档和 Unity/Yarn 宿主，也已生成 Windows Mono 开发程序；正式叙事、美术、音频和发布验收尚需补齐。[verified: ../src/README.md「开发默认规则」；../UnityProject/README.md「运行与操作」；gameplay-migration-verification.json unity.windowsMonoBuild] [opinion]

《寻找伪人》是重要玩法参考。现已借鉴其“房间交互 → 观察与会谈 → 条件选项 → 决策 → 反应 → 回房 → 睡眠与次日后果”的结构，迁移依据和具体代码对应见[玩法迁移说明](reference-gameplay-migration.md)。[verified: reference-gameplay-migration.md「已实现的对应关系」]

## 最新来源同步与实现边界

2026-10-15已同步主Wiki608版、主角6版、梦场景191版和小孩v1.1对白31版；随后已接入小孩Day1六轮、20回复及三种处置。其余角色和小孩Day2/3仍待接入，四天梦场景保持来源身份。[verified: .source-data/manifest.json documents/workbooks；terminal-ui-verification.json source/scope] [opinion]

新来源新增反叛值自我净化分支与每日开始前结局/KPI检查；交付标准3结局与正文4条的冲突继续裁决。小孩Day1六轮和−3/0/+3已采用独立包接入，不据此改变旧三天的KPI、结局和正式全局数值。[verified: .source-data/game-wiki.json content「结局」；terminal-ui-verification.json scope/source] [opinion]

小孩Excel原件与[Day1终端HTML](sources/inspection-terminal-day1-prototype.md)已归档，Day1数据一致；Unity原生Yarn与终端UI已通过测试，HTML浏览器视觉与下载点击未验收。[verified: .source-data/inspection-terminal-day1-v0.5.2.json data_matches_xlsx_day1；terminal-ui-verification.json source/planner.browserVisualVerified/unity]

排期已按用户要求排除。同步范围、完整下载入口和获取限制见[飞书同步记录](feishu-sync-2026-10-15.md)。[verified: .source-data/sync-2026-10-15.json access_gaps]

## 已经完成什么

### 默认小孩Day1终端与策划工具

- 目录、审查、发言、回复、实际记录、核验灯、三种处置、反应花屏、断信号与归档返回已接通；恢复与连点由核心和宿主门控，当前段可重播。[verified: terminal-ui-verification.json scope/unity/limits]
- [离线HTML工具](../Tools/DialoguePlanner/DialoguePlanner.html)提供节点/表单/源码编辑、分支排练、保存打开与导出；Unity校验并原生编译后激活，失败保留旧内容。[verified: ../Tools/DialoguePlanner/README.md「策划使用」；terminal-ui-verification.json scope]

### 规则与存档

- `GameSession` 统一执行日期、阶段、病人状态、会谈资格、信任、治疗次数、去留、净化配额、跨日事件和结局判定。宿主通过命令和玩家视图接入，界面不保存另一套权威数值。[verified: ../src/README.md「调用入口」「开发默认规则」]
- 普通会谈、回复分支、特殊会谈资格、处置资格、处置后反应已接通；重复处置、过期会面/节点回调和阶段不合法的操作会被拒绝。[verified: ../src/README.md「调用入口」；../src/MiniGame.Presentation/GameRuntime.cs SelectResponse/SubmitDisposition/CompleteCue]
- JSON 存档保存核心状态、命令历史、会谈与演出检查点、暂停状态和观察记录。恢复时校验规则与内容版本、重放核心历史、刷新回调 token；文件保存使用同目录临时文件与原子替换。[verified: ../src/README.md「保存与恢复」；../src/MiniGame.Presentation/GameRuntime.cs Save/Restore/ValidateCheckpoint；../src/MiniGame.Presentation/RuntimeFileSaveStore.cs L19–L43]

### Unity 与玩家操作

- 原创公寓场景、门/电脑/床、第一人称移动与观察、交互提示、中文 uGUI、病人记录、配额显示和暂停菜单已实现。[verified: ../UnityProject/README.md「运行与操作」；../UnityProject/Assets/MiniGame/Scenes/InspectionApartment.unity]
- 电脑进入/退出镜头、反应花屏、夜间床引导、睡眠与梦后推进已接通；移动、对话、观察、演出和暂停使用互斥输入上下文。[verified: ../UnityProject/README.md「运行与操作」；reference-gameplay-migration.md「房间与会谈互斥」「睡眠、梦境与跨日后果」]
- 眼睛、手部、牙齿、照片、当日通知和档案六类观察已有入口、展示、当日记录去重和存档。当前内容是原创示意与占位线索，尚不能代表正式线索系统完成。[verified: reference-gameplay-migration.md「观察再决策」；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs L174–L179、L324–L368]
- Yarn Spinner 3.2.8 已通过桥接呈现文本、继续按钮和回复选项。当前生成内容为322个核心节点、327个Yarn节点、536条文本；这些数量表示开发内容与映射覆盖，不是正式剧本完成量。[verified: gameplay-migration-verification.json yarnCompiler；../src/MiniGame.Presentation/PrototypeContent.cs L11–L16]
- 已处理读档后旧对话回调、残留黑屏/花屏、终端镜头恢复、最后一帧暂停卡住演出，以及暂停菜单被过渡遮挡的问题。[verified: gameplay-migration-verification.json unity.playMode.tests；../UnityProject/implementation-notes.md「Discovered edge cases」]

### 可以取得的产物

- 本机程序：`Builds/MiniGame/MiniGame.exe`；场景：`UnityProject/Assets/MiniGame/Scenes/InspectionApartment.unity`；操作与重建方法见[工程说明](../UnityProject/README.md)。[verified: gameplay-migration-verification.json buildPath；../UnityProject/README.md「可重放的本机验证路线」]
- 源码、场景、生成内容、依赖配置和验证汇总在仓库中；`Builds/`、`Artifacts/`、本机 SDK 和 Editor 缓存由 Git 忽略。新克隆得到源码，开发程序和本机日志需要按工程说明重建。[verified: ../.gitignore「Builds/」「Artifacts/」「.dotnet/」「UnityProject/Library/」；../UnityProject/Tools/Invoke-OfflineValidation.ps1 Action分支] [opinion]

## 已验证到什么程度

最新终端结果：SDK8.0.424锁定恢复、Release构建0警告/错误，Core75＋Presentation110＝185项通过；Node工具18项及1296条选择组合通过。真实Unity2022.3.23f1c1隔离工程EditMode2项、PlayMode9项通过；Windows Mono构建成功，0错误/1警告。四种比例、截图、Player启动与源码哈希比较范围见[本轮报告](terminal-ui-verification.json)。浏览器视觉与下载点击不在已通过范围。[verified: terminal-ui-verification.json dotnet/planner/unity/limits] [opinion]

以下是 **2026-10-13 保存的验证结果**，不是本次文档整理重新执行的测试。[verified: gameplay-migration-verification.json checkedAtUtc]

- .NET SDK 8.0.424：锁定恢复和 Release 构建通过，0警告、0错误；Core 73项与 Presentation 77项测试全部通过。[verified: gameplay-migration-verification.json sdk/dotnet]
- 固定 Yarn 编译器：327个生成节点、536条文本编译通过，0错误、0诊断。[verified: gameplay-migration-verification.json yarnCompiler]
- 真实 Unity 2022.3.23f1c1：隔离离线工程场景生成通过，图形模式 PlayMode 四项通过。验证副本与145份源码哈希一致，0差异。[verified: gameplay-migration-verification.json unity.version/sceneGeneration/playMode/sourceComparison]
- Windows Mono 开发构建成功，0错误、1警告；警告来自 `-nographics` 下无法更新环境与反射探针。Player 无图形启动10秒进入晨检阶段，无启动异常。[verified: gameplay-migration-verification.json unity.windowsMonoBuild.warningDetail/unity.playerStartup]
- 三种结局有规则与宿主测试；图形 PlayMode 完整流程走的是“被带走”结局，不能将其写成三种结局演出全部通过。[verified: ../src/MiniGame.Presentation.Tests/RuntimeEndingTests.cs PriestAttack/LockedInRoom/TakenAway；../UnityProject/Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs ActualYarnButtonsCompleteThreeDaysAndReturnToApartmentBetweenPatients]
- **2026-10-15 正常 UPM 工程导入及四项图形 PlayMode 测试通过**；启动器补齐缺失环境并打开主场景。IL2CPP、已打包 Player 的图形交互全流程、性能和最终发布版本仍需独立验收。[verified: gameplay-migration-verification.json unity.normalUpmRecovery/il2cpp/limits] [opinion]

## 必须填补的工作

### 正式玩法与数值裁决

需要确认逐日病人日程与退出条件、每日KPI数值与失败去向/检查时点、信任/特殊会谈/释放门槛、交付3结局与新增第四条分支的范围及冲突优先级，以及神父和外交官的特殊处置。Day0教学和Day4演出的制作范围也需要确定。[verified: open-questions.md Q02–Q09、Q22、Q24–Q25]

目前可执行配置为：三天、每日净化目标1、普通会谈4轮、特殊轮门槛20、释放门槛30；神父Day2加入，其他四人Day1加入；默认结局优先级为袭击→锁房→被带走。这些继续保留开发配置身份，不能当作正式策划已确认。[verified: ../src/MiniGame.Core/Rules/PrototypeRules.cs L17–L52；../src/README.md「开发默认规则」]

交付建议：形成带版本的正式日程、数值与事件配置；每项变更写清玩家可见后果，并补相应跨日、处置和结局验收。验收应覆盖“退出的人不会再进常规队列”“读档不重复结算”“结局冲突结果明确”。[opinion]

### 正式叙事与观察内容

小孩Day1来源对白已接入；还需接入Day2/3与跨日后果，补齐其他病人、晨检简报、梦境及经裁决的结局。观察画面与证据仍需按角色、日期和状态制作。[verified: terminal-ui-verification.json scope/limits；open-questions.md Q11–Q14、Q25] [opinion]

主角新专篇已明确举报妹妹与记忆断层背景；姓名/机构名称、Mini内揭露程度、神父背景和净化真实去向仍有来源冲突；监督员交互、日记、档案/新闻范围与疯狂值等变量是否采用，仍需裁决。占位文本不承担这些裁决。[verified: open-questions.md Q10–Q11、Q13、Q16–Q21；../src/MiniGame.Presentation/PrototypeContent.cs L11–L16]

交付建议：先完成一位病人的正式完整流程，覆盖普通与特殊会谈、六类观察、三种处置及次日后果；再按确认的日程扩展至五人三天。正式线索应支持有信息依据的判断，具体判定规则由策划定义。[opinion]

### 美术、音频与演出

新增[场景灯光、开场与梦境待办](open-questions.md#场景灯光开场与梦境待办2026-10-15聊天补充)：补齐开场与物件交互定义、五种房间灯光状态、闭眼后2D梦境、墙面投影与视角/操作锁定，以及梦醒亮灯揭示仍在同一房间。各项保持待制作/验收状态，截图已归档为来源。[verified: open-questions.md「场景灯光、开场与梦境待办（2026-10-15聊天补充）」] [opinion]

公寓和患者肖像目前是灰盒/示意。需要正式场景与物件、五位病人的表情和状态差分、检查近景、任务单与档案、UI视觉、梦境画面及结局演出。监督员、教学病人、日记等资产按确认的制作范围列入清单。[verified: ../UnityProject/README.md「运行与操作」；game-overview.md「表现与制作内容」；open-questions.md「工程与制作缺口」]

音乐、环境声、门/仪器/终端/按键反馈及状态切换需要制作与接入；原设计中的外部音乐和媒体链接保持参考身份，正式使用与素材来源需要确认。[verified: game-overview.md「表现与制作内容」；open-questions.md「工程与制作缺口」] [opinion]

交付建议：逐项记录资产用途、状态差分、格式、制作来源、使用许可和接入位置，完成正式的一位病人流程后检查画面、声音与选择后果能否同时理解。[opinion]

### 工程、测试与发布

正常 UPM 工程导入已于2026-10-15修复；仓库根 `Open-Unity.cmd` 可直接打开主场景。仍需验证新机器依赖获取与复现步骤；离线脚本依赖本机 Editor 与包缓存，不能视为跨机器部署已完成。渲染基线为 Built-in；URP 切换仍是后续选择。[verified: gameplay-migration-verification.json unity.normalUpmImport/rendering；../UnityProject/README.md「可重放的本机验证路线」「技术与来源边界」] [opinion]

需要补已打包 Player 的图形交互全流程，以及正式内容下的三结局、存档恢复、输入/画面/声音和性能验收。发布平台、目标设备、性能预算、工期与交付方式需确定；IL2CPP按目标平台决定并实际验证。[verified: gameplay-migration-verification.json unity.playerStartup.scope/il2cpp；open-questions.md「工程与制作缺口」] [opinion]

交付建议：在干净环境重建并运行正式切片，留存版本、依赖、测试与性能记录；按最终发布目标完成三结局回归及分发包检查，再认定达到发布交付。[opinion]

## 《寻找伪人》哪些还没有迁移

原作的随机访客池、真假身份、行动能量、物品库存、夜间谋杀、FEMA及原作结局，以及腋下/耳部检查尚未迁移；当前资源压力显示既有净化配额。这些不是已经承诺的原创项目必做缺项。[verified: reference-gameplay-migration.md「原创规则与未迁移机制」「观察再决策」]

如果要进一步采用这些机制，应先明确它们在检定员设定中的目的、可见信息、规则和制作成本，再修改核心配置与测试。继续保持 `GameSession` 为规则状态权威。[opinion]

## 参考研究的完成与缺口

参考游戏已保存恢复代码、调用与玩法分析、资源引用和编译对白索引；交付记录包含7个场景、38个Prefab、3255个Yarn节点、625份合并实现和53个插件。它们属于研究资料，不等于已接入原创游戏。[verified: ../research/no-im-not-a-human/Analysis/deliverable-verification.json exported_scenes/exported_prefabs/yarn_nodes/merged_csharp9_implementations/merged_plugins]

参考恢复工程仍有角色配置/灯光序列化、占位Shader和准确Editor验证缺口；其导入与完整语言特性验证未通过确认。继续研究应从这些缺口和未解析调用入手，独立记录成果，不据此升级原创工程版本。[verified: ../research/no-im-not-a-human/Analysis/deliverable-verification.json unity_import_verified/comprehensive_language_feature_validation；../README.md「No, I'm not a Human参考研究」] [opinion]

## 接下来按什么顺序推进

1. **确认规则**：收敛日程、KPI、信任/释放、三结局与特殊角色条件，形成正式配置基线。[opinion]
2. **制作正式完整流程**：替换一位病人的对白、观察、差分、声音与处置后果，并验证新机器的UPM依赖获取。[opinion]
3. **补齐三天五病人**：按日程加入其他角色、跨日变化、梦境和结局演出，完成对应内容回归。[opinion]
4. **完成发布验收**：在目标设备验证图形Player、三结局、存档、性能与发布包；按需验证IL2CPP。[opinion]

旧设计资料保留来源口径；涉及当前完成度时以本文、[源码说明](../src/README.md)、[工程说明](../UnityProject/README.md)和带日期的验证记录交叉检查。代码或内容变化后，应更新本文件并重做受影响的验证。[opinion]

# 游戏核心逻辑源码

这里实现五位病人、三天流程和三种结局的**纯C#规则库**，并提供不依赖Unity的宿主生命周期库。新建的[Unity开发原型](../UnityProject/README.md)接入房间、观察、Yarn会谈、处置反应和睡眠跨日；正式对白、美术与结局演出仍为后续制作内容。[verified: MiniGame.Presentation/GameRuntime.cs；../UnityProject/Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs CreateAndValidate] [opinion]

[实现说明：文件职责、设计理由与借鉴来源](architecture-and-rationale.md)解释代码为什么这样组织；[研究路线](../research/unity-2022-roadmap.md)和[游戏设计](../docs/game-overview.md)提供需求背景。

2026-10-15飞书来源已更新：主Wiki新增反叛值结局，流程画板新增每日开始前结局/KPI检查，小孩已有完整对白表；当前原型仍保留开发配置。变动与待裁决项见[同步记录](../docs/feishu-sync-2026-10-15.md)。[verified: ../docs/.source-data/game-wiki.json content「结局」；../docs/.source-data/sync-2026-10-15.json changes] [opinion]

默认Unity终端已接入小孩Day1来源对白及独立演示配置；`MiniGame.Presentation/Authoring` 将受限Yarn内容包编译为核心规则、显示内容和运行时Yarn。台词源为包内 `yarnSource`，GameSession仍裁决信任与处置。策划入口见[终端指南](../docs/terminal-ui-guide.md)，旧三天 `PrototypeRules` 继续独立保留。[verified: MiniGame.Presentation/Authoring/DialoguePackageCompiler.cs L20-L93；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L126]

## 目录与技术基线

| 位置 | 内容 |
| --- | --- |
| [MiniGame.Core](MiniGame.Core) | `.NET Standard 2.1`、C#9的逻辑库，运行依赖Newtonsoft.Json13.0.3 |
| [MiniGame.Core.Tests](MiniGame.Core.Tests) | `net8.0`测试，NUnit3.14.0、Adapter4.6.0、Test SDK17.11.1 |
| [MiniGame.Presentation](MiniGame.Presentation) | `.NET Standard 2.1`宿主生命周期、观察记录、原创占位内容、Yarn节点生成与检查点 |
| [MiniGame.Presentation.Tests](MiniGame.Presentation.Tests) | 输入门控、回调失效、三天流程、内容与存档验证 |
| [MiniGame.sln](MiniGame.sln) | 逻辑库与测试工程的解决方案 |
| [global.json](global.json) | 固定SDK8.0.424，不自动切换到其他SDK |
| [Directory.Build.props](Directory.Build.props) | 统一C#9、可空检查、确定性构建和警告视为错误 |

各工程的`packages.lock.json`锁定直接和间接依赖。SDK、NuGet缓存、`bin`、`obj`、测试产物均不作为源码提交。

## 在macOS运行

安装.NET SDK **8.0.424 ARM64**，进入本目录运行：

```bash
dotnet restore MiniGame.sln --locked-mode
dotnet build MiniGame.sln --configuration Release --no-restore
dotnet test MiniGame.sln --configuration Release --no-build --no-restore
```

本次SDK安装在`/Users/mt/.local/share/minigame/dotnet`。如果没有配置命令行路径，可在本目录直接运行：

```bash
/Users/mt/.local/share/minigame/dotnet/dotnet test MiniGame.sln --configuration Release
```

SDK压缩包已与Microsoft官方发布元数据的SHA512核对。该用户目录路径是这台Mac的环境信息，不是逻辑库的运行依赖。

## 调用入口

`GameSession`拥有运行状态。外部提交命令，读取`PlayerView`，消费结果事件，演出完成后再通知核心。不要通过存档DTO改写正在运行的游戏。

```csharp
using MiniGame.Core;

var rules = PrototypeRules.Create(); // 明确使用开发测试配置
var game = new GameSession(rules);

game.Execute(new CompleteMorningInspection());
game.Execute(new CompleteBriefing());
game.Execute(new Interact(InteractionTarget.Computer));

var view = game.GetPlayerView();
var selected = game.Execute(new SelectResponse(
    view.CurrentEncounterId!, view.CurrentNodeId!, "empathize"));

// 宿主根据ResponseBranchStarted播放该分支；播放完成才提交完成通知。
var branch = game.GetPlayerView();
game.Execute(new CompleteResponseBranch(
    branch.CurrentEncounterId!, branch.CurrentNodeId!));
```

每个命令返回`CommandResult`：`Accepted`、`Reason`、只读`Events`。拒绝的命令不产生事件，不修改状态，也不写入操作历史。实际应用应检查结果，不能假定调用一定成功。

| 命令 | 允许时机与效果 |
| --- | --- |
| `CompleteMorningInspection` | 晨检完成，进入任务简报 |
| `CompleteBriefing` | 简报完成，允许操作电脑 |
| `Interact(Computer)` | 公寓阶段开始当日工作；空队列直接进入夜间 |
| `SelectResponse` | 普通／特殊轮选择合法回答，记录信任变化并进入对应反馈分支 |
| `CompleteResponseBranch` | 匹配当前会面和分支后，汇合下一普通轮、特殊轮或处置阶段 |
| `SubmitDisposition` | 所有会谈结束后一次性提交治疗／净化／释放，进入反应阶段 |
| `CompleteReaction` | 反应完成后切下一病人或结束工作；不再次结算处置 |
| `Interact(Bed)` | 夜间开始睡眠；梦后推进次日并判定跨日事件与结局 |
| `CompleteSleep`／`CompleteDream` | 宿主通知相应演出结束，进入梦境／梦后阶段 |

会面ID和节点ID必须来自当前玩家视图。旧会面的回调、错误节点、连点处置、提前操作和结束后操作会被拒绝。命令需由一个宿主线程按顺序调用；本库没有并发会话控制。

`PlayerView`提供日期、阶段、当前人物／节点、选项ID、可用处置、允许交互和剩余净化配额，不公开隐藏信任。完整快照含信任与历史，供存档和内部调试使用。

## 开发默认规则

本节描述保留的旧三天 `PrototypeRules`。默认小孩Day1内容包另采用六轮、实际回复ID与−3/0/+3、初始信任0、释放阈值3、净化剩余5；它不把Day2/3正式对白或全局正式数值自动接入。核心允许非空普通轮列表，内容包导入上限为128轮；旧配置仍为4轮。[verified: ../UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json profile/yarnSource；MiniGame.Core/Rules/DialogueDefinitions.cs L51–L58；MiniGame.Presentation/Authoring/DialoguePackageCompiler.cs L58] [opinion]

这些是用户采纳的**首轮开发配置**，不是已确认的正式策划参数；集中在[PrototypeRules.cs](MiniGame.Core/Rules/PrototypeRules.cs)。

- Day1–3，神父Day2加入，另外四人Day1加入；退出或终止的人不再进入后续常规队列。
- 每日净化目标1；旧配置普通会谈4轮，每轮1–4个回复；核心不再限制为4–5轮。[verified: MiniGame.Core/Rules/DialogueDefinitions.cs L31、L51–L58]
- 初始信任0；`empathize`／`neutral`／`pressure`的测试变化为+10／0／−10；特殊轮门槛20，释放门槛30。信任没有0–100钳制，整数溢出会拒绝操作。
- 每场最多一轮特殊会谈，满足条件也要等待普通轮全部完成；反馈分支完成后才进入下一阶段。
- 小孩、执政者、医生第三次治疗终止并记录死亡；外交官第三次治疗退出，实际死亡、撤离或保护不做判断。
- 神父第三次治疗后的次日终止是可配置规则；从Day2开始的默认三天日程只会治疗两次，不强行压缩长期剧情。
- 神父释放次日袭击；三天结束后，存在未达标日则锁房，否则被带走。默认冲突优先级为袭击→锁房→被带走。
- Day4只用于结局收束，不生成第四天工作；Day0教学未实现。
- 保存真实回答、处置和每日指标，不合并疯狂值、偏离指数与狂热值，也不引入真假／善恶标准答案。

可以构造自己的`GameRules`并注入会话。配置在创建时校验，集合被复制并只读暴露；规则指纹覆盖所有影响运行的配置。更改数值或日程后，旧规则存档会被拒绝，不自动迁移。

节点和选项ID是逻辑占位标识，没有在本轮补写正式台词。`empathize`等名称仅用于验证，后续可替换为每病人、每日期不同的回答定义。

## 保存与恢复

```csharp
var json = SnapshotCodec.Serialize(game);
var restored = SnapshotCodec.Deserialize(json, rules);

var store = new FileSaveStore("/由宿主指定的目录/save.json");
store.Save(game);
var loaded = store.Load(rules);
```

存档包括完整状态、会谈检查点、每日记录、待执行／已执行事件以及接受的命令历史。恢复会在一个新会话内部重放记录，验证所得状态与保存状态一致；重放期间不会向宿主发布事件，不会再次修改原会话或磁盘。

SchemaVersion、规则名称和完整规则指纹必须匹配；非法命令、损坏JSON、缺失字段、未知字段／枚举、不一致状态均抛出`SnapshotValidationException`。文件不存在或文件系统失败保留相应I/O异常，由宿主处理。

文件保存先写同目录临时文件并刷新，再原子替换目标；替换前失败不会先删旧存档。逻辑库不自动选择保存目录，也不自动在每次命令后写盘。宿主应在回答结算、处置提交和跨日前保存检查点。

## Unity／Yarn接入

新策划包先做结构与路径校验，再在Unity原生编译，成功后才更新活动内容资源；输入、台词与选项显示继续走宿主/桥接，处置仍由核心结算。目录、归档状态与实际会谈记录随存档恢复；当前恢复粒度为对话段，可以重播段内台词。[verified: ../docs/terminal-ui-verification.json scope/unity/limits；MiniGame.Presentation/Authoring/DialoguePackageCompiler.cs L20-L93] [opinion]

两个库通过本地UPM包供Unity2022.3使用；宿主读取 `GameRuntime.GetView()`，提交交互、回答或处置，按回调token通知演出完成。存档由 `RuntimeFileSaveStore` 保存完整核心与宿主检查点。实际Editor/Player验证范围见[玩法迁移记录](../docs/reference-gameplay-migration.md)，IL2CPP未验证。[verified: MiniGame.Core/package.json；MiniGame.Presentation/package.json；MiniGame.Presentation/GameRuntime.cs Save/CompleteCue；MiniGame.Presentation/RuntimeFileSaveStore.cs] [opinion]

适配器负责节点文本、选项呈现、事件演出、中文字体、输入上下文以及文件目录。它查询核心资格并提交命令；不得让UI按钮或Yarn变量直接成为配额、治疗次数、病人去留的另一份权威状态。[verified: ../UnityProject/Assets/MiniGame/Yarn/MiniGameYarnBridge.cs；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs] [opinion]

2026-10-13在Windows使用仓库内 `.dotnet/dotnet.exe` SDK8.0.424，完成锁定恢复、Release构建（0警告、0错误）、Core73项及Presentation77项测试，全部通过。当前结果在忽略目录 `Artifacts/Tests/`，[汇总记录](../Artifacts/Tests/migration-verification-report.json)区分纯.NET验证与Unity验证。[verified: ../Artifacts/Tests/migration-verification-report.json Sdk/Core/Presentation/Scope；../Artifacts/Tests/migration-build.log]

完整设计理由与逐文件导航见[实现说明](architecture-and-rationale.md)。正式规则缺口继续见[设计待定项](../docs/open-questions.md)。

## 最新终端验证与历史记录

2026-10-15使用固定SDK8.0.424完成锁定恢复、Release构建（0警告/错误），Core75与Presentation110共185项测试通过；策划工具18项测试及1296条选择组合通过。Unity原生导入、EditMode2项、PlayMode9项与Windows Mono构建另见[终端验证报告](../docs/terminal-ui-verification.json)。下方2026-10-12/13数据保留历史身份。[verified: ../docs/terminal-ui-verification.json dotnet/planner/unity]

2026-10-12，在这台macOS ARM64上使用.NET SDK8.0.424完成：

- 锁定依赖恢复通过；Release构建通过，0警告、0错误。
- 73项测试全部通过，0失败、0跳过，包括三个结局、神父Day3释放、五人信任独立、多个会谈检查点恢复和原子文件替换。
- 源码说明和项目入口中的46个本地链接均存在；原设计、研究材料和15个参考子模块保持原样。

测试报告由测试命令生成在`MiniGame.Core.Tests/TestResults/core-tests.trx`，属于忽略的本地产物。未来代码变化后以重新执行的结果为准；上述验证不包含Unity编辑器接入、视觉或游戏打包。

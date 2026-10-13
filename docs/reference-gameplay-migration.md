# 《寻找伪人》玩法结构迁移

《寻找伪人》是本项目重要玩法参考。本次按真实恢复代码中的“房间交互 → 对话与观察 → 条件选项 → 决策 → 反应 → 返回房间 → 睡眠与次日后果”连接原创检定员框架；权威规则仍由 `GameSession` 执行。[verified: ../research/no-im-not-a-human/Reports/gameplay-dialogue-progression.md「对话系统与条件触发」「玩家决策与NPC状态」「昼夜循环、行动与物资」；../src/MiniGame.Presentation/GameRuntime.cs TryInteract/CompleteCue] [opinion]

## 已实现的对应关系

- **房间与会谈互斥**：参考 `CharacterRoomObjectView.TryStartDialog` 的对话打开门控、`DialogView.StartDialog/EndDialog` 的进入和结束回调。新 `GameRuntime.GetView().Input` 统一门控移动、选项、观察、暂停和演出；电脑每次只开启当前病人的会谈。[verified: ../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/Rooms/CharacterRoomObjectView.cs L38–L47；../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/DialogSystem/DialogView.cs L128–L197；../src/MiniGame.Presentation/GameRuntime.cs GetView/TryInteract]
- **Yarn 负责呈现，命令进入规则服务**：参考 `DialogCommandsInstance` 的命令桥接与 `FakeOptionView.Option` 的可用条件。新桥捕获会面ID、节点ID与回调代次，选项必须同时符合 Yarn 可用条件和核心资格；选项与处置不直接改配额或患者状态。[verified: ../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/DialogSystem/Commands/DialogCommandsInstance.cs L145–L199；../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/DialogSystem/FakeOptionView.cs L23–L100；../UnityProject/Assets/MiniGame/Yarn/MiniGameYarnBridge.cs SelectResponse/PresentOptionsAsync]
- **观察再决策**：参考 `ShowSign`、`HasCheckedSignsToday` 与检查显示事件。新界面可查看眼睛、手部、牙齿、照片、当日提示、档案，按日/病人/种类去重并存档。这是原创的线索占位文本，没有实现原作腋下、耳部检查或身份真值判定。[verified: ../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/DialogSystem/Commands/DialogCommandsInstance.cs L269–L279；../research/no-im-not-a-human/Reports/npc-identity-checks.md L29；../src/MiniGame.Presentation/PresentationContracts.cs InspectionKind；../src/MiniGame.Presentation/GameRuntime.cs Inspect；../src/MiniGame.Presentation/PrototypeContent.cs GetInspection] [opinion]
- **决策后先反应，再回房**：核心处置只执行一次；反应完成只提交 `CompleteReaction`，再播放退出电脑演出。核心可能已排好下一病人，宿主仍必须回房并再次操作电脑。[verified: ../src/MiniGame.Presentation/GameRuntime.cs SubmitDisposition/CompleteCue/WorldInteractions；../src/MiniGame.Core/Runtime/GameSession.cs Dispose/FinishReaction]
- **睡眠、梦境与跨日后果**：参考 `DayNightController.Change/InitNewDay` 的阶段门控和读档保护。新原型白天完成会谈后才能睡眠，梦后只允许床，次日队列、延迟事件与结局由既有核心决定。[verified: ../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/Infrastructure/DayNight/DayNightController.cs L267–L354；../src/MiniGame.Presentation/GameRuntime.cs CompleteCue/WorldInteractions；../src/MiniGame.Core/Runtime/GameSession.cs Interact/AdvanceDay]
- **恢复会谈与演出检查点**：新存档同时保存经命令重放验证的核心JSON、终端/暂停/演出状态和观察记录。读档生成新回调token，拒绝旧回调及不符合核心生命周期的镜头状态，文件原子替换。[verified: ../src/MiniGame.Presentation/GameRuntime.cs Save/Restore/ValidateCheckpoint；../src/MiniGame.Presentation/RuntimeFileSaveStore.cs Save]

## 原创规则与未迁移机制

范围仍为3天、5病人、3结局；每日净化目标、信任阈值与对白是开发配置。原作的随机访客池、真假身份、能量与物品库存、夜间谋杀、FEMA及原作结局没有移植成新的已确认需求。当前资源压力显示既有净化配额；终止或退出病人由核心排除出后续队列。[verified: ../src/MiniGame.Core/Rules/PrototypeRules.cs Create；../src/README.md「开发默认规则」；../docs/open-questions.md Q07–Q15、Q25] [opinion]

## 工程与验证入口

- [Presentation](../src/MiniGame.Presentation/)：不依赖 Unity 的输入生命周期、公开玩家视图与宿主检查点；[77项测试](../src/MiniGame.Presentation.Tests/)。核心73项测试保持通过。[verified: ../Artifacts/Tests/migration-verification-report.json Core/Presentation]
- [Unity 工程](../UnityProject/README.md)：原创公寓、中文uGUI、六类观察、镜头过渡、夜间床引导、真实 Yarn 桥及 PlayMode 测试源码。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs CreateAndValidate；../UnityProject/Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs]
- 原创对白生成322个核心节点映射为327个Yarn节点、536条文本；固定Yarn编译器检查无诊断。文本全部标为开发占位内容。[verified: ../Artifacts/YarnSmoke/compiler-report.json coreNodes/yarnNodes/strings/diagnostics；../src/MiniGame.Presentation/PrototypeContent.cs Revision；../UnityProject/Assets/MiniGame/Content/mapping.json]
- 真实Unity2022.3.23f1c1离线工程已生成场景；图形模式PlayMode四项通过，覆盖三天Yarn/UI、观察、旧回调、读档视觉恢复和最终帧暂停。145份源码哈希与验证副本匹配，0差异。[verified: ../Artifacts/Unity/offline-playmode-results.xml test-run passed=4/failed=0；../Artifacts/Unity/offline-source-validation.json sourceFiles/mismatches]
- Windows Mono开发构建成功，0错误、1警告；启动10秒出现初始晨检就绪标记且无异常。正常UPM仍报路径错误，IL2CPP未验证；Player启动使用无图形模式，画面检查来自Editor截图。详情见[持久化验证汇总](gameplay-migration-verification.json)。[verified: ../Artifacts/Unity/unity-windows-build.json；../Artifacts/Unity/player-startup.json Ready/StartupExceptions/Scope；../Artifacts/Unity/editor-create.log L76] [opinion]

后续优先替换正式对白与人物差分、落实观察线索的策划规则、制作三天梦境和三结局演出；随机访客与额外物资需先裁决设计待定项。[opinion]

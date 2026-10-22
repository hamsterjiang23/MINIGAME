# 梦境、反叛值与海滩终局调参

2026-10-23 正式结局改为[信任总和条件与自我审判流程](../endings/guide.md)：隐藏条件不再读取反叛值，空间盒床交互直接进入感谢页。下文的反叛阈值与漂流字幕参数仅属于旧配置／预览，不能用于验收正式结局。`[verified: 本轮用户接受的结局修订方案；../../src/MiniGame.Core/Runtime/GameSession.cs FinishDream；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs TryLieDown]`

## 当前每日梦境：McQueen 图片序列

当前三晚各按编号播放 **10 张原画，共 5 秒，每张 0.5 秒**。Day1 鲸尸海滩、Day2 倒悬花海、Day3 受缚待审都使用静止画面和直接切图；不加运动、叠化、折射、重影、CRT 撕裂或色差，只有可调的轻微暗角，默认 `VignetteIntensity = 0.12`。原图自带的扫描线和红蓝细线属于素材本身，仍保留；运行时不添加这些效果。原始素材来源见[图片序列说明](image-sequence-notes.md)，当前播放规则以本文为准。`[verified: UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset L16-L78；UnityProject/Assets/MiniGame/Editor/DreamSettingsWindow.cs L168-L174；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L79-L93；UnityProject/Assets/MiniGame/Resources/DreamProjectionAnimation.shader L134-L140]`

**历史预览**：[Day1](../../Art/Dreams/Previews/day1-sequence-1fps.mp4)、[Day2](../../Art/Dreams/Previews/day2-sequence-1fps.mp4)、[Day3](../../Art/Dreams/Previews/day3-sequence-1fps.mp4)。这些旧视频保留原文件名，本轮没有把它们重录为新流程，不可据此验收当前节奏、原画显示或第二次床交互。隐藏海滩结局仍查阅[固定门口布景指南](cubemap-guide.md)，本轮每日梦境修改不扩大海滩范围。`[opinion]`

从 [MainRoomCombined](../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity) 开始正式游戏；隐藏海上梦结局默认在房间内显示动态全景，原 [DreamEndingBeach](../../UnityProject/Assets/MiniGame/Scenes/DreamEndingBeach.unity) 保留为捕获源制作场景及兼容模式。菜单 **MiniGame → 梦境与结局** 分为“每日梦境”“海滩终局”“玩家反叛值”三个页签；音频文件和音量在 **MiniGame → 音频设置** 中选择。全景操作与更新流程见[动态全景指南](cubemap-guide.md)。`[verified: DreamSettingsWindow.cs DrawEnding；MiniGameSessionHost.cs LateUpdate；DreamCubemapSetup.cs BuildSource]`

## 策划操作

- **每日梦境**：可调整“黑屏音乐时间”`BlackScreenSeconds`（默认 **5 秒**）、“起床前黑屏时间”`WakeBlackSeconds`（默认 **1 秒**），以及各晚画面、音乐用途、播放时长（默认 **5 秒**）和边角压暗强度（默认 **0.12**）。镜头定位默认 **1.2 秒**，在前面的黑屏内完成；返回默认 **1 秒**，投影边缘留白 **0.15 米**。`[verified: UnityProject/Assets/MiniGame/Editor/DreamSettingsWindow.cs L153-L175；UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset L16-L78；UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L222-L237]`
- `CrossfadeSeconds`、`MotionIntensity`、`PostIntensity` 仅保留旧资产兼容字段，已从面板隐藏，每日原画播放不读取它们。改播放时长会按画面张数均分每张停留时间；`VignetteIntensity = 0` 可关闭暗角。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceProfile.cs L10-L17；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L79-L93；UnityProject/Assets/MiniGame/Resources/DreamProjectionAnimation.shader L134-L140]`
- 点击 **预览 Day1/2/3 梦境** 查看墙面效果；工具栏可以暂停、停止预览。预览不推进剧情，停止后恢复镜头；正式演出进行中不会被预览替换。若没有接入，先打开 Combined，退出播放模式，再点击 **更新 Combined 梦境接入与墙面定位**。`[verified: DreamSettingsWindow.cs L102–110、L134–139、L320–340；DreamSequenceDirector.cs PreviewDay/StopInternal]`
- **原画播放时保持画面清晰**：逐图播放期间临时跳过房间复古效果，镜头返回或停止时恢复此前的 bypass 状态；不用修改房间复古资产。播放相机保持固定，切换原画时不再变形或淡入淡出。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L112-L114,L154-L179,L228-L231,L279-L281,L317-L319；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L85-L92]`
- “Combined 场景定位”可指定投影墙面、显示面、观影点及其位置朝向；用 **保存场景定位** 保存这些场景修改。播放模式和预览期间定位只读。`[verified: DreamSettingsWindow.cs L176–218]`
- **海滩终局**：调整前进速度、床交互距离、躺下、漂流、流云及字幕。默认前进 **1.2 米/秒**、交互距离 **1.5 米**、躺下 **1.5 秒**；起伏幅度 **0.08 米**、周期 **8 秒**、摇摆 **1.5°**、漂流 **0.04 米/秒**、流云速度 **0.006**。点击 **打开海滩终局场景** 检查场景。`[verified: DreamSettingsWindow.cs L143–173；DreamSequenceDefault.asset WalkSpeed–CloudSpeed]`
- 播放或暂停时，场景按钮显示 **退出播放并打开海滩**；点击后先结束播放，再打开场景。暂停本身不会退出 Play Mode。窗口顶部也提供同一快捷按钮，正常的场景保存提示仍会保留。`[verified: DreamSettingsWindow.cs RequestOpenBeach/CompletePendingBeachOpen/OpenBeach]`
- **海浪与岸边**：默认海浪高度 **0.24 米**、周期 **6 秒**、波长 **7 米**、冲刷距离 **1.8 米**、泡沫强度 **0.85**。海水起伏、冲岸白沫与湿沙使用同一时间轴，暂停会冻结，读取存档恢复该时间。设置入口在“海滩终局”页；[新版动态海浪预览](../../Art/Dreams/Previews/beach-waves-preview.mp4)与[实现参考](water-revision.md)单独保留。`[verified: DreamSequenceDefault.asset SeaWaveHeight–SeaFoamAmount；DreamEndingController.cs ApplyWaterParameters/RestoreCheckpoint]`
- 五句终局字幕默认在躺稳后 **2 秒**开始，每句无配音时显示 **4 秒**；可以逐句填写文字、选择可选配音并试听。有配音时按音频长度推进，留空仍显示字幕。`[verified: DreamSequenceDefault.asset SubtitleDelay/EndingLines；DreamSettingsWindow.cs L150–171；DreamEndingController.cs AdvanceSubtitles]`
- 在 **音频设置** 中为“海滩终局音乐”选择文件；该槽目前留空，“海浪环境声”已绑定提供的海浪文件。音频槽留空即不播放，不会自动借用别的梦境曲目。`[verified: MiniGameAudioProfile.cs MiniGameSound/声音条目说明；AudioDefault.asset Sounds Id13/Id14]`

用工具栏 **保存配置** 保存参数；配置编辑支持撤销／重做，播放中保存的资产修改退出播放后仍保留。场景位置需另外保存场景。`[verified: DreamSettingsWindow.cs L85–86、L102、L214–218、L352–366]`

## 玩家全局反叛值

反叛值属于**玩家**，独立于每位患者的信任，也不与疯狂值、偏离指数或狂热值合并。默认初始值 **0**、上限 **100**、梦结局阈值 **100**，未配置的回答增减为 **0**；所以默认配置不会自行积累反叛值。`[verified: PlayerRebellion.asset InitialValue/Maximum/Threshold/Choices；GameSession.cs Select；本轮已接受的数值范围约定]`

“玩家反叛值”页可按人物、日期筛选，搜索文字或稳定选项 ID，配置**普通及特殊对话中的回答选项**增减；处置不额外计入。有效回答只结算一次，数值限制在 0 至上限之间；拒绝、重复回调与读档重放不会额外增加。面板显示当前玩家全局值。`[verified: DreamSettingsWindow.cs DrawRebellion/ReloadChoices；GameSession.cs Select/Restore；GameRules.cs RebellionOptionDeltas]`

反叛规则修改、保存后只对**下一次新游戏**生效，不改写当前会话或已结算的回答；新规则与旧检查点不匹配时拒绝读取。检查特殊分支时可将初始值临时设为 100 后新开一局，同时确保没有神父袭击和 KPI 失败条件；检查后恢复正式配置。`[verified: DreamSettingsWindow.cs L85、L366；GameRules.cs WithDreamFlow；GameSession.cs Restore/ResolveEnding]` `[opinion]`

## 正式流程与操作

- **Day0**：启动显示监管者监控，点击拉远后完成教学；自动关机后直接亮床引导，不播放墙面梦境。关闭家具碰撞，保留地面、墙、玩家及床触发器；一次床交互播放闭眼后进入Day1，恢复碰撞前回到安全起床点。详见[开场指南](../opening/guide.md)。`[verified: PresentationEffects.DayZeroOpening/SleepTransition；GameRuntime.BeginNightDream/CompleteCue；BedtimeCollisionGate.Apply]`
- **Day1–3 第一次上床前**：工作结束并退出电脑后，保留普通夜景，床可交互。第一次按 E 上床，先用 **0.8 秒**闭眼，再进入 **5 秒**黑屏音乐；镜头定位在黑屏内完成，房间保持黑暗。`[verified: UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs L161-L174；src/MiniGame.Presentation/GameRuntime.cs L157-L166；UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs L499-L528；UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L130-L135,L154-L164,L222-L226]`
- **Day1–3 原画与返回**：黑屏后锁定镜头，10 张原画各停留 **0.5 秒**；播放期间输入仍由梦境演出占用。最后一张结束后，房间继续全黑，仅床的强聚光和光束已点亮；镜头用 **1 秒**转回后方床位，完成后停在 `AfterDream` 等待玩家。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L79-L93；UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L168-L190,L233-L237；UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs L178-L189,L449-L456；src/MiniGame.Presentation/GameRuntime.cs L295-L314]`
- **Day1–3 第二次上床后**：再次按 E 才开始 **0.8 秒**闭眼，随后保持全黑 **1 秒**，完成这次 Sleep cue 才进入次日或既有普通结局。梦境结束本身不会自动醒来；重复 E、旧 cue 和暂停中的完成回调不能提前推进日期。`[verified: UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs L499-L528；src/MiniGame.Presentation/GameRuntime.cs L117-L120,L163-L166,L282-L314,L358-L360；src/MiniGame.Presentation.Tests/DreamFlowRuntimeTests.cs L56-L126]`
- **Day3 特殊分支**：优先级为**神父袭击 → KPI 失败 → 海上梦结局 → 普通结局**。达到反叛阈值且没有更高优先级条件时，梦后改为仅电脑引导；终端显示审查员本人实时监控，只有“净化”一个行动按钮。鼠标点击或 Enter 确认一次后进入海滩，不消耗患者净化配额，也不新建患者会谈。`[verified: GameRules.cs WithDreamFlow；GameSession.cs FinishDream/SelfPurify/ResolveEnding；TerminalView.cs HandleSelfReviewKey；SelfReviewMonitor.cs ApplyView]`
- **隐藏海上梦结局**：房间内仅显示床，使用 **WASD** 自由行走、鼠标转头；到床边按 **E** 躺下，之后仰看流云，随床缓慢漂流起伏并显示字幕；按 **Esc** 暂停。动态海滩全景包围玩家；普通三天梦境仍采用各自的插画演出。关闭全景模式时保留原海滩场景只向前行走的兼容路径。`[verified: RoomDreamEnding.cs AdvanceExploration；DreamEndingController.cs Update/AdvanceSubtitles；MiniGameSessionHost.cs LateUpdate]`

## 保存与验收

梦境流程使用 `Application.persistentDataPath/campaign-dream-save.json`；旧的 `campaign-save.json` 保留。房间与海滩由同一个持久会话持有游戏进度，存档同时记录放映／漂流阶段及房间镜头返回位置。Combined 可用 F5／F9，海滩从 Esc 暂停菜单保存或恢复；场景正在切换时须等切换结束。`[verified: MiniGameSessionHost.cs L25、L28–40、L69–142；MiniGameBootstrap.cs Update/Save/Load；DreamEndingController.cs BuildInterface]`

**本轮 .NET 验证**：核心 **114 项**、表现层 **233 项**，合计 **347 项通过，0 失败**。报告为 `src/MiniGame.Core.Tests/TestResults/dream-night-flow_net8.0_20261022183526.trx` 与 `src/MiniGame.Presentation.Tests/TestResults/dream-night-flow_net8.0_20261022183533.trx`。`[verified: 前一报告 L702；后一报告 L1416]`

**本轮 Unity 验证**：26 项 PlayMode 与 3 项 EditMode 测试通过，覆盖原画像素、5 秒黑屏、暂停读档、聚光床、第二次交互与 1 秒起床黑屏、面板保存和撤销。详见[本轮验证](night-flow-2026-10-22/verification.json)与[聚光床截图](night-flow-2026-10-22/day1-spotlight-bed.png)。未制作新录像或 Windows 构建，未做独立听感验收。`[verified: night-flow-2026-10-22/verification.json latestTestOutcomes/scope/limitations]`

存档保留梦境黑屏、原画播放和镜头返回进度；暂停恢复 Returning 时立即恢复床聚光，暂停期间不等待渐亮。第二次床交互产生的 `AfterDream + Sleep` 检查点可以恢复，旧实例的 cue token 失效。闭眼与起床黑屏过渡按恢复后的 Sleep cue 重新演出，不能把它当作逐帧恢复。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L240-L269；UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs L178-L189；src/MiniGame.Presentation/GameRuntime.cs L662,L750-L760；UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs L499-L528]`

**历史验证**：[verification.json](verification.json) 记录于 **2026-10-19**，其中的 **293 项 .NET、49 项 PlayMode** 及旧构建／启动结果仅属于当时版本。`[verified: verification.json L3,L24,L71]`

海滩的可编辑 [Prefab](../../UnityProject/Assets/MiniGame/Art/DreamEnding/DreamEndingStage.prefab) 与 [Blender 源文件](../../Art/DreamEnding/DreamEndingBeach.blend) 沿用既有交付；本轮不将每日静止原画改造扩展到海滩演出。`[opinion]`

**更早的单图与海滩录制**：[Day1 鲸尸海滩](../../Art/Dreams/Previews/day1-whale-beach.mp4)、[Day2 无尽长路](../../Art/Dreams/Previews/day2-endless-road.mp4)、[Day3 受缚待审](../../Art/Dreams/Previews/day3-bound-judgment.mp4)、[海滩终局](../../Art/Dreams/Previews/beach-ending.mp4)、[无字幕流云](../../Art/Dreams/Previews/beach-clouds-preview.mp4)。这些是历史材料，不能作为本轮静止序列、节奏或操作流程的录像证据，也不替代实时性能或人工听感验收。`[opinion]`

随仓库交付的预览文件与哈希见 [预览清单](../../Art/Dreams/Previews/manifest.json)；选定的录制、测试及构建报告见 [证据索引](evidence/index.json)。这些副本与原报告保持字节一致，原报告中的本机 Artifacts 路径作为历史来源保留；原始 PNG 帧与构建目录不纳入此交付。`[verified: Previews/manifest.json purpose/files；evidence/index.json purpose/files]`

素材来源及本轮范围裁决见 [source-notes.md](source-notes.md)，实现中发现的问题和处理见 [implementation-notes.md](implementation-notes.md)。`[verified: source-notes.md；implementation-notes.md]`

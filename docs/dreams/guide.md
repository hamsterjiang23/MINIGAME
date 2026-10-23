# 梦境、反叛值与海滩终局调参

2026-10-23 正式结局使用[信任总和条件与自我审判流程](../endings/guide.md)：隐藏条件不再读取反叛值，床交互后用1.5秒躺下、固定仰看蓝天10秒、1秒淡黑，再回主菜单。自审复用病房背景与黄色滤镜；Day3隐藏分支梦后直接亮电脑，不亮床。音乐使用用户提供的《An Ending (Ascent)》，全黑先播放3秒，再连续播放进入海滩。下文的反叛阈值与漂流字幕参数仅属于旧配置／预览，不能用于验收正式结局。`[verified: 本轮用户接受的隐藏结局演出修复方案；../../src/MiniGame.Core/Runtime/GameSession.cs ResolveDreamReturnPhase；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs AdvanceBedEnding；../../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs SetDreamReturnReveal]`

## 当前每日梦境：McQueen 图片序列

当前三晚各按编号播放 **10 张原画，共 5 秒，每张 0.5 秒**。Day1 鲸尸海滩、Day2 倒悬花海、Day3 受缚待审都使用静止画面和直接切图；不加运动、叠化、折射、重影、CRT 撕裂或色差，只有可调的轻微暗角，默认 `VignetteIntensity = 0.12`。原图自带的扫描线和红蓝细线属于素材本身，仍保留；运行时不添加这些效果。原始素材来源见[图片序列说明](image-sequence-notes.md)，当前播放规则以本文为准。`[verified: UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset L16-L78；UnityProject/Assets/MiniGame/Editor/DreamSettingsWindow.cs L168-L174；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L79-L93；UnityProject/Assets/MiniGame/Resources/DreamProjectionAnimation.shader L134-L140]`

**历史预览**：[Day1](../../Art/Dreams/Previews/day1-sequence-1fps.mp4)、[Day2](../../Art/Dreams/Previews/day2-sequence-1fps.mp4)、[Day3](../../Art/Dreams/Previews/day3-sequence-1fps.mp4)。这些旧视频保留原文件名，本轮没有把它们重录为新流程，不可据此验收当前节奏、原画显示或第二次床交互。隐藏海滩结局仍查阅[固定门口布景指南](cubemap-guide.md)，本轮每日梦境修改不扩大海滩范围。`[opinion]`

从 [MainRoomCombined](../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity) 开始正式游戏；隐藏海上梦结局默认在房间内显示动态全景，原 [DreamEndingBeach](../../UnityProject/Assets/MiniGame/Scenes/DreamEndingBeach.unity) 保留为捕获源制作场景及兼容模式。菜单 **MiniGame → 梦境与结局** 分为“每日梦境”“海滩终局”“玩家反叛值”三个页签；音频文件和音量在 **MiniGame → 音频设置** 中选择。全景操作与更新流程见[动态全景指南](cubemap-guide.md)。`[verified: DreamSettingsWindow.cs DrawEnding；MiniGameSessionHost.cs LateUpdate；DreamCubemapSetup.cs BuildSource]`

## 策划操作

- **每日梦境**：可调整“黑屏音乐时间”`BlackScreenSeconds`（默认 **5 秒**）、各晚画面、音乐用途、播放时长（默认 **5 秒**）和边角压暗强度（默认 **0.12**）。镜头定位默认 **1.2 秒**，在前面的黑屏内完成；片尾淡黑默认 **1 秒**，投影边缘留白 **0.15 米**。“起床前黑屏时间”`WakeBlackSeconds`（默认 **1 秒**）只用于自审返回与独立预览；普通梦在片尾全黑后直接交接醒来。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs Advance/CompleteIntoWake；../../UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset BlackScreenSeconds/WakeBlackSeconds/ReturnSeconds]`
- `CrossfadeSeconds`、`MotionIntensity`、`PostIntensity` 仅保留旧资产兼容字段，已从面板隐藏，每日原画播放不读取它们。改播放时长会按画面张数均分每张停留时间；`VignetteIntensity = 0` 可关闭暗角。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceProfile.cs L10-L17；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L79-L93；UnityProject/Assets/MiniGame/Resources/DreamProjectionAnimation.shader L134-L140]`
- 点击 **预览 Day1/2/3 梦境** 查看墙面效果；工具栏可以暂停、停止预览。预览不推进剧情，停止后恢复镜头；正式演出进行中不会被预览替换。若没有接入，先打开 Combined，退出播放模式，再点击 **更新 Combined 梦境接入与墙面定位**。`[verified: DreamSettingsWindow.cs L102–110、L134–139、L320–340；DreamSequenceDirector.cs PreviewDay/StopInternal]`
- **原画播放时保持画面清晰**：逐图播放期间临时跳过房间复古效果，镜头返回或停止时恢复此前的 bypass 状态；不用修改房间复古资产。播放相机保持固定，切换原画时不再变形或淡入淡出。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L112-L114,L154-L179,L228-L231,L279-L281,L317-L319；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L85-L92]`
- “Combined 场景定位”可指定投影墙面、显示面、观影点及其位置朝向；用 **保存场景定位** 保存这些场景修改。播放模式和预览期间定位只读。`[verified: DreamSettingsWindow.cs L176–218]`
- **海滩终局**：正式房间结局保留行走、交互距离和躺下参数；躺稳后固定看天 **10 秒**、淡黑 **1 秒**。旧独立海滩预览仍使用漂流、起伏、摇摆与字幕参数；修改这些参数不改变正式结局的十秒锁定。点击 **打开海滩终局场景** 可检查兼容场景。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs UsesBedEndingSequence/AdvanceBedEnding/ApplyPose；../../UnityProject/Assets/MiniGame/Editor/DreamSettingsWindow.cs DrawEnding]`
- 播放或暂停时，场景按钮显示 **退出播放并打开海滩**；点击后先结束播放，再打开场景。暂停本身不会退出 Play Mode。窗口顶部也提供同一快捷按钮，正常的场景保存提示仍会保留。`[verified: DreamSettingsWindow.cs RequestOpenBeach/CompletePendingBeachOpen/OpenBeach]`
- **海浪与岸边**：默认海浪高度 **0.24 米**、周期 **6 秒**、波长 **7 米**、冲刷距离 **1.8 米**、泡沫强度 **0.85**。海水起伏、冲岸白沫与湿沙使用同一时间轴，暂停会冻结，读取存档恢复该时间。设置入口在“海滩终局”页；[新版动态海浪预览](../../Art/Dreams/Previews/beach-waves-preview.mp4)与[实现参考](water-revision.md)单独保留。`[verified: DreamSequenceDefault.asset SeaWaveHeight–SeaFoamAmount；DreamEndingController.cs ApplyWaterParameters/RestoreCheckpoint]`
- **旧预览字幕**：独立海滩预览的五句字幕默认在躺稳后 **2 秒**开始，每句无配音时显示 **4 秒**；有配音时按音频长度推进。正式房间结局不进入漂流字幕阶段。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs AdvanceForTests/AdvanceSubtitles；../../UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset SubtitleDelay/EndingLines]`
- 在 **音频设置** 中为“海滩终局音乐”选择文件；该槽已绑定用户指定的《An Ending (Ascent)》（`EndingAscent.mp3`），“海浪环境声”保留现有文件。音频槽留空即不播放，不会自动借用别的梦境曲目。`[verified: MiniGameAudioProfile.cs MiniGameSound/声音条目说明；AudioDefault.asset Sounds Id13/Id14]`

用工具栏 **保存配置** 保存参数；配置编辑支持撤销／重做，播放中保存的资产修改退出播放后仍保留。场景位置需另外保存场景。`[verified: DreamSettingsWindow.cs L85–86、L102、L214–218、L352–366]`

## 玩家全局反叛值

反叛值属于**玩家**，独立于每位患者的信任，也不与疯狂值、偏离指数或狂热值合并。初始值、上限和回答增减仍可配置；历史 `Threshold` 字段不再决定正式隐藏结局。正式隐藏条件使用四位患者的历史信任总和，见[结局指南](../endings/guide.md)。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs Select/ResolveDreamReturnPhase/HasHiddenEndingTrust]`

“玩家反叛值”页可按人物、日期筛选，搜索文字或稳定选项 ID，配置**普通及特殊对话中的回答选项**增减；处置不额外计入。有效回答只结算一次，数值限制在 0 至上限之间；拒绝、重复回调与读档重放不会额外增加。面板显示当前玩家全局值。`[verified: DreamSettingsWindow.cs DrawRebellion/ReloadChoices；GameSession.cs Select/Restore；GameRules.cs RebellionOptionDeltas]`

反叛规则修改、保存后只对**下一次新游戏**生效，不改写当前会话或已结算的回答；新规则与旧检查点不匹配时拒绝读取。将反叛初始值设为 100 不会触发正式隐藏分支；验收应通过真实对白与处置累积历史信任并到达 Day3 梦境。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs Restore/ResolveDreamReturnPhase/HasHiddenEndingTrust；../../UnityProject/Assets/MiniGame/Tests/PlayMode/EndingCampaignPlayModeTests.cs EndingCampaignScenario.Drive]`

## 正式流程与操作

- **Day0**：启动显示监管者监控，点击拉远后完成教学；自动关机后直接亮床引导，不播放墙面梦境。关闭家具碰撞，保留地面、墙、玩家及床触发器；一次床交互播放闭眼后进入Day1，恢复碰撞前回到安全起床点。详见[开场指南](../opening/guide.md)。`[verified: PresentationEffects.DayZeroOpening/SleepTransition；GameRuntime.BeginNightDream/CompleteCue；BedtimeCollisionGate.Apply]`
- **Day1–3 上床前**：工作结束并退出电脑后保留普通夜景，床可交互。按 E 后依次平移 **0.65 秒**、仰躺旋转 **1.25 秒**、闭眼 **0.4 秒**，保持全黑 **1.5 秒**后进入每日梦境的 **5 秒**黑屏音乐；观影镜头在黑屏中定位。上述上床时长取自当前 Combined 场景的序列化配置。`[verified: ../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity L11201–L11204；../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs SleepTransition/ApplySleepFrame；../../UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs Advance]`
- **原画与普通醒来**：黑屏后锁定镜头，10 张原画各停留 **0.5 秒**。最后一张结束后用 **1 秒**淡黑；全黑时一次完成梦境并推进次日或既有普通结局，随后从床上视角用 **0.4 秒**睁眼、**1.25 秒**回正、**0.65 秒**下床。普通梦后不进入可操作的床引导；床聚光、光束与粒子全程关闭。进入次日时仍由既有事件播放监督员敲门，不另发重复事件。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs Advance/CompleteIntoWake；../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs Present/WakeTransition/ApplyWakeFrame；../../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs SetTargets/ApplyFullRoom；../../src/MiniGame.Presentation/GameRuntime.cs Accept]`
- **普通梦后操作**：无需再次按 E。醒来演出期间锁定移动、转头和世界交互；暂停冻结演出时间，恢复后完成剩余部分再释放控制。重复床交互、旧 cue 或额外放映时钟不会重复推进日期；Day3满足隐藏条件时仍留在当天自审，不自动跨日。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs WakeTransition；../../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs Update；../../src/MiniGame.Presentation/GameRuntime.cs CompleteCue/Accept；../../UnityProject/Assets/MiniGame/Tests/PlayMode/DreamCampaignIntegrationTests.cs DayOneBedProjectsOnTheAuthoredWallThenWakesOnceAtFullBlack]`
- **Day3 隐藏分支**：梦结束前由核心只读 `DreamReturnPhase` 预告去向，与正式完成梦境使用同一条件。薇拉、埃德温、米洛、阿德里安的历史信任合计≥10时，黑幕中直接启用电脑引导并朝电脑恢复镜头；床灯、光束和粒子保持关闭。此检查优先于当晚其余结局；更早已经触发的结局不会继续到Day3。进入电脑后显示玩家目录与病房黄滤镜中的16句自审，读完才可唯一净化，不消耗患者配额或新建患者会谈。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs ResolveDreamReturnPhase/HasHiddenEndingTrust/AdvanceSelfReview/SelfPurify；../../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs SetTargets；../endings/guide.md 分支表]`
- **正式隐藏海上梦结局**：指定音乐在全黑中先播放 **3 秒**，再连续播放进入海滩。玩家使用 **WASD** 自由行走、鼠标转头，到床边点击**鼠标左键**开始 **1.5 秒**平移与旋转躺下；躺稳后锁定蓝天 **10 秒**，随后 **1 秒**淡黑，EndNote响起并同时回主菜单。上床后不再接受移动、转头或重复交互；按 **Esc** 暂停冻结演出计时。独立海滩预览保留原先前进行走、漂流和字幕流程。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs BlackHoldSeconds；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs Update/TryLieDown/AdvanceBedEnding/ApplyPose；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs CompleteEnding]`

## 保存与验收

梦境流程使用 `Application.persistentDataPath/campaign-dream-save.json`；旧的 `campaign-save.json` 保留。房间与海滩由同一个持久会话持有游戏进度，存档同时记录放映／漂流阶段及房间镜头返回位置。Combined 可用 F5／F9，海滩从 Esc 暂停菜单保存或恢复；场景正在切换时须等切换结束。`[verified: MiniGameSessionHost.cs L25、L28–40、L69–142；MiniGameBootstrap.cs Update/Save/Load；DreamEndingController.cs BuildInterface]`

**2026-10-22 历史 .NET 验证**：核心 **114 项**、表现层 **233 项**，合计 **347 项通过，0 失败**。报告为 `src/MiniGame.Core.Tests/TestResults/dream-night-flow_net8.0_20261022183526.trx` 与 `src/MiniGame.Presentation.Tests/TestResults/dream-night-flow_net8.0_20261022183533.trx`；这些结果不覆盖2026-10-23的隐藏结局修订。`[verified: 前一报告 L702；后一报告 L1416]`

**2026-10-22 历史 Unity 验证**：26 项 PlayMode 与 3 项 EditMode 测试通过，覆盖当时的原画像素、5秒黑屏、暂停读档、聚光床、第二次交互、面板保存和撤销。详见[历史验证](night-flow-2026-10-22/verification.json)与[聚光床截图](night-flow-2026-10-22/day1-spotlight-bed.png)。当时未制作新录像或 Windows 构建，未做独立听感验收；这些结果不覆盖本次自审返回和海滩收尾。`[verified: night-flow-2026-10-22/verification.json latestTestOutcomes/scope/limitations]`

存档保留梦境黑屏、原画播放和片尾淡黑进度，以及醒来阶段、经过时间、床与站立相机姿态。醒来读档首帧立即恢复相机和黑幕；暂停不消耗演出时间。旧普通梦尾的 `BlackHold`／`Revealing` 检查点统一在全黑中交接醒来，不再显示床引导；自审分支仍按原阶段恢复电脑引导。旧正式日 `AfterDream`（包括原第二次上床的检查点）加载时迁移到自动醒来；暂停加载先保持全黑，解除暂停才推进一次日期。Version 1 睡眠检查点继续兼容，Day0仍按原先单次上床流程。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs RestoreCheckpoint；../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs SleepTransitionCheckpoint.ForRestoredView/RestoreSleepCheckpoint/RestoreLegacyDreamWake；../../src/MiniGame.Presentation/GameRuntime.cs Restore/Accept；../../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs RestoreFromSession]`

**历史验证**：[verification.json](verification.json) 记录于 **2026-10-19**，其中的 **293 项 .NET、49 项 PlayMode** 及旧构建／启动结果仅属于当时版本。`[verified: verification.json L3,L24,L71]`

海滩的可编辑 [Prefab](../../UnityProject/Assets/MiniGame/Art/DreamEnding/DreamEndingStage.prefab) 与 [Blender 源文件](../../Art/DreamEnding/DreamEndingBeach.blend) 沿用既有交付；本轮不将每日静止原画改造扩展到海滩演出。`[opinion]`

**更早的单图与海滩录制**：[Day1 鲸尸海滩](../../Art/Dreams/Previews/day1-whale-beach.mp4)、[Day2 无尽长路](../../Art/Dreams/Previews/day2-endless-road.mp4)、[Day3 受缚待审](../../Art/Dreams/Previews/day3-bound-judgment.mp4)、[海滩终局](../../Art/Dreams/Previews/beach-ending.mp4)、[无字幕流云](../../Art/Dreams/Previews/beach-clouds-preview.mp4)。这些是历史材料，不能作为本轮静止序列、节奏或操作流程的录像证据，也不替代实时性能或人工听感验收。`[opinion]`

随仓库交付的预览文件与哈希见 [预览清单](../../Art/Dreams/Previews/manifest.json)；选定的录制、测试及构建报告见 [证据索引](evidence/index.json)。这些副本与原报告保持字节一致，原报告中的本机 Artifacts 路径作为历史来源保留；原始 PNG 帧与构建目录不纳入此交付。`[verified: Previews/manifest.json purpose/files；evidence/index.json purpose/files]`

素材来源及本轮范围裁决见 [source-notes.md](source-notes.md)，实现中发现的问题和处理见 [implementation-notes.md](implementation-notes.md)。`[verified: source-notes.md；implementation-notes.md]`

# 梦境、反叛值与海滩终局调参

## 当前每日梦境：McQueen 图片序列

当前三晚已接入 McQueen 最新的 30 张画面，每晚按编号播放 10 张、时长 20 秒。Day1 鲸滩、Day2 倒悬花海、Day3 红黑审视分别使用递进的折射、畸变重影、CRT 撕裂与色差；可在“每日梦境”页调整“顺序画面”“画面叠化时长”“运动强度”“梦境后处理强度”。制作细节与资料来源见[图片序列说明](image-sequence-notes.md)。[verified: DreamSequenceDefault.asset Days；DreamSettingsWindow.DrawDailyDreams；DreamProjectionAnimation.shader]

本轮预览：[Day1](../../Art/Dreams/Previews/day1-sequence.mp4)、[Day2](../../Art/Dreams/Previews/day2-sequence.mp4)、[Day3](../../Art/Dreams/Previews/day3-sequence.mp4)。下文较早的单图制作与视频记录保留历史身份；隐藏海滩结局的当前实现以[固定门口布景指南](cubemap-guide.md)为准。[verified: Previews/daily-sequences-manifest.json；cubemap-guide.md]

从 [MainRoomCombined](../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity) 开始正式游戏；隐藏海上梦结局默认在房间内显示动态全景，原 [DreamEndingBeach](../../UnityProject/Assets/MiniGame/Scenes/DreamEndingBeach.unity) 保留为捕获源制作场景及兼容模式。菜单 **MiniGame → 梦境与结局** 分为“每日梦境”“海滩终局”“玩家反叛值”三个页签；音频文件和音量在 **MiniGame → 音频设置** 中选择。全景操作与更新流程见[动态全景指南](cubemap-guide.md)。`[verified: DreamSettingsWindow.cs DrawEnding；MiniGameSessionHost.cs LateUpdate；DreamCubemapSetup.cs BuildSource]`

## 策划操作

- **每日梦境**：按 Day1 鲸尸海滩、Day2 无尽长路、Day3 受缚待审配置画面、音乐用途、播放时长与运动强度。默认每段 **20 秒**、运动强度 **1**，镜头定位 **1.2 秒**、返回 **1 秒**、投影边缘留白 **0.15 米**。`[verified: DreamSettingsWindow.cs L114–140；DreamSequenceDefault.asset AlignSeconds/ReturnSeconds/ProjectionInset/Days]`
- 点击 **预览 Day1/2/3 梦境** 查看墙面效果；工具栏可以暂停、停止预览。预览不推进剧情，停止后恢复镜头；正式演出进行中不会被预览替换。若没有接入，先打开 Combined，退出播放模式，再点击 **更新 Combined 梦境接入与墙面定位**。`[verified: DreamSettingsWindow.cs L102–110、L134–139、L320–340；DreamSequenceDirector.cs PreviewDay/StopInternal]`
- **动画播放时保持原画清晰**：墙面动画实际播放期间临时跳过房间复古效果，进入镜头返回或停止预览后恢复原状态；无需到复古面板改参数，也不改动原图预览或组件启用选择。投影矩形内采用前景优先显示，避免吊灯等房间物体盖住动画；正常房间继续使用原复古设置。`[verified: DreamSequenceDirector.cs L151–173、L270–308；RetroScreenEffect.cs L14–28；DreamProjectionSurface.shader L6–8]`
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
- **Day1–3**：工作结束并退出电脑后，亮床引导；玩家与床交互，完成闭眼过渡后才播放对应梦境。播放期间没有 Continue，不能用空格或 E 跳过；结束后自动推进到次日，或进入既有自我审查／结局分支。睡眠与梦境中的存档保留这次床交互，恢复不会要求重复上床。梦境素材后续继续由美术补充。`[verified: GameRuntime.TryInteract/CompleteCue/Save/Restore；DreamSequenceDirector.StartDay/Advance；DreamFlowRuntimeTests.BedStartsSleepBeforeAnyDreamAndRestoresAcrossBothPresentations]`
- **Day3 特殊分支**：优先级为**神父袭击 → KPI 失败 → 海上梦结局 → 普通结局**。达到反叛阈值且没有更高优先级条件时，梦后改为仅电脑引导；终端显示审查员本人实时监控，只有“净化”一个行动按钮。鼠标点击或 Enter 确认一次后进入海滩，不消耗患者净化配额，也不新建患者会谈。`[verified: GameRules.cs WithDreamFlow；GameSession.cs FinishDream/SelfPurify/ResolveEnding；TerminalView.cs HandleSelfReviewKey；SelfReviewMonitor.cs ApplyView]`
- **隐藏海上梦结局**：房间内仅显示床，使用 **WASD** 自由行走、鼠标转头；到床边按 **E** 躺下，之后仰看流云，随床缓慢漂流起伏并显示字幕；按 **Esc** 暂停。动态海滩全景包围玩家；普通三天梦境仍采用各自的插画演出。关闭全景模式时保留原海滩场景只向前行走的兼容路径。`[verified: RoomDreamEnding.cs AdvanceExploration；DreamEndingController.cs Update/AdvanceSubtitles；MiniGameSessionHost.cs LateUpdate]`

## 保存与验收

梦境流程使用 `Application.persistentDataPath/campaign-dream-save.json`；旧的 `campaign-save.json` 保留。房间与海滩由同一个持久会话持有游戏进度，存档同时记录放映／漂流阶段及房间镜头返回位置。Combined 可用 F5／F9，海滩从 Esc 暂停菜单保存或恢复；场景正在切换时须等切换结束。`[verified: MiniGameSessionHost.cs L25、L28–40、L69–142；MiniGameBootstrap.cs Update/Save/Load；DreamEndingController.cs BuildInterface]`

**验证结果**：核心及表现层 .NET 测试 **293 项通过**；Unity PlayMode 按完整测试名去重 **49 项通过**，Editor 调参测试 **3 项通过**。六个梦境相关 Shader 无编译错误；Windows 构建成功，必要资源已打包，Player 成功启动正式梦境 Campaign。构建有 10 条既有包警告：1 条 Pipeline Player 未启用、9 条 Yarn 旧输入字段未使用。详细检查与证据路径见 [verification.json](verification.json)。`[verified: verification.json core/editor/player]`

三段梦境采用附件插画加 Unity 实时运动；海滩使用 Blender 地形、原房间床和动态海水／天空。可编辑海滩 [Prefab](../../UnityProject/Assets/MiniGame/Art/DreamEnding/DreamEndingStage.prefab) 与 [Blender 源文件](../../Art/DreamEnding/DreamEndingBeach.blend) 已保存。`[verified: DreamAnimationStage.cs Render；DreamEndingSceneBuilder.cs BuildScene；verification.json authoring]`

查看录制：[Day1 鲸尸海滩](../../Art/Dreams/Previews/day1-whale-beach.mp4)、[Day2 无尽长路](../../Art/Dreams/Previews/day2-endless-road.mp4)、[Day3 受缚待审](../../Art/Dreams/Previews/day3-bound-judgment.mp4)、[海滩终局](../../Art/Dreams/Previews/beach-ending.mp4)、[无字幕流云](../../Art/Dreams/Previews/beach-clouds-preview.mp4)。这些是生产相机的固定步长画面，声音由同一配置音频后期混合；不代表实时性能或人工听感验收。Player 本轮检查范围为构建、资源包含和启动，完整跨场景流程由 Editor PlayMode 测试覆盖。`[verified: verification.json visual/audio/player.scope]`

随仓库交付的预览文件与哈希见 [预览清单](../../Art/Dreams/Previews/manifest.json)；选定的录制、测试及构建报告见 [证据索引](evidence/index.json)。这些副本与原报告保持字节一致，原报告中的本机 Artifacts 路径作为历史来源保留；原始 PNG 帧与构建目录不纳入此交付。`[verified: Previews/manifest.json purpose/files；evidence/index.json purpose/files]`

素材来源及本轮范围裁决见 [source-notes.md](source-notes.md)，实现中发现的问题和处理见 [implementation-notes.md](implementation-notes.md)。`[verified: source-notes.md；implementation-notes.md]`

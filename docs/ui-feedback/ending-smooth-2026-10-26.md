# 2026-10-26 隐藏结局镜头与音乐验收

本轮按用户确认执行：1秒渐黑、1秒全黑，随后1秒渐显与2秒退镜同时开始；上床到看天空总计4秒（转身1秒，后仰／穿墙／看天3秒）；海滩BGM降至此前70%，海浪不变。[verified: 本轮用户四项请求与后续两项确认]

## 实现与边界

- 自审提交的同一帧冻结最后的终端RenderTexture，直到黑幕全覆盖后才停用旧画面；渐黑期间保持相机位置、朝向与FOV，避免先露出房间或旧镜头回退。[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs Render；../../UnityProject/Assets/MiniGame/Runtime/TerminalDisplay.cs FreezeOutput；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs LateUpdate]
- 入场时钟改为渐黑1秒、全黑1秒、进入2秒，渐显仅覆盖进入阶段前1秒；初始化在黑幕下完成。隐藏结局退镜不再使用普通退镜前28%的起步等待，位置、旋转与FOV保持连续缓动；普通终端关机仍使用原时长与曲线。[verified: ../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs RoomEndingTransitionClock；../../UnityProject/Assets/MiniGame/Runtime/TerminalReturnMotion.cs；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs ApplyEntryPose]
- 上床前1秒原地转身，后3秒后仰、仰头溶解墙画、越墙并稳定看天空；延长最后穿墙动作，保留只有朝上接近82度后才显露海滩、越墙前墙画已消失以及末帧无翻转的限制。[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs LieDownDuration/LieDownTurnDuration/LieDownProgress；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs ApplyWallLieDown/UpdatePortalView；../../UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset LieDownSeconds]
- `EndingMusic`音量从1改为0.7，生成默认资源的Editor入口同步；海浪仍为1。[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/AudioDefault.asset Sounds[Id=13]/Sounds[Id=14]；../../UnityProject/Assets/MiniGame/Editor/MiniGameAudioSetup.cs]
- 过渡存档增加时序版本，旧0.35／3／1.2秒记录按阶段比例迁移；上床检查点保存总时长与转身时长，旧记录按原0.75秒转身＋1.5秒躺下迁移。游戏规则、结局资格及原阶段枚举顺序不变。[verified: RoomEndingTransitionCheckpoint.TimingVersion/IsValid；RoomEndingTransitionClock.Duration；BeachEndingCheckpoint.LieDownDuration/LieDownTurnDuration；DreamEndingController.RestoreCheckpoint]
- 后续看天7秒、监督员对白、再看天36秒（后20秒名单）与结尾1秒淡黑保持原流程。[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs AdvanceBedEnding；../../UnityProject/Assets/MiniGame/Tests/PlayMode/EndingCampaignPlayModeTests.cs DreamSupervisorInterruptRestoresThirtySixSecondsAndFinalTwentySecondCredits]

## 本轮验证

Unity 2022.3.23f1c1隔离工程编译及两轮相关PlayMode共29项全部通过（首轮16项，关联13项）。覆盖自审提交当帧与渐黑期间保留终端画面／镜头、渐显同时移动、全黑暂停、连续音乐与音量、30fps上床逐帧姿态和墙画溶解、旧存档迁移、中途保存恢复、原后续对白／名单流程。[verified: ../../Artifacts/EndingSmooth20261026/playmode.xml test-run；../../Artifacts/EndingSmooth20261026/related-playmode.xml test-run]

Windows x64 Mono包构建成功，0错误、0警告；启动检查存活10秒，日志未见异常。已查看显示器起点、上床中段与天空等GPU渲染图。未重新运行独立EditMode、.NET或IL2CPP；本轮没有修改`src/`核心与宿主库。运行时长与视觉连续性已测，最终听感由用户试玩验收。[verified: ../../Artifacts/EndingSmooth20261026/windows-build.json；../../Artifacts/EndingSmooth20261026/player-startup.json；../../Artifacts/EndingSmooth20261026/player-startup.log；../../Artifacts/EndingSmooth20261026/visuals/]

本地交付：[Windows程序](../../Artifacts/EndingSmooth20261026/Windows/NORM.exe)、[首轮测试](../../Artifacts/EndingSmooth20261026/playmode.xml)、[关联测试](../../Artifacts/EndingSmooth20261026/related-playmode.xml)、[镜头渲染图](../../Artifacts/EndingSmooth20261026/visuals/)。运行程序时保留整个Windows文件夹。[verified: 上述构建与测试产物]

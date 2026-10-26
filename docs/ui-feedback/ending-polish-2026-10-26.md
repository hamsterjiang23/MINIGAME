# 2026-10-26 隐藏结局第二轮试听与镜头调整

用户确认：显示器退镜由2秒改为4秒，天空渐变由约0.72秒改为1.5秒，上床全段约5秒；看天监督员对白中不播放电脑噪音。上一轮的渐黑1秒、全黑1秒、渐显1秒与移动同时开始、BGM为此前70%及海浪音量继续保留。[verified: 本轮三项反馈及用户对异步时长问题的确认]

## 修改

- 看天对白不再创建PCNoise循环，并在对白开始时立即停止电脑噪音；LateUpdate只在可行走阶段且视线朝向印刷显示器时开启它。暂停、读档及切换下一句不会重启看天对白的电脑噪音。[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs LateUpdate/BeginSupervisorDialogue]
- 退镜使用4秒完整运动，渐显覆盖其前1秒。入口约为1秒渐黑＋1秒全黑＋4秒退镜；普通终端退出的时长与初始等待仍沿用原规则。[verified: ../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs EntrySeconds/FadeInSeconds；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs ApplyEntryPose]
- 上床总时长5秒，前1秒转身；后4秒包含约1.44秒后仰看向天空、1.5秒天空渐变与1.06秒越墙。墙画只在视角达到天空方向后淡出，并在越墙前完全消失；末帧继续平滑衔接天空与海浪起伏。[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs LieDownDuration/LieDownTurnDuration；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs SkyRevealStart/SkyRevealEnd/SkyRevealDuration/ApplyWallLieDown；../../UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset LieDownSeconds]
- 入场时序版本升为2，兼容最初1.2秒与上一轮2秒退镜存档，按阶段比例转换到4秒；上床仍使用上轮加入的总时长与转身时长记录迁移。BGM、海浪、监督员配音及后续看天／名单计时未修改。[verified: RoomEndingTransitionClock.Duration；DreamEndingController.RestoreCheckpoint/ApplyPause]

## 验证

隔离Unity 2022.3.23f1c1的25项相关PlayMode全部通过：完整自审结局、入场渐黑与退镜、两代旧存档、上床中途保存恢复、监督员四句对白及读档后电脑噪音均保持停止；原有看向显示器产生噪音的独立回归通过。30fps逐帧上床测试确认5秒总时长、1.5秒天空渐变以及至少30帧可见混合，不提前露出外景、不在末帧翻转。[verified: ../../Artifacts/EndingPolish20261026/playmode.xml test-run]

本轮未改`src/`核心／宿主库，未重新运行.NET、独立EditMode或IL2CPP。Windows Mono构建与启动状态以[构建结果](../../Artifacts/EndingPolish20261026/windows-build.json)及[启动记录](../../Artifacts/EndingPolish20261026/player-startup.json)为准；听感与主观镜头速度留给用户继续验收。[verified: 本轮Git差异；上述结果文件]

本地交付：[Windows程序](../../Artifacts/EndingPolish20261026/Windows/NORM.exe)、[测试结果](../../Artifacts/EndingPolish20261026/playmode.xml)、[渲染图](../../Artifacts/EndingPolish20261026/visuals/)。运行程序时保留整个Windows文件夹。[verified: 本轮构建与测试产物]

## 最终补修：海滩准心

海滩原有独立UI只创建床的睡眠交互图标，没有普通走动的中心点。现补上与房间相同的琥珀色“·”准心：自由走动且未对准床时显示，床可交互时切换为原睡眠图标；暂停、入场、上床、看天及结束时隐藏。[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface/RefreshInterface/ApplyPause/HideForThanks]

7项相关PlayMode全部通过，新增断言覆盖走动、未对准床、暂停／恢复、对准床、上床与看天时的准心状态。最终交付使用[准心修复版Windows程序](../../Artifacts/BeachReticle20261026/Windows/NORM.exe)，测试与构建见[测试报告](../../Artifacts/BeachReticle20261026/playmode.xml)、[构建报告](../../Artifacts/BeachReticle20261026/windows-build.json)。其余声音与镜头配置沿用本页上方已确认版本。[verified: ../../Artifacts/BeachReticle20261026/playmode.xml test-run；../../Artifacts/BeachReticle20261026/windows-build.json]

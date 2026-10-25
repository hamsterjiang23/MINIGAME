# NORM BUG 修复使用说明

本页保留此前修复范围；随后新增的墙体修复、场景灯光与用户调色配置见[墙体与灯光指南](../wall-lighting/guide.md)。`[verified: ../wall-lighting/verification.json]`

本轮处理逻辑、UI、音效触发和画面颗粒；灯光调整与新梦境素材继续暂缓。`[verified: 本轮用户选择“先修其余问题，灯光和新素材暂缓”；implementation-notes.md Questions for review]`

## 门口晨检与监督员

- 监督员使用文档提供的站姿立绘，常驻门外；按[最新门口参考图](supervisor-placement-20261025.md)距门内对齐点 1.5 米、左移 0.22 米、上移 0.25 米。开关门时先避让，再转动门扇，安全距离按门和立绘尺寸校正；工作与暂停期间继续保留，由真实门体和门框遮挡。`[verified: SupervisorDoorwayView.cs Bind/PlaceAtDoorway/RefreshDoorClearance；HingedDoor.cs Update]`
- 监督员对白显示在画面下方的白色细框内，称谓为“监督员”，不显示“【开发占位】”标记。`[verified: MiniGameScreen.SetDialogueText/Render/IsSupervisorCue/WorldCueText/AddWhiteFrame]`
- 对准门按 E 进入门口观察，开门后停在门内对齐位置，不再额外向走廊推进；观察结束按 E 返回房间。`[verified: CombinedInteractionSetup.ConfigureDoorCamera；DoorViewCamera.LateUpdate/FinishEntry；ApartmentPlayer.Update DoorView 分支]`
- 扫描声只在进入新的晨检演出时播放一次；选择终端对象、查看线索、暂停恢复或重新绑定会话不再额外触发扫描声。`[verified: MiniGameAudioDirector.Present beganMorningScan 条件及 Bind]`

## 终端对白与净化要求

- 长对白完成播放后，左侧记录保留整段全文及段落，不只显示最后一页；右侧单行应答命令布局保持现有形式。`[verified: TerminalTranscriptView.Render entry.Text/playing 分支；TerminalView.AddResponse]`
- 将鼠标放在左侧对白记录区域，用滚轮上下查阅。上翻后保留阅读位置，新内容不强制拉回底部；回到底部后自动跟随最新内容。`[verified: TerminalTranscriptView 构造器 ScrollRect；TerminalTranscriptView.Render follow/offset 分支]`
- 对象目录和复核页都显示“净化要求：已执行数/当天要求”。已执行数量来自当日真实净化记录；治疗不计入净化，超额时保留实际值，例如 `2/1`。原“净化配额”仍表示剩余额度。`[verified: TerminalView.Render directoryRequirement/reviewRequirement/quota；GameSession.GetPlayerView；PlayerView.PurificationsToday/PurificationRequirement]`
- 每天开机一次；中途退出终端时物理屏幕保持实时 UI，再次进入接续进度。当天最后一位对象的处置反应完成后，才播放关机会话并熄屏。`[verified: GameRuntime.LeaveTerminal/CompleteCue；MiniGameScreen.Render terminal.SetActive；PresentationEffects.SetSurfaceGlow；existing-audit.md 物理 CRT 条目]`

## 暂停与操作手册

- 按 Esc 打开暂停菜单，选择“操作手册”查看房间、终端和系统操作。底部常驻的 WASD、E、F5、F9 操作说明已移入手册，房间只保留当前物件的交互提示。`[verified: MiniGameScreen.Build labels/actions；MiniGameScreen.Render worldHelp；PauseMenuView.BuildManual]`
- 手册内按 Esc、Enter 或点击“返回暂停菜单”，先回到暂停菜单；再按 Esc 或选择“继续检查”才恢复游戏。阅读手册期间不执行 F5/F9，隐藏的菜单项也不能提交操作。`[verified: MiniGameBootstrap.HandleEscape/Update；PauseMenuView.ShowManual/TryCloseManual/Build]`
- 房间使用 WASD 移动、鼠标转动视角、E 与准星物件交互；鼠标左键使用电脑或阅读海报、物品。终端使用上下键选项、Enter 确认、空格推进对白、数字 1–4 确认应答。F5 保存，F9 读档；没有 F2/F3 功能。`[verified: ApartmentPlayer.Update/InteractSecondaryFocused；MiniGameScreen.DialogueKeys/HandleTerminalKey；MiniGameBootstrap.Update；PauseMenuView.BuildManual]`
- 暂停菜单采用平面琥珀命令条和反白选中反馈，保留继续、保存、读取、重新开始、退出游戏功能。暂停背景保留原方向修正，不转动世界相机。`[verified: PauseMenuView.Build；PauseMenuButton.DoStateTransition；MiniGameScreen.Build labels/actions；PauseMenuBackdrop.CaptureFrame image.uvRect]`

## 画面与可调整项

- 游戏目标帧率为 30 FPS，垂直同步关闭；这是运行设置，不是对所有硬件实际帧率的保证。`[verified: PlayerRuntimeSettings.FrameLimit/Apply]` `[opinion]`
- 顶部状态、提示、对白和暂停文字统一使用终端暖黄配色，交互提示与对白边框为白色；海报阅读保留白框，已去掉原底部横线和操作文字。`[verified: MiniGameScreen.Ink/MakeText/AddWhiteFrame；PauseMenuView.Build；PropNarrationView.Build/FrameEdge]`
- 房间增加轻微、随帧变化的单色胶片颗粒，主场景配置为 `0.05`。这是独立于静态量化抖动的画面效果，不修改灯光；Overlay 终端 UI 不经过房间颗粒处理。`[verified: RoomLightingReferenceRetro.asset filmGrainStrength；RetroScreen.shader FilmGrain/frag；RetroScreenEffect.OnRenderImage；grain-notes.md]`
- 如需调节颗粒，在 Project 中选中 [RoomLightingReferenceRetro.asset](../../UnityProject/Assets/MiniGame/Resources/RoomLightingReferenceRetro.asset)，在 Inspector 的“胶片颗粒 → 颗粒强度”调整；设为 0 关闭。灯光与新梦境素材不在本轮调整范围内。`[verified: RetroScreenProfileEditor.OnInspectorGUI；RetroScreenProfile.FilmGrainStrength；implementation-notes.md Questions for review]`

## 验证范围

监督员常驻与上移的追加版本：[MiniGame.exe](../../Builds/SupervisorPresence/MiniGame.exe)、[实景图](previews/supervisor-always-outside-raised.png)及[验证记录](supervisor-presence-verification.json)。下方保留此前冻结版的完整验证范围。`[verified: supervisor-presence-verification.json build、tests、preview]`

Unity 导入、PlayMode 和构建使用隔离工程 `Artifacts/Unity/OfflineProject`。内容更新后已重新同步并逐文件校验受测快照；327 项 .NET、47 项相关 PlayMode、补字库后的 28 项 EditMode 均通过，Windows x64 Mono 非开发版构建为 0 错误、0 警告。这是本轮选定范围，不代表全部 Unity 历史测试均已运行。`[verified: verification.json dotnet/unity/sourceSnapshot；reports/final-playmode.xml；reports/font-editmode.xml；reports/windows-build.json]`

运行包按 1293 个文件的受测快照冻结；同期对白导入任务在冻结后继续修改了 4 个主工程文件，具体路径记在 `sourceSnapshot.primaryFilesChangedSinceSnapshot`，这些后续改动未纳入本包。本轮门外监督员及 UI 源码与受测副本一致。`[verified: verification.json sourceSnapshot；reports/tested-source-snapshot.json]`

实际 Player 通过正式开场→一次教学净化→一次睡眠→门口晨检→暂停手册→Day1 目录；监督员位于门外 0.9 米，脚底与站立玩家地面相同。另测 1280×720／1024×768 暂停方向与真实退出，以及终端中途退出持续亮屏、再次进入和最后自动关机。`[verified: reports/campaign-player.json；reports/pause-player.json；reports/terminal-player.json]`

运行 [MiniGame.exe](../../Builds/NormBugFix/MiniGame.exe)；分发时保留整个 `NormBugFix` 文件夹，完整压缩包与 SHA-256 见 [验证记录](verification.json)。默认双击进入正式开场，验证探针只有指定命令行参数时启用。`[verified: AmberTerminalValidation.BuildWindowsNormBugFix；NormBugPlayerProbe.Install；verification.json package]`

截图：[门外监督员](previews/supervisor-outside.png)、[暂停菜单](previews/pause.png)、[操作手册](previews/manual.png)、[正式目录](previews/terminal-directory.png)。逐条状态见 [19 条 BUG 台账](issues.json)，实现边界见 [工作记录](implementation-notes.md)。`[verified: reports/campaign-player.json captures；issues.json statusCounts]`

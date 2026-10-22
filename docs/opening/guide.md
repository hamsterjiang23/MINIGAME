# 监管者开场与Day0

正式游戏启动后，在开场终端中央显示用户提供的透明NORM标志，保留原比例与透明像素，不添加底板。点击屏幕后，镜头用2.2秒连续拉远，同一标志缩回终端左侧监控窗口；停稳后恢复监管者画面并开放Day0教学操作。Day1起继续使用各人物的动态监控室。原图与SHA256见[来源记录](../../Art/Branding/norm-logo-source.json)，最新预览与验证见[NORM开场验证](../norm-opening/verification.json)。`[verified: TerminalView.SetOpeningLogo/FitOpeningLogo；MiniGameScreen.BeginDayZeroOpening/EndDayZeroOpening；PresentationEffects.DayZeroOpening]`

教学保留五段介绍、唯一净化处置和两段反应。每个时点仅“继续”或“净化”一个按钮可以操作，底部命令条只显示提示；开场点击不推进教学。F2/F3保持移除。`[verified: DayZeroOpeningPlayModeTests.NormLogoPullsBackIntoTheTerminalThenOneButtonTutorialAndOneSleepStartDayOne；TerminalView.Render]`

反应读完后自动关机，镜头退回电脑前的Computer approach站立位置，聚光灯照床；不再回到远处的开场出生点。家具、电脑、海报和床保留原有碰撞enabled/isTrigger状态，Day0不再例外关闭碰撞。睡一次，闭眼淡出后回到安全起床点并进入Day1晨检，同时保留玩家朝向、俯仰角和视野。Day0没有墙面梦境，也不需要第二次睡眠；后续天数仍按实际进入终端的位置返回。`[verified: PresentationEffects.RememberRoomPose/CameraTransition；BedtimeGuidance.SetTargets；ApartmentPlayer.RestoreDayZeroWakePose；day0-return-verification.json]`

暂停冻结拉远、监控和关机。未完成开场时读档，回到监管者近景等待重新点击；已开始教学的检查点恢复当前未完成句，净化保持一次结算。Day0闭眼期间的存档从该段起点重播。`[verified: DayZeroOpeningPlayModeTests.PausedPullbackFreezesAndUnfinishedOpeningRestoresAtItsWaitingStart；CampaignPlayModeTests.TutorialResumesTheCurrentPageWithoutReplayingCompletedLinesAndPurifiesOnce；CampaignRuntimeTests.DayZeroOpeningUsesReviewCueAndRestoresWithoutDuplicatingSelectionOrDisposition]`

监管者图原件按原样导入为 [Day0Supervisor.jpg](../../UnityProject/Assets/MiniGame/Resources/MiniGame/Day0Supervisor.jpg)，SHA256为 `129997aaea6f3a33f724b6f253bf6bd544cc98a29eb77b1fbdf67fdcd6a6bffa`。界面继续使用PMS v0.8.12资源包。`[verified: 本轮Get-FileHash输出；TerminalView构造器]`

验证结果和运行路径见 [verification.json](verification.json)，实现边界见 [implementation-notes](implementation-notes.md)。开发运行版使用显式参数 `--day0-opening-smoke` 执行真实主场景的开场、原生UI鼠标命中、教学及睡眠检查；正常启动不加载此检查器。`[verified: DayZeroOpeningPlayerSmoke.Install/Tick]`

可查看[60fps连续拉远演示](previews/opening-pullback-60fps.mp4)、[完整终端](previews/terminal.png)和[床聚光灯](previews/bed-only.png)。终端采用连续纹理采样；新录像使用预分配内存与异步GPU回读，拉远结束后才编码、写盘。134个真实采集帧按60fps编码，首尾各加0.75秒静止停留。房间截图来自实际房间相机。平滑改造结果见[验证记录](smoothness-verification.json)。`[verified: OpeningMotionProbe.Capture/Complete；smoothness-verification.json capture/video]`

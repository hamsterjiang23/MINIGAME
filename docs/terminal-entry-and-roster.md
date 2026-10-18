# 电脑落座与每日自由接待

在融合房间瞄准电脑，按鼠标左键或 E 进入终端。第一人称角色用视线下降、身体落座与短暂停顿表现坐下，再对齐屏幕向前推进；实体屏幕填满视野后转入黑场，随后显示终端 UI。退出继续使用原有回位演出。[verified: ../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs Update；../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs SeatedEntry/CameraTransition]

当前入场总长约3秒：落座0.65秒、对齐0.65秒、推进1.15秒、淡黑0.20秒、UI显现0.35秒。角色没有外显人物模型，落座由第一人称身体和镜头完成；没有新增第三人称骨骼动画。[verified: ../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs SeatedEntry 常量；../UnityProject/Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs ConfigureOpenScene 创建 Player]

Day1–3可在名册任意选择当天待审角色。单击预览、双击或点击“进入审查”开始会谈；也可用上下键和 Enter。已经进入的会谈需完成后才能换人，当天已完成或已退场者不可再次接待。Day0仍只接待教学对象。[verified: ../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs RenderRoster/HandleDirectoryKey；../src/MiniGame.Core/Runtime/GameSession.cs CanReceiveToday/SelectPatientForToday]

名册显示顺序与实际接待顺序分开：默认高亮第一位待审角色，但其他待审项同样可进入。已接受的角色选择写入命令历史，存档恢复不重复创建会谈或结算处置；旧V1/V2存档和规则指纹保持兼容。[verified: ../src/MiniGame.Core/State/CommandRecord.cs From/ToCommand；../src/MiniGame.Core.Tests/PatientSelectionTests.cs UnselectedReservationsAndLegacyJsonRemainCompatibleButSelectionTamperingFailsReplay；../src/MiniGame.Presentation.Tests/FreePatientSelectionRuntimeTests.cs OldV1AndV2CheckpointsWithoutExplicitSelectionFieldsStillRestore]

暂停冻结落座和推镜。恢复入场检查点会从原来的站立姿态重播入场；恢复已经打开的终端会直接落座。退出或恢复房间时还原玩家位置、旋转、胶囊高度及控制器状态。屏幕终点按模型尺寸与当前画面比例更新，途中调整窗口也会重新适配。[verified: ../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs Bind/UpdateScreenDestination/SeatedEntry；../UnityProject/Assets/MiniGame/Runtime/TerminalSeating.cs Capture/SetPose/Restore]

关键帧与录制片段在 `Artifacts/TerminalSeating/`；录制片段由实际Unity测试截图按进度时间编码，约10次采样/秒。验证结果见[电脑与名册验收记录](terminal-entry-verification.json)。[verified: ../UnityProject/Assets/MiniGame/Tests/PlayMode/SeatedTerminalEntryPlayModeTests.cs MotionTimeline；../Artifacts/TerminalSeating/Visuals/terminal-motion-timeline.json frames]

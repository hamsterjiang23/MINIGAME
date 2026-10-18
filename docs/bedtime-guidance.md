# 睡前床位聚光灯

当天会谈结束后退出终端，房间用约1.1秒压暗，只留下床上方照下的暖色聚光灯；30只小飞虫在光柱中环绕、急转和振翅。门和电脑不再显示“暂不可用”，靠近床时仍显示按E休息。梦境期间光柱熄灭，梦后仅可睡觉时重新点亮，次日恢复原房间灯光。[verified: ../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs SetTargets/LateUpdate；../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs RefreshFocus/InteractFocused；../UnityProject/Assets/MiniGame/Runtime/BedtimeAtmosphere.cs]

## 预览与触发

- 正常玩法：完成当天全部对象后，返回对象清单，再返回房间；Day0教学和Day1–3均使用相同阶段条件。[verified: ../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs IsSleepOnly/SetTargets]
- 快速预览：非Play状态选择 **MiniGame/Main Room/Preview bedtime lighting**。该入口通过合法教学命令建立临时睡前状态，镜头朝向床，不写玩家存档；离开Play后仍从正常Day0开局。[verified: ../UnityProject/Assets/MiniGame/Editor/BedtimePreview.cs Open/OnPlayModeChanged]
- 重装房间玩法：原有 **MiniGame/Main Room/Setup gameplay** 同时重建灯位和绑定。效果跟随暂停，夜晚读档直接恢复正确光照，次日或关闭组件恢复原值。[verified: ../UnityProject/Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs ConfigureOpenScene；../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs Bind/OnDisable/RestoreRoom]

## 表现与控制

- 灯位：床中心正上方、房间高度2.94米处，向下的Spot Light，强度8、范围3.6米、外角56°、内角36°。仅照射 `BedtimeLit` 第28层上的床架、床垫、枕头和被子，房间其他物体保持黑暗。[verified: ../UnityProject/Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs ConfigureOpenScene/EnsureBedtimeLayer]
- 房间隔离：同时淡出其他灯、环境光、反射和窗户／灯泡／屏幕自发光；使用运行时材质属性块，不修改源材质。恢复时还原各材质槽属性、环境和相机深度设置。[verified: ../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs CaptureRoom/ApplyRoom/RestoreRoom]
- 光柱：单个锥体体积、24步柔和密度采样，使用相机深度遮挡；`_Strength` 为可见度、`_Radius` 为床面半径、`_Height` 为灯至床距离、`_VisualTime` 为显式秒时钟。最终密度系数0.18。[verified: ../UnityProject/Assets/MiniGame/Resources/BedtimeBeam.shader frag；../UnityProject/Assets/MiniGame/Runtime/BedtimeAtmosphere.cs SetVisualState]
- 飞虫：30只，共用一个动态网格；每只由躯干、双翼与六腿组成，尺寸0.014–0.035米，具有不同相位与速度。暂停或零强度时冻结时钟；不使用发光球粒子或直线下落的尘埃轨迹。[verified: ../UnityProject/Assets/MiniGame/Runtime/BedtimeAtmosphere.cs BuildInsects/UpdateInsects/FlightPosition]
- 灯光和虫群只负责表现；能够操作什么仍由游戏阶段决定。进出终端的镜头曲线、开机演出和睡眠阶段推进保持原逻辑。[verified: ../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs；bedtime-verification.json preservedTransitions]

## 验收与修订

初版已做到床外黑暗，但光柱较淡；最终将密度从0.075提高至0.18，聚光灯强度从5.5提高至8。审查同时发现旧夜间调暗与新控制器会争用顶灯，现由新控制器统一管理，并增加归档停留后退出的亮度连续性检查。[verified: ../Artifacts/Bedtime/Visuals-v1/bedtime-spotlight.png；../Artifacts/Bedtime/Visuals/bedtime-spotlight.png；../UnityProject/Assets/MiniGame/Tests/PlayMode/BedtimeGuidancePlayModeTests.cs]

技术检查：两项睡前用例及23项项目PlayMode回归通过，覆盖真实床交互、暂停、夜晚恢复、梦后引导、晨间还原、旧终端过渡与三种结局。[verified: ../Artifacts/Bedtime/bedtime-playmode-final.xml test-run；../Artifacts/Bedtime/game-playmode.xml test-run]

画面检查：白天／睡前／梦后／次日截图及16帧飞虫记录已检查；床外黑暗、向下光柱和飞虫位移达到本轮目标。用户观感验收待反馈；未设定或测量独立GPU耗时预算。[verified: ../Artifacts/Bedtime/Visuals；bedtime-verification.json visuals] [opinion]

动态记录：[睡前光柱与飞虫](../Artifacts/Bedtime/bedtime-motion.mp4)。完整技术结果、构建范围与保留项见[验证记录](bedtime-verification.json)。[verified: ../Artifacts/Bedtime/bedtime-motion.mp4；bedtime-verification.json]

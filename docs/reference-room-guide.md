# 参考房间、海报与房门

默认主场景使用员工宿舍参考文件第133帧的房间和家具位置，净空9.70078×4.41176×3.12066米。窗外两个原水泥物体保留原位置与尺寸；水平视角61.9275°，相机按画面比例换算垂直视角。[verified: ../Art/MainRoom/reference/verification.json；../UnityProject/Assets/MiniGame/Runtime/ReferenceCameraProjection.cs]

仍双击根目录 `Open-Unity.cmd` 启动。美术重导菜单为 `MiniGame/Room Art/Import and open combined room`，重建玩法为 `MiniGame/Main Room/Setup gameplay`；它们识别参考标记，生成碰撞、门、海报、走廊、玩家和电脑镜头。[verified: ../Open-Unity.cmd；../UnityProject/Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs ConfigureOpenScene]

## 操作与配置

- 门：在 `MainRoomCombined` 瞄准门按 E，镜头先定位到门内对齐点并正对门面，视线垂直门面；等待门扇完全打开后，沿门面向外的法线直线推进，全程保持正对方向，不再侧转。门外停留时按 E 返回本次交互前的位置和朝向，返回完成后才关闭门扇；玩家身体始终留在房内。[verified: ../UnityProject/Assets/MiniGame/Editor/CombinedInteractionSetup.cs ConfigureDoorCamera；../UnityProject/Assets/MiniGame/Runtime/DoorViewCamera.cs LateUpdate/RestoreRoomPose；../src/MiniGame.Presentation/GameRuntime.cs ExitDoorView/CompleteCue]
- 门扇配置：选择 `MainRoom Gameplay/Entrance hinge`，在 `HingedDoor` 设置 `Enable Toggle`、`Open Angle` 和 `Animation Seconds`；默认启用、60°、0.6秒。正角度默认向外开，门板、把手、碰撞同时移动。[verified: ../UnityProject/Assets/MiniGame/Runtime/HingedDoor.cs Configure/SetAngle；../UnityProject/Assets/MiniGame/Editor/ReferenceRoomGameplayBuilder.cs]
- 门外镜头配置：选择 `Main Camera` 的 `DoorViewCamera`，调整“门内对齐点”“门外观察点”两个Transform引用；生成目标名为 `Door camera alignment`、`Door camera outside`，可在场景中调整；两个点须保持同向正对门面，连线平行于门面法线。“定位时长（秒）”“推进时长（秒）”“返回时长（秒）”默认分别为0.65、0.9、1.1。已有Combined场景可用 `MiniGame/Main Room/Update door camera and interaction focus` 补装组件；已有目标位置和交互距离会保留。[verified: ../UnityProject/Assets/MiniGame/Runtime/DoorViewCamera.cs L13-L17；../UnityProject/Assets/MiniGame/Editor/CombinedInteractionSetup.cs InstallInOpenScene/ConfigureDoorCamera/Pose]
- 晨检：正式日首次开门并完成镜头推进后进入原晨检、简报，结束后停留门外，按 E 返回；普通门外观察不推进日期。睡前自动关闭并锁定。禁用物理开关后保持关闭，但晨检入口仍保留。[verified: ../src/MiniGame.Presentation/GameRuntime.cs EnterDoorView/CompleteCue；../UnityProject/Assets/MiniGame/Runtime/DoorViewCamera.cs LateUpdate；../UnityProject/Assets/MiniGame/Runtime/HingedDoor.cs TryInteract]
- 悬停反馈：门和床不描边、不变色；其他当前可交互物显示白色描边，电脑描边指向CRT机身，海报显示白色边框。描边不改原材质和材质属性块；不透明物体会遮挡描边。[verified: ../UnityProject/Assets/MiniGame/Runtime/WorldInteraction.cs Highlight；../UnityProject/Assets/MiniGame/Runtime/InteractionFocusOutline.cs PrepareCamera；../UnityProject/Assets/MiniGame/Resources/InteractionFocusOutline.shader VisibleOutline；../UnityProject/Assets/MiniGame/Runtime/ReadableProp.cs RebuildOutline；../UnityProject/Assets/MiniGame/Editor/CombinedInteractionSetup.cs InstallInOpenScene]
- 交互距离：打开 `MiniGame → 交互设置`，在中文面板逐个调整Combined场景中门、床、电脑和海报的“交互距离（米）”，可定位物体、撤销和重做，点击“保存场景”仅保存MainRoomCombined。播放中禁止编辑和保存，先点“退出播放后编辑”；未加载Combined时可从面板打开，打开前会询问是否保存当前未保存场景。面板直接修改对应 `WorldInteraction` 或 `ReadableProp` 的原字段，也可在Inspector调整；默认分别为3.2米和2米。距离按玩家镜头到瞄准射线命中表面计算，悬停和实际按键使用同一距离判断，只有射线首先命中的碰撞体可成为目标，不会穿过墙壁选择后方物体；门和床也受距离限制。[verified: ../UnityProject/Assets/MiniGame/Editor/InteractionSettingsWindow.cs OnGUI/GetTargets/ApplyDistance；../UnityProject/Assets/MiniGame/Runtime/WorldInteraction.cs InteractionDistance；../UnityProject/Assets/MiniGame/Runtime/ReadableProp.cs InteractionDistance；../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs RefreshFocus/InteractFocused/InteractSecondaryFocused]
- 海报：在配置距离内瞄准显示白色边框，用鼠标左键查看，E不触发。字幕默认4秒，重复点击刷新，暂停冻结计时；进入电脑或剧情演出时清除。睡前只允许床交互时停止响应。[verified: ../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs RefreshFocus/InteractSecondaryFocused；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs ShowRoomCaption/Update/Render]
- 海报配置：选择 `Readable poster beach` 或 `Readable poster instrument`，在 `ReadableProp` 配置ID、文案、交互距离、字幕时长和Renderer。正面贴图以Word截图为参考还原，来源和完整生成提示见[海报清单](../Art/Interactions/manifest.json)。[verified: ../UnityProject/Assets/MiniGame/Runtime/ReadableProp.cs Configure；../Art/Interactions/manifest.json]
- 门外：真实3D的6×2.2米连廊，有采光窗、邻门、房号、消防门、配给柜和“禁止离岗”标志。当前镜头可以推进到门外，玩家身体仍受门槛碰撞限制，不提供走廊自由移动。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferenceCorridorBuilder.cs Build；../UnityProject/Assets/MiniGame/Runtime/DoorViewCamera.cs ApplyPose；../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs Update]

门的目标开关状态和门外观察状态随宿主存档保存；旧V1/V2存档缺字段时按关闭、未观察处理。现有宿主存档没有玩家空间位置和朝向：返回姿态由镜头组件在进入门外时临时捕获，同一会话在该姿态仍保留时读档可继续返回原位；重启后加载门外存档则使用新会话当前可用的房内姿态作为返回点，不能恢复上次会话的精确站位。若存档位于返回镜头途中，读档直接恢复房内并关闭门扇，不重播返回过程。[verified: ../src/MiniGame.Presentation/GameRuntime.cs L365-L384、L392-L431；../UnityProject/Assets/MiniGame/Runtime/DoorViewCamera.cs PrepareForRuntimeChange/Bind/RememberRoomPose；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs InstallRuntime]

门框使用连续U形网格，内衬与门洞墙面错开25毫米，前后饰面突出墙面35毫米，底端埋入地面10毫米；门扇留出实际开合间隙，避免转动视角时共面闪烁。修复的几何、开关门及八视角检查见[门框修复验收](door-frame-fix-verification.json)。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferenceDoorFrameBuilder.cs；../UnityProject/Assets/MiniGame/Tests/PlayMode/DoorFrameRenderingPlayModeTests.cs]

## 制作与验证

本次门外镜头、白色描边和交互距离的测试、构建与实景检查见[Combined交互验证](combined-interactions-verification.json)。`[verified: combined-interactions-verification.json playMode/build/visual]`

参考导出脚本为 `Art/MainRoom/reference/build_reference_layout.py`，布局和原物件对照在同目录 `layout.json`、`geometry-report.json`。原始Blender和FBX不被覆写。生成贴图使用内置imagegen；[海报](../Art/Interactions/manifest.json)与[连廊表面](../Art/Exterior/manifest.json)清单保留提示、来源和SHA256。[verified: ../Art/MainRoom/reference/README.md；../Art/Interactions/manifest.json；../Art/Exterior/manifest.json]

既有参考房间验收见[参考房间验收](reference-room-verification.json)，图形截图和运行日志在 `Artifacts/ReferenceRoom/`；该历史记录不代表本次门外镜头、白色描边与交互距离改动已通过验证。原A布局融合版保留在 `Art/MainRoom/combined/` 作为历史美术来源。[verified: ../Art/MainRoom/combined/README.md；../Art/MainRoom/reference/README.md] [opinion]

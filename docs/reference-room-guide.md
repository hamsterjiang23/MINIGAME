# 参考房间、海报与房门

默认主场景使用员工宿舍参考文件第133帧的房间和家具位置，净空9.70078×4.41176×3.12066米。窗外两个原水泥物体保留原位置与尺寸；水平视角61.9275°，相机按画面比例换算垂直视角。[verified: ../Art/MainRoom/reference/verification.json；../UnityProject/Assets/MiniGame/Runtime/ReferenceCameraProjection.cs]

仍双击根目录 `Open-Unity.cmd` 启动。美术重导菜单为 `MiniGame/Room Art/Import and open combined room`，重建玩法为 `MiniGame/Main Room/Setup gameplay`；它们识别参考标记，生成碰撞、门、海报、走廊、玩家和电脑镜头。[verified: ../Open-Unity.cmd；../UnityProject/Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs ConfigureOpenScene]

## 操作与配置

- 门：瞄准按 E 开关。选择 `MainRoom Gameplay/Entrance hinge`，在 `HingedDoor` 设置 `Enable Toggle`、`Open Angle` 和 `Animation Seconds`；默认启用、60°、0.6秒。正角度默认向外开，门板、把手、碰撞同时移动。[verified: ../UnityProject/Assets/MiniGame/Runtime/HingedDoor.cs；../UnityProject/Assets/MiniGame/Editor/ReferenceRoomGameplayBuilder.cs]
- 晨检：正式日首次开门完成后进入原晨检、简报；普通开关不推进日期。睡前自动关闭并锁定。禁用物理开关后保持关闭，但晨检入口仍保留。[verified: ../src/MiniGame.Presentation/GameRuntime.cs ToggleRoomDoor/CompleteCue；../UnityProject/Assets/MiniGame/Runtime/HingedDoor.cs TryInteract]
- 海报：2米内瞄准显示描边，用鼠标左键查看，E不触发。字幕默认4秒，重复点击刷新，暂停冻结计时；进入电脑或剧情演出时清除。睡前只允许床交互时停止响应。[verified: ../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs ShowRoomCaption/Update/Render]
- 海报配置：选择 `Readable poster beach` 或 `Readable poster instrument`，在 `ReadableProp` 配置ID、文案、距离、时长和Renderer。正面贴图以Word截图为参考还原，来源和完整生成提示见[海报清单](../Art/Interactions/manifest.json)。[verified: ../UnityProject/Assets/MiniGame/Runtime/ReadableProp.cs；../Art/Interactions/manifest.json]
- 门外：真实3D的6×2.2米连廊，有采光窗、邻门、房号、消防门、配给柜和“禁止离岗”标志。玩家始终不能跨出门槛；门槛碰撞不阻挡瞄准开关门。出门与走廊探索玩法留待后续设计。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferenceCorridorBuilder.cs Build；用户本轮补充]

门的目标开关状态随宿主存档保存；读档直接恢复目标姿态，旧V1/V2存档缺字段时按关闭处理。字幕不持久化；现有Excel/Yarn正文、信任、配额和结局规则不变。[verified: ../src/MiniGame.Presentation/GameRuntime.cs Save/Restore；../src/MiniGame.Presentation.Tests/RoomDoorRuntimeTests.cs]

门框使用连续U形网格，内衬与门洞墙面错开25毫米，前后饰面突出墙面35毫米，底端埋入地面10毫米；门扇留出实际开合间隙，避免转动视角时共面闪烁。修复的几何、开关门及八视角检查见[门框修复验收](door-frame-fix-verification.json)。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferenceDoorFrameBuilder.cs；../UnityProject/Assets/MiniGame/Tests/PlayMode/DoorFrameRenderingPlayModeTests.cs]

## 制作与验证

参考导出脚本为 `Art/MainRoom/reference/build_reference_layout.py`，布局和原物件对照在同目录 `layout.json`、`geometry-report.json`。原始Blender和FBX不被覆写。生成贴图使用内置imagegen；[海报](../Art/Interactions/manifest.json)与[连廊表面](../Art/Exterior/manifest.json)清单保留提示、来源和SHA256。[verified: ../Art/MainRoom/reference/README.md；../Art/Interactions/manifest.json；../Art/Exterior/manifest.json]

实际验收见[参考房间验收](reference-room-verification.json)，图形截图和运行日志在 `Artifacts/ReferenceRoom/`。原A布局融合版保留在 `Art/MainRoom/combined/` 作为历史美术来源。[verified: ../Art/MainRoom/combined/README.md；../Art/MainRoom/reference/README.md]

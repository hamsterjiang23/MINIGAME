# 监督员门口站位与避让

按用户 2026-10-25 提供的门口参考图调整构图：监督员距门内对齐点 1.5 米、沿门口视角向左 0.22 米、脚底高于站立地面 0.25 米，保留立绘 1.8 米的参考画布高度。神父继续使用原来的结局站位。`[verified: SupervisorDoorwayView.cs PlaceAtDoorway/ApplyDoorOffset]`

在 `SupervisorDoorwayView` 的 Inspector 中可调整前后、左右、高度、最小避让距离和安全间距。开关门时先向左避让，避让完成后 `HingedDoor` 才开始旋转；完全打开后平滑回位，返回房间时提前避让。最小避让量为 0.8 米，并按门及门把手绕铰链旋转的最大半径、当前立绘完整矩形的宽度自动增大，预留 0.08 米安全间距。`[verified: SupervisorDoorwayView.cs RefreshDoorClearance/HasDoorClearance/LateUpdate; HingedDoor.cs HorizontalSweepRadius/Update]`

验收入口：`SupervisorPlacementPlayModeTests.RealDoorNeverIntersectsPortraitDuringRepeatedOpeningAndClosing`。使用真实 `MainRoomCombined` 场景，两轮开关门，六种监督员表情，暂停中途避让；每帧使用门的真实 BoxCollider 与厚 0.02 米的完整立绘矩形进行独立相交检查，并核对门开始关闭前的等待和人物回位稳定性。设置 `MINIGAME_PLACEMENT_CAPTURE_DIR` 可输出高分辨率截图、预览帧与 `verification.json`。该测试使用临时交互配置允许重复开门，不修改正式游戏的晨检后锁门设置。`[verified: SupervisorPlacementPlayModeTests.cs]`

2026-10-25：Unity 2022.3.23f1c1 实际 PlayMode 验证通过，监督员门口、实景站位、字幕与完整对白回归共 16/16；两轮门动画共检查 224 帧，未发现门扇与完整立绘矩形相交。截图按 16:9 的实际投影生成，预览关闭像素滤镜以便观察，不更改正式游戏画面配置。证据：[测试结果](../../Artifacts/Unity/SupervisorPlacement20261025/playmode-results.xml)、[相交检查](../../Artifacts/Unity/SupervisorPlacement20261025/clearance-verification.json)、[站位截图](../../Artifacts/Unity/SupervisorPlacement20261025/supervisor-position.png)、[开关门预览](../../Artifacts/Unity/SupervisorPlacement20261025/door-clearance-preview.mp4)。`[verified: linked XML test-run passed=16/failed=0; linked JSON checkedFrames=224/result=passed]`

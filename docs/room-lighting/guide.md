# Combined 房间灯光调参

在 Unity **2022.3** 打开 `MainRoomCombined`，选择 **MiniGame → 画面 → 房间灯光**。面板和配置 Inspector 使用同一组中文字段；“定位配置”可以选中当前使用的配置资产。`[verified: RoomLightingWindow.Open、Resolve、OnGUI；RoomLightingProfileEditor.DrawSettings]`

## 调整与保存

- 修改字段后，已加载房间会立即更新，支持 Undo/Redo；点击“保存配置”保存资产。在 Play Mode 中保存的配置退出播放后仍会保留。`[verified: RoomLightingWindow.OnGUI、OnUndo；RoomLightingProfileEditor.RefreshRooms]`
- “临时关闭分层”只显示连续灯光，方便判断灯的位置、范围、阴影是否合理；不会改动分层开关与档数。关闭面板时恢复正常显示。`[verified: RoomLightingWindow.OnDisable、OnGUI；RoomLightingController.PreviewUnbanded、ApplySurface]`
- 应通过面板调整主灯，不要只改 Hierarchy 中 `CeilingLight` 的数值；下一次应用配置时会按配置重设主灯。`[verified: RoomLightingController.ApplyNow]`
- 未安装配置时，退出 Play Mode，打开 Combined 后选择 **MiniGame → 画面 → 为当前主场景安装房间灯光**。此入口在现有场景接入组件与地毯，不重新生成房间。`[verified: RoomLightingSetup.InstallInOpenScene、EnsureInScene]`

## 策划可调参数

- **吊灯聚光灯**：灯罩开口偏移、颜色、强度、照射距离、内外圈角度、阴影强度与两种阴影偏移、灯泡自发光。光源始终朝下，并限制在灯罩开口下方，避免重新放回灯罩内部。`[verified: RoomLightingProfile.ValidateValues；RoomLightingController.ApplyNow]`
- **环境与窗边**：暗部环境色、环境反射、窗边冷光颜色／强度／范围、电脑微光强度。大范围 Fill 与 Exterior 灯由控制器关闭。`[verified: RoomLightingProfile 环境与窗边字段；RoomLightingController.ApplyNow]`
- **可选照明分层**：默认关闭，正式画面使用连续的聚光灯衰减。只有明确需要风格化时才开启；预留 8 档、强度 0.4、细碎程度 0。分层强度 0 为连续灯光、1 为完全分档；木纹、海报颜色和自发光不参与分档。`[verified: RoomArtStandard.shader LightingRoomStandard、surf；RoomLightingProfile lightingBandsEnabled/lightingBandsStrength/lightingDither]`
- **木地板**：木纹亮度、木纹对比度、地板光滑度；保留现有人字木纹、法线与 UV。`[verified: RoomLightingController.ApplySurface；RoomLightingSetup.EnsureInScene floor 材质引用]`
- **地面地毯**：显隐、相对地板中心位置、长宽、旋转角度与色调。默认长宽 3.4×2 米，离地至少 0.008 米；独立地毯接收阴影且不增加碰撞。`[verified: RoomLightingProfile.rugSize、ValidateValues；RoomRugSetup.Ensure]`

默认保持分层关闭，先调整“灯下亮、未照到的墙面暗”和家具遮挡，再微调地板与地毯。暗部太亮时先降低环境色；光斑不够明确时再调内外圈角度，不要同时大幅提高所有灯。`[opinion]`

Built-in 聚光灯使用运行时生成的径向 Cookie 控制内圈到外圈的过渡，内圈角度会小于外圈角度；无需策划另做 Cookie 文件。Unity 的 `innerSpotAngle` 本身只在 SRP 生效，见 [Unity 2022.3 API](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Light-innerSpotAngle.html)。`[verified: RoomLightingController.UpdateCookie；RoomLightingProfile.ValidateValues；Unity API Description]`

## 与其他演出的关系

睡前、梦境和自我审查期间，床引导继续控制房间熄灯；此时修改灯光配置只更新下次白天的基准，不应重新点亮房间。跨日或恢复检查点会使用更新后的白天基准。`[verified: RoomLightingController.SetDaylight、ApplyNow；BedtimeGuidance.UpdateDaylightLight、UpdateDaylightAmbient、ReapplyLighting、RestoreRoom]`

Combined 使用独立的 `RoomLightingReferenceRetro` 配置，默认像素尺寸 1，并关闭全屏量化，避免与材质照明分档叠加。首次安装会保留旧复古配置副本 `RetroScreenBeforeRoomLighting`；以后可在 **MiniGame → 画面 → 复古效果** 单独调整。`[verified: RoomLightingSetup.EnsureInScene、BackupRetroPath、RetroPath；RetroScreenProfile.FullscreenQuantization]`

## 回归与证据

新增 `RoomLightingPlayModeTests` 的四项测试检查主灯方向和灯罩间距、Cookie、家具投影、地毯、实时配置、临时对比、夜间调参后次日恢复，以及电脑自发光与分层参数的兼容。测试只使用克隆配置；通过正式游戏命令推进 Day0 到床引导，再执行床交互进入 Day1。`[verified: RoomLightingPlayModeTests.cs 四个 UnityTest 与 DriveToAfterDream]`

自动化断言不能证明与参考图视觉一致；需配合固定机位的修改前／连续灯光／最终分层对比，以及 720p、1080p 和地面近景检查。编译、测试、实景截图及 Player 的实际结果以本目录的验证记录为准，不将“有测试代码”视为“测试已通过”。`[opinion]`

调色、静态抖动与虚拟像素分辨率的使用见[策划风格指南](style-guide.md)；本轮结果见[验证记录](verification.json)。默认自然光保留；可选风格的同机位对比见[基础画面](evidence/style-base.png)、[仅调色](evidence/style-grade.png)、[调色与色阶](evidence/style-grade-quantize.png)、[完整风格](evidence/style-full.png)。`[verified: verification.json visual/automatedTests/build/player]`

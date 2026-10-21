# 门口视点的固定全景布景

隐藏海滩结局使用一张固定全景：玩家站在原场景的 `Player spawn` 门口出生点时，房间六面的画面与天空盒对齐；往床移动后，画面保持贴在墙、地板、天花板上，从而显露布景的形状。每日普通梦境沿用原配置。[verified: RoomDreamEnding.PrepareRoom；DreamCubemapRoomSurface.shader 的 _ProjectionOrigin]

## 直接预览

Unity 菜单 **MiniGame → 梦境 → 预览房间全景结局**，或 **MiniGame → 梦境与结局 → 海滩终局 → 预览房间全景结局**。预览从门口出生点开始，点击 Game 后用 WASD 移动、鼠标转头、靠近床按 E。[verified: DreamCubemapPreview；DreamEndingController.Update]

停止顶部播放按钮或使用 **MiniGame → 梦境 → 停止房间全景预览** 会返回原来的已保存场景。预览的保存、读档和重开按钮禁用，不写玩家存档。[verified: DreamCubemapPreview.OnState、RestoreScenes；DreamEndingController 的 IsPreview 守卫]

## 选择和制作天空盒

在 **梦境与结局 → 海滩终局** 中选择 **门口固定天空盒** 材质。默认使用 [DoorwayBeachSkybox.mat](../../UnityProject/Assets/MiniGame/Art/DreamEnding/DoorwayBeachSkybox.mat)，引用 [DoorwayBeachPanorama.asset](../../UnityProject/Assets/MiniGame/Art/DreamEnding/DoorwayBeachPanorama.asset)。[verified: DreamSettingsWindow.DrawEnding；DreamDoorwayPanoramaBaker 的资产路径与绑定]

**显示房间白色边框** 默认关闭，以便检查门口的完整海滩效果；打开后显示房间的 12 条白色棱线，床仍正常遮挡后方棱线。[verified: DreamSequenceProfile.ShowRoomOutline；RoomDreamEnding.UpdateRoomSurface；DreamCubemapRoomSurface.SetOutline]

需要更新默认海滩画面时，先制作并保存原海滩环境，再使用原全景捕获源生成按钮更新环境预制体，最后使用 **烘焙固定海滩天空盒**。烘焙器取海浪时钟第 0 秒的一帧，验证六向纹理读回一致后保存 Cubemap 和天空盒材质，不保存用户场景。[verified: DreamCubemapSetup.BuildSource；DreamDoorwayPanoramaBaker.Bake、VerifySixViews]

## 运行方式

进入布景时，位于门口眼睛位置的相机只拍选定天空盒，不拍床或房间；六面完成后固定使用这张纹理。走动、暂停恢复、躺下、漂流和读档都不会重拍它。世界空间表面点始终使用相对门口投影视点的方向取色。[verified: DreamCubemapRenderer 的天空盒构造函数；DreamEndingController.RenderRoomCubemap；DreamCubemapRoomSurface.shader]

床保持独立 3D 模型，靠近后仍可接躺下、漂流和字幕流程。背景海浪与云固定在捕获帧；修改天空盒后重新进入预览才会拍摄新画面。[verified: DreamEndingController.ApplyPose、RenderRoomCubemap]

## 验证与交付范围

本轮证据位于 `Artifacts/Unity/DreamDoorwayStatic20261021`：包含静态天空盒烘焙日志、Play Mode 测试、门口与走近床的对比图，以及对独立天空盒参考画面的像素比较。历次实现记录见 [验证记录](cubemap-verification.json)。此前 Mono ZIP 是历史动态实现，尚未包含固定门口布景。[verified: cubemap-verification.json 的 delivery、doorwayStaticRevision]
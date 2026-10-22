# 门口视点的全景布景与动态海浪

隐藏海滩结局使用固定视点的全景构图，并叠加旧海水 Shader 的动态海浪：玩家站在原场景的 `Player spawn` 门口出生点时，房间六面的画面与天空盒对齐；往床移动后，投影仍贴在墙、地板、天花板上，从而显露布景的形状。每日普通梦境沿用原配置。[verified: RoomDreamEnding.PrepareRoom；DreamCubemapRoomSurface.shader 的 _ProjectionOrigin；DreamPanoramaWater]

## 直接预览

Unity 菜单 **MiniGame → 梦境 → 预览房间全景结局**，或 **MiniGame → 梦境与结局 → 海滩终局 → 预览房间全景结局**。预览从门口出生点开始，点击 Game 后用 WASD 移动、鼠标转头、靠近床按 E。[verified: DreamCubemapPreview；DreamEndingController.Update]

参数入口为 **MiniGame → 梦境 → 海滩参数面板**。复古效果与海浪支持播放时调整、撤销和保存；操作与当前验证范围见[运行与调参说明](sea-midpoint/runtime-guide.md)。[verified: DreamSettingsWindow.OpenEndingPanel、DrawEnding、Save]

停止顶部播放按钮或使用 **MiniGame → 梦境 → 停止房间全景预览** 会返回原来的已保存场景。预览的保存、读档和重开按钮禁用，不写玩家存档。[verified: DreamCubemapPreview.OnState、RestoreScenes；DreamEndingController 的 IsPreview 守卫]

## 选择和制作天空盒

在 **梦境与结局 → 海滩终局** 中选择 **门口固定天空盒** 材质。当前默认使用 [SeaMidpointSkybox.mat](../../UnityProject/Assets/MiniGame/Art/DreamEnding/SeaMidpointSkybox.mat)，由用户提供的 Blender 海滩、两相机中点和半球天空罩重新拍摄；源文件、六面图与验证范围见[海滩中点拍摄说明](sea-midpoint/guide.md)。原 [DoorwayBeachSkybox.mat](../../UnityProject/Assets/MiniGame/Art/DreamEnding/DoorwayBeachSkybox.mat) 保留。[verified: DreamSequenceDefault.asset RoomPanoramaSkybox；SeaMidpoint/cubemap-verification.json]

**显示房间白色边框** 默认关闭，以便检查门口的完整海滩效果；打开后显示房间的 12 条白色棱线，床仍正常遮挡后方棱线。[verified: DreamSequenceProfile.ShowRoomOutline；RoomDreamEnding.UpdateRoomSurface；DreamCubemapRoomSurface.SetOutline]

原 Unity 海滩的更新入口仍是全景捕获源生成按钮和 **烘焙固定海滩天空盒**；该烘焙器会取海浪时钟第 0 秒的一帧并绑定原天空盒。当前 Blender 中点版本使用独立拍摄脚本，更新时按[海滩中点拍摄说明](sea-midpoint/guide.md)操作。[verified: DreamDoorwayPanoramaBaker.Bake；Tools/SeaDreamCapture/package_cubemap.py]

## 运行方式

进入布景时，相机固定在门口眼睛位置，拍选定天空盒和遮罩限定的旧海水网格，不拍床或房间。启用动态海浪时按配置刷新六面，暂停冻结演出时间，读档恢复同一浪形；走动、躺下和漂流不会改变捕获机位。关闭动态海浪时只保留静态全景。[verified: DreamCubemapRenderer 的天空盒构造函数与 Render；DreamEndingController.RenderRoomCubemap；DreamPanoramaWater.Apply]

床保持独立 3D 模型，靠近后仍可接躺下、漂流和字幕流程。天空与陆地使用 Blender 捕获帧；海浪复用 `MiniGame/Dream Sea` Shader。梦境复古效果使用独立配置，退出时恢复普通房间的配置与开关。[verified: DreamEndingController.ApplyPose；DreamPanoramaWater；RoomDreamEnding.RestoreRetro]

## 验证与交付范围

本轮证据位于 `Artifacts/Unity/DreamDoorwayStatic20261021`：包含静态天空盒烘焙日志、Play Mode 测试、门口与走近床的对比图，以及对独立天空盒参考画面的像素比较。历次实现记录见 [验证记录](cubemap-verification.json)。此前 Mono ZIP 是历史动态实现，尚未包含固定门口布景。[verified: cubemap-verification.json 的 delivery、doorwayStaticRevision]

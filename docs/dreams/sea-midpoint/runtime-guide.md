# Unity 2022 梦境参数面板

制作工程为 `F:/AAMINIGAME/MINIGAME/UnityProject`，基线 Unity 2022.3.23f1c1。打开 **MiniGame → 梦境 → 海滩参数面板**，窗口会定位到“梦境与结局 → 海滩终局”。点击“预览房间全景结局”即可进入同一房间梦境实现，预览不写玩家存档。[verified: UnityProject/ProjectSettings/ProjectVersion.txt；DreamSettingsWindow.OpenEndingPanel；DreamCubemapPreview.Open]

复古区使用独立 [DreamEndingRetro.asset](../../../UnityProject/Assets/MiniGame/Resources/MiniGame/DreamEndingRetro.asset)，起点复制既有参考复古配置。可调开关、强度、曝光、对比、饱和度、三段调色、色阶、Gamma、量化抖动、胶片颗粒与像素化。原图对比、撤销/重做和保存按钮沿用原复古编辑器；顶部“保存配置”也会保存这份复古资产。[verified: DreamSettingsWindow.DrawEnding、Save；RetroScreenProfileEditor.OnInspectorGUI]

海浪区使用旧 [DreamSea.shader](../../../UnityProject/Assets/MiniGame/Resources/DreamSea.shader) 与原海面网格，暴露波高、周期、波长、冲刷、泡沫、海平面高度、到岸线距离和全景刷新率。海面遮罩来自 Blender 中 Ocean001/002/004 的可见部分，Ocean005 泥沙和其他布景不属于水面。[verified: DreamPanoramaWater；Tools/SeaDreamCapture/render_water_mask.py；Art/DreamEnding/SeaMidpoint/water-mask-verification.json]

上述波浪参数和复古开关/参数在播放中实时读取，暂停时改海浪参数也会刷新一帧，但演出时间不前进。暂停恢复与检查点共用旧海浪的显式时间；门口捕获眼睛位置始终固定，因此保留离开门口后显露房间布景的效果。更换遮罩、海滩捕获源或每面分辨率后重新进入梦境。[verified: DreamCubemapRenderer.Render；DreamEndingController.AdvanceForTests、RenderRoomCubemap；DreamDynamicSeaPlayModeTests]

“启用动态海浪”关闭时恢复静态照片。此时不要求旧海面资源仍存在；“启用梦境复古效果”关闭时查看未处理画面。退出梦境恢复普通房间原有复古配置、启用状态、昼夜跟随与原图比较状态。[verified: DreamDynamicSeaPlayModeTests.StaticPanoramaDoesNotRequireTheOptionalSeaSource、CombinedDreamUsesIndependentRetroAndRestoresRoomAfterPauseAndLiveTuning]

本次使用正式工程文件的独立快照进行 Unity 2022.3 编译、GPU 截图和针对性 Play Mode/Edit Mode 测试。当前打开的原编辑器窗口无法取得有效截图，未据此宣称其面板已做人工视觉验收；未重新构建 Player，也未从第一天完整通关。[verified: Artifacts/Unity/DreamRuntime20261022；本轮 computer-use 窗口捕获超时]

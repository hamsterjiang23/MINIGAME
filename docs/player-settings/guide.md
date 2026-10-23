# 玩家画面设置

主菜单和游戏内 **Esc → 游戏设置** 共用同一个设置面板。像素选项统一称为 **像素风强度**：像素块越大，颗粒感越强；默认选择中。已有玩家保存的高、中、低设置继续保留。[verified: PlayerSettingsView.Build；PlayerGraphicsSettings.DefaultDetail/Reload]

| 强度 | 同尺寸黄色网格图例 | 实际屏幕像素块 | 说明 |
|---|---|---|---|
| 高 | 2×2 格 | 8×8 | 大格子，颗粒感强。[verified: PixelStrengthTooltip.Show/Description；PixelDetail.High] |
| 中 | 4×4 格 | 4×4 | 颗粒与细节适中，默认推荐。[verified: PixelStrengthTooltip.Show/Description；PlayerGraphicsSettings.DefaultDetail] |
| 低 | 8×8 格 | 2×2 | 小格子，细节更多。[verified: PixelStrengthTooltip.Show/Description；PixelDetail.Low] |

三个图例使用相同的 128×128 UI 尺寸，细黑线分格；仅鼠标悬停对应选项时显示，移开立即隐藏。点击选中或键盘聚焦不会让图例常驻，浮层不挡点击。图例的行列数与实际屏幕像素块尺寸是两种不同的度量：相同区域里，块越大，能容纳的格子越少。[verified: PixelStrengthTooltip.Create/Show；PixelStrengthHintTrigger.OnPointerEnter/OnPointerExit] [opinion]

悬停只解释，不更改设置。点击选项立即应用并自动保存到本机 PlayerPrefs；不改变故事存档或昼夜美术配置。UI 保持原有清晰度，梦境专用效果仍由演出配置控制。Esc 先返回暂停菜单，再按一次恢复游戏。[verified: PlayerSettingsView.Build；PlayerGraphicsSettings.SetDetail；RetroScreenEffect.PlayerPixelSettingsActive；MiniGameBootstrap.HandleEscape]

## 首次启动

没有正式继续／手动存档、没有结局完成标记、且从未确认过像素选择的玩家，会在原主菜单之前看到三个实机预览。已有画面偏好时预选原值；没有有效偏好时预选中。点击图片或方向键切换候选，点击“确认并继续”或按 Enter 保存并进入原有首次启动页，不直接开始剧情。[verified: PixelStrengthOnboarding.ShouldShow；MainMenuController.ShowStartup/HandlePixelSetupKey/ConfirmPixelSetup；PixelStrengthSelectionView]

确认后即使尚未产生游戏存档，再次启动也不提示；关闭游戏而未确认则下次继续显示，Escape 不能跳过。游戏内返回主菜单或重新开始不会插入该页面。普通设置修改不会代替首次确认。[verified: PixelStrengthOnboarding.Confirm；MainMenuController.ReturnToMenu/RequestNewInspection；PixelStrengthOnboardingPlayModeTests]

三张图来自正式 `MainRoomCombined` 日间病房的同一机位、同一模拟帧和 1280×720 GPU 渲染，仅切换像素尺寸 8／4／2。预览是本地资源，打开选择页不运行剧情。[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/PixelPreviews/capture-metadata.json captures/frame；PixelStrengthSelectionView.Build]

实现与验证记录见 [实现记录](implementation-notes.md) 和 [本轮验收](verification-20261023.md)。[opinion]

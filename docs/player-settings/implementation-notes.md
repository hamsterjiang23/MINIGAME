# 像素风强度实现记录

2026-10-23 按确认方案将默认值从低改为中；三档实际像素尺寸仍为高 8、中 4、低 2。`Reload()` 显式解析 2／4／8，缺失及无效值回退中，避免旧低档被默认值变更覆盖。[verified: PlayerGraphicsSettings.DefaultDetail/Reload]

主菜单和暂停菜单继续复用 `PlayerSettingsView`，新增 `PixelStrengthTooltip` 和悬停触发器。按用户最后确认，图例只由鼠标进入／移出控制，点击选中与键盘聚焦不显示；页面关闭时也清除。图例统一 128×128，以 2／4／8 行列示意颗粒大小，并显示直白描述。所有图例 Graphic 都关闭 raycastTarget，CanvasGroup 不拦截输入。[verified: PlayerSettingsView.Build；PixelStrengthTooltip；PixelStrengthHintTrigger.OnPointerEnter/OnPointerExit/OnDisable]

图例细线按有效缩放系数保证至少 1 个屏幕像素，并在分辨率变化时更新。初版固定 1 UI 单位在 720p 会漏掉部分线，实际格数与标签不符；此项由 Player 原图逐像素检查发现。[verified: PixelStrengthTooltip.RefreshLineThickness/LateUpdate；../../Artifacts/PixelStrength/PlayerVisible/pause-grid-medium.png]

首次页追加到现有 `MainMenuPage`，不改剧情存档结构。首次确认单独写入 `NORM.Graphics.PixelSetupCompleted.v1`；是否已有进度复用 `MainMenuContinueStore`，保持完成标记优先级。预选和悬停不写入偏好；确认同时保存档位与本地完成标记。[verified: MainMenuView.MainMenuPage；PixelStrengthOnboarding；MainMenuController.ConfirmPixelSetup]

首次页的 uGUI Submit 只消费输入，方向键和 Enter 统一由 `MainMenuController` 处理。确认记录当前帧，阻止同帧输入继续触发原开始按钮。Escape 被消费但不跳过。[verified: PixelStrengthSelectionButton.OnSubmit；MainMenuController.HandlePixelSetupKey/StartFirstInspection/Update]

三张本地预览由 `PixelStrengthPreviewCapture` 捕获正式病房相机，暂时克隆美术配置，渲染后还原相机、Canvas 和效果状态；捕获记录包括 shader 参数、姿态、帧号、资源哈希及恢复结果。`PixelStrengthPlayerProbe` 仅在开发构建显式参数或 Editor 调用时运行，使用独立存档路径，并备份和恢复两项偏好。[verified: PixelStrengthPreviewCapture.Capture；PixelStrengthPlayerProbe.PrepareScope/RestorePreferences；MiniGameBootstrap.SaveDirectory]

新增文案补齐“块／粒／荐”三个字、七个字号共 21 个字形；既有 10164 个字形的 RGBA 像素及尺寸、偏移、advance 对比未变，图集仍是 2048×2048。[verified: ../terminal-font/pixel-strength-glyph-audit-20261023.json beforeGlyphs/addedGlyphSizePairs/pixelOrMetricChanges/atlasDimensions]

设置页继续使用实时场景背景供比较；演出 PresentationProfile／PresentationBypass 保持原有优先级，不宣称三档有任何已测得的性能收益。[verified: PauseMenuBackdrop；RetroScreenEffect.ProfileForRendering] [opinion]

历史 2026-10-22 的默认低档及“不打新包”交付要求已由本轮确认方案更新；本轮结果与边界以 [验收记录](verification-20261023.md) 为准。[verified: 用户本轮确认方案]

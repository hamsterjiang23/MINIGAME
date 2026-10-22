# Deviations

- 用户修正：高／中／低对应像素尺寸 8／4／2，默认低档 2。玩家选择通过 PlayerPrefs 保存，不写入昼夜美术配置或剧情存档。[verified: 用户要求]

# Discovered edge cases

- 暂停菜单原来显示冻结且模糊的背景；设置页需要临时显示实时场景，否则玩家看不到调整结果。[verified: PauseMenuBackdrop.CaptureFrame]
- 梦境有专用 PresentationProfile／PresentationBypass，玩家像素尺度只覆盖普通场景，保留原演出处理。[verified: RetroScreenEffect.ProfileForRendering]
- 交互轮廓宽度应使用玩家实际选择的像素尺度。[verified: InteractionFocusOutline.ActivePixelSize]

# Questions for review

- 本次设置只调整像素精细度，不宣称高中低对应任何性能收益。[opinion]

## 完成记录

- 对应关系：高 8、中 4、低 2，默认低 2。[verified: 用户修正；PlayerGraphicsSettings]
- 存储：PlayerPrefs 只保存玩家选择，美术配置保持独立。[verified: PlayerGraphicsSettings.SetDetail；RetroScreenEffect.Render]
- 输入：设置页阻止暂停主菜单回调和 F5/F9；Esc 先返回暂停。[verified: PauseMenuView.SubmenuVisible；MiniGameBootstrap.Update/HandleEscape]
- 已有验证：3 项冒烟与 27 项运行回归通过；没有继续打包或新开测试。[verified: smoke.xml；playmode.xml；用户停止打包测试要求]
- 交付方式：按用户最新要求提交全部本地 Git 改动并推送。[verified: 用户要求]

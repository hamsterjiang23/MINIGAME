# 像素风强度与首次选择验收

本轮范围是设置图例、默认中档、旧偏好兼容和首次启动选择页。沿用正式 Combined 场景与已有剧情流程；没有更改核心存档结构。[verified: 用户确认方案；PixelStrengthOnboarding；PlayerGraphicsSettings；MainMenuController.ShowStartup]

最终图例规则按用户追加要求收紧为仅鼠标悬停显示：鼠标移出立即隐藏，即使该选项仍被选中；键盘聚焦不弹出图例。此规则取代最初“键盘聚焦也显示”的方案。[verified: 用户最新悬停要求；PixelStrengthHintTrigger.OnPointerEnter/OnPointerExit；PixelStrengthSettingsAssertions.AssertLegendInteractions]

## 自动检查

| 检查 | 结果与证据 |
|---|---|
| Unity 编译 | `scriptCompilationFailed=false`、`isCompiling=false`，新探针类型已导入。[verified: ../../Artifacts/PixelStrength/compile-clean.json data.result.result] |
| PlayMode `PixelStrength` | 24/24，最终仅鼠标悬停规则下通过默认／无效偏好、旧 2/4/8、四种正式进度文件、首次确认、未确认重启、候选不保存、同帧防穿透、游戏内回菜单及两处图例交互；覆盖移出隐藏和键盘聚焦不显示。[verified: ../../Artifacts/PixelStrength/playmode-hover-final.json data.result.summary/results；PixelStrengthSettingsAssertions.AssertLegendInteractions] |
| EditMode `TerminalBitmapTextTests` | 25/25；对白聊天更新字体后再次通过。[verified: ../../Artifacts/PixelStrength/editmode-font-final.json data.result.Summary] |
| 原主菜单流程 | 原 14 项中 13 项通过，1 项仍断言海滩音乐为空；更新为生产配置和相反可选配置后，该项重跑 1/1 通过。没有修改实际音乐。[verified: ../../Artifacts/PixelStrength/menu-regression.json data.result；menu-music-retry.json data.result.summary；MainMenuFlowPlayModeTests.HiddenSpaceContinueCrossfadesItsMusicAndOceanWithoutResumingVoiceEarly] |
| 字形生成与旧像素保护 | 4/4 Python 测试；新增 21 个字形，旧 10164 个字形未变。[verified: ../terminal-font/pixel-strength-glyph-audit-20261023.json unitTests/pixelOrMetricChanges] |
| Windows Player 构建 | `Succeeded`，0 error、1 warning；警告为构建场景未配置 `RuntimePipelineManager`，该插件的运行时 Pipeline 在 Player 中停用。[verified: ../../Artifacts/PixelStrength/build-hover-status.json data.result.result/totalErrors/totalWarnings/warnings] |
| Windows Player 1280×720 | 13 张完整 GPU 帧，首次选择、确认不启动剧情、重新加载后跳过，以及主菜单／暂停图例全部通过；包含两处鼠标移出后的无图例画面；`preferencesRestored=true`。[verified: ../../Artifacts/PixelStrength/PlayerHover1280/report.json passed/preferencesRestored/captures] |
| Windows Player 1024×768 | 13 张完整 GPU 帧，相同流程、两处图例及鼠标移出画面全部通过；`preferencesRestored=true`。[verified: ../../Artifacts/PixelStrength/PlayerHover4x3/report.json passed/preferencesRestored/captures] |
| 最终截图网格检查 | 12/12：两种窗口尺寸、两处设置菜单、三档图例均从原始 PNG 检出正确的 2×2／4×4／8×8 格；未修改图片。[verified: ../../Artifacts/PixelStrength/grid-audit-hover.json passed/checked/method/checks] |

测试使用独立临时存档目录，结束时恢复玩家既有偏好。首次确认后重启的 PlayMode 检查销毁旧菜单并重新加载 Combined；不是只切换页面。[verified: PixelStrengthOnboardingPlayModeTests.SetUp/TearDown/Reboot；PixelStrengthPreferenceScope.Dispose]

## 实机预览来源

三张选择图是 1280×720 的 GPU 渲染，来源 `MainRoomCombined` 日间相机。位置 `(-4.3, 0.85, -3.3)`，旋转约 `(7.198, 40.732, 0)`，FOV 37.299；三图共享模拟帧 4144。实际 shader `_PixelSize` 为 8／4／2，`_UseVirtualResolution=0`，符合玩家像素选项的渲染路径。[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/PixelPreviews/capture-metadata.json position/eulerAngles/fieldOfView/frame/captures；RetroScreenEffect.Render]

捕获期间暂时关闭 UI，只更改临时配置副本；完成后相机、Canvas、RenderTexture、PresentationProfile 和原美术配置已恢复。不是生成图片或图像后处理模拟。[verified: PixelStrengthPreviewCapture.Capture；capture-metadata.json restored/sourceProfileJson]

## 画面与输入边界

Editor／Player 探针使用正式页面和实际屏幕坐标射线，派发左键事件并调用键盘处理入口；截图来自 `ScreenCapture.CaptureScreenshotAsTexture()` 的完整 GPU 帧，包含 Overlay Canvas。采集时暂停物理输入模块，结束后恢复，避免桌面鼠标位置干扰悬停图例。这些结果属于程序化输入验证和实际画面检查，不代表人工鼠标／键盘验收。[verified: PixelStrengthPlayerProbe.Run/Hit/Click/Capture/Finish/OnDestroy] [opinion]

早期采集的 Direct3D11 RenderTexture 读回出现上下翻转，已改用原生 ScreenshotAsTexture；另两次探针问题是布局首帧射线尚未就绪、悬停已选中按钮后未先移走焦点。最终探针等完整渲染帧并显式执行焦点切换，游戏 UI 无需为这些探针时序做改变。[verified: PixelStrengthPlayerProbe.Run/VerifyExitAndFocus/Capture；../../Artifacts/PixelStrength/editor-initial-report.json error；../../Artifacts/PixelStrength/EditorFinal/report.json error]

首次 Windows 隐藏窗口运行虽然交互断言通过，截图却全黑，因此该次不计入画面验收。探针已增加黑图检查，正式采集使用可见的交互游戏窗口。[verified: ../../Artifacts/PixelStrength/PlayerVerified/report.json；PlayerVerified/onboarding-default-medium.png；PixelStrengthPlayerProbe.Capture]

720p 实机截图发现原来一个 UI 单位宽的黑线经过 Canvas 缩放后会消失，导致部分 4×4／8×8 图例显示较少格子。现按实际缩放补足线宽，保证至少一个屏幕像素，保持图例 128×128 的设计尺寸。最终两种尺寸的 12 张图例均通过原图像素检查。[verified: ../../Artifacts/PixelStrength/grid-audit-before-fix.json checks；PixelStrengthTooltip.RefreshLineThickness；../../Artifacts/PixelStrength/grid-audit-hover.json checked/passed]

4:3 的早期重跑曾失败于“移出后图例隐藏”：探针禁用了物理输入模块，但 EventSystem 仍缓存该模块，真实桌面鼠标事件继续干扰程序化事件。探针现在重置现有 EventSystem 的模块缓存，并断言 `currentInputModule==null`；重新构建后两种尺寸完整通过。这项修正属于采集隔离问题，没有据此修改产品的悬停／移出逻辑；其后取消键盘聚焦图例是用户追加需求。[verified: ../../Artifacts/PixelStrength/Player4x3Final/report.json error；PixelStrengthPlayerProbe.Run；../../Artifacts/PixelStrength/PlayerHover1280/report.json passed；../../Artifacts/PixelStrength/PlayerHover4x3/report.json passed；用户最新悬停要求]

最终画面：[1280×720 首次选择页](evidence/onboarding-1280.png)、[1024×768 首次选择页](evidence/onboarding-4x3.png)、主菜单[高](evidence/main-menu-grid-high.png)／[中](evidence/main-menu-grid-medium.png)／[低](evidence/main-menu-grid-low.png)图例、[暂停菜单中档图例](evidence/pause-grid-medium.png)、鼠标移出后的[主菜单](evidence/main-menu-no-hover.png)／[暂停菜单](evidence/pause-no-hover.png)。[网格检查记录](evidence/grid-audit-final.json)复制自最新 `grid-audit-hover.json`，对应全部 12 张原始图例截图。[verified: ../../Artifacts/PixelStrength/PlayerHover1280/report.json captures；../../Artifacts/PixelStrength/PlayerHover4x3/report.json captures；../../Artifacts/PixelStrength/grid-audit-hover.json checks]

本轮没有改动 GameSession 或 .NET 核心规则，未重复运行核心 SDK 构建；Unity 测试、目标平台构建与画面证据单独记录。[verified: 本轮修改范围] [opinion]

最终 Editor 采集在协调导入新版对白后执行。活动内容为 `1.2.2-development`，来源哈希 `e617ce0bb24e4be75b8f6aaad5f1e3fb916bcbb351d8c6335a3c3b90eb58551b`；对白与其 24 字扩充由另一聊天负责，本轮保留。最新 Editor 报告包含 13 张截图，覆盖仅悬停显示、移出隐藏及两处无悬停画面，偏好恢复成功。[verified: ../../Artifacts/DialogueRefresh20261023/unity-import.json contentVersion/sourceHash；../../Artifacts/PixelStrength/EditorHover/report.json passed/preferencesRestored/captures；PixelStrengthPlayerProbe.VerifyExitAndFocus]

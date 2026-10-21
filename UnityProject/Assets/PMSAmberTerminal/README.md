# PMS 琥珀终端 · HTML 校准版 v0.8.12

以确认的 HTML v0.8.12 为视觉基准：800×600 字符终端、暖灰病房监控、琥珀色文字、4:3 输出、编号自检、仅开机后的分区加载及柔光关机。源码、坐标及显示组件随包提供。

## 替换旧包

1. 备份自己在旧包目录中做的修改。
2. 解压 ZIP，将 `Assets/PMSAmberTerminal` 整个目录覆盖到 Unity 工程的同名目录。必须一起复制 `.meta`；这些文件保留原脚本、预制体和图片的引用。不要只复制 Runtime 或字体图片，也不要把 ZIP 本身拖入 Project 窗口。
3. 等待 Unity 重新导入。打开 `Assets/PMSAmberTerminal/Scenes/PMSAmberDemo.unity`，按 Play。菜单 **PMS Amber Terminal → Open demo scene** 也可打开。
4. 旧版的 `Fonts/NotoSansMonoCJKsc-Regular.otf` 不再使用。如宿主其他地方仍在引用它，可以保留；本版终端不会读取它。

使用 Unity 2022.3 LTS 和 Unity UI（com.unity.ugui）。不依赖浏览器、WebView 或 TextMesh Pro。默认输入使用旧 Input Manager；源码含新 Input System 分支，该分支未做运行验证。

## 这次字体如何还原

HTML 原先使用中文宋体 SimSun、英文 Lucida Console。旧包替换成另一种中文字体，导致笔画、字宽和基线都发生变化。

本版不再让 Unity 重新生成字体。包内固化了原字号、原颜色的像素字形，直接绘制其位置和字格：537 个字符、7 个字号（10、11、12、13、14、18、19）、5 种文字状态，共 18,795 个字形记录。反白字、暗字、亮字分别输出，避免同一白色字形染色造成边缘变粗。

预制体的 `Glyphs` 已指定 `HTMLBitmapFont`，无需手动选择 Font 或 TMP Font Asset，也无需在玩家电脑安装宋体或 Lucida Console。旧场景中未设置 `Glyphs` 的显示组件会读取包内 Resources 默认字形资源。

请保留字体图集、度量文件和它们的 `.meta`。图集使用 Point、不压缩、不生成 Mipmap；不能启用自动缩放。病房图像也保留原始 464×261 尺寸，不能被自动缩放到 2 的幂。正确设置已写入图片 `.meta`。

## 预览及接入

演示场景可预览名册、对话、应答、记录、处置及开关机。它只有简短的示例对白和一名演示对象，不是正式 Day1 剧情或完整游戏规则。完整 HTML 和对白参考位于 `Reference`。

加入正式场景时使用 `Prefabs/PMSAmberTerminal.prefab`。预制体只有显示组件，不会自己执行处置；不要将 `PMSPreviewDriver` 接入正式玩法。

默认输出保持 4:3。`CrtMode` 为 0 关闭 CRT、1 标准 CRT、2 老化 CRT；确认的 HTML 默认使用标准 CRT。`Ward` 对应暖灰病房图像。原始图片与暖灰处理图都在 Art 中。

源摄像机使用 Layer 31，可通过 `SourceLayer` 调整。`ShowOverlay` 控制默认全屏输出；`ReadKeyboardAndMouse` 控制自带输入。正式游戏可关闭自带输入并统一转发交互。

| 接口 | 用途 |
| --- | --- |
| `SetRoster(PMSRosterEntry[])` | 由宿主提供角色名册，显示最多五行 |
| `ShowDirectory()` | 立即显示名册，不播放加载 |
| `ShowReview(PMSReviewData)` / `SetReview(...)` | 显示或更新审查页面，不播放加载 |
| `EnterRequested(string id)` | 请求进入对应角色会谈 |
| `ContinueRequested` | 请求推进当前对白 |
| `ReplyRequested(string id)` | 请求提交应答 |
| `DispositionRequested(string type)` | 请求释放、治疗或净化 |
| `NotifyAllCharactersCompletedAfterDialogue()` | 全部当日对象完成处置、且最后反应对白结束后，由宿主调用，进入关机 |
| `ShutdownCompleted` | 熄屏完成通知 |
| `Boot()` | 重新播放开机和分区加载，不重置宿主游戏状态 |
| `HandleKey(string)` | Enter / Space / Up / Down / F3 / Escape / 1–4 |
| `PointerUV(uv, down, up)` | 3D 电脑屏幕的交互坐标，UV 原点在左下 |

GameSession / Yarn 仍应负责正式对白、状态、配额、处置条件及角色去留。本包只显示投影、发出交互请求，不建立另一份游戏规则。只在宿主确认最后反应完成后触发关机。

若接入 3D 电脑屏幕，可关闭 `ShowOverlay`，用 `ScreenTexture` 作为原始图像，通过 `ScreenMaterial` 后处理到宿主 RenderTexture。着色器会依据 Gamma / Linear 色彩空间转换最终输出；原始图像与图集保持包内已校准的非 sRGB 导入设置。

## 新增对白的字形

当前字形集覆盖确认的 HTML 内容及演示文字。新增人名或对白若含未收录字符，运行时会提示缺字并显示问号，不会悄悄改用另一种字体。

用 Windows 浏览器打开 `Tools/HTML原字形导出.html`，确认本机有原版 SimSun 和 Lucida Console，在原字形集基础上添加新文字或导入对白文件，导出 PNG 和 JSON。替换 `Art/FontAtlas/HTML_SimSun_Lucida.png` 与 `HTML_FontMetrics.json`，保留两文件的 `.meta`。不要改文件名。若新图集尺寸变大，检查导入 Max Size 不小于它；包内设置允许至 8192。

该工具只在扩展字形时需要原系统字体。它不包含或安装 Windows 字体文件。

## 校准证据与范围

`Calibration/Gamma` 和 `Calibration/Linear` 保存 Unity 2022.3 Windows Editor 实际运行捕获，覆盖名册、对话、应答、记录和处置五种画面，以及开关机检查记录。`Preview/Unity_*` 使用本次校准后的实际 Unity 捕获，不再使用旧字体预览。

`Calibration/HTML_*_Raw.png` 是执行原 HTML 绘图代码得到的本地 Canvas2D 参考图；`HTML_*_CRTModel.png` 是原 GLSL 公式的数值参考。它们不是浏览器 GPU 截图。逐像素统计见各色彩空间目录下的 JSON。不同栅格器、GPU 取样和混合舍入仍有少量像素差异，不能据此承诺任意机器逐像素完全一致。

已在独立验证项目检查脚本、资源引用、CRT 着色器、五种画面、普通页面切换不加载、关机及重新开机。未修改现有游戏工程。验证使用 Built-in 渲染管线；URP/HDRP、正式 Player 构建及完整项目集成未在此次验证范围内。

包内 `验证记录.json` 记录实际验证结果，`文件清单.json` 提供完整文件校验。

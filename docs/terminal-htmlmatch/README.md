# 电脑终端 HTML 字体与高亮

正式终端使用本次 `PMS_Unity_Resources_v0.8.12_HTMLMatched.zip` 的字形资源：中文为宋体，拉丁字符为 Lucida Console；原字号、颜色、字格和 20px 行距由位图图集提供。原生画布为 800×600，Point 采样，避免二次缩放模糊字边。[verified: Runtime/TerminalBitmapText.cs；Runtime/TerminalTextMetrics.cs；Runtime/TerminalDisplay.cs native RT]

[目录预览](directory-preview.png) · [悬停反白预览](highlight-preview.png) · [验证记录](verification.json)。[verified: 本目录交付文件]

## 在当前工程查看

用 Unity 2022.3 打开 `UnityProject`，进入 Combined 的电脑，查看对象目录、应答命令和处置命令。资源已经接到 `Assets/MiniGame/Resources/MiniGame/TerminalAssets.asset` 的 `HtmlTypeface` 与 `DisplayShader`。若重新导入供应包，执行 **MiniGame → 终端 → 接入 HTML 字体与高亮资源** 恢复导入与引用设置。[verified: Editor/TerminalHtmlMatchedSetup.cs Install；TerminalAssets.asset HtmlTypeface/DisplayShader]

## 交互规则

- 目录单击选择，双击或 Enter 进入；↑↓ 切换对象。应答支持 ↑↓、Enter、单击及数字快捷键。[verified: Runtime/TerminalView.cs HandleDirectoryKey/HandleKey；reference-v0.8.12.html row/目录/应答区域]
- 当前行是黄色整行反白、深色文字；悬停另一行时只高亮悬停行，离开后恢复键盘选择。按方向键会接管选择，鼠标再次移动时恢复悬停。[verified: Runtime/TerminalHighlightGroup.cs；Runtime/TerminalButtonVisual.cs HighlightPointer/Paint]
- 鼠标按下，文字向右下移动 1 个原生像素，并显示内框；松开、移出、禁用或窗口失焦会复位。[verified: Runtime/TerminalButtonVisual.cs OnPointerDown/ResetPressed/OnApplicationFocus]
- 未解锁的处置命令使用暗色文字，不获得反白；“当前命令”为细线框；底部提示始终保留黄色条。[verified: Runtime/TerminalButtonVisual.cs Paint/SetCommandField/SetAlwaysHighlighted]

正式游戏的会谈、配额、动态人物监控、被动历史和每日开关机仍由现有宿主管理。参考 HTML 中的 F2/F3 旧功能没有恢复。[verified: Runtime/TerminalView.cs；Tests/PlayMode/TerminalHtmlParityTests.cs RemovedFunctionKeysLeavePassiveTranscriptAndDialogueAvailable]

## 修改文本与扩充字形

当前图集覆盖正式 Campaign 和运行时文本，共 1,339 个码位、7 个字号、5 种配色；原包的 18,795 个字形像素与指标保留。扩充方法见 [字形生成说明](../../Art/TerminalHtmlMatched/README.md)。新文案包含图集以外的字符时，运行生成器再交付，运行时不会悄悄换成其它字体。[verified: ../../Art/TerminalHtmlMatched/glyph-coverage.json output/preservation/coverage；PMSBitmapFont.cs Get]

支持的字号为 10、11、12、13、14、18、19；HTML 五种文字配色保留各自的抗锯齿边缘，额外颜色只作近似着色。不要靠替换旧 `Typeface` 字段改变这套终端的字形。[verified: Runtime/TerminalBitmapText.cs SupportedSize/ResolveColor；Runtime/TerminalAssetSet.cs HtmlTypeface tooltip]

## 验证范围

Unity 2022.3.23f1c1、Gamma：28 项 Editor 测试及 9 项 PlayMode 测试通过，最新图集又单独复核真实 GPU 字形、悬停、按下和裁切。五处相同静态文字区域与供应包原图逐像素比较，最大通道差为 1/255；这项比较仅涵盖这些文字区域。[verified: verification.json EditorTests/PlayModeTests/RawReferenceComparison]

Windows Development 构建成功，保留两条构建提示：Player 中没有 RuntimePipelineManager，因此禁用 CLI；Unity 同时报告有未编译代码更改。提示原文见验证记录，未将其它正在修改的场景与特效纳入本次验收。实际 Player 完成六次应答、退出再进入、暂停、读档、实体屏幕朝向与最终关机检查。[verified: verification.json WindowsBuild.warningMessages/Player]

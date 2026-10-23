# 电脑终端 HTML 资源与交互

## 当前目录档案区：v0.8.26

[角色档案接入与验证](dossier-v0.8.26/guide.md)：六张原包肖像、五份 DOCX 资料、两行框内滚动、独立档案配额图集。运行时名单、状态、配额和会谈表情映射继续沿用现有来源。`[verified: dossier-v0.8.26/source-manifest.json profiles；TerminalView.RenderRoster]`

## 当前右侧交互区：v0.8.24

[本轮验收与正式场景／Player 画面](verification-v0.8.24.md)。`[verified: verification-v0.8.24.md 自动检查、正式场景与 Player 画面]`

右侧处置区以 [v0.8.24 HTML](reference-v0.8.24.html) 为准：三个处置使用供应包的图标整行图片，包含普通、选中、按下、禁用四态；原生图片为 248×48，只有顶部 248×44 接受点击，底部间隙不接受处置输入。`[verified: reference-v0.8.24.html L310-L323；../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs 处置构造循环]`

- 鼠标悬停与方向键共享选择，离开按钮仍保留选择；只有鼠标左键执行处置。处置阶段 Enter、小键盘 Enter、空格被消费，不提交处置或推进对白。下一句、应答与返回目录保留原键盘操作。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs HandleDispositionKey；../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs HandleTerminalKey]`
- 净化剩余用 66×32 黄色字段显示，读取 `GameSession` 的实际剩余配额；00—99 使用原包图集，更大值回退为真实数字。旧数字 Text 保留实际文本但不叠加绘制。原型默认配额与信任阈值不进入正式规则。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs Render；../../UnityProject/Assets/MiniGame/Runtime/TerminalQuotaField.cs SetValue]`
- 会谈中、暂停、查阅记录和演出锁定时不可执行；自审仅开放核心允许的净化。应答区从原生坐标 `(530,392)` 开始，四行底部为 525；旁白后的继续按钮也保持在这一边界内。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs RenderDispositionCommands、SetNarration、AddResponse]`
- 新资源位于 `Assets/PMSAmberUI_v0824`，由现有终端安装菜单绑定。图标已经缩小到定稿比例，直接使用原生像素；不覆盖现有监控、字体图集或 CRT 着色器。`[verified: ../../UnityProject/Assets/MiniGame/Editor/TerminalHtmlMatchedSetup.cs Install；resource-pack-v0.8.24-README.md L7-L20]`

以下记录保留 v0.8.12 字体与高亮接入的历史背景，历史测试数字不代表本轮验收。`[opinion]`

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

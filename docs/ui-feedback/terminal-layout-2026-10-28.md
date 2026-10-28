# 终端滚动提示与右侧布局微调（2026-10-28）

## 修改

- 对话滚动提示宽度从 2 增至 3.4 个终端逻辑像素，加粗 70%；保持距右边缘 1 像素，创建和滚动更新使用同一组尺寸。[verified: UnityProject/Assets/MiniGame/Runtime/TerminalTranscriptView.cs ThumbWidth/ThumbRightInset/UpdateThumb]
- 处置标题下移 20 像素，三条命令起点改为 y=164/228/292，行距从 48 增至 64；保留原有 248×48 图像和 248×44 点击区域，使命令区纵向分布更均匀。[verified: UnityProject/Assets/MiniGame/Runtime/TerminalView.cs L202–215]
- 应答分隔线改至 y=362，与左侧社会适应度区顶部对齐；应答内容从 y=392 下移至 402，容器高度 134，第四条回复底部为 y=535，仍位于 y=548 的底部命令条之上。[verified: UnityProject/Assets/MiniGame/Runtime/TerminalView.cs SocialAdaptationInstrument/AnswerContents/AddResponse/TerminalCommandStrip]
- 右侧中段留白缩小，命令与应答形成更均匀的纵向节奏。[opinion]
- 后续亮度微调：“当日指标 / 净化剩余”从 Dim（#a77d2d）改为 Bright（#ffcf68），与左侧“监控 / 单元”的 MonitorHeading 共用同一颜色。[verified: UnityProject/Assets/MiniGame/Runtime/TerminalView.cs MonitorHeading/DailyWorkIndicatorLabel]
- 亮度微调后，记录与应答、对白字形 PlayMode 用例各 1/1 通过。隔离测试的临时诊断日志确认指标文字为 RGBA(1.000, 0.812, 0.408, 1.000)，处于启用状态且有 36 个字形网格顶点；本次离屏截图中该标签未正常显示，因此截图不作为亮度视觉验收证据。临时诊断仅用于隔离工程，采集后恢复测试文件。[verified: Artifacts/TerminalLayout20261028/indicator-brightness.xml；Artifacts/TerminalLayout20261028/indicator-brightness-final.xml；Artifacts/TerminalLayout20261028/indicator-brightness-confirmed-editor.log INDICATOR]

## 本次验证

使用现有隔离工程 `Artifacts/InteractionVisualValidation/UnityProject`，同步当前 Assets；实际 Editor 为 `E:/minigame101/Unity2022/Editor/Unity.exe`，启动日志版本为 **2022.3.23f1 (c87a96277412)**，与该隔离工程版本一致。源工程的版本文件当前为 **2022.3.23f1c1**，本次未修改其版本或 ProjectSettings。以下结果属于隔离工程的 Editor PlayMode 验证。[verified: Artifacts/TerminalLayout20261028/playmode-verified-editor.log Initialize engine version；UnityProject/ProjectSettings/ProjectVersion.txt；Artifacts/InteractionVisualValidation/UnityProject/ProjectSettings/ProjectVersion.txt]

- 最终相关用例 **13/13 通过**：终端对白与四选项、三种 CRT 模式下的按钮命中与高亮、处置间隙不提交命令、暂停及重复提交阻断、配额显示、不同视口比例适配。更新了现有布局测试中的坐标与间隙采样位置。[verified: Artifacts/TerminalLayout20261028/playmode-verified.xml test-run result=Passed/total=13/passed=13/failed=0；UnityProject/Assets/MiniGame/Tests/PlayMode/TerminalDispositionPlayModeTests.cs AuthoredStatesAndFortyFourPixelHitAreasMatchAllThreeCrtModes]
- 扩展执行 `TerminalPlayModeTests` 时，两个旧用例失败：`DirectoryHoverStaysOnItsRowAndSingleClickEntersAvailablePatient` 期望 5 行、实际 4 行；`PassiveTranscriptDoesNotTakeKeyboardFocus` 期望推进 1 次、实际 0 次。在隔离工程恢复 HEAD 版的两个 UI 源文件后，这两个失败以相同断言再次复现，记录为本次之前即存在的问题。[verified: Artifacts/TerminalLayout20261028/playmode-final.xml；Artifacts/TerminalLayout20261028/baseline-unrelated.xml]
- 已检查真实 Unity UI 渲染纹理中的布局、四条回复与滚动提示；额外单独运行记录与应答用例，1/1 通过，并生成最终预览。预览使用测试会谈内容，不代表完整 Campaign 游玩或人工视觉验收；未生成新的 Player／外发包。[verified: Artifacts/TerminalLayout20261028/FocusedVisuals/ark-live-transcript.png；Artifacts/TerminalLayout20261028/visual.xml；Artifacts/TerminalLayout20261028/playmode-verified.xml]

本次 XML、Editor 日志和预览保存在本地忽略目录 [Artifacts/TerminalLayout20261028](../../Artifacts/TerminalLayout20261028/)。[verified: Artifacts/TerminalLayout20261028]

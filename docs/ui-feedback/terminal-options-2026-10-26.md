# 终端长选项、配额数字与医生标点

- 超出单行窗口的玩家应答在鼠标悬停 0.65 秒后向左滚动，每秒移动 24 个终端逻辑像素；尾部停留 1.6 秒后循环。移开、停用或暂停输入时复位，短选项静止，选项编号和点击区域固定。滚动按整数像素移动。[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalChoiceMarquee.cs；../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs AddResponse]
- 对象目录的净化配额改由终端位图字体绘制，数字在原 56×24 区域水平、垂直居中，背景沿用原琥珀色。数值仍来自原运行时视图。[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs DossierQuota/RenderRoster]
- 战地医生 Day1 的 `P04_D1_R6_COMMON_002` 改为“我不记得了。”；修复清单校验原文后生成新的不可变内容包，并更新活动包引用。[verified: ../../Tools/DialogueCampaign/source-repairs.json books.battlefield-doctor.repairs；../../UnityProject/Assets/Resources/MiniGame/ActiveCampaignPackage.asset]

验收方式：在 Unity 中进入对象目录查看配额，再进入含长选项的会谈，将鼠标停在该行上，观察尾部显示、循环和移开复位；进入医生 Day1 对应对白查看句号。当前等待用户人工验收，本轮不构建或压缩 Player。[opinion]

## 本轮验证

- Unity 2022.3.62f3c1 隔离工程编译、原生 Yarn 导入成功，生成 342 个节点。[verified: ../../Artifacts/Unity/TerminalFeedback20261026/import.json nativeNodes；../../Artifacts/Unity/TerminalFeedback20261026/import-editor.log]
- Play Mode 10 项通过，包括长选项延迟滚动、尾部可见、编号固定、短选项静止、移开复位、暂停阻断、原点击回调、目录滚动、处置配额及 CRT 点击映射；测试辅助文件仅位于隔离验证工程。[verified: ../../Artifacts/Unity/TerminalFeedback20261026/playmode.xml]
- 对白转换的 18 项检查通过，完整内容重建 `--check` 通过。扩展运行全部转换工具检查时，结局工具因既有 `self-review` 来源哈希不匹配报错；本轮未改该来源或放宽其校验。[verified: ../../Artifacts/Unity/TerminalFeedback20261026/conversion-check.log；../../Artifacts/Unity/TerminalFeedback20261026/conversion-tests.log；../../Artifacts/Unity/TerminalFeedback20261026/all-conversion-tests.log]
- 本轮未运行独立 .NET SDK 检查、单独 Edit Mode 检查或目标平台构建；没有修改核心行为。现有固定 SDK 8.0.424 未在本机找到。[opinion]

渲染图来自隔离工程的 Unity 终端 RenderTexture，不能替代最终 Player 或人工全流程试玩：[配额](../../Artifacts/Unity/TerminalFeedback20261026/quota-centered-pixel.png)、[选项开头](../../Artifacts/Unity/TerminalFeedback20261026/choice-start.png)、[选项尾部](../../Artifacts/Unity/TerminalFeedback20261026/choice-tail.png)。[verified: ../../Artifacts/Unity/TerminalFeedback20261026/playmode.xml TerminalFeedbackAcceptance]

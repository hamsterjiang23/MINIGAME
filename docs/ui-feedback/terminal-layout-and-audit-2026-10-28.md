# 终端布局与排查记录（2026-10-28）

人员复核共用界面的社会适应度波形移至右上指标下方，宽度472改为248；左侧对话区上移，可视高度76改为142。处置行间距64改为48，按钮图案保留原248×48比例。自我审判使用纯显示的固定q=1，不新增患者信任或改变核心结局条件。[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs 构造函数/Render；../../UnityProject/Assets/MiniGame/Runtime/TerminalAdaptationWaveform.cs SetSelfReviewReading]

排查确认正式对白缺少“遭、症、躁、符、绩”的七档字形，已用现有字体源重建图集。字体语料排除不向玩家显示的provenance.json审计说明，避免说明中的数学减号阻止正式字库生成；活动包、监督员和结局正文仍全部参与覆盖检查。[verified: ../../Tools/TerminalTypography/build_ark_atlas.py corpus；../../Tools/TerminalTypography/test_build_ark_atlas.py；../../docs/terminal-font/coverage.json]

Windows换行转换导致两个Yarn一致性测试误报，并改变已固定内容清单的字节哈希。Yarn文本比较仅统一换行；对已固定清单和来源快照恢复LF，补充Git换行属性。来源期望哈希、对白、数值和活动包绑定均未改写。[verified: ../../src/MiniGame.Presentation.Tests/CampaignPackageTests.cs NormalizeNewlines；../../.gitattributes；Tools/DialogueCampaign的--check实际执行]

## 当前执行结果

- 固定.NET SDK8.0.424：锁定恢复、Release构建通过，0警告/错误；Core160、Presentation322，共482项测试通过。[verified: ../../src/MiniGame.Core.Tests/TestResults/bug-audit-20261028.trx；../../src/MiniGame.Presentation.Tests/TestResults/bug-audit-20261028.trx]
- Campaign smoke、Campaign/监督员/结局产物检查通过；对白工具39项、字体工具5项测试通过。图集10,297个字符/字号组合的覆盖、边界和可见像素检查通过。[verified: 本次终端执行；Tools/TerminalTypography/build_ark_atlas.py --check-only]
- Unity2022.3.23f1c1：波形PlayMode3项、自审UI PlayMode6项通过；恢复按钮比例后波形PlayMode3项复测通过。已查看普通会谈与自审的实际终端渲染图。[verified: ../../Artifacts/UI-waveform-layout/social-adaptation-results.json；../../Artifacts/UI-waveform-layout/self-review-results.json；../../Artifacts/UI-waveform-layout/button-proportions-results.json]
- 字库重建后Unity字体EditMode25项通过。[verified: ../../Artifacts/BugAudit20261028/font-editmode.json]

## 尚未关闭的排查项

全面EditMode曾报告208项中201通过、7失败；返回的失败详情包含字库覆盖和墙面检查，返回详情数量与汇总不完全一致，不能据此宣称全套通过。字库已单独复测通过。墙面4项单独复测为1通过、3失败，涉及当前场景墙纸材质识别和修复网格绑定，尚未修改主场景或确认最终视觉影响。[verified: ../../Artifacts/BugAudit20261028/editmode-initial.json；../../Artifacts/BugAudit20261028/wall-editmode.json]

全面PlayMode运行中，用户转而要求提交和推送，因此取消该次运行；没有最终全套报告。运行日志已出现Day0开场等待失败，仍需区分新主菜单的测试启动前提与实际游戏问题。未新增全套通过、Player构建、IL2CPP或人工完整游玩的结论。[verified: ../../Artifacts/BugAudit20261028/playmode-at-commit.json status=running；本次cancel_tests返回cancelled；当前Editor.log DayZeroOpeningPlayModeTests.Until]

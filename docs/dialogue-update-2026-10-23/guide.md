# 乔纳、薇拉与监督员对白更新

三份用户提供的 `(1).xlsx` 按原字节更新到 `Art/chat_excel` 的现有文件名。正式会谈内容版本为 `1.2.2-development`，监督员继续使用独立的对白资源与 Yarn 项目。来源哈希与逐格差异见 [source-diff.json](reports/source-diff.json)。`[verified: reports/source-diff.json source/destination/newSha256；../../Tools/DialogueCampaign/conversion-config.json contentVersion]`

- 乔纳：13 处措辞更新；原有节点、跳转、信任值与立绘引用不变。原有末句顺序修复和 JN05→JN04 映射的相关源行已逐格复核。`[verified: reports/active-diff.json priest；reports/preflight.json priestRepairChecks]`
- 薇拉：282 处既有文案更新，新增 1 个节点、移除 2 个节点；Day2 的 `P03_D2_R5_OPTION_002` 从 +1 改为 −1，`P03_D2_R6_OPTION_003` 从 0 改为 −1。`[verified: reports/active-diff.json former-ruler；reports/source-diff.json former-ruler.semanticChanges]`
- 监督员：7 句台词和 5 处立绘引用更新，仍为 46 句、四天 16/12/11/7 句。`[verified: reports/source-diff.json supervisor.semanticChanges；../../UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor/Dialogue.json conversations]`

薇拉 `对话流程!H296` 的新版末句与 H297 残留旧句共用 `P03_D3_R6_ACTION_009`。第 297 行与上一版末行完整一致，按项目已有“前面的完整分支优先、后附冲突不启用”规则保留第 296 行，第 297 行完整保存在排除审计中；原始 XLSX 未重写。所有 1360 行原始资料均保留，1294 行启用、66 行排除。转换器新增无修复清单工作簿的重复 ID 拒绝检查，防止同 ID 后行静默覆盖前行。`[verified: ../../Tools/DialogueCampaign/source-repairs.json books.former-ruler；../../Tools/DialogueCampaign/updated_source.py apply_repairs；../../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json sourceRowCount/activeSourceRowCount/excludedSourceRowCount]`

未更新角色的 8 场会谈保持逐字一致。乔纳仍在游戏 Day2/3 使用表 Day1/2，三天日程与所有角色入院规则保持一致。`[verified: reports/preflight.json unchangedMeetings/patientsAndScheduleUnchanged]`

终端字库补入 24 字、7 种字号，共 168 个字形记录；原有 10185 个字形的像素和排版度量逐项保持一致。`[verified: reports/font-verification.json newGlyphSizePairs/preservedGlyphSizePairs/existingGlyphPixelsAndMetricsUnchanged]`

请开始新的检查期查看新版对话；内容修订哈希变化后，旧版会谈存档不能直接恢复。本次不删除存档，也不生成新的游戏安装包。Unity 导入与测试结果见 [verification.json](verification.json)。`[verified: ../../src/MiniGame.Presentation/GameRuntime.cs L678-L681；verification.json playerBuild]`

结局来源清单同步三份源表的哈希与逐格差异；所有结局对白正文保持一致，其他六份来源记录保持一致。`[verified: verification.json endings]`

Verification: Python 30/30；隔离快照 C# 410/410；Unity 导入 EditMode 5/5、监督员 PlayMode 8/8；三套生成内容逐字重建一致、缺字 0。Unity 控制台另记录一次测试收集器 `RunFinished` 的 `TaskCompletionSource.SetResult` 异常，未在本次修改该工具代码；测试结果均已完成并通过。未进行新 Player 构建或游戏画面截图验收。`[verified: verification.json python/dotnet/unity/generatedContentChecks/font/playerBuild/visualScreenshot]`

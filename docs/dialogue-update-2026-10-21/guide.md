# 新版六角色对白

本次将用户提供的六份新版工作簿原样复制到 `Art/chat_excel` 的既有文件名，内容包版本为 `1.1.0-development`。原始资料1437行，启用1295行，排除142行；全部源行仍保存在来源审计中。`[verified: source-imports.json；UnityProject/Assets/MiniGame/Content/Campaign/provenance.json sourceRowCount/activeSourceRowCount/excludedSourceRowCount]`

## 已确认的修复

- Day0：第9行作为主链结束；第10行旧ID的重复末句不再执行。`[verified: Tools/DialogueCampaign/source-repairs.json books.tutorial]`
- 外交官：保留前面的完整三日分支；第281–343行后附冲突内容不启用，共63行。`[verified: Tools/DialogueCampaign/source-repairs.json books.diplomat.excludedRanges]`
- 医生：107行依据旧版对应台词与连线恢复，保留第255行作为净化末句；另补6个处置结束目标。78行重复或后附内容不启用。`[verified: Tools/DialogueCampaign/source-repairs.json books.battlefield-doctor；provenance.json repairs/excludedRows]`
- 神父：将同时指向终点和末句的链接整理为先播放该末句、再到同一终点。`[verified: Tools/DialogueCampaign/source-repairs.json books.priest]`

以上遵循用户确认的“优先完整分支、后附冲突暂不启用、缺失连线参照旧版对应对白”规则。原始 XLSX 未重写；修复清单保存精确输入哈希及旧文本证据。`[verified: 用户确认；Tools/DialogueCampaign/source-repairs.json policy/books]`

## 条件与数值

9场会谈具有信任≥3的特殊分支。源表声明信任≥5的释放措施按该门槛执行；其他会谈保留默认配置。+100、−107均按原值保留。医生 Day1 选项−5与其直接回应+5合并成同次回答的净影响0，两个原值及单元格均保留。`[verified: provenance.json mappings/adaptations；CampaignPackageTests；CampaignConditionalRoundTests]`

旧外交官历史措辞覆盖已清空。神父“你的信徒本应拥有选择。”对应的全局反叛值配置迁移至新ID，原增量0不变。`[verified: Tools/DialogueCampaign/text-variants.json；dependent-id-migrations.json]`

## 使用与兼容

正式入口仍是 `MainRoomCombined` 的 `ActiveCampaignPackage`。旧内容存档的版本哈希与新版不匹配，请开始新的检查期；原存档文件未删除。旧单日演示及其独立内容包保持兼容。`[verified: CampaignPackageCompiler.Compile sourceHash；CampaignConditionalRoundTests；DialoguePackageImporter.ImportPackage]`

共享立绘目录同时保留新版声明与旧单日演示所需的立绘，避免新版未声明的 ML04/ML05 从旧演示中丢失。`[verified: CampaignBuilder.ImportPortraits requiredPortraits；DialoguePackageImporterTests.SharedCatalogRetainsPortraitsRequiredByTheLegacyDemo]`

终端位图字库已按新对白重建，覆盖1471个字符、51485个字形记录，原包字形像素保持不变，缺字检查为0。`[verified: Art/TerminalHtmlMatched/glyph-coverage.json output/preservation/coverage]`

详细检查和运行版路径见 [verification.json](verification.json)。`[verified: verification.json]`

![新版外交官对白的游戏内捕获](dialogue-preview.png)

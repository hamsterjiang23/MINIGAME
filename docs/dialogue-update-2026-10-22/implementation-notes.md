# 2026-10-22 新表对白更新

## Deviations

- 神父源表取消JN05声明但7句仍引用它；用户确认全部映射JN04。只改导入适配，原始XLSX逐字节保留。`[verified: 用户回复；source-repairs.json books.priest 的 user-confirmed-portrait-remap]`

## Discovered edge cases

- 六份文件哈希均变，但小孩、薇拉、医生、外交官所有解析单元格不变；神父仅立绘定义变化；Day0改为9句引导、净化措施和2句回应。`[verified: source-manifest.json；完整逐格比较]`
- 医生和神父旧连接修复的339/2个前提字段重新匹配后才更新固定哈希；外交官63行、医生78行原有冲突排除继续保留，Day0旧重复末句排除已移除。全部1440行均保留在provenance，1299行启用、141行排除。`[verified: reports/source-compatibility.json；新provenance.json]`
- Day0源表最后一句没有下一节点，适配为END_DAY0；不改新表正文或数值。`[verified: Day0对话流程J13；source-repairs.json books.tutorial]`
- 新正文有“嘿、绑、绳、趣”4个缺字，使用现有字库生成器补齐；原包18795个字形像素和度量保持不变。`[verified: glyph-coverage.json preservation/coverage]`
- 只导入并激活新的原生Yarn内容引用，复用当前共享立绘目录；不批量重建图片或场景。`[verified: ReviewedDialogueRefresh.Import 临时验证入口]`

## Questions for review

- 薇拉F157和外交官D106存在旧文案回指不一定选过的问题；用户要求完全按新表统一，正文保持原样，具体反例路径保留在审读报告。`[verified: reports/review-ruler-diplomat.md]`
- 新内容哈希使旧内容检查点不兼容，需开始新的检查期；旧存档不删除。`[verified: CampaignPackageCompiler 内容哈希约束]`
变更偏差：1项，由用户明确确认的JN05→JN04立绘映射。`[verified: verification.json changes]`
最可能回看：源表今后若修正医生/外交官重复块，需重新审读后更新哈希和适配；不得直接放宽漂移保护。`[opinion]`
发现边界：Day0主链增长、首行派生缓存缺失、JN05缺定义、字库缺字、历史修复哈希变化、旧测试硬编码行数。`[verified: 本文、source-manifest.json及reports/]`
最终验证：18项Python、343项.NET、22项Unity EditMode及4项实际Yarn流程验证通过；全部治疗路线的单项成功回调保留，组合运行随后中断，其他路线在干净隔离工程逐项通过。未构建新Player。`[verified: verification.json validation]`
后续先读：guide.md、verification.json和source-manifest.json。`[opinion]`

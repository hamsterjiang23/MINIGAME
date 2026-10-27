# 全流程对白转换

六份日常工作簿由 `conversion-config.json` 的 `file` 字段逐本绑定；监督员和神父结局工作簿使用各自入口。`build_campaign.py` 只读工作簿，生成 V2 内容包、受限 authoring Yarn 和来源审计；Unity 导入器再生成可执行 Yarn。`[verified: conversion-config.json patients[*].file；build_campaign.py build/main]`

```powershell
uv run python Tools/DialogueCampaign/build_campaign.py --smoke
uv run python Tools/DialogueCampaign/build_campaign.py
uv run python Tools/DialogueCampaign/build_campaign.py --check
uv run python Tools/DialogueCampaign/build_supervisor.py --check
uv run python Tools/DialogueCampaign/build_endings.py --check
uv run python -m unittest discover -s Tools/DialogueCampaign -p 'test_*.py'
```

`--smoke` 检查 Day0 与米洛前两天，不写入产物；`--check` 重建完整数据并逐字比较现有产物。生成目录为 `UnityProject/Assets/MiniGame/Content/Campaign/`。`[verified: build_campaign.py main/build]`

转换配置集中在 `conversion-config.json`；信任初值0、默认释放门槛3、每日目标1保留。新版表中明确写出的特殊会谈门槛3和释放门槛5通过每场会谈的元数据生效，不与玩家全局反叛值合并。`[verified: conversion-config.json；updated_source.py build_meeting；CampaignPackageCompiler.CompileMeeting]`

2026-10 新版按表头识别列：五份表使用 I/J/K/L 作为 ID、跳转、信任、立绘，小孩表仍使用 J/K/L/M；条件分别位于 M 或 N。逗号分隔目标形成源表明确指定的选项集合。旧表结构的转换分支保留，但当前正式六表全部使用新版适配。`[verified: updated_source.py source_columns/split_targets；build_campaign.py build]`

`source-repairs.json` 保存本次用户确认的修复：优先保留前面的完整分支，后附冲突块不启用；医生缺失连线依据旧版相同文本及分支恢复。清单固定输入 SHA-256 并保存旧行号、正文和跳转证据，正常重建不依赖 `Artifacts` 备份。源文件变化后必须重新复核清单，不能直接改哈希绕过检查。`[verified: source-repairs.json policy/books；updated_source.py apply_repairs]`

当前 `1.2.2-development` 的 `provenance.json` 保存全部1360行，其中1294行启用、66行排除，日程实际开放13场会谈；提前于角色入院日期的原始内容仍完整留在审计里。声明21个立绘ID、正文引用17个。部分立绘表仍仅有常量数组公式，只恢复常量，不计算任意公式。`[verified: conversion-config.json contentVersion；provenance.json sourceRowCount/activeSourceRowCount/excludedSourceRowCount/meetingCount/declaredPortraits/referencedPortraits；build_campaign.py read_source]`

旧外交官26条历史措辞覆盖已删除，当前 `text-variants.json` 为空；新版正文按源表播放。医生 Day1 选项 K64 的−5和直接回应 K65 的+5合并为同次回答的净影响0，来源值分别保留。神父固定开场+100保留；医生源表−107保留在来源审计，实际回答增量按2026-10-27用户要求调整为−10。`[verified: text-variants.json；trust-adjustments.json；provenance.json mappings/adaptations；test_campaign.py]`

2026-10-27信任修订通过 `trust-adjustments.json` 的64项显式适配生效（当前排期50项、未排期14项），不改原始工作簿或已有图修复。适配在回应增量转移完成后应用，以稳定ID、原文和未适配净值校验前置条件；原始`authoredDelta`、未适配`unadjustedTrustDelta`、最终`trustDelta`与调整理由分别保留。所有条目必须在完整生成中应用，缺失、重复或源内容变化会停止生成。判定门槛、初始信任、处置增量、对白和跳转保持原样。`[verified: build_campaign.build；updated_source.build_updated_meeting；provenance.json trustAdjustmentHash/adaptations]`

2026-10-28新增 `disposition-trust-adjustments.json`，逐项配置46个操作回应（当前排期37个，含教学）。神父各表日释放均扣200，固定开场+100继续保留；其余按对应回应与重复治疗后的感受区分。处置信任在核心接受操作后、回应播放前结算一次，回应期间的社会适应度直接使用更新后读数，重复提交及读档不会再次结算。64项回答适配、正文、图结构和各门槛保持原样。`[verified: disposition-trust-adjustments.json；GameSession.Dispose；CampaignPackageTests.OperationReactionUsesUpdatedTrustExactlyOnceAndRetainsItAcrossRestore]`

2026-10-26按用户截图补充医生 Day1 `P04_D1_R6_COMMON_002` 的句号：“我不记得了。”。此处采用 `source-repairs.json` 的前置文本校验与标点修订，原始工作簿及审计中的原始行保持不变；逐格正文检查仅为这一稳定 ID 接受已确认的句号。`[verified: source-repairs.json books.battlefield-doctor.repairs；test_campaign.py test_every_source_row_is_either_exactly_mapped_or_explicitly_excluded；../../src/MiniGame.Presentation.Tests/CampaignPackageTests.cs EveryActiveSourceRowMapsToUnmodifiedTextAndExcludedRowsRemainAudited]`

V2按会谈单独解析，每会谈最多1024源节点、整包最多8192。V1单日包仍使用原有编译规则；V2包含日程、患者、处置子集、Day0无选项引导和只读文本变体。内容修订哈希覆盖整个V2包，规则或适配文字变化同样使旧存档不兼容。`[verified: CampaignPackageCompiler.Compile/CompileMeeting；DialoguePackageCompiler.Compile]`

C#验收入口为 `CampaignPackageTests` 与 `CampaignConditionalRoundTests`，覆盖活动行/排除行分区、逐格正文、数值、特殊分支边界和版本。原有 `AuthoringPackageTests` 继续验证V1兼容。更新记录见[本次接入说明](../../docs/dialogue-update-2026-10-22/guide.md)。`[verified: src/MiniGame.Presentation.Tests/CampaignPackageTests.cs；src/MiniGame.Presentation.Tests/CampaignConditionalRoundTests.cs]`

2026-10-22更新：Day0新版12行全部启用（9句引导、1项净化、2句回应），末句补END_DAY0。神父7处JN05按用户确认统一映射JN04，原单元格与映射依据保留在审计中；其他Day1–3会谈除该立绘映射外保持一致。`[verified: docs/dialogue-update-2026-10-22/reports/source-compatibility.json]`

2026-10-23更新：医生采用237行新源，复用109项经复核修复，仅排除233/234；229/232/238为三个处置的当前末句并结束Day3，不恢复已删除旧台词。神父按新表62处立绘与5处正文更新，仅剩L178/L179的JN05映射JN04，L67新值JN03保留。外交官仍排除281–343。结局内容和完整来源快照见[结局内容说明](../../docs/ending-content/guide.md)。`[verified: source-repairs.json books；../../docs/ending-content/sources/source-refresh.json sources]`

角色工作簿里的 Day 是该角色到场后的会谈日。正式包 `1.2.2-development` 按 `sourceDay = campaignDay - firstDay + 1` 排期：乔纳在游戏 Day2/3 使用表 Day1/2，阿德里安在游戏 Day3 使用表 Day1。源节点ID、跳转和信任变化保持原表，Yarn的 day 头使用游戏日；教程仍为 Day0，正式流程仍到 Day3。来源审计的 `meetingSchedule` 和逐行映射同时记录 `sourceDay`、`campaignDay`、`scheduled`，超出三天的后续会谈保留审计而不放入可玩包。`[verified: build_campaign.py build；updated_source.py build_updated_meeting；test_campaign.py test_late_arrivals_start_at_their_first_workbook_day_without_rewriting_source_ids/test_later_workbook_visits_remain_audited_but_do_not_extend_the_three_day_campaign]`

三人对白续更：乔纳13处措辞、薇拉282处既有文案与表内分支/信任值、监督员7句台词和5处立绘引用已同步。薇拉末尾同ID旧行297排除，保留296的新句并锁定来源哈希；此前神父修复经逐行复核后继续应用。完整差异与验证见[三人对白更新](../../docs/dialogue-update-2026-10-23/guide.md)。`[verified: source-repairs.json books.priest/books.former-ruler；../../docs/dialogue-update-2026-10-23/reports/active-diff.json；../../docs/dialogue-update-2026-10-23/reports/source-diff.json supervisor.semanticChanges]`

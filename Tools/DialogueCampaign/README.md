# 全流程对白转换

六份 `Art/chat_excel/*.xlsx` 的「对话流程」为正文来源。`build_campaign.py` 只读工作簿，生成 V2 内容包、受限 authoring Yarn 和来源审计；Unity 导入器再生成可执行 Yarn。`[verified: build_campaign.py read_source/build/main]`

```powershell
uv run python Tools/DialogueCampaign/build_campaign.py --smoke
uv run python Tools/DialogueCampaign/build_campaign.py
uv run python Tools/DialogueCampaign/build_campaign.py --check
uv run python -m unittest discover -s Tools/DialogueCampaign -p 'test_*.py'
```

`--smoke` 检查 Day0 与米洛前两天，不写入产物；`--check` 重建完整数据并逐字比较现有产物。生成目录为 `UnityProject/Assets/MiniGame/Content/Campaign/`。`[verified: build_campaign.py main/build]`

转换配置集中在 `conversion-config.json`；信任初值0、默认释放门槛3、每日目标1保留。新版表中明确写出的特殊会谈门槛3和释放门槛5通过每场会谈的元数据生效，不与玩家全局反叛值合并。`[verified: conversion-config.json；updated_source.py build_meeting；CampaignPackageCompiler.CompileMeeting]`

2026-10 新版按表头识别列：五份表使用 I/J/K/L 作为 ID、跳转、信任、立绘，小孩表仍使用 J/K/L/M；条件分别位于 M 或 N。逗号分隔目标形成源表明确指定的选项集合。旧表结构的转换分支保留，但当前正式六表全部使用新版适配。`[verified: updated_source.py source_columns/split_targets；build_campaign.py build]`

`source-repairs.json` 保存本次用户确认的修复：优先保留前面的完整分支，后附冲突块不启用；医生缺失连线依据旧版相同文本及分支恢复。清单固定输入 SHA-256 并保存旧行号、正文和跳转证据，正常重建不依赖 `Artifacts` 备份。源文件变化后必须重新复核清单，不能直接改哈希绕过检查。`[verified: source-repairs.json policy/books；updated_source.py apply_repairs]`

`provenance.json` 保存全部1440行，其中1299行启用、141行排除，并保留122项适配、逐格文本映射与原数值。新版声明21个立绘ID、正文引用15个；医生空立绘字段保持为空，由显示层沿用本角色状态。部分立绘表仍仅有常量数组公式，只恢复常量，不计算任意公式。`[verified: provenance.json sourceRowCount/activeSourceRowCount/excludedSourceRowCount/repairs/mappings/declaredPortraits/referencedPortraits；build_campaign.py read_source]`

旧外交官26条历史措辞覆盖已删除，当前 `text-variants.json` 为空；新版正文按源表播放。医生 Day1 选项 K64 的−5和直接回应 K65 的+5合并为同次回答的净影响0，来源值分别保留。神父+100和医生−107按原表保留。`[verified: text-variants.json；provenance.json mappings/adaptations；test_campaign.py]`

V2按会谈单独解析，每会谈最多1024源节点、整包最多8192。V1单日包仍使用原有编译规则；V2包含日程、患者、处置子集、Day0无选项引导和只读文本变体。内容修订哈希覆盖整个V2包，规则或适配文字变化同样使旧存档不兼容。`[verified: CampaignPackageCompiler.Compile/CompileMeeting；DialoguePackageCompiler.Compile]`

C#验收入口为 `CampaignPackageTests` 与 `CampaignConditionalRoundTests`，覆盖活动行/排除行分区、逐格正文、数值、特殊分支边界和版本。原有 `AuthoringPackageTests` 继续验证V1兼容。更新记录见[本次接入说明](../../docs/dialogue-update-2026-10-22/guide.md)。`[verified: src/MiniGame.Presentation.Tests/CampaignPackageTests.cs；src/MiniGame.Presentation.Tests/CampaignConditionalRoundTests.cs]`

2026-10-22更新：Day0新版12行全部启用（9句引导、1项净化、2句回应），末句补END_DAY0。神父7处JN05按用户确认统一映射JN04，原单元格与映射依据保留在审计中；其他Day1–3会谈除该立绘映射外保持一致。`[verified: docs/dialogue-update-2026-10-22/reports/source-compatibility.json]`

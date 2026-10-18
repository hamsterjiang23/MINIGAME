# 全流程对白转换

六份 `Art/chat_excel/*.xlsx` 的「对话流程」为正文来源。`build_campaign.py` 只读工作簿，生成 V2 内容包、受限 authoring Yarn 和来源审计；Unity 导入器再生成可执行 Yarn。`[verified: build_campaign.py read_source/build/main]`

```powershell
uv run python Tools/DialogueCampaign/build_campaign.py --smoke
uv run python Tools/DialogueCampaign/build_campaign.py
uv run python Tools/DialogueCampaign/build_campaign.py --check
uv run python -m unittest discover -s Tools/DialogueCampaign -p 'test_*.py'
```

`--smoke` 检查 Day0 与米洛前两天，不写入产物；`--check` 重建完整数据并逐字比较现有产物。生成目录为 `UnityProject/Assets/MiniGame/Content/Campaign/`。`[verified: build_campaign.py main/build]`

转换配置集中在 `conversion-config.json`；信任初值0、释放门槛3、每日目标1是开发默认值。固定接待顺序是米洛、薇拉、埃德温、阿德里安、乔纳；神父从Day1开始。特殊轮次没有额外门槛。`[verified: conversion-config.json patients/initialTrust/releaseThreshold/dailyQuota/interpretations]`

外交官F列为玩家回答，G→H→I作为连续回应；只在提交F选项时应用L列增量。缺失选择集合按同角色、日期、轮次补齐。跨日跳转在会谈边界停止，由核心日循环接续；医生和外交官END记录标记为旁白。`[verified: build_campaign.py build_meeting]`

`text-variants.json` 保存26条中性适配文本，原文留在Yarn。运行时依据实际历史调用两个只读条件：`other_patient_exited_yesterday` 与 `asked_diplomat_for_evidence_yesterday`。假条件只替换措辞，保留选项ID、增量、后继。`[verified: text-variants.json；PackageGameContent.ResolveText]`

`provenance.json` 保存994行的缓存字段、补全后的字段、来源文件哈希、每句的原行列、选择归组与跨日边界记录。部分立绘表仅有常量数组公式：只解码这些常量以恢复缺少的缓存值，不计算工作簿公式；记录每个恢复单元格。共27个立绘ID，原文引用24个；AD04、ED02、ED03保留为预留状态。`[verified: provenance.json sourceRowCount/books/mappings/adaptations/declaredPortraits/referencedPortraits]`

V2按会谈单独解析，每会谈最多1024源节点、整包最多8192。V1单日包仍使用原有编译规则；V2包含日程、患者、处置子集、Day0无选项引导和只读文本变体。内容修订哈希覆盖整个V2包，规则或适配文字变化同样使旧存档不兼容。`[verified: CampaignPackageCompiler.Compile/CompileMeeting；DialoguePackageCompiler.Compile]`

C#验收入口是 `CampaignPackageTests`，覆盖原文逐格映射、信任增量、日程、介绍／反应、条件措辞、旁白和内容版本。原有 `AuthoringPackageTests` 验证V1兼容。`[verified: src/MiniGame.Presentation.Tests/CampaignPackageTests.cs；src/MiniGame.Presentation.Tests/AuthoringPackageTests.cs]`

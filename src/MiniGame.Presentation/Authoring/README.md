# 终端对白内容包

`DialoguePackageCompiler.Compile(json)`读取受支持的Yarn子集，生成独立的单日、单人物开发会话、演出段及Unity运行Yarn。它不执行任意Yarn脚本，也不替代Unity里的正式Yarn编译器。[verified: DialoguePackageCompiler.cs `Compile`；RestrictedYarnParser.cs `ParseJump`]

内容包以`yarnSource`为对白真源；`profile`是明确的开发配置，`portraits`是立绘ID目录。示例的初始信任0、释放阈值3和净化配额5对应提供的HTML测试台，不代表正式三天玩法参数。独立示例只包含小孩Day1；`PrototypeRules.Create()`的原开发内容仍独立存在。[verified: docs/assets/prototypes/inspection-terminal-day1-v0.5.2.html `S`；DialoguePackageCompiler.cs `ReadPackage`、`Compile`]

示例位于`UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json`。同目录`terminal-day1.yarn`是嵌入源的逐字镜像，方便查看；导入时以内容包为准，同时提供镜像时应验证一致。不能把两份源分别编辑成不同内容。[opinion]

## 受支持的源结构

每个节点由`title: 稳定ID`、可选类型与其他头字段、`---`、正文、`===`组成。支持头字段为`title`、`kind`、`round`、`portrait`、`patient`、`day`；可选的`patient`与`day`必须匹配包配置。[verified: RestrictedYarnParser.cs `HeaderNames`；DialoguePackageCompiler.cs `ValidateGraph`]

```yarn
title: D1_R1_OPEN1
kind: line
round: R1
portrait: ML01
---
米洛: 这里还要检测吗？
<<jump D1_R1_CHOICE>>
===

title: D1_R1_CHOICE
kind: choice
round: R1
---
-> 别害怕，慢慢说。 #id:D1_R1_O1 #trust:0
    <<jump D1_R1_CLOSING1>>
===
```

普通文本节点恰好包含一行`说话者: 文本`和一个`jump`；选项节点包含一至四个选项，每项带唯一`#id`、整数`#trust`及一个`jump`。处置节点恰好包含治疗、净化、释放三个选项，使用`#disposition:treat|purify|release`，不能叠加`trust`。[verified: RestrictedYarnParser.cs `Parse`]

附带示例保留50个原文节点、6组选项和1个处置节点。`profile.endNode`声明外部终止点，不另造一段对白。正常轮次以`round`分组，各选项必须经过纯文本回应汇合到相同的下一轮入口；最后一轮进入处置。禁止循环、嵌套选项、任意条件、赋值、自定义命令和缺失跳转目标；未使用节点产生可定位警告。[verified: DialoguePackageCompiler.cs `FollowIntro`、`FollowBranch`、`FollowReaction`、`ValidateGraph`；AuthoringPackageTests.cs `SuppliedHtmlDialogueAndCanonicalYarnMatchEveryLineOptionPortraitAndMeasure`]

文本中的反斜线与`#<>[]{}`使用反斜线转义；支持整行`//`注释，拒绝行内注释。源代码保持原样，编译器不会自动修复不支持的结构。失败通过`DialoguePackageException.Diagnostics`返回错误码、节点和源行；JSON或包配置错误没有Yarn行号。[verified: RestrictedYarnParser.cs `ParseTags`、`Ignored`、`Fail`；DialoguePackageCompiler.cs `ReadPackage`]

## 权威状态与运行投影

普通组选项的`trust`提取为核心`ResponseDefinition.TrustDelta`，运行Yarn只提交选项ID。处置仍由核心提交一次，回应段只包含原文，不执行信任或配额修改。[verified: DialoguePackageCompiler.cs `Compile`、`BuildRuntimeYarn`]

`Content.GetSegment(coreNodeId)`提供有序多行及原文节点ID。运行Yarn用`Core_编码ID`对接现有桥接器，每行包含唯一`#line`与`#source:原文节点ID`、`#portrait:立绘ID`。同一收束段可以在多个分支投影中出现，运行行ID仍不同。[verified: DialoguePackageCompiler.cs `BuildRuntimeYarn`]

Unity的Yarn工程只编译生成的运行源；不要把57节点镜像同时加入运行工程。镜像中的外部终止点由包协议声明，独立编译镜像若需验证，应在临时编译作业中加入终止节点。[opinion]

存档仍由核心及宿主维护；内容包不会承诺段内逐行恢复。宿主可在读档后重播纯演出段，必须沿用旧回调作废、回答和处置去重规则。HTML试玩仅运行受支持子集；游戏运行能力需要Unity编译和Play Mode验收。[opinion]

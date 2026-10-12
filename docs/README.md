# 心理恐怖叙事游戏开发文档

我们要完成一款第一人称、2D/3D混合的心理恐怖叙事Mini Game：玩家在员工公寓中通过电脑审查病人，在治疗、净化和释放之间作选择，同时逐渐发现自己也受到审查。当前主Wiki交付目标是**3天内容、5位病人、3种结局**。

## 建议阅读顺序

1. [我们要完成什么游戏](game-overview.md)：目标、日循环、选择、五位病人、结局与资产。
2. [程序需求v0.1](sources/program-requirements-v0-1.md)：交互与对话机制的原始要求。
3. [设计冲突与待定问题](open-questions.md)：需要确认后才能具体开发的规则。
4. [来源与引用索引](source-index.md)：版本、来源、引用链与读取状态。

## 每篇来源各自一份文档

| 类别 | 文档 |
| --- | --- |
| 三个入口 | [主Wiki](sources/game-wiki.md)、[程序需求](sources/program-requirements-v0-1.md)、[游戏梳理](sources/game-concept-outline.md) |
| 五位病人 | [神父](sources/patient-priest.md)、[小孩](sources/patient-child.md)、[女性执政者](sources/patient-former-ruler.md)、[战场医生](sources/patient-battlefield-doctor.md)、[外交官](sources/patient-diplomat.md) |
| 背景与叙事 | [叙事白皮书](sources/narrative-whitepaper.md)、[灵感墙](sources/inspiration-wall.md)、[脑暴方案15](sources/brainstorm-proposal-15.md) |
| 引用工作簿 | [神父人物工作簿](sources/priest-character-workbook.md)、[世界观名词表](sources/world-terminology.md)（两份均已读取并整理） |

各篇保留出处、读取时间、版本、内容概述、完整设计信息、下级引用与待定项。原始读取响应与引用清单保存在 `.source-data` 中；图片在 `assets` 中。两份工作簿和六块画板均已补齐；原文未填的设计明确标记。

## 整理口径

- 内容为中文，文件名为英文。按主Wiki的3天、5人、3结局界定Mini Game目标。
- 多层引用只覆盖本游戏。脑暴收集池只提取方案15，其他团队及往届作品记录为范围外。
- 不同来源的背景、处置、神父和结局差异分开保留，不自行消除矛盾；示例阈值保留示例身份。
- [原有研究资料](../Dependency)保留，提供源码与Unity候选技术参考；其中的原创机制建议不直接变成项目需求。
- 这是设计资料整理，尚未创建或验证可运行的游戏工程。

## 参考源码仓库

`Dependency`研究资料中引用的第三方仓库放在[references](../references)。逐个仓库的原始链接、固定提交、编辑器版本及下载验证结果见[仓库版本记录](source-lock.md)。

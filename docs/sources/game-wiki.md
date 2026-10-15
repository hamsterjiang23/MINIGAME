# 心理恐怖叙事游戏Wiki

## 来源与状态

- 来源：[心理恐怖叙事游戏Wiki](https://moonton.feishu.cn/wiki/SBq3wnQOqifGkHk5h8gcXPV0nb3)
- 读取时间：2026-10-15T19:49:52+08:00（Asia/Shanghai）
- 文档版本：608；底层文档：`COEBdbgoSohH7TxIEsecNU5DnoO`
- 正文已读取；图片、画板和工作簿状态见[来源索引](../source-index.md)。[verified: ../.source-data/game-wiki.json revision_id/content]

## 内容概述

Mini交付标准、日流程、四条结局描述及主角、梦境、对白表、排期和音效引用。[verified: ../.source-data/game-wiki.json content]

## 设计重点

交付标准仍为3天、5位病人、3种结局；正文第四条已改为反叛值触发的自我净化分支。数量、阈值和流程冲突不自行裁决。[verified: ../.source-data/game-wiki.json content「Mini Game交付标准」「结局」] [opinion]

## 完整设计信息（保留本篇口径）

## 心理恐怖叙事游戏Wiki

## Mini Game交付标准

1. **正式全流程相同玩家体验**
2. **3天内容**
3. **5位病人**
4. **3种结局**

## 概念

![概念参考图1](../assets/image-LDkCbIILPoTYjJxKTP6ciaaGn5b.jpg)

#### 世界观

[《微恐剧情》叙事白皮书【施工中】](narrative-whitepaper.md)

#### 通用名词

[《微恐剧情》世界观名词表](world-terminology.md)

#### 病人设定

[【人物】神父](patient-priest.md)

[【人物】小孩](patient-child.md)

[【人物】女性执政者](patient-former-ruler.md)

[【人物】战场医生](patient-battlefield-doctor.md)

[【人物】外交官](patient-diplomat.md)

#### 主角设定

[【人物】主角](player-character.md)

## 玩法

细致全流程：

画板 `IykwwdNvihVgabb0wfKctsnOnJd`：节点与连线见下方画板补充。

#### 全流程（特殊分支不在讨论范畴里）

Day0

电脑开机--转到监控界面--审判病人0--净化病人0--睡觉--做梦

Day1

醒来--监督员对话--仪器检测--介绍任务--审判病人12345--睡觉--做梦

Day2

醒来--监督员对话--仪器检测--介绍指标--审判病人12345--睡觉--做梦

Day3

醒来--监督员对话--仪器检测--介绍指标--审判病人12345--睡觉--做梦

Day4

醒来--监督员对话--仪器检测--不通过--玩家被永远锁在房间里--游戏结束

#### 对话分支

[【对话】小孩v1.1](dialogue-child-v1-1.md)

#### 结局

1. 正常全流程，Day4起床玩家被带走（条件：无）
2. 神父来杀人（条件：释放神父）
3. 没有完成KPI，跳转到Day４（条件：没有完成任意一天KPI）
4. day3梦境结束后，床不亮，电脑亮，玩家交互发现监控里正是自己，最后选项只有“净化”，执行之后进入超自然场景（条件：反叛值max）

## 程序

#### 玩法框架

#### 屏幕画面表现

#### AI管线

## 美术

#### 2D

###### 病房场景

![概念参考图2](../assets/image-NoSjbfFzwohWqxxzXrmc2jNGnOc.png)

###### 病人立绘

###### 梦境

[梦场景](dream-scenes.md)

###### UI

#### 3D

###### 主角场景

![概念参考图3](../assets/image-KJDObSLgpo8mLfx6PlCc9hdxnpb.png)

###### 主角场景物件

#### 音乐&音效马步庭(Mcqueen)

音乐需求：

1. 开始菜单 https://suno.com/s/DEJdjK3NLSIzuZD1
2. 主Gameplay https://suno.com/song/fbb6233d-b8f0-4b16-8143-93716f8d1490
3. 电脑噪音 https://freesound.org/people/8bitmyketison/sounds/699726/
4. 做梦场景（详见**美术--2D--梦境**）
5. 不同分支结局

音效需求：

1. 检测仪扫描https://freesound.org/people/pyth0n11/sounds/706447/
2. 检测仪亮灯https://freesound.org/people/jobro/sounds/33789/
3. 对话弹出https://freesound.org/people/el_boss/sounds/788611/
4. 放映机https://freesound.org/people/juanicotolengo/sounds/265144/
5. 3D场景按键交互https://freesound.org/people/crash_358/sounds/615543/
6.  监控画面按键交互 https://freesound.org/people/Coral_Island_Studios/sounds/277669/

## 参考

| 来源 | 参考纬度 | 内容 (贴链接/图片/视频) |
|-|-|-|
|  |  |  |

## 排期

[minigame排期表](minigame-schedule.md)

本节保留飞书原文引用；用户已明确本次不需要同步排期。[verified: ../.source-data/sync-2026-10-15.json scope_exclusions]

## 本篇待定与版本差异

- 交付标准3种结局与正文四条结局描述并存；正文反叛值写max，画板写大于阈值。[verified: ../.source-data/game-wiki.json content「结局」；../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14]
- 正文Day4含检测不通过锁房，结局第一条却是正常Day4被带走；画板将前一天KPI失败称为常规结局。保留差异，见[待定问题](../open-questions.md)。[verified: ../.source-data/game-wiki.json content「全流程」「结局」；../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14]
- 程序三栏仍空；主角、梦场景和对白现在通过下级来源给出内容，不能再归为全部空白。[verified: ../.source-data/game-wiki.json content「程序」「主角设定」「对话分支」「梦境」]

## 画板补充（2026-10-15重读）

共14个节点；新节点在每天开始前检查结局，梦后阶段增加场景压暗与床聚光灯。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14/o1:12]

![最新流程画板](../assets/board-IykwwdNvihVgabb0wfKctsnOnJd-2026-10-15.jpg)

### 节点文字

- `o1:13`：开始菜单——系统加载界面——直接进入审判系统——和特殊npc对话——“净化”该npc——关机进入房间——舞台聚光灯打向床，引导交互——按E上床——醒来。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:13]
- `o1:12`：在梦播完之后，恢复自由移动，场景压暗，给床打舞台聚光灯，只能和床交互。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:12]
- `o1:1`：开始菜单。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:1]
- `o1:11`：开始一天，开门和监督员对话、操作电脑工作。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:11]
- `o43:14`：每天开始前，读取条件检测是否进结局。；若释放神父：神父结局；若天数大于等于4且反叛值不足：常规结局；若天数大于等于4且反叛值大于阈值：梦结局；若未完成前一天kpi：常规结局。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14]
- `o1:3`：Intro。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:3]

### 连线

- `c1:16`：`o1:3` → `无节点ID`。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c1:16.connector]
- `c1:14`：`o1:11` → `o1:12`：和床交互，进入梦场景。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c1:14.connector]
- `c1:17`：`o1:1` → `o1:13`。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c1:17.connector]
- `c1:1`：`o1:1` → `o1:3`。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c1:1.connector]
- `c1:13`：`o1:3` → `o1:11`：黑屏、敲门。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c1:13.connector]
- `c43:20`：`o43:14` → `o1:11`。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c43:20.connector]
- `c1:15`：`o1:12` → `o1:11`：和床交互后，进入黑屏，敲门声响起，按E起床。[verified: ../.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes c1:15.connector]

# 来源、引用关系与读取状态

本次同步：2026-10-15（Asia/Shanghai）。逐项版本、完整快照、本地原件与同步边界见[同步记录](feishu-sync-2026-10-15.md)。2026-10-12原始manifest和验收记录已归档至`.source-data/history/2026-10-12/`。[verified: .source-data/manifest.json retrieved_at；.source-data/history/2026-10-12/manifest.json retrieved_at]

## 覆盖范围与状态

- 原有11篇正文重新读取；新增主角、梦场景两篇，共13篇。主Wiki481→608、梳理3633→3638、神父797→800；其他8篇版本未变。[verified: .source-data/sync-2026-10-15.json changes；.source-data/manifest.json documents]
- 3份可读工作簿共12个子表已完整保存CSV及坐标/公式JSON；用户提供的小孩Excel原件也已归档，另两份工作簿原始Excel导出仍受限。[verified: .source-data/manifest.json workbooks；.source-data/sync-2026-10-15.json access_gaps/xlsx_originals_available]
- 用户提供的Day1终端HTML v0.5.2已原样归档，内嵌Day1数据与小孩Excel主表逐项一致。[verified: .source-data/local-deliveries-2026-10-15.json files；.source-data/inspection-terminal-day1-v0.5.2.json data_matches_xlsx_day1]
- 6块画板重读，共169节点；主流程12→14节点，并新增每日开始前结局检查，其他5块节点文字与连线关系未发现实质变化。[verified: .source-data/manifest.json whiteboards；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes]
- 新保存两张场景概念图、12张梦境预览与最新主流程画板图；12张梦境图为阅读预览接口返回内容，不冒充原始制作资源。[verified: .source-data/sync-2026-10-15.json media] [opinion]
- 排期已按用户要求排除，不再作为权限或完成度缺口。已知引用链与本轮两份本地附件已同步；知识空间目录仍不可读，不宣称整个空间或所有工作簿原件已下载。[verified: .source-data/sync-2026-10-15.json scope_exclusions/completion_scope/access_gaps] [opinion]

## 逐篇来源

| 本地文档 | 原始来源 | 版本 | 底层文档/工作簿 | 状态 |
| --- | --- | --- | --- | --- |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | [飞书原文](https://moonton.feishu.cn/wiki/SBq3wnQOqifGkHk5h8gcXPV0nb3) | 608 | `COEBdbgoSohH7TxIEsecNU5DnoO` | 已读取并整理 |
| [程序需求v0.1](sources/program-requirements-v0-1.md) | [飞书原文](https://moonton.feishu.cn/wiki/NwUdwhdB8imDWFkaUrccsgVunre) | 357 | `SMxDdzQKLoZ6FvxdfOmcXGsGnTf` | 已读取并整理 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [飞书原文](https://moonton.feishu.cn/wiki/ZNB2wgII0ioqNbkz2rHcfQwhnKU) | 3638 | `M6SNd81CXoct4ixz4xmcGzEgnuf` | 已读取并整理 |
| [【人物】神父](sources/patient-priest.md) | [飞书原文](https://moonton.feishu.cn/wiki/A5XuwDTTWijkGGkFhqUcxg8qnSg) | 800 | `UfEKdfHUMombtJxR4B4cmPt1n1f` | 已读取并整理 |
| [【人物】小孩](sources/patient-child.md) | [飞书原文](https://moonton.feishu.cn/wiki/DiI4weVOxiAknuku0uzchcf5nyc) | 32 | `ZXdPd225DoiL8axOfcyc1xH0n4d` | 已读取并整理 |
| [【人物】女性执政者](sources/patient-former-ruler.md) | [飞书原文](https://moonton.feishu.cn/wiki/Fkq1wdjODiwex7k2j4bcJrxcnEf) | 29 | `C3EYdT1B2okikLx1sbCc7lq4nbb` | 已读取并整理 |
| [【人物】战场医生](sources/patient-battlefield-doctor.md) | [飞书原文](https://moonton.feishu.cn/wiki/WWqCwCFwxisowlklgA0cnwtGnBf) | 12 | `VnLedKjLkov0SrxzGSLcCK1JnNh` | 已读取并整理 |
| [【人物】外交官](sources/patient-diplomat.md) | [飞书原文](https://moonton.feishu.cn/wiki/HwQNwN2fgiQoIrkEiLjc82S8ncg) | 9 | `HkKddpcCaopqRZxyzeXcRBSpnSg` | 已读取并整理 |
| [《微恐剧情》叙事白皮书【施工中】](sources/narrative-whitepaper.md) | [飞书原文](https://moonton.feishu.cn/docx/QyWHduBu4oc8gjxwDSMc22LDnTY) | 1513 | `QyWHduBu4oc8gjxwDSMc22LDnTY` | 已读取并整理 |
| [灵感墙](sources/inspiration-wall.md) | [飞书原文](https://moonton.feishu.cn/wiki/VOF4we0RXiJK9Zk8ZtUcivFxnVc) | 93 | `U3JXdhfLqoKmZZxJHALcMSVuncg` | 已读取并整理 |
| [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | [飞书原文](https://moonton.feishu.cn/wiki/Q4rnwla07ijZGnkkBO6cMCb0nUb) | 6436 | `FqeUdMSjTokrVpxYOXBcI1M9nXg` | 已读取并整理 |
| [神父人物设计引用工作簿](sources/priest-character-workbook.md) | [工作簿](https://moonton.feishu.cn/sheets/S0sksqaiGhWn4StVtXVc9eBKnMd) | 25 | `S0sksqaiGhWn4StVtXVc9eBKnMd` | 已读取；6个子表 |
| [《微恐剧情》世界观名词表](sources/world-terminology.md) | [工作簿](https://moonton.feishu.cn/sheets/NAhrs6l5yhwjUptoHspcq1WrnGd) | 300 | `NAhrs6l5yhwjUptoHspcq1WrnGd` | 已读取；1个子表 |
| [【人物】主角](sources/player-character.md) | [飞书原文](https://moonton.feishu.cn/wiki/AEWUwUL51i6aXJkz8R3cpcG9nNg) | 6 | `Sr9Ed2hlAo8gk6xJu0McX22Insc` | 已读取并整理 |
| [梦场景](sources/dream-scenes.md) | [飞书原文](https://moonton.feishu.cn/wiki/AqN6wASHpihwCqkmK0ecbEI5nvf) | 191 | `Yn0odFmW1or2lTxyAQLctLc9nBg` | 已读取并整理；12张图片预览 |
| [【对话】小孩v1.1](sources/dialogue-child-v1-1.md) | [工作簿](https://moonton.feishu.cn/sheets/FUlBs4PJYhiOoftbZ0mcj1Myn1e) | 31 | `FUlBs4PJYhiOoftbZ0mcj1Myn1e` | 5个子表已读；用户提供Excel原件已归档 |
| [minigame排期表](sources/minigame-schedule.md) | [飞书原文](https://moonton.feishu.cn/wiki/Bg8EwuBeMirJBbkmPbucpSmNn6b) | 本次排除 | 底层类型为sheet | 按用户要求不读取/同步 |
| [Day1终端交互原型](sources/inspection-terminal-day1-prototype.md) | [用户提供HTML原件](assets/prototypes/inspection-terminal-day1-v0.5.2.html) | 0.5.2 | 本地原件 | 源码、语法及Excel数据对应已核对；未做浏览器运行验收 |

## 多层引用关系

| 引用方 | 下级引用 | 结果 |
| --- | --- | --- |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | [【人物】神父](sources/patient-priest.md) | 已整理；重复/循环引用复用同一份文件 |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | [【人物】小孩](sources/patient-child.md) | 已整理；重复/循环引用复用同一份文件 |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | [【人物】女性执政者](sources/patient-former-ruler.md) | 已整理；重复/循环引用复用同一份文件 |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | [【人物】战场医生](sources/patient-battlefield-doctor.md) | 已整理；重复/循环引用复用同一份文件 |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | [【人物】外交官](sources/patient-diplomat.md) | 已整理；重复/循环引用复用同一份文件 |
| [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | 画板 `IykwwdNvihVgabb0wfKctsnOnJd` | 已读取，文本和连线归入该来源文档；截图保存为本地素材 |
| [程序需求v0.1](sources/program-requirements-v0-1.md) | 画板 `FfORwsEwwhfH9cbHbpQc8OMsnWe` | 已读取，文本和连线归入该来源文档；截图保存为本地素材 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [灵感墙](sources/inspiration-wall.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [《微恐剧情》叙事白皮书【施工中】](sources/narrative-whitepaper.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [《微恐剧情》世界观名词表](sources/world-terminology.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [【人物】小孩](sources/patient-child.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [【人物】女性执政者](sources/patient-former-ruler.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [【人物】战场医生](sources/patient-battlefield-doctor.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [【人物】外交官](sources/patient-diplomat.md) | 已整理；重复/循环引用复用同一份文件 |
| [《一个无敌的名字》梳理](sources/game-concept-outline.md) | [【人物】神父](sources/patient-priest.md) | 已整理；重复/循环引用复用同一份文件 |
| [【人物】神父](sources/patient-priest.md) | [神父人物设计引用工作簿](sources/priest-character-workbook.md)，子表 `Zl7v08` | 已读取，归入同一工作簿文档 |
| [【人物】神父](sources/patient-priest.md) | [神父人物设计引用工作簿](sources/priest-character-workbook.md)，子表 `vpZ2a1` | 已读取，归入同一工作簿文档 |
| [【人物】神父](sources/patient-priest.md) | [神父人物设计引用工作簿](sources/priest-character-workbook.md)，子表 `uCxhWZ` | 已读取，归入同一工作簿文档 |
| [【人物】小孩](sources/patient-child.md) | 无下级文档引用 | 正文读取后已检查 |
| [【人物】女性执政者](sources/patient-former-ruler.md) | 无下级文档引用 | 正文读取后已检查 |
| [【人物】战场医生](sources/patient-battlefield-doctor.md) | 无下级文档引用 | 正文读取后已检查 |
| [【人物】外交官](sources/patient-diplomat.md) | 无下级文档引用 | 正文读取后已检查 |
| [《微恐剧情》叙事白皮书【施工中】](sources/narrative-whitepaper.md) | [《微恐剧情》世界观名词表](sources/world-terminology.md) | 已整理；重复/循环引用复用同一份文件 |
| [灵感墙](sources/inspiration-wall.md) | 画板 `CgLkw84KhhxEP4bnCuRc3WMBnGb` | 已读取，文本和连线归入该来源文档；截图保存为本地素材 |
| [灵感墙](sources/inspiration-wall.md) | 画板 `PZ9dwGlCPhFQKxbP7u5c7MAwnMd` | 已读取，文本和连线归入该来源文档；截图保存为本地素材 |
| [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 画板 `A6dhwuJh2hsZXRbPDeFcOU9Cnwb` | 已读取，文本和连线归入该来源文档；截图保存为本地素材 |
| [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 画板 `JXADweudchbt1ob2nD9ckTVMn1e` | 已读取，文本和连线归入该来源文档；截图保存为本地素材 |

## 2026-10-12补充读取证据（历史）

| 来源 | 版本/节点 | 读取范围与结果 |
| --- | --- | --- |
| [神父人物设计引用工作簿 / Zl7v08](sources/priest-character-workbook.md) | `Zl7v08` | `A1:B7`，7行非空，未截断 |
| [神父人物设计引用工作簿 / uCxhWZ](sources/priest-character-workbook.md) | `uCxhWZ` | `A1:B5`，5行非空，未截断 |
| [神父人物设计引用工作簿 / RrqUWL](sources/priest-character-workbook.md) | `RrqUWL` | `A1:B5`，5行非空，未截断 |
| [神父人物设计引用工作簿 / jhiGax](sources/priest-character-workbook.md) | `jhiGax` | `A1:C6`，0行非空，未截断 |
| [神父人物设计引用工作簿 / vpZ2a1](sources/priest-character-workbook.md) | `vpZ2a1` | `A1:C1`，1行非空，未截断 |
| [神父人物设计引用工作簿 / Sheet1](sources/priest-character-workbook.md) | `46644e` | `A1:T200`，0行非空，未截断 |
| [《微恐剧情》世界观名词表 / 名词表](sources/world-terminology.md) | `f00ecc` | `A1:F18`，18行非空，未截断 |
| [game-wiki / 白板](sources/game-wiki.md) | `IykwwdNvihVgabb0wfKctsnOnJd`，历史12节点 | 全部节点已返回，无新文档链接，画板图像已检查 |
| [program-requirements-v0-1 / 白板](sources/program-requirements-v0-1.md) | `FfORwsEwwhfH9cbHbpQc8OMsnWe`，3节点 | 全部节点已返回，无新文档链接，画板图像已检查 |
| [inspiration-wall / 白板](sources/inspiration-wall.md) | `CgLkw84KhhxEP4bnCuRc3WMBnGb`，68节点 | 全部节点已返回，无新文档链接，画板图像已检查 |
| [inspiration-wall / 白板](sources/inspiration-wall.md) | `PZ9dwGlCPhFQKxbP7u5c7MAwnMd`，58节点 | 全部节点已返回，无新文档链接，画板图像已检查 |
| [brainstorm-proposal-15 / 白板](sources/brainstorm-proposal-15.md) | `A6dhwuJh2hsZXRbPDeFcOU9Cnwb`，22节点 | 全部节点已返回，无新文档链接，画板图像已检查 |
| [brainstorm-proposal-15 / 白板](sources/brainstorm-proposal-15.md) | `JXADweudchbt1ob2nD9ckTVMn1e`，4节点 | 全部节点已返回，无新文档链接，画板图像已检查 |

## 2026-10-12本地素材（历史）

| 素材 | 所属来源 | 状态 |
| --- | --- | --- |
| [参考图片 `GbgYbfa14oc45ZxE1c5cZBYenxf`](assets/image-GbgYbfa14oc45ZxE1c5cZBYenxf.png) | [程序需求v0.1](sources/program-requirements-v0-1.md) | 已保存并检查 |
| [参考图片 `LDkCbIILPoTYjJxKTP6ciaaGn5b`](assets/image-LDkCbIILPoTYjJxKTP6ciaaGn5b.jpg) | [心理恐怖叙事游戏Wiki](sources/game-wiki.md) | 已保存并检查 |
| [参考图片 `Wwwbb2CWAo0coPxgddgcJGSMnpd`](assets/image-Wwwbb2CWAo0coPxgddgcJGSMnpd.jpg) | [程序需求v0.1](sources/program-requirements-v0-1.md) | 已保存并检查 |
| [参考图片 `BjDlbnohlo4RSsxIC5Ycr36Rnif`](assets/image-BjDlbnohlo4RSsxIC5Ycr36Rnif.jpg) | [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 已保存并检查 |
| [参考图片 `CH9Qbm8NeouPSjxxstccivxFnhs`](assets/image-CH9Qbm8NeouPSjxxstccivxFnhs.jpg) | [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 已保存并检查 |
| [参考图片 `D4VQbtcFooHauoxTz6lcwVXgnIf`](assets/image-D4VQbtcFooHauoxTz6lcwVXgnIf.png) | [【人物】神父](sources/patient-priest.md) | 已保存并检查 |
| [参考图片 `DtT7bokG2oe03XxRnyZcMOYunGU`](assets/image-DtT7bokG2oe03XxRnyZcMOYunGU.jpg) | [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 已保存并检查 |
| [参考图片 `E9JgbQZCWoHM85xPmjgcVg0LnEG`](assets/image-E9JgbQZCWoHM85xPmjgcVg0LnEG.jpg) | [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 已保存并检查 |
| [参考图片 `Rq1ob6tp4odEbxxmHSCcsleinSf`](assets/image-Rq1ob6tp4odEbxxmHSCcsleinSf.jpg) | [【新月26】Mini game 脑暴玩法收集池](sources/brainstorm-proposal-15.md) | 已保存并检查 |
| [参考图片 `Tz7SbqCdSohcnZxJ3jBcqVT5ndh`](assets/image-Tz7SbqCdSohcnZxJ3jBcqVT5ndh.png) | [【人物】神父](sources/patient-priest.md) | 已保存并检查 |
| [参考图片 `UjUFb004tonI5FxgCHycSw5on7g`](assets/image-UjUFb004tonI5FxgCHycSw5on7g.png) | [【人物】神父](sources/patient-priest.md) | 已保存并检查 |
| [参考图片 `VRaLbCPGUoirIexlQ2Qck8TBng0`](assets/image-VRaLbCPGUoirIexlQ2Qck8TBng0.png) | [【人物】神父](sources/patient-priest.md) | 已保存并检查 |
| [完整画板 `IykwwdNvihVgabb0wfKctsnOnJd`](assets/board-IykwwdNvihVgabb0wfKctsnOnJd.jpg) | [game-wiki](sources/game-wiki.md) | 已保存并检查；图像为2560×2560的画板截图 |
| [完整画板 `FfORwsEwwhfH9cbHbpQc8OMsnWe`](assets/board-FfORwsEwwhfH9cbHbpQc8OMsnWe.jpg) | [program-requirements-v0-1](sources/program-requirements-v0-1.md) | 已保存并检查；图像为2560×2560的画板截图 |
| [完整画板 `CgLkw84KhhxEP4bnCuRc3WMBnGb`](assets/board-CgLkw84KhhxEP4bnCuRc3WMBnGb.jpg) | [inspiration-wall](sources/inspiration-wall.md) | 已保存并检查；图像为2560×2560的画板截图 |
| [完整画板 `PZ9dwGlCPhFQKxbP7u5c7MAwnMd`](assets/board-PZ9dwGlCPhFQKxbP7u5c7MAwnMd.jpg) | [inspiration-wall](sources/inspiration-wall.md) | 已保存并检查；图像为2560×2560的画板截图 |
| [完整画板 `A6dhwuJh2hsZXRbPDeFcOU9Cnwb`](assets/board-A6dhwuJh2hsZXRbPDeFcOU9Cnwb.jpg) | [brainstorm-proposal-15](sources/brainstorm-proposal-15.md) | 已保存并检查；图像为2560×2560的画板截图 |
| [完整画板 `JXADweudchbt1ob2nD9ckTVMn1e`](assets/board-JXADweudchbt1ob2nD9ckTVMn1e.jpg) | [brainstorm-proposal-15](sources/brainstorm-proposal-15.md) | 已保存并检查；图像为2560×2560的画板截图 |

## 范围外与外部参考

- 脑暴收集池其他16个游戏方案及其文档、往届展示和素材不纳入递归。
- 音乐、视频和参考游戏链接保留在来源正文中；参考媒体不表示本项目已有成品资源或实现。
- 灵感树中的19天以上结构、六类结局和伪人机制属于早期设想，保留并标注，不扩大当前Mini目标。

跳过的其他项目文档引用：

- 【新月计划】什么！校招生们竟做出了这样的产品？（Mini 立项作品展示）：`LmBfdMNqIoTcCjxQfU1cRY3NnTh`，属于方案15以外的内容。
- 【新月计划】快来围观，新脑子的奇妙游戏世界！（24届校招Mini 立项作品展示）：`VdEkwHaBpiIUL7krxFicQBzknsg`，属于方案15以外的内容。
- 新生力量登场！2025届校招Mini立项游戏作品大公开：`CliRwLrhziXhfck6aRBco4pHnZT`，属于方案15以外的内容。
- 呼啸山庄_MiniGame策划案_v1.1：`SXjvdT5b3oaviLx25BCcS5XCnEf`，属于方案15以外的内容。

## 历史访问问题与当前边界

以下2026-10-12访问记录不覆盖本次新增来源；当前排期排除及原件取得状态以上方状态及[同步记录](feishu-sync-2026-10-15.md)为准。[verified: .source-data/sync-2026-10-15.json access_gaps]

- 补充授权前，工作簿和白板缺少对应scope；授权后六块白板与神父工作簿读取成功。
- 世界观名词表曾返回40403/601124003（资源阅读权限不足）；用户申请到权限后重读成功，当前没有该读取缺口。
- 主Wiki所在知识空间的全目录曾返回131006；没有在无权限状态下反复读取或替换身份。单篇引用链均已核实。
- `.source-data`保存正文、工作簿、画板原始读取响应和manifest，`.source-data/validation.json`记录本地验收结果。
- 原文留空、示例阈值、候选名称和版本差异见[待定问题](open-questions.md)。资料整理完成不等于设计已全部定稿或游戏已实现。

## 2026-10-15新增引用链

- 主Wiki→叙事白皮书、世界观名词表：来源以前已整理，本次补入主Wiki正文链接。[verified: .source-data/game-wiki.json content「世界观」「通用名词」]
- 主Wiki→主角、梦场景：两篇正文已读取；未发现新的下级文档引用。[verified: .source-data/player-character.json content；.source-data/dream-scenes.json content]
- 主Wiki→小孩v1.1→全部5子表：已按完整物理矩形读取，来源入口原本锚定“配置方式”，没有只下载该子表。[verified: .source-data/game-wiki.json content「对话分支」；.source-data/dialogue-child-v1-1.json sheets]
- 主Wiki→排期：用户明确排除，仅保留引用，不再要求读取。[verified: .source-data/sync-2026-10-15.json scope_exclusions]
- 用户Excel→Day1 HTML：主表中的Day1台词、选项、跳转、立绘ID、信任变化与HTML内嵌数据一致；原件均有哈希。[verified: .source-data/inspection-terminal-day1-v0.5.2.json data_matches_xlsx_day1；.source-data/local-deliveries-2026-10-15.json files]

# 飞书设计来源同步：2026-10-15

同步主Wiki及其本游戏引用链，保留原文、版本、表格坐标和历史快照。本次只更新资料与文档，没有修改游戏行为。[verified: .source-data/manifest.json documents/workbooks；.source-data/sync-2026-10-15.json changes] [opinion]

## 本轮补充：用户原件与范围调整

- 排期已按用户明确要求排除，不读取、下载或同步其内容；主Wiki原始引用仍保留。[verified: .source-data/sync-2026-10-15.json scope_exclusions]
- [小孩Excel原件](assets/workbooks/dialogue-child-v1-1/dialogue-child-v1-1.xlsx)和[Day1终端HTML原件](assets/prototypes/inspection-terminal-day1-v0.5.2.html)均已按原始字节归档，SHA256与用户下载文件一致；[终端参考说明](sources/inspection-terminal-day1-prototype.md)列出界面、交互与Unity接入边界。[verified: .source-data/local-deliveries-2026-10-15.json files]
- Excel主表与飞书值一致；角色文本表A2、玩家选项表A2、配置方式A1缺少公式缓存，其余可比较值一致。原件未重算或改写。[verified: .source-data/dialogue-child-v1-1.xlsx-inspection.json sheets/all_comparable_values_match_api/formula_recalculation_run]
- HTML的50个文本节点、6组选项、20个对话选项与3种处置同Excel主表Day1逐项一致。静态图检查覆盖1296条会谈选择组合，无缺失跳转和循环；JavaScript语法通过。这不是浏览器或Unity运行证明。[verified: .source-data/inspection-terminal-day1-v0.5.2.json nodes/options/data_matches_xlsx_day1/static_graph_check/static_checks] [opinion]

## 已核实的变化

- **主Wiki481→608**：新增主角、梦场景、小孩v1.1对白、排期引用，补上白皮书与名词表链接；增加病房和主角场景概念图、六条音效链接。[verified: .source-data/game-wiki.json content；.source-data/history/2026-10-12/game-wiki.json content]
- **结局不再只有三条有效描述**：补明正常Day4起床被带走、释放神父触发袭击、任意一天KPI失败跳Day4；第四条由“r”变为反叛值触发的自我净化与超自然场景。交付标准仍写3种，需保留冲突。[verified: .source-data/game-wiki.json content「Mini Game交付标准」「结局」；.source-data/history/2026-10-12/game-wiki.json content「结局」] [opinion]
- **主流程画板12→14节点**：梦后场景压暗、床聚光灯；新增每日开始前结局检查，包括释放神父、天数与反叛值、前一天KPI。共6块画板169节点重新读取。[verified: .source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o1:12/o43:14；.source-data/manifest.json whiteboards]
- **游戏梳理3633→3638**：将高层“已经意识到”检测局限改为“明知”，并更新神父引用标题。[verified: .source-data/game-concept-outline.json content；.source-data/history/2026-10-12/game-concept-outline.json content]
- **神父797→800**：标题改为“【人物】神父”，一处图片说明变化；人物机制正文未发现实质改变。[verified: .source-data/patient-priest.json content；.source-data/history/2026-10-12/patient-priest.json content] [opinion]
- **新增[主角专篇](sources/player-character.md)，6版**：举报妹妹→记忆断层→体制奋斗的同一因果链，固定过去但不固定玩家立场；姓名未定，Demo重点Day0–3，Day4–5作为主线方向保留。[verified: .source-data/player-character.json revision_id/content「背景」「玩家角色边界」「设计约束与待定项」]
- **新增[梦场景](sources/dream-scenes.md)，191版**：Day0鲸尸海滩、Day1无尽长路、Day2枯花田、Day3受缚待审；四条音乐参考、12张图片，图片已保存阅读预览。[verified: .source-data/dream-scenes.json revision_id/content；.source-data/sync-2026-10-15.json media]
- **新增[小孩v1.1对白工作簿](sources/dialogue-child-v1-1.md)，31版**：完整5个子表，包含三天实际对白、节点、跳转、信任变化、立绘ID与配置规则。[verified: .source-data/dialogue-child-v1-1.json structure.data/sheets]
- 原有其他8篇正文及神父工作簿25版、名词表300版版本未变；本次仍重新读取正文、全部子表和画板，未仅凭版本跳过引用检查。[verified: .source-data/manifest.json documents/workbooks；.source-data/history/2026-10-12/manifest.json documents/workbooks]

## 完整表格读取与下载

已读3份工作簿、12个子表，包含隐藏行列与空白区域。每个子表都按物理行列上界读取，核对`actual_range`、`row_indices`、`col_indices`、`has_more=false`和`truncated=false`，不以首段连续区域当作整表。[verified: .source-data/manifest.json workbooks；.source-data/workbook-cells/dialogue-child-v1-1.0ADfgL.json data]

- [小孩v1.1](sources/dialogue-child-v1-1.md)：对话流程`A1:T500`、角色文本表`A1:T500`、玩家选项表`A1:T500`、立绘ID`A1:T200`、配置方式`A1:T200`；对应231、162、70、6、47个非空行。[verified: .source-data/dialogue-child-v1-1.json sheets]
- [神父工作簿](sources/priest-character-workbook.md)：全部6个子表，原文空白区保持空白。[verified: .source-data/priest-character-workbook.json sheets]
- [世界观名词表](sources/world-terminology.md)：完整物理范围`A1:P182`，非空内容至第18行。[verified: .source-data/world-terminology.json sheets]
- 全部CSV在[工作簿下载目录](assets/workbooks)，含真实坐标与公式的原始响应在[单元格证据目录](.source-data/workbook-cells)；CSV为值快照，不能替代公式原件。[verified: .source-data/manifest.json workbooks.sheets.csv/workbooks.sheets.cells_snapshot] [opinion]
- `table-get`返回范围与实际行数不一致：对话流程报`A1:M231`却只返回11行；本次归档采用完整单元格读取，不使用该结果作为完整表。[verified: .source-data/sync-2026-10-15.json table_get_observation；.source-data/workbook-cells/dialogue-child-v1-1.0ADfgL.json data]

## 需要裁决或接入的变化

- **结局数量与阈值**：3种交付对4条正文；正文“反叛值max”对画板“反叛值大于阈值”。不直接将列举顺序当作优先级。[verified: .source-data/game-wiki.json content「结局」；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14] [opinion]
- **KPI失败与普通结局**：正文写任意一天未完成跳Day4，Day4流程写锁房；画板写前一天KPI未完成进入“常规结局”。当前原型到三天结束才判断，尚未按新来源改动。[verified: .source-data/game-wiki.json content「全流程」「结局」；.source-data/board-IykwwdNvihVgabb0wfKctsnOnJd.json data.nodes o43:14；../src/README.md「开发默认规则」]
- **对白模型与数值**：小孩表为R1–R6对话、R7措施轮，真分支信任−3/0/+3；原型为4普通轮与±10。来源已有正式表，不等于已有导入器或已接入原型。[verified: .source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第10–11、20、30–33行；planner-acceptance-guide.md「验收前先读这几条」] [opinion]
- **四种隐藏数值名称**：疯狂值、偏离指数、狂热值、反叛值不自行合并。主角要求梦境是记忆回流，新增结局又写超自然场景；应明确普通梦境与结局分支边界。[verified: open-questions.md Q10/Q14；.source-data/player-character.json content「梦境与情绪表现」；.source-data/game-wiki.json content「结局」] [opinion]

## 同步范围与获取限制

- 排期按用户要求排除，不再列为待补权限。已知引用链和本轮提供的两份附件已完成资料同步，运行实现仍单独记录。[verified: .source-data/sync-2026-10-15.json scope_exclusions/content_sync_complete/completion_scope]
- 小孩原始Excel现已通过用户提供文件取得；神父工作簿、名词表的内容已完整保存CSV和公式JSON，这两份原始Excel的API导出仍返回1069902。[verified: .source-data/sync-2026-10-15.json xlsx_originals_available/access_gaps]
- 知识空间目录仍返回131006，不宣称盘点了整个空间或发现了所有未链接的对白表。[verified: .source-data/sync-2026-10-15.json access_gaps/whole_wiki_space_audited] [opinion]
- HTML浏览器预览被本地文件协议安全策略拦截，本轮只做源码、数据和语法检查；不以静态图检查替代运行与视觉验收。[verified: .source-data/inspection-terminal-day1-v0.5.2.json runtime_browser_test] [opinion]

## 连带更新与验证边界

已更新来源正文、来源索引、游戏总览、待定项、项目状态、策划验收及根目录、核心、Unity和研究路线入口；历史481版主Wiki及原manifest、validation保留在[历史快照](.source-data/history/2026-10-12)。[verified: .source-data/history/2026-10-12/manifest.json retrieved_at；.source-data/manifest.json documents]

本次验证以JSON解析、引用、CSV与单元格逐值一致、表格覆盖范围、素材解码和Git差异为范围，结果见[当前校验记录](.source-data/validation.json)。没有重跑.NET、Unity或游戏流程，也不更新历史运行测试结论。[opinion]

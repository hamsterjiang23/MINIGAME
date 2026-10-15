# 【对话】小孩v1.1

## 来源与状态

- 来源：[飞书工作簿](https://moonton.feishu.cn/sheets/FUlBs4PJYhiOoftbZ0mcj1Myn1e)；版本31；读取时间：2026-10-15T19:49:52+08:00。[verified: ../.source-data/dialogue-child-v1-1.json structure.data]
- 全部5个子表按物理行列范围读取，含空白区域与隐藏行列；无截断。CSV保存值，JSON保留真实坐标及公式。[verified: ../.source-data/dialogue-child-v1-1.json sheets；../.source-data/workbook-cells/dialogue-child-v1-1.0ADfgL.json data]
- 已收到并归档用户提供的[Excel原件](../assets/workbooks/dialogue-child-v1-1/dialogue-child-v1-1.xlsx)，5个子表完整保留，与下载目录原件SHA256一致；此前API导出失败不再是取得这份原件的缺口。[verified: ../.source-data/dialogue-child-v1-1.json original_file/original_file_status]

## Excel原件核对

主“对话流程”和立绘表的缓存值与飞书快照一致。角色文本表A2、玩家选项表A2、配置方式A1保留公式但缺少导出缓存值；除这三处外，可比较的值全部一致。没有重算或改写原件，CSV/JSON继续保留服务端已计算结果。[verified: ../.source-data/dialogue-child-v1-1.xlsx-inspection.json sheets/all_comparable_values_match_api/formula_recalculation_run]

[Day1交互原型v0.5.2](inspection-terminal-day1-prototype.md)的内嵌台词、选项、跳转、立绘ID和信任变化与Excel主表Day1逐项一致；HTML只覆盖Day1小孩，Excel另含Day2/3。[verified: ../.source-data/inspection-terminal-day1-v0.5.2.json data_matches_xlsx_day1；../.source-data/workbook-cells/dialogue-child-v1-1.0ADfgL.json data]

## 全部子表

- **对话流程**：`0ADfgL!A1:T500`；非空行231，末非空行231，公式0；[完整CSV](../assets/workbooks/dialogue-child-v1-1/0ADfgL.csv)。[verified: ../.source-data/dialogue-child-v1-1.json sheets 0ADfgL]
- **角色文本表**：`1ArAxd!A1:T500`；非空行162，末非空行162，公式1；[完整CSV](../assets/workbooks/dialogue-child-v1-1/1ArAxd.csv)。[verified: ../.source-data/dialogue-child-v1-1.json sheets 1ArAxd]
- **玩家选项表**：`2OkNPz!A1:T500`；非空行70，末非空行70，公式1；[完整CSV](../assets/workbooks/dialogue-child-v1-1/2OkNPz.csv)。[verified: ../.source-data/dialogue-child-v1-1.json sheets 2OkNPz]
- **立绘ID**：`3yaslJ!A1:T200`；非空行6，末非空行6，公式0；[完整CSV](../assets/workbooks/dialogue-child-v1-1/3yaslJ.csv)。[verified: ../.source-data/dialogue-child-v1-1.json sheets 3yaslJ]
- **配置方式**：`X84Zd2!A1:T200`；非空行47，末非空行48，公式1；[完整CSV](../assets/workbooks/dialogue-child-v1-1/X84Zd2.csv)。[verified: ../.source-data/dialogue-child-v1-1.json sheets X84Zd2]

## 制作与接入规则

- “对话流程”是唯一台词、选项、措施、ID与跳转数据源；两张派生表由公式生成，不能手改。有效编辑区为第2–500行。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第2、5–6、35行]
- 已提供Day1初见、Day2一次治疗后、Day3两次治疗后的内容；每一天为R1–R6对话及R7措施轮。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.0ADfgL.json data.ranges A:C；dialogue-child-v1-1.X84Zd2.json data.ranges 第9–11、33行]
- 捧哏与半分支选项信任变化为0；真分支为−3、0、+3；处置信任变化为0。不能用原型的±10替代这份表的实际值。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第20、30–33行] [opinion]
- J列节点/选项ID唯一；K列目标需存在，允许终点为`END_DAY1/2/3`、`END_CHARACTER`；M列立绘ID引用独立的立绘表。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第18–21、28、43–44行]
- Day1已通过对应HTML整理为Yarn内容包并进入Unity终端，保留来源ID、立绘ID、台词与信任变化；Day2/3待接入。后续编辑包内Yarn，通过[HTML工具与Unity导入](../terminal-ui-guide.md)交付，没有改写Excel原件或派生表。[verified: ../terminal-ui-verification.json source/scope；../../Tools/DialoguePlanner/README.md「策划使用」] [opinion]

## 配置说明原文

- 第1行：对话配置方式。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第1行]
- 第2行：维护原则：只编辑“对话流程”；“角色文本表”和“玩家选项表”由公式自动生成。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第2行]
- 第4行：章节；配置项；配置方式；注意事项。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第4行]
- 第5行：基本原则；唯一数据源；所有台词、选项、措施、节点ID和跳转只在“对话流程”中维护。；不要直接修改两张自动生成表。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第5行]
- 第6行：基本原则；自动生成表；“角色文本表”和“玩家选项表”读取“对话流程”第2—500行。；两张表的A2是主公式，覆盖或删除会中断生成。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第6行]
- 第7行：基本原则；立绘资源；“立绘ID”独立维护；在流程表M列填写对应ID。；只使用立绘表中已存在的ID。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第7行]
- 第8行：对话流程字段。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第8行]
- 第9行：A列；Day/状态；填写Day1｜初见、Day2｜1次治疗后、Day3｜2次治疗后等状态。；同一天保持完全相同的写法。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第9行]
- 第10行：B列；轮次；对话填写R1—R6；措施行填写“措施”。；不要混用数字1和文本R1。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第10行]
- 第11行：C列；轮次类型；填写类型1·捧哏、类型2·半分支、类型3·真分支或R7·措施轮。；类型决定选项类别和信任值规则。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第11行]
- 第12行：D列；流程段；填写轮首、玩家选项、选项回应、收束、措施选项或措施回应。；每行只表示一个节点。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第12行]
- 第13行：E列；角色文本（轮首/收束）；填写轮首和收束台词。；黄色区域。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第13行]
- 第14行：F列；玩家选项；填写普通对话选项。；青色区域。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第14行]
- 第15行：G列；角色文本（分支回应）；填写玩家选项触发的角色回应。；红色区域。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第15行]
- 第16行：H列；玩家措施；填写治疗、净化或释放。；紫色区域。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第16行]
- 第17行：I列；角色文本（措施回应）；填写措施触发的角色回应。；红色区域。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第17行]
- 第18行：J列；节点/选项ID；为当前行填写唯一ID。；整张表不得出现重复ID。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第18行]
- 第19行：K列；下一节点/回应节点；填写执行当前节点后跳转的目标ID。；目标必须存在；结束节点除外。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第19行]
- 第20行：L列；信任值影响；类型3选项填写-3、0或+3；其余选项和措施填0。；角色文本行留空。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第20行]
- 第21行：M列；立绘ID；填写ML01—ML05等资源ID。；玩家选项和措施行可留空。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第21行]
- 第22行：节点ID规范。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第22行]
- 第23行：角色轮首；D{天}_R{轮}_OPEN{序号}；示例：D2_R3_OPEN1；同一段多句按序号递增。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第23行]
- 第24行：分支回应；D{天}_R{轮}_O{选项}_T{序号}；示例：D2_R3_O2_T1；O表示选项，T表示回应句序号。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第24行]
- 第25行：角色收束；D{天}_R{轮}_CLOSING{序号}；示例：D2_R3_CLOSING1；无分支回应时，选项可直接指向收束。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第25行]
- 第26行：玩家选项；D{天}_R{轮}_O{选项}；示例：D2_R3_O2；挂靠节点ID由自动表生成。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第26行]
- 第27行：措施；D{天}_M_TREAT / PURIFY / RELEASE；示例：D2_M_PURIFY；措施回应追加_T1、_T2等。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第27行]
- 第28行：结束节点；END_DAY1 / END_DAY2 / END_DAY3 / END_CHARACTER；按剧情终点填写。；Day3第三次治疗使用END_CHARACTER。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第28行]
- 第29行：轮次与数值。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第29行]
- 第30行：类型1·捧哏；所有选项为“附和”；轮首→玩家选项→收束。；无分支回应，信任值为0。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第30行]
- 第31行：类型2·半分支；部分选项有回应；有回应的选项先进入回应节点；无回应的直接进入收束。；信任值为0。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第31行]
- 第32行：类型3·真分支；所有选项均有回应；每个选项必须配置独立回应链。；信任值使用-3、0或+3。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第32行]
- 第33行：R7·措施轮；治疗 / 净化 / 释放；措施ID指向对应的措施回应链。；措施本身信任值为0。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第33行]
- 第34行：配置步骤。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第34行]
- 第35行：步骤1；新增行；在“对话流程”第2—500行内新增或插入节点。；不要写到第500行以后。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第35行]
- 第36行：步骤2；填写结构；先填写A—D列，确定Day、轮次、类型和流程段。；结构确定后再填文本。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第36行]
- 第37行：步骤3；填写文本；角色行使用E/G/I之一；玩家行使用F或H。；同一行不要同时填写多个文本区域。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第37行]
- 第38行：步骤4；配置ID；在J列填写唯一ID。；命名必须遵循上方规范。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第38行]
- 第39行：步骤5；配置跳转；在K列填写下一节点或回应节点。；空回应选项可直接指向CLOSING。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第39行]
- 第40行：步骤6；补充属性；在L列填写信任值，在M列填写立绘ID。；措施和类型1/2选项信任值为0。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第40行]
- 第41行：步骤7；检查派生表；确认角色文本表和玩家选项表已自动更新。；不要在派生表中手动补数据。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第41行]
- 第42行：发布前检查。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第42行]
- 第43行：检查1；ID唯一性；检查J列没有重复节点ID。；重复ID会导致索引冲突。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第43行]
- 第44行：检查2；跳转完整性；检查K列中的每个目标都存在。；允许的终点仅为规定的END节点。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第44行]
- 第45行：检查3；分支完整性；类型3每个选项都有回应；类型2允许部分直达收束。；不要把空回应误写成不存在的ID。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第45行]
- 第46行：检查4；文本位置；角色文本只在E/G/I；玩家内容只在F/H。；填错列会导致自动表漏项。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第46行]
- 第47行：检查5；颜色语义；黄色=角色轮首/收束，青色=玩家选项，红色=角色回应，紫色=措施。；颜色用于阅读，生成逻辑以列为准。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第47行]
- 第48行：检查6；关键剧情；核对Day2净化文本和Day3第三次治疗的END_CHARACTER。；发布前必须逐条检查。。[verified: ../.source-data/workbook-cells/dialogue-child-v1-1.X84Zd2.json data.ranges 第48行]

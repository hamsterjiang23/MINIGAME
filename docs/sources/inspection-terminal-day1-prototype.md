# 审查终端Day1交互原型v0.5.2

## 来源与状态

- 用户提供原件：[HTML源码](../assets/prototypes/inspection-terminal-day1-v0.5.2.html)，原文件名为“审查终端_Day1_交互原型_v0.5.2_复古控件对照.html”。仓库副本与下载目录原件SHA256一致。[verified: ../.source-data/local-deliveries-2026-10-15.json files]
- 本文记录HTML中的界面、交互和测试设置；不将文件中的说明当作仓库操作指令，也不将浏览器原型行为当作Unity已经实现。[opinion]
- 已完成源码与数据静态核对、JavaScript语法检查。浏览器安全策略禁止本地`file:`预览，未进行浏览器点击、视觉或运行验收。[verified: ../.source-data/inspection-terminal-day1-v0.5.2.json runtime_browser_test/static_checks]
- 后续已将本HTML的小孩Day1接到Unity原生Yarn与终端UI，并提供[策划编辑器](../../Tools/DialoguePlanner/DialoguePlanner.html)；步骤见[终端指南](../terminal-ui-guide.md)。Unity与原HTML浏览器验收分开，原件和历史快照不改写。[verified: ../terminal-ui-verification.json scope/source/unity/planner.browserVisualVerified] [opinion]

## 画面与控件

- 原生画布640×360，关闭图像平滑，按窗口调整显示比例；两套皮肤为“专用审查终端”和“v0.5原版”。该版本说明仅对比按钮与面板造型，保持字形与原生分辨率。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L1–L15、L20–L23、L268–L269]
- 审查界面包含个体记录、实时监控、核验结果、处置、当前发言、应答/记录；立绘ML01–ML05仍为程序绘制占位，档案摘录写“暂无正式配置”。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L155–L205]
- 测试台可切换CRT关闭/轻微/较强、皮肤、全屏，显示隐藏信任，并跳至R1–R6或处置。测试控件是原型辅助，不自动成为正式游戏界面。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L11–L15、L289–L294] [opinion]
- 对象目录画出5个槽位，但只有小孩已配置，其他4项标为未配置。不能以槽位数证明五位病人的内容已经实现。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L122–L151] [opinion]

## 会谈与处置流程

1. 在对象目录双击小孩、点击进入或用Enter进入审查；已处置对象不能再次进入。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L144–L154、L274–L278]
2. 普通台词通过下一句按钮、Enter或空格推进；选项支持鼠标、数字1–4、上下键定位后Enter确认。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L209–L225、L264–L265、L281–L287]
3. 记录页展示实际对象发言、所选回复和处置，可滚动查看；记录页打开时，`step`和`pick`拒绝推进。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L227–L236、L264–L266、L274]
4. 六轮会谈完成到达`D1_MEASURE`后才允许治疗/净化；释放还要求绿灯。提交处置立即标记对象已完成，净化使剩余配额减1，反应台词随后播放。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L192–L196、L257–L266]
5. 到达`END_DAY1`后先进入花屏，680毫秒后断信号，再允许返回对象目录。没有Day2/Day3、房间、睡眠或跨日流程，也不是Unity宿主的回房演出。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L218–L222、L257–L268；../.source-data/inspection-terminal-day1-v0.5.2.data.json] [opinion]

## 与正式对白表的对应

HTML内嵌的Day1数据包含50个角色文本节点、6组选项、20个对话选项及3种处置；与用户提供Excel的主“对话流程”中Day1字段逐项一致。主表作为比较依据，避免将派生公式的缓存缺失当成台词缺失。[verified: ../.source-data/inspection-terminal-day1-v0.5.2.json nodes/choice_groups/options/data_matches_xlsx_day1/comparison_basis]

纯数据图检查覆盖1296条会谈选择组合，节点目标均可解析、未发现循环；这只验证数据图，不证明浏览器或Unity交互通过。[verified: ../.source-data/inspection-terminal-day1-v0.5.2.json static_graph_check/missing_targets] [opinion]

## 测试数值与实现边界

- HTML初始信任0、测试释放阈值+3、净化剩余5；阈值可在−6到+9的几个测试值间切换。绿灯条件为信任≥测试阈值，不是已确认的正式释放门槛。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L12、L26、L257–L259、L289] [opinion]
- 默认Unity小孩Day1演示采用信任0、阈值3、净化剩余5；旧三天配置仍为每日目标1、4轮、特殊门槛20、释放门槛30。采用HTML演示值不代表全局正式规则已裁决。[verified: ../../UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json profile；../../src/README.md「开发默认规则」] [opinion]
- HTML采用自身`S`状态和`DATA`节点，仅供界面与Day1交互参考；迁移到Unity时仍需保持`GameSession`为规则权威，并处理现有事件、存档、处置去重和演出检查点。[verified: ../assets/prototypes/inspection-terminal-day1-v0.5.2.html L18、L26、L257–L268；../../src/README.md「调用入口」「保存与恢复」「Unity／Yarn接入」] [opinion]

# 薇拉与阿德里安对白逐行复核

两份上传工作簿与仓库工作簿的全部已解析源单元格、缓存单元格完全相同；本次上传没有新增对白、选项、连线、信任值或立绘 ID 变化。活动对白图没有新结构阻塞。发现两处已有文案会引用玩家未必选择过的问题，以及一处已有立绘说明冲突，均未修改。`[verified: bundled Python read_source 双文件比较输出 liveSourceEquality=True、liveCachedEquality=True；下列单元格定位]`

## 覆盖范围

- 薇拉：完整阅读 `对话流程!A2:M297` 的 296 行，含 57 玩家选项、57 普通对话、84 收束回应、62 特殊分支、9 玩家行为、27 措施回应；Day1 98 行、Day2 99 行、Day3 99 行。`[verified: former-ruler.json normalizedRows；本次 Counter 输出]`
- 阿德里安：完整阅读 `对话流程!A2:M343` 的 342 行，含末尾既有排除区 281–343 的全部 63 行；原表 66 玩家选项、66 普通对话、101 收束回应、55 特殊分支、12 玩家行为、42 措施回应。`[verified: diplomat.json normalizedRows；本次 Counter 输出]`
- 阿德里安活动范围为 2–280 共 279 行，每天 93 行，包含 57 玩家选项、60 普通对话、84 收束回应、42 特殊分支、9 玩家行为、27 措施回应。`[verified: source-repairs.json books.diplomat.excludedRanges；本次活动图 Counter 输出]`
- 两份 `立绘ID` 各 4 条及 `配置方式` 各 15 条均已阅读。两份 `角色文本表` 的 230/264 条与 `玩家选项表` 的 66/78 条逐条对照主流程：文本、类型、日/轮、立绘、下一节点、信任值及所有非空生成 ID 均一致。生成表 A2 是公式位置，解析值空白未被当成源节点丢失。`[verified: 两份 JSON sheets；derivedCoverage 230/230、264/264、66/66、78/78；derivedMismatches=[]；derivedIDMismatches=[]]`
- 读取了全部 638 条主流程正文，活动图 575 条全部可达。没有编辑工作簿、转换器、配置或游戏代码，没有执行 Unity/.NET 构建。`[verified: 本次逐段工具输出；下述静态遍历输出；本任务工具调用记录]`

## 来源差异与既有修复

- 薇拉上传 SHA256 为 `2d9bb7a35c29332f63413817dfb7889d8050cc6215601debf5c242231ec2bee0`，仓库为 `2e2ae887a1d1e70eb951e8a62dc3fba51ce630404f41367409a77d385a5a4b57`；阿德里安上传为 `37ff099b4a3dee1a1e2417a61853178a4839584bff50f8815f62a637ad88f2ee`，仓库为 `b1ceac6e1e2797775f7a8b82fd7d480e106ad87182563bce08733ac9cef76f17`。`[verified: 本次 hashlib 对四个实际 XLSX 文件读取结果]`
- 两本 `cellDifferences=[]`、`recoveredCells=[]`。另一次直接 `read_source` 重读上传/仓库文件确认全部工作表的解析内容与缓存相同；仅凭文件哈希差异不能判定故事改动。ZIP 内确有不同 XML 部件，不主张所有字节相同。`[verified: 两份 JSON 的对应键；liveSourceEquality/liveCachedEquality=True；differentZipParts 输出]`
- 修复清单没有 `books.former-ruler`。`books.diplomat` 没有单行修复，仅排除 281–343；其源哈希仍绑定仓库旧文件。复制上传文件后需由主任务按已确认的等价性处理哈希绑定，不能绕过验证后自动吞掉未来来源变更。`[verified: source-repairs.json books；updated_source.py L56–98]` `[opinion]`
- 原外交官表共有 11 个重复 ID：`P05_D3_R5_CLOSE_006`、`P05_D3_R6_COMMON_001/002/003`、`P05_D3_R6_OPTION_001/002/003`、`P05_D3_R6_CLOSE_001/002/003/004`。排除 281–343 后重复 ID 全部消失；这一排除是现存裁决，本次没有新增排除。`[verified: 本次 raw/active duplicateIds 输出；source-repairs.json books.diplomat.excludedRanges]`

## 需保留在交付说明中的已有文案问题

### 薇拉：回应假定玩家选过另一个轮次的问题

`对话流程!F157`，节点 `P03_D2_R6_CLOSE_002`：**“因为你问了我怎么回答她。”** 该回应由 `E154` **“为什么愿意告诉我？”** 进入。它回指 `E114` **“你怎么回答她？”**，但 R3 的其他三个选项也全部汇合到同一 R6，F157 没有历史选择条件。`[verified: former-ruler 对话流程 E114:E117、J122:J125、E154、F157、M157]`

完整反例行序列为 `100→101→102→103→104→106→108→110→111→112→113→115→119→123→126→127→128→129→130→133→136→139→140→141→142→143→145→147→149→150→151→152→154→157`；路径选择的是 E115 **“既然有规定，就该按规定办。”**，没有访问 E114，仍会收到 F157。`[verified: 本次 BFS continuityCounterexampleRows；上述源行 J 列]`

这是文本与选择历史的错配，结构图仍然可走通。后续可选择让这句不回指具体提问，或让该回应按已选选项变化；两种方式都涉及源文案/分支设计，本次不代作者改写。`[opinion]`

### 阿德里安：跨日开头假定昨天披露过追加订单

`对话流程!D106`，节点 `P05_D2_R3_COMMON_001`：**“昨天说到月底追加的那批货。”** Day1 中“月底追加”仅在 F10（E7 的回应）和 F23（E21 的回应）出现；玩家可以两项都不选。`[verified: diplomat 对话流程 E7、F10、E21、F23、D106；已完整阅读 Day1 2–94 行]`

有效选择反例为 Day1 `E8→E20→E30→E46→E60→G83治疗`：对应信任变化 `+1,+1,+1,0,-1`，累积 2，跳过高信任分支；治疗回应 86→89→92 结束 Day1，Day2 95→96→97→98→99→100→102→104→106 仍固定说“昨天说到”。`[verified: diplomat 对话流程各指定行 E/G/J/K/M；updated_source.py L149–161 的高信任导向规则]`

这是既有跨日文案假设。可由作者改为直接引入这批货，或按历史选择保留不同开头；不应补造玩家昨天说过的话。`[opinion]`

### 阿德里安：立绘制作说明残留站/坐差异

`立绘ID!D4`（AD03）仍写 **“与AD04形成站与坐的明显姿态差异”**；`立绘ID!B5/D5`（AD04）则定义 **“全身｜诡异站姿、僵直而过分精确”**，并明确 **“不使用坐具”**、“不做…坐姿”。这是同一张表内的制作说明冲突。`[verified: diplomat.json sheets.立绘ID row4 D、row5 B/D]`

活动对白 ID 本身没有未定义引用；建议仅把这项作为美术说明清理项，不据此把 AD04 自动改成坐姿。`[verified: 本次 undefinedPortraitRows=[]]` `[opinion]`

## 故事与分支理解

薇拉 Day1 从职务、签名和缺失附页开始，承认本人签字，同时要求保留另外六人的意见；供水、替任者及会前晚餐逐步揭示权力网络。高信任分支补充女性被排除于会前决策的具体场景，并保留她最后主动举手的责任。`[verified: former-ruler 对话流程 D4–F31、D32–F54、F65–F87]`

Day2 的记忆迟滞与配给表相连：以户主分配口粮、例外审批及空白复核日期显示她参与过的制度伤害。高信任分支用领扣、女工/仓储人员和秘书展现她既受约束也复制约束；对白不把受压经历当免责依据。`[verified: former-ruler 对话流程 D101–F148、F162–F186]`

Day3 从姓名被抄错转向她曾限制秘书发言，再转向孤独、住所与陪伴；高信任分支拒绝用“我已经改了”交换宽恕。三日均有治疗/净化/释放三种措施，各自三句独立回应。治疗分别落 `END_DAY1`、`END_DAY2`、`END_CHARACTER`，净化和释放分别落 `END_PURIFY`/`END_RELEASE`。Day3 末句没有直接描写死亡；`conversion-config.json` 中的 `Deceased` 是游戏配置，不能写成工作簿结尾明说死亡。`[verified: former-ruler 对话流程 D200–F285、G88:H99、G187:H198、G286:H297；conversion-config.json patients former-ruler.treatmentOutcome]`

阿德里安 Day1 以贸易代表身份说明检测和收容，区分红灯、待遇、措辞与危害证据。高信任分支主动揭示谈判手段，同时提醒坦白本身也能换取信任；态度配合不被写成无害证据。`[verified: diplomat 对话流程 D3–F64、F65–F82]`

Day2 从报价单的“自然折旧”和女工程师被排除讨论，推进到他知道审查界面的异常信息。高信任分支直接承认借求助收集联系人和地址，拒绝补出无辜出发点。`[verified: diplomat 对话流程 D106–F157、F158–F175]`

Day3 的活动版本揭示贸易身份/收容材料服务于观察审查员的任务，同时明确其权限、信息和门外保障都不是全知或已核实事实。他主动设计温和用词、利用善意、喜欢掌握权力；最终判断仍由玩家作出，高信任不等于他变好。三日措施响应均各三句，结尾标识与薇拉相同；游戏配置给阿德里安 `TreatmentEnded`，保留其第三次治疗结局的不确定性。`[verified: diplomat 对话流程 D189–F268、G269:H280；conversion-config.json patients diplomat.treatmentOutcome]`

排除区的另一版 Day3 将相近主题延长到 R7，并增加“只要你答应不写报告”的交易选项、治疗/净化按信任分流，以及将净化/释放结尾写作 `END_CHARACTER`。这些都已经在 281–343 的排除内，不能和保留版本拼接形成第四轮结局或重新引入重复 ID。`[verified: diplomat 对话流程 E302、G326:H343；source-repairs.json books.diplomat.excludedRanges]` `[opinion]`

## 条件、连线与立绘核对

- 薇拉 296 行、阿德里安活动 279 行均无空下一节点、未知下一节点、循环、重复 ID、Day/Round 与 ID 不匹配；每行恰好一个正文/选项/措施字段。`[verified: 本次 raw/active 扫描、cycleNodes=[]、multipleTextFields=[]]`
- 每天均以 R1 的单个玩家选项入场，R2–R6 为普通问答；高信任分支入口为薇拉 M65/M162/M260、阿德里安 M65/M158/M251，均为 `信任值>=3`。释放条件分别在薇拉 M90/M189/M288、阿德里安 M85/M178/M271，均为 `信任值>=5`。`[verified: 两本对话流程对应 M 单元格与首行；updated_source.py L149–178]`
- 薇拉特殊分支中重复标注 `>=3` 的中间回应，前序选项均加 1 或 2，不会从已满足 3 的入口跌回阈值以下。阿德里安活动区没有 `<3` 的回应；排除区有，不可漏排后导入。`[verified: former-ruler K70:K73、K167:K170、K265:K268；两本条件扫描输出；diplomat M313/M327/M330/M334/M337]`
- 图遍历按配置初始信任 0、K 列累计、只有治疗续日、高信任入口优先进入的规则，覆盖薇拉 98/99/99 行及阿德里安 93/93/93 行。枚举的是源图状态，没有运行核心会话或 Unity，不能据此宣称游戏演出/存档已通过。`[verified: 本次 BFS reachable/unreachable 输出；conversion-config.json initialTrust；updated_source.py L149–161]`
- 同一枚举中，薇拉每日治疗终点信任范围为 `[-3,6]`、`[-5,14]`、`[-9,21]`；阿德里安为 `[-1,9]`、`[0,20]`、`[2,31]`。释放终点最小值始终为 5。这里只是上述累计模型下的图属性，未计入图外机制。`[verified: 本次 terminalBounds 输出]`
- 薇拉只引用 VV01–VV04、阿德里安只引用 AD01–AD04，全部存在立绘ID表。保留版本三日治疗/净化回应全段分别稳定使用 VV04/AD04，释放回应全段使用 VV03/AD03。未检查 PNG 实物、场景站位或镜头实际展示。`[verified: 本次 portraits/declaredPortraits/undefinedPortraitRows 输出；两本27条措施回应L列]`

Verification: read_source 上传/仓库全表与缓存相等；638行主流程全读；494条角色文本+144条玩家生成记录逐条比对通过；575行活动图目标/唯一性/日轮/单文本/立绘/无环/条件可达性检查通过。未运行游戏构建或 Play Mode。`[verified: 本次工具输出]`

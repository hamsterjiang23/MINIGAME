# Papers-Please_Unity：一次会话的进入、处置与清理

它展示了少量代码如何串起角色到达、资料生成、按钮处置和离场。适合阅读交互生命周期，不适合作为我们的剧情、信任或每日系统。

## 研究基线

| 项目 | 记录 |
|---|---|
| 研究日期 | 2026-10-12 |
| 目标引擎 | Unity 2022.3 LTS；渲染管线待定 |
| 原仓库 | `Papers-Please_Unity` |
| 来源 | [https://github.com/limeraiin/Papers-Please_Unity](https://github.com/limeraiin/Papers-Please_Unity) |
| 本地目录 | `references/Papers-Please_Unity/` |
| 固定 SHA | `bacb203893627645a90575c353e162b7c66e386e` |
| 实际引擎版本 | Unity 2020.3.25f1（E09） |
| 仓库性质 | 小型 Papers, Please 风格二维审查原型 |
| 研究优先级 | 低：最小交互循环与可选拖放参考 |
| 验证状态 | 已核对本地源码与来源锁；仅静态检查，未编译、未运行 |

需求依据：[游戏总览](../../docs/game-overview.md)；决策汇总：[借鉴矩阵](../reuse-matrix.md)；实施顺序：[Unity 2022 路线](../unity-2022-roadmap.md)。

## 实际实现证据

| ID | 已核查行为 | 本地源码与行号 | 固定版本永久链接 |
|---|---|---|---|
| E01 | 每秒递归调度生成随机角色 | [GameManager.cs:L46–L67](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/GameManager.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/GameManager.cs#L46-L67) |
| E02 | 按人物数据生成文件并制造错误字段 | [GameManager.cs:L69–L146](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/GameManager.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/GameManager.cs#L69-L146) |
| E03 | 随机姓名、国家、头像与日期生成角色 | [GameManager.cs:L148–L202](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/GameManager.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/GameManager.cs#L148-L202) |
| E04 | 红绿按钮核对真假、计分、清除资料并送走人物 | [GameManager.cs:L204–L227](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/GameManager.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/GameManager.cs#L204-L227) |
| E05 | 按钮着色动画后调用管理器 | [DeskButton.cs:L15–L27](../../references/Papers-Please_Unity/Assets/DeskButton.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/DeskButton.cs#L15-L27) |
| E06 | 来访者移动到窗口并显示头像/文件 | [MiniCharController.cs:L75–L97](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/MiniCharController.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/MiniCharController.cs#L75-L97) |
| E07 | 二维物件拖放与刚体模式切换 | [DragDrop.cs:L14–L48](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/DragDrop.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/DragDrop.cs#L14-L48) |
| E08 | 真假属性由随机值决定 | [MiniCharController.cs:L23–L48](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/MiniCharController.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/MiniCharController.cs#L23-L48) |
| E09 | 实际 Unity 版本 | [ProjectVersion.txt:L1–L2](../../references/Papers-Please_Unity/ProjectSettings/ProjectVersion.txt) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/ProjectSettings/ProjectVersion.txt#L1-L2) |
| E10 | 文件展示组件仅保存头像和文本引用 | [Document.cs:L7–L11](../../references/Papers-Please_Unity/Assets/Project%20Data/Scripts/Document.cs) | [GitHub](https://github.com/limeraiin/Papers-Please_Unity/blob/bacb203893627645a90575c353e162b7c66e386e/Assets/Project%20Data/Scripts/Document.cs#L7-L11) |

## 调用与数据流

1. `GameManager.Awake()` 设置全局实例并启动 `Spawner()`；协程等一秒生成角色，再启动自己，没有每日人数和停止条件（E01）。
2. `SpawnCharacter()` 随机选择姓名、国家、头像和日期，实例化小地图角色，传给 `MiniCharController.CharSetup()`（E03）。
3. `CharSetup()` 另用随机值决定 `isReal`；真假不是从剧情或文件矛盾计算，而是先赋值后制造对应线索（E08）。
4. 角色碰到窗口后停止移动，显示头像，调用 `SpawnItem()`。该协程记录当前来访者，生成多个文件并填入数据，假角色会按一个选定类别制造错误字段（E02、E06）。
5. 桌面红/绿按钮先播放颜色变化，再调用 `ButtonCall(isRed)`；`currentArrival.isReal == !isRed` 判定成功或失败（E04、E05）。
6. 管理器清除文件父节点的所有子对象，通知人物 `Served()` 向右移动，隐藏头像，清空当前来访者引用（E04、E06）。
7. 拖放组件通过 `Camera.main.ScreenToWorldPoint()` 计算鼠标位置，拖动时设为运动学刚体，松开按物件模式恢复动态或静态（E07）。

## 四类借鉴

| 分类 | 本游戏采取的方式 |
|---|---|
| 直接引入 | 当前不直接引入游戏代码和素材；未找到项目许可。旧 Unity 工程也尚未升级验证。 |
| 改写结构 | 低优先：当前对象进入、显示资料、提交处置、清理显示、切换下一对象的最小生命周期。 |
| 表现参考 | 红/绿按钮反馈和资料进入/退出节奏；纸张拖放仅在后续设计确认需要时参考。 |
| 自研缺口 | 终端多轮对话、对话结束门控、治疗按钮、释放资格、跨日信任与治疗阶段、配额和结局。 |

这里的二元真伪判定不能进入我们的情绪检测系统。我们的五位病人拥有固定剧情与不同责任，检测结果不等于客观犯罪或善恶真值。
没有发现本项目实现多天、信任、治疗或三结局。一个可以不断接客的循环不等于满足三天内容交付。

## 具体改造步骤

1. 用当天固定病人 ID 队列替代递归 `Spawner()`，由一个流程管理者在前一位结束表现后请求下一位。
2. 病人静态资料由我们的角色定义读取；当日对白、治疗阶段和信任从运行状态查询，不使用随机身份、姓名或日期（E03、E08）。
3. 终端进入状态完成后才展示头像/视频层和对话组件；把“当前病人”视为会话状态而不是小地图刚体对象。
4. 把 `ButtonCall(bool)` 改为明确的治疗/净化/释放处置类型，先检查对话完结和选项资格，再锁定本次提交。
5. 将 UI 按钮着色与业务接受分开：业务先接受一次处置，再播放反应和故障效果，避免延时动画期间接受多次点击（E05）。
6. 不再更新成功/失败分数；客观记录处置历史、净化数量和病人变化，制度评价与角色后果交由独立规则。
7. 清理时只销毁本次会话的临时内容，保留跨天运行状态；所有病人已结束则退出终端并开放床，而不是继续接客。
8. 如果确定需要档案拖放，重新实现终端 UI 的指针事件和层级排序；当前屏幕交互不需要移入实体纸张刚体（E07）。

## 依赖与改造成本

| 部分 | 依赖及成本判断 |
|---|---|
| 原工程整体 | Unity 2020.3.25f1、TextMeshPro、Unity.Mathematics、2D 碰撞/动画与场景绑定；升级成本未验证。 |
| 最小会话生命周期 | 逻辑量小，可自写并配合我们终端 UI；成本低，但不能省去核心病人状态开发。 |
| 原文件与队列 | 强耦合 Collider 标签、固定文本下标、刚体与人物小地图；不适合作为第一人称公寓骨架。 |
| 纸张表现 | 304 个 LFS 文件只说明资源已下载；若不需要纸质档案互动，迁移这部分没有当前收益。 |

静态细节：`SpawnItem()` 使用 `rng.Next(0, 4)`，因此原代码的 `case 4` 性别错误分支在该路径中不可达（E02）。
`fakeNames`、`fakeCountries` 被从列表移除，反复生成会耗尽特殊假信息；我们采用固定剧情配置，不继承这套内容消耗机制。
原按钮仅保护“没有当前来访者”，没有实现对话阶段和信任资格校验（E04）；这些保护须由我们的会话层补建。

## 代码与资产许可

- 仓库文件扫描未找到根 LICENSE/LICENCE 或游戏代码统一授权声明，不能凭 GitHub 可读取就判定可直接复制发行。
- 工程附带图片、字体和其他素材，资源完成下载与授权可复用是两种证据；本次未完成资产来源许可核查。
- 推荐把它作为代码阅读样本，自写小型会话生命周期，使用本游戏自有角色资料和原创表现。
- 若未来选择复制特定组件，先记录权利来源、适用许可及需保留的声明，再验证 Unity 2022.3 的依赖。

## 后续验证场景

- [ ] 当天只出现配置中的固定病人，终端暂停或重开时不会产生新角色。
- [ ] 对话未完成、处置未开放或已提交时，点击按钮不会改变状态。
- [ ] 治疗、净化和释放分别执行自己的后果；没有红绿真假计分残留。
- [ ] 连续点击和按钮表现的延迟期间只接受一次有效提交。
- [ ] 退出终端只清理界面，病人信任、治疗阶段和历史处置跨天保留。
- [ ] 最后病人结束后进入回房/睡眠流程，不再产生随机角色或空会话。
- [ ] 若使用纸张 UI，点击、拖放、排序与其他按钮不会同时触发处置。
- [ ] 在 Unity 2022.3 的最小会话场景验证输入和表现，再决定是否需要额外资料交互。

## 结论

这是可快速读懂的审查界面原型，适合低成本地理解一次会话的生命周期。优先级低于日内容与处置状态参考，不建议迁入无限随机队列或二元真伪规则。

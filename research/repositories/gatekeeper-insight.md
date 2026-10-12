# gatekeeper-insight：处置事件、日统计与分层信息

可借鉴“按钮只提交决定，各系统分别响应”与逐层展示线索的结构。原有善恶分类和随机访客机制与本游戏不同，必须改写。

## 研究基线

| 项目 | 记录 |
|---|---|
| 研究日期 | 2026-10-12 |
| 目标引擎 | Unity 2022.3 LTS；渲染管线待定 |
| 原仓库 | `gatekeeper-insight` |
| 来源 | [https://github.com/adamico/gatekeeper-insight](https://github.com/adamico/gatekeeper-insight) |
| 本地目录 | `references/gatekeeper-insight/` |
| 固定 SHA | `8d4658a4805b645eb9a234be5e90bdaab954f3f6` |
| 实际引擎版本 | Godot 4.4（E11） |
| 仓库性质 | Godot Wild Jam 小型 2D 守门审查游戏 |
| 研究优先级 | 中：事件与信息揭露参考 |
| 验证状态 | 已核对本地源码与来源锁；仅静态检查，未编译、未运行 |

需求依据：[游戏总览](../../docs/game-overview.md)；决策汇总：[借鉴矩阵](../reuse-matrix.md)；实施顺序：[Unity 2022 路线](../unity-2022-roadmap.md)。

## 实际实现证据

| ID | 已核查行为 | 本地源码与行号 | 固定版本永久链接 |
|---|---|---|---|
| E01 | 处置和日变更通过全局信号通知 | [event_bus.gd:L7–L20](../../references/gatekeeper-insight/global/event_bus.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/global/event_bus.gd#L7-L20) |
| E02 | 每日处理数、正确性和日终统计 | [game_stats.gd:L58–L112](../../references/gatekeeper-insight/global/game_stats.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/global/game_stats.gd#L58-L112) |
| E03 | 处置后实例化随机新访客 | [game.gd:L26–L58](../../references/gatekeeper-insight/scenes/game/game.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/scenes/game/game.gd#L26-L58) |
| E04 | 按观察等级逐步拼接可见信息 | [visitor.gd:L62–L78](../../references/gatekeeper-insight/scenes/visitor/visitor.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/scenes/visitor/visitor.gd#L62-L78) |
| E05 | 已知身份按 SAFE/DANGEROUS 着色 | [visitor.gd:L30–L54](../../references/gatekeeper-insight/scenes/visitor/visitor.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/scenes/visitor/visitor.gd#L30-L54) |
| E06 | 日终展示与下一日/返回菜单判断 | [day_end.gd:L21–L82](../../references/gatekeeper-insight/scenes/day_end/day_end.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/scenes/day_end/day_end.gd#L21-L82) |
| E07 | 资源中的身份分类决定处置正确性 | [visitor_stats.gd:L96–L107](../../references/gatekeeper-insight/data/visitors/visitor_stats.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/data/visitors/visitor_stats.gd#L96-L107) |
| E08 | 允许/拒绝按钮发出处置事件 | [actions_container.gd:L11–L27](../../references/gatekeeper-insight/scenes/ui/actions_container.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/scenes/ui/actions_container.gd#L11-L27) |
| E09 | 观察池消耗与已知身份记忆 | [gate_keeper.gd:L54–L89](../../references/gatekeeper-insight/scenes/gate_keeper/gate_keeper.gd) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/scenes/gate_keeper/gate_keeper.gd#L54-L89) |
| E10 | 根代码许可 | [LICENSE:L1–L21](../../references/gatekeeper-insight/LICENSE) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/LICENSE#L1-L21) |
| E11 | 实际引擎和全局加载项 | [project.godot:L11–L21](../../references/gatekeeper-insight/project.godot) | [GitHub](https://github.com/adamico/gatekeeper-insight/blob/8d4658a4805b645eb9a234be5e90bdaab954f3f6/project.godot#L11-L21) |

## 调用与数据流

1. 主场景加载守门人、随机访客和按钮界面；随后订阅允许/拒绝事件，事件到来时实例化下一位访客（E03）。
2. 按钮查找当前 visitor 分组对象，将其 `VisitorStats` 放到 `visitor_admitted` 或 `visitor_denied` 信号里（E08）。按钮不直接操作日统计。
3. 访客监听同一事件销毁自己的显示；守门人响应事件调整观察池，允许事件还可能把已知身份加入记忆（E09）。
4. `GameStats` 作为另一订阅者更新允许/拒绝和正确/错误数量、剩余访客数；达到每日目标后保存摘要，发出 `day_completed`（E02）。
5. 观察等级变化时，访客界面从信息数组中取出等级允许的多个片段并拼接展示（E04）。资源定义与界面显示相分离。
6. 已知身份会显示红/绿分类，正确性取决于身份是否危险（E05、E07）。这是该项目的玩法前提，与我们的情绪仪器叙事不同。
7. 日终面板读当日统计；下一日按钮发出 `next_day_started`，主场景隐藏面板（E06）。原项目的周结算不是我们的三天结局流程。

## 四类借鉴

| 分类 | 本游戏采取的方式 |
|---|---|
| 直接引入 | Unity 项目不直接引入 GDScript 或 Godot 场景；根 MIT 可支持保留声明后的代码研究/改编，但不产生引擎兼容。 |
| 改写结构 | 中优先：一个处置提交事件，让病人状态、日统计和界面分别响应；静态资源与运行实例分离。 |
| 表现参考 | 信息按层次揭露、制度要求与处理数量持续可见；可用于患者档案和终端提示。 |
| 自研缺口 | 固定五人跨天对话、隐藏信任、治疗阶段、晨检、梦境、三种处置与三种结局。 |

应保留“信息分层”，去掉“分层后一定知道这个人属于善恶哪一类”。本游戏仪器显示情绪或状态，不给客观危险者答案。
随机生成人物与 7 天评价都不能进入我们的三天固定角色内容；角色的离场与返场必须由病人剧情状态决定。

## 具体改造步骤

1. 在 Unity 中自写病人定义与运行状态：静态对白和素材可做配置，信任、治疗、离场、当日完成状态保存在独立实例中。
2. 用固定病人 ID 排定当天内容，替代 `VISITOR_STATS.pick_random()`；神父何日出现等版本冲突继续等待设计确认。
3. 保留集中提交入口：完成对话后检查合法处置，再发出包含日序号、病人 ID 和处置种类的事件。
4. 配额系统只读取净化事件；病人系统更新该人状态；终端系统播放对应反应并等待结束。后续病人由日流程控制器推进。
5. 对单次事件设置提交保护，避免一个病人产生多个处置或多个订阅者同时推进队列。
6. 用对话轮次、已揭露事实或确认后的信任规则决定额外信息，替代观察等级和有限观察池；不把原游戏数值转成我们的阈值。
7. 日终统计保存客观的处置记录与机构要求，去掉正确率、等级 A–F 和危险者名单，保留对玩家的叙事压力。
8. 三天之后交由本游戏结局规则处理；日末先回公寓并开放床，而不是直接用“下一天”按钮略过梦境。

## 依赖与改造成本

| 部分 | 依赖及成本判断 |
|---|---|
| 原工程整体 | Godot 4.4、Resource、PackedScene、分组查找、全局 autoload；无法作为 Unity 2022.3 组件直接导入。 |
| 事件/统计模式 | C# 重写较小，但需要明确事件唯一执行者与订阅生命周期，成本低到中等。 |
| 观察与人物系统 | 身份、安全分类和观察池属于另一套玩法；迁移为固定叙事需要重新定义数据，成本中等。 |
| 场景与素材 | 2D Godot 界面布局不等于第一人称公寓/独立终端；我们的场景和效果需自行实现。 |

静态边界值得警惕：第 7 天完成周结算时 `current_day` 未增加，而返回菜单按钮判断 `current_day > DAYS_PER_WEEK`（E02、E06）。不能把它当作已验证的完整结束模板。
主场景对处置自动创建下一访客与日终统计分别订阅同一事件；我们应在日流程入口统一判断是否还有病人，避免结束当天后继续入场。

## 代码与资产许可

- 根许可证为 MIT，版权归 Andrea D'Amico；保留版权与许可声明是直接复用代码的条件（E10）。
- 引擎不同意味着可直接采用的是思路或经改编的小段逻辑，不是把 `.gd` 放到 Unity 就能运行。
- 库内含图片、字体与音频，本次没有发现可逐项对应的素材来源授权清单；根许可的存在不能替代第三方素材核查。
- 建议自制我们的病人素材和公寓资产，不复制原角色身份、文本、界面或音效作为正式内容。

## 后续验证场景

- [ ] 每次合法处置只有一个病人离场，只增加一次处理数量。
- [ ] 当天最后一位完成后不生成新的病人，终端进入结束流程。
- [ ] 新日清空当日统计，病人治疗和信任等跨日状态继续保留。
- [ ] 多次打开/关闭终端不重复订阅事件；销毁对象解除订阅。
- [ ] 已解锁信息不会因界面刷新丢失，未解锁信息不会泄露。
- [ ] 终端检测显示高情绪值时，不自动将人物标记为邪恶或错误处置。
- [ ] 第三天结束只进入一次结局/夜间流程，避免第 4 天越界与旧周结算边界。
- [ ] 治疗、净化、释放分别使用自己的后果与表现，不复用允许/拒绝二元评分。

## 结论

将它作为事件拆分、日统计和信息揭露的中优先结构样本。Godot 玩法需重写为 Unity，尤其要删除真假裁决与随机访客前提，才能服务我们的心理恐怖叙事。

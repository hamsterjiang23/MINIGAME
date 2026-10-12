# Do-You-Trust-Them：每日内容、活动门控与过场

它提供了按天配置活动、统一广播日变更和完成任务后开放结束入口的具体样本，适合重写成本较低的三天公寓流程。

## 研究基线

| 项目 | 记录 |
|---|---|
| 研究日期 | 2026-10-12 |
| 目标引擎 | Unity 2022.3 LTS；渲染管线待定 |
| 原仓库 | `Do-You-Trust-Them` |
| 来源 | [https://github.com/katrinrash/Do-You-Trust-Them](https://github.com/katrinrash/Do-You-Trust-Them) |
| 本地目录 | `references/Do-You-Trust-Them/` |
| 固定 SHA | `6b0d507e8af609639c6228d53ee6964566ee1cdb` |
| 实际引擎版本 | 未知；仓库未包含 ProjectVersion.txt |
| 仓库性质 | 团队游戏的部分贡献代码和截图，非完整 Unity 工程 |
| 研究优先级 | 高：日循环与房间交互结构 |
| 验证状态 | 已核对本地源码与来源锁；仅静态检查，未编译、未运行 |

需求依据：[游戏总览](../../docs/game-overview.md)；决策汇总：[借鉴矩阵](../reuse-matrix.md)；实施顺序：[Unity 2022 路线](../unity-2022-roadmap.md)。

## 实际实现证据

| ID | 已核查行为 | 本地源码与行号 | 固定版本永久链接 |
|---|---|---|---|
| E01 | 每日内容配置包含任务、可用电脑和物件 | [DayContentSO.cs:L8–L34](../../references/Do-You-Trust-Them/Scripts/DaySystem/DaysManagement/DayContentSO.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/DaySystem/DaysManagement/DayContentSO.cs#L8-L34) |
| E02 | 加载日配置、广播日变更及切日 | [DaysManager.cs:L11–L59](../../references/Do-You-Trust-Them/Scripts/DaySystem/DaysManagement/DaysManager.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/DaySystem/DaysManagement/DaysManager.cs#L11-L59) |
| E03 | 活动计数完成后开放结束或最终报告按钮 | [ProgressController.cs:L26–L64](../../references/Do-You-Trust-Them/Scripts/GameProgressSystem/GeneralManagement/ProgressController.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/GameProgressSystem/GeneralManagement/ProgressController.cs#L26-L64) |
| E04 | 淡入切日、淡出恢复笔记与菜单访问 | [DaysSwitchWithAnimation.cs:L16–L54](../../references/Do-You-Trust-Them/Scripts/DaySystem/DaysTransition/DaysSwitchWithAnimation.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/DaySystem/DaysTransition/DaysSwitchWithAnimation.cs#L16-L54) |
| E05 | 按电脑名称启用当天交互 | [ComputerActivityController.cs:L12–L34](../../references/Do-You-Trust-Them/Scripts/DaySystem/Clients/ComputerActivityController.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/DaySystem/Clients/ComputerActivityController.cs#L12-L34) |
| E06 | 结局名字映射到 IEnding 并分发结果 | [StateController.cs:L26–L42](../../references/Do-You-Trust-Them/Scripts/EndingSystem/EndingsManagement/StateController.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/EndingSystem/EndingsManagement/StateController.cs#L26-L42) |
| E07 | 结局按钮直接以对象名选择结局 | [ChoiceBtnLogic.cs:L11–L24](../../references/Do-You-Trust-Them/Scripts/EndingSystem/ButtonsManagement/ChoiceBtnLogic.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/EndingSystem/ButtonsManagement/ChoiceBtnLogic.cs#L11-L24) |
| E08 | 日变更切换物件线索，收集后完成活动 | [ObjectClues.cs:L24–L64](../../references/Do-You-Trust-Them/Scripts/DaySystem/Clients/CluesSystem/ObjectClues.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/DaySystem/Clients/CluesSystem/ObjectClues.cs#L24-L64) |
| E09 | 独立结局配置负责展示文字与成就 | [EndingManager.cs:L7–L31](../../references/Do-You-Trust-Them/Scripts/EndingSystem/Endings/EndingManager.cs) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/Scripts/EndingSystem/Endings/EndingManager.cs#L7-L31) |
| E10 | 仓库说明团队项目和作者负责部分 | [README.md:L3–L35](../../references/Do-You-Trust-Them/README.md) | [GitHub](https://github.com/katrinrash/Do-You-Trust-Them/blob/6b0d507e8af609639c6228d53ee6964566ee1cdb/README.md#L3-L35) |

## 调用与数据流

1. `DaysManager.Start()` 从第一个 `DayContentSO` 开始；`LoadDay()` 更新当前配置，发出 `OnDayChanged`，隐藏结束按钮（E01、E02）。
2. 日变更的订阅者分别设置任务、电脑和物件。电脑按 `activeComputersNames` 查询名称，启用或关闭 `ScreenViewController`（E05）。
3. `ProgressController` 保存当天活动对象；活动报告完成时加入完成列表，两个列表数量相等才开放结束入口（E03）。
4. 第三天通过硬编码 `dayName == "Day3"` 改为开放报告按钮，其余日开放结束按钮。它不是通用的每日阶段或结局条件系统（E03）。
5. 淡入协程禁用笔记与菜单，等待动画后调用 `NextDay()`；淡出协程再恢复访问并刷新笔记信息（E04）。
6. 线索组件收到日变更后取配置列表首项并移除，收集时生成笔记按钮、添加文本并完成物件活动（E08）。它使用顺序消费，不能直接支持任意读档与重放。
7. 结局选择按钮把自身对象名交给 `StateController`；后者查找 `IEnding`，配置结束确认按钮，最终展示对应文本和成就（E06、E07、E09）。

## 四类借鉴

| 分类 | 本游戏采取的方式 |
|---|---|
| 直接引入 | 当前不直接复制。仓库缺项目许可和完整依赖，先用公开结构理解机制。 |
| 改写结构 | 高优先：每日数据配置、日变更事件、活动完成门控与过场期间统一禁用交互。 |
| 表现参考 | 办公环境、电脑、笔记与逐日变化；适合参考晨检后的任务更新和结束当天提示。 |
| 自研缺口 | 病人隐藏信任、治疗阶段、三种处置、情绪检测、自动结局条件与首次进入终端的过渡。 |

名字中的“Trust”不是隐藏数值信任系统的证据。本次查看的日、活动和结局脚本没有提供我们的跨天病人信任逻辑。
原游戏通过玩家选“谁是假冒者”选出结果，我们需要保留仪器并不判断善恶的设定，不加入客观真假评分。

## 具体改造步骤

1. 自写三份日内容配置，记录当天病人对话入口、配额要求、晨检/监督员/梦境内容和可互动对象。
2. 把原配置的多个并行名称/任务列表改为单个可识别条目，避免数量错位；稳定 ID 不使用对象显示名。
3. 用明确的日阶段管理晨检、终端、处置、回房、睡眠和次日，日变更事件只通知已确认的新一天。
4. 将“活动完成数量相等”改为固定任务 ID 的完成集合，重复上报不改变完成数量；哪些活动必做保持来自需求。
5. 借鉴淡入期间禁用操作，控制相机、鼠标和 E 键的统一交互模式；过场完成后按阶段恢复，而不是同时启用所有对象。
6. 梦境入口按程序需求门控：终端完成后恢复行走，只保留床可交互；不要照搬全局 Space 随时淡出的行为（E04）。
7. 若日记或档案确定纳入范围，用病人 ID 和日序号查询线索；保留内容配置，不用 `RemoveAt(0)` 改写配置源（E08）。
8. 结局展示可保留“一个结果配置对应一套表现”的思路；实际结局先由配额与事件状态判定，再交给展示层（E06、E09）。

## 依赖与改造成本

| 部分 | 依赖及成本判断 |
|---|---|
| 原仓库整体 | 仅 53 个已跟踪文件，主要是作者贡献脚本与截图；没有完整 ProjectSettings 或 Packages，不能直接打开成完整工程。 |
| 日与活动模式 | Unity、C#、ScriptableObject、TextMeshPro、UI；自写小型版本成本低到中等。 |
| 缺失依赖 | 引用了 PlayerController、NotebookManager、MenuView、ScreenViewController、Notification 等未公开组件；不能据 README 声称全部实现可迁移。 |
| 内容与状态 | 日配置需要我们的文本和事件；第三天报告不等于三结局条件，病人状态需要独立补建。 |

源仓库的 Unity 精确版本未知。普通 Unity API 可供结构研究，但兼容 Unity 2022.3 尚未经过编译确认。
静态风险包括列表重复上报、结局字符串键缺失、并行列表错位，以及 `ChoiceBtnLogic` 片段中的 `_choiceText` 未初始化（E03、E06、E07）。

## 代码与资产许可

- 扫描仓库未找到根或游戏脚本的 LICENSE/LICENCE；README 介绍贡献并不构成项目代码使用许可（E10）。
- 仓库说明原游戏由四人团队完成、作者负责部分系统，不能把公开代码等同于完整游戏的共同授权。
- 截图可以用于理解布局和交互；本次没有将其复制到本游戏，也没有核实所有画面素材的授权来源。
- 推荐记录行为和结构后自行实现，未来若要复制原代码，先取得明确的适用许可与依赖范围。

## 后续验证场景

- [ ] 切日事件只发出一次，电脑、床和晨检对象按正确阶段更新。
- [ ] 同一活动重复完成，不提前结束当天。
- [ ] Day 3 最后任务完成后进入确认后的结局流程，不读取不存在的 Day 4 配置。
- [ ] 日配置缺条目或 ID 不存在时给出可定位错误，不因并行列表索引错位静默使用他人任务。
- [ ] 终端与梦境过场不能用 Space、E 或鼠标提前跳过关键阶段。
- [ ] 已收集线索跨天保存，重进场景或读档不会消耗第二份日内容。
- [ ] 神父离场、病人治疗终止后，当天活动清单不会要求不再存在的角色。
- [ ] 三个结局由状态规则触发，结果表现不会反向改变结局条件。

## 结论

最适合借鉴每日内容组织和公寓活动门控。优先改写为独立的小系统，避免移入缺失依赖；其完成报告、最终按钮和线索展示不能替代我们尚未确定的规则。

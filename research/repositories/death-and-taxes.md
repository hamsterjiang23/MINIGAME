# DeathAndTaxes：处置后果、每日任务与持久状态

最有价值的部分是“逐人处置产生后果，日终核算制度要求，再推动叙事”。建议提炼这一结构，重新建立本游戏的三种处置和五位病人状态。

## 研究基线

| 项目 | 记录 |
|---|---|
| 研究日期 | 2026-10-12 |
| 目标引擎 | Unity 2022.3 LTS；渲染管线待定 |
| 原仓库 | `DeathAndTaxes` |
| 来源 | [https://github.com/oakwarrior/DeathAndTaxes](https://github.com/oakwarrior/DeathAndTaxes) |
| 本地目录 | `references/DeathAndTaxes/` |
| 固定 SHA | `8fc4bb9211f09a5296ef081e1359cdb27f313688` |
| 实际引擎版本 | Unity 2019.2.10f1（E12） |
| 仓库性质 | 已发行游戏公开代码；非完整可复用资产包 |
| 研究优先级 | 高：核心制度循环与状态参考 |
| 验证状态 | 已核对本地源码与来源锁；仅静态检查，未编译、未运行 |

需求依据：[游戏总览](../../docs/game-overview.md)；决策汇总：[借鉴矩阵](../reuse-matrix.md)；实施顺序：[Unity 2022 路线](../unity-2022-roadmap.md)。

## 实际实现证据

| ID | 已核查行为 | 本地源码与行号 | 固定版本永久链接 |
|---|---|---|---|
| E01 | 每日角色、任务、统计与界面初始化 | [DesktopManager.cs:L59–L109](../../references/DeathAndTaxes/Assets/_Scripts/Desktop/DesktopManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Desktop/DesktopManager.cs#L59-L109) |
| E02 | 提交后统计当日死亡/赦免数量 | [PaperworkManager.cs:L133–L151](../../references/DeathAndTaxes/Assets/_Scripts/Desktop/Paperwork/PaperworkManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Desktop/Paperwork/PaperworkManager.cs#L133-L151) |
| E03 | 按处置选择角色后果，并记录任务偏差 | [DesktopManager.cs:L326–L428](../../references/DeathAndTaxes/Assets/_Scripts/Desktop/DesktopManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Desktop/DesktopManager.cs#L326-L428) |
| E04 | 读取当日任务、比较数量和计算忠诚系数 | [GameManager.cs:L494–L539](../../references/DeathAndTaxes/Assets/_Scripts/Core/GameManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Core/GameManager.cs#L494-L539) |
| E05 | 按角色 ID 保存和更新处置标记 | [PlayerState.cs:L1180–L1225](../../references/DeathAndTaxes/Assets/_Scripts/Desktop/Player/PlayerState.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Desktop/Player/PlayerState.cs#L1180-L1225) |
| E06 | 保存叙事变量与当前日，并恢复日/夜状态 | [SaveManager.cs:L61–L119](../../references/DeathAndTaxes/Assets/_Scripts/Core/SaveManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Core/SaveManager.cs#L61-L119) |
| E07 | 日节点沿条件分支推进 | [GameManager.cs:L432–L469](../../references/DeathAndTaxes/Assets/_Scripts/Core/GameManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Core/GameManager.cs#L432-L469) |
| E08 | 传真提交动画连接日终处理 | [FaxMachine.cs:L249–L319](../../references/DeathAndTaxes/Assets/_Scripts/Desktop/Objects/FaxMachine.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Desktop/Objects/FaxMachine.cs#L249-L319) |
| E09 | 代码 MIT 许可与保留声明要求 | [LICENSE:L1–L18](../../references/DeathAndTaxes/LICENSE) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/LICENSE#L1-L18) |
| E10 | 作者明确代码开放范围与美术资源限制 | [README.md:L31–L47](../../references/DeathAndTaxes/README.md) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/README.md#L31-L47) |
| E11 | 旧存档使用 BinaryFormatter | [SaveManager.cs:L148–L173](../../references/DeathAndTaxes/Assets/_Scripts/Core/SaveManager.cs) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/Assets/_Scripts/Core/SaveManager.cs#L148-L173) |
| E12 | 实际 Unity 版本 | [ProjectVersion.txt:L1–L2](../../references/DeathAndTaxes/ProjectSettings/ProjectVersion.txt) | [GitHub](https://github.com/oakwarrior/DeathAndTaxes/blob/8fc4bb9211f09a5296ef081e1359cdb27f313688/ProjectSettings/ProjectVersion.txt#L1-L2) |

## 调用与数据流

1. `DesktopManager.StartDay(restoreFromSave)` 先准备当天桌面内容，再取得角色、更新任务信和日历；新日才清零当日统计（E01）。这区分了“进入新的一天”和“读档恢复当天”。
2. 每张角色资料绑定 Articy `template_profile`。玩家的标记保存在 `CurrentDayProfiles`，按 `ProfileID` 更新，而不是只存在按钮或纸张上（E05）。
3. 传真交互根据 `AreAllMarked()` 展示完整或未完成提示。实际传输协程先调用 `NotifyEndDay()`，等待动画，再调用 `DesktopManager.FinishDay()`（E08）。
4. `NotifyEndDay()` 汇总当日死亡/赦免；`FinishDay()` 对每位角色选用相应后果，更新累计状态，再计算未达职业或年龄规则的偏差次数（E02、E03）。
5. 当日任务和数量统计供忠诚评价、工资及叙事读取（E04）。这里的“达到数量”是旧游戏规则，不能作为我们净化配额定义的最终答案。
6. `ProgressDay()` 沿 Articy 输出连接遍历条件节点，选择下一个日节点；不是简单地加载场景序号加一（E07）。
7. 存档保存 Articy 变量与日节点 ID，恢复时根据是否已提交传真重建白天/夜晚和角色内容（E06）。场景、叙事与运行状态存在明显耦合。

## 四类借鉴

| 分类 | 本游戏采取的方式 |
|---|---|
| 直接引入 | 暂不整库导入。MIT 允许保留声明后复用相应代码，但先限定到可独立验证的小片段。 |
| 改写结构 | 高优先：每日初始化、按稳定角色 ID 记录处置、日终集中结算、不同选择产生不同后果。 |
| 表现参考 | 指令、行政工具、提交动作与制度反馈形成压力；可转成终端任务通知和结算反馈。 |
| 自研缺口 | 治疗/净化/释放三选项、每位病人隐藏信任、治疗递进、晨检、梦境及本游戏三结局条件。 |

它值得借鉴的是把制度要求与角色后果并排计算。我们不应加入旧游戏的生态、繁荣、金钱、商店和 28 天内容。
本游戏的仪器读取情绪，不能把角色的检测值写成客观善恶或处置正确性的判定。

## 具体改造步骤

1. 建立固定五位病人的静态定义和独立运行状态；以稳定 ID 关联资料、每日对话入口和处置历史，替代 Articy 生成角色对象（E05）。
2. 用三天内容配置记录当天可出现病人与制度任务；已释放、净化或终止治疗的角色是否再次出现，遵循设计确认后的规则。
3. 将处置提交集中到一个入口：检查对话结束、可用选项和当前病人，接受后只执行一次状态变化。
4. 治疗增加该病人的治疗阶段；净化更新当天完成量；释放记录离场及后续事件。数值、代价和例外继续保留为待定配置。
5. 借鉴 E02、E03，在日终核算配额、处置历史和待触发事件，避免视觉动画本身承担结局计算。
6. 将终端结束、回到房间、床交互、梦境和次日晨检分成明确阶段，替代传真、商店和电梯之间的旧流程。
7. 三种结局改为读取本游戏状态的条件规则；神父释放事件与配额失败若同时成立，其优先级尚待设计决定。
8. 如果确定需要存档，保存独立状态和叙事游标；参考 E06 的恢复顺序，自选可审查的序列化方案，不迁入 E11 的旧格式。

## 依赖与改造成本

| 部分 | 依赖及成本判断 |
|---|---|
| 原工程整体 | Unity 2019.2.10f1、Articy 生成类型、静态单例、场景和大量专属物件；整库迁移成本高，当前不选。 |
| 处置与日终结构 | 需拆除纸张、年龄职业与世界指标耦合；重写为五位病人状态，成本中等。 |
| 内容和结局 | 需要我们的每日对白、反应、梦境及结局条件；参考库不能替代内容制作，成本取决于设计完成度。 |
| 存档 | 原代码重建多种场景对象；我们的状态规模较小，可缩减，但保存范围尚未在需求中确定。 |

Unity 2022.3 对这些 C# 结构的可读性不等于项目可直接编译。尚未核查 Articy 插件升级、场景引用和所有第三方包。

## 代码与资产许可

- 根 `LICENSE` 和源文件声明使用 MIT；直接复制或改编代码应保留版权与许可文字（E09）。
- 作者在 README 明确区分开放代码与未开放美术等资产，说明移除了部分资源，不能据根许可证把画面、角色或音频直接带入项目（E10）。
- Articy 和 TwitchLib 等内容存在各自许可；使用时需检查所选文件所属组件，而不是统一按根 MIT 标记。
- 当前建议使用机制和数据流，重新制作本游戏人物、场景、文本、声音与交互界面。

## 后续验证场景

- [ ] 同一天读档不重新清零配额或重复生成已经处置的病人。
- [ ] 同一病人连续点击处置，只计数一次并只触发一次后果。
- [ ] 治疗后次日阶段正确；释放/净化后角色是否返场符合确认规则。
- [ ] 配额未达、恰好达到、超过目标分别验证；比较符号须来自设计而非照搬 E04。
- [ ] 神父事件、配额失败和完整流程结局分别可达；重叠条件按确认优先级处理。
- [ ] 终端关机动画结束后恢复房间控制，床门控和梦境不会跳过日终结算。
- [ ] 若采用存档，在对话中、处置后和夜间恢复不会丢失状态或重复执行剧情命令。

## 结论

优先把它用于处置与制度压力的设计参考，并提炼小规模的状态结构。它是成熟游戏的源码样本，不是已验证的 Unity 2022.3 模板，也没有实现我们的病人信任规则。

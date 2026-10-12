# DeadLetterOffice_Day1：教程插入、提交时序与行政反馈

原项目最具体的参考点是提交当前内容、显示反馈、清理窗口、进入下一批的时序，以及在常规流程中插入教程和特殊内容。

## 研究基线

| 项目 | 记录 |
|---|---|
| 研究日期 | 2026-10-12 |
| 目标引擎 | Unity 2022.3 LTS；渲染管线待定 |
| 原仓库 | `DeadLetterOffice_Day1` |
| 来源 | [https://github.com/AnhKimHaTran/DeadLetterOffice_Day1](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1) |
| 本地目录 | `references/DeadLetterOffice_Day1/` |
| 固定 SHA | `cb68456f96ed60e4a7e1e03ce6ccbe83e2227063` |
| 实际引擎版本 | Unity 2022.3.16f1（E08） |
| 仓库性质 | 邮件审核桌面游戏的 Day 1 工程 |
| 研究优先级 | 中低：教程与交互表现参考 |
| 验证状态 | 已核对本地源码与来源锁；仅静态检查，未编译、未运行 |

需求依据：[游戏总览](../../docs/game-overview.md)；决策汇总：[借鉴矩阵](../reuse-matrix.md)；实施顺序：[Unity 2022 路线](../unity-2022-roadmap.md)。

## 实际实现证据

| ID | 已核查行为 | 本地源码与行号 | 固定版本永久链接 |
|---|---|---|---|
| E01 | 根据天数和处理序号插入固定教学/特殊邮件 | [MailGenerator.cs:L88–L124](../../references/DeadLetterOffice_Day1/Assets/Sprites/Letter/Scripts/Generate/MailGenerator.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Letter/Scripts/Generate/MailGenerator.cs#L88-L124) |
| E02 | 点击提交先校验，再移走内容并决定下批/日终 | [ClickCoffeeCheck.cs:L25–L89](../../references/DeadLetterOffice_Day1/Assets/Sprites/Letter/Scripts/Generate/ClickCoffeeCheck.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Letter/Scripts/Generate/ClickCoffeeCheck.cs#L25-L89) |
| E03 | 字段校验与违规票据反馈 | [CheckReviewAccuracy.cs:L35–L123](../../references/DeadLetterOffice_Day1/Assets/Sprites/Review%20Sheet/Scripts/CheckReviewAccuracy.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Review%20Sheet/Scripts/CheckReviewAccuracy.cs#L35-L123) |
| E04 | 处理计数越过目标时播放日终 Timeline | [ScoreTracker.cs:L8–L45](../../references/DeadLetterOffice_Day1/Assets/Sprites/Score%20Tracker/ScoreTracker.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Score%20Tracker/ScoreTracker.cs#L8-L45) |
| E05 | 收入与罚金结算写入 UI | [ScoreTracker.cs:L60–L112](../../references/DeadLetterOffice_Day1/Assets/Sprites/Score%20Tracker/ScoreTracker.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Score%20Tracker/ScoreTracker.cs#L60-L112) |
| E06 | 固定末尾人物名字与常规随机名字 | [MailProperties.cs:L49–L93](../../references/DeadLetterOffice_Day1/Assets/Sprites/Letter/Scripts/Generate/MailProperties.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Letter/Scripts/Generate/MailProperties.cs#L49-L93) |
| E07 | 下一日直接加载后一个场景 | [nextDay.cs:L15–L27](../../references/DeadLetterOffice_Day1/Assets/Sprites/UI/Sleep_NextDay/nextDay.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/UI/Sleep_NextDay/nextDay.cs#L15-L27) |
| E08 | 实际 Unity 版本 | [ProjectVersion.txt:L1–L2](../../references/DeadLetterOffice_Day1/ProjectSettings/ProjectVersion.txt) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/ProjectSettings/ProjectVersion.txt#L1-L2) |
| E09 | 仅导出插件自己的许可证 | [LICENSE:L1–L20](../../references/DeadLetterOffice_Day1/Packages/ca.tim-cstj.export-project-to-zip/LICENSE) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Packages/ca.tim-cstj.export-project-to-zip/LICENSE#L1-L20) |
| E10 | 滑走表现脚本引用 UnityEditor | [moveDocumentsAway.cs:L1–L28](../../references/DeadLetterOffice_Day1/Assets/Sprites/Review%20Sheet/Scripts/moveDocumentsAway.cs) | [GitHub](https://github.com/AnhKimHaTran/DeadLetterOffice_Day1/blob/cb68456f96ed60e4a7e1e03ce6ccbe83e2227063/Assets/Sprites/Review%20Sheet/Scripts/moveDocumentsAway.cs#L1-L28) |

## 调用与数据流

1. `MailGenerator.GenerateMail()` 读取 `ScoreTracker.MailCounter`，先尝试特殊邮件；Day 1 根据指定序号安排教程、错误邮件和末尾事件，其他情况生成常规邮件（E01）。
2. `MailProperties` 根据当前日和计数选择固定人物名字，其他邮件随机生成名字；内容与流程存在计数耦合（E06）。
3. 玩家点击咖啡杯提交时，查找当前审核单并调用 `checkAccuracy()`。审核依次检查是否该丢弃、发送者和接收者字段（E02、E03）。
4. 校验成功增加 `MailCorrect`；失败增加 `MailIncorrect`，生成带错误原因的票据。这是客观字段比对，不是病人道德判断（E03）。
5. 提交器把邮件、信和审核单加上滑走脚本，延时销毁，随后决定继续生成或播放教程/日终 Timeline（E02）。
6. `MailCounter` 的属性 setter 以 `value > MailGoal` 触发结束，并将数值压回目标；日终收入和罚款按正确/错误数量写入 UI（E04、E05）。
7. “睡眠/下一日”脚本加载当前场景序号加一，没有在此实现跨天病人状态或结局选择（E07）。

## 四类借鉴

| 分类 | 本游戏采取的方式 |
|---|---|
| 直接引入 | 当前不直接引入游戏代码或素材；未找到适用于游戏本体的许可证。 |
| 改写结构 | 中低优先：教程走通正式流程、关键序号插入固定事件、提交后统一清理窗口与显示行政反馈。 |
| 表现参考 | 桌面物件反馈、票据出现、内容滑走和结算提示；可转化为扫描回执和终端处置反应。 |
| 自研缺口 | 跨日病人、完整多轮对话、隐藏信任、治疗递进、三种处置、房间控制和三个结局。 |

我们可以用固定特殊病人演示操作，但 Day 0 是否计入预算未定，不照抄原游戏的教程序号。
身份字段是否匹配的正确答案系统不能替代本游戏的情绪检测；旧游戏随机邮件、收入和罚金也不属于当前固定五人交付范围。

## 具体改造步骤

1. 将教程与正常病人放进同一个终端流程，使用独立教程标记控制提示，而不是创建一套只在教程中能工作的按钮。
2. 以“日序号 + 病人 ID + 流程事件”安排特殊事件，替代 `MailCounter + 1 == 2/5/7` 等硬编码判断（E01）。
3. 把校验入口改为处置前置检查：对话已结束、病人仍可处置、该选项已开放；不校验玩家选择是否符合善恶真值。
4. 处置被接受后先记录状态，再播放对应反应、故障/闪烁与离线表现；表现完成才清理窗口、切换下一人。
5. 单次提交期间锁定按钮，借鉴滑走/清理的时序但明确取消与重入行为，防止重复点击生成两批内容（E02）。
6. 将病人数与净化配额分开：所有病人结束决定终端工作完成，净化计数决定制度目标是否满足；不能把 `MailGoal` 同时当作两者。
7. 如果设计需要扫描回执或行政警告，做独立文本/表现配置；不照搬票据错误字段与薪金规则（E03、E05）。
8. 当天结束后回到公寓，开放床并进入梦境与次日晨检；不用场景索引递增承载所有跨天逻辑（E07）。

## 依赖与改造成本

| 部分 | 依赖及成本判断 |
|---|---|
| 引擎版本 | Unity 2022.3.16f1 与目标同一 LTS 系列，但尚未验证安装版本、包、场景和构建配置。 |
| 原审核流程 | TextMeshPro、PlayableDirector、标签查找、固定对象层级、邮件数据和 Timeline；拆出业务机制成本中等。 |
| 窗口与反馈 | 可以自写简单序列，保留视觉时序；原创素材与声效另行制作，成本低到中等。 |
| 日终与剧情 | 主要第一天实现，不提供固定五人三天的跨日记录；需要新建状态和内容工具。 |

静态注意事项：`MailCounter` 由生成与提交流程共同驱动，结束条件用 `>`；迁移前必须重新定义“进入的人数”和“已处理的人数”语义（E02、E04）。
`moveDocumentsAway.cs` 引入 `UnityEditor`（E10）；未构建前不能保证玩家构建通过。即使不使用其 API，也应在实际复用时核对编译边界。

## 代码与资产许可

- 文件扫描未找到游戏本体根 LICENSE/LICENCE，也没有看到游戏脚本统一的使用授权声明。
- 发现的 `ca.tim-cstj.export-project-to-zip/LICENSE` 属于第三方导出插件，不能授权邮件代码、文本、图片或字体（E09）。
- 仓库包含多种字体、像素美术和音频；LFS 下载完整只说明文件存在，不说明可发行复用。
- 当前路径是根据流程重新实现，不从本库复制正式素材；未来直接复用需明确对应代码/素材的许可范围。

## 后续验证场景

- [ ] 教程结束后与正常病人共用同一个处置入口，提示不会残留或阻止后续流程。
- [ ] 处置按钮在对话中禁用；对话完结后按规则开放，未开放释放不接受点击。
- [ ] 反应/闪烁期间连续点击不会重复结算、重复生成病人或销毁新窗口。
- [ ] 当天最后病人结束恰好关闭工作流程，没有额外生成一个“越过目标”的病人。
- [ ] 全部人已处理但净化配额未达，仍记录失败结果并进入确认后的后续剧情。
- [ ] 固定病人提前离场时，教程/特殊事件按 ID 正确定位，不因队列长度变化错位。
- [ ] 转场只运行一次，回到房间后仅允许当前阶段的 E 键交互。
- [ ] 在 Unity 2022.3 建立最小测试场景后分别检查编辑器播放与玩家构建。

## 结论

适合借鉴提交时序、教程插入和行政反馈，不能因版本接近就选作核心工程骨架。先把这些小机制改写成适合终端的结构，再验证本游戏的跨日状态。

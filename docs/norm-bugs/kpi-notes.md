# 净化要求进度

- 目录与复核页增加 `净化要求：n/m`；原有“净化配额”继续表示剩余额度。复核页状态文字缩为 24 像素高，为进度标签留出间隔。`[verified: TerminalView.cs L101、L151–152、L254]`
- `n`／`m` 直接读取当前 `GameSession` 日记录的 `Purifications`／`PurificationQuota`，经只读 `PlayerView.PurificationsToday`／`PurificationRequirement` 暴露；未新增结算、存档字段或独立计数。`[verified: GameSession.cs GetPlayerView；GameResult.cs PlayerView]`
- 已执行数量显示真实值；超过要求时例如显示 `2/1`，不从已截为零的剩余额度反推数量。教程同样读取实际要求。`[verified: TerminalView.cs L254；PurificationProgressTests.cs AssertProgress]`
- 新文案所需 `净化要求：012/` 均已有 14 像素琥珀字体导出；无需生成新素材。`[verified: 本轮 HTML_FontMetrics.json code/size/rgb 检查结果 Covered14 全部 True]`

## Deviations

- 为 UI 新增两项核心只读投影，主代理已授权；处置和日结规则未修改。`[verified: 本轮主代理批准及 GameResult.cs／GameSession.cs 差异]`

## Discovered edge cases

- 剩余配额在超额净化后固定为 0，不能作为“已执行数量”的依据。新增核心测试覆盖超额、重复提交、恢复及跨日重置；新增 PlayMode 测试覆盖目录／复核、治疗不增加计数、恢复和跨日标签。`[verified: GameSession.cs L65；PurificationProgressTests.cs；NormPurificationKpiPlayModeTests.cs]`
- 首次 Unity 切片暴露测试夹具的 `PrototypeContent` 不支持自定义开场节点；改为本测试自己的 `IGameContent`。跨日夹具启用正式梦境流程，断言床 → 睡眠 → 梦境 → 自动次日，等待重测。`[verified: Artifacts/NormBugs/Validation/slice.xml KPI failure；NormPurificationKpiPlayModeTests.cs KpiContent／WithDreamFlow]`

## Questions for review

- 无。运行验证由主代理串行执行；本子任务未启动 Unity／SDK。`[verified: 本轮执行记录]`

变更偏差：1 项。`[verified: 本文 Deviations]`
最可能回看：目录中剩余额度与完成进度的并列展示。`[opinion]`
发现边界：2 项。`[verified: 本文 Discovered edge cases]`
验证入口：`PurificationProgressTests`／`NormPurificationKpiPlayModeTests`。`[verified: 新增测试文件]`
后续先读：本文及主代理最终验证记录。`[opinion]`

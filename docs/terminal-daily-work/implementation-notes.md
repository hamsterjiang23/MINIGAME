# Deviations

- 参考 v0.8.13 增加“当日指标 / 净化剩余”，直接读取 GameSession 投影视图中的剩余配额；HTML 的测试初值5不覆盖游戏规则。
- 原有“净化要求：已完成/要求”移至人员复核标题同行，保留既有进度显示，为参考图中的新增指标留出独立一行。

# Discovered edge cases

- 计数与目录配额共用权威状态；治疗、释放、重复处置、读档及跨日均通过运行时视图更新。
- 开机逐字符随机音效已存在，保留原始四段 UIType WAV 和独立打字通道，本轮补充完整真实开机时间轴与原生资源加载回归。

# Questions for review

- 每日正式配额与失败判定沿用现有规则，本轮只补充显示。

偏差数：1，旧进度移至复核标题同行，以容纳参考 HTML 的独立剩余额度行。`[verified: TerminalView.cs L151-L158]`
复核重点：剩余额度与已完成进度都来自会话投影，不另存计数。`[verified: TerminalView.cs L259-L260]`
边界记录：处置去重、读档、跨日、超额净化、光标与重绘去重、原生音频加载。`[verified: verification.json indicator、typing]`
验证：10项相关 Play Mode 检查通过，Windows构建成功，独立Player启动就绪。`[verified: verification.json playMode、build、playerStartup]`
后续入口：[使用与验证](guide.md)及[来源清单](source-manifest.json)。`[verified: guide.md；source-manifest.json]`

# 社会适应度波形

电脑审查界面在原有监控画面下方加入“社会适应度”检测区，保留左侧监控／对话、右侧处置／应答布局。日期、净化配额、四条应答与原有 CRT 合成保留；顶部终端编号、右侧机构名称和可见的“人员复核”行移除。监控原有 464×261 区域不变，对话记录缩为可滚动的 478×76 区域。[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs constructor]

## 数值与显示

- `GameSession.GetPlayerView()` 为配置了释放处置的当前会谈提供只读 `SocialAdaptationReading`。它只暴露相对本场门槛的差距，不公开原始信任或其他人物历史；没有会谈或本场不配置释放时不提供读数。[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs GetPlayerView；../../src/MiniGame.Core/Contracts/GameResult.cs SocialAdaptationReading]
- 释放仍由核心原有 `Trust >= ReleaseTrustThreshold` 判定。显示端的归一化只控制动画，不钳制、结算或另存权威信任。负数、零门槛、不同会谈门槛及整数极值都有覆盖。[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs CanRelease；../../src/MiniGame.Core.Tests/SocialAdaptationTests.cs]
- 两条固定水平虚线围成许可带。低信任时振幅越界，频率更快，叠加不规则尖峰、成簇颤动和忽急忽缓的节奏；恰好达标时振幅触及边界，继续提高信任后振幅收小。装饰噪声受振幅包络约束，不能让达标波形越界。检测区没有分数、百分比或数值刻度。[verified: ../../src/MiniGame.Presentation/SocialAdaptationSignal.cs Parameters/Sample；../../src/MiniGame.Presentation.Tests/SocialAdaptationSignalTests.cs]
- 换会谈时重置显示，门槛跨越时立即对齐真实资格；同侧数值变化平滑过渡。暂停冻结动画。目录、归档、自审和开场标志不借用上一角色的检测读数。[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalAdaptationWaveform.cs SetReading/Update；../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs Render]

## 当前验证

- 固定 .NET SDK 8.0.424：锁定依赖恢复、Release 构建通过，0 警告／0 错误；Core 154 项、Presentation 281 项通过，共 435 项。[verified: social-adaptation-2026-10-27/verification.json dotnet]
- Unity 隔离工程使用本机已安装的 2022.3.23f1c1，源工程仍记录 2022.3.23f1，未修改源工程版本。[verified: social-adaptation-2026-10-27/verification.json unity]
- 新增 PlayMode 专项 1 项通过，覆盖真实回答驱动、达标显示、四条应答、CRT 鼠标命中、暂停冻结、中文字形及实际 RenderTexture 像素。已有 `TerminalV0824` 终端回归 24 项通过。[verified: social-adaptation-2026-10-27/verification.json unity.specialized/unity.regression]
- 回归中将配额与处置位置断言同步到新布局，修正了配额贴图名称相同导致的选择歧义；对白工具测试比较忽略 Windows／Unix 换行差异，保留对白内容比较。[verified: ../../UnityProject/Assets/MiniGame/Tests/PlayMode/TerminalDailyWorkIndicatorPlayModeTests.cs AssertIndicatorLayout；../../UnityProject/Assets/MiniGame/Tests/PlayMode/TerminalDispositionPlayModeTests.cs；../../UnityProject/Assets/MiniGame/Tests/PlayMode/NormPurificationKpiPlayModeTests.cs AssertReviewRemaining；../../src/MiniGame.Presentation.Tests/AuthoringPackageTests.cs]

[低信任实际渲染](social-adaptation-2026-10-27/low.png)／[达标实际渲染](social-adaptation-2026-10-27/passed.png)。捕获使用隔离工程中的真实 Unity 终端和开发测试规则，验证呈现与核心接入，不代表正式 Campaign 完整游玩验收。尚未重建或替换 NORM 外发程序。[verified: ../../UnityProject/Assets/MiniGame/Tests/PlayMode/SocialAdaptationPlayModeTests.cs；social-adaptation-2026-10-27/verification.json scope]

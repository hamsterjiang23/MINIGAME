# 已修复问题复核

2026-10-20；按 `NORM调整建议.docx` 第 2–14 段核对当前源码。以下“保留”表示本轮读到了对应实现，不等于本轮运行验证已通过。`[verified: Artifacts/NormBugs/docx-20261020/content.json paragraphs]`

- **30 FPS／关闭垂直同步：保留。** `PlayerRuntimeSettings.cs:7–14` 在场景加载前设置 30 和 0；当前游戏代码未再切换画质级别。录制工具的 60 FPS 仅由专用命令行选项启用。`[verified: PlayerRuntimeSettings.cs L7–14；OpeningMotionProbe.cs L89–98；TerminalClosingRecorder.cs L77–103]`
- **多段长对白第一行丢失：保留，增加衔接回归。** 完成条目显示完整 `entry.Text`，当前未完成条目才显示播放页；行高使用当前位图字体规则计算。附件两张截图指向左侧对白记录，不是右侧单行应答命令；本轮不扩大应答命令布局。`[verified: TerminalTranscriptView.cs L62–70；docx media/image1.jpeg、image2.jpeg]`
- **对话滚轮／阅读位置：保留，增加追加条目回归。** `ScrollRect` 接收滚轮，上翻后按像素保存阅读位置；返回底部才自动跟随新内容。新增测试覆盖追加打字行和回到底部后的自动跟随。`[verified: TerminalTranscriptView.cs L33–41、L52–77；NormExistingRegressionPlayModeTests.cs WheelReadingOffsetSurvivesAnAppendedTypingRowAndCanReturnToLatest]`
- **退出游戏：保留。** 暂停菜单的“退出游戏”调用真实 `Application.Quit()`；旧非开发版 Player 报告记有实际退出。本轮仍需对新构建复测。`[verified: MiniGameScreen.cs L590–593；Artifacts/Unity/PlaytestFeedback20261020/PauseAndQuitRelease/verification.json quitViaMenu]`
- **暂停后翻转：保留。** 暂停抓图在模糊后仅依据图形后端进行一次 UV 修正，未转动世界相机。旧 D3D11 Player 报告覆盖终端／房间以及两种尺寸，本轮新构建结果另记。`[verified: PauseMenuBackdrop.cs L79–89；Artifacts/Unity/PlaytestFeedback20261020/PauseAndQuitRelease/verification.json samples]`
- **鼠标灵敏度：补齐旧场景。** 默认值和正式 `MainRoomCombined` 都是 0.7；发现仍会打包的 `InspectionApartment` 残留 2，本轮将其同步为 0.7。`[verified: PlayerRuntimeSettings.cs L8；ApartmentPlayer.cs L13；MainRoomCombined.unity L6681；InspectionApartment.unity L2018]`
- **先睡觉再做梦：保留。** 玩家夜间与床交互先创建 `Sleep` 演出，完成后才提交梦境命令；Day0 一次睡眠直接进入 Day1。现有核心测试覆盖暂停、读档及两个演出阶段。`[verified: GameRuntime.cs L139–148、L275–278、L338–339；DreamFlowRuntimeTests.cs L31–61]`
- **物理 CRT 保持实时 UI／再次进入：保留。** 只要通电就保留原生 UI 渲染源，关闭全屏输出不关闭源；材质持续使用当前终端纹理。现有测试覆盖返回房间、暂停、读档、重新进入和最终熄灭。`[verified: MiniGameScreen.cs L449–450；PresentationEffects.cs L462–468；PoweredTerminalDisplayPlayModeTests.cs WorldExitPauseAndRestoreKeepLiveUiUntilTheDailyShutdown]`

## Deviations

- 在原定“核对已修条目”之外，同步了仍会打包的旧演示场景灵敏度；主代理确认该场景仍需覆盖。`[verified: 本轮主代理授权修改 InspectionApartment.unity 中 lookSensitivity 行]`

## Discovered edge cases

- 完成条目的静态快照测试不足以覆盖“最后一页 → 应答命令”衔接；新增实际 `MiniGameScreen`＋位图文字回归，完成后断言两段全文和可用应答同时存在。`[verified: NormExistingRegressionPlayModeTests.cs CompletedBitmapSpeechShowsItsFirstLineWhenReplyCommandsAppear]`
- 旧 `CampaignPresentationPlayModeTests.HistoryAndPauseFreezeTypingAndBlockContinueUntilTheyClose` 仍引用已移除的 `HistoryTab`。本轮新增回归不依赖 F3／记录页；未更改该历史测试。`[verified: CampaignPresentationPlayModeTests.cs L79–92]`

## Questions for review

- 无新增产品决定。Unity、构建和 SDK 由主代理串行执行；本子任务未启动这些进程。`[verified: 本轮执行记录]`

变更偏差：1 项。`[verified: 本文 Deviations]`
最可能回看：旧演示场景是否继续打包。`[opinion]`
发现边界：2 项。`[verified: 本文 Discovered edge cases]`
验证入口：`NormExistingRegressionPlayModeTests` 两项，以及 `PlaytestFeedbackPlayModeTests`、`DialogueBitmapPaginationTests`、`PoweredTerminalDisplayPlayModeTests`。`[verified: 本文源码核对与新增测试]`
后续先读：本文及主代理最终验证记录。`[opinion]`

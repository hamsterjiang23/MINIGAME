# Deviations

- 用户最新要求覆盖先前的“目录手动关机”：目录按钮现在只退出操作界面，电脑继续通电；全部当天会谈与反应完成后才关闭会话和熄屏。`[verified: 本轮用户“每天一次开机一次关机”]`
- Day0 启动继续使用已确认的监管者全屏监控与拉远，计入当天首次通电；Day3 梦后的本人复核直接显示实时安全画面，不再次播放日常开机音效或自检。`[opinion]`

# Discovered edge cases

- 开关机与界面开闭必须分别保存：`TerminalPowerDay` 与 `TerminalPower` 保存 Off / Booting / Running / ShuttingDown；中途退出后 Running 保持，跨日归零。`[verified: src/MiniGame.Presentation/GameRuntime.cs TerminalPowerDay / TerminalPower]`
- 老存档没有电源字段时，正在工作或打开终端视为仍在通电；已有退出演出的最后会谈恢复为关机段，避免重复结算处置。`[verified: src/MiniGame.Presentation/GameRuntime.cs InferLegacyTerminalPower]`
- 暂停中的开关机存档恢复后，音效等待取消暂停再开始；暂停不会在 UI 重入时重启电源段。`[verified: UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs PowerOn / PowerOff]`
- Day3 进入本人复核时自动点亮安全屏，恢复旧的 Off 本人复核存档也会归一为 Running；不新增开机演出或结算副作用。房间灯光逐槽保留 `_CrtPower`。`[verified: GameRuntime.cs Accept / Restore; RoomLightingController.cs ApplySurface; DreamFlowRuntimeTests.cs RestoringUnpoweredSelfReviewEnablesItsSafetyScreenWithoutBootOrCoreChanges]`
- 原音库没有专用开关机条目，新增原创程序合成继电器与 CRT 升降调音效；44.1 kHz、PCM16 单声道，生成脚本与原文件保存在仓库中，可通过现有音频设置替换。`[verified: tools/audio/generate-terminal-power.ps1 Write-TerminalSound; MiniGameAudioProfile.cs TerminalPowerOn / TerminalPowerOff]`
- 29 项定向 .NET 测试通过，包含新增 3 项电源保存、跨日、关闭后再进入与最终自动熄屏测试；Unity 与 Player 检查由主代理串行执行。`[verified: 本轮 dotnet test 输出 Passed29 / Failed0]`

# Questions for review

- 无需新增用户决策。`[opinion]`

偏离数：2；主要为最新手动退出语义覆盖旧手动关机语义。`[verified: Deviations 两项]`
最可能复查项：本人复核的安全画面通电显示。`[opinion]`
边界项：6。`[verified: Discovered edge cases 六项]`
后续先读：GameRuntime 的电源字段、CompleteCue 与 InferLegacyTerminalPower。`[opinion]`
最终验证：每日通电专项PlayMode通过；实际Player确认初始熄屏、中途保持亮屏、免开机重进及最终熄屏，开关机音效各一次；本人复核实际场景回归通过。总计302项.NET、30项PlayMode及两项Player检查，见[统一验证](../crt-screen-fit/verification.json)。`[verified: Artifacts/CrtScreenFit/daily-power.xml；Artifacts/CrtScreenFit/self-review.xml；../crt-screen-fit/verification.json]`

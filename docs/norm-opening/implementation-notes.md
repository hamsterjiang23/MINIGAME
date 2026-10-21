# Deviations

- 用户最终要求使用透明标志居中，不添加黑底；原PNG完整保留，复用已有终端监控表面。`[verified: 用户最新“不是黑底，只要放到终端的center即可”]`

# Discovered edge cases

- 原图为2172×724透明PNG，按原比例居中显示；点击后随既有镜头拉远，进入教学时恢复监管者视频。没有新增底板、菜单或输入操作。`[verified: TerminalView.SetOpeningLogo；MiniGameScreen.BeginDayZeroOpening/EndDayZeroOpening]`
- 开场镜头会按窗口比例裁取监控视口，标志宽度按较窄视口的82%计算，避免4:3或竖向窗口裁掉两边；进入教学恢复完整监管者矩形。`[verified: TerminalView.FitOpeningLogo；DayZeroOpeningPlayModeTests.NormLogoPullsBackIntoTheTerminalThenOneButtonTutorialAndOneSleepStartDayOne]`
- 未完成开场的读档仍从等待点击起点恢复，暂停保持原进度。验证继续使用真实Day0流程。`[verified: DayZeroOpeningPlayModeTests.PausedPullbackFreezesAndUnfinishedOpeningRestoresAtItsWaitingStart]`

# Questions for review

- 无。`[opinion]`

偏差数：1，按用户最后要求保留透明原图，不增加黑色底板。`[verified: Deviations]`
可再调项：标志占用视口的比例。`[opinion]`
边界数：3，透明资源、不同视口比例、暂停与恢复。`[verified: Discovered edge cases]`
验证：2项PlayMode、Windows Mono构建与实际Player开场到Day1通过；见verification.json。`[verified: verification.json playmode/build/player]`
后续入口：TerminalView.SetOpeningLogo/FitOpeningLogo、MiniGameScreen.BeginDayZeroOpening/EndDayZeroOpening。`[verified: 上述方法源码]`

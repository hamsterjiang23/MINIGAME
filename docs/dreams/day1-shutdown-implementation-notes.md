# Deviations

- 修复每日关机后的返回镜头：保留原来的返回位置，朝向床聚光区域；同步玩家yaw/pitch，防止下一帧转头重置。普通离开仍保留原朝向。`[verified: PresentationEffects.cs CameraTransition shuttingDown 分支；BedtimeGuidance.cs SleepFocusPoint]`

# Discovered edge cases

- 完整三日流程回归手动推进梦境时钟，不能单独证明正常等待与输入的体验。新增原生时钟的关机→床→梦境→Day2检查。`[verified: CampaignPlayModeTests.cs RunCampaign；DreamCampaignIntegrationTests.cs DayOneShutdownReturnsToTheVisibleBedAndNaturalDreamReachesDayTwo]`
- 首轮检查错误地等待AfterDream；当前床触发的梦境完成后直接推进到次日，应以Day2作为完成条件。这是测试断言修正，不能作为生产缺陷证据。`[verified: GameRuntime.cs CompleteCue 的 _dreamFromBed 分支；Artifacts/Day1Shutdown20261021/before-test.json]`
- 实际电脑朝向重现全黑：状态是Evening/World、锁定=false，床在视野外。默认出生朝向能看见床，掩盖了问题；测试现在先释放旧开场镜头，再设置电脑前的真实返回位置。`[verified: Artifacts/Day1Shutdown20261021/editor.log DAY1_SHUTDOWN_STATE；Artifacts/Day1Shutdown20261021/before-black.png]`

# Questions for review

- 用户已确认是Day1全员对话完毕、关机后，尚未上床。`[verified: 本轮用户回复「发生在第一天所有人对话之后，关机之后」]`

偏差：修复仅涉及Unity镜头，不改日期、工作配额或梦境规则。`[verified: PresentationEffects.cs CameraTransition；BedtimeGuidance.cs SleepFocusPoint]`
最可能调整：床焦点的取景高度。`[opinion]`
边界：默认出生朝向、电脑朝向、鼠标角度同步、正常时钟。`[verified: 上方Discovered edge cases；DreamCampaignIntegrationTests.cs 新增检查]`
后续先读：day1-shutdown-verification.json。`[opinion]`
旧Mono包保留，新包单独交付。`[opinion]`

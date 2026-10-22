# Deviations

- 角色出场日期按本轮要求更新，保留出场前对白的源表审计，运行包只包含当日可出场角色。`[verified: Tools/DialogueCampaign/build_campaign.py build]`

# Discovered edge cases

- 当前游戏进度先备份到 `Artifacts/FollowupRepair20261022/before/session.json`，再停止 Play Mode。`[verified: Unity eval saved=true]`
- 扫描音频有有效波形，调用改用当前界面绑定的音频负责人，并纳入暂停恢复；验证实际输出采样。`[verified: MiniGameYarnBridge.cs PresentLineAsync；MiniGameAudioDirector.cs Present]`
- 角色日程改变内容版本，由 `1.1.1-development` 更新为 `1.1.2-development`。`[verified: build_campaign.py build]`
- 主菜单、立绘目录保留，激活新对白包时不重建这些资源。`[opinion]`
- 终端使用固定五行对象池，已隐藏未到场角色对应的行，并把方向键索引限制在当天名单内。`[verified: TerminalView.cs SelectRoster/RenderRoster]`
- 灯光测试忽略停用的监控场景灯；监督员测试允许原有水平避门移动，继续检查立绘高度与逐句内容。`[verified: FollowupFlowPlayModeTests.cs DayZeroShutdownRestoresOnlyTheBedSpotlight；SupervisorDialoguePlayModeTests.cs EveryAuthoredLineAndExpressionPlaysOnceAcrossThreeMorningsAndTakenAway]`
- 旧检查点在新内容版本下实际抛出版本不匹配异常，测试新名单需开始新检查期；原进度备份保留。`[verified: Artifacts/FollowupRepair20261022/active-content-check.json result.result.oldSaveError]`

# Questions for review

- 独立代码审查没有发现可操作的新问题；没有执行目标平台构建或人工视听验收。`[verified: followup-verification.json tests.independentReview/limits]`

偏离数：1，出场前源表记录保留审计，但不进入运行对白。`[verified: 本文件 Deviations]`
可能需要复查：打包后的实际画面与扬声器播放。`[opinion]`
边界情况：名单对象池、方向键边界、音源归属、暂停恢复及旧存档版本。`[verified: 本文件 Discovered edge cases]`
针对性检查：.NET 347、Python 19、Unity 51 项通过；字体覆盖缺字为 0，Git 差异检查通过。`[verified: followup-verification.json tests]`
后续首先读取：本文件及 followup-verification.json；旧进度备份见 Artifacts/FollowupRepair20261022/before/session.json。`[opinion]`

# Deviations

- 本轮按用户六项要求执行，保留已有未提交改动。`[verified: 用户六项清单；git status --short]`

# Discovered edge cases

- 修改前备份当前梦后进度及受影响源文件，随后退出 Play Mode。`[verified: Artifacts/InteractionRepair20261022/before/session.json；Unity eval saved=true]`
- 处置按钮没有把悬停项同步到键盘选择；指针离开后会退回旧项。`[verified: TerminalView.cs dispositions 初始化；TerminalButtonVisual.cs OnPointerExit/Paint]`
- 角色移除只影响第二天起的终端名单，当前处置记录和核心历史保留；治疗次数直接使用 PatientView.TreatmentCount。`[opinion]`
- 睡眠增加镜头平移后，检查点必须保存起点和床上位置，旧检查点仍可恢复。`[opinion]`
- 梦境黑屏音乐 3→4.5 秒，画面 10→15 秒；返回黑屏停留保持 1 秒。`[opinion]`
- 审查 C1：跨日会把角色移到安全锚点，镜头结束位置改用角色相对眼位；C2：相应更新平移后仰视的时间断言。`[verified: PresentationEffects.cs StandingPositionForCurrentActor/RestoreSleepView；BedtimeGuidancePlayModeTests.cs SleepMovesOntoTheBedBeforeLookingUpAndWakeReturnsWithoutPopupsOrCheckpointJumps]`
- 审查 C3：旧检查点缺少平移字段，恢复时沿用演出启动前捕获的站立眼位，避免二次捕获半途镜头；添加缺字段恢复断言。`[verified: PresentationEffects.cs RestoreSleepCheckpoint；BedtimeGuidancePlayModeTests.cs checkpoint.HasPositionPose=false]`
- 扩展回归发现两项旧测试仍假设立绘回退及 F3 可开启记录；更新测试以匹配此前已生效的严格 ID 和禁用 F3 行为，保留记录查看器本身的覆盖。`[verified: TerminalHtmlParityTests.cs UnknownPortraitIdShowsNoImageAndValidIdClearsTheWarning/ReadOnlyRecordsPreserveTranscriptAndBlockUnderlyingChoicesWhileF3StaysDisabled；MiniGameScreen.cs HandleTerminalKey]`
- 严格 ID 的回归进一步发现 NO IMAGE 提示只随整体 Render 刷新，现已随每次立绘 ID 更新即时开关。`[verified: TerminalView.cs UpdatePortrait；TerminalHtmlParityTests.cs UnknownPortraitIdShowsNoImageAndValidIdClearsTheWarning]`
- 隐藏结局预览需要从新主菜单移交到运行宿主，现已补齐并隔离临时存档目录；停止后恢复原目录。`[verified: DreamCubemapPreview.cs Tick/Open/RestoreScenes]`

# Questions for review

- 隐藏结局已实际启动并移动 0.24 米；临时保存成功，停止后场景及存档路径恢复，两个正式存档哈希不变。`[verified: Artifacts/InteractionRepair20261022/hidden-preview-ready.json；hidden-preview-movement.json；hidden-preview-save.json；hidden-preview-restored.json]`
- 预览序列化产生的两处微小灯光变化和展开信件坐标变化已恢复到修改前备份；场景只保留本轮睡眠／敲门配置。`[verified: Artifacts/InteractionRepair20261022/scene-changes.diff]`

偏离数：1，扩展回归同时修正立绘警告刷新并更新旧测试预期。`[verified: 本文件 Discovered edge cases]`
可能需要复查：实际扬声器听感与睡醒演出的人工观感。`[opinion]`
边界情况：角色安全位移、旧睡眠检查点、指针间隙、首声计数、主菜单移交与预览存档隔离。`[verified: 本文件 Discovered edge cases]`
针对性检查：.NET 348、Unity 73 项通过，字体缺字为 0；实际预览启动、移动、保存和清理验证通过。`[verified: interaction-verification.json verification]`
后续首先读取：本文件和 interaction-verification.json；调试工具用法见 ../day-debug/guide.md。`[opinion]`

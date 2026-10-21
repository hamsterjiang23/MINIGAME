# Deviations

- 用户明确只借鉴海报、物件、门口监督员的交互逻辑；保留现有字体、边框规则、材质、颜色、排布与电脑角色交互。物件继续使用原来的整页旁白，监督员保留既有逐字与分页。`[verified: 本轮用户范围与美术约束；PropNarrationView.SetPage/Build；MiniGameScreen.PresentDialogueLine]`
- 本项目以专注阅读适配3D探索：物件阅读时暂停走动与鼠标转头，结束恢复；这是原创采用的交互约定，原作叙事物件是否锁视角未获得充分证据。`[verified: ApartmentPlayer.Update；原作NarrativeRoomObject.Init；本轮语义确认]`

# Discovered edge cases

- 当前物件文字在投影覆盖时会隐藏，但仍占用世界输入；保留首次避让与原版面，无可读空位时使用可见的右侧fallback。极端大物件允许覆盖其部分投影，保证能读和能退出。`[verified: PropNarrationView.UpdatePlacement/ChoosePlacement]`
- 文字开框、换页、暂停恢复、关闭过程和关闭当帧需要分别隔离重复输入；Esc“已消费”不等于发生一次新关闭，避免关闭中又打开暂停菜单。`[verified: PropNarrationView.CanAcceptInput；MiniGameScreen.TryClosePropNarration]`
- 监督员的左键背景推进、原继续按钮与Space/Enter共用帧门控；UI按钮命中不同时走背景点击，不修改终端键鼠路径。`[verified: MiniGameScreen.AdvanceWorldDialogue/TryConsumeDialogueAdvance/PointerIsOverWorldControl]`
- 真实Campaign从Day0开始，不能把窥门无对白误当成Day1晨检；测试通过公开命令完成Day0，再恢复到Day1后验证监督员。`[verified: WorldDialogueInputPlayModeTests.RestoreFirstMorning；smoke.xml]`
- 无焦点batch Editor会把物理鼠标锁定保持None；保留World状态、隐藏光标及所有输入owner释放断言，有焦点时才严格要求OS Locked。`[verified: regression.log 的focused=False；PropNarrationPlayModeTests.OversizedSourceKeepsFallbackReadableAndDismissRestoresWorldInput]`
- 并行场景修复更新了持久壁纸Mesh名称；原生旁路验证兼容新“interior wallpaper”名和旧源ID名，仍要求12片正确阴影状态。正常游戏不启用该CLI探针。`[verified: WallLightingPlayerProbe.DescribeWallpaper；native-lighting-initial-name-mismatch.json；native-lighting-final.json]`

# Questions for review

- 参考仅提取语义，不复制原作代码或美术。原作NPC明确锁移动／视角并切UI输入；字幕采用单独通路。首次点击补全文字属于本项目已有播放器行为，不宣称已核实原作未导出的LineView内部实现。`[verified: 原作DialogInteractable.Interact/OnDialogEnded、PlayerService.MakeImmovable、FakeDialogRunner.ShowSubtitle；DialogueTextPlayback.Advance]`

偏离：2项，物件专注阅读的3D适配与无空位时可见fallback。`[verified: Deviations；Discovered edge cases]`
最可能重审：极端大物件与文字重叠时的阅读位置。`[opinion]`
已处理边界：输入去重、Esc消费、暂停恢复、目标失效、测试起始日与无焦点鼠标锁定。`[verified: 本文与测试报告]`
验证：18项PlayMode、非开发Mono构建与原生运行检查，详见verification.json。`[verified: verification.json]`
后续先读：guide.md、verification.json，再看三个世界交互脚本。`[opinion]`

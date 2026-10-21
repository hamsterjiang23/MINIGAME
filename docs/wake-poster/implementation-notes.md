# Deviations

- 保留起床时恢复安全位置的既有流程，仅移除该流程对朝向和俯仰角的覆盖；防止家具碰撞恢复时玩家仍站在家具内部。`[verified: UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs PlaceAtWakePose]`

# Discovered edge cases

- Day0 起床使用独立碰撞恢复入口，普通夜晚使用梦后行走恢复入口；两个入口共用只修改位置的安全锚点处理。`[verified: UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs RestoreDayZeroWakePose/RestoreDreamTraversal]`
- 海报阅读仍在世界空间显示，边框与正文统一使用白色，一同参与透明合成；底部提示和装饰横线不再创建。分页、暂停与关闭输入沿用原处理。`[verified: UnityProject/Assets/MiniGame/Runtime/PropNarrationView.cs Build/Advance/SetPaused]`

# Questions for review

- 无新增剧情或规则决定。`[opinion]`

偏差数：0，按用户要求调整起床视角和海报文本边框。`[opinion]`
复核重点：安全锚点只用于位置，不用于每日强制朝向。`[verified: UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs PlaceAtWakePose]`
边界记录：控制器原启用状态、Day0／普通夜晚入口、海报分页与暂停。`[verified: UnityProject/Assets/MiniGame/Tests/PlayMode/DreamTraversalPlayModeTests.cs DayZeroWakeRepositionsSafelyWithoutChangingLookOrControllerState；PropNarrationPlayModeTests.cs ReadingOwnsInputPagesAndPauseWithoutChangingGameState]`
验证：本轮起床、碰撞与海报PlayMode批次12项通过，实际场景白框截图已检查；结果与证据见[统一验证](../crt-screen-fit/verification.json)。`[verified: Artifacts/CrtScreenFit/wake-poster.xml passed=12/failed=0；Artifacts/CrtScreenFit/gameplay/poster-white-frame.png]`
后续入口：上述两份 PlayMode 测试。`[opinion]`

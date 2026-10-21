# Deviations

- 用户要求策划灯光工具在Scene与Game视图都可直接编辑；统一使用持久RoomLightingProfile保存补光设置，Scene拖拽与数值窗口走同一数据入口，播放中保存后可保留。`[verified: 本轮用户要求；RoomLightingAuthoring]`
- 现有吊灯继续使用原配置，补光列表默认空，不自动捕获或改写已调好的灯光和调色。`[verified: RoomLightingProfile.SceneLights；RoomLightingController.ApplySceneLights]`

# Discovered edge cases

- 之前面板只有旧吊灯／环境／地板字段，新5盏补光不在可编辑项中；Scene相机也没有启用相同图像效果支持。`[verified: 变更前RoomLightingWindow.DrawSettings；RetroScreenEffect类声明]`
- 夜间灯光当前强度已被归零，编辑器读取白天基准，写回仍由BedtimeGuidance保持熄灯，避免误存0或临时点亮。`[verified: BedtimeGuidance.TryGetDaylightLight；RoomLightingController.CaptureSceneLight/SetDaylight]`
- 光源按转义层级路径与同名序号绑定，只解析当前场景；失效路径跳过，不创建其他光源。`[verified: RoomLightingController.GetHierarchyPath/ResolveSceneLight]`
- Unity退出播放会还原场景灯属性，delayCall可能晚于测试的后续编辑器帧；模式进入时立即应用，再保留一次延迟重刷，无需打开工具，不写资产。`[verified: RoomLightingAuthoring.OnPlayModeChanged/ReapplyLoadedRoomsAfterModeChange；reports/editmode.xml；Artifacts/LightingTools/pause-trace.log]`
- 跨EnterPlayMode的测试迭代器捕获局部变量会出现恢复后空引用；将编辑和断言移到同步静态方法，保留SessionState及真实自动保存检查。`[verified: LightingDesignerToolsTests.PlayModeWritesAndProfileInspectorEditsSurviveReturningToEditMode/EditFixtureDuringPlayMode/AssertSavedFixtureAfterPlayMode]`
- 场景测试旧断言固定要求关闭量化，与用户参考附件冲突；保留配置绑定及生命周期断言，Shader差异使用临时clone，不锁策划艺术数值。`[verified: RetroScreenPlayModeTests.MainRoomUsesProfileAndKeepsOverlayUiOutsideTheEffect/LiveProfileChangesReachTheCameraAndNeutralSettingsMatchOriginal]`

# Questions for review

- 本轮入口为Unity Editor策划工具，不增加运行包内的玩家设置功能。`[verified: RoomLightingWindow编译位置Editor]`

偏离记录：2项，均为已有界面范围内的可撤销实现选择。`[verified: 本文Deviations]`
最可能重审：灯光大规模改名或重组层级后，是否改用独立稳定ID。`[opinion]`
发现边界：6项，包含夜间基准、层级身份、模式还原、测试跨域与旧艺术参数断言。`[verified: 本文Discovered edge cases]`
继续实现先读：RoomLightingAuthoring与RoomLightingController。`[opinion]`
继续验证先读：verification.json及其中的EditMode／PlayMode报告。`[opinion]`

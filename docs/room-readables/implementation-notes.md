# Deviations

- 使用用户最终指定的三张完整物件图，信封、报架与相框随原图一起呈现；薄网格与独立透明裁切材质负责接入场景，保留原始 PNG 像素。
- 未配置的报纸日期不显示新闻；Day0不提前使用Day1内容，正式日序仍由游戏会话提供。
- 原阅读面板逐帧避让会改变尺寸并切换上下位置；改为每页首次显示时选定位置，视角转动时固定视口范围，物件重叠时原位暂隐。仅翻页、宽高比变化或初始无可用空间时重新选位。`[verified: PropNarrationView.cs L130-L188、L239-L240]`
- 海滩海报采用最新截图文案“海报里的小镇是我的故乡...但我从没去过海边”，覆盖此前海报用语；三份新增阅读物件仍使用 Word 文案。`[verified: ReferenceRoomGameplayBuilder.cs L90；本轮用户截图红框；guide.md 首段]`

# Discovered edge cases

- 两张图片的最大alpha为254，裁切阈值采用0.5；透明通道保留，外围光晕不作为实体背景。
- 跨日测试遵循真实命令链：梦境完成后进入AfterDream，再通过床交互推进日历。
- 新增三份阅读物件文案保持 Word 原文；读取、翻页和关闭不修改游戏存档，恢复后重新按当前日选择新闻。
- 暂隐期间仍更新面板的相机相对姿态，避免恢复显示时使用旧世界坐标；恢复后保留当前页。非末页显示“点击继续阅读”，末页隐藏。`[verified: PropNarrationView.cs L148-L154、L214-L218、L272]`

# Questions for review

- 物件贴图的透明裁切与阅读面板的黑底分开控制；交互物阅读面板保持不透明黑底、无框、白字。

偏差数：3，包含完整物件薄网格、逐页固定阅读位置及最新海滩文案覆盖。`[verified: 本文 L3、L5-L6]`
复核重点：连续转动时视口范围保持稳定，物件遮挡时原位暂隐并恢复同页。`[verified: PropNarrationView.cs L130-L154]`
边界记录：保留此前日期与透明裁切边界，新增初始空间不足、宽高比变化和翻页提示边界。`[verified: 本文 L10-L13；PropNarrationView.cs L134-L135、L183、L214-L218]`
验证：本轮11项PlayMode通过，97帧受控转向包含43帧遮挡暂隐，矩形位置和尺寸保持稳定；Windows构建0错误、1条Pipeline调试组件缺失警告，独立Player到达ready标记。此前外观接入基线的15项EditMode、13项PlayMode结果保留在[旧验证记录](verification.json)。`[verified: reading-stability-verification.json tests、cameraSweep、build、buildWarning、playerStartup；verification.json tests]`
后续入口：[阅读物件指南](guide.md)、[来源清单](source-manifest.json)及[阅读稳定性验证记录](reading-stability-verification.json)；修正版本为[MiniGame.exe](../../Builds/ReadingStable/MiniGame.exe)。`[verified: reading-stability-verification.json build.output]`

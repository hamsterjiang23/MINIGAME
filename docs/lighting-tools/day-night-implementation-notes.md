# Deviations

- 扩展既有RoomLightingProfile，保留旧白天字段和无时段API；夜晚单独存入night，避免现有场景资产迁移。`[verified: RoomLightingProfile.cs RoomNightLightingSettings；RoomLightingController.cs period重载]`
- Game增加F7调参面板，Scene沿用房间灯光窗口和原生手柄；两边读取同一配置。编辑时段与预览时段分开，预览不推进游戏日期。`[verified: RoomLightingTuningPanel.cs；RoomLightingWindow.cs DrawTargetAndPeriod]`
- 用户补充配置分享：增加带版本的JSON导出／导入；仅灯光字段，经临时配置验证后提交，分享不依赖资产名或本机绝对路径。两边均有文件入口，导入记为一次撤销。`[verified: RoomLightingPresetIO.cs；RoomLightingTuningPanel.cs ExportPreset/ImportPreset；RoomLightingWindow.cs ExportPreset/ImportPreset]`

# Discovered edge cases

- 原工具仅从ActiveScene找房间，非激活的已加载房间灯无法绑定；现在按所选物件场景定位，且可显式指定目标房间。`[verified: RoomLightingAuthoring.cs FindController/FindControllerForObject；RoomLightingWindow.cs Resolve]`
- 原生Inspector同步依赖窗口和当前Selection；现在全局根据实际修改的Light/Transform定位，支持锁定Inspector或关闭工具后继续编辑。`[verified: RoomLightingAuthoring.cs OnNativeModifications]`
- 夜间LateUpdate不能把编辑值归零，也不能把衰减值写成白天设置；由BedtimeGuidance统一混合独立日夜目标。`[verified: BedtimeGuidance.cs ApplyRoom；RoomLightingController.cs GetDaySettings]`
- 多房间预览只改所属光源；全局RenderSettings仅由当前ActiveScene写入，切换ActiveScene重新应用。`[verified: BedtimeGuidance.cs ApplyRoom；RoomLightingController.cs activeSceneChanged]`
- Game调参暂停游戏，记住原暂停状态；关闭帧仍阻止底层按键，F6/F7互斥，关闭后恢复跟随流程。`[verified: RoomLightingTuningPanel.cs SetPanelOpen；MiniGameScreen.cs PresentationToolsBlockInput]`
- 复查发现普通暂停截图会挡住实时Game调光；现保持逻辑暂停，但调光面板打开期间隐藏普通暂停菜单，并在关闭后恢复原状态。`[verified: MiniGameScreen.cs RefreshPresentationToolVisibility；RoomLightingTuningPanel.cs VisibilityChanged]`
- 编辑模式保存Night预览不能把实际夜间值写进Day场景；保存前临时使用Day，保存后还原预览。非Active房间保存的雾环境污染已由临时场景原生测试重现，保存时临时激活该房间后恢复原Active场景。`[verified: Artifacts/DayNightLighting20261021/editor-new-before-inactive-save-fix.json SavingAnInactiveNightRoomPreservesItsDayFogAfterReload；RoomLightingAuthoring.cs OnSceneSaving/RestoreSavingPreview]`

# Questions for review

- 无需新增玩法规则；当前工作不打包。`[verified: 用户要求“暂时不用打包”；本轮工具实现范围]`

偏差：保留旧Day API、Game新增独立输入面板、追加JSON分享。`[verified: 上方Deviations]`
最可能调整：更多快捷预设或调色控件。`[opinion]`
边界：多Scene、锁定Inspector、Undo、夜间保存、冻结截图、旧配置兼容与失败导入。`[verified: day-night-verification.json checks]`
后续先读：guide.md、day-night-verification.json。`[opinion]`
完成检查：Editor 16项、PlayMode 8项通过；未生成新安装包。`[verified: day-night-verification.json editorPassed/playModePassed/packaged]`

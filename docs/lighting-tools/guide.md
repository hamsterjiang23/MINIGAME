# 白天／夜晚灯光工具

Game 播放时按 **F7** 打开灯光面板，在白天／夜晚之间选择编辑对象；参数实时显示在画面中。关闭用 F7 或 Esc，恢复原有暂停状态及流程光照。Scene 中使用 **MiniGame → 画面 → 房间灯光** 窗口与手柄；两边共用配置资产。`[verified: RoomLightingTuningPanel.cs SetPanelOpen/SelectPeriod；RoomLightingWindow.cs DrawTargetAndPeriod]`

「编辑白天／编辑夜晚」决定修改哪组参数，「预览随流程／白天／夜晚」决定当前显示哪种效果。预览只控制光照，不修改日期、剧情和配额；Game 调光期间保持游戏逻辑暂停，并隐藏普通暂停截图，保留实时场景。`[verified: RoomLightingTuningPanel.cs SetPreview/SetPanelOpen；MiniGameScreen.cs RefreshPresentationToolVisibility]`

打开 Unity 2022 的 [MainRoomCombined](../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity)，选择 **MiniGame → 画面 → 房间灯光**。窗口可停靠在 Scene、Game 旁或浮动；编辑模式、播放和播放暂停时均使用同一个持久配置。`[verified: RoomLightingWindow.Open/OnGUI；RoomLightingAuthoring.WriteLight/ProfileEdited]`

## 调整补光

1. 在「场景补光灯」下拉框选择光源；当前场景的 `MonitorLight (1)` 至 `MonitorLight (5)` 均可选择。也可先在 Hierarchy 选中灯，窗口会跟随选择。`[verified: RoomLightingWindow.DrawSceneLights/OnSelectionChanged；MainRoomCombined.unity 中 MonitorLight 名称]`
2. 看着 Game 画面直接调整位置、旋转、颜色、白天强度、范围和阴影；聚光灯另有内外圈角度。修改会立即应用，不必重新播放。`[verified: RoomLightingWindow.DrawSceneLights；RoomLightingAuthoring.WriteLight/ApplyProfile]`
3. 在 Scene 中点击「Scene 定位」，使用「移动／旋转／范围」手柄拖拽；一次拖拽作为一次撤销。窗口打开时，选中灯的原生 Inspector／Gizmo 人工修改也同步到配置。`[verified: RoomLightingWindow.DuringSceneGUI/BeginSceneHandleInteraction/CompleteSceneHandleEvent/OnNativeModifications]`

白天吊灯、窗边、环境、电脑微光、地板和地毯在基础参数区域调整。夜晚可选择补光、吊灯、窗光、电脑微光和床引导灯，独立调整启用、位置、旋转、颜色、强度、范围、灯型、聚光内外角及阴影。夜间环境另有环境颜色、反射、未覆盖灯倍率、自发光倍率和关灯渐变时间。`[verified: RoomLightingProfileEditor.DrawSettings；RoomLightingTuningPanel.cs BuildDayRows/BuildNightRows/BuildLightRows；RoomLightingController.cs IsEditableSceneLight]`

## Scene 与 Game 预览

「场景真实预览」打开 Scene 的场景灯光与图像效果。调色入口 **MiniGame → 画面 → 调色与像素风格** 使用当前场景所绑定的配置；Scene 相机也启用相同复古效果，保留原图对比。Scene 的观察角度仍由编辑器控制。`[verified: RoomLightingWindow.EnableScenePreview；RetroScreenWindow.OnGUI；RetroScreenEffect.ImageEffectAllowedInSceneView；RetroScreenPreview.SynchronizeSceneCamera]`

若需要调整当前用户参考调色，主场景引用 [RoomLightingReferenceRetro.asset](../../UnityProject/Assets/MiniGame/Resources/RoomLightingReferenceRetro.asset)；补光及基础光保存在 [RoomLightingDefault.asset](../../UnityProject/Assets/MiniGame/Resources/RoomLightingDefault.asset)。窗口会显示实际配置资产，可点击「定位配置」。`[verified: MainRoomCombined.unity 的 profile 引用；RoomLightingWindow.Resolve/OnGUI；RetroScreenWindow.OnGUI]`

## 保存与流程

- 支持 Undo／Redo；「保存配置」立即落盘。经过本工具或配置 Inspector 修改的资产，在退出播放、进入播放、脚本重载和退出编辑器前自动保存；返回编辑模式后重新应用已保存值，关闭工具窗口也会保存灯光调整。`[verified: RoomLightingAuthoring.OnPlayModeChanged/SavePendingChanges/OnUndoRedo；RoomLightingWindow.OnDisable；RetroScreenPreview.SaveEditedProfiles]`
- 选择「编辑白天」时修改保留的白天基准，选择「编辑夜晚」时写入独立夜间覆盖；两组参数不互相污染。旧配置未设置夜晚覆盖时，继续采用黑房间与床灯的默认表现。`[verified: RoomLightingController.CaptureSceneLight/GetSceneLightSettingsList；RoomNightLightingSettings；BedtimeGuidance.ApplyRoom]`
- Game面板的「保存白天与夜晚配置」在Unity Editor保存当前配置资产；Player保存独立的 `room-lighting-settings.json`，不修改剧情存档。`[verified: RoomLightingTuningPanel.cs SaveSettings/SettingsPath]`
- 分享时点击「导出分享 JSON／导出 JSON」，选择文件发给别人；接收者在同项目场景中点击「导入他人 JSON／导入 JSON」，选择文件后即可预览，再保存为自己的配置。文件包含白天、夜晚和每盏灯的全部设置，导入可撤销，不要求双方配置资产名称相同。`[verified: RoomLightingPresetIO.cs ExportJson/TryImportJson；RoomLightingTuningPanel.cs ExportPreset/ImportPreset；RoomLightingWindow.cs ExportPreset/ImportPreset]`
- 各盏灯通过场景内相对层级路径匹配；分享给同一场景结构的项目即可对应，重命名或移动灯后需要重新绑定。Player没有文件选择器时使用 `room-lighting-preset.json`，面板显示可复制的实际路径。`[verified: RoomLightingController.GetHierarchyPath/ResolveSceneLight；RoomLightingTuningPanel.SharedPresetPath/ChooseExportPreset/ChooseImportPreset]`
- 当前默认配置的可分享示例：[RoomLightingDefault.json](presets/RoomLightingDefault.json)。已用实际配置完成导出、覆盖写入、导入和再次导出的一致性检查。`[verified: reports/day-night/share-roundtrip.json roundTripIdentical/overwriteSucceeded/sourceUnchanged]`
- 夜晚预览中保存场景时，场景文件写入白天基准，保存后继续原预览；夜间参数保存在配置资产中。锁定原生Inspector后切换选择，或关闭窗口继续改已绑定灯光，也能同步参数。`[verified: RoomLightingAuthoring.OnSceneSaving/OnSceneSaved/OnNativeModifications]`
- 未编辑的补光保持原场景数值；打开窗口不会自动生成全部覆盖项。已编辑项按当前场景的转义层级路径与同名序号绑定，重命名或改变层级后需重新选择并编辑；失效路径会跳过。`[verified: RoomLightingProfile.SceneLights；RoomLightingController.GetHierarchyPath/ResolveSceneLight/ApplySceneLights]`

日夜工具验证见 [day-night-verification.json](day-night-verification.json)，取舍见 [day-night-implementation-notes](day-night-implementation-notes.md)。此前白天工具验证保留在 [verification.json](verification.json)。`[verified: 本目录文件]`

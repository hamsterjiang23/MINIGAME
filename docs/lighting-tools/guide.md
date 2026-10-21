# 白天／夜晚独立灯光配置

主场景现在使用两份相同类型的完整配置：

- [RoomLightingDay.asset](../../UnityProject/Assets/MiniGame/Resources/RoomLightingDay.asset)：独立白天配置。
- [RoomLightingNight.asset](../../UnityProject/Assets/MiniGame/Resources/RoomLightingNight.asset)：独立夜晚配置，从用户认可的原Day外观开始。

两份初始值相同，之后分别调整；日夜不是同一资产里的倍率关系。原RoomLightingDefault及其用户调整保留，主场景不再绑定旧资产。`[verified: independent-verification.json migration/baseline；MainRoomCombined.unity profile/nightProfile]`

## Game 使用

打开MainRoomCombined场景，Play后点击Game窗口，按 **F7**。选择“编辑白天／编辑夜晚”，面板显示对应真实资产；两边提供相同的全部参数和光源列表。`[verified: RoomLightingTuningPanel.Update/Profile/BuildDayRows/BuildLightRows]`

“编辑哪份”决定修改目标，“预览白天／预览夜晚／随流程”决定画面效果。调好点击“保存白天配置／保存夜晚配置”，仅保存当前资产。F7或Esc关闭并恢复原暂停状态。调光期间游戏逻辑暂停，普通暂停截图隐藏，画面持续更新。`[verified: RoomLightingTuningPanel.SelectPeriod/SetPreview/SaveSettings/SetPanelOpen；MiniGameScreen.RefreshPresentationToolVisibility]`

## Scene 使用

打开 **MiniGame → 画面 → 房间灯光**。可停靠在Scene或Game旁，编辑模式、播放中和暂停时均可配置。先确认白天、夜晚两个资产槽不同，再选编辑时段。`[verified: RoomLightingWindow.Open/DrawProfileBindings/DrawTargetAndPeriod]`

两边都可调整环境、反射、整体光强、自发光、渐变、吊灯／窗边、照明分层、地板和地毯；逐灯列表均包括吊灯、窗光、电脑灯、床灯与补光，支持位置、旋转、颜色、亮度、范围、灯型、聚光角及阴影。Scene定位与手柄仍可用，修改支持Undo／Redo。`[verified: RoomLightingProfile；RoomLightingProfileEditor.DrawSettings；RoomLightingWindow.DrawSceneLights/DuringSceneGUI]`

### 环境光与强度

Scene展开“白天／夜晚完整基础光、环境、地板与地毯”，找到“环境与窗边”：环境光颜色决定色调，新增“环境光强度”决定暗部亮度（0关闭，1原始亮度，最高4）。Game F7参数顶部也有同名滑条。“环境反射强度”只影响材质反射，效果取决于反射贴图和材质光滑度。`[verified: RoomLightingProfile.AmbientLightColor；RoomLightingWindow.DrawSettings；RoomLightingTuningPanel.BuildDayRows]`

主场景已补齐定位点上的WindowLight与MonitorLight；五盏MonitorLight(n)补光保留。若旧场景缺少绑定，Scene工具会警告，并提供“修复窗边灯与电脑灯绑定”；退出播放后操作，再保存场景。`[verified: RoomLightingSetup.RepairMissingBaseLights；RoomLightingWindow.OnGUI]`

逐灯覆盖优先于吊灯／窗光等基础参数；需要恢复基础控制时，清除对应灯的覆盖。“灯光整体倍率”在逐灯覆盖后生效，0关闭全部灯；逐灯面板显示倍率前的原始强度，Unity Light Inspector显示倍率后的实际强度，编辑时自动换算。`[verified: RoomLightingController.BuildPeriodLights/CaptureSceneLight；RoomLightingAuthoring.SyncNativeModifications]`

不适用于当前状态的控件会禁用：平行光无照射范围，点光源／平行光无聚光角，关闭阴影后不调阴影参数。逐灯核对记录见 [strength-verification.json](strength-verification.json)，包括两时段全部10盏灯在Game与Scene中的状态写入、保存分享及渲染对比。`[verified: RoomLightingTuningPanel.BuildLightRows/AddNumber；RoomLightingWindow.DrawSceneLights；strength-verification.json sceneAudit/gameAudit/visualAudit]`

每份配置保留独立完整默认灯快照；用户主动添加的逐灯覆盖优先于基础值，清除该灯覆盖即可回到基础参数。修改Day并保存场景、关闭重开后，Night仍使用自己的默认值。`[verified: RoomLightingController.BuildPeriodLights；IndependentPeriodLightingTests.SavingDaySceneGeometryDoesNotChangeNightAfterReopening]`

## 保存与分享

- 保存只写当前编辑的配置资产。原生Inspector与Gizmo也按当前编辑时段同步；锁定Inspector或非Active房间仍可定位。`[verified: RoomLightingAuthoring.WriteLightCore/OnNativeModifications/SaveProfile]`
- 白天、夜晚分别“导出白天JSON／导出夜晚JSON”。接收者先选目标时段，再导入、预览并保存；只影响选中的那一份。v3文件包含环境光强度、完整参数和默认光源快照，不要求配置资产名称相同。v1/v2仍可导入，缺失环境光强度时默认为1。`[verified: RoomLightingPresetIO.Version/ProfileData/TryImportJson；RoomLightingTuningPanel.ExportPreset/ImportPreset；RoomLightingWindow.ExportPreset/ImportPreset]`
- 单灯按场景相对层级路径匹配，分享给同项目的人需保持场景结构。旧v1文件可导入原有完整Day基准；旧内嵌夜间倍率不再作为新完整配置使用。错误文件和未知版本不会覆盖当前值。`[verified: RoomLightingController.GetHierarchyPath；RoomLightingPresetIO.TryImportJson]`
- Player本地配置分别保存到room-lighting-settings.json与room-lighting-night-settings.json，分享文件也分开；Editor可选择文件路径，Player显示实际路径。`[verified: RoomLightingTuningPanel.CurrentSettingsPath/CurrentSharedPresetPath]`

示例：[白天JSON](presets/RoomLightingDay.json)、[夜晚JSON](presets/RoomLightingNight.json)。`[verified: 上述preset文件]`

普通Night使用完整夜晚资产；入睡、梦境和自我审查的特殊黑场仍由演出控制。床灯若明确关闭，光束与粒子也关闭。`[verified: BedtimeGuidance.ConfigureFullLighting/ApplyFullRoom/VisibleBeamStrength]`

本轮检查见 [independent-verification.json](independent-verification.json)，取舍见 [independent-implementation-notes.md](independent-implementation-notes.md)。上一版验证保留于 [day-night-verification.json](day-night-verification.json)。`[verified: 本目录验证文件]`

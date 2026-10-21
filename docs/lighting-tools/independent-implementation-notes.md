# Deviations

- 原Day外观成为Night基准；新建两个同类型完整资产，初始都复制原Day，之后独立调整。`[verified: 本轮用户要求；migration.json]`
- 增加隐藏SceneLightDefaults快照，基础值仍可调，用户SceneLights覆盖才优先；避免初始全灯覆盖使基础滑条失效，以及场景保存造成Night默认值漂移。`[verified: RoomLightingController.BuildPeriodLights]`

# Discovered edge cases

- 原Default有用户未提交的Night调整；迁移前保存原件与Day八灯快照，保留旧资产，主场景绑定新文件。`[verified: Artifacts/IndependentLighting20261021/RoomLightingDefault.user-current.asset/current-day-baseline.json；MainRoomCombined.unity]`
- 拒绝同资产绑定到两个时段；原生Inspector形成别名时Night查询为空，Game工具阻止编辑，防止写回Day。`[verified: RoomLightingController.ConfigureProfiles/GetProfile；RoomLightingTuningPanel.SelectPeriod]`
- 原Game测试仅克隆Day，现同时克隆Night，避免污染正式夜晚文件。`[verified: DayNightLightingPlayModeTests.OpenRoom/Cleanup]`
- 旧单资产保持兼容；旧v1JSON导入其完整Day基准，新v2每个文件只包含一个完整时段。`[verified: RoomLightingPresetIO.ProfileData/TryImportJson]`

# Questions for review

- 新Day具体艺术效果留给用户在独立副本继续调整；未擅自设置更亮的日光数值。`[opinion]`
- 本轮不打包、不自动推送。`[verified: 任务范围及此前用户“暂时不用打包”]`

# Completion

- Native EditMode/PlayMode 共19项通过；八灯基准逐项保留；两份v2JSON经原生导入往返一致；旧Default的SHA256与迁移前备份一致；复核CLEAN。`[verified: independent-verification.json tests/baseline/presets/migration/review]`

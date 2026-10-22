# Deviations

- 昼夜调色引用存放在现有两份 RoomLightingProfile 中，复用灯光的时段、预览和过渡时间，不新增第二套昼夜时钟。[opinion]

# Discovered edge cases

- 旧相机只有一份 RetroScreenProfile；未绑定时段配置时继续使用该资产，已有自定义参数需要原样保留。[verified: RetroScreenEffect.Profile；RetroScreenSetup.FindSavedProfile]
- Scene View 的相机不属于房间场景，需要显式绑定当前目标房间，才能跟随灯光预览。[verified: RetroScreenPreview.SynchronizeSceneCamera]
- 灯光 JSON 的数据 DTO 不包含 Unity 资产引用；导入灯光应保留调色绑定。[verified: RoomLightingPresetIO.ProfileData]
- 交互轮廓宽度需要读取正在渲染的时段／过渡配置，避免夜晚粗像素让轮廓消失；原图对比和演出旁路使用原始宽度。[verified: InteractionFocusOutline.ActivePixelSize]
- 禁用的端点不能把残留色阶、颗粒和像素参数带入过渡。此时沿用另一端的风格并仅淡出整体强度。[verified: RetroScreenProfile.BlendFrom]
- 原有 Configure/Profile 显式单配置接口继续覆盖自动选择；安装工具明确启用昼夜跟随。编辑器原图对比回调不能重置游戏侧的独立原图标志。[verified: RetroScreenEffect.Configure/Profile；RetroScreenSetup.Attach；RetroScreenPreview.SynchronizeSceneCamera]

# Questions for review

- 初始白天、夜晚沿用当前调色值，提供独立资产供用户分别调整；本轮不重做用户已经调好的色彩。[opinion]

## 完成记录

- 实现偏差：1 项，复用灯光资产绑定与既有时间轴。[verified: RoomLightingProfile.ScreenStyle；RoomLightingController.NightBlend]
- 可再次调整：夜晚初始调色沿用原样，具体视觉参数留给独立夜晚面板。[opinion]
- 边界检查：旧单配置覆盖、停用端点过渡、Scene View 目标、像素描边及灯光 JSON 绑定。[verified: DayNightRetroToolsTests；playmode-final.xml]
- 验证：28 项 EditMode 与 22 项 PlayMode 全部通过；三张实际相机截图已查看。[verified: retro-day-night-verification.json]
- 后续入口：[使用说明](../retro-screen-guide.md)与[验证记录](retro-day-night-verification.json)。[verified: 对应文件]

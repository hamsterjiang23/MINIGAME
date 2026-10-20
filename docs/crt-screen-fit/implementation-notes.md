# Deviations

- 用户追加的每日电源规则覆盖早先“目录手动关机”方案；中途退出只关闭操作UI。`[verified: 用户“每天一次开机一次关机”]`
- 用户要求检查实际曲率，因此覆盖面为贴合玻璃的曲面网格；保留原屏幕渲染器及交互绑定。`[verified: CrtScreenFitPostprocessor.Apply]`

# Discovered edge cases

- 原四顶点面在真实玻璃前方15–44毫米；平移单一平面无法同时贴合边缘与中心。源玻璃有扫描缺口及局部折面，采用保守圆角边界与沿观察轴0.65毫米偏移，四处缺口局部补采样。`[verified: Art/MainRoom/screen-fit/report.json sourceHelperGltfWorldBounds/surfaceDepthRangeGltfZ/method/scanHoleFallbackLocationsGltfXY]`
- 导入后不可读的FBX网格不能原位设置顶点；创建新网格又会丢失原子资产绑定。最终针对该FBX开启Read/Write，原位修改子网格；其他渲染器和原始FBX二进制不变。`[verified: Artifacts/CrtScreenFit/import.log MINIGAME_CRT_SURFACE_FITTED；CrtScreenFitPostprocessor.OnPreprocessModel/Apply]`
- Unity导入坐标系通过参考标记转换，不能直接套用glTF坐标；包围盒匹配误差约0.00073毫米。拍摄方向使用ReferenceDepth正向，其对应glTF前向负Z。`[verified: Artifacts/CrtScreenFit/import.log monitorBoundsError=7.299311E-07；CrtScreenFitValidation.ValidateAndCapture]`
- 仅关掉发光色不能让绿色底材变暗，因此物理屏幕材质增加默认值1的_CrtPower；普通房间材质保持原值，终端单独控制亮灭。灯光控制器读取原属性块后写入灯光参数，保留电源参数。`[verified: RoomArtStandard.shader LightingRoomStandard/surf；PresentationEffects.SetSurfaceGlow；RoomLightingController.ApplySurface]`
- Day3本人复核引导屏需直接点亮，不重复播放日常开机；旧Off本人复核检查点恢复为Running。`[verified: GameRuntime.Accept/Restore；DreamFlowRuntimeTests.RestoringUnpoweredSelfReviewEnablesItsSafetyScreenWithoutBootOrCoreChanges]`
- 初次混合PlayMode批次返回编辑器后没有生成XML，按未完成处理；后续分批执行并以各批XML为准，不把日志中的单项通过提升为整批通过。`[verified: Artifacts/CrtScreenFit/playmode.log；verification.json unity]`

# Questions for review

- 无新增剧情决定。`[opinion]`

后续检查入口：guide.md与verification.json；几何重新采样入口：Art/MainRoom/screen-fit/fit_screen.py。`[opinion]`

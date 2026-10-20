# 策划调色与像素风格

打开 **MiniGame → 画面 → 调色与像素风格**；原入口“复古效果”继续可用。房间灯光面板顶部也可跳转。当前场景使用 `RoomLightingReferenceRetro` 配置资产，支持 Inspector 编辑、Undo/Redo、保存与播放中预览。`[verified: RetroScreenEditor.cs RetroScreenWindow.Open/OnGUI；RoomLightingWindow.OnGUI]`

## 先选择起点

- **自然画面预设**：关闭调色、色阶与额外像素化，不改真实聚光灯和家具阴影参数。`[verified: RetroScreenProfile.ResetToNaturalDefaults]`
- **参考调色起点**：曝光 +0.5EV、对比度 -5、饱和度 -5，暗部轻微灰绿，中高光轻微暖黄；只启用调色，色阶、虚拟像素和高光压缩需另外开启。`[verified: RetroScreenProfile.ResetToReferenceStyle]`
- **临时查看原图**：只旁路相机后处理，不改已调参数。保存前可随时比较；它不会关闭真实场景灯光。`[verified: RetroScreenPreview.SetOriginal；RetroScreenEffect.Render]`

自然聚光灯是默认画面。先把照明关系调好，再考虑后处理；后处理不能补回没有受光信息的墙面。`[verified: RoomLightingDefault.asset lightingBandsEnabled=0；RoomLightingReferenceRetro.asset fullscreenQuantization=0]` `[opinion]`

## 可以配置什么

- **曝光、对比度、饱和度**：调整整体光色；曝光以 EV 计，+1 代表两倍。`[verified: RetroScreenProfile PostExposure/Contrast/Saturation；RetroScreen.shader Grade]`
- **暗部／中间调／高光**：每个区域有独立偏色与亮度，白色偏色值代表不偏色；区域之间平滑过渡，过渡位置可调。`[verified: RetroScreenProfile 三组 Tint/Exposure、ShadowEnd/HighlightStart；RetroScreen.shader Grade]`
- **Neutral 高光压缩**：可选地处理过亮的 HDR 高光，默认关闭。只在分区调色启用时应用，内部先转线性空间，再回到显示颜色空间。`[verified: RetroScreenEffect.Render；RetroScreen.shader NeutralTonemap/frag]`
- **色阶压缩**：默认关闭；可切换“感知亮度分档”与兼容旧配置的“最大通道分档”，另有档数、色阶强度和前后 Gamma。感知模式建议先试 10 档、较低强度。`[verified: RetroScreenProfile FullscreenQuantization/PerceptualQuantization；RetroScreen.shader quantize 分支]` `[opinion]`
- **量化前抖动**：只在色阶压缩开启时生效，使用固定 Bayer 网格；可从 0.1–0.25 试起，不会随时间播放噪声。`[verified: RetroScreen.shader Bayer4/quantize，无时间输入]` `[opinion]`
- **像素化**：可按输出像素尺寸，或按虚拟画面高度控制。16:9 下高度 540 对应 960×540、高度 360 对应 640×360；其他比例自动计算宽度，小窗口不被强制放大。`[verified: RetroScreen.shader blockSize/virtual grid；RetroScreenProfile.VirtualHeight]`

曝光或色阶过强仍可能压坏暗部、海报和地毯。建议按“只调色 → 加少量色阶和抖动 → 最后像素化”的顺序比较，看到刻意光环时先关闭色阶压缩。`[opinion]`

## 保存与演出

面板修改的是配置资产，Play Mode 中保存后退出仍保留。Overlay UI 在相机风格化之后绘制；现有梦境演出继续通过 `PresentationBypass` 旁路。原复古配置与备份保留，旧资产缺少新增字段时仍使用原量化算法。`[verified: RetroScreenProfile 字段默认值；RetroScreenEffect.Render；RetroScreenProfileEditor 保存逻辑；RetroScreenPlayModeTests]`

本工程是 Unity 2022 Built-in，未引入 URP Volume 或 Renderer Feature。这里把参考中的能力合并到既有 `OnRenderImage` Pass，配置入口只有同一个 `RetroScreenProfile`。Neutral 曲线参考固定版本的 [Unity Graphics Color.hlsl](https://github.com/Unity-Technologies/Graphics/blob/a7e4c051d256a781ab362c64316b125a1e104694/Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl#L489-L525)，按 Unity Companion License 使用；上游许可证声明位于仓库 `UnityProject/Assets/MiniGame/Art/MainRoom/RoomLighting/UnityGraphics-LICENSE.md`。`[verified: RetroScreen.shader 来源注释；UnityGraphics-LICENSE.md]`

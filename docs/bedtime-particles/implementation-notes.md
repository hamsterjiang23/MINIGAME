# Deviations

- 仅替换床聚光灯中的漂浮层；沿用原薄雾、聚光灯、BeamStrength与暂停时钟。发光微粒采用既有网格粒子载体，每颗只保留一个圆形billboard；不新增独立灯光或修改睡眠规则。`[verified: 本轮用户要求；BedtimeAtmosphere／BedtimeGuidance既有控制入口]`

# Discovered edge cases

- 原效果每只虫由9片quad组成，有虫身、翅膀与腿，使用普通透明混合和高速摆动。替换需要同时处理轮廓、材质与运动，不能仅调亮旧虫身。`[verified: 变更前BedtimeAtmosphere.BuildInsects/UpdateInsects；BedtimeInsect.shader]`
- 动画时钟必须由SetVisualState显式传入；Shader不读取Unity时间，暂停和存档恢复沿用既有演出控制。`[verified: BedtimeAtmosphere.SetVisualState；BedtimeGuidance.Bind]`
- 隔离工程的Presentation副本落后于当前门锁接口，首轮编译出现CS1061；仅同步当前GameRuntime依赖后，粒子和床前回归共9项通过。`[verified: Artifacts/BedtimeParticles/bedtime-guidance.log；staged-packages.json；atmosphere.xml；bedtime-regression.xml]`
- 单独禁用Atmosphere组件仍沿用旧的Renderer生命周期；现有BedtimeGuidance.OnDisable显式SetVisualState(0)隐藏。此次保留既有控制路径，不扩展为独立组件开关。`[verified: BedtimeAtmosphere.cs L234–248；BedtimeGuidance.cs OnDisable]`

# Questions for review

- 暂按暖白色小光点、缓慢漂浮和柔和光晕制作；颜色与数量未由用户指定，属于可调整的视觉选择。`[opinion]`

## Handoff summary

偏差：1项，沿用既有手动网格粒子载体。`[verified: 本文Deviations]`
最可能调整：微粒数量、亮心大小或亮度，取决于用户视觉反馈。`[opinion]`
边界：暂停时钟、GPU可见性、重绑销毁和隔离依赖已验证。`[verified: atmosphere.xml；staged-packages.json]`
保留：原聚光灯、薄雾形状、睡眠／昼夜流程。`[verified: 生产改动仅BedtimeAtmosphere.cs与BedtimeInsect.shader；bedtime-regression.xml]`
后续入口：先读effect-record.md和verification.json，再看真实床边截图。`[opinion]`

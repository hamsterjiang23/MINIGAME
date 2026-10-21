# Deviations

- 仅替换床聚光灯中的漂浮层；沿用原薄雾、聚光灯、BeamStrength与暂停时钟。发光微粒采用既有网格粒子载体，每颗只保留一个圆形billboard；不新增独立灯光或修改睡眠规则。`[verified: 本轮用户要求；BedtimeAtmosphere／BedtimeGuidance既有控制入口]`

# Discovered edge cases

- 原效果每只虫由9片quad组成，有虫身、翅膀与腿，使用普通透明混合和高速摆动。替换需要同时处理轮廓、材质与运动，不能仅调亮旧虫身。`[verified: 变更前BedtimeAtmosphere.BuildInsects/UpdateInsects；BedtimeInsect.shader]`
- 动画时钟必须由SetVisualState显式传入；Shader不读取Unity时间，暂停和存档恢复沿用既有演出控制。`[verified: BedtimeAtmosphere.SetVisualState；BedtimeGuidance.Bind]`

# Questions for review

- 暂按暖白色小光点、缓慢漂浮和柔和光晕制作；颜色与数量未由用户指定，属于可调整的视觉选择。`[opinion]`

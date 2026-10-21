# 床边聚光灯漂浮光粒

当前状态：制作中；技术未验、视觉待验、用户待验。`[verified: 本轮制作阶段]`

用户要求每晚床边聚光灯里去掉虫形，改为漂浮发光粒子；保留床灯和睡眠引导。默认采用暖白、柔光、小粒、慢漂浮，粒子层从属于聚光灯，不改变整屋色调。`[verified: 本轮用户要求]` `[opinion]`

进入：沿用BeamStrength由0到1的显示过程。持续：微粒在床上方光束内部缓慢漂移，无虫身、翅膀和腿；粒子大小有小幅差异。退出：随床灯淡出后完全隐藏；暂停时保持原位置与亮度，重绑后仍遵循当前床灯状态。`[opinion]`

层次：原聚光灯负责床面照明，原薄雾负责光束体积，新的微粒负责空气中的细小亮点。此次仅改第三层。`[verified: BedtimeGuidance.Spotlight；BedtimeAtmosphere.BeamRenderer及本轮修改范围]`

控制契约：`BedtimeAtmosphere.SetVisualState(strength, deltaSeconds, paused)`，强度0–1、时间单位秒；由BedtimeGuidance驱动。保留现有Shader资产身份，UV(-1..1)控制圆形亮心与柔边，顶点颜色控制暖白亮度／透明度。粒子不创建Collider，不影响游戏会话。`[verified: 既有SetVisualState与CreateRenderer；本轮实施契约]`

验收：实际光束中只看到软发光点；运动缓慢且在光束内；暂停保持；退出隐藏；睡觉与次日恢复无变化。技术与视觉分别记录，未指定或测量性能预算。`[opinion]`

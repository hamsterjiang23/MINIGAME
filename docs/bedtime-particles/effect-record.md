# 床边聚光灯漂浮光粒

当前状态：粒子层与床前流程共9项PlayMode检查通过；已检查真实床边与单粒子截图，用户视觉验收待定。`[verified: Artifacts/BedtimeParticles/atmosphere.xml test-run passed=2；bedtime-regression.xml test-run passed=7]` `[opinion]`

用户要求每晚床边聚光灯里去掉虫形，改为漂浮发光粒子；保留床灯和睡眠引导。默认采用暖白、柔光、小粒、慢漂浮，粒子层从属于聚光灯，不改变整屋色调。`[verified: 本轮用户要求]` `[opinion]`

进入：沿用BeamStrength由0到1的显示过程。持续：微粒在床上方光束内部缓慢漂移，无虫身、翅膀和腿；粒子大小有小幅差异。退出：随床灯淡出后完全隐藏；暂停时保持原位置与亮度，重绑后仍遵循当前床灯状态。`[opinion]`

层次：原聚光灯负责床面照明，原薄雾负责光束体积，新的微粒负责空气中的细小亮点。此次仅改第三层。`[verified: BedtimeGuidance.Spotlight；BedtimeAtmosphere.BeamRenderer及本轮修改范围]`

控制契约：`BedtimeAtmosphere.SetVisualState(strength, deltaSeconds, paused)`，强度0–1、时间单位秒；由BedtimeGuidance驱动。保留现有Shader资产身份，UV(-1..1)控制圆形亮心与柔边，顶点颜色控制暖白亮度／透明度。粒子不创建Collider，不影响游戏会话。`[verified: 既有SetVisualState与CreateRenderer；本轮实施契约]`

验收：实际光束中只看到软发光点；运动缓慢且在光束内；暂停保持；退出隐藏；睡觉与次日恢复无变化。技术与视觉分别记录，未指定或测量性能预算。`[opinion]`

## 实现参数

固定36颗、每颗1个面向当前渲染相机的quad，共144顶点／216索引。半尺寸7.5–15毫米，暖白顶点色，透明度0.25–0.44；慢漂移由既有VisualTime确定，不使用Shader全局时间、振翅或亮度闪烁。`[verified: UnityProject/Assets/MiniGame/Runtime/BedtimeAtmosphere.cs L10、L162–214、L241–246]`

材质为加法RGB，亮心与弱光晕采用径向衰减，保留深度遮挡，关闭深度写入与投影。原BedtimeInsect.shader文件路径和meta保留，Shader名称改为MiniGame/Bedtime/GlowParticles。`[verified: UnityProject/Assets/MiniGame/Resources/BedtimeInsect.shader L1–37；BedtimeAtmosphere.cs L56–64、L125–130]`

强度随床灯控制并淡出；强度0时两层Renderer隐藏并停止视觉时钟。粒子使用当前渲染相机的右／上方向；渲染回调不推进运动时间。重绑会隐藏并销毁旧资源，销毁组件时释放新资源与回调。`[verified: BedtimeAtmosphere.cs L74–100、L234–269；BedtimeAtmospherePlayModeTests.cs GlowCloudDriftsInsideTheConeFreezesAndReleasesReboundResources]`

## 播放证据

- 单层：截图[初始](../../Artifacts/BedtimeParticles/Captures/bedtime-particles-time-0.png)、[0.75秒](../../Artifacts/BedtimeParticles/Captures/bedtime-particles-time-075.png)、[单粒子柔边](../../Artifacts/BedtimeParticles/Captures/bedtime-particle-soft-edge.png)。GPU检查实际RGB可见、位置变化、暂停像素稳定、隐藏与资源释放。`[verified: atmosphere.xml 两项test-case result=Passed]`
- 组合：实际玩家相机、当前RoomLightingReferenceRetro后处理下的[床边](../../Artifacts/BedtimeParticles/Captures/bedtime-spotlight.png)、[后续帧](../../Artifacts/BedtimeParticles/Captures/bedtime-particles-later.png)、[次日](../../Artifacts/BedtimeParticles/Captures/bedtime-restored-morning.png)。圆形光点替代虫形，床灯和原薄雾保留。`[verified: 上述本轮实际截图]` `[opinion]`
- 运动：[16帧预览](../../Artifacts/BedtimeParticles/bedtime-particles.mp4)由实景截图顺序编码，供观察漂浮，不是带时间戳的帧率测量。历史虫形对照保留于Artifacts/Bedtime/Visuals/bedtime-spotlight.png；历史画面的后处理与当前版本不同，仅作轮廓参考。`[verified: Captures/bedtime-motion-00.png 至15.png；video-encode.log；历史截图]`
- 流程：暂停恢复、夜间家具禁用、物理床交互、单次睡觉和次日灯光恢复通过；白天／夜间灯光编辑检查通过。`[verified: bedtime-regression.xml 全部7项result=Passed]`

技术判定：本次检查通过；未测性能预算。视觉自检：满足去虫形、柔光圆点、慢漂浮目标；粒子密度与亮度为本次默认选择，最终以用户观看为准。`[verified: atmosphere.xml；bedtime-regression.xml]` `[opinion]`

## Windows Mono

非开发Windows x64 Mono构建成功，0错误／0警告。独立Player中的终端全流程和昼夜场景观察均通过，已检查[native夜景](../../Artifacts/BedtimeParticles/Player/Lighting/night-desk-bed-rear.png)。测试和构建使用隔离工程，主编辑器继续保留。`[verified: Artifacts/BedtimeParticles/windows-build.json；Player/Terminal/verification.json；Player/Lighting/verification.json；本轮Win32_Process输出]`

包体：[NORM-Windows-x64-Mono-BedtimeParticles-20261021-195001.zip](../../Builds/Deliverables/NORM-Windows-x64-Mono-BedtimeParticles-20261021-195001.zip)。解压后运行MiniGame.exe；逐文件SHA256与目录清单已核对。完整记录见[verification.json](verification.json)。`[verified: Artifacts/BedtimeParticles/package.json validation与zipPath]`

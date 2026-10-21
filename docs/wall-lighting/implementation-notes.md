# Deviations

- 本轮开始处理之前暂缓的灯光：以用户提供的 RoomLightingReferenceRetro.asset 为调色依据，远端场景从 c28a44c0 读取，先处理飞书墙体问题。`[verified: 本轮用户要求；飞书revision280第20条]`
- 远端场景不止灯光改动，还删除睡床聚光灯并增添墙片；合并时保留当天睡床引导和既有门口／CRT衔接，先区分所需灯光与问题几何。`[verified: Artifacts/WallLighting/lighting-merge-report.json]`

# Discovered edge cases

- 源壁纸是 MainRoom_30_B 而非旧 MainRoom_00_Wallpaper；部分源网格跨过实体墙面，最近贴面约0.533毫米。CPU分析只作为GPU对照的候选，不直接判定黑块根因。`[verified: Artifacts/WallLighting/material-audit.md；wall-source-geometry.json]`
- 对照分别保留正常渲染、禁用阴影、Unlit、隐藏墙壳与仅纸片不投影；诊断临时相机重置原相机的自定义投影，最后还原所有临时设置，不保存源场景。`[verified: WallSurfaceDiagnostics.Capture]`
- 双面深度ID图确认实体墙穿插到纸片前；12片壁纸投影到对应内墙面4毫米，保留UV与切向撕裂轮廓，独立保存生成网格，原FBX不改。正面视角62,872个原墙体像素在修复后显示为壁纸。`[verified: geometry-preview/repair-preview.json；surface-id-comparison.json positive-z]`
- 远端5盏新增灯为Baked且场景未绑定烘焙数据，改Realtime以保证Player实时照明与睡前动态熄灭；保留原睡床聚光灯与全部游戏组件。`[verified: applied-lighting.json realtimeOverride/protectedBlocksUnchanged]`
- 两个旧测试夹具需适配当前行为：未命名初始场景不能再加未保存场景；睡床交互需等待现有Sleep演出再断言Day1。生产流程未为测试改动。`[verified: editmode-initial.xml；playmode.xml EditingTheProfileAtBedtimeKeepsTheRoomDarkAndUpdatesTheNextMorning]`

# Questions for review

- 新梦境素材仍未纳入本轮；只处理墙体、用户灯光场景和附带调色配置。`[verified: 本轮用户要求]`

变更偏差：3项（独立生成贴墙网格；保留远端删除的床引导；五盏Baked补光转Realtime）。`[verified: reports/wall-surface-repair.json；reports/applied-lighting.json]`
最可能回看：实际显示器上的调色和颗粒观感，源附件已保留。`[opinion]`
发现边界：穿墙顶点、近贴面、双面诊断、资产归属保护、未保存场景保护、实时与烘焙灯光、两个旧测试夹具。`[verified: 本文与修复报告]`
最终验证：7项EditMode、18项相关PlayMode、0错误0警告的Windows Mono构建及原生昼夜/终端回归通过；完整包逐文件SHA-256验证。`[verified: verification.json unity/nativePlayer/terminalRegression/package]`
后续先读：guide.md、source-manifest.json、verification.json，再看WallSurfaceRepairSetup和RoomLightingController。`[opinion]`

2026-10-21重传附件复核：远端仍为c28a44c0，附件34项字段及5盏场景补光已一致；保留配置未声明的颗粒0.05与Realtime模式，未重复改写正式场景／配置。当前灯光与相机6项PlayMode通过，新结果独立记录以保留先前Windows构建证据。`[verified: reports/current-source-check.json]`

墙体图片复查：重发原图与飞书问题截图哈希一致；新GPU对照未复现电脑上方旧大块遮挡，床头斜边是撕裂纸片外露墙区。阴影关闭、材质金属贴图关闭、墙面重算法线均未消除其外形；保留该原模型轮廓。运行相机截图测试1项通过，临时诊断脚本已恢复／移除；正式场景未另改，GitHub尚无本地修复网格。`[verified: reports/wall-render-recheck.json]`

用户否定剩余黑楔观感后补查照明分配：5灯Auto且当前逐像素预算4，底墙内侧仅有边界顶点；GPU仅将5灯设ForcePixel，或临时把预算提至16，均消除异常断层。先前“底墙外露”的判断只说明表面身份，不足以解释受光错误。正式修复仅改5个Light.m_RenderMode，不改全局预算、几何或艺术参数。`[verified: reports/lighting-continuity-change.json；lighting-continuity-verification.json controls]`

本次新增偏离：逐像素照明优先级，累计4项修复选择。`[verified: reports/lighting-continuity-change.json；此前变更偏差]`
最可能重审：新增大量补光时的照明成本；本次未做性能收益结论。`[opinion]`
新发现边界：Auto灯的选择与粗细不同的网格可能形成照明断层，需同机位逐像素对照确认。`[verified: lighting-continuity-verification.json controls]`
当前验证：墙纸EditMode 3/3、灯光及真实相机PlayMode 7/7通过。`[verified: lighting-continuity-verification.json unity]`
后续先读：lighting-continuity-verification.json，再看5盏MonitorLight的RenderMode与当前截图。`[opinion]`

Mono交付更新：构建前发现隔离工程的ApartmentPlayer、PropNarrationView及其测试、PortraitCatalog落后正式工程，已同步并移除隔离旧TerminalComposite副本。新非开发Mono构建0错误0警告，门口／默认开场／终端／昼夜4份原生报告通过，ZIP159文件回读SHA通过。初次仅跑门口驱动会在夜间观察前退出，改用完整终端驱动后昼夜观察通过。`[verified: latest-mono.json；Artifacts/LightingContinuity/native-early-exit-observation.json；reports/latest-mono-package.json]`

用户授权旧包清理：13个旧时间戳压缩包及校验文件已删除，释放约1.41GiB；两次递归目录清理均被自动审批以blocked by policy拒绝，20个旧解压目录保留。最新包与可运行目录已校验仍在。`[verified: reports/latest-mono-cleanup.json]`

转向亮缝追加修复：12片墙纸之间存在124对同深度相交三角形，无网格内部自重叠。仅临时错开一片1毫米即可消除截图所指亮缝；生成器按相邻关系固定分配4/5毫米深度，重复生成不累积偏移，保留原UV与切向轮廓。写回已渲染过的Mesh时显式刷新顶点缓冲，避免编辑器继续显示旧GPU数据。`[verified: reports/camera-seams/baseline-overlap-analysis.json；reports/camera-seams/fixed-overlap-analysis.json；WallSurfaceRepairSetup.SeparateOverlappingPapers/Persist]`

验证：4项EditMode、7项PlayMode通过。真实ViewCamera在21个yaw角度下比较同深度对照与修复网格，亮度尖峰从0.419降到0.042；测量临时绕过调色和颗粒以隔离接缝，随后另测原后处理配置。场景文件与FBX未改，最新构建和启动结果见[接缝验证](camera-seam-verification.json)。`[verified: reports/camera-seams/camera-sweep.json；camera-seam-verification.json tests、preservation、build、playerStartup]`

按用户要求重新交付非开发版Mono：使用BuildOptions.None和现有Mono2x后端构建独立新目录，构建0错误、1条Pipeline组件缺失提示；构建期间运行脚本、场景和墙纸网格哈希保持不变。ZIP内160个文件解压后逐项SHA-256一致，从解压目录启动到达ready标记。`[verified: camera-seam-mono-package.json build、package、nativeStartup；reports/camera-seams/mono-release-build.json messages、sourceSnapshot]`

# 墙体与灯光修复

本轮先处理飞书调整建议 revision280 第20条墙体渲染问题，再合并 GitHub `c28a44c0` 的场景灯光，应用用户提供的 `RoomLightingReferenceRetro.asset`。原始截图、附件哈希与远端版本见 [来源记录](source-manifest.json)。`[verified: source-manifest.json]`

此前完整Windows x64 Mono交付包及历史验证见[交付记录](latest-mono.json)。`[verified: latest-mono.json]`

## 墙顶与窗旁漏光

2026-10-22：两侧墙顶亮线及窗旁断续竖线均在WindowLight照射下出现；阴影Bias/Normal Bias归零、双面投影没有解除墙顶亮线。最终采用完整封闭几何：天花板上方新增20厘米厚、四周延伸20厘米的实体屋顶，距原内饰平面2毫米以避免重合；窗墙四片替换为共用室内平面的实体Cube，向室外增加20厘米厚度，原窗洞Y/Z边界和原材质保留。五个实体均正常渲染、投射并接收阴影，没有使用ShadowsOnly。`[verified: solid-shell-verification.json controls、geometry]`

原FBX、墙纸撕裂轮廓、门窗位置、灯光和碰撞体不改。持久化墙体修复会重复更新实体，避免重建时丢失；已有复古配置按用户要求一起提交：pixelSize=2、useVirtualResolution=true、virtualHeight=540。昼夜原始相机预览、4项EditMode及保留项验证见[本轮记录](solid-shell-verification.json)，[修复前](previews/solid-shell-before.png)、[白天修复后](previews/solid-shell-day.png)、[夜间修复后](previews/solid-shell-night.png)。这些是隔离Unity 2022的EditMode原始相机渲染，本轮未重跑PlayMode或构建Player。`[verified: solid-shell-verification.json geometry、retroProfile、validation、scope]`

## 转动视角时的竖向亮缝

相邻墙纸存在同深度重叠，电脑右侧接缝重叠约1.85厘米，导致转动时出现亮缝。相同灯光下将一片墙纸临时错开1毫米，亮缝消失；正式修复为相邻重叠片分配固定深度层，当前距底墙4或5毫米。原UV、撕裂轮廓、材质、FBX和场景布局保留。`[verified: camera-seam-verification.json controls、repair、preservation]`

新版非开发Mono交付：[完整ZIP](../../Builds/Deliverables/NORM-Windows-x64-Mono-20261021-144903.zip)、[SHA-256](../../Builds/Deliverables/NORM-Windows-x64-Mono-20261021-144903.sha256)及[可运行目录](../../Builds/NORM-Windows-x64-Mono-20261021-144903/MiniGame.exe)。160个文件解压回读校验及解压目录启动通过，详见[交付记录](camera-seam-mono-package.json)。游戏相机21角度对照、网格与灯光检查见[接缝验证](camera-seam-verification.json)，[修复前](previews/seam-before.png)与[修复后](previews/seam-fixed-editmode.png)保留同机位对照。`[verified: camera-seam-mono-package.json build、package、nativeStartup；camera-seam-verification.json cameraSweep、tests]`

## 床头黑色斜块的后续修正

用户再次指出床头异常后，GPU对照确认：五盏新增补光从Auto切到ForcePixel（逐像素），或仅临时提高逐像素灯预算，都能消除墙纸缺口处的黑色亮度断层。上一轮仅确认了该区域是底墙，未充分检查两层表面的照明差异。`[verified: lighting-continuity-verification.json controls/conclusion]`

正式场景只将5盏补光的RenderMode设为ForcePixel；位置、颜色、强度、范围、上传调色、墙纸轮廓和全局质量设置均保留。更新后的场景与运行相机截图、3项墙纸EditMode和7项PlayMode结果见[光照连续性验证](lighting-continuity-verification.json)。先前验证及Windows运行包保留历史身份，本次未重新打包Player。`[verified: reports/lighting-continuity-change.json；lighting-continuity-verification.json]`

## 墙纸与黑斑

原12片壁纸存在穿入实体墙和过近贴面的顶点。保留真实深度的双色GPU对照中，墙壳穿插区域显示为蓝色，壁纸为红色；修正后这些区域由壁纸覆盖。正面同一视角有62,872个原墙体像素变为壁纸像素。`[verified: reports/surface-id-comparison.json positive-z；previews/depth-before.png／depth-after.png]`

12片壁纸使用独立生成网格，基准为内墙面向室内4毫米，重叠片再错开1毫米；保留原UV和切向撕裂轮廓，重算法线、移除投影后的退化三角形。壁纸不投射阴影，仍接收阴影；实体墙、碰撞体、材质和渲染队列保持原配置，源FBX未改。`[verified: reports/camera-seams/wall-repair.json；WallSurfaceRepairTests]`

生成网格位于 `UnityProject/Assets/MiniGame/Generated/WallSurfaceRepair/`。重新安装房间灯光或重建参考场景时会重复校验并应用，按原FBX身份重建而不累积偏移。菜单安装需先打开 MainRoomCombined；输出路径存在非本工具资产时拒绝覆盖。`[verified: RoomLightingSetup.EnsureInScene；WallSurfaceRepairSetup.Apply/Install/PreflightPersist]`

## 灯光和调色

- 场景灯光取自 GitHub 更新，新增5盏补光保留远端位置、颜色、强度和范围，设为Realtime，使其在运行中实际发光并能随睡前流程关闭。原睡床引导灯、门口镜头锚点、CRT和游戏组件保留。`[verified: reports/applied-lighting.json realtimeOverride/protectedBlocksUnchanged]`
- 附件是房间相机的后处理调色配置。已应用16级量化、Gamma 0.906／1.023、曝光0.28、对比−4.3、饱和6.8及附件各色调字段；保留此前颗粒0.05和原资产Meta GUID。原附件副本在 [sources/](sources/RoomLightingReferenceRetro.asset)。`[verified: reports/applied-lighting.json referenceAttachmentFieldsPreserved/filmGrainStrength/preservedMetaGuids]`
- 调色入口为 [RoomLightingReferenceRetro.asset](../../UnityProject/Assets/MiniGame/Resources/RoomLightingReferenceRetro.asset)；实际Light的位置和强度在 [MainRoomCombined.unity](../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity) 中。新梦境素材继续暂缓。`[verified: 本轮文件及用户范围]`

## 验证与运行

本轮检查与完整运行包见 [verification.json](verification.json)。编辑器工具与测试使用隔离工程 `Artifacts/Unity/OfflineProject`；几何前后对照使用原始世界渲染，实际Player截图则使用真正的玩家相机及所选调色后处理。`[verified: verification.json]`

2026-10-21再次核对：GitHub最新仍为`c28a44c0`，本次上传附件的34项序列化字段均已在主场景绑定的调色资产中；5盏补光的位置及灯光字段与远端一致，保留Realtime运行模式和已有颗粒0.05，无需再次改写场景。当前6项灯光／相机PlayMode测试通过，详见[本次核对记录](reports/current-source-check.json)。`[verified: reports/current-source-check.json attachment/scene/profile/unityVerification]`

用户重发墙体问题截图后，重新对当前保存场景做5个机位的深度／材质／阴影对照，并通过真实运行中玩家相机拍摄电脑墙与床头角落。大块墙纸遮挡在本地修复版中已解除，床头仍保留原墙纸缺口露出的深色底墙。当前本地修复的12片网格及修复工具尚未包含在GitHub的`origin/main`中；本轮未推送。结果与当前截图见[墙体复查记录](reports/wall-render-recheck.json)。`[verified: reports/wall-render-recheck.json observations/gameView/github]`

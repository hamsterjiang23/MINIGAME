# 交互、梦后醒来与 GM 结局跳转验收

本轮自动化结果为 **.NET 410/410、Unity EditMode 14/14、Unity PlayMode 49/49**；Player 构建、原生键鼠操作和视觉验收仍待完成。自动化通过不替代这些验收。`[verified: dotnet-all-final.log Passed 汇总；unity-editmode-final.json data.result.Summary；unity-playmode-final.json data.result.summary]` `[opinion]`

## 行为范围

| 范围 | 本轮行为与验证边界 |
|---|---|
| Day0、夜晚准心 | 未瞄准物体时保留准心；瞄准床时使用睡觉光标；暂停时隐藏世界交互光标。对象状态和光标渲染测试已通过，真实游戏画面仍待验收。`[verified: BedtimeGuidancePlayModeTests.cs L37-L49、L247-L251；unity-playmode-final.json results 的 AllFourCursorsRenderOnlyTheirYellowGlyphAndHideWhilePaused]` |
| 世界交互 | E 与鼠标左键共用主要交互，仍受焦点、距离、暂停和当前流程限制；同帧双输入只触发一次，ESC 优先，恢复暂停的同帧输入不能同时确认回答或推进对白。自动化直接驱动入口，原生键鼠验收待完成。`[verified: ApartmentPlayer.cs L112-L146；unity-playmode-final.json results 的 OnePrimaryActionReadsOpensWorksAndSleepsWithoutBypassingGates、ResumeSubmitCannotAlsoChooseAResponseOrAdvanceDialogueInTheSameFrame；PauseMenuPlayModeTests.cs L426-L474]` |
| Combined 距离 | 床、电脑、入口门为 **3.2m**；报纸、招聘、家信、海滩海报、乐器海报为 **3.5m**。测试重新加载 Combined 场景核对八项值，并验证距离控制焦点和操作；EditMode 覆盖撤销、重做和场景重载。`[verified: InteractionDistancePlayModeTests.cs L26-L40；unity-playmode-final.json results 的 PerObjectDistancesControlFocusAndUseBeyondTheFormerGlobalCap、PosterDistanceAlsoControlsFocusAndCaptionWithoutASeparateGlobalLimit；unity-editmode-final.json Results 的 DistanceEditsSupportUndoRedoValidationAndSceneReload]` |
| 普通梦后醒来 | 梦动画淡至全黑后自动跨日接醒来，不再照亮床、等待玩家再次上床；重复完成旧 cue 不能再次跨日。Day0 保留第一次床交互且不播放普通梦。`[verified: GameRuntime.cs L914-L950；unity-playmode-final.json results 的 DayOneBedProjectsOnTheAuthoredWallThenWakesOnceAtFullBlack、DayOneShutdownAndDreamWakeAutomaticallyWithPausedWakeCheckpoint、OrdinaryDreamWakesDirectlyWithoutGhostTraversalOrAnotherBedInteraction、DayZeroWaitsForTheBedAndOneInteractionSleepsWithoutShowingADream]` |
| Day3 隐藏分支 | 隐藏条件满足时进入自审电脑引导，梦尾不重新亮床；该分支没有经过普通 AfterDream 自动跨日。`[verified: GameRuntime.cs L923-L924、L941-L950；unity-playmode-final.json results 的 FinalDreamReturnsDirectlyToTerminalAcrossEveryRevealFrameAndPausedRestore、ResidentTrustSelfPurificationKeepsTheRuntimeAcrossBeachAndRestoresEndingProgress]` |
| 暂停与旧存档 | 旧 AfterDream 存档暂停恢复时留在黑幕，解除暂停后只醒来一次；新醒来检查点恢复阶段、剩余时间、相机姿态和遮罩透明度。`[verified: GameRuntime.cs L684-L724、L941-L950；unity-playmode-final.json results 的 LegacyAfterDreamLoadStartsBlackAndWaitsForPausedResumeBeforeWakingOnce、WakeFadeTurnAndStandRestoreTheirExactPausedFrameWithoutAdvancingTheDayAgain、PausedAutomaticWakeRestoresRemainingTimeWithoutAdvancingTheDayAgain]` |
| 隐藏 GM | 暂停根菜单同时按 N+O+R+M 开面板，需释放后重新按下才能再次触发；四个入口覆盖 TakenAway、LockedInRoom、PriestAttack、DreamSea。DreamSea 从自审开始，其余进入门口结局；替换当前运行并隔离旧回调，保留手动存档。原有标题四击快捷方式单独保留。`[verified: PauseMenuPlayModeTests.cs L111-L171；unity-playmode-final.json results 的 GmReplacesEachFlowAndPreservesManualSaveAndOldCallbackIsolation、GmFromFormalBeachRestoresRoomBeforeStartingAnotherDoorEnding、HiddenTitleRequiresFourConsecutiveClicksAndResetsAcrossMenuActions]` |
| GM 与旧存档交接 | 成功进入 GM 后清除旧 AfterDream 恢复标记、旧 cue、终端和门口状态，解除暂停，避免替换后误接旧梦醒来。`[verified: GameRuntime.cs L315-L332]` |

源码定位：[GameRuntime](../src/MiniGame.Presentation/GameRuntime.cs)、[ApartmentPlayer](../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs)、[BedtimeGuidancePlayModeTests](../UnityProject/Assets/MiniGame/Tests/PlayMode/BedtimeGuidancePlayModeTests.cs)、[InteractionDistancePlayModeTests](../UnityProject/Assets/MiniGame/Tests/PlayMode/InteractionDistancePlayModeTests.cs)、[PauseMenuPlayModeTests](../UnityProject/Assets/MiniGame/Tests/PlayMode/PauseMenuPlayModeTests.cs)。

## 自动化证据

| 检查 | 最终结果 | 原始产物 |
|---|---|---|
| 固定 SDK / restore | SDK **8.0.424**；锁定依赖恢复成功。`[verified: src/global.json sdk；dotnet-restore.log L1-L2]` | [restore 日志](../Artifacts/InteractionWakeGm20261023/dotnet-restore.log) |
| Release 构建 | 成功，**0 warning、0 error**。`[verified: dotnet-build-final.log L6-L8]` | [构建日志](../Artifacts/InteractionWakeGm20261023/dotnet-build-final.log) |
| .NET 全套 | Core **145** + Presentation **265** = **410**；失败 **0**、跳过 **0**。`[verified: dotnet-all-final.log L13-L16]` | [总日志](../Artifacts/InteractionWakeGm20261023/dotnet-all-final.log)、[Core TRX](../Artifacts/InteractionWakeGm20261023/dotnet-all-final_net8.0_20261023160127.trx)、[Presentation TRX](../Artifacts/InteractionWakeGm20261023/dotnet-all-final_net8.0_20261023160132.trx) |
| Unity 编译 | completed，failed=false，errors=[]。`[verified: unity-compile-final.json data.result]` | [编译结果](../Artifacts/InteractionWakeGm20261023/unity-compile-final.json) |
| Unity EditMode | InteractionWakeGm 分类 **14/14**；失败、跳过、不确定均 **0**。`[verified: unity-editmode-final.json data.result.Summary、FilterApplied]` | [EditMode 结果](../Artifacts/InteractionWakeGm20261023/unity-editmode-final.json) |
| Unity PlayMode | **49/49**；失败、跳过、不确定均 **0**。`[verified: unity-playmode-final.json data.result.summary]` | [PlayMode 结果](../Artifacts/InteractionWakeGm20261023/unity-playmode-final.json) |
| 先前文档链接检查 | **40** 个链接，missing=[]；该文件生成在本报告之前，不包含本报告的新链接。`[verified: doc-links.json checked、missing]` | [已有链接检查](../Artifacts/InteractionWakeGm20261023/doc-links.json) |

最终 Unity 结果记录的目标是 `F:\AAMINIGAME\MINIGAME\UnityProject`；下面的 UPM 恢复日志来自 `Isolated/UnityProject`，两者证据用途不同。`[verified: unity-compile-final.json data.target.projectPath；unity-editmode-final.json data.target.projectPath；unity-playmode-final.json data.target.projectPath；isolated-editor-recovered.log L76-L96]`

## 初轮失败与修复

初轮 PlayMode 为 **41 通过、8 失败**，保留在[初轮结果](../Artifacts/InteractionWakeGm20261023/unity-playmode-initial.json)。以下修复调整夹具前置状态或输入时序，保留对应行为断言；没有用跳过测试、移除失败检查或放宽阈值使这 8 项通过。最终同组仍为 49 项。`[verified: unity-playmode-initial.json data.result.summary、results；unity-playmode-final.json data.result.summary；下表源码位置]`

| 初轮失败 | 夹具修复及保留的检查 |
|---|---|
| 暂停睡前引导 1 项 | 恢复到第一次床交互前的 Evening，移除夹具提前进入梦境的操作；继续检查暂停粒子冻结、不可用家具不反馈或推进、床交互光标及灯光恢复。`[verified: BedtimeGuidancePlayModeTests.cs L208-L255]` |
| 隐藏结局自审入场 1 项 | 等待终端目录完成、PowerPhase=Running 且离开输入隔离帧，再发送确认；仍要求确认成功、进入 Review、自审身份及完整 16 行内容。`[verified: DreamCampaignIntegrationTests.cs L310-L325]` |
| 梦境穿行相关 5 项 | 夹具病人使用 `PrototypeRules.Child`，使 resident trust 配置可进入真实 SelfReview 条件；大步碰撞、停用/重绑定控制器、电脑引导和入场姿态的断言保留。`[verified: DreamTraversalPlayModeTests.cs L64-L155、L176-L186、L237-L266；unity-playmode-initial.json data.result.results 的五个 DreamTraversal 失败项]` |
| 暂停恢复输入隔离 1 项 | 新增断言确认同步夹具仍在门口返回的隔离帧，该帧按键应拒绝；等待下一帧后再选择回答。继续检查恢复同帧 Return/Space 不改变 cue 或存档，下一帧正常生效。`[verified: PauseMenuPlayModeTests.cs L439-L475]` |

另外，初轮 EditMode 为 **12/14**，两项失败来自调试进度生成未到达 Day2/Day3；记录见[初轮 EditMode](../Artifacts/InteractionWakeGm20261023/unity-editmode-initial-failures.json)。[CampaignDebugCheckpoint](../UnityProject/Assets/MiniGame/Editor/CampaignDebugCheckpoint.cs) 的生成策略改为可用时优先 `Purify`，其次 `Treat`，以满足夜间配额并推进后续调试日期；这是调试存档生成政策，不是玩家应如何处置病人的建议。`AfterDream` 调试目标也接受自动醒来后的 MorningInspection / Ending。最终 14 项全部通过。`[verified: unity-editmode-initial-failures.json data.result.Summary、Results；CampaignDebugCheckpoint.cs L45-L48、L86-L92；unity-editmode-final.json data.result.Summary]`

## Package Manager 环境故障

故障日志在项目包加载前报 `The "path" argument must be of type string. Received undefined`，resolve/list 返回 500。诊断记录根因为启动环境缺少 `ALLUSERSPROFILE`；安装的 UPM 在 Windows 上用 `path.join(process.env.ALLUSERSPROFILE, "Unity/config")` 构造全局配置路径。此问题属于启动环境，不是本轮 C# 编译错误。`[verified: upm-failure-editor.log L68-L69；upm-failure.log L4-L18；verification-progress.json PackageManager.Cause；E:/Unity/Editors/2022.3.23f1/Editor/Data/Resources/PackageManager/Server/server/app.js L186]`

在 Editor 启动环境补入 `ALLUSERSPROFILE=C:\ProgramData` 后，隔离工程日志记录 **0.31 秒完成包解析、注册 18 个包**，包含两个本地 MiniGame 包。该记录证明 UPM 恢复，不替代后续编译、测试或 Player 验收。`[verified: verification-progress.json PackageManager.Remedy；isolated-editor-recovered.log L72-L99]` `[opinion]`

证据：[故障 Editor 日志](../Artifacts/InteractionWakeGm20261023/upm-failure-editor.log)、[故障 UPM 日志](../Artifacts/InteractionWakeGm20261023/upm-failure.log)、[诊断记录](../Artifacts/InteractionWakeGm20261023/verification-progress.json)、[恢复 Editor 日志](../Artifacts/InteractionWakeGm20261023/isolated-editor-recovered.log)。

## 视觉证据限制与待验收项

- **`day0-bed-reticle` 截图无效。** [现存 PNG](../Artifacts/InteractionWakeGm20261023/captures/day0-bed-reticle.png) 是黑画面，不能证明准心可见；其旧截图助手将 Canvas 改为 ScreenSpaceCamera、切换 RenderTexture 并手动渲染，改变了待验收的呈现路径。`[verified: captures/day0-bed-reticle.png 实际像素；BedtimeGuidancePlayModeTests.cs L357；PrototypePlayModeTests.cs L265-L298]`
- **`wake-black` 截图无效。** [现存 `day1-wake-black` PNG](../Artifacts/InteractionWakeGm20261023/captures/20261023T0837158580939Z-e491b357/day1-wake-black-frame000969.png) 显示梦境与暂停菜单，而[同批 manifest](../Artifacts/InteractionWakeGm20261023/captures/20261023T0837158580939Z-e491b357/manifest.jsonl) 标记 Day2、MorningInspection、Paused=false；异步截图取得了与请求状态不一致的旧 framebuffer，不能证明梦尾全黑。请求时状态断言通过与最终 PNG 正确是两项不同检查。`[verified: 上述 PNG 实际像素；manifest.jsonl Label=day1-wake-black；EndingVisualCapture.cs L34-L61]`
- **其他截图尚未判定通过。** 文件存在、PNG 尺寸有效、SHA256 和 manifest 均不自动构成视觉验收；本报告不将任何其他 capture 标记为有效。`[verified: EndingVisualCapture.cs L64-L88]` `[opinion]`

截图助手源码：[PrototypePlayModeTests](../UnityProject/Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs)、[EndingVisualCapture](../UnityProject/Assets/MiniGame/Tests/PlayMode/EndingVisualCapture.cs)。

| 待验收项 | 状态与完成条件 |
|---|---|
| Windows Player 构建与运行 | **Pending**。补入实际构建结果、产物路径和启动运行证据后更新；本报告不根据 Editor 测试宣称 Player 已通过。`[opinion]` |
| 原生键鼠输入 | **Pending**。在实际 Player 中验证 E/左键、同帧双输入、ESC 优先、暂停恢复隔离、N+O+R+M 和 GM 返回；直接调用方法或 uGUI 回调的测试不替代硬件输入链路。`[opinion]` |
| 视觉验收 | **Pending**。重新取得保持原 Canvas/相机/分辨率、且帧状态与像素一致的实际画面，核对 Day0/夜晚准心、梦尾全黑后敲门醒来、床不闪亮、Day3 自审电脑引导和 GM 切换。`[opinion]` |

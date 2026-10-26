# 自我审判音效与 Day3 返回视角排查（2026-10-26）

反馈范围：最新外发包，自我审判净化按钮出现时无音效；Day3 梦境结束、尚未使用电脑时无法自由转头。第二条由用户进一步确认发生在使用电脑之前。[verified: 本次用户反馈]

## 音效：已复现并修复源码

原音频宿主仅在普通会谈进入 `EncounterStage.AwaitingDisposition` 时播放 `LightBeep`。自我审判通过 `SelfReviewReady` 表示对白结束，不进入普通会谈的处置阶段，导致漏播。现在分别检测这两个状态的首次变化，沿用原音源及音量；暂停恢复和重新绑定不会重复播放。[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameAudioDirector.cs L174–L184]

新增回归在旧实现上失败，提示音次数为 0、预期为 1；修复后通过，并检查按钮可见、音源实际播放及暂停/绑定后不重播。[verified: ../../Artifacts/SelfReviewBugs/before.xml；../../Artifacts/SelfReviewBugs/after.xml；../../UnityProject/Assets/MiniGame/Tests/PlayMode/SelfReviewUIPlayModeTests.cs SelfPurificationButtonPlaysOneLampBeepAndPauseOrRebindDoesNotReplayIt]

## 视角：本次未复现，尚未修复

新增集成回归通过真实 Campaign 命令走到 Day3 晚间，实际与床交互进入梦境，再完成返回。启用玩家组件后检查 World 输入、过渡锁、梦境镜头所有权、睡眠镜头及阅读输入阻挡，并在转头后逐帧确认没有被重新拉回电脑。[verified: ../../UnityProject/Assets/MiniGame/Tests/PlayMode/DreamCampaignIntegrationTests.cs FinalDreamReleasesWorldLookBeforeSelfReviewComputerInteraction；../../Artifacts/SelfReviewBugs/after.xml]

另以临时诊断脚本构建独立 Windows Mono、非 Development Player，使用正式 Campaign 的合法 Day3 晚间检查点，按实际帧时序播放睡眠及梦境。返回后记录到 125 条 SelfReview/World 样本，全部已释放过渡与梦境镜头；其中 83 条存在非零鼠标输入，水平角范围 261.124359°–307.60437°，俯仰角范围 9.916829°–15.8318319°，这次运行能够转头。[verified: ../../Artifacts/SelfReviewBugs/player-input.jsonl]

诊断版与原 ZIP 分别对待：诊断版只在隔离工程加入采样和检查点启动脚本，并包含本次音效修复；未对原外发 ZIP 的完整玩家路径做视觉验收。窗口捕获重试仍返回 `window capture timed out`，因此未取得窗口截图，不将日志当成画面验收。诊断进程结束后已移除隔离工程的临时脚本，恢复其产品配置。[verified: ../../Artifacts/SelfReviewBugs/diagnostic-build.log；../../Artifacts/SelfReviewBugs/diagnostic-player.log；本次工具返回与清理记录]

当前证据只能说明本机该路径没有持续锁定，不能排除特定运行历史或现场状态的问题，也不能据此宣称第 6 条已修复。[opinion]

## 验证范围

- 本轮隔离 Unity 2022.3.62f3c1 Play Mode：8 项通过、0 失败，覆盖自我审判 UI、一次性提交、提示音、普通音频检查点与 Day3 镜头释放；另一次检查点捕获回归 1 项通过。[verified: ../../Artifacts/SelfReviewBugs/after.xml；../../Artifacts/SelfReviewBugs/capture.xml]
- .NET Core/Presentation 规则未修改，未重跑 .NET；本轮未重跑 Edit Mode。[verified: 本次 Git 差异与执行记录]
- 临时诊断 Player 构建成功；未重建正式外发 ZIP，未宣称其完整流程通过。[verified: ../../Artifacts/SelfReviewBugs/diagnostic-build.log；本次执行记录]

## 后续外发包

按用户后续要求已生成 [NORM-SelfReviewAudioFix.zip](../../Releases/2026-10-26/NORM-SelfReviewAudioFix.zip)，详细证据见 [打包验证](../../Releases/2026-10-26/NORM-SelfReviewAudioFix.verification.json)。包含音效修复和工作区已保存的终端选项、配额数字、医生标点等修正；第 6 条仍标为未复现。[verified: ../../Releases/2026-10-26/NORM-SelfReviewAudioFix.verification.json changes/unresolved]

最终包为 Windows x64、Mono、非 Development 的 NORM；本轮 20 项 Edit Mode 与 13 项 Play Mode 通过。ZIP 解压后的 160 个文件哈希与构建产物一致，程序图标资源与此前验收版逐项相同；普通启动和缓存 720p 配置启动均确认 2560×1440 原生无边框全屏，像素偏好未变。未执行该 ZIP 的窗口视觉或人工全流程验收。[verified: ../../Releases/2026-10-26/NORM-SelfReviewAudioFix.verification.json editMode/playMode/archiveFiles/binaryChecks/nativeStartup/cached720pStartup/limitations]

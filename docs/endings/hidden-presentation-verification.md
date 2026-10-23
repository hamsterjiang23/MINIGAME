# 隐藏结局演出修复验收 — 2026-10-23

本轮实现自审病房背景、指定音乐三秒黑幕前奏、上床收尾和 Day3 无床光返回；流程见[结局说明](guide.md)。`[verified: 本轮用户实施方案及上传音乐指令；下列实际测试记录]`

## 自动验证

| 检查 | 结果与证据 |
| --- | --- |
| SDK 8.0.424 锁定恢复 | `dotnet restore MiniGame.sln --locked-mode` 通过。[恢复日志](../../Artifacts/HiddenEndingRevision20261023/dotnet-restore.log) `[verified: 日志恢复成功]` |
| Release 构建 | `dotnet build MiniGame.sln -c Release --no-restore` 通过，0警告、0错误。[构建日志](../../Artifacts/HiddenEndingRevision20261023/dotnet-build.log) `[verified: Build succeeded / 0 Warning(s) / 0 Error(s)]` |
| 核心／表现层 | `dotnet test MiniGame.sln -c Release --no-build --no-restore`：140 + 249 = 389项通过。[测试日志](../../Artifacts/HiddenEndingRevision20261023/dotnet-test.log) `[verified: 两个 Passed 汇总]` |
| Unity 编译 | 无编译错误。[编译结果](../../Artifacts/HiddenEndingRevision20261023/unity-compile.json) `[verified: data.result failed=false/errors=[]]` |
| 自审 UI | 5/5，通过两张立绘的底图、滤镜、像素及交互检查。[结果](../../Artifacts/HiddenEndingRevision20261023/unity-self-review-final.json) `[verified: summary/results]` |
| 梦后灯光 | 4/4，包含 Day3 每个返回帧和暂停恢复无床灯／光束／附属效果，普通夜晚仍亮床。[结果](../../Artifacts/HiddenEndingRevision20261023/unity-day3-final.json) `[verified: summary/results]` |
| 结局流程 | 9/9，包含真实时钟演出、暂停、躺下／看天／淡出逐阶段存读档、音乐游标恢复、重复床交互、单次EndNote、已完成存档及其他结局。[结果](../../Artifacts/HiddenEndingRevision20261023/unity-campaign-final.json) `[verified: summary/results]` |
| 转场兼容 | 11/11。[结果](../../Artifacts/HiddenEndingRevision20261023/unity-transition-final.json) `[verified: summary/results]` |
| 独立海滩预览 | 6/6。[结果](../../Artifacts/HiddenEndingRevision20261023/unity-preview-final.json) `[verified: summary/results]` |
| 监控合成／监控生命周期 | 4/4 + 7/7。[合成](../../Artifacts/HiddenEndingRevision20261023/unity-monitor-final.json)、[生命周期](../../Artifacts/HiddenEndingRevision20261023/unity-surveillance-final.json) `[verified: summary/results]` |

Unity 定向 PlayMode 共46项通过；结果文件保存最终重跑结果。[汇总](../../Artifacts/HiddenEndingRevision20261023/test-summary.json)。编辑器已退出播放和编译，Combined场景未脏。[编辑器状态](../../Artifacts/HiddenEndingRevision20261023/unity-final-state.json)。`[verified: test-summary.json Total/Passed；unity-final-state.json result playing=false/compiling=false/dirty=false]`

## 实际画面与音乐

已逐张检查正式 `MainRoomCombined` 的 Game View 截图：自审有病房及黄色监控效果，上床中段发生视角旋转，躺稳只见天空，随后逐步暗下、全黑、回菜单。最后一轮截图为1920×1080，包含场景状态与SHA-256的[截图清单](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/manifest.jsonl)可追溯。`[verified: 本轮 view_image 画面检查；manifest.jsonl Width/Height/GamePhase/EndingStage/Sha256]`

- [自审 SJ01](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-self-dialogue-SJ01-frame000685.png)、[自审 SJ02](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-self-dialogue-SJ02-frame000695.png)。`[verified: manifest.jsonl hidden-self-dialogue-SJ01/SJ02]`
- [三秒音乐黑幕](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-full-black-frame000756.png)、[入场](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-entry-camera-frame000856.png)。`[verified: manifest.jsonl BlackHold/Entering]`
- [上床中段](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-bed-lying-camera-frame000894.png)、[躺稳看天](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-bed-sky-hold-frame000919.png)。`[verified: manifest.jsonl hidden-bed-lying-camera/hidden-bed-sky-hold]`
- [淡出中段](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-bed-fading-out-frame001228.png)、[全黑](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-bed-fully-black-frame001245.png)、[回主菜单](../../Artifacts/HiddenEndingRevision20261023/20261023T0651144030158Z-aab1505a/hidden-main-menu-frame001247.png)。`[verified: manifest.jsonl 对应Label/EndingStage]`

用户上传的《An Ending (Ascent)》已绑定 `AudioDefault` 的 EndingMusic；复制前后文件SHA-256一致。真实时钟测试核对黑幕有效时间≥2.9秒、音频样本推进≥2.8秒（帧与音频缓冲容差），入场仍为同一音源及曲目且游标不回退；黑幕常量为3秒。暂停时游标冻结，存读档恢复游标容差0.25秒。`[verified: AudioDefault.asset Sounds Id13；2026-10-23 Get-FileHash 输出；EndingCampaignPlayModeTests VerifyRoomEntry/AssertContinuousEndingMusic/HiddenBedPresentationPausesAndRestoresEveryStageWithoutEarlyCompletion]`

本轮未构建独立Player，也未录制扬声器输出；音频验收范围是Unity真实AudioSource播放状态和样本推进，画面验收范围是上述正式场景截图。`[verified: 本轮执行记录仅.NET构建、Unity编译/PlayMode及Game View截图]`

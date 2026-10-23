# v0.8.24 右侧交互区验收（2026-10-23）

本轮仅更新终端右侧布局、按钮资源与处置输入，保留工作区已有剧情、梦境、GM、字体和监督员修改。核心规则与存档 DTO 没有因本轮终端调整而修改。`[verified: TerminalView、TerminalButtonVisual、TerminalQuotaField 本轮差异；本轮文件修改记录]`

## 自动检查

| 检查 | 本轮结果与证据 |
|---|---|
| Unity 编译 | 完成，0 错误。`[verified: verification-v0.8.24/compile-isolation.json data.result.errors]` |
| EditMode | 37/37 通过；包含 12 张状态图导入、00—99 图集、负值／三位数回退和现有字体检查。`[verified: verification-v0.8.24/editor-final.json summary/results]` |
| PlayMode | `TerminalV0824` 类别 23/23 通过；包含方向选择、Enter／小键盘 Enter／空格不提交、左键单次提交、右键／重复点击／禁用／暂停／记录／演出拦截、自审仅净化、四选项及三种 CRT 点击范围。`[verified: verification-v0.8.24/play-final.json summary/results]` |
| 核心配额接线 | 释放／治疗不扣净化配额，净化扣一次，存档恢复及下一天数值继续更新。`[verified: play-final.json TerminalDailyWorkIndicatorPlayModeTests、NormPurificationKpiPlayModeTests]` |
| Windows Development | `StandaloneWindows64` 构建成功，0 错误；唯一提示是场景未包含 RuntimePipelineManager，Player 内 CLI 不启用。`[verified: verification-v0.8.24/build.json result/errors/warnings]` |

复测修正了旧夹具的应答坐标、固定五人目录假设、同帧菜单输入拦截和 Combined 场景残留；未通过改动游戏规则消除失败。早期宽泛 `Terminal` PlayMode 筛选还包含 Day0 启动测试，其主菜单夹具未就绪，运行被取消；这里不声称全工程 PlayMode 全量通过。原始记录保留于 `Artifacts/TerminalV0824/play-focused-progress.json`、`play-broad-cancelled.json`。`[verified: 两份原始测试记录及 AmberTerminalPlayModeTests、TerminalHtmlParityTests、SelfReviewUIPlayModeTests 本轮差异]`

## 正式场景与 Player 画面

Editor 和 Windows Player 均加载正式 `MainRoomCombined`，使用正式 Campaign 规则及结局目录生成的 Day1 目录检查点，经过实际 UI 射线、PointerDown／Up／Click 推进会谈。两端均检查键盘不提交、按下尚未提交、一次净化使配额 01→00、旧按钮重复回调不重复提交；主菜单和手动存档路径均位于独立验收目录。`[verified: verification-v0.8.24/Editor/report.json；verification-v0.8.24/Player/report.json 的 result、manualSavePath、captures]`

| 状态 | 正式 Combined | Windows Player |
|---|---|---|
| 会谈中锁定 | [原生图](verification-v0.8.24/Editor/locked-terminal-native.png) | [原生图](verification-v0.8.24/Player/locked-terminal-native.png)；三行 Disabled。`[verified: 两端 locked-state.json commands]` |
| 选中及普通行 | [原生图](verification-v0.8.24/Editor/normal-selected-terminal-native.png) | [原生图](verification-v0.8.24/Player/normal-selected-terminal-native.png) · [CRT 图](verification-v0.8.24/Player/normal-selected-terminal-crt-gpu.png)；释放 Selected，其余 Normal。`[verified: 两端 normal-selected-state.json commands]` |
| 按下、尚未提交 | [原生图](verification-v0.8.24/Editor/pressed-before-submit-terminal-native.png) | [原生图](verification-v0.8.24/Player/pressed-before-submit-terminal-native.png)；净化 Pressed，核心存档哈希与按下前相同。`[verified: 两端 pressed-before-submit-state.json runtimeHash/commands]` |
| 提交后 | [原生图](verification-v0.8.24/Editor/committed-terminal-native.png) | [原生图](verification-v0.8.24/Player/committed-terminal-native.png)；配额 00，按钮 Disabled。`[verified: 两端 committed-state.json quota/commands]` |

每端四个阶段、每阶段三行，各 248×48 的全部 11,904 个像素与资源包对应状态 PNG 完全一致，最大通道差为 0；66×32 配额框的 01 和 00 也完全一致。比较针对原生 800×600 终端源纹理。`[verified: verification-v0.8.24/Editor/pixel-comparison.json rows/quotas；verification-v0.8.24/Player/pixel-comparison.json rows/quotas]`

原生图来自 `TerminalDisplay.Texture`；CRT 图由实际 `ScreenMaterial` 在 GPU 合成。Player 使用 Direct3D 11／RTX 4070、`-batchmode`，未使用 `-nographics`。这些是运行时渲染采集和程序化 UI 输入验收，未将其标作操作系统整窗录屏或人工鼠标键盘验收；用户的最终视觉认可仍由用户决定。`[verified: TerminalCommandPlayerProbe.Capture；Artifacts/TerminalV0824/player-development.log Direct3D/Renderer；player-development-process.json args]` `[opinion]`

HTML 的布局和交互来自 [保存的原文件](reference-v0.8.24.html)；逐像素对照来自资源包的四态 PNG。没有用原型的固定 05、测试阈值或角色对白替换正式规则，也没有重新生成现有美术。`[verified: TerminalView.Render/RenderDispositionCommands；TerminalCommandAssetTests.CommandsReferenceAllTwelveUnscaledPointFilteredArtworkStates；资源导入差异]`

## 复现入口

Editor 在正式 Combined 的 Play Mode 调用 `TerminalCommandPlayerProbe.StartForEditor(checkpointPath, outputPath)`；完成后不退出 Editor。Development Player 使用以下参数；检查点是 `GameRuntime.Save()` 的正式 Day1 目录 JSON，须与当前 Campaign／结局目录版本匹配。验收程序只编译到 Editor／Development，普通启动不激活。`[verified: TerminalCommandPlayerProbe.cs Prepare、StartForEditor、Start、Finish 及编译条件]`

```text
--terminal-v0824-smoke
--terminal-v0824-checkpoint <绝对检查点路径>
--terminal-v0824-output <绝对输出目录>
```

本次检查点与逐像素比较脚本保留于 `Artifacts/TerminalV0824/day1-directory.json`、`compare-artwork.py`；构建产物为 `Artifacts/TerminalV0824/Player/MiniGame.exe`。CLI 的字符串参数没有可靠传入数组，因此最终构建通过 `eval` 调用已存在的 `BuildCommand.Build`，明确传入 `new[]{"Development", "DetailedBuildReport"}` 和 Combined 场景数组；最终 Player 程序集已核实包含验收程序并运行通过。`[verified: Artifacts/TerminalV0824/build-development-start.json；Player/report.json]`

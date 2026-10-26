# 全屏渲染尺寸修正

本轮本机显示器为 2560×1440，NORM 旧的持久化全屏渲染尺寸为 1280×720，像素化档位仍为中（4）。全屏模式本身不能证明渲染尺寸已匹配显示器，因此此前仅检查启动无异常的验收范围不足。[verified: ../../Artifacts/Packages/NORM-20261026-NativeResolution/monitor-before.json；../../Artifacts/Packages/NORM-20261026-NativeResolution/before-player-preferences.json]

启动改为读取 `Display.main.systemWidth/systemHeight`，避免使用旧全屏状态的 `Screen.currentResolution` 作为原生尺寸来源；等待帧末切换后，连续三帧确认 `Screen.width/height` 与显示器一致。初始化延迟时最多尝试 120 帧，日志明确记录成功尺寸或未稳定状态。像素化档位、场景画面参数和对白样式不因此修改。[verified: ../../UnityProject/Assets/MiniGame/Runtime/ReleaseDisplayStartup.cs ApplyNativeResolution/Start；Unity 官方 [Display.systemWidth](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Display-systemWidth.html)、[Screen.SetResolution](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen.SetResolution.html)]

图形 batch 启动与普通启动使用同一分辨率路径，最终包启动验收不再传入 1280×720 等尺寸参数。无图形设备或显式分辨率／全屏命令行参数继续保留调用者选择。[verified: ReleaseDisplayStartup.ShouldUseNativeResolution；../../UnityProject/Assets/MiniGame/Tests/EditMode/ReleaseDisplayStartupTests.cs；../../Artifacts/Packages/NORM-20261026-NativeResolution/Package.ps1]

本轮从最终 ZIP 解压的正式 Player 记录 `render=2560x1440 monitor=2560x1440 mode=FullScreenWindow pixel=4`。这是实际引擎渲染尺寸检查，未将其等同于人工清晰度或完整流程试玩。[verified: ../../Artifacts/Packages/NORM-20261026-NativeResolution/player-startup.log NORM_DISPLAY_READY]

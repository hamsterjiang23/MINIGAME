# 敲门同步震动

按用户对首版「太轻，需要类似地震」的反馈，默认旋转震幅由 0.14 度提高到 1.1 度，并加入基准 0.022 米的上下、左右位移。每一下使用不同方向频率的 5.3–15.7 Hz 混合震荡，余震从 0.15 秒延长到 0.48 秒；四次重击的响应叠加，形成持续震荡，最后一击的余震仍在录音结束前归零，两组之间的半秒间隔保持静止。两项幅度可在相机的「敲门震动」组件中调整。`[verified: KnockImpactTrack.cs ResponseSeconds/Evaluate；KnockScreenShake.cs strength/translationStrength/PositionOffset；用户反馈]`

组件通过 Bootstrap 安装到玩家相机。敲门使用独占的 `PlayRecording` 播放，逐帧读取该 AudioSource 的 `timeSamples`；震动没有另一套累加计时器。录音播放、暂停、恢复、重新播放和开门停止时，画面跟随同一播放游标。仅在相机渲染期间叠加旋转与位移，随后恢复原始姿态。该效果跟随现有 DoorKnock 通道，包含使用相同敲门录音的结局来访。`[verified: MiniGameBootstrap.cs InstallRuntime；MiniGameAudioService.cs PlayRecording；KnockScreenShake.cs CurrentRotationOffset/CurrentPositionOffset/OnPreCull/OnPostRender]`

录音 `NewDoorKnock.wav` 为 44100 Hz 单声道，61617 样本。检测使用 5 ms RMS 窗口、攻音/释放双阈值及最短间隔，排除敲门后的低响度 rattles，随后精化至 PCM 样本。四个攻音为样本 2437、11235、20140、30497，时间约 0.055261、0.254762、0.456689、0.691542 秒。`[verified: 2026-10-25 独立 WAV 检查及生产 KnockImpactTrack 的 C# 时序校验输出]`

音频 SHA-256：`831545442e541bda66f41239294811ca4e938afb37362861c57f3b3da1db3207`。换用其他录音时会重新分析该录音，不继续使用这四个固定时间。`[verified: 2026-10-25 Get-FileHash；KnockScreenShake.cs RefreshTrack]`

## 本次验证范围（2026-10-25）

- 当前生产 `KnockImpactTrack.cs` 在独立 C# 校验器中读取真实 PCM：四次新冲击、整组开始前静止、敲击间余震、最后一击后 480 ms 内回落、尾音静止、纯静音，以及 30/60/144 fps 各 20 个周期采样通过。校验器仅用托管数值类型代替 Unity 数值类型；此结果验证算法，不验证 Unity 音频设备或渲染。`[verified: work/knock-validation/TimingHarness.exe 当次输出 PASS，work 位于仓库所在工作目录]`
- 新增 `KnockImpactTrack`、`KnockScreenShake`、修改后的音频 service 及现有 audio profile，使用安装的 Unity 2022.3.23f1c1 编译器和真实 Core/Audio API 程序集编译通过；director 仅提供接口桩。Bootstrap/整个工程不属于此次独立编译范围。`[verified: work/knock-validation/KnockUnityApiCheck.dll 当次编译退出码 0]`
- `MiniGameAudioPlayModeTests` 新增真实录音的四击检测、游标/暂停/间歇/开门、实际渲染期间偏移和恢复姿态的检查。**尚未运行**：本机 Editor 启动报告 `No valid Unity Editor license found` 并退出，未完成导入、Play Mode 或 Player 构建。激活许可证后应运行完整 `MiniGameAudioPlayModeTests`，并在 MainRoomCombined 中确认四下、间歇及力度。`[verified: work/knock-validation/editor.log；MiniGameAudioPlayModeTests.cs 新增三项检查]`
- 新版同步 MP4 预览使用既有门口截图、当前生产 C# 导出的响应曲线及原始音频，四组原声保持原始长度并各补半秒间隔，60 fps；已完成整段音视频解码检查。旋转使用小角度近似，位移按 2.5 米参考深度估算；截图边缘反射扩展以免黑边，不能替代游戏实录。首版视频独立保留。`[verified: work/knock-validation/make_preview.py；outputs/SupervisorKnock-EarthquakePreview.mp4；outputs/SupervisorKnock-SyncPreview.mp4，均位于仓库所在工作目录]`

Unity 播放游标 API 说明：[AudioSource.timeSamples](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource-timeSamples.html)。最终声画感受还需在实际显示刷新率与音频设备上检查。`[opinion]`

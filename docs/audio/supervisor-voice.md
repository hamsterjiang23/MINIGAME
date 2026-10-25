# 监督员模糊低沉的语言配音

2026-10-25 用户要求为监督员制作与语句长度对应的含糊配音。首版纯合成喉音经试听被指出太像机器轰鸣，随后改为中文男声语言底音。本轮反馈要求发声更密集连贯、没有停顿，并进一步降低人声的可辨认度；当前加强降调和咬字模糊，同时压缩语音空隙、取消标点留白，声音总长继续关联字幕分页。[verified: 本轮用户反馈；下述实现文件]

## 调研与选择

| 方案 | 能力与代价 | 本轮选择 |
| --- | --- | --- |
| 真人录音或 TTS 后处理 | 保留语言的发音、换气和句调，再处理音高和咬字 | 当前使用本机离线中文男声 TTS；后续可替换成演员录音 |
| 纯声源与声道合成 | 能生成固定时长，但周期声门与低频层容易产生机械轰鸣感 | 保留首版生成器用于历史复现 |
| 经处理的语言素材分段合成 | 复用语言音色，按当前页组成完整 PCM；较长连续片段保留自然音节衔接 | 当前运行时采用 |

上表为工程选择，不能保证主观恐怖程度。[opinion]

当前素材使用系统已安装的 `Microsoft Kangkang` 中文男声，以本项目监督员对白生成语言底音。这是离线合成语音。通过 [Microsoft SpeechSynthesizer](https://learn.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer) 选声与导出 WAV；处理阶段使用 [FFmpeg rubberband](https://ffmpeg.org/ffmpeg-filters.html#rubberband) 将音高降到 0.60 倍，约低 8.8 个半音，再做 90 Hz 高通与 950 Hz 低通。检测并交叉淡化拼接发声区，压缩原语音的句间空隙。局部逆序混合加强到 85%，片段长度在 75–160 毫秒之间变化；18% 的错位逆序语音层与短反射填充音节低谷；16% 的轻微变延迟语音反射增加湿润咕嘟感。软压缩控制峰值并提高语音主体响度。具体来源与数值记录在 [bake-report.json](../../UnityProject/Assets/MiniGame/Resources/MiniGame/SupervisorVoice/bake-report.json)。[verified: ../../Tools/SupervisorVoice/render_speech.ps1；../../Tools/SupervisorVoice/process_speech.py]

首版的纯声源/声道方案参考 [Stanford：Formant Synthesis Models](https://ccrma.stanford.edu/~jos/pasp/Formant_Synthesis_Models.html)，历史生成器为 [bake_voice.py](../../Tools/SupervisorVoice/bake_voice.py)，需明确传入 `--legacy` 才会恢复旧音色。[verified: bake_voice.py]

播放使用 Unity 2022.3 的 [AudioClip.Create](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.Create.html) 创建整页 PCM，文字显示读取 [AudioSource.timeSamples](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource-timeSamples.html)；暂停使用 [AudioSource.Pause](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioSource.Pause.html)。声音游标控制出字，因此低帧率不会累计两套计时的偏差。[verified: ../../UnityProject/Assets/MiniGame/Runtime/SupervisorVoicePlayer.cs；../../UnityProject/Assets/MiniGame/Runtime/DialogueTextPlayback.cs]

## 当前行为

- 生效于 `supervisor__` 的正式监督员对白，以及 `supervisor-ending__` 的监督员结局对白。神父、病人、自审与旁白不触发。[verified: ../../UnityProject/Assets/MiniGame/Runtime/SupervisorVoiceSynthesizer.cs IsSupervisorLine]
- 每页音频时长为「Unicode 文字单元数 ÷ 当前出字速度」，精度为一个采样点，默认 30 字/秒。标点、空格和省略号计入总时长，发声连续覆盖当前页，不在这些位置插入停顿；纯空白页沿用原有无配音行为。同一文本生成同一声音。[verified: SupervisorVoiceSynthesizer.CreateClip]
- 语音使用连续约 0.85–1.4 秒的语言片段，并只做轻微速度变化，以保留音节衔接。字幕展示作者原文，声音由经过模糊处理的语言素材拼成对应长度；较短页面截取同样长度的语言片段。[verified: SupervisorVoiceSynthesizer.AddMurmur]
- 首次点击补全当前页时立即停声；下一次翻页或进入下一句才开始新配音。分页不改变逻辑对白完成回调。[verified: DialogueTextPlayback.Advance/ShowPage]
- 暂停、历史记录、主菜单遮挡、演出输入锁与调试面板遮挡会暂停配音。恢复使用同一音频游标；切换会话、读档重绑、隐藏对白和销毁时清理旧配音。存档仍沿用原有对白行检查点，读档从该行起始重播文字与配音。[verified: MiniGameScreen.SyncSupervisorVoicePause/Bind；SupervisorVoicePlayer.Stop]
- 独立 `SupervisorVoice` 音轨追加在现有枚举末尾，经音效混音组输出；不会替换音乐、海浪或梦结局旁白音轨。[verified: MiniGameAudioService.MiniGameAudioChannel/Route]
- 无配音素材、开关关闭时沿用原有文字计时。出字速度为 0 的调试模式下文字瞬显，声音按配置字速播放，点击仍立即停止。异常超长/极慢输入超过 60 秒时回退原有文字计时，避免改变文字速度或分配过大音频；正式默认速度下已覆盖全部作者对白。[verified: SupervisorVoiceProfile；SupervisorVoiceSynthesizer.CreateClip]

## 调整与试听

在 Project 中选择 [SupervisorVoice/Default.asset](../../UnityProject/Assets/MiniGame/Resources/MiniGame/SupervisorVoice/Default.asset)，可调整启用开关、配音音量与调试瞬显模式的配音速度。实际逐字显示速度仍由 `MiniGameScreen.DialogueCharactersPerSecond` 决定。[verified: SupervisorVoiceProfile；MiniGameScreen.PresentDialogueLine]

按用户先前试听反馈，配音输出曾由 0.72 提高到 1.44；本轮在已接近目标的音色上略微加深咕嘟质感，并按最新要求将当前音量降低 20%，默认增益为 1.152（1.44 × 0.8，约降低 1.94 dB）。超过 1 的增益施加到当前页的独立 PCM，避免被 AudioSource 的音量上限截断。[verified: Default.asset Volume；SupervisorVoicePlayer.BeginPage]

菜单 **MiniGame → 音频 → 导出监督员配音试听** 会使用与游戏相同的合成器，导出五句原有对白及合并试听 WAV，默认写入 `Artifacts/Audio/SupervisorVoice`，附带文字和秒数；本轮取消了旧试听的句间 0.7 秒留白，片段直接连续拼接。设置环境变量 `MINIGAME_SUPERVISOR_VOICE_PREVIEW` 可以指定其他导出目录。[verified: ../../UnityProject/Assets/MiniGame/Editor/SupervisorVoicePreview.cs]

重新制作素材：在安装了对应中文男声的 Windows 中运行 `Tools/SupervisorVoice/render_speech.ps1`，再运行 `python Tools/SupervisorVoice/process_speech.py --ffmpeg <FFmpeg路径>`，需要 NumPy 与支持 rubberband 的 FFmpeg。原始语音和来源清单写入 `Artifacts/Audio/SupervisorSpeechSource`；10 个处理后的素材直接覆盖资源目录中的原有 WAV，保留 GUID。素材为 24 kHz 单声道 PCM16，累计约 32 秒，导入为 Decompress On Load。运行时使用已烘焙语言素材及共用波形缓存，并只持有当前页的动态音频。[verified: render_speech.ps1；process_speech.py；SupervisorVoiceProfile.TryGetBank；SupervisorVoicePlayer.Stop]

## 验证边界

本轮结果记录于 [supervisor-voice-verification.json](supervisor-voice-verification.json)。自动检查覆盖实际场景、字长/采样时长、有限波形、跳过、分页、暂停/续播、菜单待播、下一句、缺失素材与清理，以及监督员原有逐句流程和原有音频回归。试听音色的恐怖程度仍需用户试听判断；自动检查不代表该主观目标已获认可。未进行本轮 Windows Player 构建。[opinion]

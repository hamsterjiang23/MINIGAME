# 终端开关机逐字音效

开机自检和关机的 Closing session／Session closed／System halted 文字现在逐字显现；每新增一个可见字符，独立随机选择一段上传的 UIType1–4 短音，允许连续选中同一段。空格、换行、闪烁光标和重复渲染不发声。`[verified: TerminalPowerTimeline.Boot/Shutdown；TerminalTransitionView.SetPowerFrame；MiniGameAudioService.PlayTerminalTyping]`

开机自检仍为 7.5 秒，分区加载为 1.48 秒，关机时间线为 3.885 秒。暂停时停止打字短音尾部，跳过自检直接进入加载，不排队补播未显示的文字。每天一次的整机开关机声与打字声使用独立通道；中途退出 UI 和重新进入不重复开机。`[verified: TerminalPowerTimeline 时长常量；MiniGameAudioDirector.Present；MiniGameScreen.SetTerminalPowerFrame；TerminalDailyPowerPlayModeTests]`

## 音效来源与调节

- 四段来自 GitHub 提交 `073bdb12` 的 `Art/Audio/SFX/UIType/`，均保持 WAV 原字节，并与远端 LFS 的 SHA-256／长度核对。片段长度约 41–53 毫秒，44.1kHz／16位／单声道。来源、GUID 和 Unity 副本见 [源文件清单](source-manifest.json)。`[verified: source-manifest.json sourceAssets/unityImports]`
- 在 Project 中选中 [AudioDefault.asset](../../UnityProject/Assets/MiniGame/Resources/MiniGame/AudioDefault.asset)，展开“声音条目”的“终端打字 1–4”，即可替换片段或分别调音量。默认各为 `0.18`，同时受“界面音效音量”和“总音量”控制；片段留空会被随机池排除，全部留空则静音。`[verified: AudioDefault.asset Sounds Id17–20；MiniGameAudioProfile；MiniGameAudioService.PlayTerminalTyping/Route]`
- 同步的 `OptionPopSound.wav` 保留为源资料，未放入逐字随机池；原整机关机音文件没有在此次远端提交中被替换。`[verified: source-manifest.json optionPopSound/introducedBy；git show 073bdb12 --stat]`

## 验证与运行版

锁定恢复、Release 构建、.NET 测试与隔离 Unity 的相关 PlayMode 检查记录见 [verification.json](verification.json)。该记录区分测试、实际 Player 音效事件与文件哈希，不等同于对所有硬件听感的结论。`[verified: verification.json]` `[opinion]`

完整运行包路径、SHA-256 与本轮 Player 检查结果也保存在验证记录中；解压完整文件夹后运行 MiniGame.exe。`[verified: verification.json package]`

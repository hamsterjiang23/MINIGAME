# 音频配置

入口：Unity 顶部 **MiniGame → 音频设置**。Combined 已使用默认配置，直接在此面板换音频和调音量。`[verified: MiniGameAudioSettingsWindow.cs L168；MainRoomCombined.unity 的 MiniGameAudioDirector.profile]`

## 策划操作

1. 点击 **选择音频文件夹**，选本机素材目录；需要时勾选 **包含子文件夹**。选目录只列出文件，不会批量导入。`[verified: MiniGameAudioSettingsWindow.DrawSourceFolder、MiniGameAudioFileBrowser.Select]`
2. 在对应用途下的 **文件夹音频** 下拉框选文件，也可以用 **选文件** 按钮，或将 Project 内的 AudioClip 拖到 **音频片段**。选中的外部文件会复制进工程；运行和打包不依赖原电脑路径。`[verified: MiniGameAudioSettingsWindow.DrawSounds、AssignCandidate；MiniGameAudioSetup.ImportSelectedFile]`
3. 用 **试听／停止** 检查素材，再调整总音量、音乐、环境、场景音效、界面音效及各片段音量。编辑器试听是素材原声，游戏混音请在 Play Mode 中听。`[verified: MiniGameAudioProfileEditor.OnInspectorGUI；MiniGameAudioFileBrowser.Play]`
4. 点击 **保存音频配置**。支持 Undo／Redo；在 Play Mode 中保存的配置资产退出播放后仍保留。取消某用途的声音，可清空其 AudioClip。`[verified: MiniGameAudioSettingsWindow.cs L173–190；MiniGameAudioProfileEditor.OnInspectorGUI；MiniGameAudioService.SetLoop]`

支持 WAV、MP3、OGG、AIF、AIFF。外部素材按内容哈希复用，不覆盖另一份不同内容的音频；撤销恢复映射，已导入的文件仍保留在工程中。`[verified: MiniGameAudioSetup.IsSupportedAudioFile、ImportSelectedFile；MiniGameAudioSettingsWindow.AssignCandidate]`

## 当前触发映射

- 房间音乐：`BGMGameplay.wav`，房间与终端期间循环，梦境和结局期间淡出。电脑打开时，单独叠加 `PCNoise.wav`。`[verified: MiniGameAudioDirector.SynchronizeLoops]`
- 梦境音乐：按实际 Day 0／1／2 对应 `Dream0.wav`／`Dream1.wav`／`Dream2.wav`；梦境展示期间单独循环 `Projector.wav`，离开梦境时淡出。`[verified: MiniGameAudioDirector.SynchronizeLoops、DreamSound]`
- 扫描：`Scan.wav`，进入实时审查或打开检查时播放。亮灯：`LightBeep.wav`，终端进入可处置状态、现有指示灯亮起时播放。`[verified: MiniGameAudioDirector.Present；TerminalView.Render]`
- 场景交互：`3DSceneInteraction.wav`，有效门、床、电脑、海报操作；超距、拒绝和悬停不触发。`[verified: ApartmentPlayer.InteractFocused、InteractSecondaryFocused、CompleteWorldInteraction]`
- 界面交互：`UIInteraction.wav`，有效鼠标点击和键盘确认。对白：`DialoguePop.wav`，新记录的非玩家对白出现时播放，重绘、打字和读档不重播已有记录。`[verified: MiniGameAudioButton；MiniGameAudioDirector.Present、Bind]`
- 主菜单：`StartMenu.wav` 已配置，保留 `PlayMenuMusic()` 入口；当前 Combined 直接进入房间，没有新增主菜单。梦境 3 和结局未提供音乐，保持空缺。`[verified: MiniGameAudioDirector.PlayMenuMusic、DreamSound、SynchronizeLoops；MiniGameAudioProfile.MiniGameSound]`

暂停菜单保持背景音播放；暂停时不触发新的世界交互或对白音效，菜单按钮仍可发声。`[verified: MiniGameAudioDirector.Present、PlayWorldInteraction、PlayDialoguePop、PlayUIInteraction]`

## 与《寻找伪人》的对应

- 原作 `SoundsList`／`SoundListElement` 配置用途到音频的映射，本工程对应 `MiniGameAudioProfile`。`[verified: research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Scripts/Services/Sound/SoundsList.cs、SoundListElement.cs；MiniGameAudioProfile.GetEntry]`
- 原作 `ESoundSource`／`SoundServiceInstance` 将固定音源通道交给统一服务，通过 Play、PlayOneShot、停止及淡入淡出控制；本工程对应 `MiniGameAudioChannel`／`MiniGameAudioService`，使用原生 Unity 帧更新，逐通道取消旧淡变。`[verified: 原作 SoundServiceInstance.Play、PlayOneShot；Service/SoundService.FadeIn、FadeOut；MiniGameAudioService.SetLoop、PlayOneShot、Fade]`
- 原作音乐与噪声独立，梦境音乐进入／退出时淡变，音量由 AudioMixer 控制；本工程保留这些行为，按当前房间事件接入。`[verified: 原作 NotAHumanSoundService.cs L73–86；DreamController.cs L77–96；SoundSettings.cs SetVolume；MiniGameAudioDirector.SynchronizeLoops；MiniGameAudioService.ApplyVolume]`

没有移植原作资产或逐字复制其实现。素材来源、署名与本地文件哈希见 [音频来源说明](../Art/Audio/README.md) 和 [来源清单](../Art/Audio/source-manifest.json)；验证范围见 [验证记录](audio/verification.json)。`[verified: Art/Audio/source-manifest.json evidenceBoundaries、repositoryAcquisition；本轮新增实现与默认音频文件]`

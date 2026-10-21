# Deviations

- GitHub 音效提交只新增四段 UIType 与 OptionPopSound，没有替换整机关机音文件；本轮将四段短音接入开机与关机文字，保留每日一次整机电源声，OptionPopSound 作为源资料同步。`[verified: git diff HEAD..origin/main 与提交073bdb12文件列表]`
- 只提取新增音频路径；远端同步存在场景和灯光改动，继续保留当前工作区内容。`[verified: git diff HEAD..origin/main；本轮 git restore 限定 Art/Audio/SFX 路径]`

# Discovered edge cases

- 旧电源控制台按整串突现；逐字渐显在原时间段内进行，维持原开关机总时长与分区加载节奏。低帧率下一帧可能出现多个字符，每个可见字符仍各触发一次；不缓存落后声音。`[verified: TerminalPowerTimeline；MiniGameScreen.SetTerminalPowerFrame]`
- 声音增量读取不含闪烁光标的原始文字，过滤空格／换行；重复渲染、Safe／Collapse 文字移位、跳过自检均不补播。`[verified: TerminalTransitionView.SetPowerFrame]`
- 独立随机源避免扰动场景的 Unity Random；独立打字通道避免打断电源声，暂停丢弃短音尾部。`[verified: MiniGameAudioService.PlayTerminalTyping；MiniGameAudioDirector.Present]`
- 首次 Unity 回归中，6 项新音效检查和其余相关项通过；人物监控检查仍写死旧 27 个立绘，当前内容包声明 22 个。改为逐项核对当前声明 ID 集合，同时保留每个立绘的真实 GPU 可见性和遮挡阈值断言。`[verified: Artifacts/TerminalTypingAudio/playmode.xml EveryCampaignPortraitRemainsVisibleInItsOwnDynamicRoom；campaign.minigame.json portraits；AmberTerminalPlayModeTests]`

# Questions for review

- 本轮只修改终端电源文字，不将病人对白与应答选项加入逐字音效。`[verified: 本轮用户“开机…关机…跳出一个字母”]`

变更偏差：2 项（将控制台整串突现改为原时段内逐字显现；修正监控测试的旧固定立绘总数）。`[verified: 本文 Discovered edge cases]`
最可能回看：四段打字音默认0.18的相对音量，可在中文音频Inspector调整。`[opinion]`
发现边界：光标、空白、重复帧、Safe移位、暂停尾音、跳过自检和真实内容包的立绘集合。`[verified: 本文 Discovered edge cases]`
最终验证：332项.NET、23项相关PlayMode、Windows Mono构建与原生Player通过；实测开机322次、关机47次逐字音，四段均使用，额外误触发0次。`[verified: verification.json dotnet/unity/nativePlayer]`
后续先读：guide.md、source-manifest.json、verification.json，再看TerminalTransitionView和MiniGameAudioService。`[opinion]`

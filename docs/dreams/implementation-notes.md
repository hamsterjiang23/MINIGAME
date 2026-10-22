# Deviations

- 本轮每日梦境按新流程改为黑屏音乐后逐张显示静止原画；旧的插画运动、叠化和屏幕后处理不再进入每日播放。`MotionIntensity`、`PostIntensity`、`CrossfadeSeconds` 留作资产兼容字段，面板隐藏；旧单图 Render 重载保留，不能把它的效果描述成每日序列行为。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceProfile.cs L10-L17；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L65-L93]`
- 本轮保留 Day0 一次睡眠和 Day3 高反叛 SelfReview 的既有分支；每日梦境、第二次床交互和原画显示的调整不扩大海滩制作范围。`[verified: src/MiniGame.Presentation/GameRuntime.cs L305-L314；src/MiniGame.Core/Runtime/GameSession.cs L181-L196]` `[opinion]`
- 现有序列与单图视频保留历史身份，本轮没有把它们重录为新流程。当前验收必须使用本轮证据，不能沿用旧录像证明 5 秒序列或两次床交互。`[opinion]`

# Discovered edge cases

- **床聚光阶段被核心自动推进跳过。** `_dreamFromBed` 原先在 Dream 完成进入 AfterDream 后立即执行第二次 `Interact(Bed)`。现在只允许 Day0 首次 Sleep 自动推进；Day1–3 清除首床标记并等待第二次床交互，第二次 Sleep cue 完成才执行唤醒；普通最终夜仍排 Ending cue。`[verified: src/MiniGame.Presentation/GameRuntime.cs L157-L166,L295-L314,L358-L360]`
- **完整昼夜配置覆盖床灯。** `ApplyFullRoom` 原先把 AfterDream 当普通夜景，夜景配置关闭床灯时连光束也不可见。现在 Day>0 AfterDream 和显式 `SetDreamBedReveal(true)` 使用床专属照明：灯强度取 `spotIntensity`，其余灯、环境和自发光继续黑；Evening 仍用普通夜景。`[verified: UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs L161-L189,L413-L456,L458-L487]`
- **暂停恢复不能等待渐亮。** 读档先绑定 Dream 会得到零光束，而暂停禁止时钟推进；Returning 恢复因此必须立即设置 Isolation 与 BeamStrength，并同步光束。临时 reveal 在 Bind 清除，AfterDream 从权威阶段恢复；Day/Night 预览覆盖运行时效果，回 Automatic 后重新派生。`[verified: UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs L97-L105,L178-L197；UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L247-L269]`
- **镜头返回与核心阶段切换之间存在瞬态交接。** 原画结束先点亮床，镜头返回期间核心仍是 Dream；正常返回结束时保留 reveal，完成 Dream cue 让 AfterDream 接管后才清除临时值。取消、重绑或停止预览则直接清理临时状态，避免床灯残留到下一次播放。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L172-L190,L279-L314]`
- **第一人称角度需同步到返回朝向。** 返回不仅恢复相机位置和 FOV，还朝向床；释放导演的相机占用后再调用 `FacePoint` 同步玩家视角，避免下一帧鼠标视角覆盖刚完成的返回姿态。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L233-L237,L290-L297]`
- **黑屏定位不能额外叠加到 5 秒之前。** `Aligning` 负责黑屏音乐计时，定位插值限于 `min(AlignSeconds, BlackScreenSeconds)`；黑屏计时结束才亮出第一张，逐图播放固定观影姿态。音乐覆盖黑屏和原画播放，返回时停止。`[verified: UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs L39,L130-L135,L154-L179,L222-L231]`
- **闭眼最后一帧与起床黑屏必须分开。** 两次 Sleep 都先进行 0.8 秒闭眼；只有进入时已是 AfterDream 的 Sleep 再保持 `WakeBlackSeconds`。最终全黑帧已 yield，暂停后不消耗保持时间，旧 token 不能完成新过渡。Sleep cue 恢复会重新演出这段过渡，不承诺恢复到闭眼的同一帧。`[verified: UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs L499-L528；src/MiniGame.Presentation/GameRuntime.cs L282-L285,L750-L760]`
- **第二次 Sleep 需要合法检查点。** AfterDream+Sleep 现允许所有开启梦境流程的日期；首床 `DreamFromBed` 标志不能复用于该阶段。测试覆盖 Day1–3 首床、梦境、等待第二床及起床阶段的暂停、恢复、重复 E 和过期 cue，Day0 单次睡眠单独保留。`[verified: src/MiniGame.Presentation/GameRuntime.cs L590-L594,L662；src/MiniGame.Presentation.Tests/DreamFlowRuntimeTests.cs L35-L126,L196-L208]`

# Questions for review

- 本轮 Unity 原画渲染、完整流程、暂停恢复和床灯回归仍在验证中；最终结果待写入 `docs/dreams/night-flow-2026-10-22/verification.json`。当前不宣称新的 Windows 构建、新录像或人工视觉／听感验收完成。`[opinion]`
- 节奏和音乐平衡需按 [guide.md](guide.md) 的两次床交互流程实测；当前参数开放调节，人工视觉／听感结论待确认。`[opinion]`

# Historical implementation notes

- 旧版本的 .NET 293 项、去重 PlayMode 49 项及 Windows 构建／启动报告属于 2026-10-19 的 [verification.json](verification.json)，不是本轮回归结果；旧素材和录制来源仍可由 [guide.md](guide.md) 访问。`[verified: verification.json L3,L24,L71]`
- 旧制作选择：Combined 梦境采用插画平面和独立 RenderTexture 相机，而非三套完整建模场景；Blender 提供可编辑海滩地形。旧影片使用固定步长生产相机与后期混合配置音频，不能解释为实时性能或独立听感通过。此条保留为历史制作记录。`[opinion]`
- 旧接入提醒：导入墙面共享枢轴时应使用墙面 bounds 和朝内观察姿态；相机所有权应排除正常转头、参考投影和终端过渡；读取外门存档前应释放旧返回姿态。历史问题记录仍供维护时复查，不能替代本轮运行时证据。`[opinion]`
- 旧持久化提醒：反叛值属于玩家且规则身份参与存档校验；Unity JsonUtility 的默认嵌套 DTO 需要按权威阶段过滤；海滩的连续时间轴属于既有存档范围。此条保留为历史实现记录。`[opinion]`
- 旧海滩修订保留：导入地形朝向、共享海水／湿沙时间、固定相机海浪录制，以及“退出播放并打开海滩”的编辑器切换流程。相关材料仍见 [water-revision.md](water-revision.md) 与 [cubemap-guide.md](cubemap-guide.md)，本轮不重做这些内容。`[opinion]`

# Handoff

- Deviations：本轮记录 3 项制作／兼容／证据边界。`[verified: 本文件 Deviations 三条记录]`
- Most likely to revisit：逐图停留和黑屏音乐的主观节奏，当前默认分别为 0.5 秒与 5 秒。`[verified: UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset L16-L40；UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs L83-L85]` `[opinion]`
- Edge cases：本轮记录 8 项流程、灯光、暂停、交接、镜头、计时与检查点问题。`[verified: 本文件 Discovered edge cases 八条记录]`
- Validation：本轮 .NET 114+233=347 项通过、0 失败；待完成的验收见 Questions for review。`[verified: src/MiniGame.Core.Tests/TestResults/dream-night-flow_net8.0_20261022183526.trx L702；src/MiniGame.Presentation.Tests/TestResults/dream-night-flow_net8.0_20261022183533.trx L1416]`
- Read first：[guide.md](guide.md) 的当前流程与参数，再读本文件的核心推进和灯光交接记录；最后核对本轮最终验证文件的实际状态。`[opinion]`

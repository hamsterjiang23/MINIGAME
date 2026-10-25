# 结局与自我审判

2026-10-23 用户接受的修订方案是本轮实施依据。较早文档中的“反叛值阈值”“KPI三天后检查”和“床后漂流字幕”保留历史身份；本流程按下表执行。`[verified: 本轮用户 PLEASE IMPLEMENT THIS PLAN「结局与对白更新方案（修订版）」]`

| 分支 | 条件与时点 | 操作 |
| --- | --- | --- |
| 隐藏梦 | 仅 Day3 梦结束检查：薇拉、埃德温、米洛、阿德里安的历史信任合计≥10；已释放、净化、死亡或结束治疗者仍计入，排除神父、教学角色与玩家；优先于当晚其余结局。 | 梦后直接电脑绿光且无UI → 交互打开仅有玩家的目录 → 病房黄滤镜中的16句自审 → 唯一净化 → 淡黑后在全黑中播放指定音乐3秒 → 从墙面显示器用1.2秒退开转身入场 → 自由走到床看向床并用原交互键上床 → 1.5秒先原地背向床、再向后穿墙躺到枕头 → 仰看真实蓝天、随海浪轻浮10秒 → 1秒淡黑 → EndNote响起并同时回菜单。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs ResolveDreamReturnPhase；../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs BlackHoldSeconds/EntrySeconds；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs AdvanceBedEnding/ApplyPose；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs OnHiddenBed/CompleteEnding]` |
| 神父袭击 | Day2加入；释放后的自然次日开门，先于KPI失败；Day2释放对应Day3，Day3释放对应Day4。 | 开门 → 专用8句对白 → 末句黑幕 → EndNote响起并直接回菜单。`[verified: ../../Tools/DialogueCampaign/conversion-config.json priest；../../src/MiniGame.Core/Runtime/GameSession.cs AdvanceDay；../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations.priest-ending；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L145–149]` |
| KPI失败 | 任意正式日未完成配额，该晚梦境保留，下一次醒来跳到Day4。更早触发会终止流程，不能再进入Day3隐藏检查。 | 起床 → 开门 → 监督员7句结局对白 → EndNote响起并直接回菜单。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs ResolveEnding/AdvanceDay；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L145–149]` |
| 普通 | 完成三天且未命中前述分支。 | Day4起床 → 开门 → 监督员7句结局对白 → EndNote响起并直接回菜单。`[verified: ../../src/MiniGame.Presentation/GameRuntime.cs CompleteEndingLine；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L145–149]` |

历史信任使用四人各自持续保存的有符号 `Trust`：对白与处置的正负变化都计入，跨日、离院不清零；不是只累加正值，也不要求仍有留院病人。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs L193–200/L283–285/L378–407]`

对话表的 Day 按角色加入后的相对日期读取：`sourceDay = campaignDay - firstDay + 1`。乔纳在游戏Day2读取表Day1、游戏Day3读取表Day2；阿德里安在游戏Day3读取表Day1。原始节点ID保留，超出三天流程的后续对白只留在来源审计中；释放后“次日”等事件仍按游戏日期结算。`[verified: ../../Tools/DialogueCampaign/build_campaign.py L254–269；../../Tools/DialogueCampaign/conversion-config.json L40–55；../../src/MiniGame.Core/Runtime/GameSession.cs L391–395]`

自审使用常规目录、立绘、逐字显示、对话记录滚动和输入门控；不创建正式患者、会谈或KPI记录。`AdvanceSelfReviewCommand(expectedLineIndex)` 只接受当前句完成一次，16句读完才能执行 `ConfirmSelfPurification`。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs AdvanceSelfReview/SelfPurify；../../UnityProject/Assets/MiniGame/Runtime/EndingDialoguePresenter.cs Update；../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs PresentDialogueLine]`

三组结局文本来自固定版本的 `Resources/MiniGame/Endings/Dialogue.json`，源节点分别加 `priest-ending__`、`self-review__`、`supervisor-ending__` 前缀；原始ID、单元格和SHA-256见内容快照与来源清单。`[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations；../../Tools/DialogueCampaign/build_endings.py]`

普通、KPI失败和神父结局仍在最后一句完成后直接返回菜单。隐藏梦在躺下、十秒看天和一秒淡黑完成后返回菜单；完整黑幕至少显示一帧。所有分支只播放一次EndNote，与回菜单同时发生，不等待音符播完，不显示感谢页或追加确认。完成标记阻止“继续”重复播放本局，已完成存档拒绝恢复。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs Update/AdvanceBedEnding；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs CompleteEnding/TryLoadFrom；../../UnityProject/Assets/MiniGame/Runtime/MainMenuController.cs CompleteInspectionAndReturnToMenu]`

自审 SJ01／SJ02 保留原立绘，通过 `self-review` 房间配置复用普通角色的病房底图、琥珀滤镜和监控信号。Day3 返回黑幕内读取核心的 `DreamReturnPhase`，进入自审时只启用电脑引导；其他正式日梦后在全黑中自动进入次日，接床上醒来、转头和起身演出，不再第二次点击床。海滩音乐槽已绑定用户提供的《An Ending (Ascent)》；全黑中从曲首播放3秒，再从墙面显示器用1.2秒退开并转身进入梦房间，同一音源连续播放，海浪环境声保留。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientMonitorProfile.cs Defaults；../../UnityProject/Assets/MiniGame/Runtime/DreamSequenceDirector.cs CompleteIntoWake；../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs WakeTransition]`

游戏内暂停根菜单同时按住 N + O + R + M 打开 GM 结局页；正常、配额失败、神父三个选项进入相应开门结局，隐藏选项从自审第一句入口开始。选择后替换当前流程并恢复游戏，可从正式海滩返回房间切换分支；Esc 只退回暂停根菜单。日期、患者历史与已有手动存档保留，旧演出及其检查点清除；旧回调不会推进新流程。主菜单、设置、操作手册和独立海滩预览不绑定该入口；原暂停标题四击仍直接进入海滩。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs UpdateGmChord/BuildGm；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs TryEnterGmEnding；../../src/MiniGame.Presentation/GameRuntime.cs EnterGmEndingFromPause]`

编辑器导入入口是 `MiniGame/Campaign/Import endings and updated dialogue`。此入口更新活动内容包和神父／自审立绘，不重建或保存场景。旧单日演示和独立海滩预览仍保留。`[verified: ../../UnityProject/Assets/MiniGame/Editor/EndingContentImport.cs Run；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs TryLieDown]`

音乐文件原样导入为 `UnityProject/Assets/MiniGame/Audio/Music/EndingAscent.mp3`，源文件是用户上传的 `An Ending (Ascent).mp3`，SHA-256 为 `2B3405F8EB71C25EDB2961803FE51D36551CFFB877517DEA3A6755D30A1B9009`；初次进入从曲首播放，存档恢复已有播放位置。正式上床期间墙画保持固定，床的中线对齐墙面，镜头沿原床越过墙面进入真实海滩，躺稳后直接看室外天空；实现与预览入口见[半床穿墙说明](../dreams/wall-portal.md)。`[verified: 2026-10-23 Get-FileHash 源文件与工程文件相同；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs Initialize/RestoreCheckpoint/RenderRoomCubemap]`

存档另保存躺下起始姿态、看天／淡黑阶段、阶段经过时间及音乐播放位置；暂停不计入十秒，读取恢复相机与遮罩。旧 `Thanks` 检查点仍在解除暂停后直接完成，不重新演出上床。空间盒初始化失败保持黑幕，按R重试。此前验证记录保留历史身份；本轮验证记录放在 `Artifacts/HiddenEndingRevision20261023/`。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs CaptureCheckpoint/RestoreCheckpoint/ApplyPause；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs LateUpdate/ValidateVisualCheckpoint；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs Enter/RetryInitialization]`

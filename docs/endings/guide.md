# 结局与自我审判

2026-10-23 用户接受的修订方案是本轮实施依据。较早文档中的“反叛值阈值”“KPI三天后检查”和“床后漂流字幕”保留历史身份；本流程按下表执行。`[verified: 本轮用户 PLEASE IMPLEMENT THIS PLAN「结局与对白更新方案（修订版）」]`

| 分支 | 条件与时点 | 操作 |
| --- | --- | --- |
| 隐藏梦 | 仅 Day3 梦结束检查：薇拉、埃德温、米洛、阿德里安的历史信任合计≥10；已释放、净化、死亡或结束治疗者仍计入，排除神父、教学角色与玩家；优先于当晚其余结局。 | 电脑绿光且无UI → 交互打开仅有玩家的目录 → 16句自审 → 唯一净化 → 淡黑后保持全黑2秒 → 1秒淡入和相机平滑入场 → 自由走到床并交互 → EndNote响起并直接回菜单。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs L183–200；../../src/MiniGame.Core/Rules/GameRules.cs L72；../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs BlackHoldSeconds/EntrySeconds；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L136–149]` |
| 神父袭击 | Day2加入；释放后的自然次日开门，先于KPI失败；Day2释放对应Day3，Day3释放对应Day4。 | 开门 → 专用8句对白 → 末句黑幕 → EndNote响起并直接回菜单。`[verified: ../../Tools/DialogueCampaign/conversion-config.json priest；../../src/MiniGame.Core/Runtime/GameSession.cs AdvanceDay；../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations.priest-ending；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L145–149]` |
| KPI失败 | 任意正式日未完成配额，该晚梦境保留，下一次醒来跳到Day4。更早触发会终止流程，不能再进入Day3隐藏检查。 | 起床 → 开门 → 监督员7句结局对白 → EndNote响起并直接回菜单。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs ResolveEnding/AdvanceDay；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L145–149]` |
| 普通 | 完成三天且未命中前述分支。 | Day4起床 → 开门 → 监督员7句结局对白 → EndNote响起并直接回菜单。`[verified: ../../src/MiniGame.Presentation/GameRuntime.cs CompleteEndingLine；../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L145–149]` |

历史信任使用四人各自持续保存的有符号 `Trust`：对白与处置的正负变化都计入，跨日、离院不清零；不是只累加正值，也不要求仍有留院病人。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs L193–200/L283–285/L378–407]`

对话表的 Day 按角色加入后的相对日期读取：`sourceDay = campaignDay - firstDay + 1`。乔纳在游戏Day2读取表Day1、游戏Day3读取表Day2；阿德里安在游戏Day3读取表Day1。原始节点ID保留，超出三天流程的后续对白只留在来源审计中；释放后“次日”等事件仍按游戏日期结算。`[verified: ../../Tools/DialogueCampaign/build_campaign.py L254–269；../../Tools/DialogueCampaign/conversion-config.json L40–55；../../src/MiniGame.Core/Runtime/GameSession.cs L391–395]`

自审使用常规目录、立绘、逐字显示、对话记录滚动和输入门控；不创建正式患者、会谈或KPI记录。`AdvanceSelfReviewCommand(expectedLineIndex)` 只接受当前句完成一次，16句读完才能执行 `ConfirmSelfPurification`。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs AdvanceSelfReview/SelfPurify；../../UnityProject/Assets/MiniGame/Runtime/EndingDialoguePresenter.cs Update；../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs PresentDialogueLine]`

三组结局文本来自固定版本的 `Resources/MiniGame/Endings/Dialogue.json`，源节点分别加 `priest-ending__`、`self-review__`、`supervisor-ending__` 前缀；原始ID、单元格和SHA-256见内容快照与来源清单。`[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations；../../Tools/DialogueCampaign/build_endings.py]`

全部结局在最后一句完成或隐藏空间床交互后，播放一次EndNote并同时直接返回菜单；不显示感谢页，不增加结束淡出或额外确认。EndNote由菜单音频播放，菜单出现不会截断它。完成标记阻止“继续”重复播放本局，已完成存档拒绝恢复；旧存档文件保留，新内容版本或规则指纹不兼容时需开始新检查期。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/L136–149/L278–279；../../UnityProject/Assets/MiniGame/Runtime/MainMenuController.cs L181–198；../../UnityProject/Assets/MiniGame/Runtime/MainMenuContinueStore.cs ResolveResumePath；../../src/MiniGame.Presentation/GameRuntime.cs Restore]`

编辑器导入入口是 `MiniGame/Campaign/Import endings and updated dialogue`。此入口更新活动内容包和神父／自审立绘，不重建或保存场景。旧单日演示和独立海滩预览仍保留。`[verified: ../../UnityProject/Assets/MiniGame/Editor/EndingContentImport.cs Run；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs TryLieDown]`

存档覆盖等待开门、结局对白序号、自审核心进度、净化提交、全黑计时、入场镜头、空间盒及完成状态。保留旧 `Thanks` 检查点的序列化兼容；恢复后解除暂停即直接完成，不再重建感谢页。空间盒初始化失败保持黑幕，按R重试；暂停冻结转场计时。本目录的 `verification.json` 保留此前验收记录，本次修订的验证结果另见 `Artifacts/ExperienceRevision20261023/`。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs L78–82/TrySaveTo/TryLoadFrom；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs RetryInitialization；../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs Advance；../../Artifacts/ExperienceRevision20261023/core-test.log]`

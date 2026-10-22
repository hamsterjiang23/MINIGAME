# 结局与自我审判

2026-10-23 用户接受的修订方案是本轮实施依据。较早文档中的“反叛值阈值”“KPI三天后检查”和“床后漂流字幕”保留历史身份；本流程按下表执行。`[verified: 本轮用户 PLEASE IMPLEMENT THIS PLAN「结局与对白更新方案（修订版）」]`

| 分支 | 条件与时点 | 操作 |
| --- | --- | --- |
| 隐藏梦 | 仅 Day3 梦结束检查：已经入场、仍为 Active、FirstDay>0 的正式病人至少一人，信任总和≥10；优先于当晚其余结局。 | 电脑绿光且无UI → 交互打开仅有玩家的目录 → 16句自审 → 唯一净化 → 淡黑后保持全黑2秒 → 1秒淡入和相机平滑入场 → 自由走到床 → 感谢页。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs HasHiddenEndingTrust/FinishDream；../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs BlackHoldSeconds/EntrySeconds]` |
| 神父袭击 | Day2加入；释放后的自然次日开门，先于KPI失败；Day2释放对应Day3，Day3释放对应Day4。 | 开门 → 专用8句对白 → 末句黑幕 → 感谢页。`[verified: ../../Tools/DialogueCampaign/conversion-config.json priest；../../src/MiniGame.Core/Runtime/GameSession.cs AdvanceDay；../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations.priest-ending]` |
| KPI失败 | 任意正式日未完成配额，该晚梦境保留，下一次醒来跳到Day4。更早触发会终止流程，不能再进入Day3隐藏检查。 | 起床 → 开门 → 监督员7句结局对白 → 感谢页。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs ResolveEnding/AdvanceDay]` |
| 普通 | 完成三天且未命中前述分支。 | Day4起床 → 开门 → 监督员7句结局对白 → 感谢页。`[verified: ../../src/MiniGame.Presentation/GameRuntime.cs EndingStage/CompleteEndingLine]` |

自审使用常规目录、立绘、逐字显示、对话记录滚动和输入门控；不创建正式患者、会谈或KPI记录。`AdvanceSelfReviewCommand(expectedLineIndex)` 只接受当前句完成一次，16句读完才能执行 `ConfirmSelfPurification`。`[verified: ../../src/MiniGame.Core/Runtime/GameSession.cs AdvanceSelfReview/SelfPurify；../../UnityProject/Assets/MiniGame/Runtime/EndingDialoguePresenter.cs Update；../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs PresentDialogueLine]`

三组结局文本来自固定版本的 `Resources/MiniGame/Endings/Dialogue.json`，源节点分别加 `priest-ending__`、`self-review__`、`supervisor-ending__` 前缀；原始ID、单元格和SHA-256见内容快照与来源清单。`[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations；../../Tools/DialogueCampaign/build_endings.py]`

全部结局使用1.2秒结束淡出，感谢页出现后等待鼠标释放，再接受新的单击返回菜单。完成标记阻止“继续”重复播放本局，旧存档文件保留；新内容版本或规则指纹不兼容时需开始新检查期。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/EndingThanksView.cs AdvanceForTests；../../UnityProject/Assets/MiniGame/Runtime/MainMenuContinueStore.cs；../../src/MiniGame.Presentation/GameRuntime.cs Restore]`

编辑器导入入口是 `MiniGame/Campaign/Import endings and updated dialogue`。此入口更新活动内容包和神父／自审立绘，不重建或保存场景。旧单日演示和独立海滩预览仍保留。`[verified: ../../UnityProject/Assets/MiniGame/Editor/EndingContentImport.cs Run；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs TryLieDown]`

存档覆盖等待开门、结局对白序号、自审核心进度、净化提交、全黑计时、入场镜头、空间盒、感谢页及完成状态。空间盒初始化失败保持黑幕，按R重试；暂停冻结转场计时。验证结果见本目录的 `verification.json`。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameSessionHost.cs TrySaveTo/TryLoadFrom；../../UnityProject/Assets/MiniGame/Runtime/RoomDreamEnding.cs RetryInitialization；../../UnityProject/Assets/MiniGame/Runtime/RoomEndingTransitionCheckpoint.cs Advance]`

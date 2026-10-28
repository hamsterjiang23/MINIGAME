# MINIGAME 协作规则

适用于整个仓库。当前入口见 [README](README.md)，规则／宿主边界见 [核心说明](src/README.md)，Unity操作见 [工程说明](UnityProject/README.md)。[opinion]

## 当前基线与范围

- 默认开发对象是 `MainRoomCombined` 正式Campaign：Day0教学、三天工作、五位正式患者、四分支结局；Day4只作结局收束。旧 `PrototypeRules`、小孩Day1演示和美术预览独立保留，不用其配置覆盖正式流程。[verified: UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json campaign.days；src/README.md「开发默认规则」；docs/endings/guide.md「分支」] [opinion]
- Day1薇拉／医生／小孩，Day2加入神父，Day3加入外交官；完整候选会谈含教学共13场，退场与提前结局减少实际场次。数量以活动包和来源审计为准，不沿用旧16场统计。[verified: UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json campaign.days；provenance.json meetingCount] [opinion]
- 原创工程保持Unity 2022.3系列；当前工程文件记录2022.3.23f1，历史隔离测试使用2022.3.23f1c1。运行前核对实际Editor、工程版本与启动器参数，不擅自升级、降级或覆盖本地版本改动。[verified: UnityProject/ProjectSettings/ProjectVersion.txt m_EditorVersion；UnityProject/Tools/Open-Project.ps1 L1–21；docs/ui-feedback/ending-polish-2026-10-26.md「验证」] [opinion]
- 先检查Git状态，保留他人的场景、ProjectSettings、资源和研究工具改动；不要为整理文档或导入对白重建主场景。[opinion]
- 《寻找伪人》是玩法参考，优先读 [玩法研究](research/no-im-not-a-human/Reports/overview.md)；随机事件、物资、额外工作日与真假／善恶标准答案不因参考项目存在而自动进入范围。修改需求前核对当前实现及 [待定项](docs/open-questions.md)。[opinion]

## 核心与适配器

- `GameSession`是规则与状态权威；`GameRuntime`负责房间／终端生命周期、输入、演出与存档检查点；Unity／Yarn负责呈现和回调。不得在UI、Yarn变量或MonoBehaviour中另存权威信任、配额、治疗次数或人物去留。[verified: src/MiniGame.Presentation/GameRuntime.cs L15–45；src/README.md「Unity／Yarn接入」] [opinion]
- 宿主提交命令、读取玩家视图、消费事件并通知演出完成，不用存档DTO修改活动会话。处置只结算一次；旧节点、旧会面及过期演出回调不得重复执行副作用。[verified: src/README.md「调用入口」「保存与恢复」] [opinion]
- 读档同时考虑核心重放、规则／内容版本、会谈检查点、演出阶段和回调失效，不只恢复镜头而跳过状态校验。[verified: src/README.md「保存与恢复」「Unity／Yarn接入」] [opinion]
- 信任为各患者独立、有符号的累计值，仅包含对白变化，退场不清零；处置增减只用于当前回应期间的社会适应度情绪表现，不写入长期信任、不影响次日或任何条件判断。该表现由核心按已接受处置派生，读档可重放；不擅自钳制信任为0–100，不与疯狂／偏离／狂热／反叛等数值合并。[verified: src/MiniGame.Core/Runtime/GameSession.cs SocialAdaptationTrust/Select/Dispose/HasHiddenEndingTrust] [opinion]

## 当前结局契约

- 隐藏条件在Day3梦结束时检查：薇拉、医生、小孩、外交官的历史信任合计≥10，包含已释放／净化／死亡／治疗结束者，排除神父、教学角色和玩家；反叛值不再决定门槛。[verified: src/MiniGame.Core/Runtime/GameSession.cs L183–206]
- 隐藏条件成立后进入电脑自审，16句完成才允许自我净化，之后接海滩；不创建正式患者、会谈或KPI记录。[verified: docs/endings/guide.md 分支表及「自审使用常规目录」] [opinion]
- 神父释放按自然次日触发袭击，在同次跨日先于KPI失败。任意正式日未达配额，保留当晚梦境，下一次跨日转到Day4锁房；更早结束的局不能进入Day3隐藏检查。隐藏梦先在Day3梦结束时判定，其余分支随后按跨日规则处理，不用一条全局优先级掩盖时点差异。[verified: src/MiniGame.Core/Runtime/GameSession.cs L183–206、L255–263、L422–426、L496–528] [opinion]
- 演出细节以 [结局指南](docs/endings/guide.md)、[2026-10-26修订](docs/ui-feedback/ending-polish-2026-10-26.md)核对当前代码；不要恢复旧反叛门槛、三天后才检查KPI、感谢页或旧版上床时长。[opinion]

## 内容配表与导入

- 日常对白、信任增量、立绘与条件来自 [Art/chat_excel](Art/chat_excel/)；文件由 [conversion-config.json](Tools/DialogueCampaign/conversion-config.json) 的 `patients[*].file` 绑定。初始信任0、默认释放3、每日净化目标1在配置中维护，单场显式门槛覆盖默认。[verified: Tools/DialogueCampaign/conversion-config.json initialTrust/releaseThreshold/dailyQuota/patients；Tools/DialogueCampaign/README.md「全流程对白转换」]
- 角色表Day为加入后的相对会谈日，保持 `sourceDay = campaignDay - firstDay + 1` 及原节点ID。神父游戏Day2读表Day1，外交官游戏Day3读表Day1，不把后续表日强塞入三天流程。[verified: docs/endings/guide.md「对话表的 Day」] [opinion]
- 内容修改遵循“Excel／明确适配规则 → 生成包与来源审计 → 校验 → Unity导入 → 活动绑定检查”。`Content/Campaign`、ImportedDialogue、运行时Yarn为生成产物，不用手改副本替代源配置。[verified: Tools/DialogueCampaign/build_campaign.py main；UnityProject/Assets/MiniGame/Editor/EndingContentImport.cs L13–29] [opinion]
- [source-repairs.json](Tools/DialogueCampaign/source-repairs.json) 锁定源哈希与修复前置条件；源表变化后重新复核修复、排除行和测试预期，不只改哈希绕过检查，不自行纠正异常幅度数值。[verified: Tools/DialogueCampaign/updated_source.py L57–100] [opinion]
- 先运行 `build_campaign.py --smoke`，生成后执行 `--check`、监督员／结局检查与对白工具测试，命令见 [工具说明](Tools/DialogueCampaign/README.md)。既有内容更新用 `MiniGame/Campaign/Import endings and updated dialogue`；此入口不重建场景。[verified: Tools/DialogueCampaign/README.md 命令块；UnityProject/Assets/MiniGame/Editor/EndingContentImport.cs L13–29] [opinion]
- 游戏从 `Resources/MiniGame/ActiveCampaignPackage` 加载，资产位于 `UnityProject/Assets/Resources/MiniGame/ActiveCampaignPackage.asset`。核对其实际PackageSource，不能只检查生成目录。[verified: UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L503–508；ActiveCampaignPackage.asset packageSource] [opinion]
- 数值试算明确随机策略、合法处置、出场／退场、提前结局与求和时点，区分条件期望和全部开局样本。临时参数不代表Excel、活动包或游戏已经修改。[opinion]

## 证据与验证

- 结论使用 `[verified: 文件行号、JSON键或具体报告段落]` 或 `[opinion]`；区分源码实现、内容检查、历史测试、当前执行、Editor导入、PlayMode、Player启动、完整游玩与人工视觉验收。[opinion]
- 旧待定项、路线和验收可能落后于已实现的Campaign；先核对源码、活动包及较新专题记录，再判断是否缺功能。不要把旧三结局、旧门槛或历史测试数量写成当前状态。[opinion]
- 纯C#固定SDK **8.0.424**、C#9、可空检查和锁定依赖。核心／宿主行为变化，在 `src/` 依次执行 `dotnet restore MiniGame.sln --locked-mode`、`dotnet build MiniGame.sln -c Release --no-restore`、`dotnet test MiniGame.sln -c Release --no-build`；优先覆盖处置去重、跨日、结局冲突与保存恢复。[verified: src/global.json sdk.version；src/Directory.Build.props LangVersion/Nullable/RestorePackagesWithLockFile] [opinion]
- Unity表现变化按影响范围执行EditMode／PlayMode与必要构建；异步测试等到最终报告，不能把启动任务当成通过。文档变化检查本地链接、来源一致性与 `git diff --check`。[opinion]
- 历史报告引用的Artifacts、XML或程序不存在时，标明仅有文档记录，不宣称已读原始日志或当前版本已通过。不要把2026-10-23的ZIP当成10月26日修改后的交付。[opinion]
- 外发走 `MiniGame/Build/Build Windows external release`；按 [外发包指南](docs/windows-release.md)核对源码／内容版本、完整目录、构建报告、原生分辨率启动与解压后运行。Mono通过不代表IL2CPP通过。[verified: docs/windows-release.md「Windows 外发包」] [opinion]

## 研究与版本

- `src/`保存规则／宿主／测试，`UnityProject/`保存Unity适配，`Art/`保存美术和内容源，`Tools/`保存转换工具，`docs/`保存设计与验收，`research/`、`Dependency/`和`references/`保存研究与参考。[verified: README.md「框架与配表」「资料、研究与同步」]
- 完整参考游戏研究按现有私有Git／Git LFS规则管理；不修改Steam原游戏，不绕过DRM，不公开分发未经授权的原游戏代码和资产，不擅自改变仓库可见性。[opinion]
- 二进制及达到1MiB的其他材料使用LFS，小文本通常普通Git，以根 `.gitattributes` 为准；保留来源路径和哈希，搬迁建立新旧映射。[opinion]
- `references/`遵循 [source-lock](docs/source-lock.md) 固定提交，不运行 `git submodule update --remote`；复用前核对代码、依赖与资产许可。[verified: docs/source-lock.md「获取与使用说明」] [opinion]

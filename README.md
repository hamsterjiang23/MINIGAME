# NORM / MINIGAME

原创第一人称、2D/3D混合心理恐怖叙事游戏。玩家在员工公寓中作为检定员，通过电脑与患者交流，选择治疗、净化或释放，并承担跨日后果。当前正式流程为 **Day0教学＋Day1–3工作＋四分支结局**；Day4用于结局收束。[verified: UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json campaign.days；src/MiniGame.Core/Runtime/GameSession.cs L193–206、L255–263]

当前处于全流程内容与演出已接入、持续回归和体验打磨的阶段，最终发布验收仍需单独完成。[opinion] 本页于2026-10-27按当前源码、内容包及仓库内报告整理；历史验证不表示本次重新执行。[opinion]

## 启动与开发

- 主场景：[MainRoomCombined.unity](UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity)。Windows入口为 [Open-Unity.cmd](Open-Unity.cmd)，其调用的 [Open-Project.ps1](UnityProject/Tools/Open-Project.ps1) 校验Editor版本并打开融合场景。[verified: UnityProject/Tools/Open-Project.ps1 L1–29]
- 当前工程记录 **Unity 2022.3.23f1**；历史隔离测试使用 **2022.3.23f1c1**。运行前核对实际Editor与工程版本，不静默升级或覆盖已有版本改动。[verified: UnityProject/ProjectSettings/ProjectVersion.txt m_EditorVersion；docs/ui-feedback/ending-polish-2026-10-26.md「验证」] [opinion]
- 已打开Editor时使用 `MiniGame/Main Room/Open and play`。移动、会谈、存读档、暂停和旧演示的操作见 [Unity工程说明](UnityProject/README.md)。[verified: UnityProject/README.md「运行与操作」]
- 外发菜单：`MiniGame/Build/Build Windows external release`，目标为Windows x64、Mono、非Development；默认输出 `Builds/NORM/NORM.exe`。配置和交付检查见 [外发包指南](docs/windows-release.md)。[verified: docs/windows-release.md「Windows 外发包」中 Build/输出说明]

## 当前流程与完成范围

**晨检与简报 → 电脑名册与会谈 → 处置与反应 → 夜间梦境 → 次日或结局。** Day0使用独立教学角色，正式患者退场后不再进入后续常规队列。[verified: UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json campaign.days；src/MiniGame.Core/Runtime/GameSession.cs L417–438、L460–464]

| 游戏日 | 候选角色 | 来源 |
| --- | --- | --- |
| Day0 | 无名男子（教学） | [verified: campaign.minigame.json campaign.days[0].meetings] |
| Day1 | 薇拉、埃德温、米洛 | [verified: campaign.minigame.json campaign.days[1].meetings] |
| Day2 | 薇拉、埃德温、米洛、乔纳 | [verified: campaign.minigame.json campaign.days[2].meetings] |
| Day3 | 薇拉、埃德温、米洛、乔纳、阿德里安 | [verified: campaign.minigame.json campaign.days[3].meetings] |

完整候选会谈含教学共 **13场**；实际场数受处置退场与提前结局影响。当前来源审计为1360行原始资料、1294行启用资料、66行排除资料；这些是来源数量，不等于玩家实际看到的台词数量。[verified: UnityProject/Assets/MiniGame/Content/Campaign/provenance.json sourceRowCount/activeSourceRowCount/excludedSourceRowCount/meetingCount/meetingSchedule]

- **规则与存档**：患者独立信任、条件会谈、一次性处置、每日配额、跨日事件、结局、核心和演出检查点已有实现。[verified: src/README.md「调用入口」「保存与恢复」；src/MiniGame.Presentation/GameRuntime.cs L15–45]
- **房间与终端**：融合公寓、门／电脑／床交互、CRT终端、人物监控、逐字对白、阅读物件、暂停及存取档已有接入；详细操作见 [Unity工程说明](UnityProject/README.md)、[世界交互](docs/world-dialogue/guide.md)与[终端指南](docs/terminal-amber/guide.md)。[verified: UnityProject/README.md「运行与操作」]
- **对白与立绘**：正式Campaign使用Excel生成的V2包，按信任门槛进入特殊段；旧小孩Day1演示与 `PrototypeRules` 独立保留。[verified: src/README.md「开发默认规则」；Tools/DialogueCampaign/README.md「全流程对白转换」]
- **梦境与终局**：每日梦境、自审、海滩行走、上床看天、监督员对白、名单与回菜单已有接入。近期镜头、声音和准心修订见 [隐藏结局打磨](docs/ui-feedback/ending-polish-2026-10-26.md)。[verified: docs/endings/guide.md「分支」；docs/ui-feedback/ending-polish-2026-10-26.md「修改」「最终补修：海滩准心」]

### 四分支结局

| 分支 | 触发口径 | 来源 |
| --- | --- | --- |
| 隐藏梦／自我审判 | Day3梦结束，薇拉、医生、小孩、外交官历史信任合计≥10；退场者仍计入，排除神父、教学角色及玩家；16句自审与自我净化后进入海滩。 | [verified: src/MiniGame.Core/Runtime/GameSession.cs L193–206；docs/endings/guide.md「分支」] |
| 神父袭击 | 神父释放后的自然次日触发，先于同次跨日的KPI失败；Day2释放对应Day3，Day3释放对应Day4。 | [verified: src/MiniGame.Core/Runtime/GameSession.cs L255–263、L422–426；docs/endings/guide.md「分支」] |
| KPI失败／锁房 | 任意正式日未达配额，保留当晚梦境，在下一次跨日转到Day4结局；更早结束的局不能进入Day3隐藏检查。 | [verified: src/MiniGame.Core/Runtime/GameSession.cs L255–263、L496–528] |
| 普通／被带走 | 完成三天且未进入上述分支。 | [verified: src/MiniGame.Core/Runtime/GameSession.cs L255–263；docs/endings/guide.md「分支」] |

隐藏条件先在Day3梦结束时检查，其余分支在跨日结算。**反叛值仍保留，但不再决定正式隐藏结局门槛。** 演出和GM测试入口见 [结局指南](docs/endings/guide.md)。[verified: src/MiniGame.Core/Runtime/GameSession.cs L183–206、L255–263]

## 框架与配表

`GameSession`裁决权威规则和状态；`GameRuntime`管理房间／终端生命周期、输入和演出；Unity／Yarn负责场景、文本及播放回调。UI与Yarn不另存权威信任、配额或患者去留。[verified: src/MiniGame.Presentation/GameRuntime.cs L15–45；src/README.md「Unity／Yarn接入」] [opinion]

| 修改内容 | 入口 | 来源 |
| --- | --- | --- |
| 规则、状态、去重、跨日与存档 | [MiniGame.Core](src/MiniGame.Core/) | [verified: src/README.md「调用入口」「保存与恢复」] |
| 输入、演出检查点、内容编译与运行时 | [MiniGame.Presentation](src/MiniGame.Presentation/) | [verified: src/MiniGame.Presentation/GameRuntime.cs L15–45] |
| Unity表现、场景、Editor工具与测试 | [Assets/MiniGame](UnityProject/Assets/MiniGame/) | [verified: UnityProject/README.md「运行与操作」] |
| 日常对白、信任增量、立绘与条件 | [Art/chat_excel](Art/chat_excel/) 中六份角色／教学Excel；监督员和神父结局另有工作簿 | [verified: Tools/DialogueCampaign/conversion-config.json patients[*].file；Tools/DialogueCampaign/README.md 开篇] |
| 初始信任、默认释放门槛、每日配额、加入日 | [conversion-config.json](Tools/DialogueCampaign/conversion-config.json) | [verified: conversion-config.json initialTrust/releaseThreshold/dailyQuota/patients[*].firstDay] |
| 来源修复、冲突行排除与哈希约束 | [source-repairs.json](Tools/DialogueCampaign/source-repairs.json) | [verified: Tools/DialogueCampaign/updated_source.py L57–100] |
| 生成包、Yarn与逐行来源审计 | [Content/Campaign](UnityProject/Assets/MiniGame/Content/Campaign/) | [verified: Tools/DialogueCampaign/build_campaign.py main 的 artifacts/output] |
| 实际加载的内容绑定 | [ActiveCampaignPackage.asset](UnityProject/Assets/Resources/MiniGame/ActiveCampaignPackage.asset) | [verified: MiniGameBootstrap.cs L503–508；ActiveCampaignPackage.asset packageSource] |

当前默认初始信任0、释放门槛3、每日净化目标1；显式配置的特殊段≥3、释放≥5覆盖默认值。角色表的Day是加入后的相对会谈日：神父游戏Day2读表Day1，外交官游戏Day3读表Day1。[verified: Tools/DialogueCampaign/conversion-config.json initialTrust/releaseThreshold/dailyQuota/patients；Tools/DialogueCampaign/README.md「全流程对白转换」；docs/endings/guide.md「sourceDay = campaignDay - firstDay + 1」]

从Excel或明确的转换规则修改内容，重建和校验后再导入Unity；不把手改生成JSON／ImportedDialogue副本当作长期配表。源哈希变化需复核修复清单，操作详见 [对白转换工具](Tools/DialogueCampaign/README.md)。[verified: Tools/DialogueCampaign/updated_source.py L57–68；Tools/DialogueCampaign/build_campaign.py main] [opinion]

```powershell
uv run python Tools/DialogueCampaign/build_campaign.py --smoke
uv run python Tools/DialogueCampaign/build_campaign.py
uv run python Tools/DialogueCampaign/build_campaign.py --check
uv run python Tools/DialogueCampaign/build_supervisor.py --check
uv run python Tools/DialogueCampaign/build_endings.py --check
uv run python -m unittest discover -s Tools/DialogueCampaign -p 'test_*.py'
```

上述命令来自工具说明；生成后用 `MiniGame/Campaign/Import endings and updated dialogue` 更新活动内容、结局及相关立绘，该入口不重建场景。[verified: Tools/DialogueCampaign/README.md 命令块；UnityProject/Assets/MiniGame/Editor/EndingContentImport.cs L13–29]

## 验证状态与剩余验收

- **历史核心／内容检查**：[结局内容检查](docs/ending-content/checks.json)记录26项Python测试和四条命令路径探针，明确不含Unity验证；其residentTrustSum是当时的留院患者统计，不替代当前四人历史信任条件。[verified: docs/ending-content/checks.json python/commandRouteProbe/unity；src/MiniGame.Core/Runtime/GameSession.cs L199–206]
- **近期Unity历史记录**：[2026-10-26结局修订](docs/ui-feedback/ending-polish-2026-10-26.md)记载25项相关PlayMode通过，准心补修记载7项通过。本次整理时，文中两个本地XML及准心修复版程序路径不存在；保留文档记录身份，不声称已重新核验原始产物。[verified: docs/ui-feedback/ending-polish-2026-10-26.md「验证」「最终补修：海滩准心」；2026-10-27 Test-Path 对 Artifacts/EndingPolish20261026/playmode.xml、Artifacts/BeachReticle20261026/playmode.xml、Artifacts/BeachReticle20261026/Windows/NORM.exe 返回False]
- **本地历史外发包**：2026-10-23神父更新ZIP仍在 `Builds/Deliverables/`；不能据此确认它包含10月26日的后续改动，外发应按当前源码重建并记录版本。[verified: 2026-10-27 Test-Path 对 Builds/Deliverables/NORM-Windows-x64-Mono-PriestUpdate-20261023-201409.zip 返回True] [opinion]
- **待最终验收**：当前版本完整图形交互和四分支回归、暂停／存读档、声音与镜头人工体验、最终ZIP解压运行及性能验收，应对应同一版源码和构建；IL2CPP状态需独立证据。[opinion]

纯C#固定SDK **8.0.424**、C#9、可空检查和锁定依赖；行为修改后在 `src/` 执行锁定恢复、Release构建和测试。文档修改检查链接与Git差异，不代替游戏测试。[verified: src/global.json sdk.version；src/Directory.Build.props LangVersion/Nullable/RestorePackagesWithLockFile] [opinion]

## 资料、研究与同步

- 开发入口：[核心说明](src/README.md)、[Unity工程说明](UnityProject/README.md)、[全流程指南](docs/campaign-guide.md)、[结局指南](docs/endings/guide.md)、[协作规则](AGENTS.md)。[opinion]
- [设计总览](docs/game-overview.md)、[待定项](docs/open-questions.md)和[早期项目状态](docs/project-status.md)保留来源与历史判断；遇到旧三结局、反叛阈值、旧会谈数量时，核对当前代码和活动内容包。[opinion]
- 《寻找伪人》是玩法参考，研究入口为[逆向总报告](research/no-im-not-a-human/Reports/overview.md)，参考版本见[source-lock](docs/source-lock.md)。借鉴观察、对话和决策后果，不自动扩展成随机事件、物资或真假标准答案系统。[opinion]
- `research/`、`Dependency/`和`references/`保存研究及固定版本参考；二进制与大文件按根 [.gitattributes](.gitattributes) 使用Git LFS，参考子模块不使用 `git submodule update --remote`。[verified: docs/source-lock.md「获取与使用说明」] [opinion]
- 含原游戏代码和资产的研究按现有私有同步规则处理；文档更新不改变仓库可见性和第三方授权边界。`Builds/`、`Artifacts/`和Unity缓存为忽略的本地产物。[verified: .gitignore 中 Builds/、Artifacts/、UnityProject/Library/；AGENTS.md「研究与版本」] [opinion]

```bash
git lfs install
git submodule update --init --recursive
git lfs pull
git submodule foreach --recursive 'git lfs pull'
```

上述命令用于已有授权的检出获取固定版本资料；资源下载不代表编译、运行或授权验收完成。[opinion]

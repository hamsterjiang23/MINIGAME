# MINIGAME

本仓库用于开发一款原创的**第一人称、2D/3D混合心理恐怖叙事游戏**。玩家扮演检定员，在员工公寓中通过电脑与病人交流，在制度配额与个人判断之间选择治疗、净化或释放。Mini Game目标为 **3天、5位病人、3种结局**，原创项目研究基线为 **Unity 2022.3 LTS**。[verified: docs/game-overview.md「一句话定位」「这次要交付的范围」；research/README.md L3–L7]

目前已有设计资料、15个固定版本的参考仓库、纯C#规则与存档，以及[Unity开发原型](UnityProject/README.md)源码：原创公寓、中文界面、观察、Yarn会谈、处置反应、回房和睡眠跨日。正式资源、对白与演出尚未完成；原创原型和恢复的参考游戏工程分别验证。[verified: docs/source-lock.md L11–L25；UnityProject/Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs CreateAndValidate；docs/reference-gameplay-migration.md] [opinion]

2026-10-15飞书来源已更新：主Wiki新增反叛值结局，流程画板新增每日开始前结局/KPI检查，小孩已有完整对白表；当前原型仍保留开发配置。变动与待裁决项见[同步记录](docs/feishu-sync-2026-10-15.md)。[verified: docs/.source-data/game-wiki.json content「结局」；docs/.source-data/sync-2026-10-15.json changes] [opinion]

**电脑终端现已默认接入小孩Day1正式来源对白**：六轮、20个回复、三种处置，配套[独立HTML策划工具](Tools/DialoguePlanner/DialoguePlanner.html)与Unity校验导入。原有三天开发原型保留；其余病人、Day2/3及正式美术继续待接入。启动、修改和验收见[终端使用指南](docs/terminal-ui-guide.md)。[verified: docs/terminal-ui-verification.json scope/source；UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L126] [opinion]

## 玩法设计

策划优先使用[电脑终端与对白工具指南](docs/terminal-ui-guide.md)；旧三天日循环与三结局路线保留在[开发原型验收指南](docs/planner-acceptance-guide.md)。[opinion]

**晨检与任务简报 → 电脑会谈 → 治疗／净化／释放 → 夜间收束 → 睡眠与梦境 → 次日反馈。** 3D公寓承载移动与物件交互，2D终端承载病人对白、选项和处置。[verified: docs/game-overview.md「玩家的一天」]

**《寻找伪人》（No, I'm not a Human）是本项目重要的玩法参考。**[verified: 用户确认“寻找伪人为重要的玩法参考”] 重点借鉴观察与信息核对、对话中的不确定性、决策后果、昼夜节奏和资源压力，并结合检定员与病人的原创设定改造。[opinion]

- 五位病人为神父、小孩、女性执政者、战场医生和外交官；角色经历与跨日变化见[游戏总览](docs/game-overview.md)。[verified: docs/game-overview.md「五位病人分别承担什么体验」]
- 普通会谈采用多轮选项与分支汇合，符合信任条件时追加特殊会谈，结束后才允许处置；处置改变人物去留与后续事件。[verified: docs/game-overview.md「审判、对话与代价」；src/README.md「调用入口」「开发默认规则」]
- 当前原型的三种结局为被带走、神父袭击、KPI失败后锁房；主Wiki新增的反叛值分支尚未接入，正式数量、阈值、优先级和演出仍需策划确认。[verified: docs/game-overview.md「结局与可感知后果」；src/README.md「开发默认规则」]

**开发配置与正式策划分开维护。** 默认小孩Day1演示采用信任0、释放门槛3、净化剩余5；旧三天原型仍保留每日目标1、普通4轮、特殊门槛20、释放门槛30的配置。它们不是全局正式数值表；Day0、特殊角色、其余对白与演出继续见[设计待定项](docs/open-questions.md)。[verified: UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json profile；src/README.md「开发默认规则」] [opinion]

## 研发与研究进展

2026-10-15终端切片最新结果：锁定SDK8.0.424的Release构建0警告/错误，185项.NET测试、18项策划工具测试、Unity EditMode2项及PlayMode9项通过，Windows Mono构建成功。浏览器视觉和下载点击未验收，Player启动及其他范围见[本轮持久化报告](docs/terminal-ui-verification.json)。下方2026-10-13记录继续保留历史身份。[verified: docs/terminal-ui-verification.json dotnet/planner/unity/limits] [opinion]

**原创核心逻辑**位于[源码入口](src/README.md)：日期与阶段、五人独立状态、会谈门控、一次性处置、配额、跨日事件、结局和JSON存档已有实现。新 `GameRuntime` 管理输入、观察和演出检查点，`GameSession` 保持规则权威；迁移依据与边界见[寻找伪人玩法迁移](docs/reference-gameplay-migration.md)。[verified: src/MiniGame.Presentation/GameRuntime.cs；src/MiniGame.Core/Runtime/GameSession.cs]

2026-10-13在Windows重新完成SDK8.0.424锁定恢复、Release构建（0警告、0错误）与150项测试：Core73、Presentation77。固定Yarn编译器验证327个生成节点、536条文本且无诊断。Unity导入、PlayMode与Player构建另行记录，不从这些结果推导可玩结论。[verified: Artifacts/Tests/migration-verification-report.json；Artifacts/Tests/migration-build.log；Artifacts/YarnSmoke/compiler-report.json] [opinion]

Unity2022.3.23f1c1隔离工程已完成真实场景生成、四项图形模式PlayMode测试和Windows Mono开发构建（0错误、1警告）；构建产物位于 `Builds/MiniGame/MiniGame.exe`，启动检查通过。2026-10-15通过仓库根 `Open-Unity.cmd` 修复缺失启动环境后，正常UPM工程导入、主场景打开及四项图形PlayMode测试已通过；IL2CPP尚未验证。完整结果保存在[验证汇总](docs/gameplay-migration-verification.json)，可重放入口见[工程说明](UnityProject/README.md)。[verified: docs/gameplay-migration-verification.json unity；Artifacts/Unity/player-startup.json Ready/StartupExceptions] [opinion]

**《No, I'm not a Human》参考研究**位于 `research/no-im-not-a-human/`，入口为[逆向总报告](research/no-im-not-a-human/Reports/overview.md)和[交付检查记录](research/no-im-not-a-human/Analysis/deliverable-verification.json)。用户已确认将仓库改为private，完整研究目录按私有仓库同步规则纳入Git与Git LFS，包括游戏工作副本、恢复代码、资源、工具、索引和报告；访问与克隆需要仓库授权。[verified: 用户确认“改为private”] [opinion]

- 已从真实备份程序集恢复主程序集943个类型、5855个方法索引与5126个真实CIL方法体，并建立静态调用及CIL／生成C++关系；恢复工程合并625份真实实现和53个匹配插件。[verified: research/no-im-not-a-human/Reports/code-recovery.md L5、L37、L59；research/no-im-not-a-human/Analysis/deliverable-verification.json merged_csharp9_implementations/merged_plugins]
- 已导出7个场景、38个Prefab、1003张PNG和359份OGG，以及动画、UI与文件级GUID引用索引；编译对白索引包含3255个Yarn节点。[verified: research/no-im-not-a-human/Reports/unity-recovery.md L17–L22、L32；research/no-im-not-a-human/Analysis/deliverable-verification.json exported_scenes/exported_prefabs/yarn_nodes]
- 73份角色配置、49份URP灯光与1份PlayerSettings仍有序列化问题；自定义Shader存在占位，准确版本Editor导入、语义编译和运行尚未验证。导出文件与语法解析成功不能证明原游戏已完整复原。[verified: research/no-im-not-a-human/Reports/unity-recovery.md L52–L68；research/no-im-not-a-human/Analysis/deliverable-verification.json comprehensive_language_feature_validation=false/unity_import_verified=false/limits]

参考游戏使用 **Unity 6000.3.10f1、IL2CPP**；它与原创项目的Unity 2022.3基线分别记录。访客身份判定、身体检查和生存资源等是玩法研究与借鉴重点，具体采用方式结合本项目设计确定；原创规则不引入真假／善恶标准答案。受版权保护的原游戏代码和资产不公开分发。[verified: research/no-im-not-a-human/Analysis/investigation-status.json unity_version/runtime_backend；research/README.md L74；src/README.md「开发默认规则」] [opinion]

## 后续方向

1. 复核新机器依赖获取，并另行验证IL2CPP构建。[opinion]
2. 用正式台词、人物差分和观察线索替换占位内容，确认线索如何影响判断及每日资源压力。[opinion]
3. 制作三天梦境和三种结局演出；确认配置、日记与特殊角色规则后再扩展随机事件或物资。[opinion]
4. 参考研究继续从缺失角色字段、准确Editor导入和未解析动态调用入手。[opinion]

具体切片与验收场景见[Unity开发路线](research/unity-2022-roadmap.md)；该路线中的尚未实施项结合[当前核心实现](src/README.md)阅读，避免把早期研究状态当作最新代码状态。[verified: research/unity-2022-roadmap.md「按验证切片推进」；src/README.md 开篇说明] [opinion]

## 阅读入口

- [当前完成情况与待补项](docs/project-status.md)：开发原型、验证范围、正式内容缺口和后续交付顺序。
- [核心逻辑源码](src/README.md)与[实现说明](src/architecture-and-rationale.md)。
- [Unity开发原型](UnityProject/README.md)与[寻找伪人玩法迁移](docs/reference-gameplay-migration.md)。
- [游戏总览](docs/game-overview.md)、[设计文档入口](docs/README.md)与[设计待定项](docs/open-questions.md)。
- [源码借鉴研究](research/README.md)、[需求借鉴矩阵](research/reuse-matrix.md)、[Unity开发路线](research/unity-2022-roadmap.md)与[研究风险](research/risks-and-gaps.md)。
- [逆向总报告](research/no-im-not-a-human/Reports/overview.md)与[交付检查记录](research/no-im-not-a-human/Analysis/deliverable-verification.json)。
- [仓库协作规则](AGENTS.md)。

## 目录

- `src/`：原创纯C#核心逻辑、测试和实现说明。[verified: src/README.md 开篇说明及「目录与技术基线」]
- `UnityProject/`：Unity 2022.3原创公寓、输入/UI、Yarn桥、场景生成器和PlayMode测试源码。[verified: UnityProject/Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs CreateAndValidate；UnityProject/Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs]
- `docs/`：游戏设计整理、逐篇来源与待定问题。[verified: docs/game-overview.md 开篇来源说明]
- `research/`：第三方源码借鉴与开发路线；其中 `no-im-not-a-human/` 为完整逆向资料，按私有Git／Git LFS规则同步。[verified: research/README.md L11–L16；用户确认“改为private”] [opinion]
- `Dependency/`：早期技术研究材料。[verified: research/README.md L80]
- `references/`：15个固定提交的第三方Git子模块。[verified: docs/source-lock.md L11–L25、L49]

## 私有同步与获取资料

主仓库按私有访问规则管理完整研究，二进制文件及单文件大小达到1MiB的其他材料使用Git LFS；较小的文本源代码通常使用普通Git，大型C++／IL文本同样按阈值纳入LFS，具体规则由根 `.gitattributes` 管理。[opinion]

包含原游戏及恢复资料的仓库不得改为公开，未经授权的原游戏代码和素材不得公开分发。私有同步不改变这些内容的权利归属；历史报告中的“仅本地／未发布”描述保留为当时执行记录，当前同步约定以本说明为准。[verified: 用户确认“改为private”] [opinion]

先取得私有仓库访问授权，安装Git与Git LFS并配置有权访问仓库的Git凭据，再递归克隆；进入主仓库后拉取其LFS对象。[opinion]

```bash
git lfs install
git clone --recurse-submodules https://github.com/hamsterjiang23/MINIGAME.git
cd MINIGAME
git lfs pull
```

如果已经普通克隆过主仓库，初始化固定版本子模块：[opinion]

```bash
git submodule update --init --recursive
```

若此前跳过了LFS下载，补取主仓库大文件与各固定版本参考工程的大文件；子模块从各自上游取LFS对象：[opinion]

```bash
git lfs pull
git submodule foreach --recursive 'git lfs pull'
```

参考仓库通过`.gitmodules`及gitlink关联各自上游，并固定到[版本记录](docs/source-lock.md)和[source-lock.json](docs/source-lock.json)中的提交；完整历史和当前版本LFS资源从各自上游取得。不要使用 `git submodule update --remote` 自动切换研究版本。[verified: docs/source-lock.md L49–L53] [opinion]

获取完成不代表工程已经编译或运行；运行状态与许可文件在对应研究文档中记录。代码、第三方依赖和资产遵循各自许可，本仓库不重新授予参考工程或素材的权利。[verified: docs/source-lock.md L29–L45、L51–L52] [opinion]

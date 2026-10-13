# MINIGAME

本仓库用于开发一款原创的**第一人称、2D/3D混合心理恐怖叙事游戏**。玩家扮演检定员，在员工公寓中通过电脑与病人交流，在制度配额与个人判断之间选择治疗、净化或释放。Mini Game目标为 **3天、5位病人、3种结局**，原创项目研究基线为 **Unity 2022.3 LTS**。[verified: docs/game-overview.md L7–L23；research/README.md L3–L7]

目前已有设计资料、15个固定版本的参考仓库、源码借鉴研究，以及纯C#规则与存档实现。自己的Unity场景、UI、正式资源和可玩版本尚未完成；本地恢复的参考游戏工程另有验证缺口。[verified: docs/source-lock.md L11–L25；src/README.md L3；research/no-im-not-a-human/Analysis/deliverable-verification.json unity_import_verified=false]

## 玩法设计

**晨检与任务简报 → 电脑会谈 → 治疗／净化／释放 → 夜间收束 → 睡眠与梦境 → 次日反馈。** 3D公寓承载移动与物件交互，2D终端承载病人对白、选项和处置。[verified: docs/game-overview.md L39–L49]

**《寻找伪人》（No, I'm not a Human）是本项目重要的玩法参考。**[verified: 用户确认“寻找伪人为重要的玩法参考”] 重点借鉴观察与信息核对、对话中的不确定性、决策后果、昼夜节奏和资源压力，并结合检定员与病人的原创设定改造。[opinion]

- 五位病人为神父、小孩、女性执政者、战场医生和外交官；角色经历与跨日变化见[游戏总览](docs/game-overview.md)。[verified: docs/game-overview.md L19、L84–L88]
- 普通会谈采用多轮选项与分支汇合，符合信任条件时追加特殊会谈，结束后才允许处置；处置改变人物去留与后续事件。[verified: docs/game-overview.md L57–L74；src/README.md L68–L73、L83–L89]
- 三种结局目标是被带走、神父袭击、KPI失败后锁房；当前代码已有开发配置下的判定，正式阈值、优先级和演出仍需策划确认。[verified: docs/game-overview.md L94–L100；src/README.md L81、L89]

**开发配置与正式策划分开维护。** 当前配置采用每日净化目标1、普通会谈4轮、特殊轮信任门槛20、释放门槛30；神父Day2加入，另外四人Day1加入。它们用于验证流程，不是正式数值表；节点ID和回复也是逻辑占位内容。Day0教学、外交官特例、正式对白、梦境与日记等继续列在[设计待定项](docs/open-questions.md)。[verified: src/README.md L81–L95；docs/open-questions.md「开发范围与流程」「数值、信息与叙事」「世界观、角色与结局」]

## 研发与研究进展

**原创核心逻辑**位于[源码入口](src/README.md)：日期与阶段、五人独立状态、会谈门控、一次性处置、配额、跨日事件、结局和JSON存档已有实现。`GameSession`拥有权威状态，宿主通过命令、`PlayerView`与事件接入；Unity／Yarn适配尚未验证。[verified: src/README.md L3、L39–L77、L108–L118]

源码文档保留了2026-10-12在macOS ARM64上的构建与73项测试通过记录；这是历史验证，不代表本次Windows迁移重新执行了核心测试，也不包含Unity导入或打包。[verified: src/README.md L124–L130]

**《No, I'm not a Human》参考研究**位于 `research/no-im-not-a-human/`，入口为[逆向总报告](research/no-im-not-a-human/Reports/overview.md)和[交付检查记录](research/no-im-not-a-human/Analysis/deliverable-verification.json)。用户已确认将仓库改为private，完整研究目录按私有仓库同步规则纳入Git与Git LFS，包括游戏工作副本、恢复代码、资源、工具、索引和报告；访问与克隆需要仓库授权。[verified: 用户确认“改为private”] [opinion]

- 已从真实备份程序集恢复主程序集943个类型、5855个方法索引与5126个真实CIL方法体，并建立静态调用及CIL／生成C++关系；恢复工程合并625份真实实现和53个匹配插件。[verified: research/no-im-not-a-human/Reports/code-recovery.md L5、L37、L59；research/no-im-not-a-human/Analysis/deliverable-verification.json merged_csharp9_implementations/merged_plugins]
- 已导出7个场景、38个Prefab、1003张PNG和359份OGG，以及动画、UI与文件级GUID引用索引；编译对白索引包含3255个Yarn节点。[verified: research/no-im-not-a-human/Reports/unity-recovery.md L17–L22、L32；research/no-im-not-a-human/Analysis/deliverable-verification.json exported_scenes/exported_prefabs/yarn_nodes]
- 73份角色配置、49份URP灯光与1份PlayerSettings仍有序列化问题；自定义Shader存在占位，准确版本Editor导入、语义编译和运行尚未验证。导出文件与语法解析成功不能证明原游戏已完整复原。[verified: research/no-im-not-a-human/Reports/unity-recovery.md L52–L68；research/no-im-not-a-human/Analysis/deliverable-verification.json comprehensive_language_feature_validation=false/unity_import_verified=false/limits]

参考游戏使用 **Unity 6000.3.10f1、IL2CPP**；它与原创项目的Unity 2022.3基线分别记录。访客身份判定、身体检查和生存资源等是玩法研究与借鉴重点，具体采用方式结合本项目设计确定；原创规则不引入真假／善恶标准答案。受版权保护的原游戏代码和资产不公开分发。[verified: research/no-im-not-a-human/Analysis/investigation-status.json unity_version/runtime_backend；research/README.md L74；src/README.md L91] [opinion]

## 后续方向

1. 建立自己的Unity 2022.3工程，记录确切Editor补丁、渲染管线与依赖版本，接入纯C#核心和对话适配器。[opinion]
2. 做一个病人的完整流程：公寓进入电脑、普通与特殊会谈、三种处置、反应花屏、返回房间；验证输入切换与处置去重。[opinion]
3. 接通床、梦境、跨日队列与存档恢复，确认重载不会重复结算，再扩展到五病人、三天和三种结局。[opinion]
4. 确认正式配置、台词和资产范围，完成演出与目标平台验证；本地逆向继续从缺失角色字段、准确Editor导入和未解析动态调用入手。[opinion]

具体切片与验收场景见[Unity开发路线](research/unity-2022-roadmap.md)；该路线中的尚未实施项结合[当前核心实现](src/README.md)阅读，避免把早期研究状态当作最新代码状态。[verified: research/unity-2022-roadmap.md「按验证切片推进」；src/README.md L3] [opinion]

## 阅读入口

- [核心逻辑源码](src/README.md)与[实现说明](src/architecture-and-rationale.md)。
- [游戏总览](docs/game-overview.md)、[设计文档入口](docs/README.md)与[设计待定项](docs/open-questions.md)。
- [源码借鉴研究](research/README.md)、[需求借鉴矩阵](research/reuse-matrix.md)、[Unity开发路线](research/unity-2022-roadmap.md)与[研究风险](research/risks-and-gaps.md)。
- [逆向总报告](research/no-im-not-a-human/Reports/overview.md)与[交付检查记录](research/no-im-not-a-human/Analysis/deliverable-verification.json)。
- [仓库协作规则](AGENTS.md)。

## 目录

- `src/`：原创纯C#核心逻辑、测试和实现说明。[verified: src/README.md L3、L11–L15]
- `docs/`：游戏设计整理、逐篇来源与待定问题。[verified: docs/game-overview.md L3]
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

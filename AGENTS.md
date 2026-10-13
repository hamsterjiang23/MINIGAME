# MINIGAME 协作规则

适用于整个仓库；任务目标与阅读入口见 [README](README.md)，实现边界见 [核心说明](src/README.md)。[opinion]

## 目标与资料

- 开发原创的检定员心理恐怖游戏，Mini范围为3天、5病人、3结局；不因参考研究自动扩大玩法。[verified: docs/game-overview.md L7–L23] [opinion]
- 将《寻找伪人》（No, I'm not a Human）作为重要的玩法参考；设计与实现时优先查阅[玩法研究](research/no-im-not-a-human/Reports/overview.md)，借鉴观察、对话、决策后果、昼夜节奏与资源压力，并按原创设定改造。[verified: 用户确认“寻找伪人为重要的玩法参考”] [opinion]
- 原创技术基线为Unity 2022.3 LTS；参考游戏的Unity 6000.3.10f1另行记录，不据此升级原创工程。[verified: research/README.md L3；research/no-im-not-a-human/Analysis/investigation-status.json unity_version] [opinion]
- `src/`承载规则与测试，`docs/`保存设计来源和待定项，`research/`保存研究与路线，`Dependency/`保留早期资料，`references/`保存固定版本参考仓库。[verified: src/README.md L3；docs/game-overview.md L3；research/README.md L16、L80；docs/source-lock.md L49]
- 修改正式需求前查 [设计待定项](docs/open-questions.md)；`PrototypeRules`与占位对白只代表开发配置，不能写成已确认策划。[verified: src/README.md L81–L95] [opinion]

## 核心与适配器

- 保持 `GameSession` 为运行状态权威；宿主提交命令、读取 `PlayerView`、消费事件，再通知演出完成，不用存档DTO修改活动会话。[verified: src/README.md L39、L61–L77] [opinion]
- Unity／Yarn适配器负责文本、选项、输入、演出与文件目录；不另存一份权威配额、治疗次数、信任或人物去留。[verified: src/README.md L116–L118] [opinion]
- 处置提交一次；反应回调只前进。跨日与读档保留事件去重和会谈检查点，恢复不能再次执行副作用。[verified: src/README.md L70–L75、L108–L112] [opinion]
- 保存实际回答和处置；未经策划确认不合并疯狂值／偏离指数／狂热值，也不增加真假／善恶标准答案。[verified: src/README.md L91] [opinion]

## 研究与版本

- 完整 `research/no-im-not-a-human/` 按私有仓库规则纳入Git／Git LFS；克隆需仓库授权并执行主库与子模块的LFS拉取。[verified: 用户确认“改为private”] [opinion]
- 二进制及单文件达到1MiB的其他材料使用LFS，小文本通常普通Git；以根 `.gitattributes` 为准，检查大C++／IL也按阈值匹配。[opinion]
- 不修改Steam原游戏，不绕过DRM；含原游戏与恢复资料的仓库不得改公开，未经授权的原游戏代码和资产不得公开分发，研究只使用工作副本。[opinion]
- 历史证据保留原始路径与哈希；搬迁记录建立新旧映射，活动脚本和导航使用当前根路径或相对路径。[opinion]
- `references/`保持 [source-lock](docs/source-lock.md) 固定提交，不用 `git submodule update --remote`；复用前核对对应代码、依赖和资产许可。[verified: docs/source-lock.md L49–L53] [opinion]

## 证据与验证

- 结论标注 `[verified: 文件行号或JSON键]` 或 `[opinion]`；区分真实实现、占位、推断和未知，不把文件导出或语法检查写成游戏可玩证明。[opinion]
- 核心使用固定SDK **8.0.424**，不静默升级；代码遵循现有C#9、可空检查与锁定依赖。[verified: src/global.json L3–L5；src/README.md L15–L17] [opinion]
- 核心行为变化时，在 `src/` 依次执行 `dotnet restore MiniGame.sln --locked-mode`、Release构建和测试；优先验证处置去重、跨日、结局冲突和存档恢复。[verified: src/README.md L24–L26、L127] [opinion]
- 文档变化检查链接与Git差异，研究变化执行交付校验器，同步检查LFS完整性；分别报告当前测试、历史记录、Editor导入、Play Mode和目标平台构建状态。[opinion]

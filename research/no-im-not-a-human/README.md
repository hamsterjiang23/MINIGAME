# No, I'm not a Human 研究成果

用户已确认将MINIGAME仓库改为private。本目录完整研究按私有Git／Git LFS规则同步，包括原游戏工作副本、反编译代码、恢复工程、资源、工具、索引与报告；访问、克隆和拉取LFS对象需要仓库授权。获取命令见[主仓库说明](../../README.md)。[verified: 用户确认“改为private”] [opinion]

二进制及单文件达到1MiB的其他材料使用Git LFS，较小文本通常普通Git；大C++／IL文本也遵循阈值，实际匹配规则见主仓库根 `.gitattributes`。[opinion]

实际安装：E:/steam/steamapps/common/No, I'm not a Human；构建引擎6000.3.10f1/IL2CPP。原始965文件在本次分析前后SHA256一致。[verified: Analysis/Environment/source-immutability-verification.json；engine-and-recovery-summary.json]

先阅读 [总报告](Reports/overview.md)。它说明真正恢复的代码/关系/资源，以及工程编译、序列化和运行验证仍未完成的边界。[verified: Reports/overview.md]

- [完整代码与IL](DecompiledCode)：主程序集、firstpass、原始IL及独立UnityCompatible输出。[verified: 对应导出目录与CSharp9 provenance]
- [Unity工程根](RecoveredUnityProject/ExportedProject)：AssetRipper工程，导入/运行尚未验证。[verified: ProjectSettings/ProjectVersion.txt；Reports/unity-source-compatibility.md]
- [类型/方法/字段和调用图](Analysis/Managed/Assembly-CSharp)：JSON、CSV、DOT/Mermaid和校验。[verified: coverage.json/verification.json]
- [原生静态RVA](Analysis/Native)：5855个slot、4999非零条目、3416不同首选映像地址。[verified: native-map-verification.json]
- [对白与状态](Analysis/Gameplay)：编译Yarn恢复、节点/变量及系统状态资料。[verified: Reports/gameplay-dialogue-progression.md]
- [资源清单/引用](Analysis/Assets)：导出统计、GUID、脚本/插件合并、读取失败证据。[verified: recovery-summary.json及引用JSONL]
- [全部专题报告](Reports)、[残余问题](Reports/remaining-questions.md)、[170个REA工具与参数](Reports/rea-tool-catalog.md)。[verified: 相应文件]

原游戏与恢复资料供获授权的私有研究；包含这些资料的仓库不得改为公开，未经授权的原游戏代码与素材不得公开分发。原始调查没有修改原游戏、处理DRM或制作新游戏；私有同步不代表已验证工程可玩，也不改变素材权利归属。[verified: Analysis/investigation-status.json source_policy/completion_scope] [opinion]

当前位置由 [调查状态](Analysis/investigation-status.json) 记录；迁移校验与旧路径映射见 [迁移记录](Analysis/Relocation/relocation.json)。历史 JSON 中的旧绝对路径保留为原始证据，操作脚本从自身位置推导研究根目录。[verified: Analysis/investigation-status.json；Analysis/Relocation/relocation.json]

历史迁移与静态报告中的“仅本地／未发布”等描述保留为当时记录；后续已决定按本说明纳入私有Git／Git LFS同步，上传完成与远端完整性以实际同步验收为准。[verified: 用户确认“改为private”] [opinion]

# No, I'm not a Human 本地研究成果

实际安装：E:/steam/steamapps/common/No, I'm not a Human；构建引擎6000.3.10f1/IL2CPP。原始965文件在本次分析前后SHA256一致。[verified: Analysis/Environment/source-immutability-verification.json；engine-and-recovery-summary.json]

先阅读 [总报告](Reports/overview.md)。它说明真正恢复的代码/关系/资源，以及工程编译、序列化和运行验证仍未完成的边界。[verified: Reports/overview.md]

- [完整代码与IL](DecompiledCode)：主程序集、firstpass、原始IL及独立UnityCompatible输出。[verified: 对应导出目录与CSharp9 provenance]
- [Unity工程根](RecoveredUnityProject/ExportedProject)：AssetRipper工程，导入/运行尚未验证。[verified: ProjectSettings/ProjectVersion.txt；Reports/unity-source-compatibility.md]
- [类型/方法/字段和调用图](Analysis/Managed/Assembly-CSharp)：JSON、CSV、DOT/Mermaid和校验。[verified: coverage.json/verification.json]
- [原生静态RVA](Analysis/Native)：5855个slot、4999非零条目、3416不同首选映像地址。[verified: native-map-verification.json]
- [对白与状态](Analysis/Gameplay)：编译Yarn恢复、节点/变量及系统状态资料。[verified: Reports/gameplay-dialogue-progression.md]
- [资源清单/引用](Analysis/Assets)：导出统计、GUID、脚本/插件合并、读取失败证据。[verified: recovery-summary.json及引用JSONL]
- [全部专题报告](Reports)、[残余问题](Reports/remaining-questions.md)、[170个REA工具与参数](Reports/rea-tool-catalog.md)。[verified: 相应文件]

源码和原资产只供当前本地调查；本次没有进行公开发布、原游戏修改、DRM处理或新游戏制作。[verified: 调查执行边界；source-immutability-verification.json]

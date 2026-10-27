# 终端冗余文字清理

按用户红框删除检测区的“许可带”说明及全部检测状态文字、处置按钮下的会谈／锁定状态说明，并将“对话 / Rn”简化为“对话”。社会适应度波形、固定许可带虚线、释放判定、净化配额、四条应答和原有按钮命中区域保留。[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs constructor/Render/RenderDispositionCommands；../../UnityProject/Assets/MiniGame/Runtime/TerminalAdaptationWaveform.cs]

新增 `Open-Editor.cmd` 与版本匹配的 Editor 启动脚本；桌面的“打开NORM项目.cmd”调用它。启动仅打开源工程，已运行时切回现有窗口，不自动开始 Play。已通过 CMD 包装入口的 `-CheckOnly` 检查。[verified: ../../Open-Editor.cmd；../../UnityProject/Tools/Open-Editor.ps1]

## 当前验证

- .NET SDK 8.0.424：锁定恢复、Release 构建通过，0 警告／0 错误；Core 154 项、Presentation 319 项通过，共 473 项。[verified: terminal-cleanup-2026-10-28/verification.json dotnet]
- Unity 2022.3.23f1c1 隔离工程：`TerminalV0824` 26 项 PlayMode 通过，包含最新神父释放回应中的数值更新、四条应答、CRT 命中、暂停、字形及真实波形像素检查。[verified: terminal-cleanup-2026-10-28/verification.json unity]
- 对白工具：Campaign smoke／完整产物检查、监督员检查、结局检查通过；32 项 Python 测试通过。自审源快照的规范 LF 字节与既定 SHA-256 及 Git 原始对象一致，为该文件固定 LF，避免 Windows 自动换行导致误报；未修改任何来源校验哈希或正文。[verified: terminal-cleanup-2026-10-28/verification.json content；../../.gitattributes]

[清理后实际渲染](terminal-cleanup-2026-10-28/review.png)。渲染捕获使用隔离工程和开发规则，不代表正式 Campaign 完整游玩或外发 Player 重建。本次提交按用户要求包含工作区原有的回答／处置信任适配、生成包、来源审计和本机 Editor 设置。[verified: ../../UnityProject/Assets/MiniGame/Tests/PlayMode/SocialAdaptationPlayModeTests.cs；terminal-cleanup-2026-10-28/verification.json scope]

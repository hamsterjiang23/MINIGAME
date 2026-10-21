# 当日工作指标与开机字符音效

人员复核页在状态提示与处置命令之间增加“当日指标 / 净化剩余”，数字两位补零，直接读取游戏当前剩余配额。原有“净化要求：已完成/要求”保留在复核标题同行，目录配额继续读取同一会话状态。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs L151-L158、L259-L260]`

开机自检每个新出现的非空白字符从 MCQUeen 的 UIType1～4 随机抽取一个音效；数字和标点计入，空白、光标闪烁与同帧重绘不重复触发。完整时间轴回归验证322次事件，并检查四段真实导入音频已加载；这不代表实际扬声器听感验收。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalTransitionView.cs L37-L64；../../UnityProject/Assets/MiniGame/Runtime/MiniGameAudioService.cs L101-L107；verification.json typing]`

运行文件：[MiniGame.exe](../../Builds/TerminalDailyWork/MiniGame.exe)。参考 HTML 与四段音频的来源及 SHA-256 见[来源清单](source-manifest.json)，本轮结果及验证范围见[验证记录](verification.json)。`[verified: verification.json build.output；source-manifest.json html、audio]`

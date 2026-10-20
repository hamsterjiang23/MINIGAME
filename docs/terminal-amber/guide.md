# PMS 琥珀终端

本轮以用户提供的 v0.8.12 HTML 和 `PMS_Unity_Resources_v0.8.12` 为外观资源来源。当前接入版采用原生800×600终端、4:3显示、动态人物监控；正式场景继续使用原有Campaign内容。`[verified: verification.json resources/ui]`

## 操作

- Day0首次启动使用[监管者近景与连续拉远](../opening/guide.md)，直接接入教学；Day1起进入电脑播放7.5秒自检，再用1.48秒逐区载入目录。Enter可跳过自检，载入时锁定操作。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs DayZeroOpening/PowerOn；../../src/MiniGame.Presentation/TerminalPowerTimeline.cs BootDuration/LoadingDuration]`
- 单击名册选择人物，双击或Enter进入。应答默认选中第一项，上下键循环，Enter、数字1–4及单击确认；空格只推进对白。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs EnterSelected；../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs ShowResponseButtons/HandleTerminalKey]`
- F2设置、F3查阅模式及对应按钮已移除。左下自动显示最近对话；Esc使用现有暂停菜单。`[verified: verification.json ui.disabledFunctions；../../UnityProject/Assets/MiniGame/Tests/PlayMode/TerminalHtmlParityTests.cs RemovedFunctionKeysLeavePassiveTranscriptAndDialogueAvailable]`
- 目录的“关机返回房间”允许中途离开；重新开机保留当天进度。全部实际待审人物完成处置及反应对白后，终端自动关机并衔接原有夜间流程。`[verified: ../../src/MiniGame.Presentation/GameRuntime.cs CompleteCue/LeaveTerminal；../../src/MiniGame.Presentation.Tests/TerminalDirectoryRuntimeTests.cs]`
- 关机经历关闭会话、系统停止、横线收束及光点熄灭。开关机与人物断线是两种演出，副作用仍由会话层结算一次。`[verified: ../../src/MiniGame.Presentation/TerminalPowerTimeline.cs Shutdown；../../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs PowerOff/TerminalSignalLoss]`
- 关闭终端时，左上角依次输出 `Closing session...`、`Session closed.`、`System halted.`，然后熄屏回房。文字采用HTML的28/24起点及20像素行距；长帧不会跨过尚未显示的文字阶段。可查看[关闭过程60fps演示](previews/closing-session-60fps.mp4)及[专项验证](closing-verification.json)，运行版为 `Builds/TerminalClosing/MiniGame.exe`。`[verified: closing-verification.json reference/changes/capture/windowsMono]`

## 资源与调整

`UnityProject/Assets/PMSAmberTerminal`保留导入资源及原始GUID。`TerminalAssets.asset`引用其中的Noto中文字体、界面框图和CRT Shader；字体许可随包保存。CRT Shader追加了宿主关机收束，包内演示也移除了F3入口。原始文件哈希见[包来源清单](package-source-manifest.json)，导入方历史报告单独保存在[原始验证记录](package-original-verification.json)。`[verified: verification.json resources；../../UnityProject/Assets/MiniGame/Resources/MiniGame/TerminalAssets.asset；../../UnityProject/Assets/PMSAmberTerminal/Fonts/OFL.txt]`

人物监控采用464×261画框及独立动态房间，游戏内固定全景；近景仍可在“MiniGame → 人物监控室”里预览。暖灰色调与房间明暗可在人物监控配置中调整。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs UpdatePortrait；../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs；../../UnityProject/Assets/MiniGame/Resources/MiniGame/PatientMonitorDefault.asset]`

## 验证与运行

本轮实际检查结果、运行版本路径与截图见[验证记录](verification.json)。原始HTML固定在[参考快照](reference-v0.8.12.html)；包内旧验证记录仅代表导入方原版，不覆盖本轮改造。`[verified: verification.json；package-original-verification.json]`

可查看[独立Player的GPU终端预览](previews/review.png)。这是原生UI纹理经过实际CRT材质的离屏输出；隐藏窗口整屏截取不作为画面验收证据。`[verified: verification.json previews/limits]`

开发Player附带显式启动参数 `--terminal-amber-smoke`，用于自动验证真实显示层的鼠标命中、手动关机重开、六轮会谈及最终自动关机；正常启动按主场景流程运行。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/AmberTerminalPlayerSmoke.cs Install/Tick]`

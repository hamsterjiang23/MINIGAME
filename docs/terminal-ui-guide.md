# 电脑终端与策划对白工具

本页说明保留的**V1小孩Day1独立演示**与单日策划工具。主场景现在默认使用Day0＋五人三天的V2全流程，启动、对白转换及27张原创立绘见[全流程指南](campaign-guide.md)和[验收报告](campaign-verification.json)。[verified: campaign-verification.json content/portraits/unity；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs Awake/StartCampaign]

当前终端已迁移到[琥珀终端指南](terminal-amber/guide.md)：v0.8.12资产包、800×600显示、开关机与动态监控；F2／F3功能已移除。`[verified: terminal-amber/verification.json ui/resources]`

## 在 Unity 运行

1. 单日演示使用 `InspectionApartment.unity`：退出Play后通过 `MiniGame/Open prototype scene` 打开，再进入Play。先到门口按E完成晨检与简报，再到电脑开始小孩Day1。仓库根 [Open-Unity.cmd](../Open-Unity.cmd) 则启动融合房间的V2全流程。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs OpenPrototype；../UnityProject/Tools/Open-Project.ps1；campaign-verification.json unity]
2. 默认启用 Bootstrap 的 **Use Terminal Demo**，从 `Resources/MiniGame/ActiveDialoguePackage` 取得当前内容包。按现有房间交互进入电脑，在对象目录选择小孩开始审查。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L38、L115；terminal-ui-verification.json scope]
3. 发言用继续按钮、Enter或空格推进；回复用鼠标、数字1–4，或上下键定位后Enter确认。新选项默认高亮第一项，空格不选择答案。左下显示最近对话；Esc使用暂停菜单。处置权限继续读取会话规则。`[verified: terminal-amber/verification.json ui；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs HandleTerminalKey]`
4. 处置只结算一次；读完反应与断信号后，有后续待审对象时返回目录，最后一位完成后自动关机。已归档对象不能再次接待。F5保存、F9读取；终端演示使用 `terminal-demo-save.json`。`[verified: terminal-amber/verification.json ui/checks；../src/MiniGame.Presentation/GameRuntime.cs CompleteCue]`

界面当前采用v0.8.12琥珀资源，保留坐下和镜头衔接。每天首次进入才开机；目录可返回房间，电脑保持亮屏，再次进入继续当天进度。处置反应完成后局部断信号，有后续对象时回目录；当天最后一位完成后才自动关机。物理屏幕已按真实玻璃曲率贴合，开关机各有专用音效；本轮运行版与验证见[CRT指南](crt-screen-fit/guide.md)。`[verified: ../src/MiniGame.Presentation/GameRuntime.cs CompleteCue/LeaveTerminal；crt-screen-fit/verification.json]`

若需验收旧三天流程，在 Play 前取消 Bootstrap 的 **Use Terminal Demo**；开发API为 `StartLegacyPrototype()`。旧流程使用独立的 `prototype-save.json`，操作见[三天开发原型验收](planner-acceptance-guide.md#旧三天开发原型验收)。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L29-L30、L126]

## 策划修改并交付

1. 直接打开 [DialoguePlanner.html](../Tools/DialoguePlanner/DialoguePlanner.html)，内嵌示例无需服务器；继续编辑时用「打开内容包」读取上次保存的JSON。[verified: ../Tools/DialoguePlanner/build.js L5-L17；../Tools/DialoguePlanner/planner.template.html L11]
2. 从节点列表修改台词、回复、立绘ID、信任变化和后继节点，再点「应用节点修改」。新增/复制节点后接好前驱引用；删除会提示仍指向它的节点。已有ID保持稳定。[verified: ../Tools/DialoguePlanner/planner-app.js `renderEditor`、`addNode`、`delete.onclick`]
3. 使用终端排练，检查分支和定位错误；保存项目JSON用于下次打开，导出Unity包用于接入。包内 `yarnSource` 是唯一对白源；「导出 .yarn」是该源的原样副本，不要手改生成的 Runtime Yarn。[verified: ../Tools/DialoguePlanner/planner-app.js L192-L197；../Tools/DialoguePlanner/README.md「策划使用」] [opinion]
4. Unity菜单 `MiniGame/Dialogue Package Importer` →「选择内容包 JSON」→「校验并比较」→「导入并设为当前内容」。结构校验和原生Yarn编译成功后才激活新包；失败保留旧活动内容。导入后从新会话验收修改。[verified: ../UnityProject/Assets/MiniGame/Editor/DialoguePackageImporter.cs L238-L262；terminal-ui-verification.json unity.isolatedImport] [opinion]

演示配置为初始信任0、释放阈值3、净化剩余5。它是独立的HTML对照配置，不替代旧三天配置或正式数值裁决。更改规则/内容版本可能使旧存档不兼容，应保留旧文件并从新会话检查。[verified: ../UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json profile；terminal-ui-verification.json limits] [opinion]

## 支持范围与验证

首版支持带稳定ID的发言、选项、jump、立绘和整数信任变化；普通选项使用 `#id/#trust`，处置使用 `#id/#disposition`。不支持的任意条件、变量和命令必须先扩展导入器；未知语法会报错且保留原文。轮次汇合、循环、目标、立绘、处置反应与32位信任溢出均参与校验；Unity原生编译是最终检查。[verified: ../Tools/DialoguePlanner/planner-core.js `parseSource`、`validateRounds`；../src/MiniGame.Presentation/Authoring/RestrictedYarnParser.cs L128-L164] [opinion]

2026-10-18 [终端对齐验证](terminal-alignment-verification.json)：正常Unity2022.3.23f1c1工程的14项项目PlayMode测试全部通过，包含房间实体交互、六轮Yarn对白与三种处置、新增键盘/历史/归档恢复检查。九张实际Unity截图已检查；进出动画两份源码SHA256保持不变。HTML浏览器视觉仍受本地文件URL策略限制，本轮未重建Player。[verified: terminal-alignment-verification.json unity/preservedSources/reference/limits]

2026-10-15 [历史验证报告](terminal-ui-verification.json)保留SDK构建、185项核心/适配测试、18项策划工具测试、隔离工程EditMode2/PlayMode9及旧Windows Mono构建结果；这些不代表本轮重新执行了上述检查。[verified: terminal-ui-verification.json dotnet/planner/unity；terminal-alignment-verification.json limits]

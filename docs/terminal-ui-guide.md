# 电脑终端与策划对白工具

本页说明保留的**V1小孩Day1独立演示**与单日策划工具。主场景现在默认使用Day0＋五人三天的V2全流程，启动、对白转换及27张原创立绘见[全流程指南](campaign-guide.md)和[验收报告](campaign-verification.json)。[verified: campaign-verification.json content/portraits/unity；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs Awake/StartCampaign]

## 在 Unity 运行

1. 单日演示使用 `InspectionApartment.unity`：退出Play后通过 `MiniGame/Open prototype scene` 打开，再进入Play。先到门口按E完成晨检与简报，再到电脑开始小孩Day1。仓库根 [Open-Unity.cmd](../Open-Unity.cmd) 则启动融合房间的V2全流程。[verified: ../UnityProject/Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs OpenPrototype；../UnityProject/Tools/Open-Project.ps1；campaign-verification.json unity]
2. 默认启用 Bootstrap 的 **Use Terminal Demo**，从 `Resources/MiniGame/ActiveDialoguePackage` 取得当前内容包。按现有房间交互进入电脑，在对象目录选择小孩开始审查。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L38、L115；terminal-ui-verification.json scope]
3. 发言用继续按钮、Enter或空格推进；回复用鼠标、数字1–4，或上下键定位后Enter确认。新选项默认不选中，上下键循环，空格不选答案。「记录」每屏五行、长句单行裁切，滚轮翻阅，打开时阻挡对话按键；ESC先关闭记录。六轮完成后才允许处置，释放还需要绿灯。[verified: terminal-alignment-verification.json changes/unity.playMode；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs HandleTerminalKey]
4. 处置只结算一次；读完反应、花屏与断信号后返回目录，已归档对象不能再次接待。F5保存、F9读取；终端演示使用 `terminal-demo-save.json`。恢复可能重播当前对话段，不承诺逐行/逐字位置恢复。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L29-L30；terminal-ui-verification.json scope/limits] [opinion]

界面按v0.5.2默认工业皮肤调整，保留原有推镜、开机和退出回位。处置反应完成后只在监控窗口花屏，当前发言保留对象最后一句；归档页Enter回目录，再点「返回房间」退出电脑。[verified: terminal-alignment-verification.json changes/preservedSources/unity.playMode]

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

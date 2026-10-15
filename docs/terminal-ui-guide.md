# 电脑终端与策划对白工具

当前默认入口是**小孩 Day1 独立演示**：按用户提供HTML接入六轮会谈、20个回复、三种处置及对应反应。原有三天开发原型继续保留。人物立绘、档案内容和场景美术仍有占位，本切片不代表五病人三天正式内容完成。[verified: terminal-ui-verification.json scope/source；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L126] [opinion]

## 在 Unity 运行

1. 双击仓库根 [Open-Unity.cmd](../Open-Unity.cmd)，通过 `MiniGame/Open prototype scene` 打开主场景，按 Play。[verified: ../UnityProject/README.md「运行与操作」] [opinion]
2. 默认启用 Bootstrap 的 **Use Terminal Demo**，从 `Resources/MiniGame/ActiveDialoguePackage` 取得当前内容包。按现有房间交互进入电脑，在对象目录选择小孩开始审查。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L38、L115；terminal-ui-verification.json scope]
3. 发言用继续按钮、Enter或空格推进；回复用鼠标、数字1–4或上下键选择后确认。「记录」只展示实际走过的发言、回复和处置。六轮完成后才允许处置，释放还需要绿灯。[verified: terminal-ui-verification.json unity.playModePassed/source；../Tools/DialoguePlanner/planner-app.js `renderPreview`]
4. 处置只结算一次；读完反应、花屏与断信号后返回目录，已归档对象不能再次接待。F5保存、F9读取；终端演示使用 `terminal-demo-save.json`。恢复可能重播当前对话段，不承诺逐行/逐字位置恢复。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L29-L30；terminal-ui-verification.json scope/limits] [opinion]

若需验收旧三天流程，在 Play 前取消 Bootstrap 的 **Use Terminal Demo**；开发API为 `StartLegacyPrototype()`。旧流程使用独立的 `prototype-save.json`，操作见[三天开发原型验收](planner-acceptance-guide.md#旧三天开发原型验收)。[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs L29-L30、L126]

## 策划修改并交付

1. 直接打开 [DialoguePlanner.html](../Tools/DialoguePlanner/DialoguePlanner.html)，内嵌示例无需服务器；继续编辑时用「打开内容包」读取上次保存的JSON。[verified: ../Tools/DialoguePlanner/build.js L5-L17；../Tools/DialoguePlanner/planner.template.html L11]
2. 从节点列表修改台词、回复、立绘ID、信任变化和后继节点，再点「应用节点修改」。新增/复制节点后接好前驱引用；删除会提示仍指向它的节点。已有ID保持稳定。[verified: ../Tools/DialoguePlanner/planner-app.js `renderEditor`、`addNode`、`delete.onclick`]
3. 使用终端排练，检查分支和定位错误；保存项目JSON用于下次打开，导出Unity包用于接入。包内 `yarnSource` 是唯一对白源；「导出 .yarn」是该源的原样副本，不要手改生成的 Runtime Yarn。[verified: ../Tools/DialoguePlanner/planner-app.js L192-L197；../Tools/DialoguePlanner/README.md「策划使用」] [opinion]
4. Unity菜单 `MiniGame/Dialogue Package Importer` →「选择内容包 JSON」→「校验并比较」→「导入并设为当前内容」。结构校验和原生Yarn编译成功后才激活新包；失败保留旧活动内容。导入后从新会话验收修改。[verified: ../UnityProject/Assets/MiniGame/Editor/DialoguePackageImporter.cs L238-L262；terminal-ui-verification.json unity.isolatedImport] [opinion]

演示配置为初始信任0、释放阈值3、净化剩余5。它是独立的HTML对照配置，不替代旧三天配置或正式数值裁决。更改规则/内容版本可能使旧存档不兼容，应保留旧文件并从新会话检查。[verified: ../UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json profile；terminal-ui-verification.json limits] [opinion]

## 支持范围与验证

首版支持带稳定ID的发言、选项、jump、立绘和整数信任变化；普通选项使用 `#id/#trust`，处置使用 `#id/#disposition`。不支持的任意条件、变量和命令必须先扩展导入器；未知语法会报错且保留原文。轮次汇合、循环、目标、立绘、处置反应与32位信任溢出均参与校验；Unity原生编译是最终检查。[verified: ../Tools/DialoguePlanner/planner-core.js `parseSource`、`validateRounds`；../src/MiniGame.Presentation/Authoring/RestrictedYarnParser.cs L128-L164] [opinion]

最新[验证报告](terminal-ui-verification.json)：SDK8.0.424锁定恢复与Release构建通过，0警告/错误；Core75与Presentation110共185项测试通过；策划工具18项测试覆盖1296条选择组合；真实Unity2022.3.23f1c1隔离工程EditMode2项、PlayMode9项通过，Windows Mono构建成功（0错误、1个无图形环境探针警告）。[verified: terminal-ui-verification.json dotnet/planner/unity]

PlayMode包含旧流程4项、终端UI3项、原生对白2项，另检查四种画面比例并审阅捕获图。HTML浏览器视觉、文件下载点击验收仍受本地文件预览策略限制；未据Node测试宣称浏览器验收通过。Player启动结果、源码哈希一致性与其余限制以报告对应字段为准。[verified: terminal-ui-verification.json unity.playModePassed/visualsReviewed/sourceHashComparison/planner.browserVisualVerified/limits] [opinion]

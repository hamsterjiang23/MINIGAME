# 对话工作台

直接打开 [DialoguePlanner.html](DialoguePlanner.html)，从小孩 Day1 示例开始；页面内嵌内容、脚本和样式，不需要命令行、网络服务或安装依赖。[verified: build.js L5-L17；planner.template.html L11]

## 策划使用

1. 打开 HTML；需要接着上次工作时，点击「打开内容包」选择已保存的 JSON。[opinion]
2. 左侧选轮次/节点，在表单修改发言、回复、立绘、信任变化、后继节点；点击「应用节点修改」。已有节点与回复 ID 保持稳定。[verified: planner-app.js L52-L107]
3. 右侧「重置预览」从目录排练，或「从选中节点排练」检查局部。鼠标点选；终端获得焦点后可用 Enter/空格推进、数字1–4选择、上下键定位后 Enter 确认；「查看记录」只展示实际已走内容。[verified: planner-app.js L149-L185、L218-L225]
4. 检查「校验与定位」；点击错误定位源码行。错误阻止 Unity 包导出，长文本和未接入节点给出提示。新增/复制节点后，还需要把前驱的后继目标接到新节点；删除节点会列出仍指向它的引用。[verified: planner-app.js L109-L116、L134-L146、L199-L212]
5. 点击「保存项目 JSON」下载可重新打开的项目文件；点击「校验并导出 Unity 包」下载待导入的内容包。未应用的表单、元数据或源码草稿不能保存，离开页面前会提示；浏览器下载完成后请保留下载文件。[verified: planner-app.js L192-L197、L227]
6. 在 Unity 打开菜单 `MiniGame/Dialogue Package Importer`，选择 JSON，依次点击「校验并比较」和「导入并设为当前内容」。请以导入窗口的最终校验和原生编译结果为准。[verified: ../../UnityProject/Assets/MiniGame/Editor/DialoguePackageImporter.cs L238-L262] [opinion]

「导出 .yarn」下载包内原始 `yarnSource`；对话数据的权威源只有这一份，不维护第二份可独立修改的分支图。源码包含暂不支持的语法时，仍可原样保存项目或导出 Yarn，但表单修改、预览与 Unity 包发布暂停。[verified: planner-app.js L52-L55、L134-L139、L197]

## 范围与演示规则

首版为一个病人的独立 Day1 演示。初始信任0、释放阈值3、净化剩余5随示例包保存；它们是已选定的 HTML 对照演示配置。右侧额外显示的信任、阈值、路径属于策划调试信息。[verified: ../../UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json `profile`；planner-app.js L149-L157]

预览覆盖目录、发言、最多四个回复、实际记录、处置门槛、处置反应、680毫秒花屏、断信号和返回目录；立绘使用标明用途的占位，不代表角色美术已经完成。浏览器预览是受限语法的排练，不能代替 Unity 原生编译、存档恢复及运行验收。[verified: planner-app.js L149-L186；planner.template.html L9] [opinion]

支持节点头 `title / portrait / round / kind / patient / day`；正文为一条「角色名: 发言」加 `<<jump ID>>`，或一组选项及其 jump。普通回复使用稳定 `#id` 和32位整数 `#trust`；措施回复使用 `#id` 和 `#disposition:treat|purify|release`，不能声明 `#trust`（包括0）。节点和回复ID最多128字符，可含点和连字符。表单会转义字面量 `# < > { } [ ]` 和反斜线，未知转义、任意条件、变量和命令会报错。[verified: planner-core.js `parseSource`、`serializeNode`]

各轮回复必须汇合到同一个下一轮入口；处置反应只能经过发言并最终结束。浏览器同时检查不可达部分的循环、最多1024节点/128轮，以及任何回复路线上的32位信任溢出。[verified: planner-core.js `validatePackage`、`validateRounds`]

来源保持为用户提供的 [Day1 HTML 原件](../../docs/assets/prototypes/inspection-terminal-day1-v0.5.2.html) 与 [归档说明](../../docs/sources/inspection-terminal-day1-prototype.md)；内嵌示例来自仓库 [规范内容包](../../UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json)，构建不会修改原件。[verified: build.js L5-L6、L18-L25]

## 开发与验证

从仓库根目录执行以下命令；第一条重建离线 HTML，第二条检查提交的 HTML 是否与源码及示例一致，第三条运行有界数据测试。[verified: build.js L18-L25；planner.test.js 各 `test`]

```powershell
node Tools/DialoguePlanner/build.js
node Tools/DialoguePlanner/build.js --check
node --test Tools/DialoguePlanner/planner.test.js
node --check Tools/DialoguePlanner/planner-core.js
node --check Tools/DialoguePlanner/planner-app.js
```

测试核对57个源节点、项目文件无改动往返、单节点与回复编辑、稳定ID、未知语法保留、缺失目标/循环/立绘错误、添加复制删除、原生契约边界、所有1296条Day1回复组合与三种处置反应。它们是 Node 静态/逻辑验证，不能证明浏览器视觉、文件下载点击或 Unity Play Mode 验收通过。[verified: planner.test.js 各 `test`；Node 测试输出18项通过] [opinion]

# Unity 检定员开发原型

宿主实现门、电脑、床的交互，以及观察、会谈、处置、反应、回房、睡眠、梦境和跨日。`GameRuntime` 调度输入和演出，`GameSession` 保持规则权威；界面读取玩家投影。`[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs InstallRuntime；Assets/MiniGame/Runtime/MiniGameScreen.cs Render]`

2026-10-15飞书来源已更新：主Wiki新增反叛值结局，流程画板新增每日开始前结局/KPI检查，小孩已有完整对白表；当前原型仍保留开发配置。变动与待裁决项见[同步记录](../docs/feishu-sync-2026-10-15.md)。[verified: ../docs/.source-data/game-wiki.json content「结局」；../docs/.source-data/sync-2026-10-15.json changes] [opinion]

默认运行小孩Day1终端演示：已接六轮来源对白、20个回复、三种处置和实际记录。策划使用[独立HTML工具](../Tools/DialoguePlanner/DialoguePlanner.html)导出包，再通过 `MiniGame/Dialogue Package Importer` 校验、原生编译并激活；完整步骤见[终端指南](../docs/terminal-ui-guide.md)。旧三天开发原型需在Play前取消Bootstrap的 **Use Terminal Demo**，开发API为 `StartLegacyPrototype()`。[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs L18、L115、L126；../docs/terminal-ui-verification.json scope] [opinion]

## 运行与操作

策划验收的步骤、剧情分支条件与对白配置方式见[策划验收指南](../docs/planner-acceptance-guide.md)。[opinion]

在 Windows 双击仓库根的 [Open-Unity.cmd](../Open-Unity.cmd)，或在仓库根执行 `./UnityProject/Tools/Open-Project.ps1`。启动器核对 Editor 版本，只为子进程补齐缺失的 `ALLUSERSPROFILE`，并在编译后调用 `OpenPrototype` 打开主场景；已有该工程 Editor 时先提示关闭。日志写到 `Artifacts/Unity/editor.log`。`[verified: Tools/Open-Project.ps1; Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs OpenPrototype]`

已有 Editor 可用菜单 `MiniGame/Open prototype scene` 打开主场景，再按 Play。Build Settings 的首场景用于构建，单独打开工程目录不会保证 Editor 当前场景是主场景；此前实际停留在 `Untitled`。`[verified: ProjectSettings/EditorBuildSettings.asset m_Scenes; ../Artifacts/Unity/EditorRepair/before-editor.log; Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs OpenPrototype]` `[opinion]`

Windows 成品路径为仓库根 `Builds/MiniGame/MiniGame.exe`；构建须在 `unity-windows-build.json` 报告 `Succeeded` 后使用。主场景为 `Assets/MiniGame/Scenes/InspectionApartment.unity`。`[verified: Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs BuildWindows/ScenePath]`

- WASD 移动，鼠标观察；瞄准门、电脑或床，在提示出现时按 E。晨检到门，工作到电脑，夜间及梦后到床。`[verified: Assets/MiniGame/Runtime/ApartmentPlayer.cs Update/RefreshFocus；Assets/MiniGame/Runtime/MiniGameScreen.cs WorldInstruction]`
- 继续按钮或空格推进文本；鼠标或数字 1–4 选择回复。处置按钮按核心许可显示治疗、净化、释放。`[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs Update/ShowResponseButtons/Render]`
- 六类检查展示线索和原创示意；返回或 ESC 关闭。界面不给出真假或善恶标准答案。`[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs BuildInspectionDrawing/Render]`
- ESC暂停；F5保存；F9恢复。默认终端文件为 `Application.persistentDataPath/terminal-demo-save.json`，旧三天为 `prototype-save.json`；使用原子替换，恢复重新绑定Yarn代际和cue token。[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs L29-L30、Save/Load/InstallRuntime]

小孩Day1使用已提供对白；肖像、线索、梦境与结局仍有占位，其他角色与Day2/3尚未由新内容包覆盖。终端演示存档为 `terminal-demo-save.json`，旧三天存档为 `prototype-save.json`；恢复允许当前段台词重播，不承诺逐行位置。[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs L29-L30；../docs/terminal-ui-verification.json scope/limits] [opinion]

## 可重放的本机验证路线

终端切片最新结果：真实Unity2022.3.23f1c1隔离工程原生Yarn导入通过，EditMode2项、PlayMode9项通过；四种比例UI检查及截图审阅已记录，Windows Mono构建0错误/1警告。最新证据及范围见[终端报告](../docs/terminal-ui-verification.json)，以下正常UPM修复与旧四项测试记录保留原日期范围。[verified: ../docs/terminal-ui-verification.json unity] [opinion]

2026-10-15 已修复正常 UPM 启动：缺失的 `ALLUSERSPROFILE` 使 UPM 的 `getGlobalConfigRoot` 收到无效路径；启动器提供 Windows 公共数据目录后，正常工程登记14个包。另补图片编码模块，并将内嵌 Yarn 测试的包查询改为 Unity 2022 支持的 API。正常工程四项图形 PlayMode 测试通过；最新范围见[验证汇总](../docs/gameplay-migration-verification.json)的 `unity.normalUpmRecovery`。`[verified: ../Artifacts/Unity/EditorRepair/trace-upm-stderr.log L3-L10; ../Artifacts/Unity/editor.log; ../Artifacts/Unity/EditorRepair/normal-playmode-results.xml test-run]`

以下离线路线保留为历史验证和备用入口；它依赖本机缓存，不能据此推导新机器已可直接运行。`[verified: Tools/Invoke-OfflineValidation.ps1 Prepare-OfflineProject]` `[opinion]`

```powershell
# 在仓库根运行；只启动隔离工程的 Editor，并等待准确退出码。
& './UnityProject/Tools/Invoke-OfflineValidation.ps1' -Action All -CaptureVisuals -EditorPath 'E:/Unity/Editors/2022.3.23f1/Editor/Unity.exe' -CachedPackagesRoot 'C:/Users/admin/AppData/Local/Unity/cache/packages/packages.unity.com'
```

脚本复制原工程、核心源码、固定 Yarn 3.2.8 与本机缓存，检查版本与正常 manifest 一致并保留许可证和 `.meta`；只在离线副本中裁掉可选 Yarn 引用并启用真实 TMP。它使用隐藏 `Start-Process` 和 30 秒 `WaitForExit` 分段等待，不改编辑器安装或其他工程。`[verified: Tools/Invoke-OfflineValidation.ps1 Get-PackageCopy/Prepare-OfflineProject/Invoke-OfflineEditor]`

`All` 依次准备、生成场景、运行 Play Mode、构建 Windows；独立 `PlayMode` 也先生成场景和登记 Build Settings。生成场景、材质、内容、GUID 与 Build Settings 回写正常工程；`MINIGAME_REPOSITORY_ROOT` 保持根 `Builds/` 输出位置。`[verified: Tools/Invoke-OfflineValidation.ps1 Action分支/Publish-GeneratedAssets；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs RepositoryRoot]`

结果位于 `Artifacts/Unity/`：`offline-project-provenance.json`、`offline-create.log`、`offline-playmode-results.xml`、`offline-build.log`、`unity-windows-build.json`。可选图像为 `Visuals/world.png`、`terminal.png`、`eyes.png`、`pause-sleep.png`。应读取当次结果，不能只看文件存在。`[verified: Tools/Invoke-OfflineValidation.ps1；Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs CaptureIfRequested]`

四项 PlayMode 测试覆盖场景、中文字体、输入排他，真实 Yarn 按钮完成三天与六类观察，旧 Continue/Reply 回调失效，以及视觉恢复、终端镜头、睡眠最终帧暂停和暂停遮挡层。`[verified: Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs 四项UnityTest定义]`

正常 Editor 菜单 `MiniGame/Create and validate prototype` 会重新生成原型资源；仅打开现有场景请用 `MiniGame/Open prototype scene`。公开批处理方法为 `MiniGame.UnityHost.Editor.ReferencePrototypeBuilder.CreateAndValidate` / `BuildWindows`；本次未重新构建 Player。`[verified: Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs 对应公开方法; ../docs/gameplay-migration-verification.json unity.normalUpmRecovery]`

## 技术与来源边界

本轮为 Unity 2022.3.23f1c1、Built-in、Standard 材质和 uGUI；URP 是后续候选，迁移时要重做材质和照明并重新验证。`[verified: ProjectSettings/ProjectVersion.txt m_EditorVersion；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs ConfigureProject/Material]` `[opinion]`

Unity 2022.3 的 `.NET Standard` API 配置对应 2.1，Editor 枚举保留历史名字 `NET_Standard_2_0`。[官方说明](https://docs.unity3d.com/2022.3/Documentation/Manual/dotnetProfileSupport.html)。`[verified: 官方说明 ".NET Standard 2.1"；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs SetApiCompatibilityLevel]`

正常工程使用 `Packages/dev.yarnspinner.unity` 的固定参考版本副本，保留许可证和 GUID，只调整可选引用；参考子模块保持原样。只播放本工程原创 `.yarn`；隐藏数值和处置资格由核心裁决。`[verified: Packages/manifest.json dev.yarnspinner.unity；Packages/dev.yarnspinner.unity/package.json version；Assets/MiniGame/Editor/GeneratePrototypeYarn.cs Generate；Assets/MiniGame/Yarn/MiniGameYarnBridge.cs SelectResponse]`

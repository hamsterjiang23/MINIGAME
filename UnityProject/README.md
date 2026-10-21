# Unity 检定员开发原型

海报、物件与门口监督员的最新输入规则见[世界阅读与监督员交互](../docs/world-dialogue/guide.md)：保留现有美术，采用专注阅读、单次推进和结束恢复；电脑角色对话继续使用原交互。当前测试及Mono体验包见[验证记录](../docs/world-dialogue/verification.json)。`[verified: ../docs/world-dialogue/guide.md；../docs/world-dialogue/verification.json]`

策划灯光入口为 **MiniGame → 画面 → 房间灯光**：可在 Scene／Game 旁直接编辑，支持场景手柄、播放／暂停调参、撤销与退出播放自动保存。操作见[灯光编辑指南](../docs/lighting-tools/guide.md)。调色入口为 **MiniGame → 画面 → 调色与像素风格**，Scene／Game 均可预览。`[verified: Assets/MiniGame/Editor/RoomLightingWindow.cs Open/OnGUI/DuringSceneGUI；Assets/MiniGame/Editor/RoomLightingAuthoring.cs OnPlayModeChanged；Assets/MiniGame/Runtime/RetroScreenEffect.cs ImageEffectAllowedInSceneView]`

Norm 标志与监控局部扰动通过游戏内 **F6** 调参，支持两组独立参数、冻结预览、重置和保存；编辑器入口为 **MiniGame → 画面 → 标志与监控扰动**。操作及验证见[扰动指南](../docs/terminal-signal/guide.md)。`[verified: Assets/MiniGame/Runtime/TerminalSignalController.cs Update/SaveSettings；Assets/MiniGame/Editor/TerminalSignalWindow.cs Open]`

2026-10-19 默认房间改为参考宿舍的9.70078×4.41176×3.12066米布局，加入Word参考的两张交互海报、可配置60°房门与只能从房内观看的3D连廊。操作与配置见[参考房间指南](../docs/reference-room-guide.md)，验收见[参考房间报告](../docs/reference-room-verification.json)。[verified: ../docs/reference-room-guide.md；../docs/reference-room-verification.json]

当前游戏入口为 [MainRoomCombined.unity](Assets/MiniGame/Scenes/MainRoomCombined.unity)：融合房间已接入门、电脑和床的实体交互，保留窗外水泥墙与台沿。电脑有推镜、CRT开机、UI渐显和退出回位；交互与验证见[本轮报告](../docs/main-room-gameplay-verification.json)，源文件和取舍见[融合说明](../Art/MainRoom/combined/README.md)。[verified: ../docs/main-room-gameplay-verification.json scene/interactions/presentation]

第一版美术预览保留在 [MainRoomPreview.unity](Assets/MiniGame/Scenes/MainRoomPreview.unity)，原灰盒玩法场景保留在 `InspectionApartment.unity`。融合版美术重建菜单 `MiniGame/Room Art/Import and open combined room` 会恢复玩法装配。[verified: Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs ImportCombinedAndOpen/Build；Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs ConfigureOpenScene]

宿主实现门、电脑、床的交互，以及观察、会谈、处置、反应、回房、睡眠、梦境和跨日。`GameRuntime` 调度输入和演出，`GameSession` 保持规则权威；界面读取玩家投影。`[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs InstallRuntime；Assets/MiniGame/Runtime/MiniGameScreen.cs Render]`

2026-10-15飞书来源已更新：主Wiki新增反叛值结局，流程画板新增每日开始前结局/KPI检查，小孩已有完整对白表；当前原型仍保留开发配置。变动与待裁决项见[同步记录](../docs/feishu-sync-2026-10-15.md)。[verified: ../docs/.source-data/game-wiki.json content「结局」；../docs/.source-data/sync-2026-10-15.json changes] [opinion]

全流程入口使用 **MiniGame/Campaign/Import and activate**：校验六份Excel转换的994行对白、原生编译Yarn、导入27张立绘，并为 `MainRoomCombined` 启用全流程。完整操作和暂定解释见[全流程指南](../docs/campaign-guide.md)。`InspectionApartment` 保留小孩Day1演示；开发API为 `StartCampaign()`、`StartTerminalDemo()`、`StartLegacyPrototype()`，三种入口分别保留。`[verified: Assets/MiniGame/Editor/CampaignBuilder.cs ImportAndActivate/ImportPortraits；Assets/MiniGame/Runtime/MiniGameBootstrap.cs StartCampaign/StartTerminalDemo/StartLegacyPrototype；../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json sourceRowCount/declaredPortraits]`

## 运行与操作

睡前视觉引导：当天工作结束并退出终端后，仅床被天花板聚光灯照亮，光柱内有飞虫；梦后再次使用同样引导，次日恢复房间。可通过 `MiniGame/Main Room/Preview bedtime lighting` 直接预览，说明与验证见[睡前效果](../docs/bedtime-guidance.md)。[verified: Assets/MiniGame/Runtime/BedtimeGuidance.cs；Assets/MiniGame/Editor/BedtimePreview.cs]

策划验收的步骤、剧情分支条件与对白配置方式见[策划验收指南](../docs/planner-acceptance-guide.md)。[opinion]

在 Windows 双击仓库根的 [Open-Unity.cmd](../Open-Unity.cmd)，或执行 `./UnityProject/Tools/Open-Project.ps1`。启动器核对 Editor 版本、补齐子进程启动环境，编译后调用 `MainRoomGameplayBuilder.OpenAndPlay` 打开融合场景并进入 Play；已有该工程 Editor 时先提示关闭。日志写到 `Artifacts/Unity/editor.log`。[verified: Tools/Open-Project.ps1 startInfo/scenePath；Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs OpenAndPlay]

已有 Editor 时，先退出 Play，再使用 `MiniGame/Main Room/Open and play`。需要重装碰撞、镜头和宿主时使用 `MiniGame/Main Room/Setup and play`。全流程Day0直接从电脑教学开始；正式Day1–3先到门口按 E 完成晨检与简报。瞄准电脑按 E 或鼠标左键，角色落座后镜头对齐并推进到屏幕，淡入终端；在名册自由选择当日待审角色。`[verified: Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs SetupAndPlay/OpenAndPlay；Assets/MiniGame/Runtime/ApartmentPlayer.cs Update；Assets/MiniGame/Runtime/PresentationEffects.cs SeatedEntry；../src/MiniGame.Presentation/GameRuntime.cs EnterSelectedPatient]`

全流程Windows开发构建菜单为 `MiniGame/Campaign/Build Windows development player`，目标为融合场景，输出 `Builds/Campaign/MiniGame.exe`；构建与启动是否通过以[全流程报告](../docs/campaign-verification.json)为准。旧 `Builds/MiniGame/MiniGame.exe` 属于此前灰盒原型构建。`[verified: Assets/MiniGame/Editor/CampaignBuilder.cs BuildWindows；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs BuildWindows/ScenePath]`

- WASD 移动，鼠标观察；瞄准门、电脑或床，在提示出现时按 E。晨检到门，工作到电脑，夜间及梦后到床。`[verified: Assets/MiniGame/Runtime/ApartmentPlayer.cs Update/RefreshFocus；Assets/MiniGame/Runtime/MiniGameScreen.cs WorldInstruction]`
- 全流程逐字显示：继续按钮、Enter或空格先补全当前页，再翻页或推进；鼠标或数字1–4选择回复，上下键定位后Enter确认。处置按钮按核心许可显示。`[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs PresentDialogueLine/HandleTerminalKey；Assets/MiniGame/Runtime/DialogueTextPlayback.cs Advance]`
- 旧三天原型保留六类检查及原创示意；HTML终端演示隐藏这排入口，保留监控状态栏。检查弹窗返回或ESC关闭。`[verified: Assets/MiniGame/Runtime/TerminalView.cs Render；Assets/MiniGame/Runtime/MiniGameScreen.cs BuildInspectionDrawing]`
- ESC先关闭历史记录，否则暂停；F5保存、F9恢复。全流程保存为 `Application.persistentDataPath/campaign-save.json`，V1演示为 `terminal-demo-save.json`，旧三天为 `prototype-save.json`；恢复重新绑定Yarn代际和cue token。`[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs SavePath/Update/Save/Load/InstallRuntime]`

全流程包含五名正式角色的Day1–3对白和Day0教学，立绘由 [人物资源目录](../Art/Characters/README.md) 导入；晨检、梦境和结局沿用现有演出。全流程读档跳过已完成的句子，从当前未完成句的第一页重新显示。V1编辑仍使用[独立HTML工具](../Tools/DialoguePlanner/DialoguePlanner.html)，该工具拒绝修改V2全流程包。`[verified: Assets/MiniGame/Content/Campaign/provenance.json meetingCount；Assets/MiniGame/Editor/CampaignBuilder.cs ImportPortraits；Assets/MiniGame/Yarn/MiniGameYarnBridge.cs PresentLineAsync；../Tools/DialoguePlanner/planner-core.js assertEditablePackage]`

## 可重放的本机验证路线

全流程内容、原生Yarn、项目PlayMode、截图及Windows开发构建的当前结果见[全流程验收报告](../docs/campaign-verification.json)；下面的切片验证保留历史日期和范围。`[opinion]`

2026-10-18终端对齐结果：正常Unity2022.3.23f1c1工程14项项目PlayMode通过；工业风控件、键盘选择、五行历史、局部断信号和归档恢复已检查，进出终端动画源码保持不变。九张Unity捕获图与验证范围见[对齐报告](../docs/terminal-alignment-verification.json)；本轮未重建Player。此前隔离工程、构建及UPM修复结果保留原日期范围。[verified: ../docs/terminal-alignment-verification.json unity/preservedSources/limits]

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

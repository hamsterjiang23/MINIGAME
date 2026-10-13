# Unity 检定员开发原型

宿主实现门、电脑、床的交互，以及观察、会谈、处置、反应、回房、睡眠、梦境和跨日。`GameRuntime` 调度输入和演出，`GameSession` 保持规则权威；界面读取玩家投影。`[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs InstallRuntime；Assets/MiniGame/Runtime/MiniGameScreen.cs Render]`

## 运行与操作

Windows 成品路径为仓库根 `Builds/MiniGame/MiniGame.exe`；构建须在 `unity-windows-build.json` 报告 `Succeeded` 后使用。主场景为 `Assets/MiniGame/Scenes/InspectionApartment.unity`。`[verified: Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs BuildWindows/ScenePath]`

- WASD 移动，鼠标观察；瞄准门、电脑或床，在提示出现时按 E。晨检到门，工作到电脑，夜间及梦后到床。`[verified: Assets/MiniGame/Runtime/ApartmentPlayer.cs Update/RefreshFocus；Assets/MiniGame/Runtime/MiniGameScreen.cs WorldInstruction]`
- 继续按钮或空格推进文本；鼠标或数字 1–4 选择回复。处置按钮按核心许可显示治疗、净化、释放。`[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs Update/ShowResponseButtons/Render]`
- 六类检查展示线索和原创示意；返回或 ESC 关闭。界面不给出真假或善恶标准答案。`[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs BuildInspectionDrawing/Render]`
- ESC 暂停；F5 保存；F9 恢复。文件为 `Application.persistentDataPath/prototype-save.json`，使用原子替换；恢复重新绑定 Yarn 代际和 cue token。`[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs Update/Save/Load/InstallRuntime]`

对白、肖像、线索、梦境和结局演出持续标记开发占位，正式内容仍需制作。`[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs PrototypeDisclaimer/EndingName]` `[opinion]`

## 可重放的本机验证路线

本机正常 UPM 导入失败，日志记录 `path undefined`；本轮实际验证采用隔离的 `Artifacts/Unity/OfflineProject` 和 `-noUpm`。离线成功不能写成正常 UPM 导入通过。`[verified: ../Artifacts/Unity/editor-create.log "Failed to resolve packages"；Tools/Invoke-OfflineValidation.ps1 Invoke-OfflineEditor]`

```powershell
# 在仓库根运行；只启动隔离工程的 Editor，并等待准确退出码。
& './UnityProject/Tools/Invoke-OfflineValidation.ps1' -Action All -CaptureVisuals -EditorPath 'E:/Unity/Editors/2022.3.23f1/Editor/Unity.exe' -CachedPackagesRoot 'C:/Users/admin/AppData/Local/Unity/cache/packages/packages.unity.com'
```

脚本复制原工程、核心源码、固定 Yarn 3.2.8 与本机缓存，检查版本与正常 manifest 一致并保留许可证和 `.meta`；只在离线副本中裁掉可选 Yarn 引用并启用真实 TMP。它使用隐藏 `Start-Process` 和 30 秒 `WaitForExit` 分段等待，不改编辑器安装或其他工程。`[verified: Tools/Invoke-OfflineValidation.ps1 Get-PackageCopy/Prepare-OfflineProject/Invoke-OfflineEditor]`

`All` 依次准备、生成场景、运行 Play Mode、构建 Windows；独立 `PlayMode` 也先生成场景和登记 Build Settings。生成场景、材质、内容、GUID 与 Build Settings 回写正常工程；`MINIGAME_REPOSITORY_ROOT` 保持根 `Builds/` 输出位置。`[verified: Tools/Invoke-OfflineValidation.ps1 Action分支/Publish-GeneratedAssets；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs RepositoryRoot]`

结果位于 `Artifacts/Unity/`：`offline-project-provenance.json`、`offline-create.log`、`offline-playmode-results.xml`、`offline-build.log`、`unity-windows-build.json`。可选图像为 `Visuals/world.png`、`terminal.png`、`eyes.png`、`pause-sleep.png`。应读取当次结果，不能只看文件存在。`[verified: Tools/Invoke-OfflineValidation.ps1；Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs CaptureIfRequested]`

四项 PlayMode 测试覆盖场景、中文字体、输入排他，真实 Yarn 按钮完成三天与六类观察，旧 Continue/Reply 回调失效，以及视觉恢复、终端镜头、睡眠最终帧暂停和暂停遮挡层。`[verified: Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs 四项UnityTest定义]`

正常 Editor 菜单为 `MiniGame/Create and validate prototype`；公开批处理方法为 `MiniGame.UnityHost.Editor.ReferencePrototypeBuilder.CreateAndValidate` / `BuildWindows`。正常 UPM 环境修复后需要另行验证该路线。`[verified: Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs 对应公开方法]` `[opinion]`

## 技术与来源边界

本轮为 Unity 2022.3.23f1c1、Built-in、Standard 材质和 uGUI；URP 是后续候选，迁移时要重做材质和照明并重新验证。`[verified: ProjectSettings/ProjectVersion.txt m_EditorVersion；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs ConfigureProject/Material]` `[opinion]`

Unity 2022.3 的 `.NET Standard` API 配置对应 2.1，Editor 枚举保留历史名字 `NET_Standard_2_0`。[官方说明](https://docs.unity3d.com/2022.3/Documentation/Manual/dotnetProfileSupport.html)。`[verified: 官方说明 ".NET Standard 2.1"；Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs SetApiCompatibilityLevel]`

正常工程使用 `Packages/dev.yarnspinner.unity` 的固定参考版本副本，保留许可证和 GUID，只调整可选引用；参考子模块保持原样。只播放本工程原创 `.yarn`；隐藏数值和处置资格由核心裁决。`[verified: Packages/manifest.json dev.yarnspinner.unity；Packages/dev.yarnspinner.unity/package.json version；Assets/MiniGame/Editor/GeneratePrototypeYarn.cs Generate；Assets/MiniGame/Yarn/MiniGameYarnBridge.cs SelectResponse]`

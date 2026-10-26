# Windows 外发包

统一入口为 **MiniGame → Build → Build Windows external release**。该入口使用 Windows x64、Mono、非 Development 构建，产品名为 `NORM`，包含主房间、旧公寓和海滩结局三个场景。缺失监督员剧情依赖或统一图标时直接停止构建。[verified: ../UnityProject/Assets/MiniGame/Editor/ReleasePlayerBuilder.cs ApplySettings/Build；../UnityProject/Assets/MiniGame/Editor/StoryBuildValidation.cs Validate]

统一游戏图标使用原外发包的监督员头像。原始 Windows 多尺寸资源保存在 [NormGameIcon.ico](../Art/Branding/NormGameIcon.ico)，Unity 使用 [NormGameIcon.png](../UnityProject/Assets/MiniGame/Art/Branding/NormGameIcon.png)；来源、尺寸及哈希见 [图标来源](../Art/Branding/norm-game-icon-source.json)。这是游戏程序图标，开场使用的横版 NORM 标志另行保留。[verified: ../Art/Branding/norm-game-icon-source.json source/sizes/pngSha256；ReleasePlayerBuilder.IconPath]

正常启动从 `Display.main.systemWidth/systemHeight` 读取显示器原生尺寸，请求无边框全屏，并等待实际渲染尺寸连续三帧匹配；即使同名 Player 记住了 1280×720 等旧分辨率，也不把它当成显示器原生尺寸。首帧切换不会立即完成，因此同时保留启动后有限帧数的确认与日志。玩家启动后仍可用 Alt+Enter 切换窗口。Editor 和 Development 构建不强制；有图形设备的 `-batchmode` 与普通启动共用这条路径，`-nographics` 或明确提供 `-screen-fullscreen`、`-screen-width`、`-screen-height`、`-window-mode` 时保留显式选择。[verified: ../UnityProject/Assets/MiniGame/Runtime/ReleaseDisplayStartup.cs Initialize/ApplyNativeResolution/Start/ShouldUseNativeResolution；ReleasePlayerBuilder.ApplySettings]

Unity 将显示器原生尺寸和实际渲染尺寸分开暴露：[Display.systemWidth](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Display-systemWidth.html)。[Screen.currentResolution](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen-currentResolution.html) 只保证窗口模式下返回桌面分辨率，不能用它纠正已经处于低分辨率全屏的缓存；[SetResolution](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Screen.SetResolution.html) 在帧末生效。[verified: 链接中的 Unity 2022.3 官方 API 文档]

输出可通过 `MINIGAME_RELEASE_OUTPUT` 指定，构建报告通过 `MINIGAME_RELEASE_REPORT` 指定；默认分别为 `Builds/NORM/NORM.exe` 和 `Artifacts/Packages/NORM/windows-build.json`。报告包含实际图标资产路径、全屏模式、原生分辨率、构建错误与警告。[verified: ReleasePlayerBuilder.Build]

交付时应从最终 ZIP 解压启动，分别检查程序图标、正常启动全屏、旧低分辨率状态后的重新启动、运行日志和逐文件哈希。图形启动验收不再传入 1280×720 等测试尺寸；需检查 `NORM_DISPLAY_READY` 的 render 与 monitor 尺寸相同，而不只检查无异常。窗口截图和人工观感验收仍需分别记录。[opinion]

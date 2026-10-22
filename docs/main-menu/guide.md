# NORM 主菜单与设置

没有继续记录或手动存档时，启动显示 NORM 标志和“按任意键开始游戏”。已有记录时直接显示“继续检查 / 开始新的检查期 / 设置”；从 Esc 暂停菜单选择“回到主菜单”也进入这三个选项。`[verified: MainMenuController.ShowStartup/ReturnToMenu；MainMenuFlowPlayModeTests]`

同一次运行中，“继续检查”保留原会话、相机、对白打字与梦境计时。返回主菜单会保存独立的 `campaign-continue.json`；重启后按自动继续、梦境手动存档、旧手动存档的顺序读取。损坏的自动记录会报错，不会悄悄回退到旧进度。开始新检查期需要确认，先原子写入新 Day0 记录，再重新载入房间；手动存档和个人设置保留。`[verified: MainMenuController.Continue/BeginNewInspection；MainMenuContinueStore.ResolveResumePath；MainMenuPersistencePlayModeTests]`

主菜单和暂停菜单共用设置页：亮度 50–150%（默认 100%），总音量 0–100%（默认 100%），复古高／中／低对应像素大小 8／4／2（默认低）。修改即时生效并保存在本机，不修改昼夜美术配置。梦境播放继续绕过复古处理，亮度保持纯黑为黑色。`[verified: PlayerPreferences；PlayerSettingsView；PlayerBrightnessEffect；PlayerGraphicsSettings.PixelDetail/DefaultDetail]`

字体以现有暂停菜单“继续检查”为基准：使用同一个 `GameUiTypography.CreateFont()` 入口，菜单按钮与标题统一为 28 号常规字重，辅助文字保持同一字族。NORM 图形标志使用已有图片。`[verified: MainMenuFlowPlayModeTests.AssertSharedTypography；MainMenuView.ConfigureText；PauseMenuView.MakeText；PlayerSettingsView.TextAt]`

预览：[首次启动](evidence/first-start-1920x1080.png)、[主菜单](evidence/main-menu-1920x1080.png)、[设置](evidence/settings-1920x1080.png)。同目录保留 1440p 截图。`[verified: verification.json images]`

本轮去重 39 项 Unity 2022 PlayMode 检查通过，覆盖真实 Esc 按钮、原生 Day1 对白、梦境／睡眠恢复、音频续播、设置、字体和已有暂停界面回归。各次原始报告及最终逐测试结果见 [verification.json](verification.json)；前期测试夹具问题和修正保留在报告中。未生成新的独立 Windows Player。`[verified: verification.json latestPassed/latestFailed/latestCases/limitations]`

自动化或开发捕获可显式传入 `-minigameDirectPlay` 跳过启动页；正常启动不传该参数。主菜单集成测试使用独立临时继续目录，不覆盖玩家存档。`[verified: MainMenuController.ResetStatics；MainMenuFlowPlayModeTests.LoadTitle]`

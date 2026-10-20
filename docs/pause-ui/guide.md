# 暂停界面

运行 **MainRoomCombined** 后按 **Esc** 查看暂停菜单：只保留“暂停”与四个操作，移除日期／地点及常驻底部提示；暖白文字与磨损边框向右轻微收缩透视，背景冻结、柔焦并压暗边缘。海滩终局沿用共享视觉组件，保留其现有操作列表。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs Build；../../UnityProject/Assets/MiniGame/Runtime/PauseMenuPerspective.cs Project；../../UnityProject/Assets/MiniGame/Runtime/PauseMenuBackdrop.cs CaptureFrame；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface]`

- 鼠标悬停或方向键选择，Enter／Space 确认；Esc 继续。主场景保留 F5 保存、F9 读取及重新开始入口。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuButton.cs OnPointerEnter；../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs RefreshNavigation；../../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs Update]`
- 保存／读取结果显示在菜单下方；从菜单继续或读取后，同一次确认键不会推进底层对白。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs Notify/Bind/HandleTerminalKey/Render；../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs SaveFromPause/LoadFromPause]`
- 单独打开海滩预览时，保存／读取置灰；正式会话进入海滩后使用已有存档服务。暂停时收起海滩字幕与操作提示，恢复后继续。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface/ApplyPause]`

参考《寻找伪人》的浅色悬停反馈和独立于游戏时间的淡入淡出；背景和边框由当前工程程序生成。`[verified: ../../research/no-im-not-a-human/RecoveredUnityProject/ExportedProject/Assets/MonoBehaviour/Pause_UI.asset L16–29；../../research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/Infrastructure/Pause/PauseMenuView.cs L142、L158；../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs CreateTextures/Update]`

验证范围、测试名称与截图路径见 [verification.json](verification.json)，实现中的边界记录见 [implementation-notes.md](implementation-notes.md)。`[verified: verification.json tests/visual；implementation-notes.md「Discovered edge cases」]`

透视与柔焦修订的当前验证见 [perspective-verification.json](perspective-verification.json)。指针命中使用与画面一致的逆透视；每次打开暂停仅截取一次背景，批处理模式跳过截图，渲染等待超过0.5秒后恢复菜单淡入。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuHitArea.cs Raycast；../../UnityProject/Assets/MiniGame/Runtime/PauseMenuBackdrop.cs Capture/Update]`

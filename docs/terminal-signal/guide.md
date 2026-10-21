# Norm 标志与监控扰动

游戏运行时按 **F6** 打开右侧调参面板，按 **F6 / Esc** 或点击关闭收起。打开面板时鼠标、对白快捷键和开场点击由面板独占；扰动仍实时播放，可勾选“冻结扰动”定格比较。`[verified: TerminalSignalController.Update/SetPanelOpen；MiniGameScreen.HandleTerminalKey/RequestDayZeroOpening；ApartmentPlayer.Update]`

- **Norm 标志**：透明标志的间歇横条错位、RGB 分离和信号噪声。开场结束后，该画面自动改用监控参数。`[verified: TerminalView.SetOpeningLogo；TerminalSignalDistortion.shader frag]`
- **监控画面**：人物监控、Day0 监管者和本人复核共用一组扫描线、滚动干扰带及突发错位参数。终端正文和按钮不使用该局部扰动材质。`[verified: TerminalView 构造函数 signals.Attach 的三个调用]`
- **保存两组参数**：写入 `Application.persistentDataPath/terminal-signal-settings.json`，下次启动载入。未点击保存的调整只保留到本次游戏结束；“重置当前组”恢复项目默认值。`[verified: TerminalSignalController.SaveSettings/LoadSettings/ResetGroup]`

编辑器入口是 **MiniGame → 画面 → 标志与监控扰动**。Play Mode 编辑运行实例，停止播放时编辑 [默认配置](../../UnityProject/Assets/MiniGame/Resources/MiniGame/TerminalSignalDefault.asset)；已有个人保存参数优先于项目默认值。`[verified: TerminalSignalWindow.OnGUI；TerminalSignalController.Initialize/LoadSettings]`

“横向撕裂”和“RGB 分离”按画面宽度比例控制；扫描线速度是每秒移动的条纹间距，滚动干扰速度是每秒穿过的画面高度。故障时长最多占一次周期的 80%，保留清晰间隙。`[verified: TerminalSignalTuningPanel.Build；TerminalSignalDistortion.shader scanPhase/bandDistance/duty]`

“启用扰动”关闭的是新增的局部扰动层。人物监控原有的调色、噪点与固定扫描纹仍由人物监控配置及信号 Shader 控制。`[verified: PatientSurveillanceFeed.RenderFrame；PatientMonitorSignal.shader frag]`

验证结果见 [verification.json](verification.json)，实际捕获与测试原始输出位于仓库 `Artifacts/TerminalSignal/`。`[verified: verification.json evidenceRoot]`

![Norm 标志扰动](norm-preview.png)

![监控画面与实时调参面板](monitor-panel.png)

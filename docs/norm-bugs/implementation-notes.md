# Deviations

- 用户确认继续暂缓灯光和新梦境素材，本轮核对既有修复并补齐其余BUG与截图要求。`[verified: 本轮用户选择]`
- 晨检沿用现有文本，只移除开发占位标记；监督员使用文档内立绘，单独保存原图与透明处理来源。`[verified: MiniGameScreen.WorldCueText；Art/Characters/Supervisor/provenance.json]`
- 用户进一步明确监督员只能在门外，改为门外世界空间立绘，通过真实门体遮挡；移除室内HUD人物覆盖，白框对白留在画面下方。`[verified: 本轮用户“监督员只能在门外不能再场景内”]`

# Discovered edge cases

- 原每日净化要求被UI显示为剩余配额；新增只读投影读取同一当日记录，不能从剩余额反推或硬编码为1。`[verified: GameSession.GetPlayerView；PlayerView.PurificationsToday/PurificationRequirement]`
- 选项误触扫描音由进入复核/检查页触发，现改为晨检cue一次；开门镜头原会额外前进2米。`[verified: MiniGameAudioDirector.beganMorningScan；audio-door-notes.md]`
- 操作手册保持暂停，只允许返回菜单；Esc先关手册，再解除暂停，F5/F9在手册打开时不执行。`[verified: MiniGameBootstrap.HandleEscape/Update；PauseMenuView.ShowManual/TryCloseManual]`
- 主场景普通复古配置关闭量化，原有Bayer抖动无法作为独立胶片颗粒；新增单色颗粒强度，保留原调色与灯光。`[verified: grain-notes.md]`
- 追加要求将监督员改为常驻门外并上移0.1米，取消开门和晨检阶段的显示条件。Day0坐姿时使用演出记录的站立返回位置作为地面基准；开门、工作和暂停不移动人物。`[verified: SupervisorDoorwayView.cs Bind/PlaceAtDoorway；reports/supervisor-presence/supervisor-tests.json；reports/supervisor-presence/world-presentation-tests.json]`
- 另一项对白更新在验证期间修改了内容包和编译器；最终隔离副本重新同步并逐文件校验哈希，单独记录受测快照，随后复跑核心与相关 PlayMode。`[verified: Artifacts/NormBugs/Validation/tested-source-snapshot.json；dotnet-test.log]`
- 一次隔离测试在编译重载后未退出，后续构建因同一工程占用被拒绝；终止经路径核对的隔离 Editor 后串行重跑，不计入通过结果。`[verified: 本轮进程核对 OfflineProject PID24340；后续 world-kpi.xml passed=6]`
- 最终相关 PlayMode 通过，但旧原型 Yarn 的“倾”未被字库生成器纳入。将受打包的 Yarn 正文加入现有字库输入，继续使用原 HTML 栅格规则并逐字节保持供应包字形。`[verified: final-playmode.log missing exported glyph U+503E；extend-glyph-atlas.cjs collectSources]`

# Questions for review

- 灯光与新梦境素材仍待用户后续提供，不纳入本轮完成项。`[verified: 用户选择“先修其余问题，灯光和新素材暂缓”]`

变更偏差：3 项（门外监督员按追加要求调整、同步同期对白更新、补齐旧 Yarn 字形）。`[verified: 本文 Deviations 与 Discovered edge cases]`
最可能回看：监督员门口构图与颗粒强度，灯光／新梦境素材仍暂缓。`[opinion]`
发现边界：真实净化计数、扫描音归属、门口停止推进、手册锁定、坐姿地面、字库输入和隔离进程占用。`[verified: 本文 Discovered edge cases]`
最终验证：327 项 .NET、47 项相关 PlayMode、28 项字体 EditMode、非开发版 Mono 构建与三组原生 Player 检查；原始报告见 verification.json。`[verified: verification.json dotnet/unity/nativePlayers]`
后续先读：guide.md、issues.json、verification.json，再看 SupervisorDoorwayView 与 MiniGameScreen。`[opinion]`

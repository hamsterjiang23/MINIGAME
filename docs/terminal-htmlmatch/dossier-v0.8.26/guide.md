# 终端角色档案 v0.8.26

目录档案区采用 [v0.8.26 HTML](../reference-v0.8.26.html) 的原生布局和资源包肖像；五人的年龄、职业、摘要取自 [角色信息缩略版 DOCX](../../../Art/TerminalDossier/Source/角色信息缩略版.docx)。原始文件哈希与角色绑定见 [来源清单](source-manifest.json)。`[verified: source-manifest.json sources、profiles、importedAssets]`

## 数据与显示

| 游戏角色 ID | 档案肖像 | 年龄 | 职业 |
|---|---|---|---|
| child | ML01 | 8岁 | 无。`[verified: source-manifest.json profiles]` |
| priest | JN02 | 67岁 | 神父。`[verified: source-manifest.json profiles]` |
| former-ruler | VV01 | 35岁 | `***`。`[verified: source-manifest.json profiles]` |
| diplomat | AD01 | 28岁 | 外交官。`[verified: source-manifest.json profiles]` |
| battlefield-doctor | ED02 | 58岁 | 前战地医生。`[verified: source-manifest.json profiles]` |
| self-review | SJ01 | 未登记 | 未登记。`[verified: source-manifest.json profiles]` |
| tutorial | 沿用现有肖像 | 未登记 | 未登记。`[verified: source-manifest.json profiles；TerminalView.RenderRoster]` |

五份摘要逐字保留 DOCX，包括标点和星号。目录姓名、名单、阶段、状态、剩余净化配额继续来自运行时；档案配置不参与会谈表情切换。`[verified: Tools/TerminalDossier/import_sources.py extract_profiles；TerminalView.RenderRoster]`

800×600 原生坐标：肖像 `(31,323,96,128)`；年龄 `(490,348,288,26)`；职业 `(490,382,288,26)`；摘要 `(490,416,288,56)`。底图为无正文目录框架，原图肖像使用 Point、白色 Tint、无压缩、无 Mipmap；档案配额使用 56×24 数字图集。`[verified: TerminalView 构造目录区；TerminalDossierSetup.ImportTexture；TerminalQuotaField.ReadMetrics]`

摘要使用当前方舟字体、14px 字号、20px 行距，保持两行窗口。鼠标在框内时每次滚轮移动一行；溢出时右侧显示上下箭头；边界停止。切换角色、重新进入目录或重新绑定存档时回首行，同角色状态刷新保留位置。方向键继续选择角色，单击目录行、当前命令或 Enter 沿用进入复核。`[verified: TerminalDossierSummary.SetContent、OnScroll、Scroll；TerminalView.ResetNavigation、Render、HandleDirectoryKey]`

字形补齐“暴、残、母、龄”的七档字号，原有 10,353 个字形的像素与度量保持一致。`[verified: font-preservation.json preservedGlyphs、addedGlyphs、pixelOrMetricChanges]`

## 导入与维护

运行 `Tools/TerminalDossier/import_sources.py` 并提供 `--package`、`--document`、`--html`；加 `--check` 可只读核对。随后在停止 Play Mode 的 Unity 中执行 **MiniGame → 终端 → 接入 v0.8.26 角色档案**。配置位于 `Assets/MiniGame/Content/TerminalDossier/profiles.json`，安装生成独立的 `TerminalDossiers.asset`。`[verified: import_sources.py main；TerminalDossierSetup.Install]`

临时 Development Player 的探针需显式传入 `--terminal-v0826-smoke --terminal-v0826-checkpoints <绝对目录> --terminal-v0826-output <绝对目录>`。自动化检查通过真实屏幕坐标射线分发滚轮和按钮事件，输出原生及屏幕截图；所有探针存档隔离到输出目录。`[verified: TerminalDossierPlayerProbe.Prepare、PointerFor、Capture；MiniGameBootstrap.SaveDirectory]`

## 验证

档案资源 EditMode 5/5、旧处置资源 EditMode 6/6、档案 PlayMode 3/3、HTML 行为回归 PlayMode 9/9，共 **23/23** 通过。回归包含全文边界、按钮、短摘要、暂停输入、同角色刷新、重新绑定复位、重新进入终端复位和教程图片回退。独立复核及正式场景检查发现的滚动复位、Unity 空图片回退、原生输入证据问题均已修复。`[verified: verification.json tests、review；reports/playmode-dossier.json results]`

| 验收环境 | 原生终端 | 屏幕尺寸 | 原生截图数 | 结果 |
|---|---|---|---|---|
| Editor 正式 Combined 场景 | 800×600 | 1920×1080 | 13 | 通过。`[verified: reports/editor.json result、captures]` |
| Windows Development Player | 800×600 | 1280×720 | 16 | 通过，含原生滚轮上下输入。`[verified: reports/player-1280x720.json result、nativeWheelDown、nativeWheelUp]` |
| Windows Development Player | 800×600 | 1024×768 | 13 | 通过。`[verified: reports/player-1024x768.json result、captures]` |

每轮覆盖五人、教程、自审，以及未开始和已归档状态。自动化检查使用屏幕坐标命中检测分发滚轮与箭头点击，并比较浏览前后的完整运行时存档；三轮共 42 张原生画面，39 处新肖像裁切与供应包原图逐像素一致，最大通道差为 0。`[verified: capture-verification.json runs；TerminalDossierPlayerProbe.PointerFor、Tick；verify_captures.py verify]`

1280×720 的补充检查通过 Windows 窗口输入注入上下滚轮，每个方向各被 Unity `Input.mouseScrollDelta` 记录 1 帧；神父编号保持不变、摘要行偏移 0→1→0、运行时存档一致。此项是操作系统输入注入验收，不是人工操作物理鼠标。Windows 窗口截图助手超时后使用 Player 自身 `ScreenCapture` 记录画面。`[verified: verification.json nativeInput；reports/player-1280x720.json captures、nativeDownInputFrames、nativeUpInputFrames]`

[原生神父档案](evidence/day2-priest-top-800x600.png) · [原生长摘要末行](evidence/day2-priest-bottom-800x600.png) · [小孩](evidence/day1-child-top-800x600.png) · [已归档](evidence/day1-archived-former-ruler-top-800x600.png) · [自审](evidence/self-review-self-review-top-800x600.png) · [1280×720 滚动前](evidence/player-1280x720-before-wheel.png) · [滚动后](evidence/player-1280x720-after-wheel.png) · [1024×768](evidence/player-1024x768.png)。`[verified: evidence 对应 PNG 文件；capture-verification.json runs]`

临时 Development 构建成功，0 错误、1 条提示：场景没有 `RuntimePipelineManager`，Player 禁用 Unity CLI；不影响游戏 UI。验收包保存在 `Artifacts/TerminalV0826/Player/`，本轮未制作正式 Windows 交付包。两份正式存档和 Combined 场景文件在验收前后 SHA-256 一致。完整机器可读记录见 [verification.json](verification.json)。`[verified: verification.json build、scope、preservation]`

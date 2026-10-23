# 电脑 UI 字体与对话文字扰动

电脑内部的标题、名单、指标、按钮、命令、开关机控制台、实时对话与记录窗口使用方舟12px简体等宽字形。保留原终端的逻辑字号、字距、行距和换行算法，图集使用Point过滤，不依赖操作系统字体。`[verified: TerminalBitmapText.ArkTypeface/PopulateArk；TerminalIndustrialSkin.Label；TerminalTransitionView]`

## 对话记录

- 实时对话区保留滚动与逐字显示；只为框内的说话者及正文安装扰动组件。`[verified: TerminalTranscriptView.EnsureRow/Render]`
- 会谈／档案页按F3打开只读对话记录，F3或Esc关闭；方向键、滚轮可滚动。目录页F3保持不可用。查看记录时不推进正文播放，也不能确认背后的应答或处置。`[verified: TerminalView.OpenHistory/CloseHistory；MiniGameScreen.HandleTerminalKey/Update]`
- 标题、时间、指标、按钮、命令、开机文字和房间UI不加此文字扰动；现有Logo与监控效果保留。扰动只修改字形顶点与亮度，不改写文本、分页、存档记录或全局随机状态。`[verified: TerminalDialogueTextGlitch.ModifyMesh；TerminalDialogueTextGlitchTests；TerminalHtmlParityTests.ReadOnlyF3RecordsPreserveTranscriptAndBlockUnderlyingChoices]`

## 调整扰动

选择 [TerminalDialogueTextDefault.asset](../../UnityProject/Assets/MiniGame/Resources/MiniGame/TerminalDialogueTextDefault.asset)。默认让少量字形间歇向上或向下错位约2个像素，随后复原；横向错位默认为0。Inspector提供“上下错位（像素）”“受影响字形比例”“突发概率”“每次持续（秒）”等参数，两个对话区域共用此配置。`[verified: TerminalDialogueTextSettings；TerminalDialogueTextGlitch.Settings/State/ModifyMesh]`

## 字体来源与缺字

主字形来自 [Ark Pixel Font 2026.10.16](https://github.com/TakWolf/ark-pixel-font/releases/tag/2026.10.16)，12px-monospaced-zh_hans。当前语料中方舟缺少的30个字符，仅用同作者的 [Fusion Pixel Font 2026.10.16](https://github.com/TakWolf/fusion-pixel-font/releases/tag/2026.10.16) 同尺寸字形补齐；不以问号替换正文。补字清单与源文件SHA256见 [coverage.json](coverage.json)。字体及上游OFL许可保留在 [Art/Fonts/ArkPixel12](../../Art/Fonts/ArkPixel12)。`[verified: coverage.json；上述BDF/OFL源文件]`

新增对白或UI字符后运行：

```powershell
uv run Tools/TerminalTypography/build_ark_atlas.py
uv run Tools/TerminalTypography/build_ark_atlas.py --check-only
```

首次安装时在Unity运行“MiniGame → 终端 → 安装方舟像素字体”。之后图集或字形数据重新导入时，会自动清空字形缓存并刷新已加载文本。安装器设置Point、关闭mipmap和压缩，并校验实际导入尺寸。`[verified: TerminalTypographySetup.Install/RefreshLoadedFont；TerminalPixelFontRefresh.OnPostprocessAllAssets；TerminalPixelFont.TryGet]`

语料扫描包括主线内容和Resources下直接加载的监督员、结局JSON及Yarn文件。检查脚本验证所有7个逻辑字号的字符覆盖、字形边界和实际可见像素；缺字、空字形、透明字形或图集尺寸不一致都会失败。10／11px缩小时保留被采样范围内的笔画，防止“一”、破折号等细笔画整字消失；12px及以上保持原采样方式。`[verified: build_ark_atlas.py corpus/resize_glyph/validate_generated；test_build_ark_atlas.py]`

字体回归检查：

```powershell
uv run --with Pillow==12.3.0 python -m unittest discover -s Tools/TerminalTypography -v
```

Unity对应检查为 `TerminalBitmapTextTests`、`TerminalDialogueTextGlitchTests`、`DialogueBitmapPaginationTests`（EditMode）及 `TerminalHtmlParityTests`（PlayMode）。前者覆盖活动对白包、监督员和结局的所有非空白字符，后者在实际终端Canvas渲染包含原先缺字的结局句子并保存截图。`[verified: TerminalBitmapTextTests.EveryActiveDialogueCharacterHasVisibleGlyphsAtEveryLogicalSize；TerminalHtmlParityTests.EndingCharactersRenderThroughTheLiveDialogueCanvas]`

像素风强度设置与首次选择页的文案直接来自运行时源码，自动纳入 `corpus()` 的 `Runtime/*.cs` 扫描。本次补齐“块、粒、荐”三个码位的七档字形，覆盖增至1,455个码位／10,185个字形；原10,164个字形的RGBA像素、尺寸、偏移和advance全部保留，重排仍使用既有导入缓存刷新。详见[像素风强度字形核对](pixel-strength-glyph-audit-20261023.json)。`[verified: build_ark_atlas.py corpus；pixel-strength-glyph-audit-20261023.json preservedGlyphs/addedCharacters/pixelOrMetricChanges；TerminalPixelFontRefresh.OnPostprocessAllAssets]`

验证记录见 [verification.json](verification.json)。`[verified: verification.json]`

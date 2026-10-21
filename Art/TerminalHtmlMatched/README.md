# HTMLMatched 字形扩充

终端沿用资源包的 Windows Canvas2D 字形，不经过 Unity 动态字体替换。原包 18,795 个字形的 RGBA、宽高、偏移与前进量逐字节保留，仅重新排列图集位置。[verified: glyph-coverage.json preservation]

新增字形使用原工具的规则：中文及码位大于 255 的字符使用 SimSun，其余使用 Lucida Console；`textBaseline='top'`、`fillText(ch,12,12)`、非零 alpha 裁切、2 像素间隔。包含 10／11／12／13／14／18／19px 与五种原配色。[verified: extend-glyph-atlas.cjs browser rasterization；glyph-coverage.json rasterRules]

修改正式 Campaign、终端对白、Yarn 正文或运行时中文后，重新运行：

```powershell
& 'C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/bin/node.exe' 'Art/TerminalHtmlMatched/extend-glyph-atlas.cjs' --probe-only
& 'C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/bin/node.exe' 'Art/TerminalHtmlMatched/extend-glyph-atlas.cjs'
```

生成器要求 Windows 已安装宋体与 Lucida Console，Node 可使用 Playwright 和 pngjs。可通过 `--node-modules`、`--browser`、`--repo` 覆盖本机路径；启动独立的无界面 Chromium，不连接用户浏览器会话。[verified: extend-glyph-atlas.cjs CLI options and chromium.launch]

生成所需的四个原始输入已纳入 `Art/TerminalHtmlMatched/Source/`：原始 PNG、原始 Metrics JSON、HTML 导出工具和参考对白 JSON；保持供应包中的原始字节，克隆仓库后无需恢复被忽略的 `Artifacts` 目录。PNG 和原始 Metrics JSON 使用 Git LFS，克隆后需完成 LFS 拉取。可通过 `--source-root` 指向包含同名四文件的其他目录，不包含 Windows 字体文件。[verified: Source/；extend-glyph-atlas.cjs sourceRoot]

输出覆盖 `UnityProject/Assets/PMSAmberTerminal/Art/FontAtlas/HTML_SimSun_Lucida.png` 和 `HTML_FontMetrics.json`，不修改 `.meta`。Unity 导入保持 Point、无 mipmap、无压缩、非 sRGB、不缩放；Max Size 至少等于报告中的输出尺寸。[verified: extend-glyph-atlas.cjs output；PMSBitmapFont.cs Load]

覆盖报告记录每个输入的 SHA-256、输入范围、缺字检查、字体文件哈希、浏览器版本、原图保持检查和输出哈希。当前覆盖 BMP 字符；遇到非 BMP 字符会停止生成，需先扩展运行时的 `char` 字形接口。[verified: glyph-coverage.json；extend-glyph-atlas.cjs nonBmp assertion]

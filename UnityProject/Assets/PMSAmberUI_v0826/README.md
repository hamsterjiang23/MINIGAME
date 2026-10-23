# PMS 琥珀终端 UI 资源 v0.8.26 · 肖像档案

完整资源包包含现有终端布局、六名角色档案照、原有处置按钮/病房监控/字体字形/CRT 素材，以及上一张确认的肖像布局概念图。

## 导入

解压后，将 `Assets/PMSAmberUI_v0826` 连同 `.meta` 文件复制到 Unity 工程的 `Assets` 下。ZIP 本身不能作为 UnityPackage 直接导入。此包使用独立目录、命名空间和 GUID，可与旧包共存。需要更新现有界面时，请重新绑定本包资源。

推荐 Canvas 参考分辨率 **800×600**。坐标全部从左上角计算：RectTransform 使用左上锚点和左上 Pivot，Unity 位置填写 `(x,-y)`。PNG 已配置 Point、无 Mipmap、无压缩、NPOT 不缩放。Image/RawImage 的 Tint 保持白色，已着色资源无需再染黄。

## 六张角色档案照

| ID | 角色 |
|---|---|
| AD01 | 外交官 |
| ML01 | 小孩 |
| ED02 | 战场医生 |
| VV01 | 执政者 |
| SJ01 | 主角 |
| JN02 | 神父 |

- `Portraits/Native`：96×128，适合 800×600 界面按原生大小放置。
- `Portraits/Double`：192×256。
- `Portraits/Large`：384×512。
- `Portraits/Atlas`：六张原生头像组成的 576×128 图集；单图和图集的裁切信息见 `Portraits/Portraits.json`。
- `Reference/PortraitMasters`：内置 image_gen 分别生成的六张大图原稿，保留原始输出尺寸。
- `Reference/UserSources`：本次用户提供的六张角色原图。

头像是黑底、不透明、已着色的琥珀点阵 PNG，保留细微扫描质感；外框独立。放入整屏 CRT 时无需再叠加黄色滤镜。统一布局槽位不代表改变发型/帽子的天然占比：小孩保留蓬发，神父保留完整帽子，主角保留刘海遮眼。

## 选人界面布局

`Documentation/Layout.json` 包含完整坐标；新档案区位于原生 y=305—548。

- 头像框 `(30,322,98,130)`；头像放在 `(31,323,96,128)`。
- 档案标题 `(140,322)`。
- 左侧字段标签 x=140，值 x=212；行 y 为 356、380、404、428、452。
- 原有终端、对象、单元、档案状态、净化配额和当前命令全部保留。
- 右侧年龄与职业反白区分别是 `(490,348,288,26)`、`(490,382,288,26)`；登记摘要为 `(490,416,288,56)`，最多两行。
- 当前命令框 `(118,486,660,26)`，底部提示栏位置维持原样。

`UI/Dossier` 提供独立头像框、反白块、名册选中条、空档案底图及小孩档案整块示例。整块示例带有占位文字，只用于对照，正式界面建议使用空底图和独立文本组件。

`UI/Frame_Directory_800x600.png` 与 `Frame_Review_800x600.png` 是无正文文字的框架图；框架已带反白底色，不要再重复叠加相同背景。`Labels/Dossier_*.png` 提供新增字段名的原版点阵字形。

## 原有资源

`Icons`、`Labels`、`CommandRows` 保留六成图标和完整按钮状态。按钮完整行 248×48，顶部 248×44 为点击区；已包含名称与图标。`Monitor` 保留病房原图及原版暖灰处理图。`Effects`、`Shaders` 保留关机柔光和独立名称的 CRT 效果。

## 字体与指标

`Fonts/HTML_SimSun_Lucida.png` 和度量 JSON 已根据本版 HTML 重新导出，包含“登记摘要”“年龄”“职业”等当前文字。没有附带系统 TTF/OTF 文件。

可选的 `PMS UI v0.8.26 → Pixel Text` 组件会自动读取已绑定的位图字体，依赖 UGUI 和 Unity JSON 模块。字号支持 10、11、12、13、14、16、18、19。动态文本可用 `SetText(...)` 更新；字体是当前字符集的字形资源，新增正文时需要补齐字符，不能直接替换成其他字体来期待相同效果。

`Fonts/QuotaFields_00_99.png` 是审查界面的 66×32 指标字段；`Fonts/DossierQuota_00_99.png` 是档案区的 56×24 配额字段。均有裁切 JSON。指标值由你的游戏状态提供，资源本身不保存第二套配额。

## 参考与预览

- `Reference/Approved_Portrait_Layout_Concept.png`：前面确认的神父肖像布局图，完整原图已收录。
- `Reference/Selected_Concept_Preview.png`：同一布局图，供既有预览流程使用。
- `Reference/PMS_v0.8.26.html`：可离线打开的新布局 HTML，六张头像均内嵌，当前已配置对象显示小孩肖像。
- `Reference/Previous_v0.8.25.html`：上一版“登记摘要”布局备查。
- `Preview/Portraits_ContactSheet.png`：六张原生头像的整数倍预览。
- `Preview/Directory_Portrait_Raw.png` 与 `Directory_Portrait_CRT_125.png`：实际 HTML 的本地 Canvas2D 绘制和 CPU CRT 效果；不是浏览器截图。
- `Reference/GenerationPrompts.json`：六张头像的最终生成提示词，生成方式为内置 image_gen。

角色肖像是本次新增的美术资源；HTML 仍只有原来一个已配置的 Day1 对象。没有为新增角色编造年龄、职业、摘要、对白、处置规则或剧情。当前示例资料仍使用“未登记／待补录”。

这是 UI 美术和显示辅助资源包，没有自动接入正式游戏逻辑的整套玩法预制体。没有修改你的 Unity 游戏工程。校验结果见 `Validation`；浏览器和真实 GPU CRT 的逐像素还原尚未实测。

## 本次导入校验

已在独立 Unity 2022.3.62f3c1 项目中导入 106 张纹理，验证头像与字体尺寸、Point、无压缩、无 Mipmap、位图字体引用、文字组件自动载入和 CRT 着色器导入。未进行真实 GPU 显示效果或完整游戏集成测试。

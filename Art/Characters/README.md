# 原创对话角色立绘

六位角色共 27 张透明 PNG，使用内置 `image_gen.imagegen` 逐张生成；每位角色先生成 `01` 身份基准，其余状态直接引用自己的基准图进行表情编辑。原始输出、提示词和回执一并保存。`[verified: manifest.json imageCount/identityCount/generationCalls/assets[].reference；各 .receipt.json]`

## 文件与状态

- `ML/ML01–05.png`：米洛；紧张戒备、怯生生地解释、迟缓压抑、想念家人的悲伤、意外释然。`[verified: manifest.json assets 中 ML01–05.state]`
- `VV/VV01–05.png`：薇拉；平静审视、克制反驳、动摇疲惫、愤怒失控、疲倦中的释然。`[verified: manifest.json assets 中 VV01–05.state]`
- `ED/ED01–05.png`：埃德温；严厉观察、医生本能的关切、痛苦回忆被触发、迟缓疲惫、坦诚悲伤。`[verified: manifest.json assets 中 ED01–05.state]`
- `AD/AD01–05.png`：阿德里安；礼貌沉着、温和共情、观察记录、被问住的意外、收起笑容直接审视。`[verified: manifest.json assets 中 AD01–05.state]`
- `JN/JN01–05.png`：乔纳；平静祈祷、欣喜期待、焦虑恳求、狂热愤怒、坚定而近于狂喜。`[verified: manifest.json assets 中 JN01–05.state]`
- `D0/D0_01–02.png`：无名 Day0 对象；急切质疑与恳求、低头怀疑。`[verified: manifest.json assets 中 D0_01–02.state]`

每张图旁边的 `.prompt.txt` 是完整生成提示词，`.receipt.json` 保存实际工具返回的原始输出位置；[manifest.json](manifest.json) 汇总角色 ID、状态、相对路径、基准引用、SHA256、尺寸、透明像素统计与人工目视检查说明。`[verified: manifest.json assets[].prompt/receipt/reference/sha256/width/height/transparentPixels/visualReview]`

## 接入约定

提示词要求 1024×1024；内置工具实际返回的全部 27 张原图都是 **1254×1254**。本目录保留原始像素和透明通道，未缩放或抠图。Unity 导入时建议 `maxTextureSize = 1024`，无需改写原图。`[verified: manifest.json requestedSize/actualSize/originalsUnmodified/assets[].width/height]` `[opinion]`

画风采用低饱和橄榄灰、米白和自然肤色，写实手绘质感；固定略高的会谈机位、坐姿半身并保留双手。角色差分只改变表情与极小的头部角度，服装、身份和光向保持一致。`[verified: 各 .prompt.txt；manifest.json assets[].visualReview]`

人物作为透明前景叠加到监控画面，CRT、噪声、病房背景由游戏合成。底部裁切属于半身立绘构图，显示时应保持宽高比；名册可以使用同一资源显示上半身。`[opinion]`

源图内部大部分可见像素的 alpha 接近 255，仍可能小于 255；不要把“非全不透明像素”直接当作轮廓边缘。`[verified: manifest.json assets[].partialAlphaPixels；ML01 像素采样 (625,625).A=253、(625,350).A=252]`

## 验证范围

已逐张查看生成结果，核对人物身份、表情、服装、双手与构图；已逐像素检查全部 PNG 的透明通道，并验证 27 个 ID、提示词与回执齐全。`[verified: manifest.json assets[].visualReview/hasTransparency；生成与复制后校验输出 PASS: 27 portraits]`

原图检查不代表游戏内最终观感验收；实际监控尺寸下的辨识度、合成效果与加载状态需由 Unity 场景检查。`[opinion]`

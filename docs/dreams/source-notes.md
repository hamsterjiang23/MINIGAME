# 梦境与海滩终局：来源及本轮裁决

## 每日梦境采用最新附件

本轮采用用户提供的 `E:\Downloads\梦场景.docx`，仓库原件为 [Art/Dreams/Reference/梦场景.docx](../../Art/Dreams/Reference/梦场景.docx)。两份 SHA-256 均为 `d6b7769fba7b6fce710545a0dd3d9796f6ac79c8436dec8b0e8dbd857d1a15b1`；正文包含 Day1–3 和 **9 张正文参考图**。这份附件在本轮制作中优先于[旧四梦境记录](../sources/dream-scenes.md)，旧记录保留历史身份。`[verified: 两份 DOCX 的本轮 Get-FileHash；最新 DOCX document.xml P2–19；本轮已接受的来源优先级]`

- **Day1 鲸尸海滩**：正文 P3 写海鸥、海浪、远处鲸尸及海水中的血；采用 `word/media/image1.png`，导出为 [Day1_WhaleBeach.png](../../Art/Dreams/Day1_WhaleBeach.png)。音乐沿用稳定用途 `Dream0`，来源短链为 [L9fXP1ssVbkmuoiF](https://suno.com/s/L9fXP1ssVbkmuoiF)。`[verified: 最新 DOCX P3–5；Art/Dreams/manifest.json artwork[0]；DreamSequenceProfile.cs Days]`
- **Day2 无尽长路**：正文 P9 写似乎永远走不到底的路；采用 `word/media/image4.png`，导出为 [Day2_EndlessRoad.png](../../Art/Dreams/Day2_EndlessRoad.png)。音乐用途 `Dream1`，来源短链为 [zubUB9ako0L0s8rD](https://suno.com/s/zubUB9ako0L0s8rD)。`[verified: 最新 DOCX P9–11；Art/Dreams/manifest.json artwork[1]；DreamSequenceProfile.cs Days]`
- **Day3 受缚待审**：正文 P15 写主角被关在病人房间、坐椅子戴镣铐等待审判；采用 `word/media/image7.png`，导出为 [Day3_BoundJudgment.png](../../Art/Dreams/Day3_BoundJudgment.png)。使用新增音乐用途 `Dream3`，来源短链为 [Nkn1pOBzMuGpyA1N](https://suno.com/s/Nkn1pOBzMuGpyA1N)。`[verified: 最新 DOCX P15–17；Art/Dreams/manifest.json artwork[2]；DreamSequenceProfile.cs Days]`

三张画面保留附件原始 PNG，由 Unity 在运行时生成运动并投影到房间墙面；不是已制作的视频文件。旧 `Dream2` 枯花田音乐保留为备用，不再映射当前 Day3。`[verified: Art/Dreams/manifest.json note/artwork；DreamAnimationStage.cs Render；MiniGameAudioProfile.cs MiniGameSound；DreamSequenceProfile.cs Days]`

本轮用户补充要求：**墙面动画播放时不使用现有复古 Shader**。因此只在动画实际播放阶段临时跳过该效果，镜头返回或停止时恢复原状态；当前房间保存的复古强度 **1**、像素尺寸 **8**，以及原图预览和组件启用选择均不由梦境演出改写。为避免吊灯等前景物体遮挡，投影矩形内改为前景优先显示。GPU 图像回归及三段实际相机录制已完成，主观美术认可不由自动检查代替。`[verified: DreamSequenceDirector.cs L151–173、L270–308；RetroScreenEffect.cs L14–28；DreamProjectionSurface.shader L6–8；verification.json editor.playModeResults/visual]`

## 音频取得与边界

Day3 音乐标题为 **Red Glitch Slices**，作者 **htftzcfu**，[歌曲页](https://suno.com/song/a9c378c3-27d0-4436-878a-94868515ffb9)。[source.json](../../Art/Dreams/Audio/source.json) 记录正常匿名播放器流程取得公开播放内容，再转为 PCM16 WAV；它不是作者原始母带，具体再利用授权仍未核实。记录不保留登录凭据、播放授权或密钥。`[verified: Art/Dreams/Audio/source.json title/author/canonicalSource/conversion/rights/notes]`

当前 [Day3_BoundJudgment.wav](../../UnityProject/Assets/MiniGame/Audio/Music/Day3_BoundJudgment.wav) 为 **48,000 Hz、双声道、16 bit、177.6135 秒**，文件大小 **34,101,964 字节**，SHA-256 为 `3e797e90a232ce1cd8c82e1e778fbc175133c339d6fb8199a524e32d22e6dcb8`。本轮独立读取 RIFF/WAVE 头与 data 长度，并核对文件哈希和来源记录一致；这证明本地转换产物的格式和完整性，不是游戏内听感验收。`[verified: 本轮 BinaryReader RIFF/fmt/data 检查、Get-FileHash；Art/Dreams/Audio/source.json conversion.output]`

海浪由本工程的 [build_beach.py](../../Art/DreamEnding/build_beach.py) 合成，输出可循环的 [OceanWaves.wav](../../Art/DreamEnding/OceanWaves.wav)，当前音频配置已绑定 Unity 导入副本；终局歌曲 `EndingMusic` 槽仍留空，由策划在 GUI 选择文件。此前 12 项音乐／音效来源与署名记录继续见 [Art/Audio/README.md](../../Art/Audio/README.md)。`[verified: build_beach.py ambience；AudioDefault.asset Sounds Id13/Id14]`

## 海滩终局与用户确认的改动

结局原件为 `E:\Downloads\梦结局场景.docx`，SHA-256：`f2dfbcaa4d02af0762d3e41d33b06f23cd21a4ca3b7ca631a0b79fa206a86cb4`。正文 P3 要求纯 3D 海滩并放置公寓的床，P5–7 要求海浪、移动云层、上床后镜头到枕头并仰看天空随海浪起伏；飞鸟只是“也许”。P8–13 提供五句屏幕下方对白与配音意图。文档只有音乐名称 **An Ending (Ascent)**，没有音乐或图片外部链接；该名称保留为参考，不据此填入歌曲文件。`[verified: 结局 DOCX document.xml P3–13；包内 External 关系数量 0；本轮 Get-FileHash]`

本轮用户确认保留主 Wiki 的**玩家自我净化**入口，采用“Day3 反叛阈值且无更高优先级结局 → 仅电脑引导 → 本人监控、唯一净化按钮 → 海滩”链路。结局场景为海边一张床，W 前进、E 躺下、Esc 暂停，躺下后看流云并漂流；五句字幕保留，配音可留空。附件中的“直接与公寓床交互进入结局”不覆盖本轮确认的自审入口。`[verified: 本轮已接受的梦境／反叛／海滩实施范围；GameSession.cs FinishDream/SelfPurify；TerminalView.cs SelfPurify；DreamEndingController.cs Update/AdvanceSubtitles]`

玩家全局反叛值与 NPC 信任分离；普通及特殊对话回答按稳定选项 ID 结算，处置不另加。初始 0、上限 100、阈值 100、缺省增减 0 是本轮可调默认值，不把历史疯狂值、偏离指数、狂热值重新命名或合并。Day0 不投影；Day1–3 自动墙面放映；结局优先级为神父袭击、KPI 失败、海上梦结局、普通结局。`[verified: 本轮确认范围及对“普通选项”指非处置回答的澄清；PlayerRebellion.asset；GameRules.cs WithDreamFlow；GameSession.cs StartNightDream/ResolveEnding/Select]`

来源、配置与运行验收分开记录。自动测试、实际相机画面、Windows 构建及启动结果均已写入 [verification.json](verification.json)；视频音轨为配置源文件后期混音，尚未进行独立人工听感验收。`[verified: verification.json core/editor/visual/player/audio]`

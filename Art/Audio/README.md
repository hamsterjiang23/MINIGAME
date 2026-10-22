# 音频来源与署名

2026-10-23 按用户指定接入 `SFX/NewDoorKnock.wav` 与 `SFX/NewDoorShut.wav`：晨检等待开门时完整播放四次敲门的录音，每段播放结束后等待0.5秒再重复；开门立即停止，暂停冻结录音或间隔，恢复后继续。关门改用 NewDoorShut，开门沿用 DoorOpen。Unity副本与两份源文件逐一核对SHA-256一致，均沿用根目录WAV的Git LFS规则。Unity 2022.3.23f1c1实际导入及门音效PlayMode测试2项通过；本轮未构建Windows Player。`[verified: UnityProject/Assets/MiniGame/Runtime/MiniGameAudioDirector.cs UpdateKnocking；UnityProject/Assets/MiniGame/Editor/MiniGameAudioSetup.cs ApplyDoorSounds；Artifacts/Unity/DoorAudio/import.log；Artifacts/Unity/DoorAudio/playmode-results.xml test-run passed=2/failed=0]`

终端开关机逐字音使用提交 `073bdb12` 新增的 `SFX/UIType/UIType1.wav` 至 `UIType4.wav`，四段 WAV 与 Unity 副本均核对远端 LFS 哈希；逐字随机、音量设置和本轮运行版见[接入说明](../../docs/terminal-typing-audio/guide.md)。同提交的 `OptionPopSound.wav` 保留为源资料。`[verified: docs/terminal-typing-audio/source-manifest.json sourceAssets/unityImports]`

远端提交 `8a9fd32c` 将原 `Dream0.wav`、`Dream1.wav` 分别改名为 `Dream1.wav`、`Dream2.wav`，两个文件的 LFS SHA-256 均未改变，并新增 `Dream3.wav`。本工程保持已序列化的声音 ID 与 Unity 副本名称；导入器按新源文件名读取。旧枯花田音乐另存为 `Music/UnusedFlowerField.wav`。以下清单保留首次接入时的历史路径和哈希，不将其当作当前目录索引。`[verified: 8a9fd32c 与父提交的 LFS oid；MiniGameAudioSetup.cs ImportClip]`

首次接入的 **12 个 WAV：5 首音乐、7 个音效**，内容对应提交 `a50abea8ae33c058d4ac20faed960708c046a4cd`。逐文件 SHA-256、字节数已与该提交的 LFS `oid`、`size` 核对，12 项一致；历史结果与来源记录见 [source-manifest.json](source-manifest.json)。`[verified: source-manifest.json repositoryAcquisition、assets[].local]`

用途与外链来自[主 Wiki](../../docs/sources/game-wiki.md)第 132–145 行及[梦境设计](../../docs/sources/dream-scenes.md)第 26、38、50 行。**本地文件名与外部歌曲的对应属于按用途作出的设计映射，未验证外部原文件与本地 WAV 的字节或波形一致性。**`[verified: docs/sources/game-wiki.md L132–145；docs/sources/dream-scenes.md L26、L38、L50]` `[opinion]`

本轮复用已有 WAV，只读取公开来源页、检查试听 URL；没有重复下载有损 MP3／M4A 试听，也没有替换或转码这些 WAV。外站最初下载记录、转换与修改历史仍为未知；仓库 LFS 对象一致不代表外站原文件一致。`[verified: source-manifest.json evidenceBoundaries、repositoryAcquisition.originalExternalDownloadRecord]`

## 需要署名的策划来源

- `SFX/LightBeep.wav` 的指定来源：**5 beep c.wav — jobro**，[作品页](https://freesound.org/people/jobro/sounds/33789/)，[CC BY 3.0](https://creativecommons.org/licenses/by/3.0/)。本地转换／修改情况未知，不写成“原样使用”。`[verified: source-manifest.json assets[7].source.attribution]`
- `SFX/Projector.wav` 的指定来源：**19 - Manipulating knobs off a 16mm projector — juanicotolengo**，[作品页](https://freesound.org/people/juanicotolengo/sounds/265144/)，[CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)。本地转换／修改情况未知，不写成“原样使用”。`[verified: source-manifest.json assets[9].source.attribution]`

上述来源许可要求保留适当署名、许可链接及适用的修改说明；本记录保存作品标题、作者和来源链接，修改情况须依据实际处理记录填写。[CC BY 3.0 说明](https://creativecommons.org/licenses/by/3.0/)、[CC BY 4.0 说明](https://creativecommons.org/licenses/by/4.0/)。`[verified: CC BY 3.0 Attribution 与 appropriate credit；CC BY 4.0 Attribution]`

PCNoise、Scan、DialoguePop、3DSceneInteraction、UIInteraction 的指定 Freesound 来源均标注 **CC0 1.0**。七个 Freesound 页面均提供公开 MP3 试听，原始下载按钮均要求登录；UIInteraction 来源的原文件为 MP3，其余六项为 WAV。`[verified: source-manifest.json assets[5:12].source.license、originalDownloadStatus、originalFileFormat]`

## Suno 权利记录

五首音乐来源页作者均显示 **htftzcfu**；曲名、短链解析后的歌曲页及公开 M4A 播放地址已记入清单。所有五项授权状态均为 **rights-unverified**：公开可播放不证明本项目取得商业使用权，也不能证明用户与该作者的关系。`[verified: source-manifest.json assets[0:5].source、licenseNotes.sunoStatus]`

[Suno 付费权利说明](https://help.suno.com/en/articles/9601665)在 2026-09-03 的版本使用“订阅期间下载的歌曲”获得商业使用权的表述；[免费权利说明](https://help.suno.com/en/articles/9601601)限定其所述免费计划歌曲用于个人非商业用途。清单记录政策来源，不据此推定这五首歌曲的具体授权。`[verified: source-manifest.json licenseNotes.sunoPolicySources]`

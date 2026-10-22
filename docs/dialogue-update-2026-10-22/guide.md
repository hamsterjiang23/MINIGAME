# 2026-10-22 六份对白更新

六份上传XLSX原样纳入来源目录，内容版本更新为1.1.1-development。全部1440行对话流程已通读并逐格比较，来源哈希与输入文件一致。`[verified: source-manifest.json；reports/review-child-doctor-priest.md；reports/review-ruler-diplomat.md]`

## 按新表生效的变化

- Day0完整采用新主链：9句引导→唯一“净化”→2句回应；不再排除新表中的任何流程行。`[verified: 新Day0对话流程r2–13；campaign.yarn tutorial节点]`
- 神父保留新版JN01–JN04定义，并按用户确认把L62/L63/L67/L121/L122/L178/L179的JN05统一映射为JN04。源表不重写，7项映射可在provenance和修复清单追溯。`[verified: 用户确认；Tools/DialogueCampaign/source-repairs.json books.priest]`
- 小孩、薇拉、医生和外交官解析内容不变；三天会谈的生成数据与旧版完全一致，只有神父7项立绘引用变化。所有信任数值、选项措辞和正文按源表保留。`[verified: reports/source-compatibility.json dayOneToThreeUnchangedExceptConfirmedPortraitRemap]`
- 新字库补齐“嘿、绑、绳、趣”，新对白全部字形覆盖，原包字形像素及度量不变。`[verified: Art/TerminalHtmlMatched/glyph-coverage.json preservation/coverage]`

## 源表已有问题与运行适配

新表仍保留医生和外交官的后附重复/冲突块及医生缺失连线。重新核对339个医生修复前提字段和2个神父前提字段后，保留已确认的连接修复及141行排除，不另写对白；1440原始行全量留在provenance，1299行进入活动图。Day0最后一行缺少终点，仅补END_DAY0。`[verified: reports/source-compatibility.json；Tools/DialogueCampaign/source-repairs.json]`

完整阅读还发现薇拉F157和外交官D106存在“先前问过某题”的文案回指，但玩家未必选择过该题；按照用户“完全和新表统一”的要求，保持原文，不擅改剧情。反例路径及美术说明的不一致见审读报告。`[verified: reports/review-ruler-diplomat.md]`

## 使用

活动入口仍为MainRoomCombined的ActiveCampaignPackage。内容哈希已改变，需开始新的检查期；原存档文件保留。原有共享立绘资源及场景不因对白导入重建。验证结果见[verification.json](verification.json)。`[verified: reports/unity-import.json sourceHash/activeResourceUpdated；DialoguePackageImporter版本校验]`
# 结局内容与源表更新

`1.2.0-development` 内容已从8份本地工作簿及自我审判飞书 revision 18 固定快照生成。8本工作簿原字节复制进 `Art/chat_excel/`，每本的原路径、旧新 SHA-256、全部sheet有效行数和逐格差异见[来源审计](sources/source-refresh.json)。`[verified: sources/source-refresh.json sources]`

| 来源 | 对话流程数据行 | 当前处理 |
|---|---:|---|
| Day0引导 | 12 | 全部启用，末句补END_DAY0。`[verified: ../../Tools/DialogueCampaign/source-repairs.json books.tutorial]` |
| 薇拉 | 296 | 原表内容保留。`[verified: sources/source-refresh.json sources[id=former-ruler].sheets]` |
| 埃德温 | 237 | 235启用、233/234排除；保留新版正文与尾段。`[verified: ../../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json books/excludedRows]` |
| 米洛 | 292 | 原表内容保留。`[verified: sources/source-refresh.json sources[id=child].sheets]` |
| 乔纳 | 181 | 5处正文与62处立绘按新源；剩余两处JN05映射JN04。`[verified: sources/source-refresh.json sources[id=priest].sheets；../../Tools/DialogueCampaign/source-repairs.json books.priest]` |
| 阿德里安 | 342 | 279启用；后附281–343保持排除。`[verified: ../../Tools/DialogueCampaign/source-repairs.json books.diplomat.excludedRanges]` |
| 监督员 | 46 | Day1/2/3各16/12/11句；Day4七句为结局。`[verified: supervisor-source-manifest.json；provenance.json lineCounts]` |
| 神父结局 | 8 | 全部线性对白，最后一句黑屏。`[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations[id=priest-ending]]` |
| 自我审判 | 17 | 16句对白及唯一“净化”行为。`[verified: sources/self-review-revision18/snapshot.json sheets[name=对话流程].rows]` |

医生原始Day3连线缺失，旧修复中109个ID仍位于相同行号。20处新措辞更新预置校验，保留历史证据及当前措辞复用依据。新表已删除旧239–241/255等尾部行，转换不复活这些内容；229、232、238三个当前处置末句分别接END_DAY3。外交官保留已审核前段；所有工作簿中的原信任数值保持。`[verified: ../../Tools/DialogueCampaign/source-repairs.json books.battlefield-doctor.repairs/retiredRepairs；../../Tools/DialogueCampaign/test_campaign.py test_authored_extreme_deltas_and_response_delta_are_all_accounted_for]`

神父新源L62/L63/L121/L122已写JN04，L67已写JN03；这五处旧映射退出。只对新源仍存在的L178/L179 JN05实施JN04映射。H67新正文为“不会让你等待太久…”。`[verified: ../../Tools/DialogueCampaign/source-repairs.json books.priest.repairs/retiredRepairs；sources/source-refresh.json sources[id=priest].sheets[name=对话流程].cellDifferences]`

## 固定快照

[自我审判源表](https://moonton.feishu.cn/wiki/ZCbEw4VEQiFyMSkgVFMcuDaannc)的四张物理sheet完整导出：对话流程、角色文本表、玩家选项表各500×20，立绘ID为200×20。四个原始响应均为revision18、has_more=false、truncated=false；导出后再次读取revision仍为18。原始value/formula/comment响应保留于[sources/self-review-revision18](sources/self-review-revision18/snapshot.json)，转换依据row_indices/col_indices定位，逐格校验归一化快照与原始数据。`[verified: sources/self-review-revision18/0ADfgL.json data；sources/self-review-revision18/1ArAxd.json data；sources/self-review-revision18/2OkNPz.json data；sources/self-review-revision18/3yaslJ.json data；build_endings.py read_snapshot]`

```powershell
lark-cli sheets +cells-get --spreadsheet-token MkDCs3BFlhw0thtn3AEcjwDJnae --sheet-id 0ADfgL --range A1:T500 --include value,formula,comment --max-chars 500000
```

另外三表使用各自sheet ID和完整范围；本地重建仅读取固定文件，不依赖实时网络。`[verified: sources/self-review-revision18/snapshot.json sheets；../../Tools/DialogueCampaign/build_endings.py read_snapshot]`

## 展示接口

Unity资源键为 `MiniGame/Endings/Dialogue`，文件为[Dialogue.json](../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json)。包含priest-ending/PriestEnding、self-review/SelfReview、supervisor-ending/SupervisorEnding三组。每句保留原ID、命名空间sourceId、源行/单元格、原文、portraitId和blackScreen；自审finalAction为purify，finalActionText为净化。`[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json conversations]`

神父源ED_01/ED_02对应PE01/PE02，ED_03对应黑屏且portraitId为空；自审SJ_01/SJ_02对应SJ01/SJ02；监督员输出JD01–JD06并保留originalPortraitId。两个结局源重用的CUSTOM001 ID通过包前缀隔离。模板Day1只作源元数据，触发时机由游戏会话负责。`[verified: provenance.json adaptations；../../Tools/DialogueCampaign/build_endings.py linear_conversation/build]`

revision为正文、版本及全部9源hash的规范JSON SHA-256。源文件或正文变化会改变revision。审计保存所有原始内容与适配，JSON/Yarn均可确定性重建。`[verified: ../../Tools/DialogueCampaign/build_endings.py canonical_hash/build；../../Tools/DialogueCampaign/test_endings.py test_revision_covers_both_content_and_all_source_hashes]`

## 验证边界

已执行三个转换器的小样及全量构建、三个 `--check` 逐字重建、Python unittest 26项通过，以及限定内容目录的 `git diff --check`。监督员图片来源校验通过，文字转换器不写图片。该结果验证数据与静态生成物；Unity导入、Play Mode及目标平台构建由运行层单独验收。`[verified: Tools/DialogueCampaign unittest output: Ran 26 tests / OK；build_campaign.py --check output: sourceRowCount1360/meetingCount13；build_endings.py --check output: lineCounts8/16/7；build_supervisor.py --check output: days16/12/11/7]`

使用固定SDK8.0.424的独立临时console引用当前Release Core/Presentation，正式内容的4条命令路径均到达预期检查点：正常213命令到Day4 TakenAway；释放神父146命令到Day3 PriestAttack；未达KPI77命令到Day4 LockedInRoom；隐藏223命令到Day3 SelfReview。隐藏路径仍在院正式病人2人、信任总和123、反叛值0；通过真实回答/处置达成，没有改写存档DTO。详情见[检查记录](checks.json)。该probe不代表Unity场景演出已通过。`[verified: checks.json commandRouteProbe.routes]`

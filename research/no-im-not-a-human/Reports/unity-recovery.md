# Unity 工程与资源恢复报告

已实际导出一个可识别的 Unity 工程目录，并以真实 CIL 反编译代码替换对应 IL2CPP 空方法脚本；尚未在 Unity Editor 中完成导入、语义编译或运行验证。应打开 [恢复工程根目录](../RecoveredUnityProject/ExportedProject)，其中包含 `Assets`、`Packages` 和 `ProjectSettings`。[verified: Analysis/Assets/full-export.json status=200 stage=finished；Analysis/Assets/recovery-summary.json projectRoot；Analysis/Assets/script-merge-csharp9.json]

## 工具、输入与实际执行

- 使用官方 AssetRipper 2.0.0 Windows x64 便携版，目录 `Analysis/Tools/AssetRipper/2.0.0`。下载包 SHA256 与官方发布值 `9a7ef0e7c5c3ea5b90b4e6d855e2d98d5f7ec8c3f9e26fccbc194c6a7b01baf7` 一致。[verified: Analysis/Tools/AssetRipper/archive-hash.json expected/actual/matches；官方 https://github.com/AssetRipper/AssetRipper/releases/expanded_assets/2.0.0 Windows x64 项]
- 实际 `--help` 仅提供 Web 服务参数，没有独立导出 CLI。通过 `--headless --port 57941` 启动仅监听 `127.0.0.1` 的服务，读取其 `/openapi.json`，再用真实的 `POST /LoadFile`、`POST /LoadFolder` 和 `POST /Export/UnityProject`，表单字段为 `path`。[verified: Analysis/Logs/asset-ripper-help.log Usage；Analysis/Logs/asset-ripper-stdout.log Now listening；Analysis/Tools/AssetRipper/openapi.json paths及PathFormData]
- 先对 882467 字节的英文 Localization Bundle 做小切片加载与导出，恢复 9 张英文表；smoke 导出成功，进程当时工作集约 198 MB。随后完整加载 `Analysis/Inputs/GameCopy`，加载 3.43 秒、导出 35.55 秒；首次完整导出工作集峰值 5888028672 字节。[verified: Analysis/Assets/smoke-load.json；Analysis/Assets/smoke-export.json；Analysis/Assets/SmokeProject/ExportedProject/Assets/_Localization/Tables/Text；Analysis/Assets/full-load.json；Analysis/Assets/full-export.json]
- 所有输入读取自研究目录的游戏副本。为了检验序列化失败原因，又创建 `Analysis/Assets/ManagedRecoveryInput/NoImNotAHuman_Data`，复制 Data 资产并加入备份目录的真实 Managed DLL，排除 `il2cpp_data`。该派生输入被 AssetRipper 识别为 Mono，完整导出至 `Analysis/Assets/ManagedExport`；没有覆盖 GameCopy 或 Steam 游戏目录。[verified: Analysis/Assets/managed-recovery-input.json；Analysis/Assets/managed-load.json；Analysis/Logs/asset-ripper-managed-load.log L69；Analysis/Assets/managed-export.json]
- 以单个 AssetRipper 进程运行，设置 `DOTNET_GCHeapHardLimit=0x180000000`；第二轮实际进程工作集峰值 7432527872 字节。全部导出完成后已停止自有 AssetRipper 进程，未操作用户已有 Unity 进程。[verified: Analysis/Assets/asset-ripper-process.json；Analysis/Assets/managed-export.json process.PeakWorkingSet64；Analysis/Assets/asset-ripper-stop.json]

## 实際恢复范围

最终 Assets 中有 4792 个非 meta 文件、4686 个 meta 文件，非 meta 文件总字节数为 1429331494。数字描述本次导出结果，不能等同于作者原始工程的完整资产数量。[verified: Analysis/Assets/recovery-summary.json assetFileCountExcludingMeta/metaFileCount/assetBytesExcludingMeta；opinion: 导出计数与原工程计数之间的边界]

- 7 个 Scene：MainMenu、GameScene、Loading、Ball、StrangeMorning、Titery、MushroomDream；完整文件路径见 `recovery-summary.json scenes`。[verified: Analysis/Assets/recovery-summary.json scenes]
- 38 个 prefab、2780 个序列化 GameObject 文档、915 个 Transform、1865 个 RectTransform、1457 个 CanvasRenderer 文档，包含 UI 与层级数据。[verified: Analysis/Assets/recovery-summary.json extensionCounts及yamlClassCounts 1/4/224/222]
- 1003 个 PNG、1853 个 Sprite 文档、114 个 Material、106 个 Mesh 文档、359 个 OGG、12 个视频文件、15 个 TTF 字体。[verified: Analysis/Assets/recovery-summary.json extensionCounts及yamlClassCounts 21/43/213]
- 14 个 AnimationClip、3 个 AnimatorController、1 个 AudioMixer 文件和 1 个 Playable 文件。[verified: Analysis/Assets/recovery-summary.json extensionCounts及yamlClassCounts 74/91]
- 4116 个 MonoBehaviour 序列化文档，包括 Scene/Prefab 的 Components、独立 ScriptableObject 和 Localization 表；此处是文档总数，不是独立 ScriptableObject 的数量。[verified: Analysis/Assets/recovery-summary.json yamlClassCounts.114；opinion: 计数解释]
- Addressables 原始 catalog.bin、settings.json、catalog.hash 和 26 个 bundle 随 StreamingAssets 保留；Localization Tables 已另行解码为 `.asset`，而非只保留压缩 Bundle。[verified: Analysis/Assets/recovery-summary.json extensionCounts及folderCounts；RecoveredUnityProject/ExportedProject/Assets/StreamingAssets/aa；RecoveredUnityProject/ExportedProject/Assets/_Localization/Tables/Text]

## 真实代码合并与引用检查

AssetRipper Free 首次导出的 IL2CPP 脚本包含 dummy 方法。最终使用 ILSpy 明确指定 `CSharp9_0` 从真实备份 CIL 导出的源代码，只对路径、声明类型名和命名空间完全匹配的文件执行替换；主程序集 585 份、firstpass 40 份，共 625 份。两份 `Properties/AssemblyInfo.cs` 因没有对应类型声明而保持原导出内容，未将它们计为恢复实现。[verified: Analysis/Tools/AssetRipper/settings.html ScriptContentLevel Level2；Analysis/Environment/csharp9-export-provenance.json；Analysis/Assets/script-merge-csharp9.json mergedByAssembly/unmatched]

625 份脚本全部保留 AssetRipper 原有 `.meta`，合并日志保存替换前后 meta SHA256、源代码 SHA256 和目的文件 SHA256。首次 dummy 脚本保存于 `Analysis/Assets/OriginalDummyScripts`，后续 CSharp9 替换前版本也按程序集子目录留存。[verified: Analysis/Assets/script-merge.json；Analysis/Assets/script-merge-csharp9.json metaPreserved/sourceSha256/destinationSha256；Analysis/Assets/OriginalDummyScripts]

53 个插件 DLL 在新旧程序集 `AssemblyName.FullName` 完全相同的前提下，从真实 Managed 导出替换 dummy DLL，并保留 `.meta`。`__Generated.dll` 没有真实 Managed 对应文件，作为 AssetRipper 生成辅助程序集保留；没有将其伪称为恢复的作者实现。[verified: Analysis/Assets/plugin-merge.json merged/skipped及assemblyName；Analysis/Assets/OriginalDummyPlugins]

静态 GUID 检查记录了 5201 条文件级引用边、4686 个导出 GUID、无重复 GUID，仍有 2 个非内置 GUID 无导出目标，同时也是 2 个未解析 script 引用。检查仅证明 GUID 是否能找到目标文件，未验证每个 local fileID、组件类型和 Editor 导入兼容性。[verified: Analysis/Assets/recovery-summary.json externalReferenceEdges/uniqueExportedGuids/duplicateGuids/unresolvedNonBuiltinGuids；opinion: 文件级GUID检查范围]

未解析引用的来源：

- `Resources/dialogs/Project Import Data.asset`，GUID `66d23891668929be547d6fc05552a38a`。[verified: Analysis/Assets/unresolved-references.json]
- `Resources/FolderColorSettings.asset`，GUID `fb038bb58b14efd9224814bd3565b921`。[verified: Analysis/Assets/unresolved-references.json]

日志对应报出缺失 `YarnSpinner.Unity.Editor` / `Assembly-CSharp-Editor` 及 `Yarn.Unity.ProjectImportData` / `BetterFolders.FolderColorSettings`。这些名称提示它们可能属于 Editor 数据，但没有据此证明运行时不受影响。[verified: Analysis/Logs/asset-ripper-managed-export.log warning字段；opinion: Editor用途推断及影响边界]

## 玩法相关资源入口

- `Assets/MonoBehaviour/DayNightControllerData.asset`，已恢复 `_maxDayActionsByDays`、`_lastDay=14`、`_babyEndingDay=16`、`_killerEndingDay=17`、`_femaEndingDay=15`、`_coffeeBoostDays=3` 等实际序列化配置。[verified: RecoveredUnityProject/ExportedProject/Assets/MonoBehaviour/DayNightControllerData.asset L28-L41]
- `Assets/MonoBehaviour/EndingsData.asset`，已恢复 `_killsCount=4`、`_bodyEaterCount=4`、`_prophetDontCheckSignsDaysNeeded=2`、`_povistkasUsedCount=2`、`_femaCallsCount=3`。[verified: RecoveredUnityProject/ExportedProject/Assets/MonoBehaviour/EndingsData.asset L15-L19]
- `Assets/MonoBehaviour/_NoImNotAHuman.asset` 中 `compiledYarnProgram` 为非空十六进制内容，保留真实编译后的 Yarn Program。`Assets/Resources/dialogs/FakePeople2.asset` 的同名字段为空，因此不能把这个备用项目当作主要对白程序。[verified: RecoveredUnityProject/ExportedProject/Assets/MonoBehaviour/_NoImNotAHuman.asset L15；RecoveredUnityProject/ExportedProject/Assets/Resources/dialogs/FakePeople2.asset L15]
- 主英文对白及检查/昼/夜/结局 Localization 表位于 `Assets/_Localization/Tables/Text/Dialogs` 和 `_Separating/{ChecksDialogs,DayDialogs,Endings,NightDialogs,NotCharacterDialogs}`；其他语言也保留在同级目录。[verified: Analysis/Assets/exported-assets.jsonl relativePath字段；RecoveredUnityProject/ExportedProject/Assets/_Localization/Tables/Text/_Separating/Endings/Endings_en.asset L15-L24]

## 明确未恢复的内容

同样的三个序列化问题在首次 IL2CPP 导出和真实 Managed 覆盖路线中均复现；替换 DLL 未解决它们。[verified: Analysis/Assets/serialization-error-counts.json originalIL2CPP/managedOverride]

- 73 个 `_Code.Characters.CharacterSOData`，报 `ArgumentOutOfRangeException`。输出 `.asset` 只保留对象头与脚本引用，没有恢复访客身份、访问日、真假特征图片等自定义字段；全部失败对象名/文件列表见 `Analysis/Assets/character-assets-unrecovered.json`。[verified: Analysis/Assets/serialization-error-counts.json CharacterSOData=73；Analysis/Assets/character-assets-unrecovered.json；Analysis/Assets/alkonost-managed-smoke.json m_Structure为空]
- 49 个 `UnityEngine.Rendering.Universal.UniversalAdditionalLightData`，报 `read 52 bytes, expected 60 bytes`。对应自定义 Light 配置尚未可靠恢复。[verified: Analysis/Logs/asset-ripper-managed-export.log UniversalAdditionalLightData错误；Analysis/Assets/serialization-error-counts.json URPLight=49]
- `globalgamemanagers` 中 PlayerSettings 类型 129，报 `Read 900 but expected 904`。项目设置不是完整复原的原始 PlayerSettings。[verified: Analysis/Logs/asset-ripper-managed-load.log L73；Analysis/Assets/serialization-error-counts.json PlayerSettings=1]
- AssetRipper Free 的 ShaderExportMode 是 Dummy；资产属性与材料关联已导出，但原自定义 Shader 实现没有被完整反编译。ComputeShader 的 YAML 保存也不构成恢复可编辑源码的证明。[verified: Analysis/Tools/AssetRipper/settings.html ShaderExportMode Dummy及说明；opinion: ComputeShader保存与源码恢复边界]
- 尚未找回完整的原始作者工程、Editor 工具程序集、原 `.yarn` 创作文件、原始美术源文件，也没有证明导出 Scene 的行为与原游戏一致。[verified: Analysis/Assets/unresolved-references.json；opinion: 尚未建立上述恢复/行为验证证据]

已实际执行有界 UnityPy 1.25.4 + TypeTreeGeneratorAPI 0.0.10 只读探针：在 `sharedassets0.assets` 的 PathID 2615 找到 Alkonost，提取原始 1388 字节，偏移 85736720。原始字节保存为 `Analysis/Assets/Alkonost.original.raw.bin`，SHA256 为 `0b5a0740235b8377a3e9a6eecc3ba54a010684e4716192c62d8a342d88493322`，另以独立文件流重新读取同一源偏移验证完全一致。[verified: Analysis/Assets/unitypy-status.json source/raw；Analysis/Assets/Alkonost.raw.sha256-verification.json matches=true]

首次生成真实 Managed 类型树后读取失败，实际异常为 `Referenced type not found: CharacterBaseSpriteByEmotionData _Code.Characters Assembly-CSharp`。随后仅在探针进程内为该精确程序集/命名空间/类型增加动态树回退，生成节点仍包含 `m_GameObject/m_Enabled/m_Script/m_Name`，后续读取出现损坏的引用类型标识并以 `UnicodeEncodeError` 终止。第二轮日志序列化也遇到 surrogate 字符问题；第三次仅修正日志编码后保存完整异常，没有继续猜测或批量恢复。[verified: Analysis/Assets/unitypy-attempt1.json error；Analysis/Assets/unitypy-status.json fallbackTypes/error；Analysis/Assets/probe_character_unitypy.py generated_reference_type/save_status]

此探针证明可以定位并保存原始对象，也能从真实 CIL 生成候选字段树；未取得完整自定义字段值、未通过精确字节消费验证。因此73个CharacterSOData仍列为未恢复。后续入口是SerializeReference的引用类型树与真实1388字节对象；该probe没有修改源游戏、副本或恢复工程。[verified: Analysis/Assets/unitypy-status.json status=probe-failed/raw；opinion: 下一步分析入口]

## Unity Editor 验证边界

ProjectVersion 声明 `6000.3.10f1`。有界环境清单只发现 Unity 2019.4.33 和 2022.3.23 的实际可执行文件，未发现所需 6000.3.10f1；Unity Hub 元数据也没有该版本。没有用旧 Editor 强行打开、升级或导入本次恢复工程。[verified: RecoveredUnityProject/ExportedProject/ProjectSettings/ProjectVersion.txt L1；Analysis/Environment/tool-inventory.json unity_editors/unity_hub_metadata及limitations]

因此当前成果是带真实代码、主要资产和文件级引用索引的恢复工程，仍不能称为已编译、可玩或完整复制原游戏。下一步应在隔离副本中用准确版本 Editor 做导入/语义编译，并从73个原始CharacterSOData二进制记录及其SerializeReference布局继续解决解析问题；不要通过猜测数值填充缺失字段。[opinion]

## 产物索引

- `Analysis/Assets/recovery-summary.json`：最终计数、Scene路径与检查边界。[verified: Analysis/Assets/output-manifest.json relativePath/bytes]
- `Analysis/Assets/exported-assets.jsonl`：每个导出文件的大小、YAML类型计数与引用数量。[verified: Analysis/Assets/output-manifest.json relativePath/bytes]
- `Analysis/Assets/asset-references.jsonl`、`guid-index.json`、`script-references.json`、`unresolved-references.json`：文件级依赖/脚本引用证据。[verified: Analysis/Assets/output-manifest.json relativePath/bytes]
- `Analysis/Assets/script-merge-csharp9.json`、`plugin-merge.json`：精确匹配合并及meta保留记录。[verified: Analysis/Assets/output-manifest.json relativePath/bytes]
- `Analysis/Assets/character-assets-unrecovered.json`、`serialization-error-counts.json`：原始失败与替代路线结果。[verified: Analysis/Assets/output-manifest.json relativePath/bytes]
- `Analysis/Logs/asset-ripper*`：真实help、服务stdout/stderr、完整内部日志、Managed加载/导出日志。[verified: Analysis/Assets/output-manifest.json relativePath/bytes]

Verification: 英文Bundle smoke与两次完整加载/导出HTTP结果200；625份CSharp9脚本namespace/type匹配且meta SHA256保持；53个DLL程序集FullName匹配且meta保持；最终4792非meta资产/4686meta、0重复GUID、2未解析非builtin引用；UnityPy单对象原始字节SHA验证通过但SerializeReference解析失败；准确版本Editor导入与运行未执行。[verified: smoke-export.json、full-export.json、managed-export.json、script-merge-csharp9.json、plugin-merge.json、recovery-summary.json]




# 游戏文件与引擎环境

## 目标与输入保护

请求的 D:\SteamLibrary\steamapps\common\NoImNotAHuman 不存在。Steam注册表、libraryfolders.vdf及appmanifest_3180070.acf共同定位到 E:\steam\steamapps\common\No, I'm not a Human，Steam buildid为23967901。[verified: Analysis/Environment/path-discovery.json；Reports/path-discovery.md]

原始安装含965个文件，合计2,460,965,987字节。已复制到 Analysis/Inputs/GameCopy，并对全部965个源文件与副本逐文件核对SHA-256，一致。所有恢复、代码、工具和报告位于本研究目录；没有启动原游戏、修改安装内容、处理DRM或公开分发资源。[verified: Analysis/Environment/game-file-manifest.csv；source-copy-sha256.csv；Analysis/Logs/game-copy.log；investigation-status.json source_policy]

## Unity和脚本后端

UnityPlayer.dll的ProductVersion为6000.3.10f1 (e35f0c77bd8e)，恢复工程的ProjectVersion也标为6000.3.10f1。GameAssembly.dll为x86_64 PE32+、7个section且没有CLI目录，真实运行后端按GameAssembly/global-metadata/生成C++布局识别为IL2CPP；备份Assembly-CSharp是AnyCPU的PE CLI/CIL，不能因备份托管程序集的Unity引用就把实际玩家后端说成Mono。[verified: engine-and-recovery-summary.json unity_version；REA ev_a14f77266402338b57266c4676088bae77a8ca7ed2187792d23c249d49f1e6cd；rea-managed-artifact.json classification；game-file-manifest.csv][opinion]

关键实物包括 GameAssembly.dll(81,678,336字节)、UnityPlayer.dll(35,748,776字节)、备份Managed/Assembly-CSharp.dll(948,736字节)、global-metadata.dat(16,916,928字节)、备份il2cppOutput、resources.assets、sharedassets0..6、level0..6、StreamingAssets/aa及其Addressables内容。[verified: Analysis/Environment/game-file-manifest.csv；valuable-files.csv]

备份中有108个Managed DLL，以及Assembly-CSharp和依赖程序集对应的生成C/C++。主程序集的SHA256/MVID、类型/字段/方法和CIL-C++关联全部有独立索引；详情见code-recovery.md。[verified: backup-managed-files.csv；Analysis/Managed/Assembly-CSharp/assemblies-inventory.json；Reports/code-recovery.md]

## 会话中实际REA工具

当前REA服务器为rea-agents6.4.0，广告工具170个。完整工具名、输入参数类型和完整定义保存在 Analysis/Environment/rea-tools-and-parameters.json；实际会话可用性和诊断保存在 Analysis/Evidence/rea-initial-session.json。广告存在不代表每个provider已可执行。[verified: rea-initial-session.json result.server_identity.package.version/result.server_identity.catalog.counts.mcp_tools；rea-tools-and-parameters.json][opinion]

本次真正调用了 binary_session、inspect_managed_artifact（备份与GameAssembly各一次）、inspect_managed_members、export_evidence_bundle。三个规范Evidence记录已保存到rea-evidence-bundle.json。备份artifact检查成功；成员视图受到67,108,864字节representation预算限制而不可用，未把该失败算作完整索引；索引由另一个静态PEReader探针完成。[verified: Analysis/Evidence/rea-evidence-bundle.json；rea-managed-artifact.json；rea-gameassembly-classification.json；rea-managed-members.json]

Ghidra provider当前没有GHIDRA_INSTALL_DIR，REA所需12.1.x未配置；其诊断要求64位完整JDK21或更高，而实际Java为17.0.12。Hopper不支持当前win32，IDA也未配置。没有通过这些provider完成机器码反汇编；备份CIL和真实生成C++提供了可行的主要静态恢复路线。[verified: rea-initial-session.json result.analysis_provider_candidates][opinion]

## 工具与正确Editor

原始环境未在已检查PATH/候选目录发现ILSpy、Cpp2IL、Ghidra或AssetRipper；边界和原始版本输出见dependencies.md。此后在研究目录便携安装ILSpy11.1.0.9782，取得官方AssetRipper2.0.0并校验SHA256。AssetRipper内置Cpp2IL已实际解析当前metadata39并找到注册地址，不等于安装独立Cpp2IL CLI或恢复了完整原生C#方法体。[verified: dependencies.md；Analysis/Logs/ilspy-project-stdout.log；Analysis/Tools/ILSpy；Reports/unity-recovery.md；asset-ripper-internal.log]

已有Unity2019.4.33f1、2022.3.23f1及2022.3.23f1c1被实物验证，未发现匹配本游戏的6000.3.10f1 Editor；没有用错误版本强行导入。因而本次未完成Unity6000.3.10工程导入、编译和可玩性验证。[verified: Analysis/Environment/tool-inventory.json unity_editors；Reports/dependencies.md；Reports/unity-recovery.md]

现有Unity进程属于用户既有项目，调查未控制或关闭它。AssetRipper曾以6GB托管堆限制进行独立smoke与全量导出，单切片约198MB RSS、完整加载约2.30GB、导出峰值约5.89GB；该进程随后结束。[verified: Reports/unity-recovery.md；Analysis/Assets；Analysis/Logs/asset-ripper*]

## 持久化入口

- game-file-manifest.csv/json：全部源文件及长度/时间/相对路径。[verified: Analysis/Environment/game-file-manifest.csv/json]
- valuable-files.csv：代码、元数据、资源、StreamingAssets/Addressables候选。[verified: Analysis/Environment/valuable-files.csv]
- source-copy-sha256.csv：965文件源/副本身份核对。[verified: Analysis/Environment/source-copy-sha256.csv]
- tool-inventory.json与dependencies.md：已验证的环境、安装工具和缺失依赖边界。[verified: Analysis/Environment/tool-inventory.json；Reports/dependencies.md]
- REA证据bundle与成员错误原始JSON：完整工具证据及覆盖限制。[verified: Analysis/Evidence/rea-evidence-bundle.json；rea-managed-members.json]

Verification: 安装定位、文件清单、965文件SHA256、REA目标分类、真实ILSpy导出及工具版本检查已执行；Unity Editor导入未执行（匹配版本缺失）。[verified: 上述持久化证据]

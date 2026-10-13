# 本机逆向工具与依赖

该检查只读取 PATH、指定工具目录、Unity Hub 的 Editor 登记、进程路径及所选包缓存，没有全盘扫描。未执行安装、下载、大型导出或 Unity 工程导入。[verified: Analysis/Environment/tool-inventory.json scope / limitations]

## 可直接使用的运行时

- .NET SDK 10.0.201；Microsoft.NETCore.App、AspNetCore.App、WindowsDesktop.App 均为 10.0.5。路径 C:\Program Files\dotnet\dotnet.exe。[verified: Analysis/Environment/tool-inventory.json version_probes command="dotnet --list-sdks" / "dotnet --list-runtimes"]
- uv 0.11.3，C:\Users\admin\.local\bin\uv.exe。[verified: Analysis/Environment/tool-inventory.json version_probes command="uv --version"]
- Python 3.12.8，C:\Users\admin\AppData\Local\.aimana\bin\python\python.exe。[verified: Analysis/Environment/tool-inventory.json version_probes command="python --version"]
- Node v22.23.2，C:\Users\admin\AppData\Local\pi-node\current\node.exe。[verified: Analysis/Environment/tool-inventory.json version_probes command="node --version"]
- Oracle Java 17.0.12，C:\Program Files\Common Files\Oracle\Java\javapath\java.exe。[verified: Analysis/Environment/tool-inventory.json version_probes command="java -version"]

## 已定位的 Unity Editor

- 正在运行：E:\hamster_unity\unity20190433\build\WindowsEditor\Unity.exe；产品版本 2019.4.33f1_c49f43f34。[verified: Analysis/Environment/tool-inventory.json unity_processes / unity_editors path="E:\\hamster_unity\\unity20190433\\build\\WindowsEditor\\Unity.exe"]
- C:\unity_release\Unity20190433_release\Editor\Unity.exe；2019.4.33f1_085ad7ea8。[verified: Analysis/Environment/tool-inventory.json unity_editors productVersion="2019.4.33f1_085ad7ea8"]
- E:\Unity20190433_release\Editor\Unity.exe；2019.4.33f1_54a9274d8。[verified: Analysis/Environment/tool-inventory.json unity_editors productVersion="2019.4.33f1_54a9274d8"]
- E:\hamster_unity\unity20220323\build\WindowsEditor\x64\Debug\Unity.exe；2022.3.23f1_1c3a937ba526。[verified: Analysis/Environment/tool-inventory.json unity_editors productVersion="2022.3.23f1_1c3a937ba526"]
- E:\Unity\Editors\2022.3.23f1\Editor\Unity.exe；2022.3.23f1c1_4a0772a0ebd0。[verified: Analysis/Environment/tool-inventory.json unity_editors productVersion="2022.3.23f1c1_4a0772a0ebd0"]
- Hub 的旧登记 E:\hamster_unity\unity20190433\build\WindowsEditor\2022.3.62f3c1\Editor\Unity.exe 已不存在，不能按此登记声称已装 2022.3.62f3。[verified: Analysis/Environment/tool-inventory.json unity_editors exists=false / unity_hub_metadata]
- PATH 中的 C:\Users\admin\AppData\Local\Unity\bin\unity.exe 没有产品版本信息，因此 Editor 选择应使用上述已验证的真实 Editor 路径。[verified: Get-Item output path="C:\\Users\\admin\\AppData\\Local\\Unity\\bin\\unity.exe" productVersion=null][opinion]

Unity Editor 能否导入恢复工程、许可证状态及目标游戏对应版本，均不能从上述文件存在性检查得出；尚未做导入测试。[verified: Analysis/Environment/tool-inventory.json limitations="Unity Editor license/batchmode/project import not tested."]

## 反编译器与资源恢复工具

- ilspycmd / ILSpy：PATH 未发现；C:\Users\admin\.dotnet\tools 不存在；所选 NuGet 缓存未发现 ICSharpCode.Decompiler 或 ilspycmd。[verified: Analysis/Environment/tool-inventory.json commands name="ilspycmd" / "ILSpy", bounded_roots, selected_nuget_packages]
- Cpp2IL：PATH 及指定工具根目录未发现。[verified: Analysis/Environment/tool-inventory.json commands name="Cpp2IL" / "cpp2il", bounded_roots]
- Ghidra / analyzeHeadless：PATH 及指定工具根目录未发现。[verified: Analysis/Environment/tool-inventory.json commands name="ghidraRun" / "analyzeHeadless", bounded_roots]
- AssetRipper / GUI / CLI：PATH 及指定工具根目录未发现。[verified: Analysis/Environment/tool-inventory.json commands name="AssetRipper" / "AssetRipper.GUI" / "AssetRipper.CLI", bounded_roots]
- Mono.Cecil 缓存存在于 C:\Users\admin\.nuget\packages\mono.cecil；该缓存本身不等于已装 ILSpy。[verified: Analysis/Environment/tool-inventory.json selected_nuget_packages Name="mono.cecil"][opinion]

“未发现”仅适用于所检查的边界，不证明这些工具在整台机器上的任何位置都不存在。[verified: Analysis/Environment/tool-inventory.json limitations="No exhaustive whole-disk search. Not found means absent from PATH and inspected bounded roots, not proven absent from host."]

## REA CLI

PATH 未发现 rea，但 npm 的现有缓存包含 rea-agents 6.4.0 的两个副本。可用候选入口如下；本次环境扫描未执行 CLI，因此 CLI 可执行性和 Ghidra provider 状态仍需主调查的实际调用验证。[verified: Analysis/Environment/tool-inventory.json commands name="rea", rea_cached_packages version="6.4.0", cli_execution_tested=false]

```text
node C:\Users\admin\AppData\Local\npm-cache\_npx\af752b90495482d7\node_modules\rea-agents\scripts\rea.mjs
node C:\Users\admin\AppData\Local\npm-cache\_npx\dea9475797100b20\node_modules\rea-agents\scripts\rea.mjs
```

## 官方获取入口候选

以下官方项目主页已通过在线读取确认，但本次环境扫描未获取或安装它们。[verified: Analysis/Environment/tool-inventory.json official_acquisition_candidates verified_online=true / limitations="No software installed, upgraded or downloaded."]

- [ILSpy 官方仓库](https://github.com/icsharpcode/ILSpy)，用于 .NET 程序集反编译。[verified: 官方 README "ILSpy is the open-source, cross-platform .NET assembly browser and decompiler."]
- [Cpp2IL 官方仓库](https://github.com/SamboyCoding/Cpp2IL)，用于 Unity IL2CPP 分析。[verified: 官方仓库 About "Work-in-progress tool to reverse unity's IL2CPP toolchain."]
- [Ghidra 官方仓库](https://github.com/NationalSecurityAgency/ghidra)，当前 master README 的 Install 要求 JDK 25 64-bit；本机 PATH 中只验证了 Java 17。应按所选发行版重新确认 Java 要求。[verified: 官方 README Install L247-L255][verified: Analysis/Environment/tool-inventory.json version_probes command="java -version"][opinion]
- [AssetRipper 官方仓库](https://github.com/AssetRipper/AssetRipper)，用于游戏资源分析。[verified: 官方仓库 About "GUI application to analyze game files"]

Verification: tool-inventory.json 写入后通过 ConvertFrom-Json 重新解析；17 个 PATH 探测项、6 个 Editor 候选、5 个真实 Editor 路径、2 个 REA 缓存包。[verified: 写入检查输出 commands=17 / editor_candidates=6 / found_editors=5 / rea_cached_packages=2]

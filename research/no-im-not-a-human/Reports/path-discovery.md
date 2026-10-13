# 游戏安装路径定位

已确认实际游戏目录为 `E:\steam\steamapps\common\No, I'm not a Human`。[verified: Analysis/Environment/path-discovery.json evidence.gameMarkers]

用户提供的 `D:\SteamLibrary\steamapps\common\NoImNotAHuman` 和 `D:\SteamLibrary` 均不存在。[verified: Analysis/Environment/path-discovery.json boundedSearch.pathProbes]

定位证据：

- Steam 用户注册表 `HKCU\Software\Valve\Steam` 的 `SteamPath=e:/steam`，两个 HKLM Valve Steam 键的 `InstallPath=E:\steam`。[verified: Analysis/Environment/path-discovery.json evidence.registry]
- `E:\steam\steamapps\libraryfolders.vdf` 仅声明 `E:\steam` 库，包含 app `3180070`。[verified: Analysis/Environment/path-discovery.json evidence.libraryConfigText]
- `E:\steam\steamapps\appmanifest_3180070.acf` 声明 `name=No, I'm not a Human`、`installdir=No, I'm not a Human`、`StateFlags=4`、`buildid=23967901`、`SizeOnDisk=2460965987`。[verified: Analysis/Environment/path-discovery.json evidence.manifestFields]
- 实际目录含 `NoImNotAHuman.exe`、`UnityPlayer.dll`、`GameAssembly.dll`、`NoImNotAHuman_Data` 和 `NoImNotAHuman_BackUpThisFolder_ButDontShipItWithYourGame`。[verified: Analysis/Environment/path-discovery.json evidence.gameMarkers]

搜索边界：只读取 Steam 注册表、libraryfolders 配置、两个 app manifest、游戏根目录和 Data 目录的一层清单，并检查 C/D/E/F 盘根目录的 Steam、SteamLibrary、steamapps 候选；没有递归全盘扫描，没有启动游戏，没有修改源游戏文件。[verified: Analysis/Environment/path-discovery.json boundedSearch and sourceMutationsPerformed]

后续分析入口为上述 E 盘游戏目录；研究产物已迁入 MINIGAME 下的本地研究目录，当前位置与原路径映射见[迁移记录](../Analysis/Relocation/relocation.json)。[verified: Analysis/investigation-status.json current_research_root；Analysis/Relocation/relocation.json]

Verification: Test-Path 检查、Steam 注册表读取、libraryfolders 与 appmanifest 读取、游戏根目录及 Data 一层 Get-ChildItem 均成功，五项游戏标志存在。[verified: Analysis/Environment/path-discovery.json]

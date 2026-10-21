# 主场景复古效果调参

打开 `MainRoomCombined`，选择 **MiniGame → 画面 → 复古效果** 或 **调色与像素风格**。Scene／Game 均可预览，进入 Play Mode 后也可以边走动边调参。窗口读取当前场景绑定的资产；当前主场景使用 [RoomLightingReferenceRetro.asset](../UnityProject/Assets/MiniGame/Resources/RoomLightingReferenceRetro.asset)，未绑定时回退到 `RetroScreenDefault.asset`。Scene 请打开场景灯光与 Effects，也可在[房间灯光工具](lighting-tools/guide.md)点击「场景真实预览」。`[verified: RetroScreenWindow.OnGUI；RetroScreenEffect.cs ExecuteAlways/ImageEffectAllowedInSceneView；MainRoomCombined.unity 的 profile 引用；RoomLightingWindow.EnableScenePreview]`

## 参数

- **启用效果**：关闭时显示原图；默认开启。
- **整体强度**：0–1，默认 0.65；混合原图与完整效果，0 为原图。
- **像素尺寸**：1–8 个输出像素，默认 2；1 不额外像素化。
- **色阶分段数**：4–64，默认 24；数值越低，明暗分层越明显。这不是整张画面的颜色总数。
- **暗部层次（前置 Gamma）**：0.25–2，默认 0.8；改变进入色阶量化前的明暗分布。
- **输出明暗（后置 Gamma）**：0.25–3，默认 1.25；改变量化后的明暗分布。

以上是早期轻度预设的范围和默认值，当前房间参考配置以资产实际值为准。`[verified: RetroScreenProfile.cs ResetToLightDefaults；Resources/RoomLightingReferenceRetro.asset]`

建议先调整像素尺寸和色阶分段数，再调整暗部与输出明暗；最后用整体强度融合。先检查墙角、床下和门边，再看电脑界面。`[opinion]`

## 保存与原图对比

面板与配置 Inspector 使用同一套中文控件，修改支持 Undo/Redo。**保存配置**立即写入配置资产；Play Mode 中编辑的也是这个资产。修改及预设切换在退出播放、脚本重载、退出编辑器前自动保存，返回编辑模式仍保留；预设切换可撤销。`[verified: RetroScreenProfileEditor.OnInspectorGUI；RetroScreenPreview.ProfileEdited/SaveEditedProfiles/SaveAndClear]`

**临时查看原图**只改变预览状态，不修改参数。关闭调参面板、切换 Inspector、切换播放状态或脚本重载后恢复效果。`[verified: RetroScreenPreview.SetOriginal/Clear；RetroScreenProfileEditor.OnDisable；RetroScreenWindow.OnDisable]`

效果挂在主场景玩家相机上；对白、菜单和 CRT Overlay 在相机效果之后显示。房间里的海报和物件仍属于 3D 画面，会受到像素化影响。当前场景生成器会重新引用已有默认配置，避免恢复策划已调整的参数为初始值。`[verified: MainRoomGameplayBuilder.cs RetroScreenSetup.Attach；MiniGameScreen.cs EnsureBuilt；RetroScreenSetup.EnsureProfile]`

## 接入与复验

首次接入只添加相机组件，不生成房间。若旧场景缺少组件，退出 Play Mode 后使用 **MiniGame → 画面 → 为当前主场景安装复古效果**，再保存场景。`[verified: RetroScreenSetup.InstallInOpenScene]`

当前工程包含本机既有的 Unity 2022.3 兼容版 Pipeline `0.4.0-exp.1`，保留包内许可证；它用于 CLI 控制 Editor。官方 CLI 自动安装器对此 2022.3 工程返回版本拒绝，因此没有通过自动安装器安装，也没有升级 Unity。`[verified: 本轮 unity pipeline install 返回 requires Unity 6.0 or higher；Packages/com.unity.pipeline/package.json unity/version；LICENSE.md]`

兼容包中三份依赖 DLL 已从 Unity 官方同版本归档恢复，保留原始 `CHECKSUMS` 完整性检查；归档 SHA-1 为 `f49636739437acc8a1b6120b491338e352808553`。`[verified: 本轮 packages.unity.com 返回 dist.shasum；VerifyBundledChecksums 返回通过]`

本工程将 Pipeline 限定为 Editor 使用，避免把编辑器 Roslyn 依赖带入 Player；不提供游戏运行时 CLI 服务。`[verified: Packages/com.unity.pipeline/Runtime/Unity.Pipeline.asmdef includePlatforms=Editor；构建日志 No RuntimePipelineManager components found]`

从仓库根运行：`[opinion]`

```powershell
unity command --project-path ./UnityProject editor_status
unity command --project-path ./UnityProject menu --path "MiniGame/画面/复古效果"
unity command --project-path ./UnityProject run_tests --mode editor --filter RetroScreenTests --async_tests true
unity command --project-path ./UnityProject test_status
unity command --project-path ./UnityProject run_tests --mode playmode --filter RetroScreenPlayModeTests --async_tests true
unity command --project-path ./UnityProject test_status
```

两个测试批次依次执行；等待一个完成再启动下一个。验证记录见 [复古效果验证报告](retro-screen-verification.json)。`[opinion]`

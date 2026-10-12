# Moral Matrix：2D / 3D 表现与交互门控参考

> 静态源代码研究；本页不表示我们的游戏已实现或参考工程已在目标环境运行。

## 仓库与研究基线

| 项目 | 记录 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 原始仓库 | [moral-matrix](https://github.com/sedmugen/moral-matrix) |
| 本地目录 | [references/moral-matrix](../../references/moral-matrix) |
| 固定完整 SHA | `96ee5522390d92c4fb2345e5e7b5cf758ad831a2` |
| 原版引擎 / 管线 | Unity 2023.1.20f1 / URP 15.0.7 |
| 项目性质 | 完整 Unity 2.5D 叙事工程；点击移动、线性对话、房间摄像机和视频演出 |
| 研究优先级 | 中：选择性研究小模块，第一人称控制另行实现 |
| 我们的目标 | Unity 2022.3 LTS；URP 14 主候选、Built-in 对照，具体补丁尚未锁定 |
| 当前验证状态 | 已核查源码、配置、许可和固定 SHA；未导入引擎、未编译、未运行、未测性能 |

版本依据：[仓库记录](../../docs/source-lock.md)。需求依据：[游戏总览](../../docs/game-overview.md)与[程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 与本游戏的对应关系

我们的游戏是第一人称小公寓，按 E 操作电脑、床和门；此工程是鼠标点击地面、NavMesh 移动和房间摄像机。
共同价值在于将 2D 对象放进 3D 场景、交互时冻结移动、切换摄像机与演出后还原。
患者主要显示在独立电脑界面，八方向人物不是五位患者的必需资产。
线性台词仅能参考表现，不能承担多选回复、患者信任、处置与跨日状态。

## 实际实现证据

| 证据 ID | 实际实现或边界 | 本地证据与永久链接 |
| --- | --- | --- |
| E01 | 原版引擎明确为 2023.1.20f1 | [ProjectVersion.txt:1–2](../../references/moral-matrix/ProjectSettings/ProjectVersion.txt) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/ProjectSettings/ProjectVersion.txt#L1-L2) |
| E02 | URP 15.0.7、AI Navigation 和 TextMeshPro 等依赖 | [manifest.json:2–16](../../references/moral-matrix/Packages/manifest.json) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Packages/manifest.json#L2-L16) |
| E03 | 左键射线落地后设置 NavMeshAgent 目的地，不是第一人称 E 操作 | [ClickMovement.cs:22–46](../../references/moral-matrix/Assets/Scripts/ClickMovement.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/ClickMovement.cs#L22-L46) |
| E04 | Billboard 可完整朝向相机或仅水平朝向 | [Billboard.cs:19–32](../../references/moral-matrix/Assets/Scripts/Billboard.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/Billboard.cs#L19-L32) |
| E05 | 按相机相对角度选择八方向并翻转精灵 | [BillboardEightDirections.cs:23–30](../../references/moral-matrix/Assets/Scripts/BillboardEightDirections.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/BillboardEightDirections.cs#L23-L30) |
| E06 | 点击门且接近后切相机、更新引用、传送 | [DoorClickDetector.cs:50–76](../../references/moral-matrix/Assets/Scripts/DoorClickDetector.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/DoorClickDetector.cs#L50-L76) |
| E07 | NPC 展开线性 string 数组对话、禁移动、逐字显示，结束发事件 | [NpcDialogue.cs:53–155](../../references/moral-matrix/Assets/Scripts/NpcDialogue.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/NpcDialogue.cs#L53-L155) |
| E08 | StreamingAssets 视频覆盖显示，直接暂停 timeScale，结束清空 RT 并恢复 | [CutsceneManager.cs:29–76](../../references/moral-matrix/Assets/Scripts/CutsceneManager.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/CutsceneManager.cs#L29-L76) |
| E09 | 持久音源管理与自动播放主题，不是我们的日流程音乐状态 | [AudioManager.cs:14–45](../../references/moral-matrix/Assets/Scripts/AudioManager.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/AudioManager.cs#L14-L45) |
| E10 | AudioMixer 的对数音量换算 | [SettingsMenu.cs:11–18](../../references/moral-matrix/Assets/Scripts/SettingsMenu.cs) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/Assets/Scripts/SettingsMenu.cs#L11-L18) |
| E11 | 根 MIT 许可；保留版权和许可是明确条件 | [LICENSE:1–21](../../references/moral-matrix/LICENSE) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/LICENSE#L1-L21) |
| E12 | 存档与更多多选分支在 README 中仍列为未完成 | [README.md:125–133](../../references/moral-matrix/README.md) · [固定版本](https://github.com/sedmugen/moral-matrix/blob/96ee5522390d92c4fb2345e5e7b5cf758ad831a2/README.md#L125-L133) |

## 四类借鉴结论

| 类别 | 可用方向 | 本游戏边界 |
| --- | --- | --- |
| 直接引入候选 | Billboard 的小型朝向/数学逻辑，对数音量换算 | 保留 MIT；先验证 2022.3，不搬整个项目 |
| 改写结构 | 交互开始/结束事件、移动门控、相机目标更新、视频清场 | 改成自己的第一人称 E 输入与统一阶段状态 |
| 表现参考 | 3D 房间中的纸片对象、局部视频覆盖、房间切换 | 不增加八方向患者、多房间或视频梦境承诺 |
| 自研缺口 | 第一人称控制、电脑连续过渡、光标切换、梦后仅床、跨日患者状态 | 原工程没有完成这些本游戏规则 |

## 原版调用与数据流程

```text
左键地面 → 射线 → NavMeshAgent.SetDestination
左键 NPC 且接近 → 对话面板 → 禁用 ClickMovement
string[] 逐字显示 → 下一句 → CloseText → OnTalkEnd → 恢复移动
点击门且靠近 → 切相机 / 更新引用 / Teleport
PlayVideo → RawImage 覆盖 / timeScale=0 → 结束 → 清理 / 恢复
```

E07 的 CloseText 会把对白替换成拒绝台词，不能保存我们的复杂患者日程。
E06 是立即切相机，不代表已实现靠近电脑到 2D 工作界面的连续过渡。
E08 的全局时间暂停不具备“梦播完可以移动，但仅床可交互”的语义。

## 面向 Unity 2022.3 的改造步骤

1. 在独立 2022.3 验证工程抽取 Billboard 和音量逻辑，不降级原参考工程。
2. 自建第一人称移动与中心视线射线；E 调用交互，不加入跳跃或玩家肢体。
3. 门、电脑、床交给统一阶段状态；进入电脑前记录相机和光标状态。
4. 过渡期间冻结移动和视角，完成后启用 UI；退出时按相反顺序还原。
5. 梦播完恢复移动，仅床进入交互白名单；再按 E 床才推进黑屏、敲门和次日。
6. 对话显示接入另选分支工具；配额、信任、处置和患者状态由游戏系统维护。
7. 若使用视频，记录进入前时间/音频状态并处理正常结束、跳过、错误与取消。

## 依赖与迁移成本

原工程 URP 15 对应 Unity 2023.1；2022.3 路线应验证 URP 14，不能直接沿用其管线资产。[Unity 官方兼容表](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/requirements.html)。
小型朝向模块依赖较少；整场景迁移则带入 NavMesh、模型、材质、动画、视频与相机约定。
先用占位房间、纸片图片和临时声音验证，不将原大型媒体作为最小交付前提。
原 API、材质与实际 Editor 补丁兼容性仍未验证，不能把源码存在当成可运行证据。

## 代码与资产许可

- 根 MIT 见 E11；复用较大代码段时保留版权、许可证和固定版本出处。
- 人物、模型、纹理、音乐、字体、视频和外部依赖要逐项核对；当前未完成资产审核。
- 项目根许可不是每个外部资源已审核的证明，不将原剧情或素材自动列为生产资产。

## 后续验证场景

| 场景 | 预期结果 |
| --- | --- |
| 观察纸片人物 | 朝向稳定，不露错误背面；按实际需要决定方向差分 |
| 连按 E / 对话时移动 | 只进入一次交互，终端与演出输入门控一致 |
| 电脑进入、退出 | 相机与光标正确还原，没有双重交互 |
| 梦境收束 | 可以移动，仅床可交互，次日只推进一次 |
| 视频结束、跳过、失败 | 覆盖层、时间与音频恢复，RT 无旧帧 |
| 2022.3 构建 | 小模块编译与材质通过，记录实际补丁/管线/平台 |

## 最终取舍

将其作为小型表现与交互组织参考，优先级低于分支对话和日流程骨架，不以整项目降级换取不同于需求的控制方式。

相关综合判断：[借鉴矩阵](../reuse-matrix.md) · [Unity 2022 落地路线](../unity-2022-roadmap.md)。

# URP Retro CRT Shader：电脑监控的优先视觉候选

> 静态源代码研究；本页不表示我们的游戏已实现或参考工程已在目标环境运行。

## 仓库与研究基线

| 项目 | 记录 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 原始仓库 | [URP_RetroCRTShader](https://github.com/Cyanilux/URP_RetroCRTShader) |
| 本地目录 | [references/URP_RetroCRTShader](../../references/URP_RetroCRTShader) |
| 固定完整 SHA | `ccd515c616f409e7aabda4fd02a46fa97317de10` |
| 原版引擎 / 管线 | 无完整 ProjectVersion；README 声明 Unity 2022+；实际为 URP Fullscreen ShaderGraph |
| 项目性质 | MIT Graph、材质与双相机低分辨率 RenderTexture 示例；不是游戏框架 |
| 研究优先级 | 中：采用 URP 14 时优先验证终端效果 |
| 我们的目标 | Unity 2022.3 LTS；URP 14 主候选、Built-in 对照，具体补丁尚未锁定 |
| 当前验证状态 | 已核查源码、配置、许可和固定 SHA；未导入引擎、未编译、未运行、未测性能 |

版本依据：[仓库记录](../../docs/source-lock.md)。需求依据：[游戏总览](../../docs/game-overview.md)与[程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 与本游戏的对应关系

我们的监控画面需要轻度扫描线/噪声，处置反应后需要短暂花屏收束。
仓库提供效果材质、Graph 和相机示例，不提供患者、信任、日程或输入管理。
建议让公寓和终端使用不同强度，正常阅读保持清晰；这不是已经确定的风格数值。
低分辨率双相机是可选技术路线，不要求强制像素化中文台词。

## 实际实现证据

| 证据 ID | 实际实现或边界 | 本地证据与永久链接 |
| --- | --- | --- |
| E01 | README 声明 2022+ 与 Fullscreen Graph / FullscreenPassRendererFeature | [README.md:1–7](../../references/URP_RetroCRTShader/README.md) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/README.md#L1-L7) |
| E02 | Graph 实际具有 UniversalFullscreenSubTarget | [Retro.shadergraph:9028–9047](../../references/URP_RetroCRTShader/Retro/Retro.shadergraph) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Retro/Retro.shadergraph#L9028-L9047) |
| E03 | Renderer Asset 实际配置 Full Screen Pass、效果材质与注入点 | [BlitToMain.asset:3–22](../../references/URP_RetroCRTShader/Retro/BlitToMain.asset) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Retro/BlitToMain.asset#L3-L22) |
| E04 | 材质启用弯曲、失真、RGB 条纹、滚动静电等分支 | [Retro.mat:14–20](../../references/URP_RetroCRTShader/Retro/Retro.mat) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Retro/Retro.mat#L14-L20) |
| E05 | 材质保存失真、扫描线、静电和弯曲强度 | [Retro.mat:48–67](../../references/URP_RetroCRTShader/Retro/Retro.mat) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Retro/Retro.mat#L48-L67) |
| E06 | 主摄像机 culling mask 为零，避免重复画示例场景 | [RetroScene.unity:814–831](../../references/URP_RetroCRTShader/Retro/RetroScene.unity) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Retro/RetroScene.unity#L814-L831) |
| E07 | 低分辨率摄像机输出到 RenderTexture | [RetroScene.unity:1036–1049](../../references/URP_RetroCRTShader/Retro/RetroScene.unity) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Retro/RetroScene.unity#L1036-L1049) |
| E08 | 实体屏幕、单相机路线与运行时变体限制有明确说明 | [README.md:19–24](../../references/URP_RetroCRTShader/README.md) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/README.md#L19-L24) |
| E09 | 示例控制器 Q/E 自由上下飞行，不是本游戏 E 交互 | [SimpleCameraController.cs:85–112](../../references/URP_RetroCRTShader/Scripts/SimpleCameraController.cs) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Scripts/SimpleCameraController.cs#L85-L112) |
| E10 | 新输入系统示例路径仍有 TODO | [SimpleCameraController.cs:166–178](../../references/URP_RetroCRTShader/Scripts/SimpleCameraController.cs) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/Scripts/SimpleCameraController.cs#L166-L178) |
| E11 | 根 MIT 许可；保留版权与许可声明 | [LICENSE.md:1–21](../../references/URP_RetroCRTShader/LICENSE.md) · [固定版本](https://github.com/Cyanilux/URP_RetroCRTShader/blob/ccd515c616f409e7aabda4fd02a46fa97317de10/LICENSE.md#L1-L21) |

## 四类借鉴结论

| 类别 | 可用方向 | 本游戏边界 |
| --- | --- | --- |
| 直接引入候选 | 固定版本 Graph、材质与 RT 样例 | 先验证 2022.3 / URP 14，保留 MIT，不称已兼容 |
| 改写结构 | 终端专用显示、状态驱动强度、Renderer 配置 | 画面源和输入门控由我们的工程提供 |
| 表现参考 | 弯曲、扫描线、静电、RGB 条纹和滚动故障 | 不同时堆高全部效果，不遮住中文对白 |
| 自研缺口 | 电脑靠近过渡、UI/光标门控、患者/梦境调度 | 仓库仅解决显示，不承担游戏规则 |

## 原版调用与数据流程

```text
LowRes Camera 渲染场景 → LowResRT
Retro Material 的 _MainTex 读取 LowResRT
Main Camera 不画场景对象 → BlitToMain Renderer
Full Screen Pass 用 Retro Material → 屏幕输出 → 后续后处理
固定效果关键字决定分支；强度参数决定扫描线/噪声/失真
```

E03、E06、E07 共同证明实际存在相机和 Pass 配置，不只是效果宣传。
原版流程不包含 UI 坐标映射、实体电脑材质或连续靠近的相机演出。
处置先完成业务提交，花屏仅反馈结果，不能依赖视觉播放来唯一决定状态推进。
优先考虑固定关键字、只动画强度，减少运行时开关变体的额外工作。

## 面向 Unity 2022.3 的改造步骤

1. 在独立 2022.3 LTS / URP 14 验证工程建立摄像机和终端占位图。
2. 验证 Graph 导入与 Full Screen Pass，记录具体 Editor 和 URP 补丁。
3. 先验证终端画面进入 RT，再连接效果显示，不同时改玩家控制和患者逻辑。
4. 实体电脑表面按 E08 改 Lit/Unlit Graph；独立工作界面可采用终端摄像机/RT。
5. 单摄像机全屏路线按 E08 换为 Blit Source，确认效果仅落在目标阶段或区域。
6. 建正常阅读与花屏反应样例，固定效果分支，用强度参数演出并还原。
7. 电脑过渡期间两种输入都关闭；显示切换完成才启用 UI，退出还原公寓输入。
8. 打包检查 shader 变体和中文可读性，再决定是否保留双相机低分辨率。

## 管线、依赖与成本

URP 14 支持 2022.2–2022.x，符合我们的 2022.3 基线。[Unity 官方兼容表](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/requirements.html)。
Full Screen Pass 使用效果材质和指定注入点；采样颜色需检查 Color/Blit Source 设置。[Unity 官方说明](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-features/renderer-feature-full-screen-pass.html)。
没有完整工程版本声明，2022+ 是作者说明，不是在我们的补丁版本上实际运行的证据。
主要依赖为 URP、ShaderGraph、Renderer 与 RT，无需联网或额外对话框架。
双相机增加离屏渲染和 RT 成本；样例避免重复画场景，不意味着总体成本已测得很低。
Built-in 不能直接套用 URP Fullscreen Graph，另看 CrowFX 或移植后验证。

## 代码与资产许可

- 根 MIT 见 E11，复用 Graph、配置或较大代码时保留版权/许可并登记固定版本。
- 示例画面用于评估；患者、字体、音效和正式美术另行确认来源。
- 仓库不提供五位患者、公寓或梦境内容，效果能力不能补齐制作资产。
- 示例控制器不接入玩家输入，以免 E 变成上飞键。

## 后续验证场景

| 场景 | 预期结果 |
| --- | --- |
| 中文台词与按钮 | 效果后清晰可读，没有裁切、缩放损坏 |
| 正常 → 花屏 → 正常 | 参数恢复，无永久失真和上一患者残留 |
| 实体屏幕 → 工作界面 | 画面连续，没有重复交互和明显跳变 |
| RT / 分辨率变化 | 扫描线尺度合理，画面源正确，不拉伸 |
| 编辑器与发布包 | 所需变体保留，运行时切换若采用仍显示 |
| 暂停 / 退出 / 重进 | 状态重置，公寓不继承强终端效果 |
| 性能对照 | 无效果、单相机、双相机分别测量，记录硬件 |

## 最终取舍

URP 14 下优先验证的效果候选，只承担终端显示；先完成一个患者的阅读与处置花屏，再接入完整日循环。

相关综合判断：[借鉴矩阵](../reuse-matrix.md) · [Unity 2022 落地路线](../unity-2022-roadmap.md)。

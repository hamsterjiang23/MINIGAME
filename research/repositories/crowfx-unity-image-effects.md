# CrowFX：Built-in 效果对照与 URP 备选

> 静态源代码研究；本页不表示我们的游戏已实现或参考工程已在目标环境运行。

## 仓库与研究基线

| 项目 | 记录 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 原始仓库 | [CrowFX-unity-image-effects](https://github.com/Luci0n/CrowFX-unity-image-effects) |
| 本地目录 | [references/CrowFX-unity-image-effects](../../references/CrowFX-unity-image-effects) |
| 固定完整 SHA | `5a322fa7bb1f386323be36b7117d1fb321237020` |
| 原版引擎 / 管线 | 包 2.2.0；声明最低 Unity 2022.3；Built-in 主路径，URP/HDRP 实验适配 |
| 项目性质 | MIT 图像效果包；CRT/VHS/glitch、参数预设及 Editor 工具 |
| 研究优先级 | 中：URP 14 备选；若选 Built-in，则优先验证效果 |
| 我们的目标 | Unity 2022.3 LTS；URP 14 主候选、Built-in 对照，具体补丁尚未锁定 |
| 当前验证状态 | 已核查源码、配置、许可和固定 SHA；未导入引擎、未编译、未运行、未测性能 |

版本依据：[仓库记录](../../docs/source-lock.md)。需求依据：[游戏总览](../../docs/game-overview.md)与[程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 与本游戏的对应关系

本游戏需要可阅读的监控画面和短暂花屏，并不需要完整复古效果大全。
CrowFX 参数丰富，适合 Built-in 技术对照；URP 14 主候选先验证 Cyanilux。
正常、处置、梦境等参数预设值得参考，但不决定剧情，也不补写未知梦境内容。
包声明、实际代码路径与运行验证须分开，最低版本不能推出稳定兼容。

## 实际实现证据

| 证据 ID | 实际实现或边界 | 本地证据与永久链接 |
| --- | --- | --- |
| E01 | 包声明 2.2.0、最低 2022.3，Built-in 主线且 URP/HDRP 实验性 | [package.json:1–7](../../references/CrowFX-unity-image-effects/CrowFX/package.json) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/package.json#L1-L7) |
| E02 | ApplyProfile/SaveToProfile 连接预设与组件参数 | [CrowImageEffects.cs:842–862](../../references/CrowFX-unity-image-effects/CrowFX/CrowImageEffects.cs) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/CrowImageEffects.cs#L842-L862) |
| E03 | OnRenderImage 调 RenderStack；零强度直接透传 | [CrowImageEffects.cs:885–903](../../references/CrowFX-unity-image-effects/CrowFX/CrowImageEffects.cs) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/CrowImageEffects.cs#L885-L903) |
| E04 | 按启用状态执行抖动、残影、故障、VHS/CRT，并释放临时 RT | [CrowImageEffects.cs:951–1005](../../references/CrowFX-unity-image-effects/CrowFX/CrowImageEffects.cs) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/CrowImageEffects.cs#L951-L1005) |
| E05 | CRT shader 有弯曲、扫描线、点阵、噪声与闪烁 | [CE_CRTDisplay.shader:17–32](../../references/CrowFX-unity-image-effects/CrowFX/Stages/CE_CRTDisplay.shader) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/Stages/CE_CRTDisplay.shader#L17-L32) |
| E06 | VHS shader 有 tracking、色彩渗漏、掉带和隔行参数 | [CE_VHSTape.shader:17–24](../../references/CrowFX-unity-image-effects/CrowFX/Stages/CE_VHSTape.shader) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/Stages/CE_VHSTape.shader#L17-L24) |
| E07 | URP 注释明确旧式 14–16 实验路径，没有 RenderGraph 实现 | [CrowFXRendererFeature.cs:8–17](../../references/CrowFX-unity-image-effects/CrowFX/Integrations/URP/CrowFXRendererFeature.cs) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/Integrations/URP/CrowFXRendererFeature.cs#L8-L17) |
| E08 | URP 提交 command buffer，调用即时 RenderStack，再提交输出 | [CrowFXRendererFeature.cs:91–126](../../references/CrowFX-unity-image-effects/CrowFX/Integrations/URP/CrowFXRendererFeature.cs) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/Integrations/URP/CrowFXRendererFeature.cs#L91-L126) |
| E09 | Hidden shader 按名称寻找，构建可剥离后静默透传 | [Performance-and-Builds.md:33–44](../../references/CrowFX-unity-image-effects/Documentation/Performance-and-Builds.md) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/Documentation/Performance-and-Builds.md#L33-L44) |
| E10 | Editor 测试检查 Pass 计数和历史内存，当前未运行 | [CrowFXPipelineTests.cs:24–42](../../references/CrowFX-unity-image-effects/CrowFX/Tests/Editor/CrowFXPipelineTests.cs) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/CrowFX/Tests/Editor/CrowFXPipelineTests.cs#L24-L42) |
| E11 | 根 MIT 许可；保留版权与许可声明 | [LICENSE:1–21](../../references/CrowFX-unity-image-effects/LICENSE) · [固定版本](https://github.com/Luci0n/CrowFX-unity-image-effects/blob/5a322fa7bb1f386323be36b7117d1fb321237020/LICENSE#L1-L21) |

## 四类借鉴结论

| 类别 | 可用方向 | 本游戏边界 |
| --- | --- | --- |
| 直接引入候选 | Built-in 固定版本包、CRT/VHS 阶段、预设组件 | 先在 2022.3 构建验证，保留 MIT |
| 改写结构 | 游戏事件切预设、限制启用阶段、shader 保留登记 | URP 适配单独评估，不拖慢主循环 |
| 表现参考 | 正常轻噪声、处置短故障、阶段预设与调试 | 原预设数值和全套滤镜不是美术标准 |
| 自研缺口 | E 交互、电脑过渡、对话/处置、梦境输入门控 | 效果包完全不承担游戏状态或故事内容 |

## 原版调用与数据流程

```text
CrowFXProfile → ApplyProfile → CrowImageEffects 参数
Built-in 相机 → OnRenderImage → RenderStack
画面进入临时 RT → 按开启阶段处理 → Master Present → 输出
finally 释放临时 RT；残影/运动类另维护历史缓冲
URP Feature → 复制相机色缓冲 → Submit → 即时 RenderStack
复制回相机 → Submit；作者标记 experimental
```

E04 中关闭的阶段不调用对应处理，但栈仍有源画面和最终呈现开销，不写成零成本。
E05 根据实际 RT 尺寸工作，可参考离屏电脑显示；实际连接仍需验证。
E07 的 Unity 6 / URP 17 缺口只写为未来边界，当前 2022.3 无需因此直接淘汰。
E08 的即时绘制与提交混合是具体风险，不能只用“兼容 URP”一句话覆盖。

## 面向 Unity 2022.3 的改造步骤

1. 建与 URP 主候选分开的 Built-in 对照工程，用同一终端占位图与中文对白。
2. 只启用 CRT 和少量 VHS/抖动，验证正常阅读、花屏和恢复，不堆全部效果。
3. 建正常/花屏预设，由我们的会面流程显式应用，退出终端时重置。
4. 专用 RT 产生图像，按选定方式接实体电脑或工作界面。
5. 若评估 URP 14，单独记录 Feature 实验结果，不以 Built-in 结果代替通过。
6. 检查发布包并保护实际使用的 Hidden shader，避免编辑器正常但构建无效果。
7. 检查患者切换、暂停、尺寸变化、重进的历史残留，必要时关闭历史效果。
8. 版本、视觉、构建和性能都可接受才决定接入；否则保留表现参考。

## 管线、依赖与成本

Unity 2022.3 官方说明 OnRenderImage 不由 SRP 调用；URP 需自己的接入，不能只挂组件。[官方 OnRenderImage 文档](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.OnRenderImage.html)。
URP 14 对应 2022.2–2022.x，这个版本关系不证明 CrowFX 实验适配通过。[Unity 官方兼容表](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/requirements.html)。
包无额外必装依赖声明；URP 适配有 URP/Core Runtime 程序集条件。
阶段增加 Pass 和临时 RT，残影/运动类增加历史纹理；成本随分辨率和参数增长。
E10 证明仓库有测试，未证明当前测试通过，也不替代硬件与打包测试。
当前交付不需要 XR、HDRP 或完整胶片工具，不据此增加 Mini Game 范围。

## 代码与资产许可

- 根 MIT 见 E11，引入包、shader 或较大改写时保留版权/许可和完整 SHA。
- 正式患者图、字体、音频、输入纹理仍需自己的资产记录；包许可不能覆盖这些内容。
- Unity 包和额外第三方资源依各自许可，不由 CrowFX 的 MIT 改变条件。
- 效果包不提供我们的剧情、角色与梦境素材。

## 后续验证场景

| 场景 | 预期结果 |
| --- | --- |
| Built-in 正常/花屏 | 效果、参数恢复、中文可读，记录实际补丁 |
| URP 14 对照 | Feature 真正执行，输出正确，无提交异常 |
| 编辑器 / 发布包 | Hidden shader 保留，没有静默直通 |
| 多次患者交接 | 历史纹理不残留上一患者画面 |
| 暂停 / 重进 / RT 尺寸变化 | 无黑屏、旧帧，参数和缓冲恢复 |
| 公寓 / 电脑 / 梦境 | 只作用目标图像，游戏系统维持输入门控 |
| 无效果 / CRT / CRT+VHS | 记录帧率、显存、清晰度和硬件，不声称未测低成本 |

## 最终取舍

保留为 Built-in 对照与 URP 备选；首要目标是患者会面与日循环，不因效果功能多而提前堆叠全部阶段。

相关综合判断：[借鉴矩阵](../reuse-matrix.md) · [Unity 2022 落地路线](../unity-2022-roadmap.md)。

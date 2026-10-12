# Human Please：监控工作界面与恐怖节奏参考

> 静态源代码研究；本页不表示我们的游戏已实现或参考工程已在目标环境运行。

## 仓库与研究基线

| 项目 | 记录 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 原始仓库 | [human-please](https://github.com/lamnguyenkhoa/human-please) |
| 本地目录 | [references/human-please](../../references/human-please) |
| 固定完整 SHA | `802d042324abd39bd47d34e3a589186eb1ef2fbf` |
| 原版引擎 / 管线 | Godot 4.2 / GL Compatibility；应用版本 0.3 |
| 项目性质 | 二维审查工程；Control 界面、人物队列、文书和声光事件 |
| 研究优先级 | 高：优先学习终端交接；整体重写，单独许可模块另评估 |
| 我们的目标 | Unity 2022.3 LTS；URP 14 主候选、Built-in 对照，具体补丁尚未锁定 |
| 当前验证状态 | 已核查源码、配置、许可和固定 SHA；未导入引擎、未编译、未运行、未测性能 |

版本依据：[仓库记录](../../docs/source-lock.md)。需求依据：[游戏总览](../../docs/game-overview.md)与[程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 与本游戏的对应关系

监控人物区、桌面文书、按钮与交接遮挡，对我们的电脑会面有直接参考价值。
原版是 Godot 二维边检场景，没有第一人称公寓、床或 3D 到 2D 连续过渡。
允许/拒绝不能替代治疗/净化/释放；原版合法性真值也不是我们的患者道德判断。
护照检查、随机排序、停电和手电筒不因此成为已批准游戏需求。

## 实际实现证据

| 证据 ID | 实际实现或边界 | 本地证据与永久链接 |
| --- | --- | --- |
| E01 | 配置标明 Godot 4.2、GL Compatibility 和主菜单入口 | [project.godot:11–24](../../references/human-please/project.godot) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/project.godot#L11-L24) |
| E02 | 工作开始重置计数，选择后发事件，再延迟载入下一人 | [game_manager.gd:34–69](../../references/human-please/singleton/game_manager.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/singleton/game_manager.gd#L34-L69) |
| E03 | 监控遮挡、人物显示、按钮禁用和画面缩放实际存在 | [camera_area.gd:35–105](../../references/human-please/scene/camera_area.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/scene/camera_area.gd#L35-L105) |
| E04 | 每日文档/物品入桌面，处置后收走当前人物文书 | [working_area.gd:18–44](../../references/human-please/ui/working_area.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/ui/working_area.gd#L18-L44) |
| E05 | 拖动、右键放大与还原、文书区域限制 | [moveable_document.gd:29–58](../../references/human-please/entity/document/moveable_document.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/entity/document/moveable_document.gd#L29-L58) |
| E06 | 放大时保存父容器/位置/缩放，退出后还原 | [zoom_view.gd:16–39](../../references/human-please/ui/zoom_view.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/ui/zoom_view.gd#L16-L39) |
| E07 | 过场切 BGM，定时断电停止音乐并开启暗色遮罩和屏幕光 | [work_day.gd:49–85](../../references/human-please/scene/work_day.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/scene/work_day.gd#L49-L85) |
| E08 | 手电是跟随鼠标的 PointLight2D，不是三维公寓照明 | [flashlight.gd:1–4](../../references/human-please/entity/tool/flashlight.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/entity/tool/flashlight.gd#L1-L4) |
| E09 | 监控场景实际绑定 CRT/VHS 参数材质 | [CameraArea.tscn:244–266](../../references/human-please/ui/CameraArea.tscn) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/ui/CameraArea.tscn#L244-L266) |
| E10 | 独立 shader 明确 CC0 与原始出处，只覆盖此模块 | [crt_vhs.gdshader:1–18](../../references/human-please/shader/crt_vhs.gdshader) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/shader/crt_vhs.gdshader#L1-L18) |
| E11 | sound_manager 插件单独 MIT，不覆盖整个游戏 | [LICENSE:1–21](../../references/human-please/addons/sound_manager/LICENSE) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/addons/sound_manager/LICENSE#L1-L21) |
| E12 | 原版合法性判断来自护照、访问事由和号码等条件 | [game_manager.gd:71–80](../../references/human-please/singleton/game_manager.gd) · [固定版本](https://github.com/lamnguyenkhoa/human-please/blob/802d042324abd39bd47d34e3a589186eb1ef2fbf/singleton/game_manager.gd#L71-L80) |

## 四类借鉴结论

| 类别 | 可用方向 | 本游戏边界 |
| --- | --- | --- |
| 直接引入候选 | 单独 CC0 shader 算法的移植评估 | Godot 代码不能直接进 Unity，许可不覆盖整体游戏 |
| 改写结构 | 患者就绪/结束事件、交接遮挡、控件启停、文书归属 | Unity 重写；按病人终止状态排次日队列 |
| 表现参考 | 分区监控、声音先行、局部亮光、短暂信号故障 | 不新增护照、随机停电、手电系统与资产 |
| 自研缺口 | E 交互、电脑过渡、三处置、信任/制度评估、梦后仅床 | 原工程没有这些本游戏规则的完整实现 |

## 原版调用与数据流程

```text
start_work → 重置计数 → 首人提示 → load_next_character
CharacterResource 入队 → next_subject_readied → 人物 / 文书更新
allow / deny → 统计合法性 → subject_resolved
CameraArea 遮挡禁按钮 + WorkingArea 收文书 → 下一人
队列结束 → end_day → ResultScreen；过场按配置换 BGM
lightout → 停音乐 → 暗色遮罩 + 屏幕局部光
```

E02 的信号把数据推进与视听组件连接，适合改成我们的患者会面事件。
E12 有可判定合法/非法的答案；本游戏仪器只测情绪，不据此给人贴正确/错误标签。
E03 依据当前有没有人物禁按钮；我们还要增加“对话未结束不能处置”的门槛。
E05–E06 可启发任务单查看；档案与日记是否制作仍遵循待定设计。

## 面向 Unity 2022.3 的改造步骤

1. 自建终端 Canvas，将人物、文本、文书与三种处置分成独立区域。
2. 集中管理患者就绪、对话中、可处置、反应、交接遮挡五个界面阶段。
3. 次日队列读取我们的患者状态；终止人物不机械地重排，信任阈值使用待定配置。
4. 普通/特殊对白结束后才启用处置；隐藏绿键与灯色不照搬 allow/deny。
5. 处置先提交一次状态、配额和实际记录，再播反应与花屏，防止连续点击重复提交。
6. 交接时清理旧头像和临时文书，再启用下一位，避免残留操作。
7. 公寓进入/退出电脑、光标切换和梦境床白名单交给独立 3D 流程。

## 依赖与迁移成本

Godot 的 Control、PackedScene、信号、Tween、PointLight2D 都需 Unity 对应实现，整体是跨引擎重写。
成本主要在交互和中文布局；先用占位患者、一份任务单做完整会面，不必复制全部桌面机制。
二维屏幕光不能解决第一人称公寓照明；Godot shader 也不证明 Unity 管线兼容。
URP 14 的效果可用 Full Screen Pass 建立，优先与 Cyanilux 候选对照。[Unity 官方 Full Screen Pass 文档](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-features/renderer-feature-full-screen-pass.html)。
Built-in 是另一个验证路线，不把 Godot 效果文件认定为任意 Unity 管线可直接导入。

## 代码与资产许可

- 当前代码树未发现整体游戏根许可证，整体代码与资源按学习/重写处理，不直接搬运。
- E10 的 CC0 仅覆盖 CRT/VHS shader；保留原始出处，移植后仍需验证。
- E11 的 MIT 仅覆盖 sound_manager 插件，不扩展到整个游戏。
- 音效文件名含外部作者或素材编号，不能代替许可证；人物、字体、音乐与图片逐项确认。
- 可学习分区、事件交接和节奏，正式实现使用自己的代码与来源明确的素材。

## 后续验证场景

| 场景 | 预期结果 |
| --- | --- |
| 会面开始 / 对话中 | 三处置不能提前提交，界面状态一致 |
| 对话后连续点击 | 只提交一次，配额与实际记录只改变一次 |
| 治疗后次日 / 终止人物 | 队列仅保留可以继续出现的患者 |
| 花屏与交接 | 旧文本/文书/头像清理，期间输入关闭 |
| 文书放大退出 | 父容器与布局恢复，不跨患者残留 |
| 电脑退出 / 梦播完 | 公寓输入恢复，梦后仅床可推进次日 |
| 中文与分辨率变化 | 台词与按钮清晰，噪声不损坏关键信息 |

## 最终取舍

优先借鉴会面交接和声光节奏，整体用自己的 Unity 实现；单独许可 shader 不能使整个游戏变成可直接复用源码。

相关综合判断：[借鉴矩阵](../reuse-matrix.md) · [Unity 2022 落地路线](../unity-2022-roadmap.md)。

# 游戏源码借鉴研究

> 研究日期：2026-10-12。目标：Unity 2022，默认以 2022.3 LTS 为兼容性基线。研究依据是本地固定提交的实际源码；尚未导入、编译、试玩或打包参考项目。

我们要制作的是第一人称、2D/3D混合的心理恐怖叙事游戏：检定员在员工公寓中通过电脑与五位病人交流，在治疗、净化与释放之间作出选择。当前 Mini Game 目标为 **3天、5位病人、3种结局**。游戏定位与交付边界见[游戏总览](../docs/game-overview.md)，具体操作见[程序需求](../docs/sources/program-requirements-v0-1.md)。

这份研究回答三个问题：哪些现成代码与我们的需求相符；借鉴后需要怎样改造；哪些关键功能仍需要自行开发。Unity 2022 是本次用户指定的技术方向，2022.3 LTS是研究默认值；它不改变原始飞书设计文档各自的口径。

## 先读哪些文档

1. [借鉴矩阵](reuse-matrix.md)：按实际游戏功能查找代码来源，辨认现成能力与自研部分。
2. [Unity 2022路线图](unity-2022-roadmap.md)：从一个病人的完整会面开始，扩展到五人三天。
3. [风险与缺口](risks-and-gaps.md)：查看版本、许可、存档边界及尚未确定的设计。
4. 下表中的逐仓库文档：核对调用流程、源码位置、固定版本链接与改造步骤。

共19份Markdown：本入口、三份综合文档与15份独立仓库研究。仓库原始版本和获取核验记录见[版本记录](../docs/source-lock.md)与[机器可读记录](../docs/source-lock.json)。

## 最值得采用的组合

| 要解决的问题 | 优先研究来源 | 采用方式 |
| --- | --- | --- |
| 每天做什么、什么条件可以睡觉 | [Do-You-Trust-Them](repositories/do-you-trust-them.md) | 学习每日内容配置、活动完成和切日通知；在自己的工程重写 |
| 处置怎样改变配额与后续人物状态 | [DeathAndTaxes](repositories/death-and-taxes.md) | 提炼当日要求、逐人处置、后果记录与日终反馈；去掉Articy和原游戏指标耦合 |
| 多轮对话、有限选项和条件对白 | [Yarn Spinner](repositories/yarn-spinner-unity.md) | 优先验证并引入固定版本，自己补业务状态与会谈检查点；[Ink](repositories/ink-unity-integration.md)作为备选 |
| 电脑工作界面与病人进出 | [Human, Please](repositories/human-please.md) | 学习监控区、文书区、操作区及交接遮挡；用Unity重新实现 |
| 事件拆分与信息逐层揭露 | [Gatekeeper](repositories/gatekeeper-insight.md) | 重写事件和展示结构；检测读数不映射为客观善恶或标准答案 |
| 公寓中的2D资产、梦境与交互隔离 | [Moral Matrix](repositories/moral-matrix.md)、[The Guest](repositories/the-guest.md) | 选择性参考呈现和输入责任划分；自行完成第一人称控制与电脑过渡 |
| CRT扫描线与处置花屏 | [Retro CRT](repositories/urp-retro-crt-shader.md) | URP14路线优先验证，效果先限定在电脑画面；[CrowFX](repositories/crowfx-unity-image-effects.md)作为Built-in候选 |

这里的“采用”是后续开发建议。当前没有建立本游戏工程，也没有把任何框架或素材导入成品。

## 15个仓库索引

优先级以当前Mini需求的贴合程度排序，不表示完整度或运行成功。迁移成本是结构判断，不是工期承诺。

| 仓库研究 | 性质与记录版本 | 优先级 | 主要借鉴点 | 复用边界 |
| --- | --- | --- | --- | --- |
| [DeathAndTaxes](repositories/death-and-taxes.md) | 正式游戏公开代码；Unity2019.2.10f1 | 高 | 每日裁决、指标后果、角色ID状态、保存时机 | 源码MIT；部分素材与依赖未公开，第三方许可另核 |
| [Do-You-Trust-Them](repositories/do-you-trust-them.md) | 部分Unity脚本；未提供工程版本 | 高 | 按天配置、活动门控、夜间收束 | 未发现本体根许可；部分依赖脚本缺失 |
| [YarnSpinner-Unity](repositories/yarn-spinner-unity.md) | 对话框架3.2.8；声明Unity2022.3 | 高 | 分支、条件选项、完成事件、变量桥接 | MIT；需补完整业务存档与会谈检查点 |
| [human-please](repositories/human-please.md) | Godot4.2游戏工程 | 高 | 监控界面、交接遮挡、灯光与音效 | 本体未见根许可；独立CRT shader标CC0，插件许可不覆盖本体 |
| [gatekeeper-insight](repositories/gatekeeper-insight.md) | Godot4.4游戏工程 | 中 | 事件总线、日终统计、观察信息层级 | MIT；Godot结构在Unity重写，素材来源另核 |
| [ink-unity-integration](repositories/ink-unity-integration.md) | 叙事框架2.0.0；声明Unity2022.3 | 中，备选 | 分支汇合、条件文本、叙事状态JSON | MIT；需自建监控对话呈现，避免与Yarn双运行时 |
| [moral-matrix](repositories/moral-matrix.md) | Unity2023.1.20f1／URP15.0.7 | 中 | 纸片人物朝向、摄像机切换、视频与音效 | MIT；点击NavMesh不是第一人称；不建议整工程降级 |
| [The-Guest](repositories/the-guest.md) | Unity6000.0.58f2／URP17.0.4 | 中 | 人物配置／运行记忆分离、E交互和输入隔离 | 未发现本体根许可；不默认采用自由输入LLM或整套Unity6依赖 |
| [URP_RetroCRTShader](repositories/urp-retro-crt-shader.md) | URP效果示例；无工程版本文件 | 中，URP优先 | RenderTexture、CRT、扫描线、噪声 | MIT；实体电脑材质与全屏效果需分别接线和打包验证 |
| [CrowFX-unity-image-effects](repositories/crowfx-unity-image-effects.md) | 特效包2.2.0；声明Unity2022.3 | 中，Built-in候选 | CRT／VHS／glitch效果与预设 | MIT；URP/HDRP实验性，需检查Shader剥离与性能 |
| [DeadLetterOffice_Day1](repositories/dead-letter-office-day1.md) | Unity2022.3.16f1；第一天工程 | 中低 | 教学插入、行政反馈、纸面交互 | 未发现本体根许可；邮件随机校验不构成我们的病人系统 |
| [Papers-Please_Unity](repositories/papers-please-unity.md) | Unity2020.3.25f1；未完成Jam工程 | 低 | 单人资料显示、二键处置、拖放 | 未发现项目许可；缺多天、信任与会谈门控 |
| [im-a-blind-human](repositories/im-a-blind-human.md) | 原游戏IL2CPP／MelonLoader无障碍MOD | 低，专项参考 | 输入上下文、覆盖层占用、集中处理文本事件 | 未发现本体根许可；依赖原游戏程序集，不是完整游戏源码 |
| [NINAHSaveEditor](repositories/ninah-save-editor.md) | Windows Forms／.NET Framework4.8工具 | 低，调试参考 | 状态分类、日期与物体开关编辑 | Unlicense；原游戏类型名与序列化结构不可作为我们存档接口 |
| [NINAH-Docs](repositories/ninah-docs.md) | 攻略网站与MDX内容 | 低，内容测试参考 | 线索成因、机制条件、结局测试情景的表达 | File to Downloader自定义许可；分析方法可参考，正文和原作图片不直接复制 |

## 怎样读证据

每份研究文档提供本地源码链接与完整SHA的GitHub永久链接，标签标明对应行号。上游后续更新不改变本轮研究所指向的代码。各文档以E01等编号组织证据；综合文档链接到相应逐库研究。

| 标记 | 含义 | 当前状态 |
| --- | --- | --- |
| 静态已核查 | 已阅读实现、调用位置或项目配置 | 本轮已执行，具体证据逐库列出 |
| 包声明兼容 | package.json或官方文档声明最低版本 | 可用于候选筛选，不等同于导入成功 |
| 实际运行验证 | Editor导入、编译、Play Mode、目标平台打包 | 本轮未执行，场景列入后续清单 |

借鉴分为四类：**直接引入候选**用于许可和版本可核对的框架／效果；**改写结构**用于机制与数据组织；**表现参考**用于监控节奏、灯光、声音和视觉；**自研缺口**用于没有现成匹配实现的需求。缺少许可证的游戏脚本按结构研究处理，不列为可直接纳入发行项目的代码。

## 需要我们自己完成的核心

- 第一人称公寓、E交互、连续靠近电脑、开机遮挡、光标切换和退出还原。
- 五位固定病人的独立隐藏信任、治疗次数／差分、返场与终止状态。
- 普通会谈结束后特殊对白，再解锁治疗／净化／释放，最后播放反应与花屏。
- 配额扣减、制度对玩家的隐藏评估，以及三种结局的条件、时点和优先级。
- 跨日事件、完整业务存档及会谈检查点，避免读档后重复处置或重复播放一次性事件。

阈值、配额、结局条件和外交官保护规则等继续保留为[设计待定项](../docs/open-questions.md)。研究不补造正式规则。旧报告的随机访客、食品／精力、生存资源、服装身份和自由输入LLM属于其他机制建议，不自动进入本游戏。

## 版本依据与材料保留

[Yarn官方安装说明](https://docs.yarnspinner.dev/yarn-spinner-for-unity/installation-and-setup)声明Yarn Spinner 3支持Unity2022.3及以上；本地包也声明2022.3。[Unity官方URP兼容表](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/requirements.html)列出URP14对应2022.2–2022.x。本研究因此采用2022.3 LTS＋URP14为主要候选组合，同时记录Built-in下的CrowFX路线。

[Dependency原研究材料](../Dependency)保留作为早期技术参考；[docs](../docs/README.md)保留逐篇设计来源；[references](../references)保留原始仓库。正式开发前需要确定具体2022.3补丁版本、目标平台和渲染管线，再按路线图完成实际验证。

## 文档检查结果

本轮文档检查已通过：19份Markdown覆盖15个仓库，305个本地链接均有对应目标；143组源码证据的本地文件、固定提交、Git路径及行号范围一致。原有65个资料文件的内容哈希、15个参考仓库的HEAD与工作区状态均保持原样。

借鉴矩阵覆盖25项需求，路线图另列14个后续游戏验证场景。上述结果是文档检查，不是引擎编译、游戏运行或发布测试通过记录。

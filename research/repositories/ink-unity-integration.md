# Ink：分支回流与叙事存档的备选

## 研究基线

| 项目 | 内容 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 本游戏目标 | Unity 2022.3 LTS；3 天、5 位病人、3 种结局 |
| 原仓库 | [inkle/ink-unity-integration](https://github.com/inkle/ink-unity-integration) |
| 本地目录 | [references/ink-unity-integration](../../references/ink-unity-integration/) |
| 固定提交 | `aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6` |
| 实际版本 | Integration 包 2.0.0；包声明 Unity 2022.3；仓库工程 2022.3.62f3 |
| 仓库性质 | Ink 运行时、Unity 集成与基础示例 |
| 建议优先级 | 中：叙事框架备选 |
| 验证状态 | 已读取本地源码与版本 / 许可文件；未导入、未编译、未运行 |

版本来自 [来源锁定记录](../../docs/source-lock.json)。本页结论是静态研究，不能据此宣称 Unity 2022.3 中可运行。
游戏需求依据 [游戏总览](../../docs/game-overview.md) 与 [程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 对本游戏的结论

Ink 的分支与 gather 汇合非常适合不同回答后回到共同病人台词。
其 StoryState 提供叙事运行状态的 JSON 存取，值得在有会谈中途恢复需求时重点验证。
首版仍将 Yarn 作为主选，Ink 为备选；不要为了比较长期保留两套正式对话运行时。
若团队更看重长篇文本编写、汇流分支和叙事进度恢复，可以在验证后改选 Ink。

## 源码证据

| ID | 已核验的代码 / 文本位置 | 永久来源 | 说明 |
| --- | --- | --- | --- |
| E01 | [Packages/com.inkle.ink-unity-integration/package.json:1–6](../../references/ink-unity-integration/Packages/com.inkle.ink-unity-integration/package.json) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/Packages/com.inkle.ink-unity-integration/package.json#L1-L6) | 集成包版本及最低 Unity 声明 |
| E02 | [Assets/Ink/Demos/Basic Demo/Scripts/BasicInkExample.cs:18–64](../../references/ink-unity-integration/Assets/Ink/Demos/Basic%20Demo/Scripts/BasicInkExample.cs) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/Assets/Ink/Demos/Basic%20Demo/Scripts/BasicInkExample.cs#L18-L64) | Story 创建、输出、选项与选择接口 |
| E03 | [Assets/Ink/Demos/Basic Demo/Ink/story.ink:1–19](../../references/ink-unity-integration/Assets/Ink/Demos/Basic%20Demo/Ink/story.ink) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/Assets/Ink/Demos/Basic%20Demo/Ink/story.ink#L1-L19) | 分支及 gather 汇合 |
| E04 | [Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/StoryState.cs:17–60](../../references/ink-unity-integration/Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/StoryState.cs) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/StoryState.cs#L17-L60) | 状态格式与叙事运行状态 JSON 存取 |
| E05 | [Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/Story.cs:2110–2158](../../references/ink-unity-integration/Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/Story.cs) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/Story.cs#L2110-L2158) | 外部函数与动作绑定 |
| E06 | [Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/Story.cs:2456–2493](../../references/ink-unity-integration/Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/Story.cs) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/Packages/com.inkle.ink-unity-integration/InkLibs/InkRuntime/Story.cs#L2456-L2493) | 叙事变量观察接口 |
| E07 | [LICENCE.md:1–12](../../references/ink-unity-integration/LICENCE.md) | [固定版本](https://github.com/inkle/ink-unity-integration/blob/aa7b8b4f78e2046f203e47f89a10a6f9f9ba67c6/LICENCE.md#L1-L12) | MIT 许可 |

## 四种借鉴分类

| 分类 | 本库对应内容 | 我们的处理 |
| --- | --- | --- |
| 直接引入 | MIT 的 Ink runtime 与 Unity integration | 备选验证通过后固定版本引入 |
| 改写结构 | Story / choices / variables / 外部函数桥接 | 建立监控会谈适配层，连接病人业务状态 |
| 表现参考 | BasicInkExample 动态生成文本与选项 | 参考最小连接方式，重做逐句、打字及病人 UI |
| 自研缺口 | 配额、去留、处置阶段、日循环、房间输入、业务存档 | 由自己的游戏状态层实现 |

## 适配的数据流程

1. 日循环选择某病人的当天内容入口，并将必要的病人状态交给叙事适配器。
2. Story 在可继续时输出下一句，适配器呈现角色、文字、打字效果和监控演出。
3. 读取 currentChoices，检查最多四个选项，再绑定玩家选择与 ChooseChoiceIndex。
4. 分支正文可以更新叙事变量或调用业务桥接；信任更新只提交到当前病人。
5. 各回答通过 gather / 跳转回到公共轮次，避免日后文本成倍复制。
6. 普通会谈后根据配置与已触发标记进入特殊对话，随后通知处置 UI。
7. 玩家处置先由业务层记录，再跳入对应反应内容，完成后关闭监控。
8. 存档同时保存 StoryState JSON 和游戏业务状态，读取时恢复二者的一致快照。

阈值仍按病人与日期配置，不把示例值固化为统一规则。
如果为每位病人创建独立 Story，应明确每个 Story 的生命周期和跨日保存策略。
如果共用一个 Story，应以稳定病人标识分隔信任与事件状态，防止变量命名冲突。
当前资料没有决定存档结构，这些是验证路线中的建议，不是已存在的游戏接口。

## 实施步骤

1. 在 Unity 2022.3 验证工程导入固定 integration 包，编译一份原创短文本。
2. 写两轮会谈、不同信任变化与公共 gather，检查编辑器编译与运行输出。
3. 自己实现逐句前进与选项面板，不照用一次读取全部连续文字的示例节奏。
4. 用外部函数桥接信任变化、监控演出及会谈完成消息，明确调用副作用。
5. 加入特殊对话、处置反应及日循环的入口选择。
6. 验证 StoryState 与病人业务快照恢复，最后与 Yarn 的编写和接线成本比较。

BasicInkExample（E02）是最小教学示例，不是完成的病人会谈系统。
其中循环一次 Continue 到无后续内容，再创建所有按钮；不符合逐句监控交流的现成要求。
旧式 Unity UI 文本示例也需要接入我们实际的中文字体与监控 UI。

## 叙事状态与业务存档

E04 的 ToJson / LoadJson 直接面向叙事运行状态，且源码含状态格式版本号。
它比只保存变量更适合验证“会谈中途继续”，但并不自动保存房间、配额或治疗记录。
业务存档至少需要日期、当前病人、业务阶段、治疗次数、处置记录及下一日候选患者。
叙事状态与业务状态应在同一提交点保存，避免文本已完成而业务处置尚未提交的错位。
修改剧情节点或升级 runtime 后，旧状态能否继续加载也需要实际验证。
不因存在 ToJson 就声称任意剧情版本之间自动兼容。

## Unity 2022.3 兼容与迁移成本

E01 声明 Unity 2022.3；仓库 ProjectVersion.txt 指向 2022.3.62f3，接近目标工程基线。
主要成本是会谈表现层、业务桥接、内容组织和一致存档，不必重写 Ink 解析器。
变量观察与外部函数（E05、E06）可连接业务，但必须决定状态归属，避免两边同时修改同一信任。
本地尚未导入或运行；包版本 2.0.0 是 integration 包版本，不能代称整个 Ink 语言生态版本。
基础示例不证明异步打字、镜头转换、监控闪屏或所有中文文本表现已经完成。

## 与 Yarn 的取舍

| 需求 | Ink 的判断 | 当前建议 |
| --- | --- | --- |
| 固定轮次、有限回答 | Story choices 可以实现，四选项上限需要自检 | 两者都可用 |
| 不同回答回流 | gather 语法与示例直接表达汇合 | Ink 优势，优先让编剧试写 |
| Unity 对话呈现 | 基础示例简单，监控呈现需要自己接 | 首版 Yarn 接口更贴近会谈 UI |
| 会谈中途存档 | StoryState 提供运行状态 JSON | Ink 值得重点验证 |
| 日循环与结局 | 不属于完整游戏业务状态 | 两者均需自研 |

## 代码与资产许可

E07 明确 integration 与 Ink 使用 MIT；保留随软件分发所需的版权和许可。
示例故事、第三方 UI / 字体及素材的范围需要按实际文件来源核对。
用原创病人文本和自己的终端界面进行验证，不复制示例对白成为游戏内容。

## 后续验证场景

- 两个回答汇合后只输出一次公共台词。
- 四选项有效，超上限时内容检查报告明确位置。
- 每位病人信任与特殊对话标记独立。
- 逐句打字和跳过不会意外 Continue 两次。
- 保存于选项前 / 后并恢复，文本与业务状态一致。
- 外部函数副作用不会在读取后重复改变信任或扣配额。
- 剧情改版后旧存档有明确拒绝 / 迁移结果。
- Unity 2022.3 导入、编译、中文字体、输入锁及处置回调实测。

关联阅读：[需求借鉴矩阵](../reuse-matrix.md) · [Unity 2022 路线](../unity-2022-roadmap.md)。

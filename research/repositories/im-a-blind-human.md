# im-a-blind-human：输入边界与文本可访问性参考

## 研究基线

| 项目 | 内容 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 本游戏目标 | Unity 2022.3 LTS；3 天、5 位病人、3 种结局 |
| 原仓库 | [ObjectInSpace/im-a-blind-human](https://github.com/ObjectInSpace/im-a-blind-human) |
| 本地目录 | [references/im-a-blind-human](../../references/im-a-blind-human/) |
| 固定提交 | `adcb4d1f814a56de24e76a38ac546427b9496be3` |
| 实际版本 | .NET 6；要求 Windows 原游戏、MelonLoader 0.7+；README 描述原游戏 Unity 6.3 / IL2CPP |
| 仓库性质 | 原游戏的无障碍 MOD，不是完整游戏源码 |
| 建议优先级 | 低：专项参考 |
| 验证状态 | 已读取本地源码与版本 / 许可文件；未导入、未编译、未运行 |

版本来自 [来源锁定记录](../../docs/source-lock.json)。本页结论是静态研究，不能据此宣称 Unity 2022.3 中可运行。
游戏需求依据 [游戏总览](../../docs/game-overview.md) 与 [程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 对本游戏的结论

最有价值的是其明确区分世界漫游与 UI 占用，避免覆盖层打开时仍触发 3D 互动。
我们的公寓、电脑终端、处置弹窗、梦境和暂停也必须拥有清楚的输入边界。
本库还示范将人物台词、系统提示与结局文本接到朗读 / 日志事件，便于后续可访问性扩展。
它依赖已安装原游戏的程序集和运行状态，不能当独立 Unity 工程直接移植。

## 源码证据

| ID | 已核验的代码 / 文本位置 | 永久来源 | 说明 |
| --- | --- | --- | --- |
| E01 | [README.md:3–13](../../references/im-a-blind-human/README.md) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/README.md#L3-L13) | MOD 性质、原游戏、平台与 MelonLoader 要求 |
| E02 | [NoImNotAHumanAccess.csproj:3–53](../../references/im-a-blind-human/NoImNotAHumanAccess.csproj) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/NoImNotAHumanAccess.csproj#L3-L53) | 框架目标、原游戏及 IL2CPP 引用 |
| E03 | [src/World/InputModeGate.cs:8–63](../../references/im-a-blind-human/src/World/InputModeGate.cs) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/src/World/InputModeGate.cs#L8-L63) | UI 占用状态与世界输入的安全门控 |
| E04 | [src/World/InputContext.cs:7–59](../../references/im-a-blind-human/src/World/InputContext.cs) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/src/World/InputContext.cs#L7-L59) | 输入上下文分类及覆盖层优先检查 |
| E05 | [src/Dialogue/DialoguePatches.cs:14–28](../../references/im-a-blind-human/src/Dialogue/DialoguePatches.cs) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/src/Dialogue/DialoguePatches.cs#L14-L28) | 不同文本输出入口与原游戏 hook |
| E06 | [src/Dialogue/DialogueNarrator.cs:72–116](../../references/im-a-blind-human/src/Dialogue/DialogueNarrator.cs) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/src/Dialogue/DialogueNarrator.cs#L72-L116) | 清理文本、说话者、去重与朗读提交 |
| E07 | [README.md:44–58](../../references/im-a-blind-human/README.md) | [固定版本](https://github.com/ObjectInSpace/im-a-blind-human/blob/adcb4d1f814a56de24e76a38ac546427b9496be3/README.md#L44-L58) | 原游戏 Unity 6.3 / IL2CPP 的作者说明与构建依赖 |

## 四种借鉴分类

| 分类 | 本库对应内容 | 我们的处理 |
| --- | --- | --- |
| 直接引入 | 没有覆盖 MOD 本体的根许可；且依赖原游戏运行环境 | 不直接引入 MOD 或原游戏程序集 |
| 改写结构 | 输入上下文、UI 占用门控、文本事件、选项焦点维护 | 在自己的 Unity 工程原创实现同类职责 |
| 表现参考 | 键盘操作提示、可重复读文本、说话者名与文本清理 | 可作为后续字幕 / 读屏体验设计参考 |
| 自研缺口 | 固定剧情、病人状态、配额、处置、日循环、完整存档 | MOD 不提供我们的游戏业务系统 |

## 适配的数据流程

1. 将房间漫游、终端会谈、处置、梦境演出和暂停定义为明确的输入上下文。
2. 打开 UI 时申请输入占用，关闭时释放；世界互动只在允许的上下文执行。
3. 镜头过渡期间也占用输入，避免同一帧同时进入电脑又触发床或门。
4. UI 获得鼠标与选项导航；恢复房间时还原光标、视角及 E 互动。
5. 每条台词提供说话者、最终文本、文本类别和稳定内容标识。
6. 会谈、系统提示、晨检、结局统一发布文本事件，字幕、日志与可选朗读订阅。
7. 打字效果可更新可见字符，但同一最终台词事件只提交一次。
8. 暂停 / 取消 / 场景切换清理输入占用与文本任务，不留下持续锁定。

这是推荐的原创工程职责划分；本库实际通过反射读原游戏字段与方法。
不应在我们的工程中重做原游戏 IL2CPP 反射，而应在自己拥有的状态接口上实现门控。
保留一处真实输入状态来源，避免只根据某个面板是否可见来推断世界互动已恢复。

## 具体可借鉴的错误教训

E03 说明 UI 覆盖层存在时，按局部对象枚举可能误判仍处于 3D 漫游。
其门控读取原游戏的 UI 占用计数；无法读到状态时禁止世界互动，避免误触叠加窗口。
我们的等价设计应直接维护明确的模式或占用状态，不需要照搬原字段名。
E04 的菜单、暂停、照片、冰箱、收音机、电话与 3D 分类，不应全部成为我们新增功能。
本游戏只实现已纳入范围的终端、处置、房间、床和梦境上下文。

E05 区分普通会谈、系统提示和开场 / 结局的文本入口，说明只监听一个字幕组件可能漏掉文本。
E06 处理说话者名、富文本清理及短时去重；可参考统一字幕 / 文本事件适配层。
中文文本、动态变量和屏幕故障字样应先形成最终显示内容，再送入可访问性接口。

## Unity 2022.3 迁移成本

csproj（E02）为 .NET 6，并引用 MelonLoader、Harmony、Unity 引擎和原游戏 Assembly-CSharp。
README 描述原游戏 Unity 6.3，不是本仓库具有可直接打开的 Unity 6 项目。
这些程序集路径与原游戏类型对我们的 Unity 2022.3 项目无效，直接搬运成本高且无必要。
源码 hook 使用旧 Yarn LineView；我们的 Yarn 3 主选使用 Presenter 接口，不能复制其调用签名。
输入门控与文本事件的思想则不依赖引擎版本，原创实现成本主要在状态协调和验证。
完整读屏接入需另外核对 Unity 2022、平台、语音库与发布需求；不是本次默认实现范围。

## 实施步骤

1. 先实现明确的输入模式和 UI 进入 / 退出接口，并给每次占用配对释放路径。
2. 用终端、处置弹窗、暂停三个覆盖层验证优先级及关闭顺序。
3. 接入会谈文本事件，随后接晨检与结局，检查没有遗漏入口。
4. 让逐字打字显示与最终文本事件分离，避免重复朗读 / 记录。
5. 提供简单状态日志用于复现卡死，不必移植 MOD 的反射诊断工具。
6. 如果产品后来要求读屏，再选择与发布平台兼容的方案，独立验证许可及运行表现。

本次只完成源码观察；没有安装 MOD、加载原游戏、执行 hook 或验证读屏实际输出。
README 对支持平台与运行行为的描述仍属于原作者陈述，不能替代本地运行证据。

## 代码与资产许可

来源锁定记录与本地检查未发现覆盖 MOD 本体的根许可证，不默认复制其代码。
libs 中的 UniversalSpeech 许可仅覆盖对应库，不能推广到整个 MOD 或原游戏。
原游戏程序集、台词提取与图片也不能因 MOD 仓库公开而视为本游戏可用素材。
本次仅写原创分析和代码位置引用，不使用其提取数据作为我们的剧情或训练数据。

## 后续验证场景

- 终端打开后 E 不能触发世界对象；关闭后正确恢复。
- 处置弹窗叠在终端上，再关闭弹窗时不能提前恢复房间输入。
- 暂停、镜头过渡、梦境和退出场景各有正确占用清理。
- 同一按键在跳过打字的帧不再触发下一步世界互动。
- 五类文本入口：会谈、特殊对话、晨检、提示、结局都产生最终文本事件。
- 富文本、中文标点、说话者名和动态变量不会重复或漏出占位符。
- 重复台词的去重只针对短时打字重复，不吞掉之后合法出现的同句台词。
- 如果增加读屏，另验 Windows / 目标平台、键盘焦点及实际朗读，不以静态源码认定通过。

关联阅读：[需求借鉴矩阵](../reuse-matrix.md) · [Unity 2022 路线](../unity-2022-roadmap.md)。

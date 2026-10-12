# NINAHSaveEditor：存档结构与开发调试参考

## 研究基线

| 项目 | 内容 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 本游戏目标 | Unity 2022.3 LTS；3 天、5 位病人、3 种结局 |
| 原仓库 | [juliangrtz/NINAHSaveEditor](https://github.com/juliangrtz/NINAHSaveEditor) |
| 本地目录 | [references/NINAHSaveEditor](../../references/NINAHSaveEditor/) |
| 固定提交 | `cc5e9e767d27396798abe5c7cbe80cb329efdd76` |
| 实际版本 | .NET Framework 4.8；Windows Forms；非 Unity 项目 |
| 仓库性质 | No, I’m not a Human 的外部存档编辑器 |
| 建议优先级 | 低：专项参考 |
| 验证状态 | 已读取本地源码与版本 / 许可文件；未导入、未编译、未运行 |

版本来自 [来源锁定记录](../../docs/source-lock.json)。本页结论是静态研究，不能据此宣称 Unity 2022.3 中可运行。
游戏需求依据 [游戏总览](../../docs/game-overview.md) 与 [程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 对本游戏的结论

此库展示如何把日夜、物体状态和资源拆分查看，适合启发我们的存档字段和开发调试面板。
它能编辑原游戏存档，不是原游戏存档控制器源码，也不提供完整游戏循环。
不建议将其解密工具或原游戏序列化格式引入我们的 Unity 工程。
我们的收益来自清晰的状态组织、可检查的存档与快速复现剧情路径。

## 源码证据

| ID | 已核验的代码 / 文本位置 | 永久来源 | 说明 |
| --- | --- | --- | --- |
| E01 | [README.md:6–20](../../references/NINAHSaveEditor/README.md) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/README.md#L6-L20) | Windows 存档工具的性质、要求与作者功能说明 |
| E02 | [NINAHSaveEditor/Types.cs:7–16](../../references/NINAHSaveEditor/NINAHSaveEditor/Types.cs) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/NINAHSaveEditor/Types.cs#L7-L16) | 强耦合原游戏类型名称的状态类别 |
| E03 | [NINAHSaveEditor/Forms/Editor.cs:41–89](../../references/NINAHSaveEditor/NINAHSaveEditor/Forms/Editor.cs) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/NINAHSaveEditor/Forms/Editor.cs#L41-L89) | 读取原游戏消耗品、物体状态与日夜 / 日期 |
| E04 | [NINAHSaveEditor/Forms/Editor.cs:104–130](../../references/NINAHSaveEditor/NINAHSaveEditor/Forms/Editor.cs) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/NINAHSaveEditor/Forms/Editor.cs#L104-L130) | 写入状态及另一套序列化副本 |
| E05 | [NINAHSaveEditor/Utils.cs:12–37](../../references/NINAHSaveEditor/NINAHSaveEditor/Utils.cs) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/NINAHSaveEditor/Utils.cs#L12-L37) | 特定 Steam AppID 与 Windows 注册表路径 |
| E06 | [NINAHSaveEditor/NINAHSaveEditor.csproj:4–14](../../references/NINAHSaveEditor/NINAHSaveEditor/NINAHSaveEditor.csproj) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/NINAHSaveEditor/NINAHSaveEditor.csproj#L4-L14) | Windows WinExe 与 .NET Framework 4.8 |
| E07 | [LICENSE:1–22](../../references/NINAHSaveEditor/LICENSE) | [固定版本](https://github.com/juliangrtz/NINAHSaveEditor/blob/cc5e9e767d27396798abe5c7cbe80cb329efdd76/LICENSE#L1-L22) | Unlicense 软件授权声明 |

## 四种借鉴分类

| 分类 | 本库对应内容 | 我们的处理 |
| --- | --- | --- |
| 直接引入 | 工具软件为 Unlicense，但强耦合原游戏格式与 Windows | 暂不直接引入；许可允许性不等于适配价值 |
| 改写结构 | 独立日夜、物体状态、资源字段与树状检查 | 为自己的病人、会谈、配额与日循环建立调试视图 |
| 表现参考 | 表单查看 / 修改状态、存档错误反馈 | 借鉴开发工具易观察的交互，不进入游戏玩家 UI |
| 自研缺口 | 本游戏保存、读取、版本、恢复、处置去重与结局状态 | 在自己的 Unity 状态层实现 |

## 适配的数据流程

1. 游戏运行状态先成为唯一可信来源：日期 / 阶段、当前病人、配额和各病人状态。
2. 会谈结束或其他选定检查点形成业务快照，合并对话引擎必要的恢复数据。
3. 存档写入自己的文件格式，并包含业务格式版本和内容版本信息。
4. 开发调试面板读取当前状态或存档，按病人、日循环、房间分组查看。
5. 调试修改经过显式接口提交，显示修改前后差异，不直接绕过所有业务约束。
6. 用修改后的状态进入真实会谈 / 次日流程，确认结果由实际逻辑产生。
7. 读取存档时先校验版本和必要字段，再恢复病人、会谈、场景及输入阶段。
8. 校验已提交处置，不重复扣配额、不重复治疗、不重复触发特殊对话。

以上是开发建议，不表示本游戏已有保存系统或确定的存档格式。
自己的数据以稳定病人标识为键，避免用原游戏程序集完整类型名组织业务字段。
信任、治疗次数、去留和结局标记需明确归属，不能由多个面板各存一份。

## 原库能证明与不能证明什么

E03 显示原工具读取日 / 夜、日期、时间、物体开关和消耗品；这是外部观测到的存档字段。
这些字段不证明原游戏完整的患者、访客、剧情或结局内部控制逻辑已开源。
E02 把存档类别写为原游戏类型名称与程序集元信息，适配自己的游戏时应避免这种强耦合。
E04 同步另一个序列化副本，提醒我们不能为同一状态制造多个互相竞争的来源。
README 声称能解密、编辑及重新加密（E01），本次没有运行验证该声明。
不读取 Crypto/Keys.cs，不研究或复制原游戏密钥，不操作玩家的实际存档。

## 推荐的开发调试能力

| 目标情景 | 可观察状态 | 用来验证什么 |
| --- | --- | --- |
| 放走神父后的次日 | 日期、神父去留、已释放记录 | 次日结局事件是否正确到达 |
| 未完成净化配额 | 当日配额、已完成量、失败标记 | 失败路径与晨检是否一致 |
| 第三次治疗 | 病人治疗次数、下一日状态 | 治疗后角色表现与去留 |
| 特殊对话 | 病人信任、日期、已触发标记 | 阈值判断与唯一触发 |
| 会谈恢复 | 节点 / 轮次、业务阶段、处置记录 | 不重复副作用与输入恢复 |

这些场景来自本游戏设计，而不是把参考游戏的物品或日程添加到 Mini 范围。
调试控制应仅在开发模式可见，不把修改信任、配额或日期的入口放进玩家终端。

## Unity 2022.3 迁移成本

E06 为 .NET Framework 4.8 的 Windows Forms；依赖注册表、Windows UI 与原游戏文件路径。
E05 根据 Steam 用户目录查找特定 AppID，不能直接作为我们的跨平台存档路径。
若制作独立编辑器需额外维护工具项目；Mini 阶段更适合 Unity 内部开发面板或简单 JSON 检查。
本库没有可直接迁移的 Unity 场景、首人称、会谈或监控 UI。
因为目标游戏的数据由我们定义，重新实现小型调试面板通常比迁移整个工具更清楚。

## 实施步骤与失败路径

1. 确定业务状态归属及恢复检查点，再决定正式字段，避免先仿照原存档格式。
2. 做保存 / 读取的往返验证，确保五人记录和配额一致。
3. 增加开发视图，优先支持病人、日期、阶段和处置记录观察。
4. 为旧版本、损坏 JSON、缺失字段和中断写入给出可复现的错误结果。
5. 验证重复加载不产生额外治疗、配额扣减或剧情事件。
6. 随内容定版补结局回归测试，不凭调试面板显示某值就认定游戏路径已完成。

## 代码与资产许可

E07 是 Unlicense，覆盖该软件的复制、修改与分发等授权声明。
它不授予原游戏台词、图像、品牌或存档内容的重新使用权。
工具资源、第三方 Newtonsoft.Json 等依赖仍需按对应来源核查。
默认仅原创分析工具结构，不把原游戏字段、资源图标或密钥移入我们的工程。

## 后续验证场景

- 五位病人的状态保存 / 读取完全对应，不丢失独立信任与去留。
- 完成处置后读取同一快照不会再次扣配额或增加治疗次数。
- 关键结局路径通过真实日循环验证，而非仅修改字段。
- 缺失文件、损坏内容、未知版本和不完整快照有明确失败结果。
- 开发修改不能静默创建互相矛盾的病人状态。
- 调试面板和修改接口不出现在正式玩家流程中。

关联阅读：[需求借鉴矩阵](../reuse-matrix.md) · [Unity 2022 路线](../unity-2022-roadmap.md)。

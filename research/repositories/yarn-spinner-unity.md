# Yarn Spinner：固定剧情会谈的主选框架

## 研究基线

| 项目 | 内容 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 本游戏目标 | Unity 2022.3 LTS；3 天、5 位病人、3 种结局 |
| 原仓库 | [YarnSpinnerTool/YarnSpinner-Unity](https://github.com/YarnSpinnerTool/YarnSpinner-Unity) |
| 本地目录 | [references/YarnSpinner-Unity](../../references/YarnSpinner-Unity/) |
| 固定提交 | `478309e5e70a1d88e94be47c65c793b851d35f56` |
| 实际版本 | UPM 包 3.2.8；package.json 声明 Unity 2022.3 |
| 仓库性质 | Unity 对话框架与编辑器工具 |
| 建议优先级 | 高：先验证的对话主选 |
| 验证状态 | 已读取本地源码与版本 / 许可文件；未导入、未编译、未运行 |

版本来自 [来源锁定记录](../../docs/source-lock.json)。本页结论是静态研究，不能据此宣称 Unity 2022.3 中可运行。
游戏需求依据 [游戏总览](../../docs/game-overview.md) 与 [程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 对本游戏的结论

优先用 Yarn 承载人工编写的病人会谈，用自己的状态层承载日期、配额、病人去留和结局。
它的条件选项、节点跳转、Unity 呈现组件及完成事件，适合我们每天 4–5 轮、分支回流的固定对话。
最多四个回答是我们的内容约束，Yarn 不会自动替我们保证这一上限。
本研究将 Yarn 作为主选，Ink 作为备选；这是技术建议，不代表游戏工程已安装任何框架。

## 源码证据

| ID | 已核验的代码 / 文本位置 | 永久来源 | 说明 |
| --- | --- | --- | --- |
| E01 | [package.json:1–7](../../references/YarnSpinner-Unity/package.json) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/package.json#L1-L7) | 包版本与 Unity 基线声明 |
| E02 | [Runtime/Views/OptionsPresenter.cs:165–233](../../references/YarnSpinner-Unity/Runtime/Views/OptionsPresenter.cs) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/Runtime/Views/OptionsPresenter.cs#L165-L233) | 条件选项、按钮创建与选择回传 |
| E03 | [Runtime/DialogueRunner/DialogueRunner.cs:305–338](../../references/YarnSpinner-Unity/Runtime/DialogueRunner/DialogueRunner.cs) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/Runtime/DialogueRunner/DialogueRunner.cs#L305-L338) | 节点与整场对话生命周期事件 |
| E04 | [Runtime/DialogueRunner/DialogueRunner.ActionRegistration.cs:13–29](../../references/YarnSpinner-Unity/Runtime/DialogueRunner/DialogueRunner.ActionRegistration.cs) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/Runtime/DialogueRunner/DialogueRunner.ActionRegistration.cs#L13-L29) | 命令及函数的业务桥接接口 |
| E05 | [Runtime/DialogueRunner/DialogueRunner.Utility.cs:36–98](../../references/YarnSpinner-Unity/Runtime/DialogueRunner/DialogueRunner.Utility.cs) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/Runtime/DialogueRunner/DialogueRunner.Utility.cs#L36-L98) | 从持久目录读写变量 JSON |
| E06 | [Runtime/Storage/InMemoryVariableStorage.cs:234–290](../../references/YarnSpinner-Unity/Runtime/Storage/InMemoryVariableStorage.cs) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/Runtime/Storage/InMemoryVariableStorage.cs#L234-L290) | 变量批量导出与恢复，不是执行位置快照 |
| E07 | [LICENSE.md:1–21](../../references/YarnSpinner-Unity/LICENSE.md) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/LICENSE.md#L1-L21) | MIT 许可与版权保留要求 |
| E08 | [Tests/Editor/Editor Test Resources/TestYarnScript.yarn:3–18](../../references/YarnSpinner-Unity/Tests/Editor/Editor%20Test%20Resources/TestYarnScript.yarn) | [固定版本](https://github.com/YarnSpinnerTool/YarnSpinner-Unity/blob/478309e5e70a1d88e94be47c65c793b851d35f56/Tests/Editor/Editor%20Test%20Resources/TestYarnScript.yarn#L3-L18) | 有限选项与节点跳转示例 |

## 四种借鉴分类

| 分类 | 本库对应内容 | 我们的处理 |
| --- | --- | --- |
| 直接引入 | MIT 许可的 Yarn Unity 包、编译器集成、运行器与呈现接口 | 固定提交验证后按包引入，保留版权与许可 |
| 改写结构 | 变量存储、条件选项、节点完成 / 对话完成事件 | 接入自己的 PatientState 与会谈阶段控制 |
| 表现参考 | 选项按钮、逐句呈现、输入取消与显示不可用选项 | 用监控终端视觉重做 UI，不照搬示例美术 |
| 自研缺口 | 每日会谈编排、处置去重、配额、跨日、三种结局、业务存档 | 游戏状态层与内容校验器另行实现 |

## 适配的数据流程

1. 日循环状态机确定日期和当前病人，从病人配置选择当天的 Yarn 入口节点。
2. 病人运行状态提供独立信任、治疗次数、去留及已触发特殊对话标记。
3. 对话开始时，锁定房间输入，隐藏处置按钮，按逐句方式呈现病人台词。
4. Yarn 输出有限选项；选择可发出信任变化命令，游戏状态层执行后继续公共节点。
5. 普通会谈结束后，按本病人 / 本日期的配置检查特殊对话，满足时仅进入一次。
6. 整场会谈完成后解锁处置 UI；处置提交只执行一次并记录病人状态及配额结果。
7. 根据选定处置启动独立反应节点；反应结束后播放画面故障，关闭本次监控。
8. 完成当天业务后由日循环转入房间 / 上床 / 梦境，再推进下一日。

信任阈值及数值在源设计中仍有示例口径，不因框架变量类型而固定为 0–100 或阈值 50。
Yarn 变量只表达叙事所需状态；核心病人记录应有稳定病人标识，避免跨角色串值。
不同回答可以改变信任，但回到同一后续节点；无需为每个回答复制整段日后剧情。
命令及函数注册见 E04，可用于信任更新、音效与画面事件；不可把状态变化藏进按钮显示逻辑。

## 实施步骤与责任边界

1. 在独立验证工程安装锁定的包，仅制作一位病人、两轮会谈和一个条件节点。
2. 建立监控 UI 的 DialoguePresenter；沿用接口，替换通用示例的外观与排版。
3. 通过显式函数查询病人状态，通过显式命令提交对话产生的业务事件。
4. 使用整场完成事件解锁处置；节点完成事件仅用于节点级演出，二者不可混同。
5. 写内容检查：最多四个选项、入口节点存在、公共回流存在、特殊节点不会重复进入。
6. 在确认跨日数据与存档恢复后，再扩为五位病人与三天文本。

以上是后续实现路线；本次没有编写 Yarn 剧情或导入包。
需要统一异常路径：终止 / 取消会谈、缺失节点、空选项不能留下锁死的输入状态。

## 存档边界

E05 的方法会将变量存为 JSON，再加载到 VariableStorage；E06 证明其内容为数值、字符串和布尔字典。
这里的“存状态”不是完整会谈执行位置快照，不能只调用该方法就声称能原位继续对话。
游戏存档还需记录日期、病人、业务阶段、会谈检查点、治疗次数、配额及处置提交记录。
建议首个验证按节点边界恢复；若要求对话中任意一行恢复，需要额外保存轮次并验证副作用不重复。
同一信任变化或净化扣配额在读取存档后再次执行，会破坏结局条件，必须有明确恢复策略。

## Unity 2022.3 迁移成本

包声明支持 Unity 2022.3（E01），因此静态上比 Unity 6 游戏项目更接近目标基线。
主要成本是监控呈现、业务桥接及存档，并不是重新写分支解释器。
仓库使用 Yarn 3 的 Presenter / OptionsPresenter API；旧 Yarn 2 的 LineView 示例需要转换。
包声明不等于本项目导入验证；输入系统、TextMeshPro、字体、中文换行和异步取消仍需实测。
不将整个仓库的测试目录或编辑器分析工程当作游戏运行依赖。

## 代码与资产许可

E07 为根 MIT 许可；引入其软件或较大代码片段时保留版权与许可文本。
示例图像、字体、第三方依赖及品牌标识应按各自来源检查，不能用根代码许可概括所有素材。
默认使用自己的监控 UI、病人图像与字体，不把样例呈现当成我们的成品资产。

## 后续验证场景

- 四选项与超四选项文本分别通过 / 被内容检查拒绝。
- 两条回答改变不同信任结果，但回到公共下一轮。
- 五个病人的信任、治疗次数及特殊对话标记互不串值。
- 特殊对话不满足时跳过，满足时只进入一次。
- 处置在会谈结束前不可操作，反应结束后才关闭监控。
- 保存 / 读取节点边界后不重复执行信任与配额副作用。
- 终端、弹窗、暂停期间 E 键不触发房间互动。
- Unity 2022.3 中验证导入、中文字体、打字跳过、选项焦点与取消路径。

关联阅读：[需求借鉴矩阵](../reuse-matrix.md) · [Unity 2022 路线](../unity-2022-roadmap.md)。

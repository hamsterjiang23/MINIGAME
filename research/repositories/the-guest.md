# The-Guest：人物配置、会谈记忆与交互责任分离

## 研究基线

| 项目 | 内容 |
| --- | --- |
| 研究日期 | 2026-10-12 |
| 本游戏目标 | Unity 2022.3 LTS；3 天、5 位病人、3 种结局 |
| 原仓库 | [PeppeLe21/The-Guest](https://github.com/PeppeLe21/The-Guest) |
| 本地目录 | [references/The-Guest](../../references/The-Guest/) |
| 固定提交 | `40a9d70baed36ee05ae86578939c8d182ff3b3a4` |
| 实际版本 | Unity 6000.0.58f2；URP 17.0.4、Cinemachine 3.1.5、uGUI 2.0.0 |
| 仓库性质 | 含 LLM 自由输入对话的 Unity 游戏工程 |
| 建议优先级 | 中：人物数据与输入结构参考 |
| 验证状态 | 已读取本地源码与版本 / 许可文件；未导入、未编译、未运行 |

版本来自 [来源锁定记录](../../docs/source-lock.json)。本页结论是静态研究，不能据此宣称 Unity 2022.3 中可运行。
游戏需求依据 [游戏总览](../../docs/game-overview.md) 与 [程序需求](../../docs/sources/program-requirements-v0-1.md)。

## 对本游戏的结论

值得研究静态人物资料与运行记忆分离、对话完成回调以及 UI 打开时暂停移动的责任划分。
本库以自由输入、LLM 返回信任增量与邀请 / 拒绝行为为核心，不能直接满足我们的人工固定剧情。
Mini Game 默认仍采用固定选项；将在线模型接入会增加剧情不确定性与网络依赖，不自动纳入交付。
The-Guest 没有发现覆盖游戏本体的根许可证，默认只原创实现其设计思路。

## 源码证据

| ID | 已核验的代码 / 文本位置 | 永久来源 | 说明 |
| --- | --- | --- | --- |
| E01 | [ProjectSettings/ProjectVersion.txt:1–2](../../references/The-Guest/ProjectSettings/ProjectVersion.txt) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/ProjectSettings/ProjectVersion.txt#L1-L2) | Unity 6 工程版本 |
| E02 | [Packages/manifest.json:3–17](../../references/The-Guest/Packages/manifest.json) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Packages/manifest.json#L3-L17) | Cinemachine、Input System、URP 与 uGUI 依赖 |
| E03 | [Assets/Scripts/Demo/NPCMemory.cs:10–77](../../references/The-Guest/Assets/Scripts/Demo/NPCMemory.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/NPCMemory.cs#L10-L77) | 按 costume 的信任字典与运行记忆 |
| E04 | [Assets/Scripts/Demo/DialogueUI.cs:420–478](../../references/The-Guest/Assets/Scripts/Demo/DialogueUI.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/DialogueUI.cs#L420-L478) | 自由输入、模型增量、零值加分与行为执行 |
| E05 | [Assets/Scripts/Demo/DialogueUI.cs:186–258](../../references/The-Guest/Assets/Scripts/Demo/DialogueUI.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/DialogueUI.cs#L186-L258) | 固定文本模式及完成 / 取消回调 |
| E06 | [Assets/Scripts/Demo/PlayerController.cs:30–57](../../references/The-Guest/Assets/Scripts/Demo/PlayerController.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/PlayerController.cs#L30-L57) | 触发器发现 IInteractable 与范围提示 |
| E07 | [Assets/Scripts/Demo/PlayerController.cs:86–105](../../references/The-Guest/Assets/Scripts/Demo/PlayerController.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/PlayerController.cs#L86-L105) | UI 输入门控及 E 互动 |
| E08 | [Assets/Scripts/Demo/PlayerController.cs:110–154](../../references/The-Guest/Assets/Scripts/Demo/PlayerController.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/PlayerController.cs#L110-L154) | 按相机方向移动并转向角色，不是第一人称视角控制 |
| E09 | [Assets/Scripts/Demo/NPCProfile.cs:5–27](../../references/The-Guest/Assets/Scripts/Demo/NPCProfile.cs) | [固定版本](https://github.com/PeppeLe21/The-Guest/blob/40a9d70baed36ee05ae86578939c8d182ff3b3a4/Assets/Scripts/Demo/NPCProfile.cs#L5-L27) | ScriptableObject 人物资料与初始信任 |

## 四种借鉴分类

| 分类 | 本库对应内容 | 我们的处理 |
| --- | --- | --- |
| 直接引入 | 游戏本体没有明确根许可；工程依赖也不同 | 暂无直接引入项，不整套复制工程 |
| 改写结构 | NPCProfile / NPCMemory、IInteractable、完成回调、UI 输入门控 | 自行实现对应责任边界，改为固定病人状态 |
| 表现参考 | 信任反馈、打字音、对话镜头切入、笔记展示 | 参考反馈节奏；我们的信任隐藏，不照抄显式滑条 |
| 自研缺口 | 固定四选项、逐日剧情、处置、配额、独立信任存档与结局 | 由 Yarn + 游戏状态层实现 |

## 适配的数据流程

1. PatientProfile 作为静态配置保存病人标识、人物信息、当天剧情入口与治疗表现阶段。
2. PatientState 保存本病人信任、治疗次数、去留和特殊对话标记，不放到共享 UI 组件里。
3. RoomInteractor 发现可交互物并显示 E 提示，由当前输入状态决定是否允许执行。
4. 终端交互进入会谈模式，关闭房间移动与 E 互动，开放鼠标 / UI 输入。
5. Yarn 或 Ink 驱动人工会谈；状态层提交信任变化，UI 只负责表现。
6. 普通及特殊对话结束后进入处置，处置反应结束回调再退出监控。
7. 恢复房间状态；如果进入梦境前阶段，仅让床保持可交互。
8. 将病人业务状态与会谈检查点保存，跨日与重新启动时恢复。

以上命名只是推荐职责，不是项目中已经存在的接口或实现。
NPCMemory 原本在一个 NPC 内按 costume 区分信任与摘要（E03），我们的五人状态不需要服装维度。
其 0–100、初始 50、正负 1–5 都属于该游戏设定，不是我们的已定数值。
对本游戏可记录结构化“已知事实 / 已触发节点”，不需要 LLM 自动生成会谈摘要。

## 输入与视角责任

E06 中 OnTriggerEnter / OnTriggerExit 用触发器发现 IInteractable，E 键调用 Interact。
E07 在对话、笔记或衣柜打开时暂停移动，说明输入状态与世界互动需要协调。
这些职责可以参考，但不能把该 PlayerController 标成我们第一人称控制器的现成实现。
E08 的角色移动按相机方向计算，并旋转角色本体；它不是完整第一人称视角控制。
我们仍需实现无身体 / 四肢、无跳跃、鼠标视角、终端镜头靠近与返回的第一人称体验。
触发器范围提示可以与视线检测组合，避免多个物件重叠时选择错误目标。

## 固定剧情的改造步骤

1. 先原创实现静态配置、运行状态与 UI 的分离，不复制未明确许可的类代码。
2. 去掉自由输入、模型决定信任、自动邀请与拒绝，把事件交给固定节点与确定规则。
3. 把对话模式、处置模式、房间模式、暂停模式的输入占用统一管理。
4. 以一个病人验证会谈结束回调、处置反应和离开终端的顺序。
5. 接入跨日与文件存档；不能把场景间 DontDestroyOnLoad 当成跨运行保存。
6. 之后再讨论笔记是否属于 Mini 范围，避免因参考仓库存在笔记就自动扩大需求。

E04 显示模型输出直接影响信任与行为；其中零增量强制改为 +1，不能照抄我们的信任规则。
E05 的固定文本模式与完成 / 取消回调值得参考，但特殊对话唯一性仍需自己控制。
扫描提示、处置反馈和闪屏可用确定的演出命令，不让网络回复决定配额或结局。

## Unity 2022.3 迁移成本

E01 为 Unity 6 工程；E02 的 URP 17.0.4、Cinemachine 3.1.5、uGUI 2.0.0 不能整体照搬目标工程。
资产、场景、材质、渲染管线和相机组件存在回迁成本，静态结构参考远低于整项目降级成本。
NotebookManager 等文件使用 Unity.Cinemachine 命名空间，需按选定 2022 相机包 API 重接。
保持目标工程自己的 manifest，分项验证依赖，不能靠修改 ProjectVersion.txt 宣称完成迁移。
本次未读 API 密钥、未调用任何模型服务，也未测试网络失败或工程运行表现。

## 代码与资产许可

来源锁定记录没有列出根许可，检查未发现覆盖 The-Guest 游戏本体的根 LICENSE。
Unity、TextMeshPro、MAST、字体、图像和第三方资产的授权范围不能由仓库公开状态推定。
默认仅写原创结构分析并链接出处；不复制人物设定、插图、模型、音乐或代码进入本游戏。
若后续确定需要原代码或素材，应先取得对应授权并记录具体文件范围。

## 后续验证场景

- 多个物体触发器重叠时 E 提示对应正确物体。
- 对话 / 终端 / 弹窗打开时，移动、视角与房间 E 互动按模式正确关闭。
- 第一人称镜头过渡不会调用第三人称角色转向逻辑。
- 会谈完成、取消与处置反应完成分别触发正确回调。
- 五位病人信任独立，零增量保持零，不产生强制加分。
- 隐藏信任不因参考仓库的滑条组件而泄漏到玩家 UI。
- 重启后状态来自文件存档，而不是仅靠跨场景存活对象。
- 如果未来单独探索 LLM，另验网络失败、输出校验及确定性；不计入当前完成状态。

关联阅读：[需求借鉴矩阵](../reuse-matrix.md) · [Unity 2022 路线](../unity-2022-roadmap.md)。

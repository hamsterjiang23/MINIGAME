# 借鉴风险与自研缺口

> 研究日期：2026-10-12。本文记录实际代码、配置和来源内容中的限制；Unity2022.3 LTS为研究默认基线。包声明、源码检查与运行验证是不同证据，当前参考工程均未导入、编译、试玩或打包。

## 1. 版本与迁移

| 来源 | 已核查事实 | 对我们的影响与处理 |
| --- | --- | --- |
| [Yarn](repositories/yarn-spinner-unity.md) | 3.2.8，package.json声明2022.3；Presenter API是Yarn3 | 候选版本符合研究基线；仍需导入验证。旧LineView教程和MOD调用不能直接套用 |
| [Ink](repositories/ink-unity-integration.md) | integration2.0.0，声明2022.3；工程记录2022.3.62f3 | 可作为备选；不与Yarn同时运行，示例UI不等于我们的逐句会面UI |
| [DeadLetterOffice](repositories/dead-letter-office-day1.md) | Unity2022.3.16f1 | 版本接近但机制不匹配；标签、序号、场景和Timeline引用需拆分 |
| [DeathAndTaxes](repositories/death-and-taxes.md) | Unity2019.2.10f1，Articy／静态管理器耦合，部分资源未公开 | 提炼状态和结算结构；不假定旧工程直接打开即可构成可运行底座 |
| [Papers-Please](repositories/papers-please-unity.md) | Unity2020.3.25f1，未完成原型 | 只作资料呈现／拖放专项参考，缺我们的多天会面与信任 |
| [Moral Matrix](repositories/moral-matrix.md) | Unity2023.1.20f1／URP15.0.7，点击NavMesh移动 | 不整工程降级；billboard、视频与音效小模块单独验证，第一人称另做 |
| [The Guest](repositories/the-guest.md) | Unity6000.0.58f2／URP17.0.4／Cinemachine3.1.5／uGUI2.0.0 | 不能整体复制manifest到2022；借鉴人物和输入结构，去掉服装／LLM依赖 |
| [Gatekeeper](repositories/gatekeeper-insight.md)、[Human, Please](repositories/human-please.md) | Godot4.4／4.2 | Node、Signal、Resource和二维灯光需用Unity组件、事件与数据重新实现 |
| [Do-You-Trust-Them](repositories/do-you-trust-them.md) | 仅部分脚本，未给完整工程／版本；缺若干类型 | 按天配置和活动门控可以研究，不能宣称完整项目可编译 |
| [Retro CRT](repositories/urp-retro-crt-shader.md) | 无完整工程版本文件；Fullscreen ShaderGraph与双相机样例 | URP14候选；实体屏幕、全屏Blit各有不同接线，须实测构建 |
| [CrowFX](repositories/crowfx-unity-image-effects.md) | 2.2.0声明2022.3；Built-in主路线，URP/HDRP实验性 | Built-in可优先验证；URP14下另测实际接线和性能，不把声明当稳定性结论 |
| [无障碍MOD](repositories/im-a-blind-human.md)、[存档编辑器](repositories/ninah-save-editor.md)、[攻略](repositories/ninah-docs.md) | 分别依赖原游戏程序集、Windows工具及MDX网站内容 | 只研究输入边界、测试工具与机制表达，不能归类为可独立移植的游戏工程 |

[Unity官方兼容表](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/requirements.html)支持URP14作为Unity2022.x候选。该对应关系不证明任何参考Shader在本游戏已经成功运行。

## 2. 代码、资产与许可范围

这份清单是本地许可文件与作者说明的记录。代码复用、框架引入、资源使用分别检查，不从仓库可下载推断成品素材授权。

| 分组 | 仓库与记录 | 本轮建议 |
| --- | --- | --- |
| 根许可MIT | Gatekeeper、DeathAndTaxes、Moral Matrix、Yarn、Ink、Retro CRT、CrowFX | 明确代码文件可列为复用候选，保留版权／许可；第三方插件、字体、音频和图片单独记录来源 |
| Unlicense工具 | NINAHSaveEditor | 可研究工具实现；原游戏数据类型、程序集与存档秘密不随工具许可变成我们的接口 |
| 未发现本体根许可 | DeadLetterOffice、Do-You-Trust-Them、Human, Please、Papers-Please_Unity、The Guest、im-a-blind-human | 本轮归为结构／表现研究，自行写适配实现；不列为可直接复制的发行代码或素材 |
| 单文件／插件例外 | Human, Please的crt_vhs.gdshader标CC0，sound_manager插件为MIT；DeadLetterOffice导出插件另有许可 | 记录其具体文件范围，不推广为整个游戏的授权 |
| 自定义许可 | NINAH-Docs的File to Downloader许可 | 原创分析其组织方法并指向出处；不复制攻略正文或原作图片为核心产品内容 |
| 明确缺失资源 | DeathAndTaxes README说明部分资源和美术未公开 | 克隆完整不等于原游戏资源完整；不要把被剔除资源算为可以借用的资产 |

各仓库文档的许可证据均指向本地文件与固定提交。NINAH-Docs许可包含商业核心使用限制和模型训练等限制，详见[该库许可研究](repositories/ninah-docs.md)，不将其描述为MIT或一般开放素材库。

本轮研究不导入原作角色、完整对白、商标、商业美术、音乐或宣传截图。游戏正式资产应有自己的来源与授权记录。参考仓库中的密钥文件、API凭据和原游戏存档密钥不作为研究内容或开发依赖。

## 3. 没有现成匹配实现的部分

| 缺口 | 现有来源能提供什么 | 我们需要补做什么 |
| --- | --- | --- |
| 第一人称电脑过渡 | 第三人称／点击移动、摄像机切换、E触发与UI冻结 | 无肢体无跳跃的视角控制，进入／退出电脑连续过渡，光标与输入还原 |
| 三动作病人状态 | 死亡／赦免、允许／拒绝、服装维度信任 | 治疗／净化／释放、稳定病人ID、独立信任、治疗次数与在场状态 |
| 完整会面门控 | 分支框架、条件选项、完成事件、交接遮挡 | 普通→特殊→可处置→反应→花屏，四选项和每场一次处置约束 |
| 逐日病人调度 | 每日SO与按角色ID记录后果 | 神父初见、处理后退出、治疗返场、终止与特殊跨日事件的组合 |
| 道德模糊性 | 真伪校验与SAFE／DANGEROUS答案 | 情绪读数、真实经历和实际后果分别记录，不以异常读数当标准善恶答案 |
| 三结局选择 | 结局展示／分发、跨日叙事状态 | 对我们三个结局的条件、判定时点和冲突优先级进行配置及验证 |
| 一致业务存档 | Yarn变量JSON、Ink叙事JSON、旧游戏保存时机 | 业务状态＋会谈检查点＋场景／事件状态的完整快照与副作用去重 |

“Do You Trust Them”名称不是隐藏信任实现的证据；“完整Git克隆”也不是游戏可运行、资源齐全或满足我们的设计的证据。

## 4. 会改变实现的设计待定项

下列编号沿用[docs问题清单](../docs/open-questions.md)，不另创一套正式答案。

| 编号 | 未定规则 | 需要避免的错误 |
| --- | --- | --- |
| Q02、Q03、Q22 | Day0／Day4预算、五人日程、神父Day2出现与治疗死亡时点 | 每天机械召回全部五人；Day3末释放后凭空扩展制作天数 |
| Q05、Q06 | 三键与继续观察、绿键含义 | 从旧游戏借来第四动作，或把释放静默改成观察 |
| Q07 | 当日净化配额、检查时点与失败后果 | 编造每日指标，或以“处理数量足够”替代净化数量 |
| Q08、Q09 | 信任范围、特殊轮阈值、灯色与释放门槛 | 将0–100／50示例固化；直接采用The Guest的开门信任值 |
| Q10 | 疯狂值、偏离指数、狂热值关系 | 合并不同语义，或把一个库的评分当我们的心理评估 |
| Q11–Q14 | 档案、新闻、日记、正式对白与梦境内容 | 把可参考的拖放、生成器、视频机制变成新增必做系统 |
| Q21、Q24 | 神父背景冲突、外交官实际处置与保护规则 | 覆盖角色文档，或对外交官默认执行普通第三次治疗规则 |
| Q25、Q26 | 三结局条件／优先级，第四条r为占位 | 仅靠结局名字完成判定，或扩大为四种结局 |
| 工程／制作项 | 具体2022.3补丁、管线、平台、性能预算、正式资产 | 未运行即宣称兼容、稳定、可发布或可商用资产齐全 |

Unity2022方向已在本轮由用户指定；原始docs仍保留来源口径，其“引擎未定”是飞书资料自身的状态，不应据此否认本轮选择。

## 5. 运行时最需要验证的边界

- **重复提交**：输入连点、演出回调、读档恢复均不得让同一会面再次扣配额或增加治疗次数。
- **对话完成粒度**：节点完成不等于整场会谈完成；普通结束不等于特殊轮结束；反应对白不能重新解锁处置。
- **多层输入占用**：电脑、弹窗、暂停和梦境叠加时，关闭某一层不能提前放开世界交互；异常退出也要恢复正确上下文。
- **状态与呈现一致**：UI灯色、可见按钮和场景人物均由权威状态读取；不从颜色、纹理名或GameObject存在推断人物真相。
- **跨日终止与事件**：释放／净化角色不进入普通队列，神父袭击走事件条件；外交官特殊处置按明确配置处理。
- **存档一致**：框架状态与业务状态一起恢复；旧BinaryFormatter格式只作历史代码观察，不直接用作我们的存档方案。
- **视觉构建**：Hidden Shader与关键字变体可能被剥离，Editor出现效果不代表打包后存在；CRT必须保持中文台词清晰。
- **内容可达与实际知识**：分支回流、特殊轮、三结局可达；对白不引用玩家未做、未说、未揭露的信息。

对应行为测试见[路线图C01–C14](unity-2022-roadmap.md)。当前没有测试通过记录，未启动参考工程，也没有游戏实现成果。

## 6. 本轮文档验收与后续记录

本轮文档检查已通过：19份Markdown覆盖15库，305个本地链接存在，143组源码证据中的完整SHA、Git路径、本地文件和行号范围一致；原有65个资料文件与15个参考仓库保持原样。此检查验证文档可追溯性，不替代游戏测试。

后续开发的每项依赖分别记录：选用的固定版本、原许可与资产来源、导入结果、编译结果、Play Mode行为、目标平台构建和性能结果。失败时保留准确状态；没有实际运行记录的候选继续标为待验证。

[返回入口](README.md) · [借鉴矩阵](reuse-matrix.md) · [Unity 2022路线图](unity-2022-roadmap.md)

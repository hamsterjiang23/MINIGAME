# 全流程对白与角色立绘指南

全流程使用六份新版 Excel 的1437行来源资料，其中1295行启用、142行按确认规则排除，覆盖 Day0 与五名角色三天的16场会谈。规则由 `GameSession` 结算，Yarn 负责发言、选项和演出；修复与兼容性见[本次更新](dialogue-update-2026-10-21/guide.md)。`[verified: ../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json sourceRowCount/activeSourceRowCount/excludedSourceRowCount/meetingCount]`

## 启动与操作

1. 在非 Play 状态运行 Unity 菜单 **MiniGame/Campaign/Import and activate**，校验全流程包、原生编译 Yarn、导入表内声明的立绘，并把融合房间设为全流程入口；再用 **MiniGame/Main Room/Open and play**。缺少图片或编译失败会报错。`[verified: ../UnityProject/Assets/MiniGame/Editor/CampaignBuilder.cs ImportAndActivate/ImportPortraits；../UnityProject/Assets/MiniGame/Editor/MainRoomGameplayBuilder.cs OpenAndPlay]`
2. Day0启动直接显示监管者监控，点击屏幕后连续拉远到完整终端；五段教学后仅可净化，播放两段反应并自动关机。回房后聚光灯照床，关闭家具碰撞，保留地面、墙和玩家；床交互一次播放闭眼后进入Day1。Day1–3完成门口晨检和简报后，到电脑自由选择当天待审角色。详见[开场指南](opening/guide.md)。`[verified: MiniGameBootstrap.StartTerminalDemo；PresentationEffects.DayZeroOpening/SleepTransition；GameRuntime.CompleteCue；BedtimeCollisionGate.Apply]`
3. 发言默认每秒30字。点击继续、Enter或空格，先补全当前页，再次操作翻页或推进；长句自动分页，历史保存完整原文。应答默认首项，上下键循环，Enter、数字1–4或鼠标确认。F2/F3已移除，Esc暂停；Day0命令条为被动提示，只有当前继续或净化按钮可操作。`[verified: MiniGameScreen.HandleTerminalKey/ShowResponseButtons；TerminalView.Render]`
4. 处置反应及结案文字结束后，监控断信号并归档；还有待审对象时返回目录继续。当天全部对象完成后自动关机；目录也可手动关机，再开机保留进度。`[verified: GameRuntime.CompleteCue/ReturnToDirectory/LeaveTerminal]`

F5保存、F9恢复、ESC暂停。全流程单独使用 `Application.persistentDataPath/campaign-save.json`；读档从当前未完成的一句重新显示，页数回到该句第一页，已完成的句子跳过，已提交的回答和处置不重复结算。`[verified: ../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs SavePath/Save/Load；../UnityProject/Assets/MiniGame/Yarn/MiniGameYarnBridge.cs PresentLineAsync；../UnityProject/Assets/MiniGame/Runtime/DialogueTextPlayback.cs Begin]`

左下自动显示最近对话，没有可切换的记录模式。Day0仅显示净化处置；提交后播放人物反应，暂停锁定所有处置按钮。`[verified: TerminalView.Render；MiniGameScreen.HandleTerminalKey]`

睡前与梦后仅能操作床时，房间渐暗，只留下床上方的聚光灯和飞舞的小虫，以亮区引导玩家休息；门与电脑不再弹出不可用提示。效果预览、参数与恢复行为见[睡前床位聚光灯](bedtime-guidance.md)。[verified: ../UnityProject/Assets/MiniGame/Runtime/BedtimeGuidance.cs SetTargets；../UnityProject/Assets/MiniGame/Runtime/ApartmentPlayer.cs RefreshFocus/InteractFocused]

## 当前流程与开发默认值

- 正式三天可在名册自由选择当天仍待审的角色；进入会谈后锁定当前角色，归档后再选择下一位，每人每天只能接待一次。名册保留米洛、薇拉、埃德温、阿德里安、乔纳的显示顺序，已退出者不可进入。神父从 Day1 出场，Day0 无名男子只用于教学。`[verified: ../src/MiniGame.Core/Runtime/GameSession.cs SelectPatient/CanReceiveToday；../src/MiniGame.Presentation/GameRuntime.cs EnterSelectedPatient；../Tools/DialogueCampaign/conversion-config.json patients]`
- 初始信任 **0**、默认释放门槛 **3**、每日净化目标 **1**。新版明确声明的特殊会谈要求信任 **≥3**，明确声明的释放要求 **≥5**；其他会谈沿用默认值。Day0 净化信任影响改为源表中的 **0**，其配额不参与正式三天KPI。原始增量与一次−5/+5合并适配可在来源审计中追溯。`[verified: ../Tools/DialogueCampaign/conversion-config.json；../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json mappings/adaptations；../src/MiniGame.Presentation/Authoring/CampaignPackageCompiler.cs CompileMeeting]`
- 结局优先级为 **神父袭击 → KPI失败锁房 → 正常被带走**。神父释放后次日袭击；每日记录KPI，正式三天结束统一判断失败。没有新增第四结局或反叛值算法。`[verified: ../src/MiniGame.Presentation/Authoring/CampaignPackageCompiler.cs Compile；../src/MiniGame.Core/Runtime/GameSession.cs Dispose/AdvanceDay]`
- 普通角色第三次治疗终止；外交官使用“档案终止”，不补写死亡或净化真实性。神父第三次治疗的终止事件推迟至次日。医生、外交官的第三人称结案记录显示在应答区域，保留最后一句人物发言。`[verified: ../Tools/DialogueCampaign/conversion-config.json patients；../Tools/DialogueCampaign/build_campaign.py build_meeting；../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs PresentDialogueLine；../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs SetNarration]`

## 修改对白与立绘

正文仍在 `Art/chat_excel` 的「对话流程」修改。先运行 `uv run python Tools/DialogueCampaign/build_campaign.py --smoke` 检查 Day0＋米洛前两天，再运行不带参数的同一命令生成完整包；回 Unity 重新执行全流程导入菜单。`--check` 可检查生成物是否与来源一致；详细转换规则见 [转换工具说明](../Tools/DialogueCampaign/README.md)。`[verified: ../Tools/DialogueCampaign/build_campaign.py main/build]`

新版按表头识别列，五份表使用 I/J/K/L 表示节点ID、下一节点、信任影响和立绘，小孩仍使用 J/K/L/M。正文逐句播放；逗号分隔目标按源表形成选择集合。来源审计保留原ID、行列、缓存、文件哈希、活动行、排除行及每项修复依据。`[verified: ../Tools/DialogueCampaign/updated_source.py source_columns/build_meeting；../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json mappings/books/repairs/excludedRows]`

旧外交官的26条历史措辞覆盖不再用于新版；当前按新工作簿的正文和信任条件执行。梦境反叛值配置中的既有选项已按相同台词迁移新ID，数值保留。`[verified: ../Tools/DialogueCampaign/text-variants.json；../UnityProject/Assets/MiniGame/Resources/MiniGame/PlayerRebellion.asset Choices]`

新版声明22个立绘ID，正文显式引用16个。原图、提示及生成记录保留在 [Art/Characters](../Art/Characters/README.md)；`PortraitCatalog` 供监控与名册使用。空立绘字段继承本角色状态，切换角色重新初始化；医生新版未填写立绘字段，沿用其基础立绘。`[verified: ../UnityProject/Assets/MiniGame/Content/Campaign/provenance.json declaredPortraits/referencedPortraits/books；../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs Render/SetLine/RenderRoster]`

当前原图为1254×1254透明PNG，原文件保留；Unity导入上限设为1024，关闭MipMap。CRT由界面叠加，不烘焙进人物图。逐张尺寸、生成提示和参考关系见人物资源清单。`[verified: ../Art/Characters/manifest.json actualSize/assets[].hasTransparency；../Art/Characters/ML/ML01.prompt.txt；../UnityProject/Assets/MiniGame/Editor/CampaignBuilder.cs ImportPortraits]`

## 兼容入口与验收

全流程资源为 `ActiveCampaignPackage`，V1小孩Day1仍用独立的 `ActiveDialoguePackage`；`MainRoomCombined` 由全流程导入启用 `useCampaign`，`InspectionApartment` 保留V1。开发接口分别为 `StartCampaign()`、`StartTerminalDemo()`、`StartLegacyPrototype()`，存档分别是 `campaign-save.json`、`terminal-demo-save.json`、`prototype-save.json`。旧HTML策划工具拒绝编辑V2，避免把全流程包写成单日结构。`[verified: ../UnityProject/Assets/MiniGame/Editor/CampaignBuilder.cs ImportAndActivate；../UnityProject/Assets/MiniGame/Runtime/MiniGameBootstrap.cs StartCampaign/StartTerminalDemo/StartLegacyPrototype/SavePath；../Tools/DialoguePlanner/planner-core.js assertEditablePackage]`

Windows开发构建入口为 **MiniGame/Campaign/Build Windows development player**，输出 `Builds/Campaign/MiniGame.exe`。内容校验、.NET测试、Unity原生编译、PlayMode、截图和Player启动分别记录在 [全流程验收报告](campaign-verification.json)；以各项实际结果为准，不把导入成功等同于完整运行验收。`[verified: ../UnityProject/Assets/MiniGame/Editor/CampaignBuilder.cs BuildWindows/WriteReport]` `[opinion]`

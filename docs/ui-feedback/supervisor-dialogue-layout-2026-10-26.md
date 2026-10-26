# 监督员对白留白与整句显示

门口监督员和梦结局共用完整句子的实际字体排版结果。保留短句原有框高，多行优先使用已有上下留白，达到每侧约 0.75 个字号的最小留白后才增加框高；黑底直接贴合这个最终区域，不再另外叠加固定上下扩边。字号、红字、像素化材质、横向位置和宽度沿用此前设置。[verified: ../../UnityProject/Assets/MiniGame/Runtime/SupervisorDialogueStyle.cs ApplyText/Layout/ApplyBackdrop；用户本轮红框参考]

逐字显示期间始终按完整句子定框，不再按已经出现的片段重算高度。旧实现以最长一行字符数估算一至四行，且每帧使用正在显示的片段布局，因此会在逐字出现时改变框高，也无法准确处理显式换行。[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs supervisorLineText/SetSupervisorDialogueBounds/Render；本轮修改前 SupervisorDialogueStyle.Layout]

门口监督员的一句 Yarn 对白现在和梦结局一样整句显示，不按可见框高再切为多页；第一次输入补全整句，下一次输入推进，保留同帧重复输入拦截和语音停止规则。原文与已有换行均保留，电脑角色及终端旁白的既有分页规则继续使用各自路径。[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs PresentDialogueLine/TryConsumeDialogueAdvance；../../UnityProject/Assets/MiniGame/Runtime/DreamSupervisorDialogue.cs Present]

本轮新增检查覆盖四种视口下全部 46 句门口对白、4 句梦结局对白和显式四行文本的实际字形边界；会谈回归要求每句门口对白只占一页，输入回归检查逐字显示框高不变及补全／推进各自只执行一次。[verified: ../../UnityProject/Assets/MiniGame/Tests/EditMode/SupervisorDialogueLayoutTests.cs；../../UnityProject/Assets/MiniGame/Tests/PlayMode/SupervisorDialoguePlayModeTests.cs EveryAuthoredLineAndExpressionPlaysOnceAcrossThreeMorningsAndTakenAway；../../UnityProject/Assets/MiniGame/Tests/PlayMode/WorldDialogueInputPlayModeTests.cs WholeSupervisorSentencesKeepStableBoundsAndNeedOnlyRevealThenAdvance]

实际测试、Unity 渲染图和新包验收结果见本轮交付的 `Releases/2026-10-26/NORM-DialogueLayout.verification.json`；Editor 检查、Player 启动与人工全流程试玩分别记录。[opinion]

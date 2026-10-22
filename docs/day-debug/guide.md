# 日期与阶段调试

在 Unity 菜单打开 **MiniGame → 调试 → 日期与阶段跳转**。选择第零天至第三天、进入阶段，再点击 **开始播放并跳转**；播放中也可以再次跳转。[verified: UnityProject/Assets/MiniGame/Editor/CampaignDebugWindow.cs「MenuItem」「目标日期」「开始播放并跳转」]

| 日期 | 可用入口 |
|---|---|
| 第零天 | 开场教学、教学前房间、教学电脑、关机、睡前房间、第一次睡觉。[verified: CampaignDebugCheckpoint.cs「PointsForDay」] |
| 第一至三天 | 晨检前、晨检后房间、工作电脑、关机、睡前房间、入梦、当晚梦境、梦后房间／自检。[verified: CampaignDebugCheckpoint.cs「PointsForDay」] |

勾选 **进入后暂停** 可停在目标阶段，按 Esc 继续。梦境入口先等待动画开始再暂停。第零天没有普通梦境，第一次睡觉后进入第一天。[verified: CampaignDebugWindow.cs「waitingForDream」「已暂停，Esc 继续」；CampaignDebugEditorTests.cs「DayZeroKeepsOpeningAndFirstSleepButHasNoOrdinaryDreamTarget」]

每次跳转会替换当前播放进度。前置剧情使用正式命令推进：优先治疗，回答选择反叛增量最低项。这是固定的调试前史，查看其他选择的后果仍需在目标阶段继续操作。[verified: CampaignDebugCheckpoint.cs「Advance」「RebellionOptionDeltas」] [opinion]

窗口内的 **隐藏结局 · 海滩梦境** 区域提供 **预览隐藏结局** 和 **停止隐藏结局预览**。停止当前播放后启动，WASD 移动、鼠标查看、E 躺下；停止预览后恢复原场景。该入口复用已有独立预览，不要求先推进三天剧情。`[verified: CampaignDebugWindow.cs StartHiddenEndingPreview/OnGUI；DreamCubemapPreview.cs Open/Stop/RestoreScenes]`

调试期间的手动存档和主菜单继续进度写入 `Application.temporaryCachePath/CampaignDayDebug`，窗口提供打开目录按钮。停止播放后恢复正式存档目录。本轮实际手动保存及反复跳转前后，正式存档哈希一致。[verified: CampaignDebugWindow.cs「BeginSaveScope」「EnteredEditMode」；verification.json「live.formalSavesUnchanged」]

播放中重新编译导致运行状态或界面失效时，跳转工具会尝试重新进入播放一次；如果仍失败，窗口显示错误，详细堆栈在 Console。[verified: CampaignDebugWindow.cs「staleScreen」「recoveryAttempts」「Debug.LogException」]

验证范围：全部 30 个日期／阶段检查点可恢复并接受下一步命令；实际编辑器连续切换 8 个入口，并核对存档隔离、暂停及菜单输入释放。本工具仅供编辑器使用，本轮没有构建 EXE。[verified: verification.json「editMode」「live」「delivery」]

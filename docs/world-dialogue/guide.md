# 世界阅读与监督员交互

范围仅为海报、物件阅读和门口监督员；本轮保留现有美术参数、材质、正文排版与电脑角色交互。`[verified: 本轮用户范围及美术约束；本轮生产修改范围]`

- 对准物件后仍用原左键进入阅读；阅读期间停止走动和鼠标转头，显示鼠标。短段整页显示，左键／空格／Enter每次只翻一页，最后一页确认后淡出并恢复探索。`[verified: ApartmentPlayer.Update/InteractSecondaryFocused；PropNarrationView.Show/Advance]`
- 正文不会因为物件投影进入文字区域而消失。最初仍按现有版面避让物件；极端情况下无可读空位，使用原右侧版面的可见fallback，允许与物件投影局部重叠。`[verified: PropNarrationView.UpdatePlacement/ChoosePlacement]`
- Esc先结束阅读；关闭过程中及关闭当帧的重复Esc由阅读状态消费，后续探索帧才恢复普通暂停入口。暂停与恢复保留页码，恢复当帧不推进。`[verified: MiniGameScreen.TryClosePropNarration；PropNarrationView.SetPaused/CanAcceptInput]`
- 监督员文字未显示完时，左键或空格先补全当前页，下一次输入才继续；原“继续”按钮和Enter也使用同一判定。新句出现当帧与同一帧的重复输入不继续推进。点击UI按钮时不会再额外触发一次背景推进。`[verified: DialogueTextPlayback.Advance；MiniGameScreen.AdvanceWorldDialogue/TryConsumeDialogueAdvance/PointerIsOverWorldControl]`

参考仅借鉴原作的输入占用、独立字幕通路和结束恢复，不复制代码、美术或剧情。原作叙事物件调用`ShowSubtitle`，NPC对话明确切UI并锁住移动／视角；本项目物件专注阅读及逐帧去重属于原创适配。`[verified: research/no-im-not-a-human/DecompiledCode/Assembly-CSharp/_Code/Rooms/NarrativeRoomObject.cs L32-L44；_Code/Infrastructure/DialogInteractable.cs L73-L92；_Code/Infrastructure/Player/PlayerService.cs MakeImmovable；本轮实现]`

验证结果与Mono包见[verification.json](verification.json)，边界记录见[implementation-notes](implementation-notes.md)。`[verified: 本目录文件]`

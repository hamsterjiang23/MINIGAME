# Deviations

- 2026-10-23 GM ending menu: hold N + O + R + M together on the in-game pause root to open four ending routes. Opening checks the current held state, including keys held before returning to the root; only a successful GM opening consumes the chord. After returning from GM, release at least one key before opening it again. Sequential letters are not accepted. Normal, quota failure and priest routes resume at the door; hidden ending resumes at the first self-review line. Esc returns to the pause root. Main menu, settings, manual and standalone beach previews do not open GM. The four-click title shortcut remains a separate direct-beach route. `[verified: PauseMenuView.ConfigureGmEndings/UpdateGmChord/TryCloseGm; MiniGameSessionHost.TryEnterGmEnding; GameRuntime.EnterGmEndingFromPause]`
- 2026-10-23 hidden ending shortcut: the existing “暂停” title is the secret button; four clicks with at most 1.25 seconds between clicks enter the real hidden ending. Count changed from the initial seven-click proposal to the user's four-click correction. `[verified: PauseMenuView.HiddenEndingClickCount/HiddenEndingClickInterval; user correction 2026-10-23]`
- 2026-10-20 perspective revision: remove persistent context and footer. Keep transient save/load feedback only after an action; the idle room menu has the title and four actions. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs Build/SetStatus]`
- The shared beach presentation keeps its pre-existing five actions; this request changes the room's four-action menu, not ending navigation policy. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface]`

- Room and beach pause menus share one authored visual component; terminal dialogue styling stays within its current CRT presentation. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs EnsureBuilt; ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface]`
- Original procedural charcoal and focus-frame textures replace any need to import reference-game artwork. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs CreateTextures]`

# Discovered edge cases

- The GM key chord is latched until at least one key is released; closing GM while keys remain held cannot reopen it. A chord pressed on a submenu also requires release before returning to the root. `[verified: PauseMenuView.UpdateGmChord; PauseMenuPlayModeTests.GmChordRequiresAllKeysAndANewPressOnThePauseRoot]`
- GM validates the selected ending on an isolated runtime restored from the current checkpoint, then cancels the old room/beach presentation and binds the replacement. Old callbacks retain their previous runtime; room-ending coroutines and scene-load completions also use a generation guard. GM does not write the manual save. `[verified: MiniGameSessionHost.TryEnterGmEnding/BeginRoomEnding/TransitionTo; EndingCampaignPlayModeTests.GmReplacesEachFlowAndPreservesManualSaveAndOldCallbackIsolation]`
- The shortcut uses unscaled time, ignores duplicate callbacks in one frame, and resets on pause close, disable, or another menu action. Submenus cannot activate it. `[verified: PauseMenuView.ClickHiddenEnding/ResetHiddenEndingClicks; PauseMenuPlayModeTests.HiddenTitleRequiresFourConsecutiveClicksAndResetsAcrossMenuActions]`
- An explicit core command records the bypass for save replay; patient decisions, trust, dates and treatment history remain intact. The presentation discards the prior cue and closes inspections/terminal before entering the existing ending flow; repeated activation after an ending is rejected. `[verified: GameSession.EnterHiddenEnding; CommandRecord.From/ToCommand; GameRuntime.EnterHiddenEndingFromPause; EndingFlowRuntimeTests.PauseShortcutEntersReplayableHiddenEndingWithoutChangingPatientHistory]`
- 2026-10-20 Mono Player 修复：可见 Direct3D11 窗口中确认暂停背景上下颠倒；在 RawImage 输出端统一纵向 UV，并在模糊完成后恢复原 RenderTexture.active。图形回归包含暂停／恢复／重复暂停及分辨率切换。

- Perspective changes actual pointer bounds: projected hit rectangles enclose the trapezoid, then inverse projection rejects empty corners. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuHitArea.cs Configure/Raycast]`
- Background capture must exclude the pause overlay. Its fade waits for one end-of-frame capture; batch mode skips capture and a 0.5-second unscaled watchdog releases rendering stalls. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuBackdrop.cs Capture/CaptureFrame/Update]`

# Questions for review

- Visual acceptance of the perspective/soft-focus strength remains with the user. `[opinion]`

- The existing sleep-fade regression locates the root Image named PauseModal; this hierarchy contract is preserved. `[verified: ../../UnityProject/Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs L368-372; ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs Build]`
- Repainting a paused runtime preserves keyboard focus. `[verified: verification.json tests.runs KeyboardFocusSkipsDisabledRowsSurvivesUpdatesAndRestoresPreviousSelection]`
- Save/load feedback appears inside the pause overlay. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs Notify; ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs SaveFromPause/LoadFromPause]`
- Standalone beach previews have no persistent session: save/load remain disabled. `[verified: verification.json visual.standaloneBeachSaveLoadEnabled]`
- The initial synchronous raycast test observed native Graphic depth -1 before Canvas registration. The corrected test waits for a valid depth, then requires full-viewport interception. An alpha workaround was removed after diagnostics disproved it. `[verified: ../../Artifacts/PauseUI/modal-diagnostic.json results; ../../UnityProject/Assets/MiniGame/Tests/PlayMode/PauseMenuPlayModeTests.cs FullscreenModalInterceptsClicksAndFadesWhileTimeIsStopped]`
- Resume and load rebind both reject underlying dialogue confirmation in the pause-close frame; the following frame works normally. `[verified: verification.json review; tests.runs ResumeSubmitCannotAlsoChooseAResponseOrAdvanceDialogueInTheSameFrame]`
- Both scenes prefer the same font family; beach hints and captions hide while paused and return afterward. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs ApplyPause/BuildInterface; ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs EnsureBuilt]`
- A paused resolution change refreshes the frozen background instead of stretching the previous aspect ratio. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuBackdrop.cs Update]`

# Validation

- 2026-10-23 shortcut: SDK 8.0.424 locked restore and Release build succeeded; .NET 387/387, pause Play Mode 8/8 and ending Play Mode 8/8 passed. Windows64 built with zero errors and one Pipeline diagnostic warning; standalone Player interaction was not replayed. `[verified: ../../Artifacts/HiddenEndingCheat20261023/verification.json dotnet/unity/build/playerInteraction]`
- Native room captures inspected at 1280x720, 1920x1080, 1024x768 and the original 2560x1440 Game view. All rows and shortcuts fit. `[verified: verification.json visual.inspected]`
- Four modal/input, one sleep-overlay, three campaign presentation and six beach checks passed. Windows packaging succeeded with one Pipeline warning; standalone interaction was not replayed. `[verified: verification.json tests/build]`
- Unity crashed natively in SourceAssetDBReadTxn/GetFileIDsForLoadingScene after an earlier test run. The project editor was restarted; the unrelated Unity 2019 project was left running. Subsequent tests and captures succeeded. The cause of the native crash was not established. `[verified: verification.json nativeEditorIssue]`

# Perspective revision handoff

- Revision deviations: two, transient action feedback retained and beach actions preserved. `[verified: Deviations above]`
- Most likely revisit: perspective and blur strength after user visual review. `[opinion]`
- Handled boundaries: projected pointer bounds, capture exclusion, render stalls, resolution changes. `[verified: Discovered edge cases above]`
- Current evidence: [perspective-verification.json](perspective-verification.json); earlier checks remain in verification.json. `[verified: Validation above]`
- Next reader: start with PauseMenuView and PauseMenuBackdrop. `[opinion]`

# Hidden ending shortcut handoff

- Requirement deviations: zero from the final four-click request. `[verified: PauseMenuView.HiddenEndingClickCount; user correction 2026-10-23]`
- Possible adjustment: the 1.25-second maximum gap is a tuning choice. `[opinion]`
- Covered boundaries: time-scale zero, duplicate callbacks, interrupted sequences, submenus, stale cues, save replay and ending completion. `[verified: ../../Artifacts/HiddenEndingCheat20261023/PauseMenuPlayModeTests.json results; EndingFlowRuntimeTests.PauseShortcutEntersReplayableHiddenEndingWithoutChangingPatientHistory]`
- Verification and Windows package hash: [report](../../Artifacts/HiddenEndingCheat20261023/verification.json). `[verified: report dotnet/unity/build/package]`
- Next reader: start with PauseMenuView.ClickHiddenEnding and GameRuntime.EnterHiddenEndingFromPause. `[opinion]`

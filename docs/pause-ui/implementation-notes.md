# Deviations

- 2026-10-20 perspective revision: remove persistent context and footer. Keep transient save/load feedback only after an action; the idle room menu has the title and four actions. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs Build/SetStatus]`
- The shared beach presentation keeps its pre-existing five actions; this request changes the room's four-action menu, not ending navigation policy. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface]`

- Room and beach pause menus share one authored visual component; terminal dialogue styling stays within its current CRT presentation. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs EnsureBuilt; ../../UnityProject/Assets/MiniGame/Runtime/DreamEndingController.cs BuildInterface]`
- Original procedural charcoal and focus-frame textures replace any need to import reference-game artwork. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PauseMenuView.cs CreateTextures]`

# Discovered edge cases

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

- Native room captures inspected at 1280x720, 1920x1080, 1024x768 and the original 2560x1440 Game view. All rows and shortcuts fit. `[verified: verification.json visual.inspected]`
- Four modal/input, one sleep-overlay, three campaign presentation and six beach checks passed. Windows packaging succeeded with one Pipeline warning; standalone interaction was not replayed. `[verified: verification.json tests/build]`
- Unity crashed natively in SourceAssetDBReadTxn/GetFileIDsForLoadingScene after an earlier test run. The project editor was restarted; the unrelated Unity 2019 project was left running. Subsequent tests and captures succeeded. The cause of the native crash was not established. `[verified: verification.json nativeEditorIssue]`

# Perspective revision handoff

- Revision deviations: two, transient action feedback retained and beach actions preserved. `[verified: Deviations above]`
- Most likely revisit: perspective and blur strength after user visual review. `[opinion]`
- Handled boundaries: projected pointer bounds, capture exclusion, render stalls, resolution changes. `[verified: Discovered edge cases above]`
- Current evidence: [perspective-verification.json](perspective-verification.json); earlier checks remain in verification.json. `[verified: Validation above]`
- Next reader: start with PauseMenuView and PauseMenuBackdrop. `[opinion]`

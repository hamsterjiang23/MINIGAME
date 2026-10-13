# Deviations

- The reference roadmap lists URP as a candidate. This prototype uses Unity 2022.3 Built-in and generated Standard materials to minimize rendering dependencies while validating the interaction loop; a later URP pass must replace materials and revalidate lighting. `[opinion]`
- Reference semantics are reconstructed in original source and graybox geometry. No recovered commercial scene, image, audio, shader or dialogue text is imported. `[verified: Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs CreateAndValidate; Packages/manifest.json dependencies]`
- Normal UPM resolution failed before compilation on this Editor installation. A reproducible isolated Assets-copy project validates the same C# sources with -noUpm; its two copied Yarn asmdefs omit unused optional integrations and enable real cached TMP. Normal UPM success remains unverified. `[verified: Tools/Invoke-OfflineValidation.ps1 Prepare-OfflineProject; ../Artifacts/Unity/offline-project-provenance.json scope]`

# Discovered edge cases

- Saved gameplay can resume during a branch/reaction cue; callbacks capture the current cue token and encounter IDs rather than applying another disposition. `[verified: Assets/MiniGame/Yarn/MiniGameYarnBridge.cs RunNodeAsync and SelectResponse]`
- World movement is gated by RuntimeView.Input; pause, inspection and presentation lock the room controls. `[verified: Assets/MiniGame/Runtime/ApartmentPlayer.cs Update]`
- A restored EnterTerminal cue must preserve the room return pose; the camera adapter captures it once rather than replacing it with the terminal pose. `[verified: Assets/MiniGame/Runtime/PresentationEffects.cs Bind and Present]`
- Escape unpauses before attempting to close an inspection, including a paused inspection restored from disk. `[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs Update]`
- After the ending cue completes, the screen retains the core ending name and restart-menu hint instead of leaving a locked room with no outcome text. `[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs Render and EndingName]`
- Dialogue and disposition rectangles occupy separate vertical ranges; the conversation panel cannot intercept clicks on the three disposition buttons. `[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs EnsureBuilt and Render]`
- Restoring a checkpoint without a cue clears fade, glitch and transition locks even when the previous cue had a dirty overlay. `[verified: Assets/MiniGame/Runtime/PresentationEffects.cs Bind]`
- Pausing on the final yielded sleep or camera frame holds completion until unpause instead of stranding a rejected cue. `[verified: Assets/MiniGame/Runtime/PresentationEffects.cs CameraTransition and SleepTransition]`
- A restored active terminal snaps to its destination even if a room return pose was already captured by a partially completed transition. `[verified: Assets/MiniGame/Runtime/PresentationEffects.cs Bind]`
- The pause menu moves above the full-screen fade; its return button is checked with real uGUI raycasts after a render frame, including a fully opaque sleep fade. `[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs Render; Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs RestoredEffectsClearOverlaysSnapTerminalAndPauseAtFinalSleepFrame]`
- Headless PlayMode runs thousands of frames per second, so presentation test deadlines use realtime rather than a fixed frame budget. `[verified: ../Artifacts/Unity/offline-playmode-observed-results.xml durations; Assets/MiniGame/Tests/PlayMode/PrototypePlayModeTests.cs WaitForButton]`

# Questions for review

- Formal dialogue, portraits, dream scenes and outcome staging remain design/content replacements for explicitly labelled developer placeholders. `[opinion]`

Deviations: 3; the Built-in versus URP choice is the one most likely to be revisited. `[opinion]`
Edge cases: 11 documented above. `[verified: Discovered edge cases list]`
Implemented boundary: Unity owns input and presentation; the runtime owns state. `[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs InstallRuntime]`
Actual validation: isolated real Editor scene generation, four graphics PlayMode tests, four reviewed captures and Windows Mono build passed; 145 C# source hashes match. Build reports one null-renderer ambient/reflection-probe warning from -nographics. `[verified: ../Artifacts/Unity/offline-playmode-results.xml passed=4; unity-windows-build.json errors=0/warnings=1; offline-source-validation.json mismatches=0; build-warnings.json]`
Next read: UnityProject/README.md and current Artifacts/Unity logs; Editor, PlayMode and Windows-build outcomes are determined there. `[opinion]`

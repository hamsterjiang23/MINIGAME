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

## 2026-10-15 normal Editor recovery

- Deviation: normal UPM now works using a child-process ALLUSERSPROFILE fallback to Windows CommonApplicationData; global settings and the Editor installation are unchanged. The previous offline-only validation remains historical. [verified: Tools/Open-Project.ps1 EnvironmentVariables; ../docs/gameplay-migration-verification.json unity.normalUpmRecovery]
- Edge cases: embedded Yarn tests require FindForAssetPath on Unity 2022; screenshot tests require com.unity.modules.imageconversion. Initial import can report stale assembly failures before resolving the changed manifest, so the final startup was checked separately. [verified: Packages/dev.yarnspinner.unity/Tests/Runtime/DialogueRunnerTests/BuiltinLineProviderTests.cs L94/L128; Packages/manifest.json; ../Artifacts/Unity/EditorRepair/normal-playmode.log; ../Artifacts/Unity/editor.log]
- Scene entry: this installation restored Untitled even with -openfile. The launcher now executes OpenPrototype after compilation, with an unsaved-scene prompt for manual menu use; it opens the existing scene without regenerating assets. [verified: ../Artifacts/Unity/EditorRepair/openfile-editor.log; Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs OpenPrototype; ../Artifacts/Unity/editor.log MINIGAME_MAIN_SCENE_OPENED] [opinion]
- Validation: four graphics PlayMode tests passed against the normal UPM project, exit 0; the final visible Editor loaded InspectionApartment with zero compiler or package-resolution failures. Licensing-client diagnostics and an update-check 404 remain, followed by successful license resolution; no Player/IL2CPP rebuild was run. [verified: ../docs/gameplay-migration-verification.json unity.normalUpmRecovery]
- Summary: one recovery from the offline route; two compilation compatibility fixes; next read Tools/Open-Project.ps1 and unity.normalUpmRecovery. [verified: ../docs/gameplay-migration-verification.json unity.normalUpmRecovery] [opinion]

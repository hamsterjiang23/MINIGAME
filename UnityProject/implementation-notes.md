# Deviations

- The reference roadmap lists URP as a candidate. This prototype uses Unity 2022.3 Built-in and generated Standard materials to minimize rendering dependencies while validating the interaction loop; a later URP pass must replace materials and revalidate lighting. `[opinion]`
- Reference semantics are reconstructed in original source and graybox geometry. No recovered commercial scene, image, audio, shader or dialogue text is imported. `[verified: Assets/MiniGame/Editor/ReferencePrototypeBuilder.cs CreateAndValidate; Packages/manifest.json dependencies]`

# Discovered edge cases

- Saved gameplay can resume during a branch/reaction cue; callbacks capture the current cue token and encounter IDs rather than applying another disposition. `[verified: Assets/MiniGame/Yarn/MiniGameYarnBridge.cs RunNodeAsync and SelectResponse]`
- World movement is gated by RuntimeView.Input; pause, inspection and presentation lock the room controls. `[verified: Assets/MiniGame/Runtime/ApartmentPlayer.cs Update]`
- A restored EnterTerminal cue must preserve the room return pose; the camera adapter captures it once rather than replacing it with the terminal pose. `[verified: Assets/MiniGame/Runtime/PresentationEffects.cs Bind and Present]`
- Escape unpauses before attempting to close an inspection, including a paused inspection restored from disk. `[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs Update]`
- After the ending cue completes, the screen retains the core ending name and restart-menu hint instead of leaving a locked room with no outcome text. `[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs Render and EndingName]`
- Dialogue and disposition rectangles occupy separate vertical ranges; the conversation panel cannot intercept clicks on the three disposition buttons. `[verified: Assets/MiniGame/Runtime/MiniGameScreen.cs EnsureBuilt and Render]`

# Questions for review

- Formal dialogue, portraits, dream scenes and outcome staging remain design/content replacements for explicitly labelled developer placeholders. `[opinion]`

Deviations: 2; the Built-in versus URP choice is the one most likely to be revisited. `[opinion]`
Edge cases: 6 documented above. `[verified: Discovered edge cases list]`
Implemented boundary: Unity owns input and presentation; the runtime owns state. `[verified: Assets/MiniGame/Runtime/MiniGameBootstrap.cs InstallRuntime]`
Cheap validation: own runtime, editor-builder and PlayMode-test sources compiled against local Unity 2022.3 API assemblies with fixed .NET 8.0.424; Yarn bridge was stubbed in that isolated probe. `[verified: current Probe build output "0 Warning(s), 0 Error(s)"]`
Next read: UnityProject/README.md and current Artifacts/Unity logs; Editor, PlayMode and Windows-build outcomes are determined there. `[opinion]`

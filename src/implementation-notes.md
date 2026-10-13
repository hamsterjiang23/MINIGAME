# Implementation notes

## Deviations

- The source game uses Unity 6000.3.10f1; this migration retains the project's Unity 2022.3 baseline. Gameplay behavior is adapted to the existing authoritative GameSession rather than copying original implementation or assets. [opinion]
- Context7 credentials are unavailable; Unity API calls will be checked against official 2022.3 documentation and the pinned Yarn Spinner 3.2.8 source. [opinion]
- Built-in rendering is used for the original graybox prototype; URP remains a later rendering decision. [opinion]
- Both available Unity 2022.3 installations fail UPM even for a minimal project. Actual Editor validation uses a separate ignored offline project with dependencies copied from the local cache and `-noUpm`; normal UPM import remains unresolved. [verified: ../docs/gameplay-migration-verification.json unity.normalUpmImport]
- Yarn 3.2.8 is embedded with two optional assembly-reference lists pruned. Keep TextMeshPro 3.0.7: the unmodified fallback TMP shim produced real compiler errors in Unity 2022.3. [opinion]

## Discovered edge cases

- The system only had .NET 10; fixed SDK 8.0.424 is now installed in ignored `.dotnet/`, with its official SHA512 checked. [opinion]
- The connected Unity MCP client belongs to a different 2019.4 project. Validate this project's own 2022.3 Editor separately. [opinion]
- Source CharacterSOData has 73 unresolved assets and its shader export contains placeholders. Do not transplant missing configuration into gameplay. [opinion]
- Three camera-checkpoint corruption tests exposed skipped encounter lifecycle checks. Restore now checks the first normal round and the last authoritative reaction command; all three regressions pass. [verified: ../Artifacts/Tests/migration-verification-report.json IllegalCameraCheckpoint]
- Stopping Yarn does not by itself await outstanding line/options callbacks. The presenter and bridge drain old work before a restored node starts; focused static review and the real PlayMode regression passed. [opinion]
- Unity package compilation does not inherit Directory.Build.props nullable settings. Explicit `#nullable enable` matches the existing .NET compilation context without changing rules. [opinion]
- The final animation frame must wait for unpause before cue completion; loading a new runtime must clear old fade/glitch and snap active terminal checkpoints. The pause menu renders above transition overlays. [verified: ../UnityProject/Assets/MiniGame/Runtime/PresentationEffects.cs Bind/CameraTransition/SleepTransition; ../UnityProject/Assets/MiniGame/Runtime/MiniGameScreen.cs Render]

## Questions for review

None. Existing three-day patient rules remain development configuration; presentation content is explicitly a prototype. [opinion]

Deviations: Unity version, rendering pipeline, offline Editor validation and optional package integration are explicit above. [opinion]
Edge cases: SDK pin, isolated MCP client, missing source assets, corrupt host checkpoints and cancellation drain. [opinion]
Validation: Core 73 + Presentation 77 tests and four real graphical Unity PlayMode tests pass; Windows Mono build succeeded, and the player reached the initial phase without startup exceptions. [verified: ../docs/gameplay-migration-verification.json dotnet/unity]
Source boundary: original rules/content/geometry; commercial game implementation is evidence for the lifecycle. [opinion]
Next read: docs/reference-gameplay-migration.md and UnityProject/README.md for current Unity outcomes. [opinion]

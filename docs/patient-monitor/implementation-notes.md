# Deviations

- Rooms use original procedural 3D meshes; patients use newly generated seated cutouts with preserved dialogue IDs. This retains the existing 2D character system while providing a real room camera. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientMonitorRoom.cs Build; ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs Build]`
- Existing realistic art remains as a fallback; the portrait catalog selects the new expression sheets. `[verified: ../../UnityProject/Assets/MiniGame/Editor/CampaignBuilder.cs ImportPortraits; ../../Artifacts/PatientMonitor/import-stability.json paths]`

# Discovered edge cases

- The dream stage already owns layer 29 and self review owns layer 30. Patient rooms reserve layer 31 and live far outside the apartment camera range. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/DreamAnimationStage.cs StageLayer; ../../UnityProject/Assets/MiniGame/Runtime/SelfReviewMonitor.cs ProxyLayer; ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs StageLayer/Build]`
- Archive and self-review hide the regular patient feed through the existing review hierarchy; patient rooms store no authority over disposition or session state. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs Render; ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs Show]`
- RenderTexture.active is restored after camera rendering and blitting. Editor-only previews use an isolated preview scene and clean up runtime-owned materials, meshes and textures. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs RenderFrame/ReleaseStage; ../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs StartPreview/StopPreview]`
- Generated sheets can have widths not divisible by five. Import uses rounded shared cell boundaries; each imported sprite retains its stable portrait ID. `[verified: ../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs ImportArtwork]`
- Generated body positions vary inside equal cells. A common alpha crop per character and individually registered feet/center pivots prevent expression changes from translating the actor. Source PNGs stay unchanged. `[verified: ../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs CharacterCrop]`
- Pitching a flat patient cutout toward the overhead camera put its body behind the mattress. The final cutout stays upright, with feet at bed-front z=-0.65. All 27 expressions in wide/close views passed GPU furniture-visibility checks at the terminal display aspect. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientMonitorRoom.cs ActorAnchor; ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs RenderFrame; ../../Artifacts/PatientMonitor/occlusion.json results]`
- Paused profile edits recreate the render texture. A dirty first-frame flag renders one valid frozen frame after rebuilding, without advancing the animation clock. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs Ready/Advance; ../../Artifacts/PatientMonitor/monitor-tests-final.json results]`
- Editor preview cameras explicitly bind their preview scene; preview dimensions update on every editor tick to cover Undo. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs Build; ../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs TickPreview]`
- The new zoom button exposed a missing CanvasRenderer in the existing custom focus graphic under RectMask2D. Creating the renderer explicitly fixes native UI construction. Initial failures are retained, and presentation/self-review/pause regressions pass after the fix. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/TerminalButtonVisual.cs CreateFocusOutline; ../../Artifacts/PatientMonitor/presentation-initial-failure.json; ../../Artifacts/PatientMonitor/presentation-tests.json; ../../Artifacts/PatientMonitor/self-review-tests.json; ../../Artifacts/PatientMonitor/pause-tests.json]`

# Questions for review

- User visual acceptance remains separate from agent inspection and GPU visibility checks. `[opinion]`

# Handoff

- Deviations: two presentation choices, procedural 3D rooms and seated cutouts. `[verified: Deviations]`
- Most likely to revisit: replace cutouts with rigged actors if later animation scope requires them. `[opinion]`
- Edge cases handled: layer ownership, archive hiding, render-target state, editor cleanup, cell registration, bed depth, paused reconfiguration and masked UI focus. `[verified: Discovered edge cases]`
- Read first: guide.md, then verification.json and art-generation.json. `[opinion]`
- No core rule state or player save was changed by the new visual adapter. `[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs; ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs diff]`

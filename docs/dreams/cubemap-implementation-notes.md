# Deviations

- User explicitly chose a static anamorphic set: the existing doorway Player spawn eye is the only correct viewing position; walking toward the bed should reveal the flat painted room. The room now photographs its selected skybox once and freezes that texture for the whole ending, including pause and checkpoint restores.
- Projection samples world-surface position minus the fixed doorway eye, with no per-axis room-size normalization. This preserves the skybox view at the anchor while producing intentional off-axis wall perspective. White borders are retained as an optional diagnostic toggle, off by default to keep the doorway illusion.
- Added a baked beach Cubemap/skybox authoring path so the designer can select another skybox. The background capture prefab remains only as a baking source and fallback; room movement no longer invokes the dynamic capture path.

## Earlier prototypes (superseded by the fixed doorway scenery)

- Added user-requested white outlines to all twelve physical room edges. The room shader derives edge distance from local room coordinates and blends white over the existing view-ray Cubemap. Boundary coordinates are used only for lines; source movement and panorama projection are unchanged.
- The frozen-background translation test disables only the edge overlay because visible room boundaries correctly have physical parallax. Separate rendered checks cover the twelve edges, interior color and occlusion.

- Further play feedback clarified that stable means coherent, not translation-invariant: a stationary beach made WASD feel like moving only the bed. The capture viewpoint now receives player eye displacement from the fixed room start, with the same yaw transform as sampled rays. Nearby sand/coast and the real bed therefore both respond to walking.
- A changed capture position bypasses the stationary 10 Hz refresh limit; every movement frame gets one complete six-face cube. Lying interpolates from the saved translated eye origin back to the existing pillow path, preserving checkpoint continuity. No new save fields are needed.

- Follow-up user acceptance supersedes fixed-wall image perspective: the panorama must remain coherent during walking while the real bed keeps parallax. Six room meshes now sample world position minus the rendering camera position; removed per-axis room-size normalization. Source capture stays at one sea viewpoint while exploring.
- Earlier finite-wall tests proved orientation and deliberately expected wall-image parallax; they did not establish stable panoramic viewing from room corners. Replaced that assumption with rendered-image comparisons across multiple positions in a rectangular room.

- User clarified that the runtime cube must be mapped to four finite room walls, floor and ceiling. Replaced the room camera's infinite skybox path with six inward-facing meshes at the original room bounds; cube sampling is fixed to local surface position. The captured sea source and six-face swap remain unchanged.
- Physical bed drift now clamps the rotated bed bounds inside the finite room. The offshore capture still follows the original virtual ending trajectory.
- Preview Stop initially revealed that live restoration during destruction re-enabled an unloading SupervisorDoorwayView. Destruction now releases owned capture resources only; explicit live exits restore the room. The preview also stops before Play Mode unload.

- Work is implemented in Artifacts/DreamCubeWork because the primary Unity project has another running batch process and unrelated changes. Owned files will merge only after validation and original-file hash checks.
- Existing sea/sand shaders use absolute world coordinates. A default-zero _DreamOrigin offset preserves the original beach and permits an isolated capture stage.

# Discovered edge cases

- A cross-scene DreamSea load previously replayed the generic saved pillow camera before calibrating the doorway. Room-ending loads now discard that redundant generic pose and use the ending checkpoint, retaining the scene's authored standing eye for projection.
- The normal self-review route can also leave a seated camera offset at ending entry. ApartmentPlayer now remembers its authored standing eye before presentation binding, so both fresh entries and loads calibrate the doorway independently of seated or lying camera state.

- Unity dynamic batching can pretransform mesh vertices into world space. The outline derives its boundary coordinate from world position minus the explicit room center; raw vertex coordinates incorrectly whitened face interiors in the first nonzero-center render test.

- Cubemap source must exclude bed renderers, UI, cameras and gameplay/audio controllers; the visible room bed remains a real 3D object.
- Source ambient lighting and external-light masks must be restored after every capture, along with RenderTexture.active.
- Legacy beach Walking checkpoints have no room pose and now restore to the doorway start; lying/drifting retain phase and subtitle clocks.

# Questions for review

- None. User-approved scope is hidden DreamSea ending only; daily dream images and flow remain unchanged.

## Preview entry follow-up

- Added an Editor-only direct preview menu and ending-settings button, using temporary Play Mode rather than advancing the campaign.
- Preview disables save/load/restart actions, including save/load callback guards; stopping restores the prior saved scene setup.
- An Editor smoke invoked the actual Open/Stop methods, reached the live cubemap, exercised guarded save/load callbacks, and verified player save and profile bytes stayed unchanged. Evidence: Artifacts/Unity/DreamCubemapPreview20261021/verification.json.

- Capture-source generation uses a preview scene because Unity refuses a new additive scene while an untitled scene is unsaved. The source beach is not saved or rewritten.
- Cleanup records the incoming CharacterController enabled flag before releasing the seated/opening presentation.
- HideAndDontSave stage clones have no valid public Scene handle. Leak checks enumerate all live source components by activeInHierarchy, rather than filtering them out by Scene.IsValid.

# Historical dynamic implementation summary

- Deviations: isolated source snapshot, default-zero shader origin offset, preview-scene source generation.
- Most likely to revisit: higher capture resolution or refresh rate should be measured again on the target hardware.
- Edge cases covered: complete six-face swaps, room cleanup, legacy pose migration, pause, ordinary-save restore and hidden-ending reentry.
- Scope: only DreamSea presentation changes; ordinary daily illustrations remain byte-identical.
- Read first next time: cubemap-guide.md and cubemap-verification.json, with native evidence distinguishing fast legal-command traversal from earlier visual playback.

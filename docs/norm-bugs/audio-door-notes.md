# Deviations

- None; scanner timing and door hold follow the BUG document's paragraphs 19–21.

# Discovered edge cases

- Scan was mapped to entering terminal review or opening a patient inspection, rather than the morning instrument cue. It now follows a new `MorningInspection` cue token; rebinds and pause changes do not replay the sound.
- The combined scene's authored alignment and observation positions are `(-6.89472, .88711995, -3.1748798)` and `(-8.89472, .88711995, -3.1748798)`: a further 2 metre displacement along the doorway normal after initial alignment.
- The observation anchor now coincides with alignment, 0.85 metres inside the doorway. Coincident poses finish entry after the door opens, and return uses the full 1.1 seconds for motion instead of an initial stationary half-leg. Separated custom anchors retain their existing path.
- Latest user steering keeps the supervisor permanently outside the physical doorway. `SupervisorDoorwayView` creates a world SpriteRenderer 1.75 metres forward of the inside alignment (0.9 metres past the doorway), preserves original portrait aspect at 1.8 metres height, and raises its bottom 0.1 metres above the standing floor. It has no camera/Canvas parent or interaction collider; ordinary world depth occludes it behind the frame or closed door. `[verified: SupervisorDoorwayView.cs Bind/PlaceAtDoorway/EnsureSurface]`
- Supervisor visibility no longer depends on door state or inspection/briefing cues. The existing actor remains enabled while the host is active, through pause, closing the door and entering work; checkpoint rebind reuses it. `[verified: SupervisorDoorwayView.cs Bind/OnDisable/OnEnable; reports/supervisor-presence/supervisor-tests.json]`
- Fresh Day0 effects can disable and shorten the player's capsule before supervisor binding. Placement uses the effects' captured standing return position when available, so the initial seated pose cannot lower the permanently placed actor. `[verified: SupervisorDoorwayView.cs PlaceAtDoorway; NormWorldPresentationPlayModeTests.RealRoomManualKeepsPauseUntilTheSecondEscapeAndMorningCanBeRead]`

# Questions for review

- Unity validation is coordinated by the root agent; no second Unity process or SDK test run is started by this worker.

Deviations: 0.
Most likely revisit: the exact doorway composition, if the supplied reference needs a later camera position adjustment.
Edge cases found: incorrect scan ownership, pause/rebind replay risk, a two metre hall push, and a stationary return half-leg.
Next validation: `MiniGameAudioPlayModeTests` and `DoorViewPlayModeTests` in the isolated Unity project.
Read first: runtime audio director and door camera, then the two PlayMode fixtures.

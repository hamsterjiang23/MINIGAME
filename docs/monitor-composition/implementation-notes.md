# Deviations

- Preserve the existing ward plate, character artwork and the uncommitted VV04 bed placement. Investigate duplicate rendering before changing character art or framing.

# Discovered edge cases

- Each PatientSurveillanceFeed created its stage at the same world position and used the same layer. The same-scene two-feed GPU regression reproduced cross-feed contamination: adding a second patient changed 43,379 pixels in the first output. Separate stages by 128 units along Z; allocation scans existing stages, including disabled ones, instead of relying on a static pool.
- TerminalView also keeps previous/current fallback portraits below the live feed. The fallback must not become an additional visible character when the live monitor is available.
- The isolated single-terminal capture already occluded fallback portraits with the opaque video. Their enabled state was nevertheless incorrect; explicitly hide them while the live feed is showing, and restore them when the feed is unavailable. This is defensive composition, not evidence that fallback alone caused the user's screenshot.
- Profile edits can hide a feed without a new dialogue line. VisibilityChanged now updates the fallback on selection, hiding and rebuild; rebinding a new image/profile clears the old owned resources first.
- Script reload can preserve stage references but lose the room dictionary/static data. Scan live stage positions and rebuild if the owned room dictionary is empty; full game-session hot reload was not tested or promised.
- Real five-person captures showed seated doctor/priest poses floating in front of the bed and half-body cutouts ending above the screen edge. Add per-portrait placement overrides, preserving the pre-existing VV04 bed anchor and all artwork/color settings. Doctor/priest anchors were raised once more after visual review of seat contact.
- The wider terminal regression found F3 had returned in current source despite the user's earlier removal request. Reinstate F2/F3 rejection and remove the visible F3 hint; keep passive transcript rendering. The previously failing Amber native composite test passes.
- Shared Editor is running. Unity tests and capture use the isolated project; no stop, restart or save of the user's scene.

# Questions for review

- Check a real VV04 terminal frame and all five patient feeds. Preserve the user's current monitor colors, distortion controls and character identity.

## Handoff summary

Deviations: 2, per-portrait placement corrections and restoring the previously requested F2/F3 restriction.
Most revisitable: doctor/priest seat anchors and the foreground half-body framing, based on user visual feedback.
Edge cases: concurrent feeds, disabled feeds, fallback availability, rebind, pause, stage cleanup and script-reload ownership.
Validation: failing two-feed reproduction before changes; 15 final PlayMode checks pass, with actual five-patient/terminal GPU captures.
Read first: guide.md, verification.json and Artifacts/MonitorComposition/Final/monitor-vv04-terminal.png.

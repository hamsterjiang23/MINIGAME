# Deviations

- None from the confirmed behavior: door camera holds outside until E returns; door/bed have no focus highlight; other interactions have white outlines and per-object distances.

# Discovered edge cases

- Door motion must wait for camera entry completion before acknowledging its runtime token; return must finish before closing the physical leaf.
- Preserve ordinary ToggleRoomDoor for legacy mechanics tests and older hosts; the combined scene opts into EnterDoorView/ExitDoorView.
- The global 3.2m ray cap would silently override a designer's longer object range. The first collider now determines occlusion, and its own interaction distance gates both focus and use.
- Keep the body inside the threshold during camera travel. Existing saves do not store player world coordinates; new sessions use their available room pose as the return point, while in-session restores retain the captured original pose.
- Restore the old door camera before rebinding the terminal presentation, so a terminal checkpoint cannot capture an outside position as its room return pose.
- DoorView requires its own visible return prompt; world reticle and other interaction prompts remain separately gated.
- Re-enabling the camera component during a pending return resumes that cue instead of stranding presentation input.
- Combined-room tests now suspend live hardware input during their explicit pose/click sequences; otherwise mouse motion after a transition invalidates exact restored-angle assertions.
- Real Game-view capture found the first outside anchor looking directly into the open leaf. Shifted both camera anchors 0.35m toward the latch side and aimed along the corridor; added a leaf-clearance and outside-sightline regression.
- The user's live retro profile uses 8px blocks at full strength. Adapted white silhouette/border width to the active camera's pixel size, preserving the profile, and added real post-process continuity/bypass tests.
- User corrected the viewing direction: align at the fixed door-front point facing the doorway normal, then push straight along that normal without corridor-facing yaw. Updated both saved anchors and new-scene defaults while retaining their leaf-clearance offset.

# Questions for review

- None pending.

## Handoff

- Deviations: no scope expansion; runtime door-view behavior is opt-in for the combined scene, leaving legacy hinge APIs intact.
- Most likely to revisit: full cross-session player-position saving, which the pre-existing host format does not support.
- Edge cases: cue ordering, pause/rebind, hardware-input test isolation, leaf clearance, coarse-pixel outline continuity.
- Static review: camera/restore/UI issues fixed and reviewed; adaptive outlines reviewed without remaining findings.
- Read first: ../reference-room-guide.md and ../combined-interactions-verification.json.

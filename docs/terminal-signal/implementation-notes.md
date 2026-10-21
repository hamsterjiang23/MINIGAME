# Deviations

- Reuse the existing monitor RawImages: the shared opening surface switches between the Norm profile and monitor profile when its content changes. Patient and self-review feeds use the monitor profile. No fullscreen scene pass is needed.
- F6 opens an overlay for direct parameter editing. The panel blocks gameplay input while effects keep animating; Escape closes it without also opening pause. Explicit game pause freezes the effect clock.

# Discovered edge cases

- The Norm PNG has transparent margins. Displaced RGB channels need their own alpha coverage; displaced samples outside its UV rectangle must remain transparent.
- The terminal renders through an offscreen Canvas. The control panel lives on the main overlay Canvas and must be brought to the last sibling when opened.
- Material instances belong to individual RawImages and are released with their components. Runtime settings clone the project asset; saving writes a separate visual-settings JSON, never the campaign save.
- Existing patient-video grading and faint fixed scanlines remain part of its source image; these new controls govern the added moving interference.
- GPU baseline comparison caught framebuffer alpha being multiplied twice. The shader now follows the installed UI/Default premultiplied output convention; strict baseline comparisons are retained.
- The tuning overlay must gate dialogue events without caching its temporary block in button availability. Otherwise a line installed during tuning stays disabled after closing.
- The existing terminal display uses an Overlay render queue. The tuning panel owns a Canvas with sorting override above its parent and its own raycaster, so it has an explicit rendering and input order.

# Questions for review

- User visual acceptance and target-device performance remain to be evaluated separately from import, PlayMode and GPU checks.

# Handoff

- Implementation choices: local RawImage processing and an F6 overlay; two independent parameter groups.
- Most likely to revisit: default burst strength and frequency after user visual review.
- Regression cases: alpha, pause/freeze, profile isolation, input interception and dialogue arriving during tuning.
- Evidence: 17 passing tests, in-engine captures, successful Windows Mono build and Player startup smoke.
- Read first: guide.md, verification.json; raw evidence and the isolated validation snapshot remain in Artifacts/TerminalSignal.

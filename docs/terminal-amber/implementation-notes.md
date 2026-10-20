# Amber terminal implementation

## Deviations

- User steering: remove F2 settings and F3 history-browsing modes; the transcript remains a passive conversation display.
- User supplied PMS_Unity_Resources_v0.8.12 during implementation. Imported its original GUIDs, font, frame artwork and shader as the resource foundation; retained the host's native controls to preserve Yarn and rule integration. The packaged demo remains separate.
- Extended the supplied CRT shader with the host's deterministic shutdown collapse. Source package manifest is retained before these integration edits.

- Hidden-window full-backbuffer captures are black. Standalone previews explicitly render the native UI RenderTexture through the actual CRT material and validate non-black pixels; these are labeled as GPU composites.

## Discovered edge cases

- `ExitTerminal` also closes an individual patient's video channel. Only a leaving-directory cue may run the full shutdown and room return.
- Existing tests assume no default response selection and manual departure after the final patient. Those expectations must change with this accepted plan.
- Original working copies of overlapping runtime files are retained in ignored `Artifacts/TerminalAmber/Baseline/` before edits.
- PMS source-layer default moved to unused layer 27 because the game's dynamic patient feed already uses layer 31.
- Reference HTML SHA256: `8d102a75618fa02b7b3c6ceac0609cb3f6e66521c8e62f00499bfaf11d57aadb`.

- Review fixed live boot skipping, pause-resume Enter consumption, top EventSystem hit ordering/modal suppression, and narration/transcript wrapping.
- Noto font line metrics required vertical overflow for fixed single-line cells; narration and transcript keep explicit wrapping/clipping.
- Restored the existing saturation control as a warm-gray tint blend; verified its output changes independently of patient identity.
- The first isolated build still contained an older RoomLightingController with an Editor-only Light member. The current working copy already removed that member; the build copy was synchronized without editing the working copy.
- Standalone automation waits for a rendered frame before pointer dispatch and distinguishes the morning door interaction from the apartment door-view affordance.

## Questions for review

- None.

Deviations: 4, including the user-requested resource foundation and F2/F3 removal.
Most likely to revisit: exact CRT/font treatment after human review of the delivered screenshots.
Edge cases: 10 recorded, including cue reuse, loading, input gating, text metrics and hidden-window capture.
Validation: 295 .NET tests, 32 distinct staged PlayMode cases, 27 GPU visibility checks, Windows Mono build and standalone UI automation passed.
Read first next time: guide.md and verification.json; original pre-turn overlapping sources are under Artifacts/TerminalAmber/Baseline.

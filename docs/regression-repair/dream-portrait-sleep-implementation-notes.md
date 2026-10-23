# Deviations
- None. Keep artwork bytes, strict portrait IDs, dream duration and the 1.5-second sleep hold unchanged.

# Discovered edge cases
- The supplied doctor reference labels the seated pose ED01, but the supplied and imported seated PNG is ED04. Placement follows the visible pose without remapping IDs.
- Old sleep checkpoints have no lying orientation; preserve their previous orientation when resuming instead of changing a saved animation halfway through.
- Pillow and mattress renderer bounds establish the physical long-axis orientation. Without authored bed parts, the prior ceiling pose receives a clockwise 90-degree roll.
- Source-pixel tests explicitly disable the new border; production dreams keep 15 seconds and ten original frames. The previous editor/sequence fixtures still expected the superseded five-second defaults and were updated.

# Changes and verification
- Each dream has independent left/right/bottom/top border widths and strengths, softness and fixed contour irregularity. The shader protects its center and uses no time/frame input in the border.
- VV04 uses height multiplier .42 and bottom anchor (.515,.30); ED04 uses .45 and (.52,.29), approximating the supplied JPG room-wide figures on the bed's left long edge. Other portrait entries and image bytes are unchanged.
- New sleep checkpoints store the pillow orientation; sleep and wake share it. The eye moves to pillow top + .12m before rotating, then restores the actor-relative standing eye after waking. Existing black hold remains 1.5 seconds.
- Initial worker handoff used only git diff --check. The owner subsequently compiled the project, ran DreamImageSequence (6), DreamSettingsEditor (6), BedtimeGuidance (3), the supplied seated-portrait test (1), and the Day1 dream/bed return test (1); all passed. Results are stored in Artifacts/ExperienceRevision20261023/*Tests.json and the named single-test JSON files.
- The owner and independent reviewer checked the supplied seated reference JPGs against 1920x1080 and 2560x1440 native feed captures. Hips contact the left bed edge, feet reach the floor, and both actors retain a medium room scale. Static dream-edge renders at both resolutions keep the central image clear. See Artifacts/ExperienceRevision20261023/Visuals/reference-seated-* and dream-black-edges-*.

# Handoff summary
- Deviations: 0.
- Native seat-contact and border-strength review completed at both requested resolutions.
- Edge cases recorded: source label mismatch, old checkpoints, missing bed geometry, old test defaults.
- Read first: this note and the actual Desktop supplied JPGs; do not substitute historical generated previews.
- Remaining whole-task verification is tracked in experience-implementation-notes.md and the owner verification report.

# Questions for review
- None. Visual comparison is an implementation check, not a claim of user acceptance.

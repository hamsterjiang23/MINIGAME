# Deviations
- None. Replace the 11 supplied named portraits; retain unspecified expressions and characters.

# Discovered edge cases
- Campaign imports preferred the generated sheets. NamedPortraits now takes priority both during campaign imports and sheet imports.
- PNG canvases contain transparent margins. Preserve source bytes; store alpha bounds as Sprite metadata.
- The existing Ward_Original.jpg is the requested white ward. Optional profile Backdrop renders this plate with upright portrait cutouts through the existing CCTV signal pass; leaving Backdrop empty retains the 3D mode.

# Questions for review
- Other agents have pending opening/collision edits. Keep those out of this portrait commit.

# Review fixes
- C1: Apply existing ActorHeight/ActorOffset in backdrop mode; hide 3D room/camera controls there, and expose Backdrop in the monitor tool.
- C2: Anchor close-up framing at the top of the portrait, keeping the head visible. Added native pixel/size/offset/head-bound regression checks.
- First test launch aborted during the framework scene-save task; a clean retry completed 6/6. First edit-mode capture read the existing catalog lookup cache; domain reload and path checks confirmed all 11 named replacements before regenerating previews.

# Completion
- Deviations: 0; supplied PNG pixels and missing portrait IDs retained.
- Framing controls remain available in the monitor tool; close-ups preserve headroom.
- Edge cases: override import priority, transparent margins, catalog cache, neutral color, owned sprite lifetime.
- Native compilation passed; final monitor tests 6/6; exact PNG copies 11/11; all named resource paths verified.
- Read verification.json and source-manifest.json first; no player package generated.

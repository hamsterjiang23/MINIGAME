# Deviations

- Initial six GPU/EditMode checks passed in the isolated project. User then requested live main-project work through Unity CLI; all remaining validation runs in the current Editor.
- CLI auto-install rejects 2022. Reused the local 2022.3-compatible Pipeline 0.4.0-exp.1 package with its license; user refreshed once and CLI now connects to the main project. No Unity upgrade.
- The copied compatibility package contained three replaced dependency DLLs that failed its build integrity check. Downloaded official 0.4.0-exp.1 from Unity's registry, verified archive SHA-1 f49636739437acc8a1b6120b491338e352808553, and restored those DLLs to the manifest's SHA-256 values. Kept the integrity gate unchanged; CLI eval and integrity validation passed after reload.
- Player compilation exposed that the compatibility package's Roslyn plugins are Editor-only while its core assembly targeted Player too. Restricted Unity.Pipeline.asmdef to Editor, matching this project's CLI-only use. No runtime Pipeline manager is installed in the game.
- HDR colours are normalized by their peak before gamma/quantization, then restored. This prevents overflow and clipping while retaining the intended LDR treatment.

# Discovered edge cases

- Odd dimensions leave partial pixel blocks. Sample a real texel centre within the remaining block rather than relying on repeat wrapping.
- Existing screenshot helpers temporarily move Overlay UI into the camera, which would incorrectly pixelate it. New visual tests must composite UI after the processed world image.
- Scene and builder already have unrelated edits. Preserve them and patch only the effect attachment.
- Review found two camera-generation paths. Both now retain the previous profile reference before deleting the gameplay root and attach the effect to the replacement camera. The tuning window follows profile-reference changes.
- Full art reimport also replaces the scene. It now reads the previous loaded/saved profile before replacing the scene, then passes it through the temporary camera to gameplay setup.
- The new saved-scene test initially hit Unity's prohibition on additive loads while the Test Runner scene is untitled. Saving the test harness to a temporary asset, then cleaning it in finally, resolves this; final EditMode run passed 7/7.

# Questions for review

- None pending.

## Handoff

- Deviations: 4 (validation location, HDR treatment, official dependency restoration, Editor-only CLI package).
- Most likely to revisit: replace the local Pipeline compatibility integration when a supported 2022 CLI package is available.
- Edge cases: 7, including partial pixel blocks, UI capture, profile retention and the untitled test harness.
- Review: C1–C3 fixed; follow-up static review found no remaining bugs.
- Read first: ../retro-screen-guide.md and ../retro-screen-verification.json; captures and CLI evidence are in Artifacts/Unity/RetroScreen.

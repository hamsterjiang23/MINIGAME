# Deviations

- Import the 22 transparent PNGs only; keep the supplied JPG sheets as source references outside runtime assets. Eleven PNGs already match byte-for-byte and keep their existing GUID/slice metadata.
- Keep legacy story/portrait IDs ML04, ML05 and JN05 as aliases to ML03, ML03 and JN04. No old character art should return when an old save or preview uses these IDs.

# Discovered edge cases

- New ED01-03 and JN01-03 are foreground half-body drawings, not the older seated sprites. ED04 is seated; JN04 is standing. Update placements according to the supplied poses, keeping VV04's existing bed placement.
- The tutorial previously overlaid a static Day0Supervisor image on the live feed. Keep NORM as the opening logo, then use tutorial/D0_01 in the actual monitor; the doorway JD supervisor is a separate character and remains unchanged.
- D0_02/D0_03 are available in the catalog, but no expression timings are invented where the authored dialogue has no portrait metadata.
- Import in the isolated Unity project, then copy generated meta/catalog back with stable GUIDs. Do not stop or restart the shared Editor.
- A minimal new texture meta defaulted to Cubemap in this environment, so the first import had no Sprite subasset. Explicitly set both importer and texture settings to Texture2D; the retry imported all 22 images and 25 catalog IDs successfully. PNG bytes were not changed.
- The Day0 end-to-end check had two stale expectations: a sparse Closing session console was treated as a full monitor image, and the bed lamp was required on despite the user's current night profile explicitly disabling it. Keep the flow assertions, verify console pixels against its actual text, and check the lamp/beam/particles against the configured night state; do not edit the user's lighting.

# Questions for review

- Review all six roles and the Day0 logo-to-guide transition against real GPU captures. Preserve original PNG pixels and current monitor color/distortion settings.

- A final source-parity check found newer apartment assets, campaign text and Player probes in the primary worktree. Synchronize them into the isolated build project and rerun the eight scene/tutorial/monitor checks before producing the final package; keep earlier snapshot reports as history.
- The Player opening probe also had hard-coded dialogue counts and assumed every furniture collider was enabled. Derive the exact expected tutorial line IDs from the compiler, and compare only authored room/interactable collider states against their captured baseline; exclude temporary CRT/proxy colliders. The final standalone opening reaches Day1 and confirms exactly one purification, one sleep and actual collision restoration.
- Quarantine three obsolete isolated-project C# files (including the removed BedtimeCollisionGate) rather than silently compiling code absent from the primary project. Original files/meta remain under Artifacts/NamedPortraits/RetiredStaging.

## Handoff summary

Deviations: 2, legacy-ID aliases and replacing the obsolete Day0 static image with the supplied tutorial character.
Most revisitable: expression timing for D0_02/D0_03; they are available but not automatically assigned to untagged dialogue.
Edge cases: transparent RGB, imported Texture2D shape, per-pose crop/placement, preview limits and stale verification assumptions.
Validation: 32 asset/preview checks, 20 distinct flow checks after targeted retries, 22 real GPU frames, Mono build and native opening/terminal observations.
Read first: guide.md, verification.json and source-manifest.json; user visual acceptance remains pending.

# Deviations

- Unity MCP reports no connected clients and Blender MCP cannot connect. Use the running Unity 2022.3 CLI and an isolated Blender 4.5.11 background process; never save the supplied source. [verified: unity_listClients count=0; get_addon_status connection error; editor_status unityVersion]
- Preserve the three incidental Unity serialization changes by guarded baseline-document restoration after saving; the full scene verifier then passes. [verified: restore_serialization_noise.py output; reports/scene-preservation.json passed]
- Reloading through CLI timed out and the Editor exposed no native window. Backed up the delivered scene and restarted only that Editor; the recovered instance loads eight exterior renderers, valid materials and a 300 m far plane. [verified: editor_status ready; recovered eval scene/renderers/invalidMaterials/far]

# Discovered edge cases

- The new exterior is seven buildings at Blender x < -20 m plus ground. The current Unity backdrop is fifteen enabled meshes at x > 2.4 m, outside the window at x=2.15 m. Preserve every other existing object and component. [verified: source-inventory.json objects; unity-import.json hiddenBackdrop]
- The existing camera far plane is 45 m; the supplied exterior extends past 100 m. Increase only the camera far plane to 300 m when installing the exterior. [verified: Unity eval Main Camera far=45; source-inventory.json objects]
- Two unrelated source images are missing. Validate textures used by the selected exterior separately before import. [verified: source-inventory.json images]
- The worktree and main scene already contain user edits. Take a byte-for-byte scene backup and compare every existing serialized document with an explicit change allowance. [verified: git status --short; editor scene dirty=False]
- Unity saving also changes two near-zero preview-light intensities and removes two obsolete audio fields already absent from the compiled component. Restore these three documents from the baseline after verification, with exact difference preconditions; do not alter the current audio source. [verified: scene-delta.json modified fileIDs 1082089431, 1708081630, 1288090742]

# Questions for review

- None; changes stay within the requested window exterior. [opinion]

Deviations: 3. [verified: Deviations entries above]
Revisit first: Unity's serialization of existing preview-light and obsolete audio fields. [opinion]
Edge cases: exterior range, unrelated missing source images, existing worktree edits, automatic serialization. [verified: Discovered edge cases above]
Review: three script issues fixed; fresh review found no remaining bugs. [verified: review follow-up result]
Next session: read guide.md and reports/scene-preservation.json before reinstalling. [opinion]

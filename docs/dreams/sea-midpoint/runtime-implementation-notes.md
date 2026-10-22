# Deviations

- Keep the fixed doorway panorama and its anamorphic room projection. Add an isolated legacy Dream Sea mesh rendered into the panorama at the fixed capture origin; use an authored visibility mask to protect land, props and sky.

# Discovered edge cases

- The existing room ending bypasses RetroScreenEffect and captures only once. Both must change for adjustable retro and moving waves.
- RetroScreenEffect and its editor already have unrelated day/night changes. Add a transient presentation override without changing those profiles or lighting transitions.
- Only the Unity 2019 client is connected to UniMCP. Target F:/AAMINIGAME/MINIGAME/UnityProject with Unity 2022.3.23f1c1; validate in a separate snapshot when the live project is locked.
- Blender Ocean005 uses KB3D_ANT_MudM and is sand, not water. The initial object-index slice exposed this; mask only Ocean001/002/004 and retain their original depth/alpha occlusion.
- Fresh Unity Package Manager startup needed ALLUSERSPROFILE=C:/ProgramData. The live original Unity 2022 window did not refresh or provide screenshots; runtime validation uses a full production-source snapshot.
- Review fixed disabled-water dependency on missing legacy assets, isolated the retro comparison from wave-height edits, and validated the skybox before allocating hidden capture objects.

# Questions for review

- None blocking. User confirmed adjustable panel, legacy water waves, and Unity 2022.

Deviation count: 2 (masked legacy mesh over the panorama; isolated editor validation because the live window was unresponsive).
Most likely revisit: shoreline distance and sea level can be artist-tuned for the fixed photo mask.
Edge cases handled: sand named Ocean005, unset package environment, paused edits, fixed capture eye, optional static mode, retro state restoration.
Validation: actual Unity 2022 GPU capture, focused Play Mode and Edit Mode tests; no Player build or full campaign playthrough.
Read next: runtime-guide.md and Artifacts/Unity/DreamRuntime20261022.

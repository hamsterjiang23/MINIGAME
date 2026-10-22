# Deviations

- User requested an upper hemisphere sky after the first smoke images. Replace the capture-only flat sky planes and red backdrop cube with a 700 metre inward hemisphere, keeping the source scene unchanged.
- The existing environment JPG is a 1024x768 preview sheet. Map only its top sky strip onto the hemisphere so the grass and material preview balls never enter the sky.
- Review caught a superseded house template. Refresh from current MainRoomReference.blend and markers.Spawn; compensate the FBX horizontal sign flips with house rotation=capture yaw+pi.

# Discovered edge cases

- Camera and Camera2 face approximately opposite directions and are about 6.73 metres apart. Use their world-position midpoint, not either camera or the midpoint of their view targets.
- The user corrected the initial extra 1.56 metre offset. The capture eye now equals the two cameras' world-position midpoint; lower the reference floor spawn by the player's 1.56 metre eye height.
- The room is a positioning reference only and must be excluded from renders, as explicitly confirmed by the user.
- The working tree already contains unrelated lighting and terminal edits. Do not include or revert them.
- World environment nodes were disconnected from Background.Color. The original dark red world color was visible overhead; it was not a missing room light.
- GPU compute preferences were unset. GPU smoke temporarily selects CUDA and restores the preferences afterward; the original scene and user preferences are not saved.
- Actual Unity rendering caught an upside-down native Cubemap despite raw byte equality. Use PNG top-down storage and validate with six rendered views.
- Unity 2022.3 Cubemap has GetPixels rather than GetPixels32; the isolated validation converts returned Color values to Color32.
- Remove the sea source's SM_bed_1 before recapturing, and regenerate the visibility mask so the old bed silhouette cannot leave a hole in animated water. Adjust relative sea level by the eye-height delta while retaining existing artist controls.

# Questions for review

- No blocking questions. Six GPU views passed, the room spawn error is 0 metres, and the full Campaign was not run.

Deviation count: 4 (hemisphere request, sky preview UV range, current room source correction, camera-height correction).
Most likely revisit: replace the low-resolution existing sky JPG with an authored high-resolution panorama.
Edge cases: disconnected World, preview-sheet ground, unset CUDA, import axis signs, native face row convention, Unity pixel API.
Validation: Blender six-face render, Unity 2022.3 D3D11 import and GPU sampling; no full Campaign or Player build.
Read first next session: guide.md and Art/DreamEnding/SeaMidpoint/cubemap-verification.json.

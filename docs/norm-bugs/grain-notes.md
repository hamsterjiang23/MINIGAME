# Deviations

- None. Film grain is a separate scene post-process control; scene lighting and colour grading values are unchanged.

# Discovered edge cases

- `ditherStrength` only perturbs quantization thresholds with a static Bayer grid; it does not produce visible film grain while the combined scene disables quantization.
- `RetroScreenProfile.FilmGrainStrength` defaults to 0 and clamps to 0–0.15; NaN and Infinity become 0. All existing reset presets disable it. The combined scene's `RoomLightingReferenceRetro` asset starts at 0.05.
- The shader adds signed monochrome noise after grading/quantization, with a slightly stronger response in dark areas, and leaves alpha untouched. The overall effect strength still mixes the source and processed colour. A repeated render within one frame shares the noise field; a later frame changes it.
- The world camera owns this effect. ScreenSpaceOverlay terminal controls remain outside its pass; existing presentation and original-image bypasses also bypass grain.

# Questions for review

- Root coordinates GPU and real scene capture. Adjust the combined profile strength from 0.05 only after viewing the resulting scene; no worker Editor or SDK process is started.

Deviations: 0.
Most likely revisit: combined scene grain strength after a full resolution GPU capture.
Edge cases found: quantization-independent visibility, default compatibility, invalid values, alpha/mean brightness, temporal variation, and presentation bypass.
Next validation: `RetroScreenFilmGrainPlayModeTests` plus the existing retro shader tests.
Read first: profile strength field and shader FilmGrain function.

# Deviations

- Unity 2022 Built-in ignores `Light.innerSpotAngle` (official API description). A generated radial spotlight cookie implements the configured inner cone; the light still uses real attenuation and shadows.
- Initial 110°/3/5.5m lighting made the walls too dark. Fixed-view calibration widened the cone and raised indirect visibility; the saved reference profile is the calibrated source of truth.
- Full-strength 8-band lighting produced broad bullseye plateaus. Added a designer-controlled blend (default 0.4) between continuous and quantized light; retained 8 bands, zero dither and untouched albedo detail.
- User rejected the visible rings during implementation. Final reference defaults disable material banding altogether, retaining it only as an opt-in style control. Real downward spot attenuation and furniture shadows now define the image.
- User then supplied a URP reference and requested designer controls. The project stays Built-in; the existing post-effect profile gains optional three-tone grading, luminance quantization, static dither and virtual-resolution sampling. Natural lighting stays the default.

# Discovered edge cases

- The oval above the computer is the hanging shade casting a shadow from `CeilingLight`. Isolated lamp-caster and individual-light A/B renders reproduced it without the screen effect.
- `BedtimeGuidance` caches daylight lights, ambient and emission at bind. Live profile edits must update those baselines even while the room is isolated, without turning daylight back on.
- The existing floor already has herringbone UVs; `B_SM_rug_2` is bed linen, not a floor rug. Source mesh and textures stay intact.
- Per-material property blocks override renderer-wide blocks. CRT glow now updates its material slots; night restoration only restores its owned colour, retaining live material band settings.
- `Light.lightmapBakeType` is Editor-only in this Unity version. The first Windows build caught its runtime reference; bake-mode authoring now lives in Editor setup rather than runtime application.
- The floor had a normal texture but no `_NORMALMAP` keyword. Setup now activates the existing normal-map variant without replacing source UVs or textures.
- CLI array arguments were accepted without applying the intended Development option. The resulting release Player omitted the development-only probe. Verification build now uses explicit `BuildPlayerOptions` through CLI eval and records its flags.
- Build verification left enabled build scenes loaded additively in the Editor. Unmodified extra scenes are closed before the fixed-room capture; actual Player verification loads Combined alone.

# Questions for review

- None. Fixed-view screenshots and Player verification will be recorded separately from compilation and automated tests.

## Handoff summary

Deviations: 5, including the user-directed natural-light default and later optional style controls.
Most likely to revisit: the reference grading preset is an editable starting point, not user visual acceptance.
Edge cases: 8, including Built-in cone control, night baselines, slot blocks, normal keywords, Player-only compilation and CLI build behaviour.
Validation: see verification.json for individual Editor/PlayMode results, configuration persistence, image captures and actual Windows Player evidence.
Read first: guide.md for room lighting, style-guide.md for grading/pixel controls, then verification.json for scope and results.

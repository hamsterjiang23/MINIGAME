# Deviations

- Preserve the terminal's existing cell advances and line pitch while replacing the glyph pixels with official Ark 12px monospaced simplified Chinese pixels. `[opinion]`
- Current code removed F3 history. The user explicitly selected both live and F3 record areas, so restore a read-only F3 viewer without altering RuntimeView transcript state. `[verified: TerminalView.HistoryOpen; user clarification]`

# Discovered edge cases

- Changing Text.font alone does not replace TerminalBitmapText's custom atlas rendering. `[verified: TerminalBitmapText.OnPopulateMesh]`
- Keep the user's pre-existing RoomLightingDay.asset adjustment untouched. `[verified: initial git status]`

# Questions for review

- Keep all new text distortion limited to transcript glyph meshes; titles, controls, metrics, boot text and world UI stay stable. `[verified: user request and clarification]`

- The transient shadow/highlight lighting request was withdrawn by the user as a wrong-chat message. Removed only this task's temporary lighting hunks; concurrent ScreenStyle/NightBlend changes remain untouched. `[verified: user correction; scoped git diff]`

- Latest steering changes the default to sparse random vertical glyph displacement: HorizontalPixels=0, VerticalPixels=2.5 (about 2px at default .65 intensity). The serialized settings asset is explicitly migrated. `[verified: TerminalDialogueTextDefault.asset]`
- Preview mesh checks temporarily use Seed=123 and restore production seeds, avoiding statistical same-direction samples. `[verified: TerminalHtmlParityTests.AssertGlyphEffectIsScoped]`
- Atlas reinstall invalidates glyph dictionaries and live meshes; generated-atlas coverage checks reject stale glyph/size pairs. `[verified: TerminalPixelFont.InvalidateMetrics; TerminalTypographySetup.Install; build_ark_atlas.py]`

# Completion

- Deviations: Ark pixels retain existing UI cell metrics; 30 missing corpus characters use same-size Fusion pixels; F3 read-only viewer restored after user confirmation. `[verified: verification.json font/scope]`
- Most likely to revisit: artistic displacement amount and burst cadence. `[opinion]`
- Edge cases: serialized horizontal defaults, deterministic tests, atlas hot-rebuild cache, stale glyph coverage, and wrong-chat lighting request handled. `[verified: implementation notes above; verification.json review/withdrawnLightingRequest]`
- Validation: 35 passing tests, 16 native frames with no changed pixels outside dialogue regions, two inspected UI captures, review CLEAN. `[verified: verification.json tests/motion/review]`
- Start with README.md, verification.json and TerminalDialogueTextDefault.asset for further tuning. `[opinion]`

# Dialogue glyph audit — 2026-10-23

- Reproduced missing glyphs before rebuilding: the direct Resources ending catalog contained 21 characters absent from the atlas. The old coverage check only included supervisor Resources, so it did not catch these missing ending characters. `[verified: Artifacts/DialogueGlyphAudit20261023/before.json missingOrSuspiciousSources; baseline-failure.json results]`
- Reproduced entirely blank glyphs after nearest-neighbour reduction: `! ' . : T |` at 10px and `- – — 一` at 11px. Coverage now checks actual alpha pixels as well as metrics; downsampling keeps covered strokes in a binary mask. Native 12px and larger sampling is unchanged. `[verified: before.json blankGlyphs; Tools/TerminalTypography/build_ark_atlas.py resize_glyph/validate_generated; test_build_ark_atlas.py]`
- Resource JSON/Yarn are included recursively. Regenerated atlas covers 1,452 characters at seven sizes. Final strict UTF-8/source and glyph-pixel audit found no missing glyphs, blank glyphs or pixel mismatches; active imported campaign bytes match the authoring package. `[verified: docs/terminal-font/coverage.json corpusCharacters/missing; Artifacts/DialogueGlyphAudit20261023/after.json]`
- Added an Ark atlas/metrics import callback that invalidates cached UV metrics and dirties loaded labels. A forced metrics reimport produced a fresh glyph object with visible dimensions; the imported texture remains 2048×2048. `[verified: TerminalPixelFontRefresh.OnPostprocessAllAssets; Artifacts/DialogueGlyphAudit20261023/reimport-check.json result]`
- Validation: 35 EditMode and nine PlayMode tests passed. Inspected the live terminal captures containing 您、蚕、血、蓝、澈. Windows build succeeded with zero errors; its one warning disables the editor automation Pipeline in Player. Standalone Player interaction is not part of this capture check. `[verified: Artifacts/DialogueGlyphAudit20261023/*Tests.json summary; Visuals/ending-glyphs-1.png; Visuals/ending-glyphs-2.png; windows-build.txt; windows-build-warnings.txt]`
- The broader dialogue compiler suite exposed pre-existing ending metadata drift: shared conversion config is 1.2.1-development while the ending resource is 1.2.0-development. The generated package differs only in contentVersion/revision and the provenance only in revision; Yarn is identical. Kept the existing ending catalog and save revision during this font repair. `[verified: Tools/DialogueCampaign/conversion-config.json contentVersion; Resources/MiniGame/Endings/Dialogue.json contentVersion; comparison recorded in Artifacts/DialogueGlyphAudit20261023/source-tests.json]`

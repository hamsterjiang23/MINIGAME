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

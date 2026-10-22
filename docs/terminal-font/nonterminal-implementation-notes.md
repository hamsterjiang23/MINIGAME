# Deviations

- User clarified: match the Continue Inspection font face, keep all original sizes. Select one installed face for nonterminal UI instead of passing a mixed-family candidate array into each Font. `[verified: user clarification; GameUiTypography.Family]`

# Discovered edge cases

- Existing pause title and action text already receive one Font but use sizes40/28. Keep these sizes; unify the font selection policy across the main UI and the ending UI. `[verified: PauseMenuView.Build/MakeText; MiniGameScreen.EnsureBuilt; DreamEndingController.BuildInterface]`
- Font instances remain owned separately because both owners destroy their font during teardown. TerminalBitmapText retains the Ark atlas. `[verified: MiniGameScreen.OnDestroy; DreamEndingController.OnDestroy; TerminalBitmapText.ArkTypeface]`

# Questions for review

- Push resumed with the named-portrait replacement request. Preserve concurrent edits from other agents. `[verified: user pause instruction; initial git status]`

# Verification

- Native Unity selected SimSun as the single family; all 26 pause/settings/manual labels share the reference Font. Title/action sizes remain 40/28. Compilation completed without errors; PauseMenuPlayModeTests passed 7/7. See nonterminal-verification.json. No player build or push.

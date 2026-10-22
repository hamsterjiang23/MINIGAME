# Deviations

- None at implementation start. The accepted plan adds a title/menu overlay, a separate automatic continuation record, shared local settings, and resumable sleep timing.

# Discovered edge cases

- The existing menu-music helper stops all audio. Menu suspension must preserve gameplay sources and their sample positions instead.
- The current bootstrap begins Day0 in Awake. First launch must defer runtime creation, while explicit test/development start methods remain usable.
- The active automatic continuation record takes precedence over manual saves; a broken active record must not silently fall back to an older inspection period.
- New inspection periods publish an atomic paused Day0 seed before unloading the prior scene. Failed writes leave the previous period intact; cold loads pause before binding presenters.
- The menu controller survives scene reloads, so each newly attached room must ensure an EventSystem exists. The UI is covered without disabling directors or resetting their captured camera poses.
- Room-ending takeover used to disable every room behaviour except retro; the independent player brightness effect is now preserved too.
- Existing development/test entry points can use `-minigameDirectPlay`; new main-menu tests explicitly exercise normal startup and isolated continuation files.
- The user designated the existing Continue Inspection label as the typography baseline. Menu/settings/confirmation buttons and titles use its 28-point normal style and the same GameUiTypography font factory; small supporting text retains the same family.

# Questions for review

- No outstanding implementation questions. Six title/settings captures were inspected at 1080p and 1440p. Standalone executable build/relaunch and independent listening are outside this run's verification.

# Handoff

- Deviations: no product-scope deviations; explicit direct-play argument retained for existing development automation.
- Most likely to revisit: auxiliary text sizes if the user wants them identical to the 28-point action/title style as well as the same font family.
- Edge cases: audio sample retention, atomic new-period seed, invalid active continuation priority, sleep phase timing, reload EventSystem, brightness during room-ending takeover, and startup input consumption.
- Validation: 39 unique PlayMode tests passed; current source hashes and six reviewed screenshots are recorded in verification.json.
- Read first: guide.md for behavior and verification.json for exact test evidence and limits.

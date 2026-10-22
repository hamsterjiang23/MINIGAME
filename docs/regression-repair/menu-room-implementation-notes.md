# Deviations

- Sleep means a 1.5-second fully black hold after the existing bed movement, upward look and fade; preserve the established dream opening and film durations.
- Menu entry reuses the existing 2.2-second terminal-camera pullback. The title action now starts it automatically; direct-play development entry retains its existing opening interaction.
- The roster regression now meets the production daily purification quota. Vera/Edwin retain treatment history while the purified child/priest leave the following day's directory; the test can reach later arrivals without triggering the quota ending.

# Discovered edge cases

- The family letter's visible mesh was moved independently of its parent collider. Repair the collider to the visible paper and test actual focus/left-click reading.
- Keep existing dirty worktree changes; starting runtime, tests and combined scene are copied under `Artifacts/MenuRoomRepair20261023/before`.
- Attach readable collision to the visible paper child so future mesh movement cannot separate targeting. The newspaper's old approach marker was over six metres away; move only the test marker to the current paper.
- Current ending content requires a revision in generated test saves. Configure the real catalog in campaign fixture factories; preserve production save validation.
- A concurrent ending test called an internal audio-button helper from the test assembly; use its existing public `ActivateWithAudio` entry so the project compiles.
- Returning to the menu must suspend gameplay-loop playback without clearing its requested clip when pause notifications arrive. The separate title track still stops before gameplay.
- Menu startup ends at the Introduction cue, whose first Yarn line arrives asynchronously. Tests now await that real line rather than assuming the cue is already null.
- Unity's scene loader crashed during overlapping work on the shared Editor. Preserve the native stack, restart only this project's Editor, wait for full Play Mode teardown between suites, and use the shared local handoff file for serialized ownership.
- First restart exposed the Windows package-manager environment issue; the recovered Editor was launched with `ALLUSERSPROFILE=C:\ProgramData`.

# Questions for review

- None.

# Completion

- Completed all five requested changes: menu fade/pullback, neutral initial button selection, letter targeting, separate terminal stage/status/unit, and 1.5-second full black sleep hold.
- Final regression results: 81 passed, 0 failed across ten suites. Seven final suites ran in a physically copied Unity project; all ten production repair files match the main project by SHA-256.
- Checked captured menu midpoint/pullback/gameplay frames, letter reading, and terminal stage layout. These captures support the tested states, not a standalone Player build claim.
- Spatial audio output test now creates its own artifact directory; near peak 0.4213514 versus far peak 0.02290129. Font coverage is 10003 glyph/size pairs with no missing pairs; git diff --check passed.
- Restarted the main Unity 2022.3.23f1c1 Editor after its native crash. Main Editor ownership remains with the concurrent endings task; the repair verification used a separate Editor and did not interrupt it.

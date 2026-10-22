# Date and phase debug tool

## Deviations

- None from the requested Day0–Day3 editor jump tool. Each target uses a new isolated checkpoint; earlier choices use treatment where allowed and the lowest rebellion-delta response. The panel states this prerequisite policy.

## Discovered edge cases

- Day0 has no ordinary dream. Expose its opening, room, terminal, shutdown, bedtime and first sleep only.
- Daily dreams keep the same date until the second bed interaction. The target predicate uses both date and presentation/core stage.
- Manual F5 saves and main-menu continuation must use the debug directory while this session is active. The override is editor-only and is cleared when Play Mode ends.
- Concurrent menu/audio work briefly had unresolved merge markers. They were resolved in the shared worktree before applying the debug save-path changes; the tool does not alter that audio merge.
- The main-menu resume coroutine could unpause the newly restored target or keep input blocked. The editor-only dismissal cancels it before jumping; the live test includes this overlap.
- In-Play script reload leaves serialized UI objects with missing constructed view/runtime objects. The jump detects this and restarts Play Mode once, retaining the requested target and isolated save scope.
- The isolated Unity test project's package paths initially resolved to stale copied core sources. Both source packages were refreshed before the passing test run; only editor code was changed after that baseline.

## Validation

- Edit Mode: 9 tests cover all 30 supported checkpoints, next-action acceptance, invalid targets, Day0 first sleep and save routing. See verification.json for the final result and raw XML location.
- Live editor: 8 successive jumps pass, including menu continuation overlap and paused dream arrival. Manual saves land in the debug directory, formal save hashes stay unchanged, and stopping Play clears the override. Raw result: Artifacts/DayDebug/live-verification.json.
- Code review: the menu coroutine issue was addressed. No player build or Git push was requested for this tool.

## Questions for review

- None at present.

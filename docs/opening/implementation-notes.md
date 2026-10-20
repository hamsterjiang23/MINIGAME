# Day0 opening implementation notes

## Deviations

- The supplied supervisor reference is used unchanged as the Day0 feed texture. It is a still image; later patients retain their dynamic room feeds. The camera pulls back within the native UI composition so the same pixels remain continuous.

## Discovered edge cases

- Day0 already skips the wall dream: BeginNightDream moves directly to AfterDream, and one bed interaction advances to Day1. Preserve that authoritative path.
- The opening uses the existing EnterTerminal cue on Day0 Review. Restoring that unfinished cue restarts the monitor close view; running tutorial checkpoints retain the current line.
- The footer duplicates dialogue confirmation in normal play. During Day0 it must be passive so only Continue or the sole Purify action is clickable.
- Day0 keeps CharacterController collision active, unlike later dream traversal. Before restoring furniture on Day1, move the player to the authored wake pose so sleeping inside a disabled furniture volume cannot trap them.
- Day0 sleep uses the existing Sleep cue while core remains AfterDream; completion performs the bed command once. This preserves the eye-close fade and replayable checkpoints without introducing a second sleep.
- Physical-room regression fixtures must explicitly restore an untouched world checkpoint using the current session rules; fresh games now start inside the terminal, and dream-enabled rules differ from the raw imported package.
- Graphics.DrawTexture defaults to a half-strength vertex tint. The Player recorder now supplies white explicitly so recorded CRT pixels match the actual RawImage display; movie durations use measured frame timestamps.
- Smoothness follow-up: the original synchronous PNG recorder yielded about 7.45fps. It no longer captures during motion; a separate opt-in asynchronous GPU recorder stores preallocated frames and encodes after movement.
- The 60fps recorder alone temporarily sets targetFrameRate=60/vSync=0 and restores them on cleanup. Timing-only hidden batch runs remain uncapped and cannot establish visible presentation FPS.
- Cached source layers need an explicit dirty mark for equal-sized reply replacements; new native panels/text also inherit their parent's layer. The GPU reply-replacement and pointer check covers this path.
- Closing-session follow-up: the console previously inherited middle-left alignment across a tall text box. Fixed x28/y24 rows with 20px pitch now match HTML, and delayed-frame advancement stops at each output boundary before continuing.

## Questions for review

- None blocking the authorized implementation.

- Deviations: 1, the supplied still supervisor reference is used directly.
- Most revisitable: replace that still with dedicated supervisor animation when supplied.
- Edge cases: 11, including opening replay, safe collision restoration, single-sleep checkpoints, asynchronous capture, equal-sized UI replacement and closing-console alignment/visibility.
- Validation: closing follow-up 297 .NET/5 affected PlayMode checks, Mono build and standalone Day0 operation passed; Closing text was visible in all 50 sampled Closing frames. Earlier full campaign/smoothness checks remain in their dated reports.
- Read first: guide.md and verification.json; raw diagnostic runs remain in Artifacts/Opening.

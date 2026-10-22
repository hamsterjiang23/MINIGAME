# Day0 opening implementation notes

## Deviations

- The supplied supervisor reference is used unchanged as the Day0 feed texture. It is a still image; later patients retain their dynamic room feeds. The camera pulls back within the native UI composition so the same pixels remain continuous.

## Discovered edge cases

- Day0 already skips the wall dream: BeginNightDream moves directly to AfterDream, and one bed interaction advances to Day1. Preserve that authoritative path.
- The opening uses the existing EnterTerminal cue on Day0 Review. Restoring that unfinished cue restarts the monitor close view; running tutorial checkpoints retain the current line.
- The footer duplicates dialogue confirmation in normal play. During Day0 it must be passive so only Continue or the sole Purify action is clickable.
- Historical Day0 furniture suppression was removed on 2026-10-22 per the user bug list; authored collider states now remain unchanged. The single sleep still moves the player to the safe Day1 wake position.
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

## 2026-10-22 Day0退场位置与碰撞

### Deviations

- 按用户第42、43条反馈，撤销Day0专用家具碰撞抑制：不改变物件原有enabled/isTrigger；起床定位保留在BedtimeGuidance的Day0→Day1状态切换中。`[verified: BedtimeGuidance.SetTargets；本轮删除BedtimeCollisionGate]`

### Discovered edge cases

- Day0自动开场未经过玩家走到电脑的过程，旧缓存直接保存出生点，距Computer approach约2.9575米；改为先使用已有电脑接近锚点，再捕获正常站立返回姿态。Day1及以后仍保留实际进入位置。`[verified: reports/day0-return-baseline.xml FreshOpeningCachesTheComputerStandingReturnPose；PresentationEffects.RememberRoomPose]`
- 原碰撞关闭导致B_HT01 collider从enabled=true变成false；新回归同时覆盖床交互、单次睡眠、暂停、读档及原本禁用/触发的碰撞体。`[verified: reports/day0-return-baseline.xml MainRoomDayZeroKeepsFurnitureSolidAndBedTouchableThroughOneSleep；DayZeroCollisionPlayModeTests]`

### Questions for review

- 本轮不修改核心日程/处置规则；仅调整Unity房间演出。`[opinion]`

变更偏差：1项，撤销旧版Day0家具碰撞关闭要求。`[verified: day0-return-verification.json changes]`
最可能回看：若移动电脑接近锚点，需同步检查退场位置是否仍可站立。`[opinion]`
发现边界：自动开场没有实际步行接近电脑；暂停/读档不能改变原本禁用或触发的碰撞体。`[verified: day0-return-verification.json tests]`
验证：目标7项PlayMode通过；额外3项灯光/像素截图断言在旧Runtime对照中同样失败，不归入本次修复；未构建原生Player。`[verified: day0-return-verification.json tests]`
后续先读：day0-return-verification.json、guide.md及PresentationEffects.RememberRoomPose。`[opinion]`

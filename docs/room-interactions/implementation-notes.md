# Deviations

- The Unity global asset defaults to locking after dialogue; the reusable GameRuntime API keeps its historical unlocked default until the host applies its global policy. This preserves non-Unity consumers and older unit fixtures.
- Policy is not a game-save field. The host reapplies the current asset when installing/restoring a runtime and when the asset changes in Play Mode.

# Discovered edge cases

- Locking must only block opening a closed door. Existing outside-camera return and door-close sequences remain available so completing dialogue or toggling the setting cannot strand the camera outside.
- Player focus previously bypassed the runtime's allowed interactions whenever HingedDoor.EnableToggle was true. Focus now respects the authoritative allowed list and labels a locked door.
- Day0 has no completed supervisor visit; each later MorningInspection naturally permits the required first opening without maintaining a duplicate daily completion flag.

# Questions for review

- None. User explicitly selected the global default “监督员对话结束后锁门”.

# Completion summary

- Global Unity default is enabled; reusable runtime callers retain their existing default until the host applies the policy.
- Revisit the non-serialized policy only if a future product requirement makes it a per-save player preference.
- Covered outside-camera return, repeated opening, Day0, next morning, live toggles, cached views and save restoration.
- Locked restore and Release build passed on SDK 8.0.424; .NET tests passed 343/343 and Unity Play Mode passed 13/13 in a matching isolated snapshot.
- Read guide.md and verification.json first; the primary editor was occupied by a separate validation and was left running.

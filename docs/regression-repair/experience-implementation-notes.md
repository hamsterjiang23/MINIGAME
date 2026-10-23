# Deviations

- Fresh starts and terminal resumes use the 0.45-second controls fade plus 2.2-second pullback. Saved world/dream scenes keep their saved camera and use a 0.2-second reveal after the controls fade.
- The serialized Thanks enum remains readable for existing checkpoints; the host consumes it immediately without creating a thanks page. Incompatible dialogue/policy versions still reject restoration.
- The authored bed pose follows the mattress-to-pillow axis, approximately a clockwise quarter turn from the old approach framing; it does not preserve an arbitrary approach yaw.

# Discovered edge cases

- Main audio merge was already active when this task began. After the merge task handed back the main Editor and audio ownership at f31b26ce, this task appended EndNote and integrated the transition/ending paths. This task does not stage, commit or push. Coordination state is in Artifacts/ExperienceRevision20261023/coordination.json.
- Dialogue sourceDay changes require content version 1.2.1-development; hidden-ending policy fingerprint advances to 3. Old incompatible saves remain on disk and are rejected explicitly.
- PlayerRebellion.asset contained a zero-delta priest source-Day3 option that is no longer scheduled. It still failed strict production rule validation, so that obsolete entry was removed rather than remapped or silently filtered. Production-profile consistency and rejection of unscheduled zero-delta options are now covered by Editor tests.
- Hidden-space music belongs to DreamEndingController.Audio, not the disabled room audio service. Continue transitions EndingMusic and Ocean on that owner while leaving Voice paused until input resumes.
- The authored EndingMusic clip is optional and currently empty; production uses Ocean. The native Continue regression separately checks that production case and a cloned two-track configuration without modifying the authored soundbank.
- The supplied doctor contact sheet labels the seated drawing ED01, but the seated source PNG is ED04. Placement changes preserve the table/PNG ID.
- The initial Player opening reached the tutorial but still spent about 130 ms inside the start action. Opt-in stage timings located repeated content decoding, seed serialization, lighting-tool construction and bedtime presentation setup. The title now prepares an unbound paused seed, its serialized checkpoint, Yarn runner, initial terminal render, lighting controls and hidden bedtime resources. It neither installs an active session nor writes a save before Start.
- New inspection atomically saves the prepared checkpoint through RuntimeFileSaveStore.SaveSerialized, then binds the existing room. Content/dream configuration invalidates the prepared seed. Returning to the menu prepares the next seed without replacing the retained runtime.
- Lighting controls initially missed their cache because construction selects the first light while the reuse check required an empty selection. Reuse now validates the selected light and the ordered light list. Hidden controls are not refreshed during Start; opening the tool rebuilds and refreshes them. Tests cover selection retention and invalidation when lights change.
- ScreenCapture on a hidden Windows window produced black frames. Early captures are diagnostic only. Final captures use a visible Player, WaitForEndOfFrame and asynchronous GPU readback with the platform Y orientation corrected. Empty images now fail the capture check. PNG encoding and disk writes occur after the first tutorial line.
- Final opening runs use actual 1920x1080 and 2560x1440 windows with 1280x720 asynchronous recordings paced at 60 fps. Start actions measured 33.53/37.32 ms, P95 frames 17.56/18.04 ms, maxima 43.40/43.73 ms, and zero frames over 50 ms. The production target-frame-rate setting is unchanged. Evidence: Artifacts/ExperienceRevision20261023/player-summary.json and Artifacts/Opening/Smooth/experience-delivery*/verification.json.
- Both recordings have one unchanged logo rectangle throughout the controls fade, zero empty frames/readback errors, and a completed 2.65-second transition into the first tutorial line. Videos preserve the recorded timestamps and contain no audio; audio lifecycle is checked separately in Unity tests.
- The completed-hidden-save regression now also starts a new inspection from that menu and checks the same scene/bootstrap, a new Day0 runtime, restored room camera/UI, removal of the hidden-room owner and no second EndNote.

# Questions for review

- None.

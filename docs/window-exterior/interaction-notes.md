# Deviations

- Use an invisible room-facing quad for focus and hit detection. The original glass has a thin X-axis cube mesh, whereas the shared readable outline expects an XY surface. Original glass and frame are not modified. [verified: ReadableProp.cs RebuildOutline; Unity eval window localMeshBounds/scale]

# Discovered edge cases

- Window text is static across days and uses the existing world-input, distance and dream-traversal gates. [verified: WindowObservation.asset changesWithDay; ApartmentPlayer.cs CanRead/RefreshFocus]
- Other work is updating the shared interaction tests; add a separate window regression and preserve those edits. [verified: InteractionDistancePlayModeTests.cs current per-object loop]
- RestoreCheckpoint intentionally blocks input during the restoration frame. The regression yields one frame before testing the first E interaction. [verified: first test failure at input assertion; second run passed]
- Startup clears Edit Mode static fields. Runtime capture sets SkipStartup in Play Mode and loads the real campaign before staging the window. [verified: first capture session was null; subsequent capture opened=True/visible=True]
- Package Manager needs ALLUSERSPROFILE in the Editor process environment. Starting this Editor with the existing ProgramData path fixes package_list; no registry or dependency changes are needed. [verified: app.js getGlobalConfigRoot; package-list.json success=true]

# Questions for review

- None; use the screenshot's sentence verbatim. [verified: user screenshot interaction text]

Deviations: one invisible interaction surface. [verified: Deviations above]
Revisit first: the generic readable outline's XY geometry assumption. [opinion]
Edge cases: restore-frame input, startup domain reload and UPM environment. [verified: Discovered edge cases above]
Review: no bugs found; real-scene PlayMode and UI capture passed. [verified: review result; window-interaction-playmode.json; window-dialogue.png]
Next session: use WindowObservation.asset for text edits and coordinate Editor ownership with the linked repair chat. [opinion]

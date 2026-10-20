# Deviations

- New night flow is opt-in for the formal Combined campaign; legacy prototype behavior remains available.
- Tripo is authorized but Chrome is not exposed by the browser connection. Blender 4.5.11 is available and can produce the required scene without an external generation dependency.
- Daily films use the supplied illustrations with real-time Unity motion and a dedicated RenderTexture camera. They are animated illustrated plates, not three fully modeled Blender scenes; Blender supplies the editable beach terrain. Replacing the plate renderer with modeled stages can preserve the same director and configuration contract.
- Evidence videos use fixed-step rendering of the production camera and postmixed configured audio. They demonstrate presentation sequencing, not live keyboard input, real-time performance or an independent listening review.

# Discovered edge cases

- The October 19 DOCX supersedes the older dream ordering: Day1 whale beach, Day2 endless road, Day3 restrained judgment. Preserve the unused flower-field asset.
- Current room-wall meshes have imported world-space vertices and a shared pivot; use the selected wall bounds and authored inside-facing pose rather than Transform.forward alone.
- Player rebellion must be global, not patient trust. Rule changes receive a new rules identity so saved command history cannot silently replay under changed scoring.
- Camera ownership must exclude normal player look, reference projection and terminal transitions while the dream director holds the camera.
- Restore cancels all outgoing seating/door/dream owners before applying the saved standing and camera pose. Cross-scene visual DTOs are accepted only for their matching authoritative core phase.
- Unity JsonUtility can rehydrate null nested visual DTOs as default objects; phase-based filtering prevents an ordinary room save from acquiring a phantom beach checkpoint.
- Ending saves also retain the continuous world clock so clouds and sea motion do not jump when restoring a walk or drift checkpoint.
- User clarified during implementation that wall animation must bypass the existing retro screen shader. Use a transient presentation bypass, preserving the designer's profile and manual original-image comparison flag; restore it when playback ends or is interrupted.
- Live room capture showed the hanging lamp and furniture occluding the wall film. The independent projection surface now draws above foreground geometry within its authored rectangle; it remains disabled outside playback.
- The imported Blender terrain rose toward the intended sea, hiding the ocean. The beach builder now measures imported height versus Z and corrects the terrain orientation before saving the scene.
- Loading an outside-door save must discard any outgoing return-pose capture before applying the saved envelope pose; runtime-only rebinds retain their existing capture behavior.

# Questions for review

- Final subjective pacing and sound balance remain for designer review of the supplied videos; no independent listening approval is claimed.

# Beach wave revision

- The user rejected the rigid beach appearance and requested visibly moving waves, then authorized consulting public shader implementations. Preserve the earlier recording as the before version.
- Reference Gerstner vertex displacement and shallow-water noise foam, with shared sea/sand time and shoreline functions. Keep existing bed, sky and ending controls.
- Capture from a stationary production camera to separate wave motion from camera travel. Runtime time remains pauseable and saved; Editor material time may animate independently outside play.
- First GPU pass exposed faint foam and distant horizontal bands. A second pass treats foam as white coverage and filters distant geometric normals; the 12-second fixed-camera recording and water-verification.json preserve the result and technical checks.

# Beach scene opening

- Pausing Unity still leaves Play Mode active, so disabling the scene button trapped designers without an explanation. The window now offers "退出播放并打开海滩" and queues opening until Edit Mode, retaining the request across assembly reloads with SessionState. The shortcut is also visible in the top toolbar.

# Handoff

- Deviations: three production/evidence choices, plus the Combined-only opt-in compatibility boundary.
- Most likely to revisit: replace illustrated dream plates with fully modeled stages if the art direction calls for it.
- Edge cases: eleven camera, persistence, projection, source and coordinate issues recorded above.
- Validation: 293 .NET tests, 49 unique PlayMode tests, three Editor tests; Windows build and startup succeeded.
- Read first: guide.md for configuration, verification.json for evidence scope, then this file before changing presentation ownership.

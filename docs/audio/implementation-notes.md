# Deviations

- The source game's sound-bank, named-source, Play/PlayOneShot, fade and mixer architecture is reimplemented with native Unity 2022 components and frame updates. No dependency on its Zenject, DOTween or UniTask audio code is introduced.
- Reuse the 12 fully downloaded repository WAVs unchanged, rather than replacing them with lossy web previews. Source identity is by the supplied design mapping, not a verified byte match to remote originals.
- Combined currently starts in the room; StartMenu has a configured clip and explicit entry point but no invented menu. Missing ending music and Day3 dream music remain unassigned.
- Scan plays when entering a live review or opening an existing inspection; the lamp beep follows the actual AwaitingDisposition lamp transition. This maps the provided detector sounds onto current game events, not onto invented inspection rules.

# Discovered edge cases

- Runtime replacement must unsubscribe the old view, stop transient audio, restore loops, and seed transcript keys so checkpoint loading does not replay dialogue/scan/lamp cues.
- The terminal Continue button clears its listeners repeatedly; audio belongs in the button activation path, including keyboard activation.
- Overlapping fades need cancellation per channel; a stale fade-out must not stop a newly requested loop. Music and computer noise have separate sources.
- The recovered pause controller does not globally pause audio. Pause keeps background audio running while gameplay commands remain gated; the service still exposes explicit per-channel pause/resume.
- The real saved scene exposed a startup-order gap: Bootstrap can bind before a serialized audio service's first OnEnable. Reconcile loops in Start after scene enable callbacks, with a saved-Combined-scene regression.
- A cleared music slot must resume when a new file is assigned without waiting for another game command. The director detects changes to loop clip references and reconciles only those edits.

# Questions for review

- Final music/SFX loudness and the short Projector sample's repeated-loop character require listening in the actual room.

## Handoff

- Deviations: four documented adaptations to current content, dependencies and asset availability.
- Most likely to revisit: scan timing when the design gains a dedicated detector animation.
- Edge cases handled: startup order, interrupted fades, restored transcripts, duplicate keyboard delivery, clearing/replacing clips and persistent Play Mode authoring.
- Validation: see verification.json for exact Editor, PlayMode, real audio-output and Windows build scope.
- Read first: ../audio-guide.md for operation and ../../Art/Audio/source-manifest.json for source attribution.

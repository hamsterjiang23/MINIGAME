# Deviations
- None. Adjust VV04 composition in the game; preserve the original character and ward image pixels.

# Discovered edge cases
- A room-wide move would also move standing expressions. Add optional per-portrait size and viewport anchor settings; VV04 alone uses the bed anchor.
- Main Editor automation timed out. Validate the exact runtime sources and shaders in an isolated Unity 2022.3 probe; do not restart the shared Editor.

# Questions for review
- Check the native rendered seat contact against the bed edge. No push or player build requested in this turn.

# Verification
- Isolated Unity 2022.3 editor compiled the exact three current runtime source files and current monitor shaders, then rendered VV04 on the original white ward plate.
- Native assertions passed for seat anchor (.625,.15), size multiplier .74 and fallback anchor (.42,.01) when changing sprite. See verification.json.
- Reviewed source changes: CLEAN. Main-project PlayMode suite not rerun because shared Editor automation timed out. No player build or push this turn.

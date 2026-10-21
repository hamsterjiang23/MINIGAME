# Deviations

- The supplied package covers the HTML sample, while Campaign contains additional characters. Extend the atlas with the same Canvas2D rules, retaining all supplied glyph pixels and metrics.
- Preserve the existing passive dialogue history, formal GameRuntime commands, daily power timeline and shutdown collapse. The package demo is not the campaign authority.

# Discovered edge cases

- The previous terminal rendered at 1600×1200 with bilinear filtering, which changes the supplied 800×600 font edges. Use the actual native buffer and Point filtering.
- The host assembly cannot directly reference default Assembly-CSharp types. The vendor runtime gets a dedicated asmdef; host/test references are explicit.
- Per-character widths, line measurement and rendering must use the same fixed cells. Keep the existing Text API through a bitmap subclass so runtime callbacks and UI tests remain valid.
- Dialogue paging previously used TextGenerator while bitmap text uses fixed cells. The bitmap branch now measures actual glyph height and line pitch, preserves text elements and CRLF, and keeps the ordinary Text path unchanged.
- Full passive history uses a ScrollRect. Regression checks now verify all saved records and the newest row in viewport coordinates, rather than assuming five fixed rows.
- Regenerating an atlas repacks UVs. Clear the cached font index and dirty live labels when the metrics/atlas are imported, or the existing editor could display old coordinates against a new image.
- Four original generator inputs are tracked under Art/TerminalHtmlMatched/Source so a clone can extend the atlas without restoring ignored Artifacts. No Windows font binaries are included.

# Questions for review

- None. Font coverage, interactive state rendering, existing campaign UI and Player captures are verified separately.

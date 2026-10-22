# Deviations

The 2026-10 source revision uses explicit per-line IDs, comma-separated choices and trust conditions. The user approved retaining earlier complete branches and repairing missing doctor links from matching old dialogue. Repairs and inactive rows are recorded in source-repairs.json.

# Discovered edge cases

- Some XLSX cells omit cached values and store literal array formulas. The converter fills only missing cells from constant arrays, never evaluates formulas; provenance keeps both stored and reconstructed fields.
- Legacy shared doctor/diplomat END rows remain supported by the old-format converter.
- Doctor and diplomat decision preambles have no ordinary choice. They are attached to the preceding answer branches, so their text plays before the disposition gate without fabricating a player answer.
- New workbooks declare worksheet dimension A1 despite containing hundreds of rows; parse real cells and detect the ID header rather than trusting dimensions.
- The new doctor's Day3 IDs reuse one round label. Restored choice groups follow old matching text and branch boundaries, not raw row adjacency.
- A doctor's -5 answer immediately leads to a +5 line; compile one net-zero response and retain both authored values. No branch condition occurs between them.
- Nine optional special rounds use the existing Core SpecialRound threshold. Explicit release conditions override the meeting default without merging patient trust and player rebellion.
- The dream-flow rebellion asset referenced an old priest option ID. It is remapped by exact same text to the new option, retaining delta 0.
- Updated dialogue requires rebuilding the bitmap glyph atlas; preserving old font pixels does not prove new character coverage.
- Native Yarn removes empty nodes. Emit `<<stop>>` for silent response segments and verify all mapped node names after native import, before activating the candidate.
- The new source declares fewer portraits. The shared catalog must include both current campaign and retained V1 demo requirements so ML04/ML05 remain available to the demo.

# Questions for review

No unresolved source-repair decision remains. Human acceptance of the source-authorized branch selection is separate from technical validation.

Source adaptations: 115 recorded repairs; 142 inactive rows remain auditable out of 1437 originals.
Most revisitable: author-supplied corrected doctor/diplomat workbooks can replace the pinned repair manifest.
Edge cases: header drift, duplicate IDs, absent links, optional rounds, per-line deltas, dependent IDs and glyph coverage.
Next read: README.md, source-repairs.json, then docs/dialogue-update-2026-10-21/verification.json.
Historical baseline: 994 rows/16 meetings, Python3 and authoring tests30; current results are recorded in the dated update report.

Review fix: trust bounds are now checked after every ordinary choice as well as after disposition. A later opposite delta cannot hide an earlier Int32 overflow. Added max/+1/-1 and min/-1/+1 regressions plus wrong-day/missing-choice negative cases; focused authoring suite 34/34 passes. Canonical campaign assets unchanged.

2026-10-22 source refresh: all1440 authored rows were reviewed. Day0 now keeps all12 source rows (9 introduction lines, one Purify action,2 reaction lines); the obsolete duplicate exclusion is removed. Priest JN05 was removed from the new portrait definitions; the user explicitly approved JN04 for its7 remaining references. Existing other-day prose and numeric rules are unchanged. The original341 repair preconditions were rechecked before repinning file hashes. Current totals:1299 active rows,141 retained conflict exclusions,122 recorded adaptations. Evidence: docs/dialogue-update-2026-10-22/verification.json.

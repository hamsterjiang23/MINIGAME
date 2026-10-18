# Deviations

None. Development values and the diplomat multi-column interpretation are the accepted defaults.

# Discovered edge cases

- Some XLSX cells omit cached values and store literal array formulas. The converter fills only missing cells from constant arrays, never evaluates formulas; provenance keeps both stored and reconstructed fields.
- Shared doctor/diplomat END rows are referenced from multiple days. They become separately namespaced visits while retaining their single original source row.
- Doctor and diplomat decision preambles have no ordinary choice. They are attached to the preceding answer branches, so their text plays before the disposition gate without fabricating a player answer.

# Questions for review

None blocking. All adaptation text is isolated from original spreadsheet text.

Deviations count: 0.
Most revisitable: explicit diplomat wording adaptations, isolated in text-variants.json.
Edge cases found: missing cached array cells, shared END records, decision preambles, cross-day edges.
Next read: README.md and conversion-config.json, then provenance.json.
Validation: smoke Day0 plus child two days; full 994 rows and 16 meetings; Python 3/3; C# CampaignPackageTests plus V1 AuthoringPackageTests 30/30; deterministic regeneration passes.

Review fix: trust bounds are now checked after every ordinary choice as well as after disposition. A later opposite delta cannot hide an earlier Int32 overflow. Added max/+1/-1 and min/-1/+1 regressions plus wrong-day/missing-choice negative cases; focused authoring suite 34/34 passes. Canonical campaign assets unchanged.

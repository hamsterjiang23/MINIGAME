# Deviations

- The offline deliverable is assembled into one HTML from small editable JS/CSS/template files. This preserves direct opening without fetch while keeping the parser testable.

# Discovered edge cases

- Source containing unsupported Yarn constructs must remain readable and downloadable unchanged; form edits and preview stay disabled until the source is supported. Browser preview is a constrained rehearsal, not the native Yarn compiler.
- Form serialization escapes literal markup and retains source comments, patient/day headers and line tags. Unedited source retains exact bytes through JSON/Yarn export; edited node comments may move within that node.
- Browser and C# both check sequential-round convergence, finite reaction routes, all-node cycles, strict literal escapes and 32-bit trust bounds across every route. Native Yarn compilation remains final. Browser visual/download interactions were not exercised because the existing file preview was security-blocked.

# Questions for review

- None at implementation start; the HTML demo profile is explicitly separate from production rules.

Deviations count: 1 (assembled offline HTML).
Most revisitable: add richer graph editing only after the list/form workflow is accepted.
Edge cases found: unsupported source preservation, escaped markup, source comments, dangling references, unapplied drafts.
Next read: README.md, then planner-core.js and planner.test.js.
Validation: 18 Node tests pass, including strict native-contract edge cases, all 1296 reply routes and three disposition reactions; browser acceptance remains separate.

V2 guard: full campaign JSON is rejected before parsing or replacing the current V1 document, with a direct conversion/import instruction. Save, export, and preview also guard V2. Node tests now 19/19; rebuilt offline HTML matches its sources.

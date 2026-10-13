# Implementation notes

## Deviations

- The final recursive deletion was rejected by automatic execution policy (`blocked by policy`). The F: target is complete and verified; the original D: research directory remains. No alternate deletion mechanism was attempted.

## Discovered edge cases

- Active scripts contain the old D: research root; derive the root from each script location. Preserve historical JSON evidence and use a relocation mapping instead of rewriting original provenance.
- Source contains 23,961 files, 1,218 directories and 7,004,476,419 bytes at preflight; no reparse points. F: has sufficient space, CPU load is 6%, and free RAM is approximately 23 GiB. Hash and copy serially with a bounded buffer.
- Independent migration review identified that copy verification only required the original directories to exist. Final verification additionally checks the complete source directory set and rejects reparse points and unexpected destination directories.

## Questions for review

None. Original Steam files and repository core source are outside the mutation scope.

## Completion summary

- Deviations: 1; old research-directory cleanup blocked by execution policy.
- Most likely to revisit: remove the retained D: duplicate after resolving the policy block.
- Edge cases: historical absolute paths, future report regeneration, directory/reparse-point coverage.
- Target verification: 23,961 originals checked; 14 approved changes; 138 local links valid; syntax and independent reviews passed.
- Next entry: relocation.json and acceptance.json, then repository README.md and AGENTS.md.

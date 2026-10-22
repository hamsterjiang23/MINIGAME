# Employee apartment 1.0 room replacement

## Deviations

- The supplied file includes a corridor and a different doorway. Import only the room and adapt the entrance wall to the existing gameplay door; preserve the current corridor, door animation, camera anchors, lights and interaction state. Revisit only if the corridor is explicitly replaced.

## Discovered edge cases

- The standard combined-room import rebuilds gameplay and the corridor. Use an in-place art replacement instead, retaining scene object references used by the terminal, bed, lighting and hidden ending.
- Blender source and backup files remain ignored. Read the supplied source without modifying it; publish exported Unity assets and an import manifest.
- Keep the supplied rug mesh in its authored position and disable the old generated rug quad. Its optional lighting-controller rug reference is cleared so the quad sizing controls cannot stretch the authored mesh; day/night lighting and pixel profiles remain assigned.
- Three decorative source posters overlap existing story readables. Their renderers are disabled while the readable objects and their interaction bindings stay in place.
- The source chair has two unavailable legacy images. Use the same source wood fallback as the previous room importer. Source color-space settings and carpet roughness are retained during interchange.
- The source CRT is tilted and smaller than the previous monitor. Preserve the runtime display material, replace its mesh, normalize the screen UVs and reposition the seated/alignment camera anchors. The physical-screen four-corner test verifies the FBX horizontal handedness correction.

## Questions for review

- None at present.

## Completion

Deviations: two visual adaptations, for the existing doorway and story readables.
Most likely to revisit: doorway alignment if the corridor is replaced later.
Handled edge cases: source corridor exclusion, missing chair maps, generated rug overlay, overlapping posters and monitor UV handedness.
Validation: three targeted Play Mode flows and the physical-screen four-corner check passed; protected corridor/door YAML subtrees match the baseline.
Read first next session: verification.json and Art/MainRoom/employee-apartment/README.md.

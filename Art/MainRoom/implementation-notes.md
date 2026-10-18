# Deviations

- The published GLB is converted to FBX with the installed Blender 4.5.11 for Unity's native model importer. Textures and a Standard material manifest are extracted from the same GLB; source art is preserved. [verified: scripts/export_unity.py main]
- The imported art gets a separate MainRoomPreview scene with approximate realtime lights. InspectionApartment remains the gameplay entry; the preview has no patient or room interaction integration. [verified: ../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs ImportAndOpen]

# Discovered edge cases

- The GLB repeats a linen material with texCoord=-1 and exports 16 albedo/bump inputs as normal textures. The conversion normalizes negative UV indices and omits those invalid normal inputs; the actual Tripo normal remains. [verified: scripts/export_unity.py main; ../../UnityProject/Assets/MiniGame/Art/MainRoom/materials.json omittedNonNormalBumpInputs]
- The reference camera is beyond the front wall. The preview retains the front closure meshes but disables their renderers to match the source preview composition. [verified: scripts/finalize_room.py cam.location/front.hide_render; ../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs ImportAndOpen]
- The source's double-sided curtain planes need a cull option that Built-in Standard does not expose. RoomArtStandard preserves the source setting and flips back-face normals; preview creation leaves the project's quality settings unchanged. [verified: ../../UnityProject/Assets/MiniGame/Art/MainRoom/RoomArtStandard.shader; ../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs MakeMaterial]
- glTF color and emissive factors are linear; the current Gamma project requires conversion before shader consumption. Metallic/roughness channel data stays linear. [verified: ../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs SourceColor/Texture; ../../UnityProject/ProjectSettings/ProjectSettings.asset m_ActiveColorSpace]

# Questions for review

- Gameplay placement, door/bed/terminal interactions, colliders and lighting across days remain future work. [opinion]

Summary: two conversion/preview choices; four source-format edge cases handled. [verified: Deviations and Discovered edge cases above]
The lighting approximation is the main choice to revisit. [opinion]
Validation: real Unity import, material mapping and a reviewed GPU camera capture. [verified: ../../docs/main-room-import-verification.json import/evidence]
Gameplay and performance acceptance remain outside this preview. [verified: ../../docs/main-room-import-verification.json limits]
Next read: README.md and the import verification JSON. [opinion]

## 2026-10-18 room combination

- Composition: retain MainRoom's shell, original graphics, window/heating, CRT and rug; replace desk/chair, bed/bedding, cabinet and selected props from MainRoom2. Source inputs remain intact; combined editable/export files live under `combined/`. [verified: scripts/combine_rooms.py groups/replaced/report]
- Portability: selected MainRoom2 materials are converted to simple Principled image inputs. Missing chair material input uses the source wood material; Blender displacement/volumetric lighting and unrelated source meshes are excluded. [verified: scripts/combine_rooms.py portable_material/selected]
- Static detail grouping keeps small rug fibres, keyboard keys, window frames and radiator parts in named meshes while preserving bed, furniture and poster objects. Object counts are asset statistics, not measured runtime performance. [verified: scripts/combine_rooms.py join_groups] [opinion]
- MainRoom2 contains keyed object locations at frame 133. Selected furniture/props are frozen to their evaluated world matrices before clearing animation, otherwise GLB import resets some locations. Export now checks each imported object's room/floor bounds and desktop support height. [verified: scripts/combine_rooms.py frozen_animations/placements; combined/reports/composition.json source_b_frozen_object_animations/source_b_placement_checks]
- User steering: preserve the concrete wall outside the window. The actual MainRoom2 concrete wall and ledge are retained at their source distance from the window, with transparent glass and a separate close-up capture for verification. [verified: user request; scripts/combine_rooms.py ExteriorConcrete; ../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs Build]

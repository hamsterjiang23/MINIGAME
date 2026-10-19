# Reference room art

`MainRoomReference.blend` and `MainRoomReference.glb` use the unmodified
`Art/MainRoom2/minigame员工宿舍.blend` at frame 133. `geometry-report.json`
records the original world transforms and source hashes; `verification.json`
records independent static checks. These checks do not establish Unity gameplay.

The interior is 9.70078 × 4.41176 × 3.12066 metres. Furniture stays at its
reference location and scale. The accepted A CRT and peripherals replace the B
computer at the original B workstation. Two small desktop props move aside to
clear its footprint. Old posters, old computer models, hidden humans, oversized
leak decals, and the original hidden door are excluded.

Only oversized shell geometry is clipped; source UVs are retained. A 1.5 × 2.5 m
door opening is built into the right short wall. Both exterior concrete objects
retain their original transforms and dimensions. The nested reference floor
material is converted explicitly, including its HSV settings and UV mapping;
portable floor roughness is 0.62. Exported textures are capped at 2048 pixels.

`layout.json` records all marker coordinates in Blender world XYZ with Z up.
The existing FBX exporter converts these to Unity Y up. Markers for the origin
and unit axes let the gameplay builder recover the frame without hard-coded
Unity axis signs. `materials.json.referenceLayout` contains the same metadata.

From the repository root:

```powershell
& 'E:/Blender 4.5.11/blender.exe' --background --threads 2 --python Art/MainRoom/reference/build_reference_layout.py
& 'E:/Blender 4.5.11/blender.exe' --background --threads 2 --python Art/MainRoom/scripts/export_unity.py -- --source Art/MainRoom/reference/MainRoomReference.glb --output UnityProject/Assets/MiniGame/Art/MainRoom/Combined --evidence-dir Artifacts/ReferenceRoom/Export --layout-json Art/MainRoom/reference/layout.json
uv run python Art/MainRoom/reference/verify_reference.py
```

Keep the existing Unity asset `.meta` files when regenerating the published art.
The original Blender and FBX reference sources are never overwritten.

# Employee apartment import

Source: `employee apartment1.0.blend`, SHA-256 `31d1e5b81f2a33afdbe8480ad8eb2e9533f2be555763c7b53464940b7c50cf94`. The exporter reads frame 133 without saving the source. Blender source files remain ignored. [verified: Artifacts/EmployeeApartmentImport/export/export-manifest.json sourceSha256, frame, sourceUnchanged; .gitignore Blender patterns]

The exported room retains the supplied furnishings, bookshelf, posters, computer and window exterior. Its own corridor, entrance wall, door and frame are excluded so the Unity scene can keep the existing corridor and doorway. Oversized room shells are clipped to the existing room bounds. [verified: export_apartment.py CORRIDOR, PLACEHOLDERS, SHELLS, main]

The computer screen is split as `CRT screen emission`, with full 0–1 UVs; its body is `CRT_Monitor_Tripo`, and keyboard/mouse/base unit are `A_TerminalAccessories`. Remaining source meshes use `B_` names. [verified: export_apartment.py split_screen, main]

Wallpaper/poster color adjustments, texture mapping and the carpet roughness ramp are baked for interchange. Source image color spaces are retained. The chair's two missing legacy images use the same source wood fallback as the previous room importer. Window glass retains the existing transparent Unity treatment. [verified: export_apartment.py bake_hsv, bake_mapping, prepare_material, main]

Run from the repository root with Blender 4.5.11:

```powershell
& 'E:/Blender 4.5.11/blender.exe' --background --factory-startup --disable-autoexec --threads 2 --python Art/MainRoom/employee-apartment/export_apartment.py -- --source 'C:/Users/admin/Downloads/employee apartment1.0.blend'
& 'E:/Blender 4.5.11/blender.exe' --background --factory-startup --disable-autoexec --threads 2 --python Art/MainRoom/scripts/export_unity.py -- --source Artifacts/EmployeeApartmentImport/export/EmployeeApartment.glb --output UnityProject/Assets/MiniGame/Art/EmployeeApartment --evidence-dir Artifacts/EmployeeApartmentImport/unity-export --layout-json Artifacts/EmployeeApartmentImport/export/layout.json
```

The first command writes the GLB, source-to-export object mapping, screen axes and dimensions, and room marker metadata under `Artifacts/EmployeeApartmentImport/export/`. The second extracts the textures and emits `MainRoom.fbx` and `materials.json` in the new Unity asset directory. [verified: export_apartment.py main; ../scripts/export_unity.py main]

Export validation covers source hash preservation, required mesh names, corridor exclusion and full-range screen UVs. Scene bindings, collisions, live terminal appearance and gameplay require Unity verification. [verified: Artifacts/EmployeeApartmentImport/export/export-verification.json scope]

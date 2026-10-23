"""Export only the supplied apartment's seven buildings and ground, retaining world placement.

Run in Blender with the supplied .blend already open; no source file is saved.
Pass --output to choose the intermediate GLB destination.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path

import bpy
from mathutils import Vector


EXTERIOR_NAMES = (
    "立方体.034", "平面.032", "立方体.003", "立方体.016",
    "立方体.017", "立方体.019", "立方体.020", "立方体.021",
)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    objects = [bpy.data.objects.get(name) for name in EXTERIOR_NAMES]
    if any(obj is None or obj.type != "MESH" for obj in objects):
        raise ValueError("The supplied apartment does not contain the expected exterior meshes")
    bpy.ops.object.select_all(action="DESELECT")
    report: list[dict[str, object]] = []
    for obj in objects:
        corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
        if obj.name != "平面.032" and max(point.x for point in corners) >= -20:
            raise ValueError(f"Exterior candidate overlaps the room: {obj.name}")
        obj.select_set(True)
        report.append({"sourceName": obj.name, "exportName": "Exterior_" + obj.name,
                       "vertices": len(obj.data.vertices),
                       "min": [min(p[i] for p in corners) for i in range(3)],
                       "max": [max(p[i] for p in corners) for i in range(3)]})
    bpy.context.view_layer.objects.active = objects[0]
    args.output.parent.mkdir(parents=True, exist_ok=True)
    materials = {slot.material for obj in objects for slot in obj.material_slots if slot.material}
    textures = {node.image for material in materials if material.use_nodes
                for node in material.node_tree.nodes if node.type == "TEX_IMAGE" and node.image}
    missing = [image.name for image in textures if not image.packed_file
               and not Path(bpy.path.abspath(image.filepath)).is_file()]
    if missing:
        raise ValueError(f"Exterior uses missing texture files: {missing}")
    original_names = [(obj, obj.name) for obj in objects]
    try:
        for obj, name in original_names:
            obj.name = "Exterior_" + name
        bpy.ops.export_scene.gltf(filepath=str(args.output), export_format="GLB", use_selection=True,
                                  export_apply=True, export_animations=False, export_cameras=False,
                                  export_lights=False)
    finally:
        for obj, name in original_names:
            obj.name = name
    manifest = {"source": bpy.data.filepath,
                "sourceSha256": hashlib.sha256(Path(bpy.data.filepath).read_bytes()).hexdigest(),
                "sourceFrame": bpy.context.scene.frame_current, "objects": report,
                "materials": sorted(material.name for material in materials),
                "textures": sorted(image.name for image in textures), "missingTextures": missing,
                "coordinates": "Retain Blender world coordinates; existing FBX converter maps to Unity (-x,z,-y)",
                "scope": "Exterior only: seven apartment blocks and ground; no room, camera or source lights"}
    args.output.with_suffix(".json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("WINDOW_EXTERIOR_EXPORTED", json.dumps({"objects": len(objects), "materials": len(materials),
          "textures": len(textures), "missingTextures": missing}), flush=True)


if __name__ == "__main__":
    main()

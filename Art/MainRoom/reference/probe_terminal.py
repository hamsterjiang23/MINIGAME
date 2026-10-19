"""Inspect only the accepted terminal dimensions before reference-layout placement."""
from __future__ import annotations

from pathlib import Path
import json
import bpy
from mathutils import Vector


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(root / "Art/MainRoom/combined/exports/MainRoomCombined.glb"))
    for obj in bpy.context.scene.objects:
        if not obj.name.startswith(("CRT", "A_Terminal", "Computer power")):
            continue
        points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
        print(json.dumps({"name": obj.name,
            "min": [round(min(point[i] for point in points), 5) for i in range(3)],
            "max": [round(max(point[i] for point in points), 5) for i in range(3)],
            "matrix": [list(row) for row in obj.matrix_world]}, ensure_ascii=False))


if __name__ == "__main__":
    main()

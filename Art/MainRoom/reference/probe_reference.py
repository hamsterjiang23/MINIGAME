"""Read the unmodified room source at frame 133 and write a compact geometry inventory."""
from __future__ import annotations

import json
from pathlib import Path

import bpy
from mathutils import Vector


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    source = root / "Art/MainRoom2/minigame员工宿舍.blend"
    bpy.ops.wm.open_mainfile(filepath=str(source))
    bpy.context.scene.frame_set(133)
    bpy.context.view_layer.update()
    objects: list[dict[str, object]] = []
    for obj in bpy.context.scene.objects:
        points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
        objects.append({
            "name": obj.name, "type": obj.type,
            "min": [round(min(point[i] for point in points), 5) for i in range(3)],
            "max": [round(max(point[i] for point in points), 5) for i in range(3)],
            "location": list(obj.matrix_world.translation),
            "matrix": [list(row) for row in obj.matrix_world],
            "materials": [slot.material.name if slot.material else "" for slot in obj.material_slots],
            "hideRender": obj.hide_render, "hideViewport": obj.hide_viewport,
            "vertices": len(obj.data.vertices) if obj.type == "MESH" else 0,
        })
    output = Path(__file__).parent / "probe.json"
    output.write_text(json.dumps({"frame": bpy.context.scene.frame_current, "objects": objects},
                                 ensure_ascii=False, indent=2), encoding="utf-8")
    for obj in objects:
        print(json.dumps({key: obj[key] for key in ("name", "type", "min", "max", "materials", "hideRender", "vertices")}, ensure_ascii=False))


if __name__ == "__main__":
    main()

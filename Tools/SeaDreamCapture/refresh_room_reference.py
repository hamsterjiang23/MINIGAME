"""Refresh the hidden house from the current published reference-room authoring file."""

import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def main() -> None:
    stage = bpy.data.scenes["Dream sea midpoint capture"]
    reference = bpy.data.collections["Room alignment reference - hidden for capture"]
    for obj in list(reference.all_objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    root_path = Path("F:/AAMINIGAME/MINIGAME")
    room_path = root_path / "Art/MainRoom/reference/MainRoomReference.blend"
    layout = json.loads(
        (root_path / "Art/MainRoom/reference/layout.json").read_text(encoding="utf-8")
    )
    spawn = Vector(layout["markers"]["Spawn"])
    center = Vector(stage.get("sea_capture_spawn", stage["sea_capture_midpoint"]))
    capture_yaw = float(stage["sea_capture_yaw"])
    room_yaw = capture_yaw + math.pi
    with bpy.data.libraries.load(str(room_path), link=False) as (source, target):
        target.objects = list(source.objects)
    root = bpy.data.objects.new("House placement - spawn at camera midpoint", None)
    reference.objects.link(root)
    for obj in target.objects:
        if obj is None:
            continue
        reference.objects.link(obj)
        if obj.parent is None:
            obj.parent = root
        if obj.type == "MESH":
            obj.display_type = "WIRE"
    root.matrix_world = (
        Matrix.Translation(center)
        @ Matrix.Rotation(room_yaw, 4, "Z")
        @ Matrix.Translation(-spawn)
    )
    marker = bpy.data.objects.new("Player spawn - Camera Camera2 midpoint", None)
    reference.objects.link(marker)
    marker.location = center
    marker.empty_display_type = "ARROWS"
    marker.empty_display_size = 0.6
    bpy.context.window.scene = stage
    bpy.context.view_layer.update()
    actual = root.matrix_world @ spawn
    error = (actual - center).length
    if error > 1e-5:
        raise RuntimeError("Reference house spawn does not match camera midpoint")
    forward = Matrix.Rotation(capture_yaw, 3, "Z") @ Vector((0, 1, 0))
    stage.camera.rotation_euler = forward.to_track_quat("-Z", "Y").to_euler()
    path = Path(stage["sea_capture_output"])
    settings_path = path / "capture-settings.json"
    settings = json.loads(settings_path.read_text(encoding="utf-8"))
    settings.update(
        {
            "room_source": str(room_path),
            "room_authored_spawn": list(spawn),
            "room_rotation_blender_degrees": math.degrees(room_yaw),
            "spawn_alignment_error_m": error,
            "room_reference_hidden_for_render": reference.hide_render,
            "render": {
                "engine": "CYCLES",
                "device": "GPU CUDA",
                "face_size": 1024,
                "samples": 32,
            },
            "coordinate_contract": "Reference FBX converts Blender (-x,-y,z) to Unity (x,z,y). House rotation=capture yaw+pi, so Unity (x,y,z) matches captured Rz(yaw)*(x,z,y). Native Cubemap PNG rows are top-down, verified on Unity D3D11.",
        }
    )
    settings_path.write_text(
        json.dumps(settings, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    bpy.ops.wm.save_as_mainfile(filepath=str(path / "SeaMidpointAligned.blend"))
    print(
        json.dumps(
            {
                "active_file": bpy.data.filepath,
                "spawn_alignment_error_m": error,
                "room_source": str(room_path),
                "room_yaw_degrees": math.degrees(room_yaw),
                "source_direction_for_unity_forward": list(forward),
                "room_hidden_for_capture": reference.hide_render,
            },
            ensure_ascii=False,
        )
    )


main()

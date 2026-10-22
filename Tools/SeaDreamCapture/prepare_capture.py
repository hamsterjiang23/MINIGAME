"""Run in Blender through its approved MCP script directory."""

import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def main() -> None:
    original = bpy.context.scene
    first = original.objects.get("Camera")
    second = original.objects.get("Camera2")
    if (
        first is None
        or second is None
        or first.type != "CAMERA"
        or second.type != "CAMERA"
    ):
        raise RuntimeError("Camera and Camera2 must both exist in the live scene")
    output = Path("F:/AAMINIGAME/MINIGAME/Art/DreamEnding/SeaMidpoint")
    output.mkdir(parents=True, exist_ok=True)
    center = (first.matrix_world.translation + second.matrix_world.translation) * 0.5
    eye = center.copy()
    spawn = eye - Vector((0, 0, 1.56))
    removed = []
    for obj in list(original.objects):
        if obj.name.split(".")[0] in {"SM_bed_1", "SM_mattress_1", "SM_pillow_1"}:
            removed.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)
    stage = original.copy()
    stage.name = "Dream sea midpoint capture"
    reference = bpy.data.collections.new(
        "Room alignment reference - hidden for capture"
    )
    stage.collection.children.link(reference)
    reference.hide_render = True
    room_path = "F:/AAMINIGAME/MINIGAME/Art/MainRoom/reference/MainRoomReference.blend"
    layout = json.loads(
        Path("F:/AAMINIGAME/MINIGAME/Art/MainRoom/reference/layout.json").read_text(
            encoding="utf-8"
        )
    )
    with bpy.data.libraries.load(room_path, link=False) as (source, target):
        target.objects = list(source.objects)
    for obj in target.objects:
        if obj is not None:
            reference.objects.link(obj)
    bpy.context.view_layer.update()
    authored_spawn = Vector(layout["markers"]["Spawn"])
    forward = first.matrix_world.to_quaternion() @ Vector((0, 0, -1))
    forward.z = 0
    forward.normalize()
    yaw = math.atan2(-forward.x, forward.y)
    # Compensate the current room FBX's two horizontal sign flips.
    rotation = Matrix.Rotation(yaw + math.pi, 4, "Z")
    transform = (
        Matrix.Translation(spawn) @ rotation @ Matrix.Translation(-authored_spawn)
    )
    root = bpy.data.objects.new("House placement - spawn at camera midpoint", None)
    reference.objects.link(root)
    for obj in target.objects:
        if obj is not None and obj.parent is None:
            obj.parent = root
    root.matrix_world = transform
    marker = bpy.data.objects.new("Player spawn - Camera Camera2 midpoint", None)
    reference.objects.link(marker)
    marker.location = spawn
    marker.empty_display_type = "ARROWS"
    marker.empty_display_size = 0.6
    camera_data = bpy.data.cameras.new("Sea midpoint six-face capture")
    camera_data.type = "PERSP"
    camera_data.lens_unit = "FOV"
    camera_data.angle = math.pi / 2
    camera_data.clip_start = 0.03
    camera_data.clip_end = 2500
    camera = bpy.data.objects.new("Sea midpoint capture eye", camera_data)
    stage.collection.objects.link(camera)
    camera.location = eye
    stage.camera = camera
    stage.render.engine = "CYCLES"
    stage.cycles.device = "CPU"
    stage.cycles.samples = 32
    stage.cycles.use_denoising = True
    stage.render.threads_mode = "FIXED"
    stage.render.threads = 6
    stage.render.resolution_x = 1024
    stage.render.resolution_y = 1024
    stage.render.resolution_percentage = 100
    stage.render.image_settings.file_format = "PNG"
    stage.render.image_settings.color_mode = "RGBA"
    stage.render.image_settings.color_depth = "8"
    stage.render.film_transparent = False
    stage.use_nodes = False
    stage["sea_capture_output"] = str(output)
    stage["sea_capture_midpoint"] = list(center)
    stage["sea_capture_eye"] = list(eye)
    stage["sea_capture_spawn"] = list(spawn)
    stage["sea_capture_yaw"] = yaw
    report = {
        "source": bpy.data.filepath,
        "source_had_unsaved_changes": bpy.data.is_dirty,
        "cameras": {
            obj.name: list(obj.matrix_world.translation) for obj in (first, second)
        },
        "camera_midpoint_blender": list(center),
        "spawn_blender": list(spawn),
        "eye_blender": list(eye),
        "capture_eye_matches_camera_midpoint": True,
        "panorama_sea_level": round(-eye.z, 6),
        "removed_source_objects": removed,
        "eye_height_m": 1.56,
        "room_source": room_path,
        "room_authored_spawn": list(authored_spawn),
        "room_reference_hidden_for_render": reference.hide_render,
        "room_rotation_blender_degrees": math.degrees(yaw + math.pi),
        "initial_forward_blender": list(forward),
        "render": {
            "engine": "CYCLES",
            "device": "CPU",
            "threads": 6,
            "face_size": 1024,
            "samples": 32,
        },
        "coordinate_contract": "The current room imports with two horizontal sign flips, compensated by house yaw=capture yaw+pi. Native D3D11 Cubemap payload uses PNG top-down rows, verified by GPU skybox rendering.",
    }
    (output / "capture-settings.json").write_text(
        json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    previous_scene = bpy.context.window.scene
    try:
        bpy.context.window.scene = stage
        camera.rotation_euler = forward.to_track_quat("-Z", "Y").to_euler()
        bpy.ops.wm.save_as_mainfile(
            filepath=str(output / "SeaMidpointAligned.blend"), copy=True
        )
    finally:
        bpy.context.window.scene = previous_scene
    print(json.dumps(report, ensure_ascii=False))


main()

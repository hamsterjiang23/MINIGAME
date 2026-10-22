"""Remove the original sea bed and calibrate the capture eye, rather than feet, to Camera/Camera2."""

import json
import math
import re
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def main() -> None:
    stage = bpy.data.scenes["Dream sea midpoint capture"]
    reference = bpy.data.collections["Room alignment reference - hidden for capture"]
    first, second = stage.objects["Camera"], stage.objects["Camera2"]
    eye = (first.matrix_world.translation + second.matrix_world.translation) * 0.5
    previous_eye = stage.camera.matrix_world.translation.copy()
    spawn = eye - Vector((0, 0, 1.56))
    output = Path(stage["sea_capture_output"])
    settings = json.loads(
        (output / "capture-settings.json").read_text(encoding="utf-8")
    )
    original_scene_names = [scene.name for scene in bpy.data.scenes]
    for scene in list(bpy.data.scenes):
        if scene.name.startswith("Sea panorama water mask capture"):
            for obj in list(scene.objects):
                if len(obj.users_scene) == 1:
                    bpy.data.objects.remove(obj, do_unlink=True)
            bpy.data.scenes.remove(scene)
    reference_objects = set(reference.all_objects)
    beds = [
        obj
        for obj in stage.objects
        if obj not in reference_objects
        and obj.name.split(".")[0] in {"SM_bed_1", "SM_mattress_1", "SM_pillow_1"}
    ]
    remove = set(beds)
    for bed in beds:
        remove.update(
            obj for obj in bed.children_recursive if obj not in reference_objects
        )
    removed = sorted(obj.name for obj in remove)
    for obj in remove:
        bpy.data.objects.remove(obj, do_unlink=True)
    root = bpy.data.objects["House placement - spawn at camera midpoint"]
    authored_spawn = Vector(settings["room_authored_spawn"])
    yaw = float(stage["sea_capture_yaw"])
    root.matrix_world = (
        Matrix.Translation(spawn)
        @ Matrix.Rotation(yaw + math.pi, 4, "Z")
        @ Matrix.Translation(-authored_spawn)
    )
    marker = reference.objects.get("Player spawn - Camera Camera2 midpoint")
    if marker is not None:
        marker.location = spawn
    stage.camera.location = eye
    stage.camera.rotation_euler = (
        (Matrix.Rotation(yaw, 3, "Z") @ Vector((0, 1, 0)))
        .to_track_quat("-Z", "Y")
        .to_euler()
    )
    stage["sea_capture_midpoint"] = list(eye)
    stage["sea_capture_eye"] = list(eye)
    stage["sea_capture_spawn"] = list(spawn)
    bpy.context.window.scene = stage
    bpy.context.view_layer.update()
    error = ((root.matrix_world @ authored_spawn) + Vector((0, 0, 1.56)) - eye).length
    if error > 1e-5:
        raise RuntimeError("The room's standing eye must match the two-camera midpoint")
    profile = Path(
        "F:/AAMINIGAME/MINIGAME/UnityProject/Assets/MiniGame/Resources/MiniGame/DreamSequenceDefault.asset"
    )
    match = re.search(
        r"^  PanoramaSeaLevel: ([^\r\n]+)",
        profile.read_text(encoding="utf-8"),
        re.MULTILINE,
    )
    previous_level = float(
        settings.get("panorama_sea_level", float(match.group(1)) if match else -6.5)
    )
    settings.update(
        {
            "camera_midpoint_blender": list(eye),
            "cameras": {
                obj.name: list(obj.matrix_world.translation) for obj in (first, second)
            },
            "previous_eye_blender": list(previous_eye),
            "eye_blender": list(eye),
            "spawn_blender": list(spawn),
            "capture_eye_matches_camera_midpoint": True,
            "eye_height_m": 1.56,
            "removed_source_objects": sorted(
                set(settings.get("removed_source_objects", [])) | set(removed)
            ),
            "capture_eye_alignment_error_m": error,
            "spawn_alignment_error_m": (
                root.matrix_world @ authored_spawn - spawn
            ).length,
            "panorama_sea_level": round(previous_level + previous_eye.z - eye.z, 6),
            "capture_revision": "source-bed-removed-camera-height",
            "blender_frame": stage.frame_current,
        }
    )
    (output / "capture-settings.json").write_text(
        json.dumps(settings, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    bpy.ops.wm.save_as_mainfile(filepath=str(output / "SeaMidpointAligned.blend"))
    print(
        json.dumps(
            {
                "removed_objects": removed,
                "eye": list(stage.camera.matrix_world.translation),
                "spawn": list(spawn),
                "camera_average_height": eye.z,
                "eye_alignment_error_m": error,
                "updated_panorama_sea_level": settings["panorama_sea_level"],
                "previous_scenes": original_scene_names,
                "remaining_scenes": [scene.name for scene in bpy.data.scenes],
            },
            ensure_ascii=False,
        )
    )


main()

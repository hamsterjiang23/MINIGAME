"""Render a smoke face or all six faces from the prepared Blender scene."""

import json
import time
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def main() -> None:
    stage = bpy.data.scenes.get("Dream sea midpoint capture")
    if stage is None:
        raise RuntimeError("Prepare the midpoint capture scene first")
    output = Path(stage["sea_capture_output"])
    arguments = globals().get("args", {})
    smoke = bool(arguments.get("smoke", False))
    use_gpu = bool(arguments.get("gpu", False))
    size, samples = (128, 4) if smoke else (1024, 32)
    faces = [
        ("px", (1, 0, 0), (0, 1, 0)),
        ("nx", (-1, 0, 0), (0, 1, 0)),
        ("py", (0, 1, 0), (0, 0, -1)),
        ("ny", (0, -1, 0), (0, 0, 1)),
        ("pz", (0, 0, 1), (0, 1, 0)),
        ("nz", (0, 0, -1), (0, 1, 0)),
    ]
    # Handedness conversion: Unity X/Y/Z -> Blender X/Z/Y, then source heading.
    basis = Matrix(((1, 0, 0), (0, 0, 1), (0, 1, 0)))
    rotation = Matrix.Rotation(float(stage["sea_capture_yaw"]), 3, "Z")
    camera = stage.camera
    old_scene = bpy.context.window.scene
    completed = []
    preferences = bpy.context.preferences.addons["cycles"].preferences
    previous_compute = preferences.compute_device_type
    previous_device = stage.cycles.device
    previous_devices = {device.id: device.use for device in preferences.devices}
    started = time.monotonic()
    folder = output / ("Smoke" if smoke else "Faces")
    folder.mkdir(exist_ok=True)
    try:
        if use_gpu:
            preferences.compute_device_type = "CUDA"
            preferences.get_devices()
            for device in preferences.devices:
                device.use = device.type == "CUDA"
            if not any(device.use for device in preferences.devices):
                raise RuntimeError("No CUDA device is available")
            stage.cycles.device = "GPU"
        bpy.context.window.scene = stage
        stage.render.resolution_x = stage.render.resolution_y = size
        stage.cycles.samples = samples
        for name, direction, up in faces:
            forward = rotation @ basis @ Vector(direction)
            vertical = rotation @ basis @ Vector(up)
            right = forward.cross(vertical).normalized()
            camera.rotation_euler = (
                Matrix((right, vertical, -forward)).transposed().to_euler()
            )
            stage.render.filepath = str(folder / (name + ".png"))
            bpy.ops.render.render(write_still=True, scene=stage.name)
            completed.append(name)
            status = {
                "smoke": smoke,
                "device": stage.cycles.device,
                "completed_faces": completed,
                "elapsed_seconds": time.monotonic() - started,
                "size": size,
                "samples": samples,
            }
            (output / "render-progress.json").write_text(
                json.dumps(status, indent=2), encoding="utf-8"
            )
        print(json.dumps(status))
    finally:
        stage.render.resolution_x = stage.render.resolution_y = 1024
        stage.cycles.samples = 32
        stage.cycles.device = previous_device
        for device in preferences.devices:
            if device.id in previous_devices:
                device.use = previous_devices[device.id]
        preferences.compute_device_type = previous_compute
        bpy.context.window.scene = old_scene


main()

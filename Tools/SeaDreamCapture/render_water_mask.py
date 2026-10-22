"""Capture original Ocean* visibility from the same fixed eye using Blender's object-index pass."""

import json
import math
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def main() -> None:
    source = bpy.data.scenes["Dream sea midpoint capture"]
    stage = bpy.data.scenes.new("Sea panorama water mask capture")
    reference = set(
        bpy.data.collections[
            "Room alignment reference - hidden for capture"
        ].all_objects
    )
    for obj in source.objects:
        if obj.type not in {"MESH", "LIGHT"} or obj in reference:
            continue
        clone = obj.copy()
        # Ocean005 is the mud/sand substrate, despite its name. Only the other ocean layers are water/foam.
        clone.pass_index = (
            1 if obj.name.split(".")[0] in {"Ocean001", "Ocean002", "Ocean004"} else 0
        )
        stage.collection.objects.link(clone)
        clone.matrix_world = obj.matrix_world.copy()
    stage.world = source.world
    stage.render.engine = "CYCLES"
    stage.cycles.samples = 4
    stage.cycles.device = "GPU"
    stage.render.resolution_x = stage.render.resolution_y = 512
    stage.render.resolution_percentage = 100
    stage.render.image_settings.file_format = "PNG"
    stage.render.image_settings.color_mode = "RGBA"
    stage.view_settings.view_transform = "Standard"
    stage.view_settings.look = "None"
    stage.view_settings.exposure = 0
    stage.view_layers[0].use_pass_object_index = True
    stage.view_layers[0].pass_alpha_threshold = 0.05
    stage.use_nodes = True
    nodes = stage.node_tree.nodes
    nodes.clear()
    render = nodes.new("CompositorNodeRLayers")
    render.scene = stage
    mask = nodes.new("CompositorNodeIDMask")
    mask.index = 1
    mask.use_antialiasing = True
    composite = nodes.new("CompositorNodeComposite")
    stage.node_tree.links.new(render.outputs["IndexOB"], mask.inputs[0])
    stage.node_tree.links.new(mask.outputs[0], composite.inputs[0])
    camera_data = bpy.data.cameras.new("Water mask eye")
    camera_data.lens_unit = "FOV"
    camera_data.angle = math.pi / 2
    camera_data.clip_start = 0.03
    camera_data.clip_end = 2500
    camera = bpy.data.objects.new("Water mask eye", camera_data)
    stage.collection.objects.link(camera)
    camera.location = Vector(source["sea_capture_eye"])
    stage.camera = camera
    basis = Matrix(((1, 0, 0), (0, 0, 1), (0, 1, 0)))
    rotation = Matrix.Rotation(float(source["sea_capture_yaw"]), 3, "Z")
    faces = [
        ("px", (1, 0, 0), (0, 1, 0)),
        ("nx", (-1, 0, 0), (0, 1, 0)),
        ("py", (0, 1, 0), (0, 0, -1)),
        ("ny", (0, -1, 0), (0, 0, 1)),
        ("pz", (0, 0, 1), (0, 1, 0)),
        ("nz", (0, 0, -1), (0, 1, 0)),
    ]
    output = Path(source["sea_capture_output"]) / "WaterMask"
    output.mkdir(exist_ok=True)
    previous_scene = bpy.context.window.scene
    preferences = bpy.context.preferences.addons["cycles"].preferences
    compute = preferences.compute_device_type
    devices = {device.id: device.use for device in preferences.devices}
    try:
        preferences.compute_device_type = "CUDA"
        preferences.get_devices()
        for device in preferences.devices:
            device.use = device.type == "CUDA"
        bpy.context.window.scene = stage
        for name, direction, up in faces:
            forward = rotation @ basis @ Vector(direction)
            vertical = rotation @ basis @ Vector(up)
            camera.rotation_euler = (
                Matrix((forward.cross(vertical), vertical, -forward))
                .transposed()
                .to_euler()
            )
            stage.render.filepath = str(output / (name + ".png"))
            bpy.ops.render.render(write_still=True, scene=stage.name)
            (output / "progress.json").write_text(
                json.dumps({"last_face": name, "size": 512}), encoding="utf-8"
            )
    finally:
        bpy.context.window.scene = previous_scene
        for device in preferences.devices:
            if device.id in devices:
                device.use = devices[device.id]
        preferences.compute_device_type = compute
    print("Water mask: six 512-pixel faces completed")


main()

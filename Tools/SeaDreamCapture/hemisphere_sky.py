"""Replace the capture scene's flat sky backdrops with an inward sky hemisphere."""

import json
import math
from pathlib import Path

import bpy
from mathutils import Vector


def main() -> None:
    stage = bpy.data.scenes["Dream sea midpoint capture"]
    reference = bpy.data.collections["Room alignment reference - hidden for capture"]
    replaced = {"平面.001", "平面.002", "立方体"}
    reference_objects = set(reference.all_objects)
    environment = [
        obj
        for obj in stage.objects
        if obj.name not in replaced and obj not in reference_objects
    ]
    # Give the capture its own collection links; original artist objects remain untouched.
    for collection in list(stage.collection.children):
        stage.collection.children.unlink(collection)
    for obj in list(stage.collection.objects):
        stage.collection.objects.unlink(obj)
    collection = bpy.data.collections.new("Sea environment with hemisphere sky")
    stage.collection.children.link(collection)
    stage.collection.children.link(reference)
    for obj in environment:
        collection.objects.link(obj)
    center = Vector(stage["sea_capture_midpoint"])
    center.z -= 2
    radius, segments, rings = 700.0, 128, 48
    vertices: list[tuple[float, float, float]] = []
    faces: list[tuple[int, int, int, int]] = []
    for ring in range(rings + 1):
        theta = ring / rings * math.pi / 2
        for segment in range(segments + 1):
            phi = segment / segments * 2 * math.pi
            vertices.append(
                (
                    radius * math.sin(theta) * math.cos(phi),
                    radius * math.sin(theta) * math.sin(phi),
                    radius * math.cos(theta),
                )
            )
    for ring in range(rings):
        for segment in range(segments):
            a = ring * (segments + 1) + segment
            # Inward-facing quads. Duplicate seam vertices preserve wrap continuity.
            faces.append((a, a + 1, a + segments + 2, a + segments + 1))
    mesh = bpy.data.meshes.new("Upper hemisphere inward sky mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    uv = mesh.uv_layers.new(name="Sky only - upper preview panorama")
    for polygon in mesh.polygons:
        polygon.use_smooth = True
        for loop_index in polygon.loop_indices:
            vertex = mesh.loops[loop_index].vertex_index
            ring, segment = divmod(vertex, segments + 1)
            uv.data[loop_index].uv = (
                segment / segments,
                0.675 + (1 - ring / rings) * 0.325,
            )
    dome = bpy.data.objects.new("Hemispherical sky cover - 700m radius", mesh)
    collection.objects.link(dome)
    dome.location = center
    dome.visible_shadow = False
    dome.visible_diffuse = False
    material = bpy.data.materials.new("Hemisphere sky - blue clouds emission")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    nodes.clear()
    texture = nodes.new("ShaderNodeTexImage")
    texture.image = bpy.data.images["NoEmotion-Dayhdr_Day_0003_Preview.jpg"]
    texture.extension = "REPEAT"
    emission = nodes.new("ShaderNodeEmission")
    emission.inputs["Strength"].default_value = 1
    output = nodes.new("ShaderNodeOutputMaterial")
    material.node_tree.links.new(texture.outputs["Color"], emission.inputs["Color"])
    material.node_tree.links.new(emission.outputs[0], output.inputs["Surface"])
    dome.data.materials.append(material)
    world = stage.world.copy()
    world.name = "Sea capture neutral sky ambient"
    stage.world = world
    background = world.node_tree.nodes.get("Background")
    background.inputs["Color"].default_value = (0.22, 0.32, 0.45, 1)
    background.inputs["Strength"].default_value = 0.35
    report = {
        "shape": "upper hemisphere",
        "radius_m": radius,
        "center_blender": list(center),
        "replaced_capture_backdrops": sorted(replaced),
        "source_scene_backdrops_preserved": True,
        "texture": texture.image.name,
        "uv_v_range": [0.675, 1],
        "casts_shadows": False,
        "room_hidden_for_capture": reference.hide_render,
    }
    path = Path(stage["sea_capture_output"])
    (path / "hemisphere-sky.json").write_text(
        json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    old = bpy.context.window.scene
    try:
        bpy.context.window.scene = stage
        bpy.ops.wm.save_as_mainfile(
            filepath=str(path / "SeaMidpointAligned.blend"), copy=True
        )
    finally:
        bpy.context.window.scene = old
    print(json.dumps(report, ensure_ascii=False))


main()

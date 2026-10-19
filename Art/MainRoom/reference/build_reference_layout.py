"""Freeze reference B at frame 133, retaining accepted A terminal without changing sources.

Blender --background --threads 2 --python Art/MainRoom/reference/build_reference_layout.py
"""
from __future__ import annotations

import hashlib
import importlib.util
import json
import math
import re
import colorsys
import sys
from array import array
from pathlib import Path

import bmesh
import bpy
from mathutils import Matrix, Vector


def world_bounds(obj: bpy.types.Object) -> tuple[list[float], list[float]]:
    points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    return ([min(point[i] for point in points) for i in range(3)],
            [max(point[i] for point in points) for i in range(3)])


def clip_mesh(obj: bpy.types.Object, minimum: tuple[float, float, float],
              maximum: tuple[float, float, float]) -> None:
    """Clip oversized source shell while retaining source UV coordinates and materials."""
    transform = obj.matrix_world.copy()
    mesh = bmesh.new()
    mesh.from_mesh(obj.data)
    bmesh.ops.transform(mesh, matrix=transform, verts=list(mesh.verts))
    for axis in range(3):
        for value, sign in ((minimum[axis], -1), (maximum[axis], 1)):
            normal = Vector((0, 0, 0))
            normal[axis] = sign
            point = Vector((0, 0, 0))
            point[axis] = value
            bmesh.ops.bisect_plane(mesh, geom=list(mesh.verts) + list(mesh.edges) + list(mesh.faces),
                                   dist=.00001, plane_co=point, plane_no=normal,
                                   clear_outer=True, clear_inner=False)
    mesh.to_mesh(obj.data)
    mesh.free()
    obj.matrix_world = Matrix.Identity(4)
    obj.data.update()


def cube(name: str, minimum: tuple[float, float, float], maximum: tuple[float, float, float],
         material: bpy.types.Material) -> bpy.types.Object:
    centre = Vector([(minimum[i] + maximum[i]) / 2 for i in range(3)])
    bpy.ops.mesh.primitive_cube_add(size=1, location=centre)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = Vector([maximum[i] - minimum[i] for i in range(3)])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    # Doorway reveal UVs use world metres rather than stretched whole-wall mapping.
    for polygon in obj.data.polygons:
        normal = polygon.normal
        axes = (1, 2) if abs(normal.x) > .5 else ((0, 2) if abs(normal.y) > .5 else (0, 1))
        for loop_index in polygon.loop_indices:
            co = obj.data.vertices[obj.data.loops[loop_index].vertex_index].co + centre
            obj.data.uv_layers.active.data[loop_index].uv = (co[axes[0]], co[axes[1]])
    return obj


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    destination = Path(__file__).parent
    source_b = root / "Art/MainRoom2/minigame员工宿舍.blend"
    source_a = root / "Art/MainRoom/combined/exports/MainRoomCombined.glb"
    spec = importlib.util.spec_from_file_location("room_materials", root / "Art/MainRoom/scripts/combine_rooms.py")
    assert spec is not None and spec.loader is not None
    helper = importlib.util.module_from_spec(spec)
    sys.dont_write_bytecode = True
    spec.loader.exec_module(helper)
    bpy.ops.wm.open_mainfile(filepath=str(source_b))
    bpy.context.scene.frame_set(133)
    bpy.context.view_layer.update()
    minimum = (-2.05606, -.69033, -.72788)
    maximum = (7.64472, 3.72143, 2.39278)
    shell_names = {"Wall__3_", "Wall__2_", "Wall__2_.001", "Wall__2_.002", "Wall__2_.003",
                   "Wall__3_.001", "Awal", "Floor", "Awall2", "Awall3"}
    old_posters = {"平面." + str(i).zfill(3) for i in range(15, 26)}
    excluded = old_posters | {"door", "tijiwu", "平面.013", "平面.014", "立方体.014",
                              "3DGeom~1_Defintion.001", "AAAhuman", "C_mouse_low_blinn15_0",
                              "C_monitor01_low_blinn14_0", "C_monitor01_low_blinn14_0.001",
                              "C_keyboard_low_blinn2_0", "C_conectionPlate_low_lambert1_0",
                              "Obj3d66-790374-57-30"}
    retained = [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and obj.name not in excluded]
    originals: list[dict[str, object]] = []
    original_names: dict[bpy.types.Object, str] = {}
    for obj in retained:
        name = obj.name
        original_names[obj] = name
        originals.append({"object": "B_" + name, "original": name,
                          "matrix": [list(row) for row in obj.matrix_world],
                          "bounds": world_bounds(obj)})
        world = obj.matrix_world.copy()
        obj.animation_data_clear()
        obj.constraints.clear()
        obj.parent = None
        obj.matrix_world = world
    for obj in list(bpy.data.objects):
        if obj not in retained:
            bpy.data.objects.remove(obj, do_unlink=True)
    cache: dict[str, bpy.types.Material] = {}
    wood = bpy.data.materials["Lime._-_019"]
    # The source floor uses a nested Poliigon material, which the shallow helper cannot trace.
    # Preserve its actual color map, original HSV settings and original UV mapping explicitly.
    floor_source = bpy.data.materials["木地板014"]
    floor_group = floor_source.node_tree.nodes["WoodFlooring14_6K"]
    floor_material = bpy.data.materials.new("B | Reference WoodFlooring14")
    floor_material.use_nodes = True
    floor_shader = floor_material.node_tree.nodes.get("Principled BSDF")
    floor_shader.inputs["Roughness"].default_value = .62
    floor_image = floor_group.node_tree.nodes["COLOR"].image.copy()
    floor_image.name = "ReferenceFloor_BaseColor_HSV"
    floor_image.scale(2048, 2048)
    pixels = array("f", [0]) * (2048 * 2048 * 4)
    floor_image.pixels.foreach_get(pixels)
    saturation = floor_group.inputs["Saturation"].default_value
    value = floor_group.inputs["Value"].default_value
    for i in range(0, len(pixels), 4):
        h, s, v = colorsys.rgb_to_hsv(pixels[i], pixels[i + 1], pixels[i + 2])
        pixels[i], pixels[i + 1], pixels[i + 2] = colorsys.hsv_to_rgb(h, min(1, s * saturation), v * value)
    floor_image.pixels.foreach_set(pixels)
    floor_image.update()
    floor_image.pack()
    texture = floor_material.node_tree.nodes.new("ShaderNodeTexImage")
    texture.image = floor_image
    floor_material.node_tree.links.new(texture.outputs["Color"], floor_shader.inputs["Base Color"])
    normal_image = floor_group.node_tree.nodes["NORMAL"].image.copy()
    normal_image.name = "ReferenceFloor_Normal"
    normal_image.colorspace_settings.name = "Non-Color"
    normal_texture = floor_material.node_tree.nodes.new("ShaderNodeTexImage")
    normal_texture.image = normal_image
    normal_node = floor_material.node_tree.nodes.new("ShaderNodeNormalMap")
    floor_material.node_tree.links.new(normal_texture.outputs["Color"], normal_node.inputs["Color"])
    floor_material.node_tree.links.new(normal_node.outputs["Normal"], floor_shader.inputs["Normal"])
    glass = bpy.data.materials.new("CombinedWindowGlass")
    glass.use_nodes = True
    glass.surface_render_method = "BLENDED"
    shader = glass.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (.19, .23, .22, .12)
    shader.inputs["Alpha"].default_value = .12
    shader.inputs["Roughness"].default_value = .3
    for obj in retained:
        name = original_names[obj]
        for slot in obj.material_slots:
            source = slot.material
            if source is None:
                continue
            if source.name == "KB3D_CSZ_GlassMetallicBlue":
                slot.material = glass
            elif source == floor_source:
                slot.material = floor_material
            else:
                slot.material = helper.portable_material(wood if source.name == "Material 0" else source, cache)
        obj.hide_render = False
        obj.hide_viewport = False
        obj.hide_set(False)
        obj.name = ("B_ExteriorConcrete_" if any(slot.material and "KB3D_HAN_ConcreteB" in slot.material.name
                    for slot in obj.material_slots) else "B_") + name
        obj["room_source"] = "MainRoom2 frame 133"
        if name in shell_names and name != "Awall3":
            low = (minimum[0] - .2, minimum[1] - .2, minimum[2])
            high = (maximum[0] + .2, maximum[1] + .2, maximum[2])
            if name in {"Floor", "Awal"}:
                low = (minimum[0], minimum[1], minimum[2] - .0001)
                high = (maximum[0], maximum[1], maximum[2] + .0001)
            clip_mesh(obj, low, high)
            if name == "Floor":
                mapping = floor_source.node_tree.nodes["SimpleUVMapping"]
                scale = mapping.inputs["Scale"].default_value
                aspect = mapping.inputs["Aspect Ratio"].default_value
                for uv in obj.data.uv_layers.active.data:
                    uv.uv.x *= scale
                    uv.uv.y *= scale * aspect
        elif name == "平面" or re.fullmatch(r"平面\.0(?:0[1-9]|1[01])", name):
            clip_mesh(obj, (minimum[0] - .22, minimum[1] - .2, minimum[2]),
                      (maximum[0], maximum[1] + .2, maximum[2]))
    # Original right wall is solid. Build a genuine opening with the same material.
    old_right = next(obj for obj, name in original_names.items() if name == "Awall3")
    right_material = old_right.data.materials[0]
    bpy.data.objects.remove(old_right, do_unlink=True)
    door_y = 2.82488
    door_bottom = minimum[2]
    door_height = 2.5
    for name, lower, upper in [
        ("B_ReferenceDoorWallLowerSide", (maximum[0], minimum[1], minimum[2]),
         (maximum[0] + .2, door_y - .75, maximum[2])),
        ("B_ReferenceDoorWallUpperSide", (maximum[0], door_y + .75, minimum[2]),
         (maximum[0] + .2, maximum[1], maximum[2])),
        ("B_ReferenceDoorWallLintel", (maximum[0], door_y - .75, door_bottom + door_height),
         (maximum[0] + .2, door_y + .75, maximum[2])),
    ]:
        cube(name, lower, upper, right_material)
    # Keep the accepted CRT, keyboard, mouse, base unit and power cable only.
    previous = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(source_a))
    accepted = {"CRT screen emission", "CRT_Monitor_Tripo", "A_TerminalAccessories", "Computer power cable"}
    terminal: list[bpy.types.Object] = []
    for obj in set(bpy.data.objects) - previous:
        name = re.sub(r"\.\d+$", "", obj.name)
        if name not in accepted:
            bpy.data.objects.remove(obj, do_unlink=True)
            continue
        obj.parent = None
        obj.name = name
        terminal.append(obj)
    transform = (Matrix.Translation(Vector((3.004, -.36, .1045))) @ Matrix.Rotation(math.pi / 2, 4, "Z")
                 @ Matrix.Translation(Vector((2.51, 1.1, -.79))))
    for obj in terminal:
        obj.matrix_world = transform @ obj.matrix_world
        obj["room_source"] = "Accepted A terminal translated to original B workstation"
    bpy.context.view_layer.update()
    screen = next(obj for obj in terminal if obj.name == "CRT screen emission")
    screen_min, screen_max = world_bounds(screen)
    screen_centre = [(screen_min[i] + screen_max[i]) / 2 for i in range(3)]
    # Move two small desktop props out of the retained A base-unit footprint.
    prop_adjustments: list[dict[str, object]] = []
    for name, target in {"B_SM_Bottle_Set1.002": (2.65, -.53, .1045),
                         "B_SM_OldTinContainer01": (2.25, -.1, .1045)}.items():
        obj = bpy.data.objects[name]
        lower, upper = world_bounds(obj)
        centre_bottom = Vector(((lower[0] + upper[0]) / 2, (lower[1] + upper[1]) / 2, lower[2]))
        obj.location += Vector(target) - centre_bottom
        prop_adjustments.append({"object": name, "targetBottomCentre": target,
                                 "reason": "Clear original B desktop for accepted A terminal"})
    markers: dict[str, list[float] | tuple[float, float, float]] = {
        "ReferenceOrigin": minimum,
        "ReferenceRight": (minimum[0] + 1, minimum[1], minimum[2]),
        "ReferenceDepth": (minimum[0], minimum[1] + 1, minimum[2]),
        "ReferenceUp": (minimum[0], minimum[1], minimum[2] + 1),
        "DoorCentre": (maximum[0] + .1, door_y, minimum[2] + 1.25),
        "DoorHinge": (maximum[0] + .1, door_y + .75, minimum[2]),
        "DoorApproach": (maximum[0] - 1.05, door_y, minimum[2] + .055),
        "ScreenCentre": screen_centre,
        "ComputerApproach": (3.004, 1.15, minimum[2] + .055),
        "SeatEye": (3.004, screen_centre[1] + 1.05, screen_centre[2] + .18),
        "TerminalAlign": (3.004, screen_centre[1] + .62, screen_centre[2]),
        "BedCentre": (-.8358, .3634, -.11411),
        "BedApproach": (.25, .45, minimum[2] + .055),
        "Spawn": (5.9, 1.75, minimum[2] + .055),
        "PosterBeach": (-.8358, minimum[1] + .025, .90),
        "PosterDevice": (4.25, minimum[1] + .025, .90),
        "CeilingLight": (2.26, 1.71, 2.15),
        "WindowLight": (-1.75, 1.55, 1.52),
        "MonitorLight": (screen_centre[0], screen_centre[1] + .12, screen_centre[2]),
        "FillLight": (5.7, 2.2, 1.65),
        "ExteriorLight": (-3.8, 1.55, 2.0),
        "PreviewPosition": (7.1, 2.9, .89),
        "PreviewTarget": (.1, .6, .35),
        "WindowDetailPosition": (-.4, 1.55, 1.15),
        "WindowDetailTarget": (-5.8, 1.55, 1.15),
    }
    layout = {
        "version": 1, "coordinateConvention": "Blender world XYZ, Z up; exported FBX converts to Unity Y up",
        "sourceFrame": 133, "interiorMinimum": minimum, "interiorMaximum": maximum,
        "interiorSize": [maximum[i] - minimum[i] for i in range(3)],
        "horizontalFieldOfView": 61.9275,
        "door": {"width": 1.5, "height": 2.5, "thickness": .08, "outwardAxis": [1, 0, 0]},
        "markers": markers,
    }
    (destination / "layout.json").write_text(json.dumps(layout, ensure_ascii=False, indent=2), encoding="utf-8")
    bpy.context.view_layer.update()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    # Remove unused source images including both original 8K human textures.
    bpy.data.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=True)
    image_sizes: list[dict[str, object]] = []
    for image in bpy.data.images:
        if not image.users or not image.size[0]:
            continue
        original_size = list(image.size)
        if max(image.size) > 2048:
            ratio = 2048 / max(image.size)
            image.scale(round(image.size[0] * ratio), round(image.size[1] * ratio))
        image_sizes.append({"name": image.name, "sourceSize": original_size, "exportSize": list(image.size)})
        if image.source == "FILE" and not image.packed_file:
            image.pack()
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    scene = bpy.context.scene
    scene.name = "MainRoom_Reference133"
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1
    bpy.ops.export_scene.gltf(filepath=str(destination / "MainRoomReference.glb"), export_format="GLB",
                              use_selection=True, export_apply=True, export_extras=True,
                              export_cameras=False, export_lights=False)
    bpy.ops.wm.save_as_mainfile(filepath=str(destination / "MainRoomReference.blend"), compress=True)
    bpy.context.view_layer.update()
    report = {
        "inputs": [{"path": str(path.relative_to(root)), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
                   for path in (source_a, source_b)],
        "sourceFrame": 133, "layout": layout, "originalTransforms": originals,
        "excludedSourceObjects": sorted(excluded), "desktopPropAdjustments": prop_adjustments,
        "exportedObjects": [{"name": obj.name, "bounds": world_bounds(obj), "vertices": len(obj.data.vertices)}
                            for obj in meshes],
        "textures": image_sizes,
        "meshCount": len(meshes),
        "triangles": sum(sum(len(face.vertices) - 2 for face in obj.data.polygons) for obj in meshes),
        "scope": "Art geometry/coordinate validation only; Unity gameplay verified separately",
    }
    assert len([obj for obj in meshes if obj.name.startswith("B_ExteriorConcrete_")]) == 2
    for suffix in ("SM_bed_1", "SM_mattress_1", "SM_pillow_1", "SM_rug_2"):
        assert bpy.data.objects.get("B_" + suffix) is not None
    (destination / "geometry-report.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print("REFERENCE_LAYOUT_EXPORTED", json.dumps({"meshCount": len(meshes), "triangles": report["triangles"],
                                                   "screenCentre": screen_centre, "size": layout["interiorSize"]}))


if __name__ == "__main__":
    main()

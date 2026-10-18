"""Compose the two supplied rooms without modifying either input.

Run from the repository with Blender --background --threads 2 --python this-file.
"""
from __future__ import annotations

import hashlib
import json
import math
import re
import struct
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


def base_name(name: str) -> str:
    return re.sub(r"\.\d+$", "", name)


def upstream_image(socket: bpy.types.NodeSocket) -> tuple[bpy.types.Image | None, str]:
    pending = [(link.from_node, link.from_socket.name) for link in socket.links]
    seen: set[str] = set()
    while pending:
        node, channel = pending.pop(0)
        if node.name in seen:
            continue
        seen.add(node.name)
        if node.type == "TEX_IMAGE" and node.image and all(node.image.size):
            return node.image, channel
        for input_socket in node.inputs:
            for link in input_socket.links:
                pending.append((link.from_node, channel if node.type == "SEPARATE_COLOR" else link.from_socket.name))
    return None, ""


def portable_material(source: bpy.types.Material, cache: dict[str, bpy.types.Material]) -> bpy.types.Material:
    if source.name in cache:
        return cache[source.name]
    material = bpy.data.materials.new("B | " + source.name)
    material.use_nodes = True
    material.use_backface_culling = False
    cache[source.name] = material
    shader = material.node_tree.nodes.get("Principled BSDF")
    old = next((node for node in source.node_tree.nodes if node.type == "BSDF_PRINCIPLED"), None)
    if old is None:
        raise ValueError("Selected material has no Principled surface: " + source.name)
    for input_name in ("Base Color", "Roughness", "Metallic"):
        shader.inputs[input_name].default_value = old.inputs[input_name].default_value
        image, channel = upstream_image(old.inputs[input_name])
        if image is None:
            continue
        texture = material.node_tree.nodes.new("ShaderNodeTexImage")
        texture.image = image
        if input_name == "Base Color":
            image.colorspace_settings.name = "sRGB"
            material.node_tree.links.new(texture.outputs["Color"], shader.inputs[input_name])
        else:
            image.colorspace_settings.name = "Non-Color"
            separate = material.node_tree.nodes.new("ShaderNodeSeparateColor")
            material.node_tree.links.new(texture.outputs["Color"], separate.inputs["Color"])
            component = channel if channel in {"Red", "Green", "Blue"} else "Red"
            material.node_tree.links.new(separate.outputs[component], shader.inputs[input_name])
    image, _ = upstream_image(old.inputs["Normal"])
    if image and "normal" in image.name.lower():
        texture = material.node_tree.nodes.new("ShaderNodeTexImage")
        texture.image = image
        image.colorspace_settings.name = "Non-Color"
        normal = material.node_tree.nodes.new("ShaderNodeNormalMap")
        normal.inputs["Strength"].default_value = .55
        material.node_tree.links.new(texture.outputs["Color"], normal.inputs["Color"])
        material.node_tree.links.new(normal.outputs["Normal"], shader.inputs["Normal"])
    return material


def transform_group(objects: list[bpy.types.Object], origin: tuple[float, float, float],
                    target: tuple[float, float, float], rotation: float, scale: float) -> None:
    transform = (Matrix.Translation(Vector(target)) @ Matrix.Rotation(rotation, 4, "Z")
                 @ Matrix.Scale(scale, 4) @ Matrix.Translation(-Vector(origin)))
    for obj in objects:
        world = obj.matrix_world.copy()
        obj.parent = None
        obj.matrix_world = transform @ world


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    source_a = root / "Art/MainRoom/exports/MainRoom.glb"
    source_b = root / "Art/MainRoom2/minigame员工宿舍.blend"
    destination = root / "Art/MainRoom/combined"
    scratch = root / "Artifacts/Unity/RoomMerge"
    for folder in ("scene", "exports", "reports"):
        (destination / folder).mkdir(parents=True, exist_ok=True)
    scratch.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=str(source_b))
    groups = {
        "Bed": ["SM_bed_1", "SM_mattress_1", "SM_pillow_1", "SM_rug_2", "SM_cardboard_box_5"],
        "WorkFurniture": ["obj_01", "HT01", "HT01.001"],
        "WorkProps": ["SM_cup_2", "SM_CigaretteBox02", "SM_CigaretteBox01", "SM_Bottle_Set1.002",
                      "SM_MERGED_SM_book_16.002", "SM_MERGED_SM_book_16.004"],
        "Cabinet": ["SM_CupboardChunk", "SM_MERGED_SM_book_16.001", "SM_Bottle_Set1",
                    "SM_Bottle_Set1.003", "SM_SingleBox", "SM_MovingBoxes"],
    }
    groups["ExteriorConcrete"] = [obj.name for obj in bpy.context.scene.objects if obj.type == "MESH"
                                  and any(slot.material and slot.material.name == "KB3D_HAN_ConcreteB"
                                          for slot in obj.material_slots)]
    if len(groups["ExteriorConcrete"]) != 2:
        raise ValueError("Expected the source exterior concrete wall and ledge")
    selected = {name for names in groups.values() for name in names}
    missing = selected - set(bpy.data.objects.keys())
    if missing:
        raise ValueError("Missing source objects: " + str(sorted(missing)))
    original_names = {obj: obj.name for obj in bpy.context.scene.objects if obj.name in selected}
    worlds = {obj: obj.matrix_world.copy() for obj in original_names}
    frozen_animations = [obj.name for obj in original_names if obj.animation_data]
    for obj, world in worlds.items():
        obj.animation_data_clear()
        obj.constraints.clear()
        obj.parent = None
        obj.matrix_world = world
    for obj in list(bpy.data.objects):
        if obj not in original_names:
            bpy.data.objects.remove(obj, do_unlink=True)
    wood = bpy.data.materials["Lime._-_019"]
    cache: dict[str, bpy.types.Material] = {}
    for obj in original_names:
        for slot in obj.material_slots:
            source = wood if slot.material.name == "Material 0" else slot.material
            slot.material = portable_material(source, cache)
        obj.hide_render = False
        obj.hide_viewport = False
        obj.hide_set(False)
    for group, names in groups.items():
        objects = [bpy.data.objects[name] for name in names]
        if group == "Bed":
            transform_group(objects, (-.8358, .3634, -.7279), (-1.65, 2.12, 0), -math.pi / 2, 1)
        elif group in {"WorkFurniture", "WorkProps"}:
            transform_group(objects, (3.004, -.287, -.729), (-2.39, -.70, 0), -math.pi / 2, .94)
        elif group == "ExteriorConcrete":
            transform_group(objects, (-2.15, 1.551, -.728), (1.1, 3, 0), -math.pi / 2, 1)
        else:
            transform_group(objects, (.665, 3.381, -.728), (2.39, .88, 0), -math.pi / 2, 1.1)
        collection = bpy.data.collections.new("B | " + group)
        bpy.context.scene.collection.children.link(collection)
        for obj in objects:
            for previous in list(obj.users_collection):
                previous.objects.unlink(obj)
            collection.objects.link(obj)
            obj.name = ("B_ExteriorConcrete_" if group == "ExteriorConcrete" else "B_") + original_names[obj]
    # The new terminal is retained from A, so B's desk props need a clear screen sightline.
    for name, target in {"SM_Bottle_Set1.002": (-2.20, -.05, .783), "SM_cup_2": (-2.60, -.45, .783),
                         "SM_CigaretteBox01": (-2.68, -1.40, .783), "SM_CigaretteBox02": (-2.12, -1.30, .783)}.items():
        obj = next(obj for obj, original in original_names.items() if original == name)
        bpy.context.view_layer.update()
        points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
        current = Vector(((min(p.x for p in points) + max(p.x for p in points)) / 2,
                          (min(p.y for p in points) + max(p.y for p in points)) / 2,
                          min(p.z for p in points)))
        obj.location += Vector(target) - current

    # Remove invalid normals in the interchange source before using it in Blender.
    raw = source_a.read_bytes()
    size = struct.unpack_from("<I", raw, 12)[0]
    doc = json.loads(raw[20:20 + size])
    for material in doc["materials"]:
        if "normalTexture" in material:
            texture = doc["textures"][material["normalTexture"]["index"]]
            if "normal" not in doc["images"][texture["source"]].get("name", "").lower():
                material.pop("normalTexture")
        for value in list(material.values()) + list(material.get("pbrMetallicRoughness", {}).values()):
            if isinstance(value, dict) and value.get("texCoord", 0) < 0:
                value["texCoord"] = 0
    encoded = json.dumps(doc, separators=(",", ":")).encode()
    encoded += b" " * (-len(encoded) % 4)
    rest = raw[20 + size:]
    prepared = scratch / "source-a-portable.glb"
    prepared.write_bytes(struct.pack("<4sII", b"glTF", 2, 20 + len(encoded) + len(rest))
                         + struct.pack("<I4s", len(encoded), b"JSON") + encoded + rest)
    previous_objects = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(prepared))
    imported = set(bpy.data.objects) - previous_objects
    glass = bpy.data.materials.new("CombinedWindowGlass")
    glass.use_nodes = True
    glass.surface_render_method = "BLENDED"
    glass_shader = glass.node_tree.nodes.get("Principled BSDF")
    glass_shader.inputs["Base Color"].default_value = (.16, .20, .19, .12)
    glass_shader.inputs["Alpha"].default_value = .12
    glass_shader.inputs["Roughness"].default_value = .35
    replaced = {"Desk top", "Desk pedestal", "Desk right support", "Desk rear modesty panel", "Desk shallow drawer",
                "Shallow drawer pull", "Back upright", "Back cross slat", "Flattened pillow", "Thin mattress",
                "Rumpled blanket", "Side cabinet body", "Side cabinet door", "Side cabinet top", "Cabinet inset panel",
                "Cabinet small handle", "Stacked book", "Book page block"}
    removed_a: list[str] = []
    join_groups: dict[str, list[bpy.types.Object]] = {}
    for obj in imported:
        name = base_name(obj.name)
        if (name in replaced or name.startswith(("Drawer ", "Chair ", "Mattress ", "Bed end ", "Bed rounded ",
                                                  "Bed side ", "Bed vertical ", "Desk bottle ", "Cabinet bottle ",
                                                  "Mug ", "Storage box"))):
            removed_a.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)
            continue
        if name == "Parquet | individual boards":
            floor_material = portable_material(wood, cache)
            for slot in obj.material_slots:
                slot.material = floor_material
        if name == "Fogged window pane":
            for slot in obj.material_slots:
                slot.material = glass
        if name.startswith(("Carpet fringe", "Worn carpet")):
            category = "A_Rug"
        elif name.startswith(("Keyboard ", "Space bar", "Mouse", "Computer base", "Diskette", "Case status")):
            category = "A_TerminalAccessories"
        elif name.startswith(("Cast iron radiator", "Radiator ", "Exposed heating")):
            category = "A_Heating"
        elif name.startswith(("Window ", "Wide chipped sill", "Fogged window", "Curtain rod")):
            category = "A_Window"
        elif name.startswith(("Front ", "Entrance ")):
            category = "Front closure"
        else:
            category = ""
        if category and obj.type == "MESH":
            join_groups.setdefault(category, []).append(obj)
    # Keep furniture and source posters editable; combine only static tiny details.
    for category, objects in join_groups.items():
        bpy.ops.object.select_all(action="DESELECT")
        for obj in objects:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = objects[0]
        bpy.ops.object.join()
        bpy.context.object.name = category
    for obj in bpy.context.scene.objects:
        if obj.type == "MESH":
            obj["room_source"] = "MainRoom2" if obj.name.startswith("B_") else "MainRoom"
    bpy.data.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=True)
    scene = bpy.context.scene
    scene.name = "MainRoom_Combined"
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 12
    scene.cycles.use_denoising = True
    world = bpy.data.worlds.new("Combined room ambient")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (.17, .20, .18, 1)
    world.node_tree.nodes["Background"].inputs[1].default_value = .20
    scene.world = world
    for name, position, color, energy, diameter in [
        ("Ceiling practical", (.35, -.85, 2.51), (.70, .82, .69), 105, 1.3),
        ("Window fill", (1.05, 2.65, 2.05), (.47, .58, .72), 95, 1.25),
        ("Soft room fill", (1.3, -2.8, 2.2), (.66, .69, .60), 32, 3.0),
    ]:
        light = bpy.data.lights.new(name, "AREA")
        light.energy, light.color, light.shape, light.size = energy, color, "DISK", diameter
        obj = bpy.data.objects.new(name, light)
        scene.collection.objects.link(obj)
        obj.location = position
        obj.rotation_euler = (Vector((-.3, .5, .5)) - obj.location).to_track_quat("-Z", "Y").to_euler()
    camera_data = bpy.data.cameras.new("Combined reference camera")
    camera = bpy.data.objects.new("Combined reference camera", camera_data)
    scene.collection.objects.link(camera)
    camera.location = (-.1, -3.9469, 1.2546)
    yaw, pitch = -.1784, -.05288
    direction = Vector((math.sin(yaw) * math.cos(pitch), math.cos(yaw) * math.cos(pitch), math.sin(pitch)))
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera_data.lens, camera_data.clip_start = 18.1924, .03
    scene.camera = camera
    scene.render.resolution_x, scene.render.resolution_y = 1280, 720
    scene.render.resolution_percentage = 100
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.update()
    placements = []
    for obj, original_name in original_names.items():
        points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
        minimum = [min(point[index] for point in points) for index in range(3)]
        maximum = [max(point[index] for point in points) for index in range(3)]
        exterior = original_name in groups["ExteriorConcrete"]
        if exterior and minimum[1] < 3.1:
            raise ValueError("Exterior concrete intrudes into the room: " + obj.name)
        if not exterior and (minimum[0] < -2.81 or maximum[0] > 2.81 or minimum[1] < -3.41 or maximum[1] > 3.01 or minimum[2] < -.015):
            raise ValueError(f"Combined object outside room/floor: {obj.name} {minimum} {maximum}")
        if original_name in groups["WorkProps"] and minimum[2] < .75:
            raise ValueError("Desktop prop is below the desk: " + obj.name)
        placements.append({"object": obj.name, "bounds_min": minimum, "bounds_max": maximum, "exterior": exterior})
    meshes = [obj for obj in scene.objects if obj.type == "MESH"]
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.gltf(filepath=str(destination / "exports/MainRoomCombined.glb"),
                              export_format="GLB", use_selection=True, export_cameras=False,
                              export_lights=False, export_apply=True, export_extras=True)
    for obj in meshes:
        if obj.name.startswith("Front closure"):
            obj.hide_render = True
    for image in bpy.data.images:
        if image.users and image.source == "FILE" and not image.packed_file:
            image.pack()
    bpy.ops.wm.save_as_mainfile(filepath=str(destination / "scene/MainRoomCombined.blend"), compress=True)
    report = {
        "inputs": [{"path": str(path.relative_to(root)), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
                   for path in (source_a, source_b)],
        "source_a_retained": ["room shell and original poster graphics", "window/curtain and heating", "CRT and low-poly input peripherals", "central rug and loose newspaper"],
        "source_b_retained": groups,
        "source_b_relocated_to_a_layout": True,
        "source_b_frozen_object_animations": frozen_animations,
        "source_b_placement_checks": placements,
        "window_exterior": "Original MainRoom2 concrete wall and ledge, outside the window; transparent glass preserves depth",
        "work_surface_adjustment": "Bottle, cup and packets repositioned to keep the retained CRT visible",
        "source_a_replaced_objects": sorted(removed_a),
        "static_detail_groups": list(join_groups),
        "wood_surface_source": "MainRoom2: Lime._-_019 / KB3D_GAE_WoodA",
        "mesh_objects": len(meshes),
        "triangles": sum(sum(len(face.vertices) - 2 for face in obj.data.polygons) for obj in meshes),
        "scope": "Combined editable art and Unity preview; no gameplay/collider/performance acceptance",
        "render_preview_front_closure_hidden": True,
    }
    (destination / "reports/composition.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print("ROOM_COMBINED", json.dumps({"meshes": len(meshes), "source_b_objects": len(selected),
                                        "triangles": report["triangles"]}), flush=True)


if __name__ == "__main__":
    main()

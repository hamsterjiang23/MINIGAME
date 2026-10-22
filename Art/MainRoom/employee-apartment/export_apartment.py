"""Export the supplied apartment, retaining the game's existing corridor boundary.

Run in Blender background mode with --disable-autoexec --threads 2.
The source Blender file is read only; generated interchange files go to Artifacts.
"""
from __future__ import annotations

import argparse
import colorsys
import hashlib
import importlib.util
import json
import sys
from array import array
from pathlib import Path
from types import ModuleType

import bmesh
import bpy
from mathutils import Matrix, Vector


MINIMUM = (-2.05606, -.69033, -.72788)
MAXIMUM = (7.64472, 3.72143, 2.39278)
CORRIDOR = {
    "Awal.001", "SM_MERGED_Attic_Light_02_LP3.002", "立方体.004", "立方体.015",
    "柱体", "柱体.001", "平面.027", "平面.028", "平面.029",
    *[f"Wall__3_.{index:03d}" for index in (2, 4, 5, 6, 7, 8, 9, 10, 11)],
}
PLACEHOLDERS = {"door", "tijiwu", "AAAhuman", "3DGeom~1_Defintion.001", "平面.013", "平面.014"}
SHELLS = {"Floor", "Awal", "Wall__3_", "Wall__3_.001", "Wall__2_", "Wall__2_.001", "Wall__2_.002", "Wall__2_.003"}
ACCESSORIES = {"C_keyboard_low_blinn2_0", "C_mouse_low_blinn15_0", "C_conectionPlate_low_lambert1_0"}


def load_helper(path: Path, name: str) -> ModuleType:
    spec = importlib.util.spec_from_file_location(name, path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load existing room helper: {path}")
    module = importlib.util.module_from_spec(spec)
    sys.dont_write_bytecode = True
    spec.loader.exec_module(module)
    return module


def bounds(obj: bpy.types.Object) -> list[list[float]]:
    points = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    return [[round(min(point[axis] for point in points), 6) for axis in range(3)],
            [round(max(point[axis] for point in points), 6) for axis in range(3)]]


def upstream_nodes(socket: bpy.types.NodeSocket) -> list[bpy.types.Node]:
    pending = [link.from_node for link in socket.links]
    seen: set[str] = set()
    result: list[bpy.types.Node] = []
    while pending:
        node = pending.pop(0)
        if node.name in seen:
            continue
        seen.add(node.name)
        result.append(node)
        for item in node.inputs:
            pending.extend(link.from_node for link in item.links)
    return result


def bake_hsv(image: bpy.types.Image, name: str, hue: float, saturation: float,
             value: float, factor: float = 1) -> bpy.types.Image:
    copied = image.copy()
    copied.name = name
    if max(copied.size) > 2048:
        scale = 2048 / max(copied.size)
        copied.scale(round(copied.size[0] * scale), round(copied.size[1] * scale))
    pixels = array('f', [0]) * (copied.size[0] * copied.size[1] * 4)
    copied.pixels.foreach_get(pixels)
    for index in range(0, len(pixels), 4):
        original = pixels[index:index + 3]
        h, s, v = colorsys.rgb_to_hsv(*original)
        corrected = colorsys.hsv_to_rgb((h + hue - .5) % 1, min(1, s * saturation), v * value)
        for channel in range(3):
            pixels[index + channel] = original[channel] * (1 - factor) + corrected[channel] * factor
    copied.pixels.foreach_set(pixels)
    copied.update()
    copied.pack()
    return copied


def prepare_material(source: bpy.types.Material, helper: ModuleType,
                     cache: dict[str, bpy.types.Material], adjustments: list[dict[str, object]]) -> bpy.types.Material:
    if source.name in cache:
        return cache[source.name]
    if source.name == 'KB3D_CSZ_GlassMetallicBlue':
        material = bpy.data.materials.new('CombinedWindowGlass')
        material.use_nodes = True
        material.surface_render_method = 'BLENDED'
        shader = material.node_tree.nodes.get('Principled BSDF')
        shader.inputs['Base Color'].default_value = (.19, .23, .22, .12)
        shader.inputs['Alpha'].default_value = .12
        shader.inputs['Roughness'].default_value = .3
        cache[source.name] = material
        adjustments.append({'material': source.name, 'unityWindowGlass': 'Same transparent window treatment as existing room'})
        return material
    color_spaces = {node.image: node.image.colorspace_settings.name for node in source.node_tree.nodes
                    if node.type == 'TEX_IMAGE' and node.image}
    material = helper.portable_material(source, cache)
    for image, color_space in color_spaces.items():
        image.colorspace_settings.name = color_space
    old = next(node for node in source.node_tree.nodes if node.type == 'BSDF_PRINCIPLED')
    shader = material.node_tree.nodes.get('Principled BSDF')
    for label in ('Emission Color', 'Emission Strength', 'Alpha'):
        shader.inputs[label].default_value = old.inputs[label].default_value
    nodes = upstream_nodes(old.inputs['Base Color'])
    hue_node = next((node for node in nodes if node.type == 'HUE_SAT'), None)
    image_node = next((node for node in nodes if node.type == 'TEX_IMAGE' and node.image), None)
    if hue_node is not None and image_node is not None:
        settings = {label: float(hue_node.inputs[label].default_value) for label in ('Hue', 'Saturation', 'Value', 'Fac')}
        image = bake_hsv(image_node.image, f'Apartment_{source.name}_HSV', settings['Hue'], settings['Saturation'], settings['Value'], settings['Fac'])
        output_image = next(node for node in material.node_tree.nodes if node.type == 'TEX_IMAGE' and node.image == image_node.image)
        output_image.image = image
        adjustments.append({'material': source.name, 'bakedHueSaturation': settings, 'image': image.name})
    roughness_nodes = upstream_nodes(old.inputs['Roughness'])
    ramp = next((node for node in roughness_nodes if node.type == 'VALTORGB'), None)
    roughness_image = next((node for node in roughness_nodes if node.type == 'TEX_IMAGE' and node.image), None)
    if ramp is not None and roughness_image is not None:
        copied = roughness_image.image.copy()
        if max(copied.size) > 2048:
            ratio = 2048 / max(copied.size)
            copied.scale(round(copied.size[0] * ratio), round(copied.size[1] * ratio))
        pixels = array('f', [0]) * (copied.size[0] * copied.size[1] * 4)
        copied.pixels.foreach_get(pixels)
        for index in range(0, len(pixels), 4):
            value = pixels[index] * .2126 + pixels[index + 1] * .7152 + pixels[index + 2] * .0722
            corrected = ramp.color_ramp.evaluate(value)[0]
            pixels[index:index + 4] = array('f', (corrected, corrected, corrected, 1))
        baked = bpy.data.images.new(f'Apartment_{source.name}_Roughness', width=copied.size[0], height=copied.size[1], alpha=False)
        baked.colorspace_settings.name = 'Non-Color'
        baked.pixels.foreach_set(pixels)
        baked.update()
        baked.pack()
        texture = material.node_tree.nodes.new('ShaderNodeTexImage')
        texture.image = baked
        separate = material.node_tree.nodes.new('ShaderNodeSeparateColor')
        material.node_tree.links.new(texture.outputs['Color'], separate.inputs['Color'])
        material.node_tree.links.new(separate.outputs['Red'], shader.inputs['Roughness'])
        adjustments.append({'material': source.name, 'bakedRoughnessRamp': ramp.name})
    if old.inputs['Alpha'].default_value < 1:
        material.surface_render_method = 'BLENDED'
    return material


def floor_material(source: bpy.types.Material) -> bpy.types.Material:
    group = source.node_tree.nodes['WoodFlooring14_6K']
    material = bpy.data.materials.new('B | Apartment WoodFlooring14')
    material.use_nodes = True
    shader = material.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Roughness'].default_value = .62
    color = bake_hsv(group.node_tree.nodes['COLOR'].image, 'ApartmentFloor_BaseColor_HSV', .5,
                     group.inputs['Saturation'].default_value, group.inputs['Value'].default_value)
    texture = material.node_tree.nodes.new('ShaderNodeTexImage')
    texture.image = color
    material.node_tree.links.new(texture.outputs['Color'], shader.inputs['Base Color'])
    normal_image = group.node_tree.nodes['NORMAL'].image.copy()
    normal_image.name = 'ApartmentFloor_Normal'
    normal_image.colorspace_settings.name = 'Non-Color'
    texture = material.node_tree.nodes.new('ShaderNodeTexImage')
    texture.image = normal_image
    normal = material.node_tree.nodes.new('ShaderNodeNormalMap')
    material.node_tree.links.new(texture.outputs['Color'], normal.inputs['Color'])
    material.node_tree.links.new(normal.outputs['Normal'], shader.inputs['Normal'])
    return material


def bake_mapping(obj: bpy.types.Object, slot_index: int, source: bpy.types.Material) -> None:
    if not source.use_nodes or obj.data.uv_layers.active is None:
        return
    node = next((node for node in source.node_tree.nodes if node.type == 'MAPPING'), None)
    if node is None:
        return
    if node.vector_type != 'POINT':
        raise ValueError(f'Unsupported texture coordinate mapping: {source.name} {node.vector_type}')
    location = Vector(node.inputs['Location'].default_value)
    rotation = node.inputs['Rotation'].default_value
    scale = node.inputs['Scale'].default_value
    matrix = (Matrix.Translation(location) @ Matrix.Rotation(rotation[2], 4, 'Z')
              @ Matrix.Rotation(rotation[1], 4, 'Y') @ Matrix.Rotation(rotation[0], 4, 'X')
              @ Matrix.Diagonal(Vector((*scale, 1))))
    uv_layer = obj.data.uv_layers.active.data
    for polygon in obj.data.polygons:
        if polygon.material_index != slot_index:
            continue
        for loop_index in polygon.loop_indices:
            uv = uv_layer[loop_index].uv
            mapped = matrix @ Vector((uv.x, uv.y, 0))
            uv_layer[loop_index].uv = (mapped.x, mapped.y)


def split_screen(obj: bpy.types.Object) -> tuple[bpy.types.Object, dict[str, object]]:
    material_index = next(index for index, material in enumerate(obj.data.materials) if material.name.endswith('cumputer'))
    screen = obj.copy()
    screen.data = obj.data.copy()
    screen.name = 'CRT screen emission'
    bpy.context.scene.collection.objects.link(screen)
    for target, retain_screen in ((screen, True), (obj, False)):
        mesh = bmesh.new()
        mesh.from_mesh(target.data)
        remove = [face for face in mesh.faces if (face.material_index == material_index) != retain_screen]
        bmesh.ops.delete(mesh, geom=remove, context='FACES')
        mesh.to_mesh(target.data)
        mesh.free()
        target.data.update()
    obj.name = 'CRT_Monitor_Tripo'
    points = [screen.matrix_world @ vertex.co for vertex in screen.data.vertices]
    centre = sum(points, Vector()) / len(points)
    normal_transform = screen.matrix_world.to_3x3().inverted().transposed()
    normal = sum((normal_transform @ polygon.normal * polygon.area for polygon in screen.data.polygons), Vector()).normalized()
    up = Vector((0, 0, 1))
    up = (up - normal * up.dot(normal)).normalized()
    # FBX/Unity changes handedness; this basis keeps text increasing toward camera right.
    right = up.cross(normal).normalized()
    us = [(point - centre).dot(right) for point in points]
    vs = [(point - centre).dot(up) for point in points]
    umin, umax, vmin, vmax = min(us), max(us), min(vs), max(vs)
    layer = screen.data.uv_layers.active or screen.data.uv_layers.new(name='TerminalUI')
    for polygon in screen.data.polygons:
        for loop_index in polygon.loop_indices:
            point = screen.matrix_world @ screen.data.vertices[screen.data.loops[loop_index].vertex_index].co
            layer.data[loop_index].uv = (((point - centre).dot(right) - umin) / (umax - umin),
                                         ((point - centre).dot(up) - vmin) / (vmax - vmin))
    centre += right * ((umin + umax) / 2) + up * ((vmin + vmax) / 2)
    bpy.context.view_layer.update()
    return screen, {'centre': list(centre), 'normal': list(normal), 'right': list(right), 'up': list(up),
                    'width': umax - umin, 'height': vmax - vmin, 'bounds': bounds(screen),
                    'uvConvention': '0..1 toward Unity camera-right and bottom-to-top; horizontal basis compensates FBX handedness'}


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', default='C:/Users/admin/Downloads/employee apartment1.0.blend')
    parser.add_argument('--output', default='Artifacts/EmployeeApartmentImport/export')
    options = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])
    source = Path(options.source).resolve()
    output = (root / options.output).resolve()
    output.mkdir(parents=True, exist_ok=True)
    source_hash = hashlib.sha256(source.read_bytes()).hexdigest()
    helper = load_helper(root / 'Art/MainRoom/scripts/combine_rooms.py', 'apartment_materials')
    geometry = load_helper(root / 'Art/MainRoom/reference/build_reference_layout.py', 'apartment_geometry')
    bpy.ops.wm.open_mainfile(filepath=str(source))
    bpy.context.scene.frame_set(133)
    bpy.context.view_layer.update()
    retained = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH' and obj.name not in CORRIDOR | PLACEHOLDERS]
    excluded = [{'name': obj.name, 'reason': 'existing corridor/door retained' if obj.name in CORRIDOR else 'placeholder/non-mesh'}
                for obj in bpy.context.scene.objects if obj not in retained]
    original_names = {obj: obj.name for obj in retained}
    original_bounds = {obj: bounds(obj) for obj in retained}
    for obj in list(bpy.data.objects):
        if obj not in retained:
            bpy.data.objects.remove(obj, do_unlink=True)
    cache: dict[str, bpy.types.Material] = {}
    adjustments: list[dict[str, object]] = []
    floor_source = bpy.data.materials['木地板014']
    floor = floor_material(floor_source)
    wood = bpy.data.materials['Lime._-_019']
    for obj in retained:
        name = original_names[obj]
        world = obj.matrix_world.copy()
        obj.animation_data_clear()
        obj.constraints.clear()
        obj.parent = None
        obj.matrix_world = world
        obj.hide_render = False
        obj.hide_viewport = False
        obj.hide_set(False)
        for index, slot in enumerate(obj.material_slots):
            material = slot.material
            if material is None:
                continue
            bake_mapping(obj, index, material)
            if material == floor_source:
                slot.material = floor
            else:
                if material.name == 'Material 0':
                    adjustments.append({'object': name, 'missingSourceTextureFallback': wood.name})
                    material = wood
                slot.material = prepare_material(material, helper, cache, adjustments)
        if name in SHELLS:
            low, high = (MINIMUM[0] - .2, MINIMUM[1] - .2, MINIMUM[2]), (MAXIMUM[0] + .2, MAXIMUM[1] + .2, MAXIMUM[2])
            if name in {'Floor', 'Awal'}:
                low, high = (MINIMUM[0], MINIMUM[1], MINIMUM[2] - .0001), (MAXIMUM[0], MAXIMUM[1], MAXIMUM[2] + .0001)
            geometry.clip_mesh(obj, low, high)
            if name == 'Floor':
                mapping = floor_source.node_tree.nodes['SimpleUVMapping']
                scale, aspect = mapping.inputs['Scale'].default_value, mapping.inputs['Aspect Ratio'].default_value
                for uv in obj.data.uv_layers.active.data:
                    uv.uv.x *= scale
                    uv.uv.y *= scale * aspect
        elif name == '平面' or name.startswith('平面.'):
            geometry.clip_mesh(obj, (MINIMUM[0] - .22, MINIMUM[1] - .2, MINIMUM[2]),
                               (MAXIMUM[0], MAXIMUM[1] + .2, MAXIMUM[2]))
        obj.name = 'B_' + name
    monitor = next(obj for obj in retained if original_names[obj] == 'C_monitor01_low_blinn14_0')
    screen, screen_layout = split_screen(monitor)
    accessories = [obj for obj in retained if original_names[obj] in ACCESSORIES]
    mapping = [{'sourceName': original_names[obj], 'exportName': 'A_TerminalAccessories' if obj in accessories else obj.name,
                'classification': 'terminal' if original_names[obj].startswith('C_') else 'shell' if original_names[obj] in SHELLS else 'roomOrWindowExterior',
                'originalBounds': original_bounds[obj]} for obj in retained]
    bpy.ops.object.select_all(action='DESELECT')
    for obj in accessories:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = accessories[0]
    bpy.ops.object.join()
    bpy.context.object.name = 'A_TerminalAccessories'
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
    for image in bpy.data.images:
        if not image.users or not all(image.size):
            continue
        if max(image.size) > 2048:
            ratio = 2048 / max(image.size)
            image.scale(round(image.size[0] * ratio), round(image.size[1] * ratio))
        if image.source == 'FILE' and not image.packed_file:
            image.pack()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.gltf(filepath=str(output / 'EmployeeApartment.glb'), export_format='GLB', use_selection=True,
                              export_apply=True, export_extras=True, export_cameras=False, export_lights=False, export_animations=False)
    bpy.context.view_layer.update()
    layout = json.loads((root / 'Art/MainRoom/reference/layout.json').read_text(encoding='utf-8'))
    layout['markers']['ScreenCentre'] = screen_layout['centre']
    layout['markers']['MonitorLight'] = list(Vector(screen_layout['centre']) + Vector(screen_layout['normal']) * .12)
    (output / 'layout.json').write_text(json.dumps(layout, indent=2), encoding='utf-8')
    report = {'source': str(source), 'sourceSha256': source_hash, 'sourceUnchanged': hashlib.sha256(source.read_bytes()).hexdigest() == source_hash,
              'frame': 133, 'coordinateConvention': 'Blender XYZ Z up; export_unity.py converts to Unity Y up',
              'roomMinimum': MINIMUM, 'roomMaximum': MAXIMUM, 'screen': screen_layout, 'objectMapping': mapping,
              'excludedObjects': excluded, 'materialAdjustments': adjustments, 'preservedDoorWallMaterial': 'B | KB3D_MIL_ConcreteA',
              'exportedObjects': [{'name': obj.name, 'bounds': bounds(obj), 'vertices': len(obj.data.vertices)} for obj in meshes],
              'meshCount': len(meshes), 'triangles': sum(sum(len(poly.vertices) - 2 for poly in obj.data.polygons) for obj in meshes)}
    (output / 'export-manifest.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print('APARTMENT_EXPORTED', json.dumps({'meshCount': len(meshes), 'triangles': report['triangles'], 'sourceUnchanged': report['sourceUnchanged']}), flush=True)


if __name__ == '__main__':
    main()

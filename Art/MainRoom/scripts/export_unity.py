"""Convert the published GLB to native Unity FBX and a Standard material manifest.

Run with Blender --background --threads 2 --python Art/MainRoom/scripts/export_unity.py.
The original GLB and Blender sources are never changed.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
import re
import struct
import sys
from pathlib import Path

import bpy


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", default="Art/MainRoom/exports/MainRoom.glb")
    parser.add_argument("--output", default="UnityProject/Assets/MiniGame/Art/MainRoom")
    parser.add_argument("--evidence-dir", default="Artifacts/Unity/MainRoom")
    options = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    source = root / options.source
    output = root / options.output
    scratch = root / options.evidence_dir
    (output / "Textures").mkdir(parents=True, exist_ok=True)
    scratch.mkdir(parents=True, exist_ok=True)
    raw = source.read_bytes()
    magic, version, total = struct.unpack_from("<4sII", raw)
    if magic != b"glTF" or version != 2 or total != len(raw):
        raise ValueError("Expected a complete GLB 2 file")
    json_size = struct.unpack_from("<I", raw, 12)[0]
    doc = json.loads(raw[20:20 + json_size])
    bin_offset = 20 + json_size
    bin_size, bin_type = struct.unpack_from("<I4s", raw, bin_offset)
    if bin_type != b"BIN\x00":
        raise ValueError("Expected embedded GLB image and mesh data")
    binary = raw[bin_offset + 8:bin_offset + 8 + bin_size]
    image_paths: list[str] = []
    for index, image in enumerate(doc.get("images", [])):
        view = doc["bufferViews"][image["bufferView"]]
        offset = view.get("byteOffset", 0)
        suffix = ".png" if image["mimeType"] == "image/png" else ".jpg"
        name = re.sub(r"[^A-Za-z0-9_-]", "_", image.get("name", "image"))
        relative = f"Textures/{index:02d}_{name}{suffix}"
        (output / relative).write_bytes(binary[offset:offset + view["byteLength"]])
        image_paths.append(relative)

    def texture_path(info: dict | None) -> str:
        if not info:
            return ""
        return image_paths[doc["textures"][info["index"]]["source"]]

    materials: list[dict] = []
    omitted_bump_inputs = 0
    for index, material in enumerate(doc.get("materials", [])):
        label = material.get("name", str(index))
        material["name"] = f"MainRoom_{index:02d}"
        pbr = material.get("pbrMetallicRoughness", {})
        normal = texture_path(material.get("normalTexture"))
        # Several source bump nodes exported their albedo image as a normal map.
        # Preserve albedo; only the actual Tripo tangent-space normal is portable.
        if normal and "normal" not in normal.lower():
            material.pop("normalTexture", None)
            normal = ""
            omitted_bump_inputs += 1
        for value in material.values():
            if isinstance(value, dict) and value.get("texCoord", 0) < 0:
                value["texCoord"] = 0
        for value in pbr.values():
            if isinstance(value, dict) and value.get("texCoord", 0) < 0:
                value["texCoord"] = 0
        strength = material.get("extensions", {}).get(
            "KHR_materials_emissive_strength", {}).get("emissiveStrength", 1)
        materials.append({
            "name": material["name"], "label": label,
            "doubleSided": material.get("doubleSided", False),
            "windowGlass": label == "CombinedWindowGlass",
            "color": pbr.get("baseColorFactor", [1, 1, 1, 1]),
            "metallic": pbr.get("metallicFactor", 1),
            "roughness": pbr.get("roughnessFactor", 1),
            "baseTexture": texture_path(pbr.get("baseColorTexture")),
            "normalTexture": normal,
            "normalScale": material.get("normalTexture", {}).get("scale", 1),
            "metallicRoughnessTexture": texture_path(pbr.get("metallicRoughnessTexture")),
            "emission": [value * strength for value in material.get("emissiveFactor", [0, 0, 0])],
        })
    encoded = json.dumps(doc, separators=(",", ":")).encode()
    encoded += b" " * (-len(encoded) % 4)
    prepared = scratch / "MainRoom-import.glb"
    prepared.write_bytes(struct.pack("<4sII", b"glTF", 2, 28 + len(encoded) + len(binary))
                         + struct.pack("<I4s", len(encoded), b"JSON") + encoded
                         + struct.pack("<I4s", len(binary), b"BIN\x00") + binary)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(prepared))
    yaw, pitch = -.1784, -.05288
    position = (-.1, -3.9469, 1.2546)
    target = (position[0] + math.sin(yaw) * math.cos(pitch) * 4,
              position[1] + math.cos(yaw) * math.cos(pitch) * 4,
              position[2] + math.sin(pitch) * 4)
    markers = {
        "PreviewPosition": position, "PreviewTarget": target,
        "CeilingLight": (.35, -.85, 2.51), "WindowLight": (1.05, 2.65, 2.05),
        "MonitorLight": (-2.10, -.60, 1.1), "FillLight": (1.3, -2.8, 2.2),
        "ExteriorLight": (1.1, 4.7, 3.0),
        "WindowDetailPosition": (1.1, 1.9, 1.85), "WindowDetailTarget": (1.1, 6.7, 1.85),
    }
    for name, location in markers.items():
        marker = bpy.data.objects.new("Unity_" + name, None)
        bpy.context.scene.collection.objects.link(marker)
        marker.location = location
    bpy.ops.export_scene.fbx(
        filepath=str(output / "MainRoom.fbx"), object_types={"MESH", "EMPTY"},
        axis_forward="-Z", axis_up="Y", apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS", bake_space_transform=True,
        bake_anim=False, add_leaf_bones=False, path_mode="STRIP")
    manifest = {
        "source": source.relative_to(root).as_posix(),
        "sourceSha256": hashlib.sha256(raw).hexdigest(),
        "meshCount": len(doc["meshes"]), "materials": materials,
        "omittedNonNormalBumpInputs": omitted_bump_inputs,
        "hasExteriorConcrete": any(node.get("name", "").startswith("B_ExteriorConcrete_") for node in doc["nodes"]),
        "scope": "Art preview; no gameplay integration, colliders or performance acceptance",
    }
    (output / "materials.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print("MINIGAME_UNITY_FBX_EXPORTED", json.dumps({
        "meshes": manifest["meshCount"], "materials": len(materials),
        "textures": len(image_paths), "omittedBumpInputs": omitted_bump_inputs}), flush=True)


if __name__ == "__main__":
    main()

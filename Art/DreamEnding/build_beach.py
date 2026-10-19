"""Editable original beach terrain and a seamless, synthesized ocean ambience.

Run with Blender --background --threads 2 --python Art/DreamEnding/build_beach.py.
No external model, texture or recorded sound is used by this generator.
"""
from __future__ import annotations

import math
from pathlib import Path
import wave

import bpy
import numpy as np


ROOT = Path(__file__).resolve().parent
ASSETS = ROOT.parent.parent / "UnityProject/Assets/MiniGame/Art/DreamEnding"


def material(name: str, color: tuple[float, float, float, float]) -> bpy.types.Material:
    result = bpy.data.materials.new(name)
    result.diffuse_color = color
    result.use_nodes = True
    shader = result.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = color
    shader.inputs["Roughness"].default_value = 0.92
    return result


def terrain() -> None:
    """Blender Z is up; negative Y is shore. Unity builder checks FBX axis orientation."""
    vertices: list[tuple[float, float, float]] = []
    faces: list[tuple[int, int, int, int]] = []
    width, depth = 160, 120
    for row in range(depth + 1):
        y = -100 + row * 180 / depth
        for column in range(width + 1):
            x = -150 + column * 300 / width
            shore = 0.28 - (y + 5) * 0.075
            dunes = max(0, min(1, (-y - 12) / 30))
            z = shore + dunes * (0.9 * math.sin(x * 0.026 + y * 0.052)
                                + 0.42 * math.sin(x * 0.081 - y * 0.038))
            vertices.append((x, y, z))
            if row < depth and column < width:
                start = row * (width + 1) + column
                faces.append((start, start + 1, start + width + 2, start + width + 1))
    mesh = bpy.data.meshes.new("Sculptable_Shore_Grid")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("BeachSand", mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material("Warm fine sand", (0.77, 0.67, 0.49, 1)))
    for polygon in mesh.polygons:
        polygon.use_smooth = True


def ambience() -> None:
    # Frequency-domain noise is periodic; envelopes also use integer cycles.
    # The resulting 30-second file loops without an artificial fade to silence.
    rate, seconds = 44100, 30
    samples = rate * seconds
    rng = np.random.default_rng(87211)
    frequencies = np.fft.rfftfreq(samples, 1 / rate)
    spectrum_shape = np.minimum(frequencies / 130, 1) / (1 + (frequencies / 1700) ** 1.4)
    spectrum_shape[0] = 0
    time = np.arange(samples) / rate
    channels: list[np.ndarray] = []
    for offset in (0.0, 0.22):
        phases = rng.uniform(-math.pi, math.pi, len(frequencies))
        noise = np.fft.irfft(spectrum_shape * np.exp(1j * phases), n=samples)
        noise /= max(float(np.std(noise)), 1e-8)
        swell = (0.5 + 0.5 * np.sin(2 * math.pi * time / 10 + offset)) ** 2
        long_swell = 0.72 + 0.28 * np.sin(2 * math.pi * time / 30 + offset)
        channels.append(noise * (0.018 + 0.055 * swell) * long_swell)
    pcm = np.clip(np.stack(channels, axis=1), -0.8, 0.8)
    with wave.open(str(ROOT / "OceanWaves.wav"), "wb") as output:
        output.setnchannels(2)
        output.setsampwidth(2)
        output.setframerate(rate)
        output.writeframes((pcm * 32767).astype("<i2").tobytes())
    (ASSETS / "OceanWaves.wav").write_bytes((ROOT / "OceanWaves.wav").read_bytes())


def main() -> None:
    ASSETS.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    terrain()
    bpy.context.scene["README"] = (
        "Original sculptable beach. Unity adds animated sea, procedural cloud sky, "
        "and the four original Combined bed meshes. Sand uses metres."
    )
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT / "DreamEndingBeach.blend"))
    bpy.ops.export_scene.fbx(filepath=str(ASSETS / "BeachTerrain.fbx"),
                             object_types={"MESH"}, add_leaf_bones=False,
                             axis_forward="-Z", axis_up="Y", apply_unit_scale=True,
                             bake_space_transform=True)
    ambience()
    print("DREAM_ENDING_ASSETS_READY", ASSETS)


if __name__ == "__main__":
    main()

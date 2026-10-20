"""Merge the existing rug and fringe into one Unity-ready mesh, without Blender dependencies."""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import struct
from pathlib import Path
from typing import cast


def load_glb(path: Path) -> tuple[dict[str, object], bytes]:
    data = path.read_bytes()
    if data[:4] != b"glTF" or struct.unpack_from("<I", data, 4)[0] != 2:
        raise ValueError("Expected a glTF 2 GLB")
    json_length, chunk_type = struct.unpack_from("<II", data, 12)
    if chunk_type != 0x4E4F534A:
        raise ValueError("GLB is missing its JSON chunk")
    document = cast(dict[str, object], json.loads(data[20 : 20 + json_length]))
    binary_offset = 20 + json_length
    binary_length, binary_type = struct.unpack_from("<II", data, binary_offset)
    if binary_type != 0x004E4942:
        raise ValueError("GLB is missing its BIN chunk")
    return document, data[binary_offset + 8 : binary_offset + 8 + binary_length]


def read_accessor(
    document: dict[str, object], binary: bytes, index: int
) -> list[tuple[float, ...]]:
    accessors = cast(list[dict[str, object]], document["accessors"])
    views = cast(list[dict[str, object]], document["bufferViews"])
    accessor = accessors[index]
    view = views[int(cast(int, accessor["bufferView"]))]
    component = int(cast(int, accessor["componentType"]))
    format_code, component_bytes = {5123: ("H", 2), 5125: ("I", 4), 5126: ("f", 4)}[
        component
    ]
    width = {"SCALAR": 1, "VEC2": 2, "VEC3": 3}[str(accessor["type"])]
    stride = int(cast(int, view.get("byteStride", width * component_bytes)))
    offset = int(cast(int, view.get("byteOffset", 0))) + int(
        cast(int, accessor.get("byteOffset", 0))
    )
    return [
        tuple(
            float(value)
            for value in struct.unpack_from(
                "<" + format_code * width, binary, offset + i * stride
            )
        )
        for i in range(int(cast(int, accessor["count"])))
    ]


def export(source: Path, destination: Path) -> dict[str, object]:
    document, binary = load_glb(source)
    meshes = cast(list[dict[str, object]], document["meshes"])
    nodes = cast(list[dict[str, object]], document["nodes"])
    positions: list[tuple[float, float, float]] = []
    normals: list[tuple[float, float, float]] = []
    triangles: list[tuple[int, int, int]] = []
    for node in nodes:
        if "mesh" not in node:
            continue
        if "matrix" in node or "rotation" in node or "scale" in node:
            raise ValueError(
                "The rug export changed; inspect node transforms before rebuilding"
            )
        translation = cast(list[float], node.get("translation", [0.0, 0.0, 0.0]))
        mesh = meshes[int(cast(int, node["mesh"]))]
        for primitive in cast(list[dict[str, object]], mesh["primitives"]):
            attributes = cast(dict[str, int], primitive["attributes"])
            offset = len(positions)
            for point in read_accessor(document, binary, attributes["POSITION"]):
                positions.append(tuple(point[i] + translation[i] for i in range(3)))
            for normal in read_accessor(document, binary, attributes["NORMAL"]):
                normals.append((normal[0], normal[1], normal[2]))
            indices = read_accessor(
                document, binary, int(cast(int, primitive["indices"]))
            )
            for i in range(0, len(indices), 3):
                triangles.append(
                    tuple(offset + int(indices[i + j][0]) + 1 for j in range(3))
                )
    lower = tuple(min(point[i] for point in positions) for i in range(3))
    upper = tuple(max(point[i] for point in positions) for i in range(3))
    size = tuple(upper[i] - lower[i] for i in range(3))
    centre = tuple((lower[i] + upper[i]) / 2 for i in range(3))
    # Unit XZ dimensions let the room profile directly control width and depth.
    # Real 8 mm source thickness is retained; all 260 fringe pieces become one submesh.
    lines = ["# Derived from Art/MainRoom/exports/09_Rug.glb", "o WornReferenceRug"]
    for point in positions:
        lines.append(
            f"v {(point[0] - centre[0]) / size[0]:.8f} {point[1] - lower[1]:.8f} {(point[2] - centre[2]) / size[2]:.8f}"
        )
    for point in positions:
        u = max(0.0, min(1.0, (point[0] - lower[0]) / size[0]))
        v = max(0.0, min(1.0, (point[2] - lower[2]) / size[2]))
        lines.append(f"vt {u:.8f} {v:.8f}")
    for normal in normals:
        transformed = (normal[0] * size[0], normal[1], normal[2] * size[2])
        length = math.sqrt(sum(value * value for value in transformed))
        lines.append("vn " + " ".join(f"{value / length:.8f}" for value in transformed))
    for triangle in triangles:
        lines.append("f " + " ".join(f"{index}/{index}/{index}" for index in triangle))
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return {
        "source": source.as_posix(),
        "sourceSha256": hashlib.sha256(source.read_bytes()).hexdigest(),
        "sourceMeshCount": len(meshes),
        "output": destination.as_posix(),
        "vertices": len(positions),
        "triangles": len(triangles),
        "submeshes": 1,
        "normalisedSize": [1, size[1], 1],
        "defaultScale": [3.4, 1, 2],
        "uv": "Top-down projection; complete original albedo, no tiling",
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--source", type=Path, default=Path("Art/MainRoom/exports/09_Rug.glb")
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path(
            "UnityProject/Assets/MiniGame/Art/MainRoom/RoomLighting/WornReferenceRug.obj"
        ),
    )
    args = parser.parse_args()
    report = export(args.source, args.output)
    (Path(__file__).parent / "rug-mesh-report.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(report, ensure_ascii=False))


if __name__ == "__main__":
    main()

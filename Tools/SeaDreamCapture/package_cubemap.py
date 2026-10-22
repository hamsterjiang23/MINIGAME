"""Package rendered PNG faces into Unity's native uncompressed RGBA32 Cubemap.

Run with uv run --with pillow Tools/SeaDreamCapture/package_cubemap.py.
"""

import json
import re
import uuid
from pathlib import Path

from PIL import Image, ImageDraw, ImageStat

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Art/DreamEnding/SeaMidpoint"
UNITY = ROOT / "UnityProject/Assets/MiniGame"
FACES = ("px", "nx", "py", "ny", "pz", "nz")


def contact_sheet(folder: Path, destination: Path) -> list[dict[str, object]]:
    sheet = Image.new("RGB", (3 * 384, 2 * 410), "#202124")
    cross = (
        Image.new("RGB", (4096, 3072), (24, 26, 30)) if folder.name == "Faces" else None
    )
    placements = {
        "py": (1, 0),
        "nx": (0, 1),
        "pz": (1, 1),
        "px": (2, 1),
        "nz": (3, 1),
        "ny": (1, 2),
    }
    draw = ImageDraw.Draw(sheet)
    metrics: list[dict[str, object]] = []
    for index, name in enumerate(FACES):
        with Image.open(folder / (name + ".png")) as opened:
            picture = opened.convert("RGB")
        stat = ImageStat.Stat(picture)
        metrics.append(
            {
                "face": name,
                "size": list(picture.size),
                "mean_rgb": stat.mean,
                "extrema": stat.extrema,
            }
        )
        x, y = index % 3 * 384, index // 3 * 410
        sheet.paste(picture.resize((384, 384)), (x, y + 26))
        draw.text((x + 10, y + 7), name.upper(), fill="white")
        if cross is not None:
            column, row = placements[name]
            cross.paste(picture, (column * 1024, row * 1024))
    sheet.save(destination)
    if cross is not None:
        cross.save(OUTPUT / "cubemap-cross.png")
    return metrics


def meta(path: Path, file_id: int) -> str:
    destination = Path(str(path) + ".meta")
    if destination.exists():
        match = re.search(r"^guid: (\w+)$", destination.read_text(), re.MULTILINE)
        if match is None:
            raise ValueError("Missing GUID in " + str(destination))
        return match.group(1)
    guid = uuid.uuid4().hex
    destination.write_text(
        f"fileFormatVersion: 2\nguid: {guid}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: {file_id}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n",
        encoding="utf-8",
    )
    return guid


def main() -> None:
    import sys

    smoke = "--smoke" in sys.argv
    folder = OUTPUT / ("Smoke" if smoke else "Faces")
    metrics = contact_sheet(
        folder, OUTPUT / ("smoke-contact.png" if smoke else "faces-contact.png")
    )
    if smoke:
        print(json.dumps(metrics))
        return
    payload = bytearray()
    size = 1024
    for name in FACES:
        with Image.open(folder / (name + ".png")) as image:
            if image.size != (size, size):
                raise ValueError("Unexpected face dimensions: " + name)
            # Native Cube faces have top-down storage on Unity's D3D11 renderer.
            payload.extend(image.convert("RGBA").tobytes())
    template = (UNITY / "Art/DreamEnding/DoorwayBeachPanorama.asset").read_text(
        encoding="utf-8"
    )
    header, remainder = template.split("  _typelessdata: ", 1)
    _, separator, suffix = remainder.partition("\n")
    header = header.replace(
        "m_Name: DoorwayBeachPanorama", "m_Name: SeaMidpointPanorama"
    )
    if f"  image data: {len(payload)}\n" not in header:
        raise ValueError("Template size differs from six 1024-pixel RGBA32 faces")
    cube = UNITY / "Art/DreamEnding/SeaMidpointPanorama.asset"
    cube.write_text(
        header + "  _typelessdata: " + payload.hex() + separator + suffix,
        encoding="utf-8",
    )
    cube_guid = meta(cube, 8900000)
    material = UNITY / "Art/DreamEnding/SeaMidpointSkybox.mat"
    text = (UNITY / "Art/DreamEnding/DoorwayBeachSkybox.mat").read_text(
        encoding="utf-8"
    )
    text = text.replace("m_Name: DoorwayBeachSkybox", "m_Name: SeaMidpointSkybox")
    text = re.sub(
        r"m_Texture: \{fileID: 8900000, guid: \w+, type: 2\}",
        f"m_Texture: {{fileID: 8900000, guid: {cube_guid}, type: 2}}",
        text,
    )
    material.write_text(text, encoding="utf-8")
    material_guid = meta(material, 2100000)
    profile = UNITY / "Resources/MiniGame/DreamSequenceDefault.asset"
    text = profile.read_text(encoding="utf-8")
    text, count = re.subn(
        r"  RoomPanoramaSkybox: \{fileID: 2100000, guid: \w+, type: 2\}",
        f"  RoomPanoramaSkybox: {{fileID: 2100000, guid: {material_guid}, type: 2}}",
        text,
    )
    if count != 1:
        raise ValueError("Expected exactly one RoomPanoramaSkybox binding")
    profile.write_text(text, encoding="utf-8")
    report = {
        "faces": metrics,
        "cube": str(cube.relative_to(ROOT)),
        "cubemap_guid": cube_guid,
        "material_guid": material_guid,
        "rgba32_bytes": len(payload),
        "profile_bindings_updated": count,
        "runtime_verification": "pending",
    }
    (OUTPUT / "cubemap-verification.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8"
    )
    print(json.dumps(report))


if __name__ == "__main__":
    main()

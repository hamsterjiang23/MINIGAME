# /// script
# requires-python = ">=3.10"
# dependencies = ["Pillow==12.3.0"]
# ///
"""Bake official Ark 12px BDF pixels into the terminal's existing logical cells."""
from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Art/Fonts/ArkPixel12/ark-pixel-12px-monospaced-zh_hans.bdf"
OUTPUT = ROOT / "UnityProject/Assets/MiniGame/Art/Fonts/ArkPixel12"
SIZES = (10, 11, 12, 13, 14, 18, 19)


def read_glyphs(path: Path = SOURCE) -> dict[int, Image.Image]:
    glyphs: dict[int, Image.Image] = {}
    code, advance, box = -1, 12, (0, 0, 0, 0)
    rows: list[str] = []
    bitmap = False
    for line in path.read_text(encoding="utf-8").splitlines():
        if line.startswith("ENCODING "):
            code = int(line.split()[1])
        elif line.startswith("DWIDTH "):
            advance = int(line.split()[1])
        elif line.startswith("BBX "):
            box = tuple(map(int, line.split()[1:]))
        elif line == "BITMAP":
            bitmap, rows = True, []
        elif line == "ENDCHAR":
            canvas = Image.new("L", (max(1, advance), 12))
            width, height, offset_x, offset_y = box
            for y, row in enumerate(rows):
                value = int(row, 16)
                for x in range(width):
                    px, py = offset_x + x, 10 - offset_y - height + y
                    if 0 <= px < canvas.width and 0 <= py < 12 and value & (1 << (len(row) * 4 - x - 1)):
                        canvas.putpixel((px, py), 255)
            glyphs[code] = canvas
            bitmap = False
        elif bitmap:
            rows.append(line)
    return glyphs


def corpus() -> tuple[set[int], list[str]]:
    paths = sorted((ROOT / "UnityProject/Assets/MiniGame/Runtime").glob("*.cs"))
    paths += sorted((ROOT / "UnityProject/Assets/MiniGame/Content").rglob("*.yarn"))
    paths += sorted((ROOT / "UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor").glob("*.yarn"))
    paths += sorted((ROOT / "src").rglob("*.cs"))
    paths += sorted((ROOT / "UnityProject/Assets/MiniGame/Content").rglob("*.json"))
    paths += sorted((ROOT / "UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor").glob("*.json"))
    codes = set(range(32, 127)) | {0x3000}
    for path in paths:
        if "obj" in path.parts or "bin" in path.parts:
            continue
        text = path.read_text(encoding="utf-8-sig")
        if path.suffix == ".json":
            text += extract_text(json.loads(text))
        codes.update(ord(char) for char in text if 32 <= ord(char) < 0x10000)
    return codes, [str(path.relative_to(ROOT)).replace("\\", "/") for path in paths if "obj" not in path.parts and "bin" not in path.parts]


def extract_text(value: object) -> str:
    if isinstance(value, str):
        return value
    if isinstance(value, dict):
        return "".join(extract_text(item) for item in value.values())
    if isinstance(value, list):
        return "".join(extract_text(item) for item in value)
    return ""


def build(check_only: bool) -> None:
    glyphs = read_glyphs()
    codes, paths = corpus()
    missing = sorted(codes - glyphs.keys())
    fallback_codes = missing.copy()
    fallback_source = ROOT / "Art/Fonts/ArkPixel12/Fallback/fusion-pixel-12px-monospaced-zh_hans.bdf"
    fallback = read_glyphs(fallback_source)
    for code in missing:
        if code in fallback:
            glyphs[code] = fallback[code]
    missing = sorted(codes - glyphs.keys())
    report = {"release": "2026.10.16", "variant": "12px-monospaced-zh_hans", "sourceSha256": hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
              "sourceGlyphs": len(glyphs) - len(fallback_codes), "corpusCharacters": len(codes),
              "fallbackCharacters": [{"code": f"U+{code:04X}", "character": chr(code)} for code in fallback_codes],
              "fallbackSourceSha256": hashlib.sha256(fallback_source.read_bytes()).hexdigest(),
              "missing": [{"code": f"U+{code:04X}", "character": chr(code)} for code in missing], "corpusFiles": paths}
    report_path = ROOT / "docs/terminal-font/coverage.json"
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({key: value for key, value in report.items() if key != "corpusFiles"}, ensure_ascii=False))
    if missing:
        raise SystemExit("Ark font is missing corpus characters; do not ship question-mark substitutions.")
    if check_only:
        metrics_path = OUTPUT / "ArkPixel12.json"
        if not metrics_path.exists():
            raise SystemExit("Generated atlas is missing; rebuild it first.")
        generated = json.loads(metrics_path.read_text(encoding="utf-8"))
        pairs = {(glyph["code"], glyph["size"]) for glyph in generated["glyphs"]}
        missing_pairs = {(code, size) for code in codes for size in SIZES} - pairs
        if missing_pairs:
            raise SystemExit(f"Generated atlas is stale ({len(missing_pairs)} missing glyph/size pairs); rebuild it.")
        print(json.dumps({"generatedAtlasCoverage": True, "verifiedGlyphSizePairs": len(codes) * len(SIZES)}))
        return
    tiles: list[tuple[int, int, Image.Image, int, int, int]] = []
    for size in SIZES:
        for code in sorted(codes):
            source = glyphs[code]
            advance = size if code > 255 else math.ceil(size * .6)
            width = max(1, round(source.width * size / 12))
            resized = source.resize((width, size), Image.Resampling.NEAREST)
            bounds = resized.getbbox()
            if bounds is None:
                tiles.append((code, size, Image.new("L", (1, 1)), 0, 0, advance))
            else:
                x0, y0, x1, y1 = bounds
                tiles.append((code, size, resized.crop(bounds), (advance - width) // 2 + x0, y0, advance))
    width, x, y, shelf = 2048, 1, 1, 0
    placements: list[tuple[int, int]] = []
    for _, _, tile, _, _, _ in tiles:
        if x + tile.width + 1 > width:
            x, y, shelf = 1, y + shelf + 2, 0
        placements.append((x, y))
        x += tile.width + 2
        shelf = max(shelf, tile.height)
    height = 1 << (y + shelf + 2).bit_length()
    atlas = Image.new("RGBA", (width, height), (255, 255, 255, 0))
    metrics: list[dict[str, int]] = []
    for (code, size, tile, offset_x, offset_y, advance), (x, y) in zip(tiles, placements, strict=True):
        rgba = Image.new("RGBA", tile.size, "white")
        rgba.putalpha(tile)
        atlas.paste(rgba, (x, y))
        visible = tile.getbbox() is not None
        metrics.append({"code": code, "size": size, "x": x, "y": y, "w": tile.width if visible else 0, "h": tile.height if visible else 0,
                        "offsetX": offset_x, "offsetY": offset_y, "advance": advance})
    OUTPUT.mkdir(parents=True, exist_ok=True)
    atlas.save(OUTPUT / "ArkPixel12.png")
    (OUTPUT / "ArkPixel12.json").write_text(json.dumps({"atlasWidth": width, "atlasHeight": height, "glyphs": metrics}, separators=(",", ":")) + "\n", encoding="utf-8")
    print(json.dumps({"atlas": [width, height], "glyphs": len(metrics), "sizes": SIZES}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--check-only", action="store_true")
    build(parser.parse_args().check_only)

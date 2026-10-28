from __future__ import annotations

import json
import tempfile
import unittest
from unittest.mock import patch
from pathlib import Path
from PIL import Image
import build_ark_atlas as atlas


class GlyphAtlasTests(unittest.TestCase):
    def test_audit_only_characters_do_not_block_runtime_font_generation(self: GlyphAtlasTests) -> None:
        with tempfile.TemporaryDirectory(prefix="minigame-font-corpus-") as directory:
            root = Path(directory)
            content = root / "UnityProject/Assets/MiniGame/Content/Campaign"
            content.mkdir(parents=True)
            (content / "campaign.minigame.json").write_text('{"text":"遭症躁符绩"}', encoding="utf-8")
            (content / "provenance.json").write_text('{"reason":"−107"}', encoding="utf-8")
            with patch.object(atlas, "ROOT", root):
                codes, _ = atlas.corpus()
            self.assertTrue({ord(char) for char in "遭症躁符绩"} <= codes)
            self.assertNotIn(ord("−"), codes)

    def test_corpus_includes_all_direct_resource_dialogue(self: GlyphAtlasTests) -> None:
        codes, paths = atlas.corpus()
        for category in ("Supervisor", "Endings"):
            path = f"UnityProject/Assets/MiniGame/Resources/MiniGame/{category}/Dialogue.json"
            self.assertIn(path, paths)
            data = json.loads((atlas.ROOT / path).read_text(encoding="utf-8"))
            self.assertTrue({ord(char) for char in atlas.extract_text(data) if ord(char) >= 32} <= codes)

    def test_reduction_preserves_a_single_pixel_at_every_source_position(self: GlyphAtlasTests) -> None:
        for width in (6, 12):
            for y in range(12):
                for x in range(width):
                    source = Image.new("L", (width, 12))
                    source.putpixel((x, y), 255)
                    for size in (10, 11):
                        with self.subTest(width=width, x=x, y=y, size=size):
                            result = atlas.resize_glyph(source, size)
                            self.assertIsNotNone(result.getbbox())
                            self.assertTrue(set(result.tobytes()) <= {0, 255})

    def test_native_and_larger_glyphs_keep_the_existing_pixels(self: GlyphAtlasTests) -> None:
        source = atlas.read_glyphs()[ord("中")]
        for size in (12, 13, 14, 18, 19):
            expected = source.resize((round(source.width * size / 12), size), Image.Resampling.NEAREST)
            self.assertEqual(atlas.resize_glyph(source, size).tobytes(), expected.tobytes())

    def test_validation_rejects_transparent_pixels_even_with_present_metrics(self: GlyphAtlasTests) -> None:
        with tempfile.TemporaryDirectory(prefix="minigame-glyph-validation-") as directory:
            root = Path(directory)
            metrics = {"atlasWidth": 2, "atlasHeight": 2, "glyphs": [
                {"code": ord("A"), "size": size, "x": 0, "y": 0, "w": 1, "h": 1} for size in atlas.SIZES]}
            (root / "metrics.json").write_text(json.dumps(metrics), encoding="utf-8")
            image = Image.new("RGBA", (2, 2), (255, 255, 255, 0))
            image.save(root / "atlas.png")
            with self.assertRaisesRegex(ValueError, "Invisible glyph"):
                atlas.validate_generated({ord("A")}, root / "metrics.json", root / "atlas.png")
            image.putpixel((0, 0), (255, 255, 255, 255))
            image.save(root / "atlas.png")
            self.assertEqual(atlas.validate_generated({ord("A")}, root / "metrics.json", root / "atlas.png"), 7)
            with self.assertRaisesRegex(ValueError, "stale"):
                atlas.validate_generated({ord("A"), ord("B")}, root / "metrics.json", root / "atlas.png")
            metrics["glyphs"][0]["w"] = 0
            (root / "metrics.json").write_text(json.dumps(metrics), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "empty glyph"):
                atlas.validate_generated({ord("A")}, root / "metrics.json", root / "atlas.png")


if __name__ == "__main__":
    unittest.main()

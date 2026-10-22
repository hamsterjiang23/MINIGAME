from __future__ import annotations

import copy
import json
import unittest
from unittest.mock import patch

from build_supervisor import ROOT, compile_dialogue, digest
from build_campaign import read_source


class SupervisorSourceTests(unittest.TestCase):
    def test_every_source_text_id_and_next_pointer_is_retained(self: SupervisorSourceTests) -> None:
        workbook = ROOT / "Art/chat_excel/【对话】监督员.xlsx"
        sheets, _, _ = read_source(workbook)
        expected = {row["I"]: row for row in sheets["对话流程"][1:] if row.get("I")}
        data, _ = compile_dialogue(workbook)
        lines = [line for day in data["conversations"] for line in day["lines"]]
        self.assertEqual(len(lines), 46)
        self.assertEqual({line["id"] for line in lines}, set(expected))
        for line in lines:
            self.assertEqual(line["text"], expected[line["id"]]["D"])
            self.assertEqual(line["next"], expected[line["id"]].get("J", ""))

    def test_day_boundary_preserves_the_day_four_surprise(self: SupervisorSourceTests) -> None:
        data, adaptations = compile_dialogue(ROOT / "Art/chat_excel/【对话】监督员.xlsx")
        self.assertEqual([len(day["lines"]) for day in data["conversations"]], [16, 12, 11, 7])
        self.assertEqual(data["conversations"][3]["lines"][0]["text"], "惊——喜。")
        self.assertEqual(data["conversations"][2]["lines"][-1]["next"], "P02_D4_R1_COMMON_002")
        self.assertTrue(any(item.get("cell") == "J40" and item["kind"] == "day-boundary" for item in adaptations))

    def test_missing_portrait_inherits_and_approved_supervisor_png_bytes_match(self: SupervisorSourceTests) -> None:
        data, _ = compile_dialogue(ROOT / "Art/chat_excel/【对话】监督员.xlsx")
        line = next(line for day in data["conversations"] for line in day["lines"] if line["sourceRow"] == 20)
        self.assertEqual(line["portraitId"], "JD_03")
        manifest = json.loads((ROOT / "docs/portrait-remap/source-manifest.json").read_text(encoding="utf-8-sig"))
        images = [image for image in manifest["entries"] if image["role"] == "supervisor"]
        self.assertEqual(len(images), 6)
        for image in images:
            self.assertEqual(digest(ROOT / "UnityProject" / image["target"]), image["sourceSha256"])

    def test_ambiguous_duplicate_ids_are_rejected(self: SupervisorSourceTests) -> None:
        workbook = ROOT / "Art/chat_excel/【对话】监督员.xlsx"
        sheets, cached, recovered = read_source(workbook)
        invalid = copy.deepcopy(sheets)
        duplicate = dict(invalid["对话流程"][1])
        duplicate.update(row="48", D="冲突文本")
        invalid["对话流程"].append(duplicate)
        with patch("build_supervisor.read_source", return_value=(invalid, cached, recovered)):
            with self.assertRaisesRegex(ValueError, "duplicate"):
                compile_dialogue(workbook)

    def test_compilation_is_deterministic_without_writing_the_workbook(self: SupervisorSourceTests) -> None:
        workbook = ROOT / "Art/chat_excel/【对话】监督员.xlsx"
        before = digest(workbook)
        first = compile_dialogue(workbook)
        second = compile_dialogue(workbook)
        self.assertEqual(first, second)
        self.assertEqual(digest(workbook), before)


if __name__ == "__main__":
    unittest.main()

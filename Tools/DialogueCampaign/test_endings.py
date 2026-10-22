"""Source fidelity, namespace isolation and revision checks for the ending content pack."""
from __future__ import annotations

import copy
import json
import unittest
from typing import cast

from build_campaign import ROOT, Record, read_source
from build_endings import AUDIT, OUTPUT, build, canonical_hash, read_snapshot, verified_sources, yarn_source


class EndingContentTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls: type[EndingContentTests]) -> None:
        cls.package, cls.audit = build()
        cls.sources = verified_sources()
        cls.conversations = {str(item["id"]): item for item in cast(list[Record], cls.package["conversations"])}

    def test_all_nine_sources_and_all_four_full_cloud_sheets_are_verified(self: EndingContentTests) -> None:
        self.assertEqual(len(self.sources), 9)
        sheets = read_snapshot(self.sources["self-review"])
        self.assertEqual({name: len(rows) for name, rows in sheets.items()},
                         {"对话流程": 18, "角色文本表": 17, "玩家选项表": 2, "立绘ID": 3})
        self.assertEqual(self.audit["lineCounts"], {"priest-ending": 8, "self-review": 16, "supervisor-ending": 7})

    def test_source_text_and_linear_order_are_exact_in_both_generic_id_books(self: EndingContentTests) -> None:
        books = {
            "priest-ending": read_source(ROOT / str(self.sources["priest-ending"]["file"]))[0],
            "self-review": read_snapshot(self.sources["self-review"]),
        }
        all_source_ids: list[str] = []
        for identifier, sheets in books.items():
            lines = cast(list[Record], self.conversations[identifier]["lines"])
            originals = [row for row in sheets["对话流程"][1:] if row.get("D")]
            self.assertEqual([line["text"] for line in lines], [row["D"] for row in originals])
            self.assertEqual([line["id"] for line in lines], [row["I"] for row in originals])
            for line, row in zip(lines, originals, strict=True):
                self.assertEqual(line["sourceRow"], int(row["row"]))
                self.assertEqual(line["sourceCell"], "D" + row["row"])
                self.assertEqual(line["sourceId"], identifier + "__" + row["I"])
                all_source_ids.append(str(line["sourceId"]))
        self.assertEqual(len(all_source_ids), len(set(all_source_ids)))
        self.assertEqual(cast(list[Record], self.conversations["priest-ending"]["lines"])[0]["id"],
                         cast(list[Record], self.conversations["self-review"]["lines"])[0]["id"])

    def test_priest_last_line_uses_black_screen_and_never_a_doctor_portrait(self: EndingContentTests) -> None:
        conversation = self.conversations["priest-ending"]
        lines = cast(list[Record], conversation["lines"])
        self.assertEqual([line["portraitId"] for line in lines], ["PE01"] * 6 + ["PE02", ""])
        self.assertEqual([line["blackScreen"] for line in lines], [False] * 7 + [True])
        self.assertEqual(lines[-1]["originalPortraitId"], "ED_03")
        self.assertEqual(conversation["finalAction"], "")

    def test_self_review_has_only_the_authored_final_purification(self: EndingContentTests) -> None:
        conversation = self.conversations["self-review"]
        self.assertEqual(conversation["finalAction"], "purify")
        self.assertEqual(conversation["finalActionText"], "净化")
        self.assertEqual(conversation["finalActionSourceCell"], "G18")
        self.assertEqual(conversation["finalActionSourceId"], "self-review__CUSTOM001_D1_R1_PLAYER_ACTION_001")
        self.assertEqual({line["portraitId"] for line in cast(list[Record], conversation["lines"])}, {"SJ01", "SJ02"})
        self.assertEqual(yarn_source(self.package).count("-> "), 1)
        self.assertEqual(yarn_source(self.package).count("#disposition:purify"), 1)

    def test_supervisor_ending_contains_exactly_the_authored_day_four_rows(self: EndingContentTests) -> None:
        source = read_source(ROOT / str(self.sources["supervisor"]["file"]))[0]
        expected = [row for row in source["对话流程"][1:] if row.get("A") == "Day4"]
        lines = cast(list[Record], self.conversations["supervisor-ending"]["lines"])
        self.assertEqual([line["text"] for line in lines], [row["D"] for row in expected])
        self.assertEqual([line["sourceRow"] for line in lines], list(range(41, 48)))
        self.assertEqual(lines[0]["text"], "惊——喜。")

    def test_revision_covers_both_content_and_all_source_hashes(self: EndingContentTests) -> None:
        body = {key: value for key, value in self.package.items() if key != "revision"}
        self.assertEqual(canonical_hash(body), self.package["revision"])
        changed_text = copy.deepcopy(body)
        conversations = cast(list[Record], changed_text["conversations"])
        cast(list[Record], conversations[0]["lines"])[0]["text"] = "Different source text"
        self.assertNotEqual(canonical_hash(changed_text), self.package["revision"])
        changed_hash = copy.deepcopy(body)
        cast(list[Record], changed_hash["sources"])[0]["sha256"] = "0" * 64
        self.assertNotEqual(canonical_hash(changed_hash), self.package["revision"])

    def test_checked_in_artifacts_match_deterministic_build(self: EndingContentTests) -> None:
        self.assertEqual(json.loads((OUTPUT / "Dialogue.json").read_text(encoding="utf-8")), self.package)
        self.assertEqual((OUTPUT / "Flow.yarn").read_text(encoding="utf-8"), yarn_source(self.package))
        self.assertEqual(json.loads(AUDIT.read_text(encoding="utf-8")), self.audit)


if __name__ == "__main__":
    unittest.main()

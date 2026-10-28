"""Guards against silently applying prose edits to changed sources or gameplay fields."""
import copy
import json
from pathlib import Path
import tempfile
import unittest

from approved_text import MANIFEST, apply_text_edits, load_manifest


class ApprovedTextTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.path = Path(self.directory.name) / "edits.json"
        self.manifest = load_manifest()
        self.spec = self.manifest["books"]["tutorial"]
        self.edit = self.spec["edits"][0]
        self.row = {"row": "5", "id": self.edit["id"], "open": self.edit["expectedText"],
                    "delta": "-2", "next": "NEXT", "portrait": "D0_01", "condition": "信任值>=3"}

    def save(self, manifest=None):
        self.path.write_text(json.dumps(manifest or self.manifest, ensure_ascii=False), encoding="utf-8")
        return self.path

    def apply(self, rows=None, source_hash=None):
        return apply_text_edits("tutorial", source_hash or self.spec["sourceSha256"],
                                rows if rows is not None else [self.row], manifest_path=self.save())

    def test_authorization_covers_exactly_twenty_four_issues_and_excludes_retained_prose(self):
        issues = {issue for spec in self.manifest["books"].values() for edit in spec["edits"] for issue in edit["issueIds"]}
        expected = {f"p0.{i}" for i in range(1, 3)} | {f"p1.{i}" for i in range(1, 10)} | {f"p2.{i}" for i in range(1, 18)}
        expected -= {"p1.7", "p2.12", "p2.14", "p2.15"}
        self.assertEqual(issues, expected)
        self.assertNotIn("self-review", self.manifest["books"])
        self.assertEqual(set(self.manifest["preservedIssueIds"]), {"p1.7", "p2.12", "p2.14"})

    def test_text_changes_preserve_side_effects_targets_conditions_and_input_rows(self):
        original = copy.deepcopy(self.row)
        result, audit = self.apply()
        self.assertEqual(self.row, original)
        self.assertEqual({key: value for key, value in result[0].items() if key != "open"},
                         {key: value for key, value in original.items() if key != "open"})
        self.assertEqual(result[0]["open"], self.edit["newText"])
        self.assertEqual(audit[0]["before"], original["open"])
        self.assertEqual(audit[0]["issueIds"], ["p2.1"])

    def test_changed_workbook_hash_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "source drift"):
            self.apply(source_hash="0" * 64)

    def test_changed_text_is_rejected_instead_of_fuzzy_replacement(self):
        with self.assertRaisesRegex(ValueError, "precondition failed"):
            self.apply(rows=[{**self.row, "open": "作者刚刚改过的另一句"}])

    def test_missing_and_ambiguous_nodes_are_rejected(self):
        for rows in ([], [self.row, dict(self.row)]):
            with self.subTest(rows=rows):
                with self.assertRaisesRegex(ValueError, "missing or ambiguous"):
                    self.apply(rows=rows)

    def test_protected_issues_numeric_fields_and_duplicate_edits_are_rejected(self):
        for kind in ("protected", "numeric", "duplicate"):
            manifest = copy.deepcopy(self.manifest)
            edits = manifest["books"]["tutorial"]["edits"]
            if kind == "protected":
                edits[0]["issueIds"] = ["p1.7"]
            elif kind == "numeric":
                edits[0]["field"] = "delta"
            else:
                edits.append(dict(edits[0]))
            with self.subTest(kind=kind):
                with self.assertRaisesRegex(ValueError, "Invalid, duplicate or protected"):
                    load_manifest(self.save(manifest))


if __name__ == "__main__":
    unittest.main()

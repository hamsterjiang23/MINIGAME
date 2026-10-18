"""Converter checks independent of Unity and the C# package consumer."""
from __future__ import annotations

import unittest

from build_campaign import ROOT, build, constant_rows, read_source


class CampaignConversionTests(unittest.TestCase):
    def test_constant_arrays_preserve_delimiters_inside_quoted_text(self) -> None:
        self.assertEqual(constant_rows('{"A,B","C;D";"quoted ""word""",""}'),
                         [["A,B", "C;D"], ['quoted "word"', ""]])
        self.assertEqual(constant_rows('FILTER(A1:B2,A1:A2<>"")'), [])

    def test_formula_only_portrait_metadata_is_recovered_without_recalculation(self) -> None:
        path = next(p for p in (ROOT / "Art/chat_excel").glob("*.xlsx") if "外交官" in p.name and not p.name.startswith("~$"))
        sheets, cached, recovered = read_source(path)
        self.assertEqual(len(cached["立绘ID"]), 1)
        self.assertEqual([row["A"] for row in sheets["立绘ID"][1:]], ["AD01", "AD02", "AD03", "AD04", "AD05"])
        self.assertTrue(any(cell["sheet"] == "立绘ID" and cell["cell"] == "A5" for cell in recovered))

    def test_smoke_and_full_conversion_cover_source_rows_without_modification(self) -> None:
        smoke, audit = build(smoke=True)
        self.assertEqual(audit["meetingCount"], 3)
        self.assertEqual(smoke["formatVersion"], 2)
        _, full = build()
        self.assertEqual(full["sourceRowCount"], 994)
        self.assertEqual(full["meetingCount"], 16)
        self.assertEqual(full["declaredPortraits"], 27)
        self.assertEqual(len(full["referencedPortraits"]), 24)


if __name__ == "__main__":
    unittest.main()

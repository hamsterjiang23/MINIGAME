"""Converter checks independent of Unity and the C# package consumer."""
from __future__ import annotations

import unittest
import tempfile
from pathlib import Path
from typing import cast
from zipfile import ZipFile

from build_campaign import HERE, ROOT, Record, Row, build, build_meeting, constant_rows, read_source
from updated_source import apply_repairs, build_updated_meeting, normalize_rows, source_columns


class CampaignConversionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.package, cls.audit = build()

    def test_constant_arrays_preserve_delimiters_inside_quoted_text(self) -> None:
        self.assertEqual(constant_rows('{"A,B","C;D";"quoted ""word""",""}'),
                         [["A,B", "C;D"], ['quoted "word"', ""]])
        self.assertEqual(constant_rows('FILTER(A1:B2,A1:A2<>"")'), [])

    def test_formula_only_portrait_metadata_is_recovered_without_recalculation(self) -> None:
        namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
        relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "formula.xlsx"
            with ZipFile(path, "w") as archive:
                archive.writestr("xl/workbook.xml", f'<workbook xmlns="{namespace}" xmlns:r="{relationships}">'
                                 '<sheets><sheet name="立绘ID" sheetId="1" r:id="rId1"/></sheets></workbook>')
                archive.writestr("xl/_rels/workbook.xml.rels", '<Relationships>'
                                 '<Relationship Id="rId1" Target="worksheets/sheet1.xml"/></Relationships>')
                archive.writestr("xl/worksheets/sheet1.xml", f'<worksheet xmlns="{namespace}">'
                                 '<dimension ref="A1"/><sheetData><row r="2"><c r="A2">'
                                 '<f>{"AD01","甲";"AD02","乙"}</f></c></row></sheetData></worksheet>')
            sheets, cached, recovered = read_source(path)
            self.assertEqual([row["A"] for row in sheets["立绘ID"]], ["AD01", "AD02"])
            self.assertTrue(all("A" not in row for row in cached["立绘ID"]))
            self.assertEqual(len(recovered), 4)

    def test_smoke_and_full_conversion_have_expected_meetings(self) -> None:
        smoke, audit = build(smoke=True)
        self.assertEqual(audit["meetingCount"], 3)
        self.assertEqual(smoke["formatVersion"], 2)
        self.assertEqual(self.audit["sourceRowCount"], 1440)
        self.assertEqual(self.audit["activeSourceRowCount"], 1299)
        self.assertEqual(self.audit["excludedSourceRowCount"], 141)
        self.assertEqual(self.audit["meetingCount"], 16)
        self.assertEqual(self.package["contentVersion"], "1.1.1-development")

    def test_every_source_row_is_either_exactly_mapped_or_explicitly_excluded(self) -> None:
        mappings = cast(list[Record], self.audit["mappings"])
        exclusions = cast(list[Record], self.audit["excludedRows"])
        for book in cast(list[Record], self.audit["books"]):
            pid = book["patientId"]
            rows = cast(list[Row], book["rows"])
            active = [mapping for mapping in mappings if mapping["patientId"] == pid]
            excluded = [exclusion for exclusion in exclusions if exclusion["patientId"] == pid]
            self.assertEqual(len({mapping["row"] for mapping in active}), len(active))
            self.assertEqual(len({entry["row"] for entry in excluded}), len(excluded))
            self.assertFalse({entry["row"] for entry in active} & {entry["row"] for entry in excluded})
            self.assertEqual({int(row["row"]) for row in rows},
                             {entry["row"] for entry in active + excluded})
            by_row = {int(row["row"]): row for row in rows}
            for mapping in active:
                original = by_row[int(str(mapping["row"]))]
                self.assertEqual(mapping["text"], original[str(mapping["column"])])
                self.assertEqual(mapping["authoredDelta"], original.get(str(mapping["deltaColumn"]), ""))
            self.assertTrue(all(entry["reason"] for entry in excluded))

    def test_authored_extreme_deltas_and_response_delta_are_all_accounted_for(self) -> None:
        mappings = cast(list[Record], self.audit["mappings"])
        by_id = {str(mapping["sourceId"]): mapping for mapping in mappings}
        doctor_option = by_id["battlefield-doctor__P04_D1_R6_OPTION_002"]
        doctor_response = by_id["battlefield-doctor__P04_D1_R6_CLOSE_004"]
        self.assertEqual(doctor_option["authoredDelta"], "-5")
        self.assertEqual(doctor_option["trustDelta"], 0)
        self.assertEqual(doctor_response["authoredDelta"], "5")
        self.assertEqual(doctor_response["trustAppliedTo"], doctor_option["sourceId"])
        self.assertEqual(by_id["battlefield-doctor__P04_D2_R4_OPTION_001"]["trustDelta"], -107)
        self.assertEqual(by_id["priest__P01_D1_R1_OPTION_001"]["trustDelta"], 100)
        self.assertEqual(by_id["priest__P01_D2_R1_OPTION_001"]["trustDelta"], 100)
        self.assertEqual(sum(int(str(mapping["authoredDelta"]) or "0") for mapping in mappings),
                         sum(int(str(mapping.get("trustDelta", 0))) for mapping in mappings))
        self.assertIn("#id:battlefield-doctor__P04_D2_R4_OPTION_001 #trust:-107",
                      str(self.package["yarnSource"]))

    def test_doctor_recovered_branches_match_old_evidence_and_never_join_options(self) -> None:
        repairs = cast(list[Record], self.audit["repairs"])
        recovered = [repair for repair in repairs if repair["kind"] == "restore-old-corresponding-branch"]
        self.assertEqual(len(recovered), 107)
        by_row = {int(str(repair["row"])): repair for repair in recovered}
        self.assertEqual(cast(Record, by_row[134]["after"])["next"],
                         "P04_D3_R1_OPTION_001,P04_D3_R1_OPTION_002,P04_D3_R1_OPTION_003")
        self.assertEqual(cast(Record, by_row[138]["after"])["next"], "P04_D3_R1_COMMON_002")
        self.assertEqual(cast(Record, by_row[139]["after"])["next"], "P04_D3_R1_CLOSE_004")
        self.assertEqual(cast(Record, by_row[232]["after"])["next"], "P04_D3_R1_ACTION_014")
        self.assertEqual(cast(Record, by_row[255]["evidence"])["oldId"], "ED_D3_PURIFY_T3")
        self.assertEqual(cast(Record, by_row[255]["after"])["next"], "P04_D3_R1_CLOSE_047")
        exclusions = cast(list[Record], self.audit["excludedRows"])
        doctor_excluded = {entry["row"] for entry in exclusions if entry["patientId"] == "battlefield-doctor"}
        self.assertIn(233, doctor_excluded)
        self.assertIn(234, doctor_excluded)
        self.assertIn(290, doctor_excluded)
        self.assertNotIn(255, doctor_excluded)
        self.assertTrue(all(cast(Record, repair["evidence"])["oldSha256"] for repair in recovered))

    def test_repair_manifest_rejects_new_source_hash_instead_of_guessing(self) -> None:
        with self.assertRaisesRegex(ValueError, "does not match"):
            apply_repairs("battlefield-doctor", "future-revision", [], HERE / "source-repairs.json")

    def test_new_tutorial_is_complete_and_confirmed_priest_alias_keeps_source_evidence(self) -> None:
        books = {book["patientId"]: book for book in cast(list[Record], self.audit["books"])}
        tutorial = cast(list[Row], books["tutorial"]["activeRows"])
        self.assertEqual([int(row["row"]) for row in tutorial], list(range(2, 14)))
        self.assertEqual([row["open"] for row in tutorial if row["open"]],
                         ["嘿！", "你好啊。", "别害怕，我动不了。", "这玩意把我绑的严严实实呢。", "……",
                          "不过我发现了一件有趣的事。", "有些人啊，根本不需要绳子。", "……", "你说是不是？"])
        self.assertEqual(tutorial[-1]["next"], "END_DAY0")
        original = {int(row["row"]): row for row in cast(list[Row], books["priest"]["rows"])}
        aliases = [repair for repair in cast(list[Record], self.audit["repairs"])
                   if repair["kind"] == "user-confirmed-portrait-remap"]
        self.assertEqual([repair["row"] for repair in aliases], [62, 63, 67, 121, 122, 178, 179])
        for repair in aliases:
            self.assertEqual(original[int(str(repair["row"]))]["L"], "JN05")
            self.assertEqual(cast(Record, repair["after"])["portrait"], "JN04")
        self.assertNotIn("JN05", self.audit["referencedPortraits"])
        declared = {portrait["id"] for portrait in cast(list[Record], self.package["portraits"])}
        self.assertTrue(set(cast(list[str], self.audit["referencedPortraits"])) <= declared)

    def test_priest_terminal_continuation_is_one_linear_effect_free_response(self) -> None:
        priest = next(book for book in cast(list[Record], self.audit["books"]) if book["patientId"] == "priest")
        by_row = {int(row["row"]): row for row in cast(list[Row], priest["activeRows"])}
        self.assertEqual(by_row[60]["next"], by_row[67]["id"])
        self.assertEqual(by_row[67]["next"], "END_DAY1")
        self.assertEqual(by_row[67]["actionResponse"], "不要让我等待太久…")
        self.assertFalse(by_row[67]["option"] or by_row[67]["action"])
        self.assertEqual(int(by_row[67]["delta"] or "0"), 0)

    def test_special_entries_release_thresholds_and_implied_conditions_are_preserved(self) -> None:
        campaign = cast(Record, self.package["campaign"])
        meetings = [meeting for day in cast(list[Record], campaign["days"])
                    for meeting in cast(list[Record], day["meetings"])]
        gated = [meeting for meeting in meetings if "specialRoundStartNode" in meeting]
        self.assertEqual(len(gated), 9)
        self.assertEqual({meeting["patientId"] for meeting in gated}, {"child", "former-ruler", "diplomat"})
        for meeting in gated:
            self.assertEqual(meeting["specialTrustThreshold"], 3)
            self.assertEqual(meeting["releaseThreshold"], 5)
            self.assertIn("title: " + str(meeting["specialRoundStartNode"]), str(meeting["yarnSource"]))
        implied = [item for item in cast(list[Record], self.audit["adaptations"])
                   if item["kind"] == "implied-special-condition"]
        self.assertEqual(len(implied), 9)
        self.assertTrue(all(int(str(item["minimumReachableTrust"])) >= 3 for item in implied))
        self.assertEqual(campaign["variants"], [])

    def test_child_schema_uses_actual_type_and_condition_column(self) -> None:
        path = next(path for path in (ROOT / "Art/chat_excel").glob("*.xlsx") if "小孩" in path.name)
        rows = read_source(path)[0]["对话流程"]
        columns = source_columns(rows[0])
        self.assertEqual(columns["type"], "C")
        self.assertEqual(columns["condition"], "N")
        normalized = normalize_rows(rows[1:], columns)
        self.assertEqual(normalized[0]["type"], rows[1]["C"])
        self.assertEqual(normalized[0]["id"], "P02_D1_R1_OPTION_001")

    def test_legacy_schema_still_compiles_choice_and_action(self) -> None:
        rows = [{"row": "2", "J": "X_D1_START", "B": "R1", "E": "开场"},
                {"row": "3", "J": "X_D1_O1", "B": "R1", "D": "玩家选项", "F": "回答", "K": "X_D1_DECISION"},
                {"row": "4", "J": "X_D1_DECISION", "B": "R2", "E": "决定", "K": "X_D1_MEASURE"},
                {"row": "5", "J": "X_D1_TREAT", "B": "R2", "H": "治疗", "K": "END_DAY1"}]
        meeting = build_meeting({"id": "legacy", "name": "角色"}, rows, 1, [], [])
        self.assertIn(" #disposition:treat", str(meeting["yarnSource"]))
        self.assertEqual(meeting["startNode"], "legacy__X_D1_START")

    def test_unknown_condition_is_rejected_instead_of_flattened(self) -> None:
        rows = [{"row": "2", "id": "P_D1_COMMON_001", "round": "R1", "open": "文本",
                 "next": "END_DAY1", "condition": "信任值<3"}]
        columns = {"open": "D", "delta": "K"}
        with self.assertRaisesRegex(ValueError, "Unimplemented condition"):
            build_updated_meeting({"id": "test", "name": "角色"}, rows, 1, columns, [], [])


if __name__ == "__main__":
    unittest.main()

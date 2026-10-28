"""Converter checks independent of Unity and the C# package consumer."""
from __future__ import annotations

import unittest
import tempfile
import re
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
        self.assertEqual(self.audit["sourceRowCount"], 1360)
        self.assertEqual(self.audit["activeSourceRowCount"], 1294)
        self.assertEqual(self.audit["excludedSourceRowCount"], 66)
        self.assertEqual(self.audit["meetingCount"], 13)
        self.assertEqual(self.package["contentVersion"], "1.2.2-development")

    def test_daily_rosters_follow_the_confirmed_arrival_order(self) -> None:
        campaign = cast(Record, self.package["campaign"])
        days = cast(list[Record], campaign["days"])
        expected = [
            ["tutorial"],
            ["former-ruler", "battlefield-doctor", "child"],
            ["former-ruler", "battlefield-doctor", "child", "priest"],
            ["former-ruler", "battlefield-doctor", "child", "priest", "diplomat"],
        ]
        for day, roster in zip(days, expected, strict=True):
            self.assertEqual([meeting["patientId"] for meeting in cast(list[Record], day["meetings"])], roster)

    def test_tutorial_initial_trust_override_keeps_formal_patients_at_default(self) -> None:
        smoke, _ = build(smoke=True)
        for package in (self.package, smoke):
            patients = cast(list[Record], cast(Record, package["campaign"])["patients"])
            self.assertEqual(next(p["initialTrust"] for p in patients if p["id"] == "tutorial"), -2)
            self.assertEqual(cast(Record, package["profile"])["initialTrust"], -2)
            self.assertTrue(all(p["initialTrust"] == 0 for p in patients if p["id"] != "tutorial"))

    def test_late_arrivals_start_at_their_first_workbook_day_without_rewriting_source_ids(self) -> None:
        days = cast(list[Record], cast(Record, self.package["campaign"])["days"])
        expected = [(2, "priest", "P01_D1", 1), (3, "priest", "P01_D2", 2),
                    (3, "diplomat", "P05_D1", 1)]
        for campaign_day, patient, source_prefix, source_day in expected:
            meeting = next(item for item in cast(list[Record], days[campaign_day]["meetings"])
                           if item["patientId"] == patient)
            self.assertEqual(meeting["startNode"], f"{patient}__{source_prefix}_R1_OPTION_001__MENU")
            self.assertEqual(meeting["endNode"], f"{patient}__END_DAY{source_day}")
            self.assertEqual(set(re.findall(r"^day: (\d+)$", str(meeting["yarnSource"]), re.MULTILINE)),
                             {str(campaign_day)})
        priest_first = next(item for item in cast(list[Record], days[2]["meetings"]) if item["patientId"] == "priest")
        self.assertIn("请告诉我，今天就能接受净化吗？", str(priest_first["yarnSource"]))
        self.assertNotIn("你昨天把我从门前拖了回来。", str(priest_first["yarnSource"]))
        diplomat_first = next(item for item in cast(list[Record], days[3]["meetings"]) if item["patientId"] == "diplomat")
        self.assertIn("先核对身份。姓名、职业，再说明你是怎么被送进来的。", str(diplomat_first["yarnSource"]))
        self.assertNotIn("我接到的，是观察审查人员怎样工作的任务。", str(diplomat_first["yarnSource"]))

    def test_later_workbook_visits_remain_audited_but_do_not_extend_the_three_day_campaign(self) -> None:
        schedule = cast(list[Record], self.audit["meetingSchedule"])
        future = {(item["patientId"], item["sourceDay"], item["campaignDay"])
                  for item in schedule if not item["scheduled"]}
        self.assertEqual(future, {("priest", 3, 4), ("diplomat", 2, 4), ("diplomat", 3, 5)})
        self.assertEqual(sum(bool(item["scheduled"]) for item in schedule), 13)
        yarn = str(self.package["yarnSource"])
        for patient, source_day, campaign_day in future:
            mappings = [item for item in cast(list[Record], self.audit["mappings"])
                        if item["patientId"] == patient and item["sourceDay"] == source_day]
            self.assertTrue(mappings)
            self.assertTrue(all(not item["scheduled"] and item["campaignDay"] == campaign_day for item in mappings))
            self.assertTrue(all("title: " + str(item["sourceId"]) + "\n" not in yarn for item in mappings))
        self.assertEqual([day["day"] for day in cast(list[Record], cast(Record, self.package["campaign"])["days"])],
                         [0, 1, 2, 3])

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
                expected_text = original[str(mapping["column"])]
                if pid == "battlefield-doctor" and original.get("I") == "P04_D1_R6_COMMON_002":
                    self.assertEqual(expected_text, "我不记得了")
                    expected_text += "。"  # User-approved punctuation; original workbook stays intact.
                edits = [edit for edit in self.audit["approvedTextEdits"]
                         if edit["sourceId"] == mapping["sourceId"]]
                for edit in edits:
                    self.assertEqual(edit["before"], expected_text)
                    expected_text = edit["after"]
                self.assertEqual(mapping["text"], expected_text)
                self.assertEqual(mapping["authoredDelta"], original.get(str(mapping["deltaColumn"]), ""))
            self.assertTrue(all(entry["reason"] for entry in excluded))

    def test_authored_extreme_deltas_and_response_delta_are_all_accounted_for(self) -> None:
        mappings = cast(list[Record], self.audit["mappings"])
        by_id = {str(mapping["sourceId"]): mapping for mapping in mappings}
        doctor_option = by_id["battlefield-doctor__P04_D1_R6_OPTION_002"]
        doctor_response = by_id["battlefield-doctor__P04_D1_R6_CLOSE_004"]
        self.assertEqual(doctor_option["authoredDelta"], "-5")
        self.assertEqual(doctor_option["unadjustedTrustDelta"], 0)
        self.assertEqual(doctor_option["trustDelta"], -1)
        self.assertEqual(doctor_response["authoredDelta"], "5")
        self.assertEqual(doctor_response["trustAppliedTo"], doctor_option["sourceId"])
        self.assertEqual(by_id["battlefield-doctor__P04_D2_R4_OPTION_001"]["authoredDelta"], "-107")
        self.assertEqual(by_id["battlefield-doctor__P04_D2_R4_OPTION_001"]["trustDelta"], -10)
        self.assertEqual(by_id["priest__P01_D1_R1_OPTION_001"]["trustDelta"], 100)
        self.assertEqual(by_id["priest__P01_D2_R1_OPTION_001"]["trustDelta"], 100)
        adjustments = [item for item in self.audit["adaptations"]
                       if item["kind"] == "user-approved-trust-adjustment"]
        self.assertEqual(sum(int(str(mapping["authoredDelta"]) or "0") for mapping in mappings)
                         + sum(int(item["after"]) - int(item["before"]) for item in adjustments),
                         sum(int(str(mapping.get("trustDelta", 0))) for mapping in mappings))
        self.assertIn("#id:battlefield-doctor__P04_D2_R4_OPTION_001 #trust:-10",
                      str(self.package["yarnSource"]))

    def test_trust_adjustment_rejects_changed_text_or_delta(self) -> None:
        import json
        specs = json.loads((HERE / "trust-adjustments.json").read_text(encoding="utf-8"))["adjustments"]
        source_id = "battlefield-doctor__P04_D2_R4_OPTION_001"
        original = next(item for item in specs if item["sourceId"] == source_id)
        patient = {"id": "battlefield-doctor", "name": "埃德温"}
        book = next(item for item in self.audit["books"] if item["patientId"] == patient["id"])
        for field, value in (("expectedText", "changed"), ("expectedEffectiveDelta", -10)):
            with self.subTest(field=field):
                changed = {**original, field: value}
                with self.assertRaisesRegex(ValueError, "Trust adjustment precondition failed"):
                    build_updated_meeting(patient, book["activeRows"], 2, book["schemaColumns"], [], [],
                                          trust_adjustments={source_id: changed})

    def test_every_disposition_has_an_explicit_audited_reaction_delta(self) -> None:
        import json
        entries = json.loads((HERE / "disposition-trust-adjustments.json").read_text(encoding="utf-8"))["adjustments"]
        self.assertEqual(len(entries), 46)
        mappings = {m["sourceId"]: m for m in self.audit["mappings"]}
        action_ids = {book["patientId"] + "__" + row["id"] for book in self.audit["books"]
                      for row in book["activeRows"] if row.get("action")}
        self.assertEqual({e["sourceId"] for e in entries}, action_ids)
        self.assertEqual(sum(bool(mappings[e["sourceId"]]["scheduled"]) for e in entries), 37)
        for entry in entries:
            self.assertEqual(mappings[entry["sourceId"]]["trustDelta"], entry["trustDelta"])
            self.assertEqual(mappings[entry["sourceId"]]["authoredDelta"], "0")
        priest_release = [e for e in entries if e["sourceId"].startswith("priest__") and e["expectedDisposition"] == "release"]
        self.assertEqual(len(priest_release), 3)
        self.assertTrue(all(e["trustDelta"] == -200 for e in priest_release))
        entry = entries[0]
        pid = entry["sourceId"].split("__", 1)[0]
        book = next(b for b in self.audit["books"] if b["patientId"] == pid)
        with self.assertRaisesRegex(ValueError, "Trust adjustment precondition failed"):
            build_updated_meeting({"id": pid, "name": "测试"}, book["activeRows"], 0, book["schemaColumns"], [], [],
                                  trust_adjustments={entry["sourceId"]: {**entry, "expectedDisposition": "release"}})

    def test_doctor_recovered_branches_match_old_evidence_and_never_join_options(self) -> None:
        repairs = cast(list[Record], self.audit["repairs"])
        recovered = [repair for repair in repairs if repair["kind"] == "restore-old-corresponding-branch"]
        self.assertEqual(len(recovered), 103)
        by_row = {int(str(repair["row"])): repair for repair in recovered}
        self.assertEqual(cast(Record, by_row[134]["after"])["next"],
                         "P04_D3_R1_OPTION_001,P04_D3_R1_OPTION_002,P04_D3_R1_OPTION_003")
        self.assertEqual(cast(Record, by_row[138]["after"])["next"], "P04_D3_R1_COMMON_002")
        self.assertEqual(cast(Record, by_row[139]["after"])["next"], "P04_D3_R1_CLOSE_004")
        for row in (229, 232, 238):
            self.assertEqual(cast(Record, by_row[row]["after"])["next"], "END_DAY3")
        self.assertFalse({239, 240, 241, 255} & set(by_row))
        exclusions = cast(list[Record], self.audit["excludedRows"])
        doctor_excluded = {entry["row"] for entry in exclusions if entry["patientId"] == "battlefield-doctor"}
        self.assertEqual(doctor_excluded, {233, 234})
        self.assertTrue(all(cast(Record, repair["evidence"])["oldSha256"] for repair in recovered))
        doctor = next(book for book in cast(list[Record], self.audit["books"])
                      if book["patientId"] == "battlefield-doctor")
        active = cast(list[Row], doctor["activeRows"])
        self.assertEqual(len(active), 235)
        self.assertEqual(max(int(row["row"]) for row in active), 238)
        self.assertEqual(next(row["actionResponse"] for row in active if row["row"] == "238"),
                         "打开门，让我自己承担后果。")

    def test_repair_manifest_rejects_new_source_hash_instead_of_guessing(self) -> None:
        with self.assertRaisesRegex(ValueError, "does not match"):
            apply_repairs("battlefield-doctor", "future-revision", [], HERE / "source-repairs.json")

    def test_unlisted_workbook_rejects_duplicate_ids_before_a_graph_can_overwrite_text(self: CampaignConversionTests) -> None:
        rows = [{"row": "2", "id": "X_D1_LINE", "open": "新版正文"},
                {"row": "3", "id": "X_D1_LINE", "open": "残留旧句"}]
        with self.assertRaisesRegex(ValueError, "duplicate IDs"):
            apply_repairs("unlisted-patient", "unlisted-hash", rows, HERE / "source-repairs.json")

    def test_ruler_refresh_keeps_new_final_line_and_audits_the_stale_duplicate(self: CampaignConversionTests) -> None:
        ruler = next(book for book in cast(list[Record], self.audit["books"])
                     if book["patientId"] == "former-ruler")
        active = {row["id"]: row for row in cast(list[Row], ruler["activeRows"])}
        final = active["P03_D3_R6_ACTION_009"]
        self.assertEqual(final["row"], "296")
        self.assertEqual(final["actionResponse"], "谢谢...这次不是客套。我是真的，想活着回家。")
        self.assertEqual(final["next"], "END_RELEASE")
        excluded = [row for row in cast(list[Record], self.audit["excludedRows"])
                    if row["patientId"] == "former-ruler"]
        self.assertEqual([row["row"] for row in excluded], [297])
        self.assertEqual(cast(Record, excluded[0]["source"])["actionResponse"],
                         "谢谢。今天这句...不用替我记得那么正式。")
        self.assertEqual(active["P03_D2_R5_OPTION_002"]["delta"], "-1")
        self.assertEqual(active["P03_D2_R6_OPTION_003"]["delta"], "-1")
        self.assertIn("P03_D1_R2_CLOSE_007", active)
        self.assertNotIn("P03_D1_R3_COMMON_004", active)
        self.assertNotIn("P03_D2_R4_CLOSE_006", active)

    def test_new_tutorial_is_complete_and_confirmed_priest_alias_keeps_source_evidence(self) -> None:
        books = {book["patientId"]: book for book in cast(list[Record], self.audit["books"])}
        tutorial = cast(list[Row], books["tutorial"]["activeRows"])
        self.assertEqual([int(row["row"]) for row in tutorial], list(range(2, 14)))
        self.assertEqual([row["open"] for row in tutorial if row["open"]],
                         ["嘿！", "你好啊。", "别害怕，我动不了。", "这玩意把我绑得严严实实呢。", "……",
                          "不过我发现了一件有趣的事。", "有些人啊，根本不需要绳子。", "……", "你说是不是？"])
        self.assertEqual(tutorial[-1]["next"], "END_DAY0")
        original = {int(row["row"]): row for row in cast(list[Row], books["priest"]["rows"])}
        aliases = [repair for repair in cast(list[Record], self.audit["repairs"])
                   if repair["kind"] == "user-confirmed-portrait-remap"]
        self.assertEqual([repair["row"] for repair in aliases], [178, 179])
        for repair in aliases:
            self.assertEqual(original[int(str(repair["row"]))]["L"], "JN05")
            self.assertEqual(cast(Record, repair["after"])["portrait"], "JN04")
        self.assertEqual(original[67]["L"], "JN03")
        priest_active = {int(row["row"]): row for row in cast(list[Row], books["priest"]["activeRows"])}
        self.assertEqual(priest_active[67]["portrait"], "JN03")
        self.assertNotIn("JN05", self.audit["referencedPortraits"])
        declared = {portrait["id"] for portrait in cast(list[Record], self.package["portraits"])}
        self.assertTrue(set(cast(list[str], self.audit["referencedPortraits"])) <= declared)

    def test_priest_terminal_continuation_is_one_linear_effect_free_response(self) -> None:
        priest = next(book for book in cast(list[Record], self.audit["books"]) if book["patientId"] == "priest")
        by_row = {int(row["row"]): row for row in cast(list[Row], priest["activeRows"])}
        self.assertEqual(by_row[60]["next"], by_row[67]["id"])
        self.assertEqual(by_row[67]["next"], "END_DAY1")
        self.assertEqual(by_row[67]["actionResponse"], "不会让你等待太久…")
        self.assertFalse(by_row[67]["option"] or by_row[67]["action"])
        self.assertEqual(int(by_row[67]["delta"] or "0"), 0)

    def test_special_entries_release_thresholds_and_implied_conditions_are_preserved(self) -> None:
        campaign = cast(Record, self.package["campaign"])
        meetings = [meeting for day in cast(list[Record], campaign["days"])
                    for meeting in cast(list[Record], day["meetings"])]
        gated = [meeting for meeting in meetings if "specialRoundStartNode" in meeting]
        self.assertEqual(len(gated), 7)
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

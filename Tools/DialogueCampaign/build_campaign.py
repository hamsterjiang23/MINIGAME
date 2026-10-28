"""Compile authoritative XLSX dialogue into a bounded, auditable V2 campaign package."""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import posixpath
import re
import sys
from pathlib import Path
from typing import cast
from xml.etree import ElementTree as ET
from zipfile import ZipFile

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(ROOT / "Tools/DialogueAtlas"))
from build_atlas import NS, REL_ID, read_workbook
from updated_source import apply_repairs, build_updated_meeting, normalize_rows, source_columns
from approved_text import apply_text_edits, manifest_hash

Row = dict[str, str]
Record = dict[str, object]


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def constant_rows(value: str) -> list[list[str]]:
    """Decode literal Excel string arrays only; never execute workbook expressions."""
    if not value.startswith("{") or not value.endswith("}"):
        return []
    tokens = re.findall(r'"(?:[^"]|"")*"|[,;]', value[1:-1])
    if "".join(tokens) != value[1:-1]:
        raise ValueError("Unsupported literal array syntax")
    rows: list[list[str]] = [[]]
    for token in tokens:
        if token == ";":
            rows.append([])
        elif token != ",":
            rows[-1].append(next(csv.reader([token]))[0])
    return rows


def read_source(path: Path) -> tuple[dict[str, list[Row]], dict[str, list[Row]], list[Record]]:
    cached = read_workbook(path)
    sheets = {name: [dict(row) for row in rows] for name, rows in cached.items()}
    recovered: list[Record] = []
    with ZipFile(path) as archive:
        relations = {r.attrib["Id"]: r.attrib["Target"] for r in ET.fromstring(archive.read("xl/_rels/workbook.xml.rels"))}
        workbook = ET.fromstring(archive.read("xl/workbook.xml")).find("s:sheets", NS)
        assert workbook is not None
        for sheet in workbook:
            name = sheet.attrib["name"]
            if name not in ("对话流程", "立绘ID"):
                continue
            target = relations[sheet.attrib[REL_ID]]
            target = target.lstrip("/") if target.startswith("/") else posixpath.normpath("xl/" + target)
            row_map = {int(row["row"]): row for row in sheets[name]}
            for cell in ET.fromstring(archive.read(target)).findall(".//s:c", NS):
                formula = cell.find("s:f", NS)
                if formula is None or not formula.text:
                    continue
                literal = constant_rows(formula.text)
                if not literal:
                    continue
                origin = re.fullmatch(r"([A-Z]+)(\d+)", cell.attrib["r"])
                assert origin is not None and len(origin[1]) == 1
                for offset, values in enumerate(literal):
                    number = int(origin[2]) + offset
                    row = row_map.setdefault(number, {"row": str(number)})
                    for index, value in enumerate(values):
                        column = chr(ord(origin[1]) + index)
                        if value and column not in row:
                            row[column] = value
                            recovered.append({"sheet": name, "cell": f"{column}{number}", "formulaCell": cell.attrib["r"], "value": value})
            sheets[name] = [row_map[number] for number in sorted(row_map)]
    return sheets, cached, recovered


def day_of(raw: str) -> int | None:
    match = re.search(r"(?:^|_)D(\d+)(?:_|$)", raw)
    return int(match[1]) if match else None


def action_of(row: Row) -> str:
    match = re.search(r"_(TREAT|PURIFY|RELEASE)$", row["J"])
    return match[1].lower() if match and day_of(row["J"]) is not None else ""


def is_option(row: Row) -> bool:
    return bool(action_of(row)) or row.get("D") == "玩家选项"


def escape(text: str) -> str:
    return re.sub(r"([\\#<>\[\]{}])", r"\\\1", text.replace("\r", " ").replace("\n", " "))


def build_meeting(patient: Record, rows: list[Row], day: int, mappings: list[Record], adaptations: list[Record],
                  campaign_day: int | None = None) -> Record:
    pid = str(patient["id"])
    by_raw = {r["J"]: r for r in rows}
    selected = [r for r in rows if day_of(r["J"]) == day]
    for row in list(selected):
        target = row.get("K", "")
        if target in by_raw and day_of(target) is None and by_raw[target] not in selected:
            selected.append(by_raw[target])
    end = f"{pid}__END_DAY{day}"

    def name(raw: str) -> str:
        suffix = f"__visit_day{day}" if raw in by_raw and day_of(raw) is None else ""
        return pid + "__" + raw + suffix

    groups: dict[str, list[Row]] = {}
    for row in selected:
        groups.setdefault(row.get("B", ""), []).append(row)
    gates: dict[str, str] = {}
    alias: dict[str, str] = {}
    for group, group_rows in groups.items():
        options = [row for row in group_rows if is_option(row)]
        if not options:
            continue
        measure = bool(action_of(options[0]))
        gate = f"{pid}__D{day}_{'MEASURE' if measure else group + '_CHOICE'}"
        gates[group] = gate
        for row in selected:
            target = row.get("K", "")
            if target not in by_raw and target and ((measure and target.endswith("_MEASURE")) or
                    (not measure and target == re.sub(r"_O\d+$", "_CHOICE", options[0]["J"]))):
                alias[target] = gate

    def target_of(row: Row) -> str:
        raw = row.get("K", "")
        if raw in alias:
            return alias[raw]
        if raw in by_raw:
            target_day = day_of(raw)
            if target_day is not None and target_day != day:
                adaptations.append({"kind": "cross-day-boundary", "sourceId": name(row["J"]), "originalTarget": raw, "meetingEnd": end})
                return end
            return name(raw)
        if raw.startswith("END_") or (not raw and day_of(row["J"]) is None):
            return end
        if not raw and row.get("B", "") in gates and not is_option(row):
            adaptations.append({"kind": "inferred-choice-hub", "sourceId": name(row["J"]), "group": row.get("B", ""), "target": gates[row.get("B", "")]})
            return gates[row.get("B", "")]
        raise ValueError(f"Unresolved target {pid}/{day}/{row['J']}: {raw}")

    nodes: list[Record] = []
    for row in selected:
        option = is_option(row)
        columns = [column for column in ("GHI" if option and pid == "diplomat" else "EGI" if not option else "") if row.get(column)]
        target = target_of(row)
        ids = [name(row["J"]) + ("__" + column if option or index else "") for index, column in enumerate(columns)]
        for index, column in enumerate(columns):
            node = {"id": ids[index], "kind": "narration" if day_of(row["J"]) is None else "line", "round": row.get("B", ""),
                    "portrait": row.get("M", ""), "speaker": "终端" if day_of(row["J"]) is None else patient["name"],
                    "text": row[column], "next": ids[index + 1] if index + 1 < len(ids) else target}
            nodes.append(node)
            mappings.append({"sourceId": ids[index], "patientId": pid, "day": day, "originalId": row["J"], "row": int(row["row"]), "column": column, "text": row[column], "kind": node["kind"]})
        if option:
            column = "F" if row.get("F") else "H"
            if not row.get(column):
                raise ValueError(f"Option missing text: {row['J']}")
            delta = int(row.get("L", "0"))
            nodes.append({"id": name(row["J"]), "kind": "option", "round": row.get("B", ""), "text": row[column],
                          "next": ids[0] if ids else target, "trust": delta, "action": action_of(row)})
            mappings.append({"sourceId": name(row["J"]), "patientId": pid, "day": day, "originalId": row["J"], "row": int(row["row"]), "column": column, "text": row[column], "kind": "option", "trustDelta": delta})
        elif not columns:
            raise ValueError(f"Empty source row: {row['J']}")
    for group, gate in gates.items():
        options = [n for n in nodes if n["kind"] == "option" and n["round"] == group]
        nodes.append({"id": gate, "kind": "measure" if options[0]["action"] else "choice", "round": group, "options": options})
    actual = [n for n in nodes if n["kind"] != "option"]
    node_map = {str(n["id"]): n for n in actual}
    start = name(selected[0]["J"])
    visited: set[str] = set()

    def visit(node_id: str, active: set[str]) -> None:
        if node_id == end:
            return
        if node_id in active:
            raise ValueError(f"Cycle: {node_id}")
        if node_id in visited:
            return
        node = node_map[node_id]
        for target in ([str(node["next"])] if "next" in node else [str(o["next"]) for o in cast(list[Record], node["options"])]):
            visit(target, active | {node_id})
        visited.add(node_id)

    visit(start, set())
    if visited != set(node_map):
        raise ValueError(f"Unreachable nodes: {set(node_map) - visited}")
    yarn: list[str] = []
    for node in actual:
        yarn.extend(["title: " + str(node["id"]), "kind: " + str(node["kind"]), "round: " + str(node["round"]),
                     "patient: " + pid, "day: " + str(day if campaign_day is None else campaign_day)])
        if node.get("portrait"):
            yarn.append("portrait: " + str(node["portrait"]))
        yarn.append("---")
        if "options" in node:
            for option in cast(list[Record], node["options"]):
                tags = " #id:" + str(option["id"]) + " #trust:" + str(option["trust"])
                if option["action"]:
                    tags += " #disposition:" + str(option["action"])
                yarn.extend(["-> " + escape(str(option["text"])) + tags, "    <<jump " + str(option["next"]) + ">>"])
        else:
            yarn.extend([str(node["speaker"]) + ": " + escape(str(node["text"])), "<<jump " + str(node["next"]) + ">>"])
        yarn.extend(["===", ""])
    return {"patientId": pid, "startNode": start, "endNode": end, "yarnSource": "\n".join(yarn)}


def build(smoke: bool = False, source_directory: Path | None = None) -> tuple[Record, Record]:
    config = cast(Record, json.loads((HERE / "conversion-config.json").read_text(encoding="utf-8-sig")))
    trust_adjustments: dict[str, Record] = {}
    for filename in ("trust-adjustments.json", "disposition-trust-adjustments.json"):
        trust_manifest = json.loads((HERE / filename).read_text(encoding="utf-8"))
        for adjustment in trust_manifest["adjustments"]:
            source_id = adjustment["sourceId"]
            if (source_id in trust_adjustments or not adjustment["expectedText"] or not adjustment["reason"]
                    or type(adjustment["expectedEffectiveDelta"]) is not int
                    or type(adjustment["trustDelta"]) is not int
                    or adjustment.get("expectedDisposition", "") not in ("", "treat", "purify", "release")):
                raise ValueError(f"Invalid or duplicate trust adjustment: {source_id}")
            trust_adjustments[source_id] = adjustment
    patients = cast(list[Record], config["patients"])
    if smoke:
        patients = [patient for patient in patients if patient["id"] in {"tutorial", "child"}]
    mappings: list[Record] = []
    adaptations: list[Record] = []
    books: list[Record] = []
    portraits: list[Record] = []
    excluded: list[Record] = []
    repairs: list[Record] = []
    text_edits: list[Record] = []
    schedule: list[Record] = []
    source_directory = source_directory or ROOT / "Art/chat_excel"
    days: list[Record] = [{"day": day, "quota": config["tutorialQuota"] if day == 0 else config["dailyQuota"], "meetings": []} for day in range(3 if smoke else 4)]
    for patient in patients:
        filename = str(patient["file"])
        if Path(filename).name != filename:
            raise ValueError(f"Patient source must be one explicit filename: {filename}")
        path = source_directory / filename
        if not path.is_file():
            raise ValueError(f"Missing patient source: {path}")
        sheets, cached, recovered = read_source(path)
        rows = sheets["对话流程"][1:]
        columns = source_columns(sheets["对话流程"][0])
        if columns:
            normalized = normalize_rows(rows, columns)
            active, source_repairs, source_excluded = apply_repairs(
                str(patient["id"]), digest(path), normalized, HERE / "source-repairs.json")
            repairs.extend(source_repairs)
            excluded.extend(source_excluded)
            active, approved = apply_text_edits(str(patient["id"]), digest(path), active)
            text_edits.extend(approved)
        else:
            assert len({r["J"] for r in rows}) == len(rows)
            active = rows
        for row in sheets["立绘ID"][1:]:
            portraits.append({"id": row["A"], "label": row["B"]})
        file = path.relative_to(ROOT).as_posix() if path.is_relative_to(ROOT) else path.as_posix()
        book = {"patientId": patient["id"], "file": file, "sha256": digest(path), "rows": rows,
                "cachedRows": cached["对话流程"][1:], "recoveredCells": recovered,
                "schemaColumns": columns, "activeRows": active}
        books.append(book)
        for source_day in range(len(days)):
            if (patient["id"] == "tutorial") != (source_day == 0):
                continue
            campaign_day = 0 if patient["id"] == "tutorial" else int(str(patient["firstDay"])) + source_day - 1
            mapping_start, adaptation_start = len(mappings), len(adaptations)
            meeting = (build_updated_meeting(patient, active, source_day, columns, mappings, adaptations, campaign_day,
                                             trust_adjustments)
                       if columns else build_meeting(patient, active, source_day, mappings, adaptations, campaign_day))
            scheduled = campaign_day < len(days)
            # The workbook day is the patient's visit day. Keep unused future visits in the audit,
            # while the playable campaign remains Day0 plus three working days.
            for item in mappings[mapping_start:] + adaptations[adaptation_start:]:
                item.update({"sourceDay": source_day, "campaignDay": campaign_day, "scheduled": scheduled})
            schedule.append({"patientId": patient["id"], "sourceDay": source_day, "campaignDay": campaign_day,
                             "scheduled": scheduled, "startNode": meeting["startNode"], "endNode": meeting["endNode"]})
            if scheduled:
                cast(list[Record], days[campaign_day]["meetings"]).append(meeting)
    variants = json.loads((HERE / "text-variants.json").read_text(encoding="utf-8-sig")) if not smoke else []
    rules = [{"id": p["id"], "name": p["name"], "firstDay": p["firstDay"], "initialTrust": p.get("initialTrust", config["initialTrust"]),
              "treatmentLimit": 3, "treatmentOutcome": p["treatmentOutcome"], "treatmentDelayDays": p.get("treatmentDelayDays", 0),
              "attacksAfterRelease": p.get("attacksAfterRelease", False)} for p in patients]
    sources = [str(m["yarnSource"]) for d in days for m in cast(list[Record], d["meetings"])]
    tutorial = cast(list[Record], days[0]["meetings"])[0]
    tutorial_trust = next(rule["initialTrust"] for rule in rules if rule["id"] == tutorial["patientId"])
    package = {"formatVersion": 2, "packageId": "inspection-campaign", "contentVersion": config["contentVersion"], "yarnSource": "\n".join(sources), "portraits": portraits,
               "profile": {"patientId": "tutorial", "patientName": "无名男子", "day": 0, "startNode": tutorial["startNode"], "endNode": tutorial["endNode"], "initialTrust": tutorial_trust, "releaseThreshold": config["releaseThreshold"], "initialQuota": config["tutorialQuota"]},
               "campaign": {"patients": rules, "days": days, "releaseThreshold": config["releaseThreshold"], "variants": variants}}
    all_mappings = {str(m["sourceId"]): m for m in mappings}
    if not smoke:
        adjusted_ids = {str(item["sourceId"]) for item in adaptations
                        if item["kind"] == "user-approved-trust-adjustment"}
        if adjusted_ids != set(trust_adjustments):
            raise ValueError(f"Unapplied trust adjustments: {set(trust_adjustments) - adjusted_ids}")
    for variant in variants:
        assert variant["sourceId"] in all_mappings, variant
    for book in books:
        id_column = "id" if book["schemaColumns"] else "J"
        for row in cast(list[Row], book["activeRows"]):
            if smoke and day_of(row[id_column]) == 3:
                continue
            assert any(m["patientId"] == book["patientId"] and m["originalId"] == row[id_column]
                       and m["row"] == int(row["row"]) for m in mappings), row
    audit = {"sourceRowCount": sum(len(cast(list[Row], b["rows"])) for b in books), "books": books, "mappings": mappings, "adaptations": adaptations,
             "meetingSchedule": schedule,
             "activeSourceRowCount": sum(len(cast(list[Row], b["activeRows"])) for b in books),
             "excludedSourceRowCount": len(excluded), "excludedRows": excluded, "repairs": repairs,
             "repairManifestHash": digest(HERE / "source-repairs.json"),
             "textEditManifestHash": manifest_hash(), "approvedTextEdits": text_edits,
             "trustAdjustmentHash": digest(HERE / "trust-adjustments.json"),
             "dispositionTrustAdjustmentHash": digest(HERE / "disposition-trust-adjustments.json"),
             "configHash": digest(HERE / "conversion-config.json"), "variantHash": digest(HERE / "text-variants.json"),
             "meetingCount": sum(len(cast(list[Record], d["meetings"])) for d in days), "declaredPortraits": len(portraits),
             "referencedPortraits": sorted({r["portrait" if b["schemaColumns"] else "M"]
                                            for b in books for r in cast(list[Row], b["activeRows"])
                                            if r.get("portrait" if b["schemaColumns"] else "M")})}
    return package, audit


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--smoke", action="store_true")
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    package, audit = build(args.smoke)
    output = ROOT / "UnityProject/Assets/MiniGame/Content/Campaign"
    artifacts = {"campaign.minigame.json": package, "provenance.json": audit}
    if not args.smoke:
        for filename, data in artifacts.items():
            text = json.dumps(data, ensure_ascii=False, indent=2) + "\n"
            if args.check:
                assert (output / filename).read_text(encoding="utf-8") == text, filename
            else:
                output.mkdir(parents=True, exist_ok=True)
                (output / filename).write_text(text, encoding="utf-8", newline="\n")
        yarn = str(package["yarnSource"])
        if args.check:
            assert (output / "campaign.yarn").read_text(encoding="utf-8") == yarn
        else:
            (output / "campaign.yarn").write_text(yarn, encoding="utf-8", newline="\n")
    print(json.dumps({key: audit[key] for key in ("sourceRowCount", "meetingCount", "declaredPortraits", "referencedPortraits")}, ensure_ascii=False))


if __name__ == "__main__":
    main()

"""Compile isolated ending conversations from eight pinned books and one full cloud snapshot."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
from typing import cast

from build_campaign import HERE, ROOT, Record, Row, digest, escape, read_source
from build_supervisor import compile_dialogue

SOURCES = ROOT / "docs/ending-content/sources/source-refresh.json"
OUTPUT = ROOT / "UnityProject/Assets/MiniGame/Resources/MiniGame/Endings"
AUDIT = ROOT / "docs/ending-content/provenance.json"


def canonical_hash(value: object) -> str:
    return hashlib.sha256(json.dumps(value, ensure_ascii=False, sort_keys=True,
                                     separators=(",", ":")).encode("utf-8")).hexdigest()


def verified_sources() -> dict[str, Record]:
    manifest = cast(Record, json.loads(SOURCES.read_text(encoding="utf-8")))
    sources = cast(list[Record], manifest["sources"])
    expected = {"tutorial", "former-ruler", "battlefield-doctor", "child", "priest", "diplomat",
                "supervisor", "priest-ending", "self-review"}
    if len(sources) != 9 or {entry["id"] for entry in sources} != expected:
        raise ValueError("Expected all nine explicitly pinned content sources.")
    for entry in sources:
        if digest(ROOT / str(entry["file"])) != entry["sha256"]:
            raise ValueError(f"Pinned source changed: {entry['id']}")
    return {str(entry["id"]): entry for entry in sources}


def read_snapshot(entry: Record) -> dict[str, list[Row]]:
    snapshot = cast(Record, json.loads((ROOT / str(entry["file"])).read_text(encoding="utf-8")))
    if snapshot["revision"] != 18 or entry["revision"] != 18:
        raise ValueError("Self-review source revision must be the reviewed revision18.")
    sheets = cast(list[Record], snapshot["sheets"])
    expected = {"对话流程": 500, "角色文本表": 500, "玩家选项表": 500, "立绘ID": 200}
    if len(sheets) != 4 or {sheet["name"] for sheet in sheets} != set(expected):
        raise ValueError("Self-review snapshot must include all four full sheets.")
    result: dict[str, list[Row]] = {}
    for sheet in sheets:
        name = str(sheet["name"])
        path = ROOT / str(sheet["rawFile"])
        if digest(path) != sheet["rawSha256"]:
            raise ValueError(f"Self-review raw response changed: {name}")
        envelope = cast(Record, json.loads(path.read_text(encoding="utf-8-sig")))
        data = cast(Record, envelope["data"])
        if not envelope["ok"] or data["has_more"] or data["revision"] != 18:
            raise ValueError(f"Incomplete or mixed-revision cloud response: {name}")
        areas = cast(list[Record], data["ranges"])
        if len(areas) != 1:
            raise ValueError(f"Unexpected cloud ranges: {name}")
        area = areas[0]
        row_count = expected[name]
        if (area["truncated"] or area["row_indices"] != list(range(1, row_count + 1))
                or area["col_indices"] != list("ABCDEFGHIJKLMNOPQRST")
                or data["returned_cell_count"] != row_count * 20):
            raise ValueError(f"Cloud sheet was truncated: {name}")
        rows: list[Row] = []
        cells = cast(list[list[Record]], area["cells"])
        for number, values in zip(cast(list[int], area["row_indices"]), cells, strict=True):
            row: Row = {"row": str(number)}
            for column, cell in zip(cast(list[str], area["col_indices"]), values, strict=True):
                if "value" in cell and str(cell["value"]):
                    row[column] = str(cell["value"])
            if len(row) > 1:
                rows.append(row)
        if rows != sheet["rows"]:
            raise ValueError(f"Normalized snapshot does not match raw cells: {name}")
        result[name] = rows
    return result


def linear_conversation(identifier: str, node: str, speaker: str, sheets: dict[str, list[Row]],
                        portrait_map: dict[str, str], expected_lines: int, final_action: str = "") -> Record:
    rows = sheets["对话流程"][1:]
    by_id = {row.get("I", ""): row for row in rows}
    if "" in by_id or len(by_id) != len(rows) or len(rows) != expected_lines + bool(final_action):
        raise ValueError(f"Ending source has missing/duplicate/unexpected rows: {identifier}")
    declared = {row["A"] for row in sheets["立绘ID"][1:]}
    if declared != set(portrait_map):
        raise ValueError(f"Ending portrait definitions changed: {identifier}/{declared}")
    lines: list[Record] = []
    for index, row in enumerate(rows):
        number = int(row["row"])
        expected_next = rows[index + 1]["I"] if index + 1 < len(rows) else ""
        if row.get("J", "") != expected_next or int(row.get("K") or "0") != 0 or row.get("M"):
            raise ValueError(f"Ending requires a linear, effect-free source: {identifier}/{number}")
        if row.get("L", "") not in portrait_map:
            raise ValueError(f"Unknown ending portrait: {identifier}/L{number}")
        if final_action and index == expected_lines:
            if row.get("G") != "净化" or any(row.get(column) for column in "DEFH"):
                raise ValueError("The self-review ending must have exactly one final purification choice.")
            continue
        if not row.get("D") or any(row.get(column) for column in "EFGH"):
            raise ValueError(f"Expected one authored ending line at {identifier}/D{number}")
        portrait = portrait_map[row["L"]]
        lines.append({"id": row["I"], "sourceId": identifier + "__" + row["I"],
                      "sourceRow": number, "sourceCell": f"D{number}", "text": row["D"],
                      "portraitId": portrait, "blackScreen": row["L"] == "ED_03",
                      "originalPortraitId": row["L"], "originalNextId": row.get("J", "")})
    result: Record = {"id": identifier, "node": node, "speaker": speaker, "lines": lines,
                      "finalAction": final_action}
    if final_action:
        action = rows[-1]
        result.update({"finalActionSourceId": identifier + "__" + action["I"],
                       "finalActionSourceRow": int(action["row"]), "finalActionSourceCell": "G" + action["row"],
                       "finalActionText": action["G"], "finalActionPortraitId": portrait_map[action["L"]]})
    return result


def build() -> tuple[Record, Record]:
    sources = verified_sources()
    priest, _, recovered = read_source(ROOT / str(sources["priest-ending"]["file"]))
    self_review = read_snapshot(sources["self-review"])
    conversations = [
        linear_conversation("priest-ending", "PriestEnding", "乔纳", priest,
                            {"ED_01": "PE01", "ED_02": "PE02", "ED_03": ""}, 8),
        linear_conversation("self-review", "SelfReview", "玩家自己", self_review,
                            {"SJ_01": "SJ01", "SJ_02": "SJ02"}, 16, "purify"),
    ]
    supervisor, supervisor_adaptations = compile_dialogue(ROOT / str(sources["supervisor"]["file"]))
    day_four = next(day for day in cast(list[Record], supervisor["conversations"]) if day["day"] == 4)
    ending_lines: list[Record] = []
    for line in cast(list[Record], day_four["lines"]):
        ending_lines.append({"id": line["id"], "sourceId": "supervisor-ending__" + str(line["id"]),
                             "sourceRow": line["sourceRow"], "sourceCell": f"D{line['sourceRow']}",
                             "text": line["text"], "portraitId": str(line["portraitId"]).replace("_", ""),
                             "blackScreen": False, "originalPortraitId": line["portraitId"],
                             "originalNextId": line["next"]})
    conversations.append({"id": "supervisor-ending", "node": "SupervisorEnding", "speaker": "监督员",
                          "lines": ending_lines, "finalAction": ""})
    config = cast(Record, json.loads((HERE / "conversion-config.json").read_text(encoding="utf-8")))
    hashes = [{"id": key, "sha256": entry["sha256"]} for key, entry in sorted(sources.items())]
    body: Record = {"formatVersion": 1, "contentVersion": config["contentVersion"],
                    "sources": hashes, "conversations": conversations}
    package: Record = {**body, "revision": canonical_hash(body)}
    audit: Record = {"revision": package["revision"], "sourceManifestSha256": digest(SOURCES),
                    "sources": [{key: value for key, value in entry.items() if key != "sheets"}
                                for entry in sources.values()],
                    "sheetCoverage": [{"source": key, "sheets": entry["sheets"]} for key, entry in sources.items()],
                    "lineCounts": {str(item["id"]): len(cast(list[Record], item["lines"])) for item in conversations},
                    "adaptations": [
                        {"kind": "namespace", "sources": ["priest-ending", "self-review"],
                         "reason": "Both authored books reuse CUSTOM001 IDs; namespaced source IDs are unique."},
                        {"kind": "portrait-remap", "source": "priest-ending", "map": {"ED_01": "PE01", "ED_02": "PE02", "ED_03": "black-screen"}},
                        {"kind": "portrait-remap", "source": "self-review", "map": {"SJ_01": "SJ01", "SJ_02": "SJ02"}},
                        {"kind": "source-day-is-template", "sources": ["priest-ending", "self-review"],
                         "reason": "Authored Day1 labels are template metadata; explicit ending files are routed by GameSession."},
                        {"kind": "ending-boundary", "source": "supervisor", "rows": [line["sourceRow"] for line in ending_lines],
                         "reason": "Authored Day4 is ending content, outside patient daily campaign meetings."},
                        *[{"source": "supervisor", **item} for item in supervisor_adaptations],
                        *[{"source": "priest-ending", "kind": "literal-array-recovered", **item} for item in recovered],
                    ]}
    return package, audit


def yarn_source(package: Record) -> str:
    lines: list[str] = []
    for conversation in cast(list[Record], package["conversations"]):
        lines.extend(["title: " + str(conversation["node"]), "---"])
        for line in cast(list[Record], conversation["lines"]):
            tags = f" #line:{line['sourceId']} #source:{line['sourceId']}"
            tags += " #blackScreen:true" if line["blackScreen"] else f" #portrait:{line['portraitId']}"
            lines.append(f"{conversation['speaker']}: {escape(str(line['text']))}{tags}")
        if conversation["finalAction"]:
            lines.append(f"-> {conversation['finalActionText']} #id:{conversation['finalActionSourceId']} #disposition:purify")
        lines.extend(["===", ""])
    return "\n".join(lines)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--smoke", action="store_true")
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    package, audit = build()
    artifacts = {OUTPUT / "Dialogue.json": json.dumps(package, ensure_ascii=False, indent=2) + "\n",
                 OUTPUT / "Flow.yarn": yarn_source(package),
                 AUDIT: json.dumps(audit, ensure_ascii=False, indent=2) + "\n"}
    if not args.smoke:
        for path, text in artifacts.items():
            if args.check:
                if path.read_text(encoding="utf-8") != text:
                    raise ValueError(f"Generated ending artifact differs: {path}")
            else:
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(text, encoding="utf-8", newline="\n")
    print(json.dumps({"revision": package["revision"], "lineCounts": audit["lineCounts"], "sources": 9}, ensure_ascii=False))


if __name__ == "__main__":
    main()

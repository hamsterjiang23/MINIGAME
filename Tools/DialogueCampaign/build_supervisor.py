"""Import the supervisor's source graph without changing patient rules or the workbook."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path
from typing import cast

from build_campaign import Record, escape, read_source
from approved_text import apply_text_edits, content_revision, manifest_hash

ROOT = Path(__file__).resolve().parents[2]


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def compile_dialogue(workbook: Path) -> tuple[dict[str, object], list[dict[str, object]]]:
    sheets, _, recovered = read_source(workbook)
    rows, text_edits = apply_text_edits("supervisor", digest(workbook), sheets["对话流程"][1:],
                                      id_column="I", columns={"open": "D"})
    portraits = {row["A"]: row.get("B", "") for row in sheets["立绘ID"][1:]}
    if set(portraits) != {f"JD_{index:02}" for index in range(1, 7)}:
        raise ValueError("Expected exactly six JD portrait identifiers.")
    nodes: dict[str, dict[str, object]] = {}
    adaptations: list[dict[str, object]] = []
    for row in rows:
        if not row.get("I"):
            continue
        if row["I"] in nodes or row.get("C") != "普通对话" or any(row.get(column) for column in ("E", "F", "G", "H", "M")):
            raise ValueError(f"Unsupported or duplicate supervisor node at row {row['row']}.")
        match = re.fullmatch(r"Day([1-4])", row["A"])
        if not match or not row.get("D") or row.get("K", "0") != "0":
            raise ValueError(f"Unsupported day/text/side effect at row {row['row']}.")
        nodes[row["I"]] = {"id": row["I"], "day": int(match[1]), "text": row["D"],
            "next": row.get("J", ""), "portraitId": row.get("L", ""), "sourceRow": int(row["row"])}
    conversations: list[dict[str, object]] = []
    for day in range(1, 5):
        daily = {key: node for key, node in nodes.items() if node["day"] == day}
        incoming = {str(node["next"]) for node in daily.values() if node["next"] in daily}
        starts = set(daily) - incoming
        if len(starts) != 1:
            raise ValueError(f"Day{day} needs one unambiguous start node.")
        current = starts.pop()
        seen: set[str] = set()
        lines: list[dict[str, object]] = []
        previous_portrait = "JD_01"
        while current:
            if current in seen or current not in daily:
                raise ValueError(f"Invalid cycle or target in Day{day}: {current}")
            seen.add(current)
            node = dict(daily[current])
            portrait = str(node["portraitId"])
            if not portrait:
                portrait = previous_portrait
                adaptations.append({"cell": f"L{node['sourceRow']}", "kind": "inherit-portrait", "value": portrait})
            if portrait not in portraits:
                raise ValueError(f"Unmapped portrait: {portrait}")
            node["portraitId"] = portrait
            previous_portrait = portrait
            lines.append(node)
            target = str(node["next"])
            if target and target not in nodes:
                raise ValueError(f"Unknown next node: {target}")
            if target and nodes[target]["day"] != day:
                adaptations.append({"cell": f"J{node['sourceRow']}", "kind": "day-boundary", "originalNext": target,
                    "policy": "End this day's presentation; Day4 starts at its own root in the TakenAway ending."})
                break
            current = target
        if seen != set(daily):
            raise ValueError(f"Unreachable source nodes in Day{day}: {set(daily) - seen}")
        conversations.append({"day": day, "morningCount": len(lines) - 2 if day < 4 else 0, "lines": lines})
    if len(nodes) != 48:
        raise ValueError(f"Expected 48 authored supervisor lines, found {len(nodes)}.")
    result: dict[str, object] = {"revision": "supervisor-v1:" + content_revision("supervisor", digest(workbook)), "conversations": conversations,
        "textEditManifestHash": manifest_hash(), "approvedTextEdits": text_edits,
        "portraits": [{"id": key, "description": value} for key, value in portraits.items()]}
    adaptations.extend({"kind": "literal-array-recovered", **item} for item in recovered)
    adaptations.extend(text_edits)
    return result, adaptations


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workbook", type=Path, default=ROOT / "Art/chat_excel/【对话】监督员.xlsx")
    parser.add_argument("--smoke", action="store_true")
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    result, adaptations = compile_dialogue(args.workbook)
    resource = ROOT / "UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor"
    yarn: list[str] = []
    conversations = cast(list[dict[str, object]], result["conversations"])
    for conversation in conversations:
        day = conversation["day"]
        lines = cast(list[dict[str, object]], conversation["lines"])
        split = int(str(conversation["morningCount"]))
        stages = [("MorningInspection", lines[:split]), ("Briefing", lines[split:])] if int(str(day)) < 4 else [("Ending", lines)]
        for stage, selected in stages:
            yarn.extend([f"title: Supervisor_D{day}_{stage}", "---"])
            for line in selected:
                yarn.append(f"监督员: {escape(str(line['text']))} #line:supervisor_{line['id']} #source:supervisor__{line['id']} #portrait:{line['portraitId']}")
            yarn.extend(["===", ""])
    manifest = {"workbook": args.workbook.relative_to(ROOT).as_posix(), "sha256": digest(args.workbook),
        "sheet": "对话流程", "sourceRows": sum(len(cast(list[Record], day["lines"])) for day in conversations), "adaptations": adaptations,
        "portraitPolicy": "Preserve the approved live portraits in docs/portrait-remap/source-manifest.json; this text converter never copies images.",
        "stagePolicy": "For Day1-3, the final two authored lines use the existing Briefing cue; preceding lines use MorningInspection. Day4 is TakenAway ending content."}
    artifacts = {
        resource / "Dialogue.json": json.dumps(result, ensure_ascii=False, indent=2) + "\n",
        resource / "Flow.yarn": "\n".join(yarn),
        resource / "DialogueProject.yarnproject": json.dumps({"projectFileVersion": 3,
            "sourceFiles": ["Flow.yarn"], "excludeFiles": [], "baseLanguage": "zh-Hans", "localisation": {}}, indent=2) + "\n",
        ROOT / "docs/ending-content/supervisor-source-manifest.json": json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
    }
    if not args.smoke:
        for path, text in artifacts.items():
            if args.check:
                if path.read_text(encoding="utf-8") != text:
                    raise ValueError(f"Generated supervisor artifact differs: {path}")
            else:
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(text, encoding="utf-8", newline="\n")
    print(json.dumps({"days": [len(cast(list[object], day["lines"])) for day in conversations], "adaptations": adaptations}, ensure_ascii=False))


if __name__ == "__main__":
    main()

"""Normalize the authored 2026-10 workbook schema and apply pinned, audited repairs."""
from __future__ import annotations

import json
import re
from pathlib import Path
from typing import cast

Row = dict[str, str]
Record = dict[str, object]
TEXT_FIELDS = ("open", "option", "response", "action", "actionResponse")


def source_columns(header: Row) -> dict[str, str]:
    """Detect the ID column rather than relying on unreliable XLSX dimensions."""
    id_column = next((column for column, text in header.items() if text == "节点/选项ID"), "")
    if id_column == "I":
        return dict(zip(
            ("day", "round", "type", *TEXT_FIELDS, "id", "next", "delta", "portrait", "condition"),
            "ABCDEFGHIJKLM",
        ))
    if id_column == "J" and header.get("N") == "触发条件":
        return {"day": "A", "round": "B", "type": "C", "open": "E", "option": "F",
                "response": "G", "action": "H", "actionResponse": "I", "id": "J",
                "next": "K", "delta": "L", "portrait": "M", "condition": "N"}
    return {}


def normalize_rows(rows: list[Row], columns: dict[str, str]) -> list[Row]:
    return [{"row": row["row"], **{field: row.get(column, "") for field, column in columns.items()}}
            for row in rows if any(row.get(column) for column in columns.values())]


def split_targets(value: str) -> list[str]:
    return [target.strip() for target in re.split("[,，]", value) if target.strip()]


def action_of(row: Row) -> str:
    label = row.get("action", "").strip()
    if not label:
        return ""
    actions = {"治疗": "treat", "继续治疗": "treat", "治疗（第3次）": "treat",
               "净化": "purify", "释放": "release"}
    if label not in actions:
        raise ValueError(f"Unknown authored disposition at row {row['row']}: {label}")
    return actions[label]


def day_of(row: Row) -> int:
    match = re.search(r"(?:^|_)D(\d+)(?:_|$)", row["id"])
    if match is None:
        raise ValueError(f"Missing day in ID: {row['id']}")
    return int(match[1])


def apply_repairs(patient_id: str, source_hash: str, rows: list[Row],
                  manifest_path: Path) -> tuple[list[Row], list[Record], list[Record]]:
    """Reject drift in any repaired workbook; never silently repair a future revision."""
    manifest = cast(Record, json.loads(manifest_path.read_text(encoding="utf-8")))
    specs = cast(dict[str, Record], manifest["books"])
    if patient_id not in specs:
        return [dict(row) for row in rows], [], []
    spec = specs[patient_id]
    if source_hash != spec["sourceSha256"]:
        raise ValueError(f"Repair manifest does not match {patient_id} workbook SHA256")
    by_row = {int(row["row"]): row for row in rows}
    excluded: list[Record] = []
    exclude_numbers: set[int] = set()
    for exclusion in cast(list[Record], spec["excludedRanges"]):
        numbers = range(int(str(exclusion["firstRow"])), int(str(exclusion["lastRow"])) + 1)
        exceptions = cast(list[int], exclusion.get("exceptRows", []))
        for number in numbers:
            if number in exceptions:
                continue
            if number not in by_row or number in exclude_numbers:
                raise ValueError(f"Invalid exclusion {patient_id}/{number}")
            exclude_numbers.add(number)
            excluded.append({"patientId": patient_id, "row": number, "originalId": by_row[number]["id"],
                             "reason": exclusion["reason"], "source": dict(by_row[number])})
    active = [dict(row) for row in rows if int(row["row"]) not in exclude_numbers]
    active_by_row = {int(row["row"]): row for row in active}
    repairs: list[Record] = []
    for repair in cast(list[Record], spec["repairs"]):
        number = int(str(repair["row"]))
        row = active_by_row[number]
        expected = cast(dict[str, str], repair["expected"])
        if any(row.get(field, "") != value for field, value in expected.items()):
            raise ValueError(f"Repair precondition failed {patient_id}/{number}")
        changes = cast(dict[str, str], repair["set"])
        before = {field: row.get(field, "") for field in changes}
        row.update(changes)
        repairs.append({"patientId": patient_id, "row": number, "originalId": row["id"],
                        "kind": repair["kind"], "before": before, "after": changes,
                        "evidence": repair["evidence"]})
    ids = [row["id"] for row in active]
    if len(ids) != len(set(ids)):
        raise ValueError(f"Active duplicate IDs remain in {patient_id}")
    return active, repairs, excluded


def build_updated_meeting(patient: Record, rows: list[Row], day: int,
                          columns: dict[str, str], mappings: list[Record],
                          adaptations: list[Record]) -> Record:
    pid = str(patient["id"])
    selected = [row for row in rows if day_of(row) == day]
    if not selected:
        raise ValueError(f"No source rows for {pid}/Day{day}")
    by_id = {row["id"]: row for row in selected}
    end = f"{pid}__END_DAY{day}"

    def name(raw: str) -> str:
        return pid + "__" + raw

    def is_option(row: Row) -> bool:
        return bool(row.get("option") or row.get("action"))

    special_entries: set[str] = set()
    gates: dict[tuple[str, ...], Record] = {}
    destinations: dict[str, str] = {}

    def gate_for(targets: list[str]) -> str:
        options = [by_id[target] for target in targets]
        kinds = {bool(action_of(row)) for row in options}
        if len(kinds) != 1 or not all(is_option(row) for row in options):
            raise ValueError(f"Mixed choice kinds at {pid}/{targets}")
        # Sources sometimes list the same measure options in different orders.
        # One stable action menu is sufficient; ordinary option order is authored.
        if True in kinds:
            targets = sorted(targets, key=lambda target: int(by_id[target]["row"]))
        key = tuple(targets)
        if key not in gates:
            first = by_id[targets[0]]
            gate_id = name(first["id"]) + "__MENU"
            gates[key] = {"id": gate_id, "kind": "measure" if True in kinds else "choice",
                          "round": first["round"], "targets": targets}
        return str(gates[key]["id"])

    for row in selected:
        targets = split_targets(row["next"])
        if not targets:
            raise ValueError(f"Unrepaired blank target {pid}/{row['row']}/{row['id']}")
        live = [target for target in targets if target in by_id]
        terminals = [target for target in targets if target.startswith("END_")]
        unknown = set(targets) - set(live) - set(terminals)
        if unknown:
            raise ValueError(f"Unknown authored targets {pid}/{row['id']}: {unknown}")
        option_targets = [target for target in live if is_option(by_id[target])]
        line_targets = [target for target in live if not is_option(by_id[target])]
        if option_targets and line_targets:
            if len(line_targets) != 1 or not all(action_of(by_id[target]) for target in option_targets):
                raise ValueError(f"Unsupported mixed branch {pid}/{row['id']}")
            special = line_targets[0]
            if by_id[special].get("condition") != "信任值>=3":
                raise ValueError(f"Optional special entry lacks supported condition: {special}")
            special_entries.add(special)
            gate_for(option_targets)
            destinations[row["id"]] = name(special)
            adaptations.append({"kind": "conditional-special-route", "patientId": pid, "day": day,
                                "row": int(row["row"]), "originalId": row["id"],
                                "originalTargets": targets, "specialEntry": name(special),
                                "skipTo": gate_for(option_targets), "minimumTrust": 3})
        elif option_targets:
            destinations[row["id"]] = gate_for(option_targets)
        elif len(line_targets) == 1 and not terminals:
            destinations[row["id"]] = name(line_targets[0])
        elif terminals and not line_targets:
            destinations[row["id"]] = end
        else:
            raise ValueError(f"Unsupported multiple line/terminal targets {pid}/{row['id']}: {targets}")

    start_row = selected[0]
    start = gate_for([start_row["id"]]) if is_option(start_row) else name(start_row["id"])
    if len(special_entries) > 1:
        raise ValueError(f"Multiple optional special entries in {pid}/Day{day}")
    special_entry = next(iter(special_entries), "")
    release_thresholds = {int(row["condition"].split(">=")[1]) for row in selected
                          if action_of(row) == "release" and row.get("condition")}
    if len(release_thresholds) > 1:
        raise ValueError(f"Conflicting release thresholds in {pid}/Day{day}")
    nodes: dict[str, Record] = {}
    options: dict[str, Record] = {}
    transferred_deltas: dict[str, int] = {}
    line_delta_destinations: dict[str, str] = {}
    for row in selected:
        delta = int(row.get("delta") or "0")
        if not delta or is_option(row):
            continue
        predecessors = [source for source in selected if row["id"] in split_targets(source["next"])]
        if len(predecessors) != 1 or not is_option(predecessors[0]) or row.get("condition"):
            raise ValueError(f"Cannot safely apply line trust delta {pid}/{row['row']}: {delta}")
        predecessor = predecessors[0]
        if split_targets(predecessor["next"]) != [row["id"]]:
            raise ValueError(f"Conditional predecessor of line trust delta {pid}/{row['row']}")
        transferred_deltas[predecessor["id"]] = transferred_deltas.get(predecessor["id"], 0) + delta
        line_delta_destinations[row["id"]] = name(predecessor["id"])
        adaptations.append({"kind": "line-trust-on-unique-answer", "patientId": pid, "day": day,
                            "row": int(row["row"]), "originalId": row["id"], "authoredDelta": delta,
                            "sourceCell": columns["delta"] + row["row"],
                            "appliedTo": name(predecessor["id"]), "predecessorRow": int(predecessor["row"]),
                            "authoredOptionDelta": int(predecessor.get("delta") or "0"),
                            "effectiveOptionDelta": int(predecessor.get("delta") or "0") + delta,
                            "reason": "The answer has this sole immediate response; no intervening condition or choice."})
    for row in selected:
        present = [field for field in TEXT_FIELDS if row.get(field)]
        if len(present) != 1:
            raise ValueError(f"Expected one authored text field at {pid}/{row['row']}: {present}")
        field = present[0]
        option = is_option(row)
        delta = int(row.get("delta") or "0")
        kind = "option" if option else "narration" if row.get("narration") == "true" else "line"
        node: Record = {"id": name(row["id"]), "kind": kind, "round": row["round"],
                        "text": row[field], "next": destinations[row["id"]],
                        "portrait": row.get("portrait", "")}
        if option:
            node.update({"trust": delta + transferred_deltas.get(row["id"], 0), "action": action_of(row)})
            options[row["id"]] = node
        else:
            node["speaker"] = "终端" if kind == "narration" else patient["name"]
            nodes[str(node["id"])] = node
        mapping: Record = {"sourceId": node["id"], "patientId": pid, "day": day,
                           "originalId": row["id"], "row": int(row["row"]), "column": columns[field],
                           "text": row[field], "kind": kind, "authoredDelta": row.get("delta", ""),
                           "deltaColumn": columns["delta"]}
        if option:
            mapping["trustDelta"] = node["trust"]
        if row["id"] in line_delta_destinations:
            mapping["trustAppliedTo"] = line_delta_destinations[row["id"]]
        mappings.append(mapping)
        condition = row.get("condition", "")
        if condition:
            if action_of(row) == "release" and condition == "信任值>=5":
                pass
            elif not special_entry or condition != "信任值>=3":
                raise ValueError(f"Unimplemented condition {pid}/{row['row']}: {condition}")
    for gate in gates.values():
        gate["options"] = [options[raw] for raw in cast(list[str], gate["targets"])]
        nodes[str(gate["id"])] = gate

    visited: set[str] = set()
    used_options: set[str] = set()

    def visit(node_id: str, stack: set[str]) -> None:
        if node_id == end:
            return
        if node_id in stack:
            raise ValueError(f"Cycle: {node_id}")
        if node_id in visited:
            return
        node = nodes[node_id]
        if "options" in node:
            choices = cast(list[Record], node["options"])
            used_options.update(str(option["id"]) for option in choices)
            targets = [str(option["next"]) for option in choices]
        else:
            targets = [str(node["next"])]
        for target in targets:
            visit(target, stack | {node_id})
        visited.add(node_id)

    visit(start, set())
    if visited != set(nodes) or used_options != {str(option["id"]) for option in options.values()}:
        raise ValueError(f"Unreachable active source in {pid}/Day{day}: "
                         f"{set(nodes) - visited}; options={set(options) - {key.split('__', 1)[-1] for key in used_options}}")
    if special_entry:
        minimum_trust: dict[str, int] = {}

        def propagate(node_id: str, trust: int) -> None:
            if node_id == end or (node_id in minimum_trust and minimum_trust[node_id] <= trust):
                return
            minimum_trust[node_id] = trust
            node = nodes[node_id]
            if node["kind"] == "measure":
                return
            if "options" in node:
                for choice in cast(list[Record], node["options"]):
                    propagate(str(choice["next"]), trust + int(str(choice["trust"])))
            else:
                propagate(str(node["next"]), trust)

        propagate(name(special_entry), 3)
        for row in selected:
            if row.get("condition") != "信任值>=3" or row["id"] == special_entry:
                continue
            lower_bound = minimum_trust.get(name(row["id"]))
            if lower_bound is None or lower_bound < 3:
                raise ValueError(f"Condition is not implied by special entry: {pid}/{row['id']}")
            adaptations.append({"kind": "implied-special-condition", "patientId": pid, "day": day,
                                "row": int(row["row"]), "originalId": row["id"],
                                "condition": row["condition"], "minimumReachableTrust": lower_bound,
                                "entry": name(special_entry)})
    yarn: list[str] = []

    def escape(value: str) -> str:
        return re.sub(r"([\\#<>\[\]{}])", r"\\\1", value.replace("\r", " ").replace("\n", " "))

    for node in nodes.values():
        yarn.extend(["title: " + str(node["id"]), "kind: " + str(node["kind"]), "round: " + str(node["round"]),
                     "patient: " + pid, "day: " + str(day)])
        if node.get("portrait"):
            yarn.append("portrait: " + str(node["portrait"]))
        yarn.append("---")
        if "options" in node:
            for option_node in cast(list[Record], node["options"]):
                tags = f" #id:{option_node['id']} #trust:{option_node['trust']}"
                if option_node["action"]:
                    tags += " #disposition:" + str(option_node["action"])
                yarn.extend(["-> " + escape(str(option_node["text"])) + tags,
                             "    <<jump " + str(option_node["next"]) + ">>"])
        else:
            yarn.extend([str(node["speaker"]) + ": " + escape(str(node["text"])),
                         "<<jump " + str(node["next"]) + ">>"])
        yarn.extend(["===", ""])
    meeting: Record = {"patientId": pid, "startNode": start, "endNode": end, "yarnSource": "\n".join(yarn)}
    if special_entry:
        meeting.update({"specialRoundStartNode": name(special_entry), "specialTrustThreshold": 3})
    if release_thresholds:
        meeting["releaseThreshold"] = next(iter(release_thresholds))
    return meeting

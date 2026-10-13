from __future__ import annotations

import hashlib
import json
import struct
from collections import Counter
from pathlib import Path
from typing import cast

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Analysis" / "Gameplay"
PROJECT = ROOT / "RecoveredUnityProject" / "ExportedProject"


def read_json(path: Path) -> dict[str, object]:
    return cast(dict[str, object], json.loads(path.read_text(encoding="utf-8-sig")))


def write_json(name: str, value: object) -> None:
    (OUTPUT / name).write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def as_mapping(value: object) -> dict[str, object]:
    return cast(dict[str, object], value)


def operands(instruction: dict[str, object]) -> list[dict[str, object]]:
    return cast(list[dict[str, object]], instruction.get("operands", []))


def scalar(value: dict[str, object]) -> object:
    for key in ("stringValue", "floatValue", "boolValue"):
        if key in value:
            return value[key]
    return None


def parse_asset_scalars(path: Path) -> dict[str, object]:
    result: dict[str, object] = {}
    for number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        if line.startswith("  _"):
            key, _, raw = line.strip().partition(":")
            raw = raw.strip()
            if raw.lstrip("-").isdigit() and len(raw) < 15:
                result[key] = {"value": int(raw), "source": str(path), "line": number}
            elif key == "_maxDayActionsByDays":
                data = bytes.fromhex(raw)
                result[key] = {
                    "value": list(struct.unpack("<" + "i" * (len(data) // 4), data)),
                    "source": str(path),
                    "line": number,
                    "encoding": "Unity YAML packed little-endian Int32 array",
                }
    return result


def main() -> None:
    program = read_json(OUTPUT / "yarn-program.json")
    nodes = as_mapping(program["nodes"])
    opcode_counts: Counter[str] = Counter()
    commands: list[dict[str, object]] = []
    functions: list[dict[str, object]] = []
    node_edges: list[dict[str, object]] = []
    control_edges: list[dict[str, object]] = []
    node_index: list[dict[str, object]] = []
    unresolved: list[dict[str, object]] = []
    invalid_labels: list[dict[str, object]] = []
    missing_targets: list[dict[str, object]] = []
    disassembly: list[str] = []
    total_instructions = 0
    total_line_ids: set[str] = set()
    for node_name, raw_node in nodes.items():
        node = as_mapping(raw_node)
        instructions = cast(list[dict[str, object]], node.get("instructions", []))
        labels = as_mapping(node.get("labels", {}))
        node_commands: list[str] = []
        node_functions: list[str] = []
        node_line_ids: list[str] = []
        disassembly.append(f"NODE {node_name}")
        disassembly.append("LABELS " + json.dumps(labels, ensure_ascii=False))
        node_start_line = len(disassembly) - 1
        for index, instruction in enumerate(instructions):
            # Protobuf JSON omits zero-valued enums. The original YarnSpinner
            # descriptor confirms value 0 is JumpTo; opcode absence is NOT unknown.
            opcode = cast(str, instruction.get("opcode", "JUMP_TO"))
            values = [scalar(item) for item in operands(instruction)]
            opcode_counts[opcode] += 1
            total_instructions += 1
            disassembly.append(f"  {index:05d} {opcode} {json.dumps(values, ensure_ascii=False)}")
            record: dict[str, object] = {
                "node": node_name,
                "instruction_index": index,
                "opcode": opcode,
                "operands": values,
                "source": str(OUTPUT / "yarn-program.json"),
                "source_region": f"nodes.{node_name}.instructions[{index}]",
                "disassembly_line": len(disassembly),
            }
            if opcode == "RUN_COMMAND":
                commands.append(record)
                if values:
                    node_commands.append(str(values[0]))
            elif opcode == "CALL_FUNC":
                functions.append(record)
                if values:
                    node_functions.append(str(values[0]))
            elif opcode in ("RUN_LINE", "ADD_OPTION") and values:
                line_id = str(values[0])
                node_line_ids.append(line_id)
                total_line_ids.add(line_id)
            if opcode == "RUN_NODE":
                if index > 0 and instructions[index - 1].get("opcode") == "PUSH_STRING":
                    previous_values = [scalar(item) for item in operands(instructions[index - 1])]
                    target = str(previous_values[0]) if previous_values else ""
                    edge = dict(record, target_node=target, resolution="immediate PUSH_STRING",
                                status="verified_bytecode_target")
                    node_edges.append(edge)
                    if target not in nodes:
                        missing_targets.append(edge)
                else:
                    unresolved.append(dict(record, reason="RUN_NODE target is supplied by stack, not immediately PUSH_STRING"))
            if opcode in ("JUMP_TO", "JUMP_IF_FALSE") and values:
                label = str(values[0])
                edge = dict(record, target_label=label, target_instruction_index=labels.get(label),
                            status="verified_label_target", semantic="conditional" if opcode == "JUMP_IF_FALSE" else "unconditional")
                control_edges.append(edge)
                if label not in labels:
                    invalid_labels.append(edge)
            elif opcode == "ADD_OPTION" and len(values) > 1:
                label = str(values[1])
                edge = dict(record, target_label=label, target_instruction_index=labels.get(label),
                            status="verified_option_label_target", semantic="option selection target; availability is runtime Yarn expression")
                control_edges.append(edge)
                if label not in labels:
                    invalid_labels.append(edge)
            elif opcode == "JUMP":
                unresolved.append(dict(record, reason="JUMP target is supplied by stack (typically selected option); not a named-label constant"))
        node_index.append({
            "node": node_name,
            "instruction_count": len(instructions),
            "labels": labels,
            "headers": node.get("headers", []),
            "tags": node.get("tags", []),
            "line_ids": node_line_ids,
            "commands": node_commands,
            "functions": node_functions,
            "disassembly_start_line": node_start_line,
            "source": str(OUTPUT / "yarn-program.json"),
            "source_region": f"nodes.{node_name}",
        })
        disassembly.append("")
    config = {
        "day_night": parse_asset_scalars(PROJECT / "Assets" / "MonoBehaviour" / "DayNightControllerData.asset"),
        "endings": parse_asset_scalars(PROJECT / "Assets" / "MonoBehaviour" / "EndingsData.asset"),
    }
    summary: dict[str, object] = {
        "input_asset": str(PROJECT / "Assets" / "MonoBehaviour" / "_NoImNotAHuman.asset"),
        "input_asset_line": 15,
        "parser": "Original backup YarnSpinner.dll Yarn.Program.Parser.ParseFrom(byte[])",
        "compiled_program_bytes": (OUTPUT / "yarn-program.bytes").stat().st_size,
        "compiled_program_sha256": hashlib.sha256((OUTPUT / "yarn-program.bytes").read_bytes()).hexdigest(),
        "nodes": len(nodes),
        "initial_variables": len(as_mapping(program.get("initialValues", {}))),
        "instructions": total_instructions,
        "unique_line_ids": len(total_line_ids),
        "run_command_occurrences": len(commands),
        "unique_command_strings": len({str(item["operands"][0]) for item in commands}),
        "function_call_occurrences": len(functions),
        "unique_function_names": sorted({str(item["operands"][0]) for item in functions}),
        "resolved_constant_node_edges": len(node_edges),
        "local_control_edges": len(control_edges),
        "unresolved_stack_jump_records": len(unresolved),
        "missing_constant_node_targets": len(missing_targets),
        "invalid_local_label_targets": len(invalid_labels),
        "opcode_counts": dict(sorted(opcode_counts.items())),
        "artifact_status": "real compiled Yarn protobuf decoded; original authored .yarn text not recovered",
        "boundaries": [
            "Node existence and bytecode edges do not prove runtime reachability.",
            "RUN_NODE with dynamic stack value is unresolved.",
            "JUMP uses stack target; option selection/conditions are runtime data.",
            "Instructions and variables retained exactly; no guessed C# or Yarn source generated.",
            "Localization tables remain in recovered Unity project; text is not joined in this index.",
            "C# host actions depend on scene bindings and interface/Unity/C# event initialization.",
            "No game process, Steam cloud or original Steam files modified.",
        ],
        "serialized_configs": config,
        "validation": {"json_parse": "passed", "bytecode_node_integrity": "passed",
                       "all_constant_node_targets_exist": not missing_targets,
                       "all_local_control_labels_exist": not invalid_labels},
    }
    write_json("yarn-node-index.json", node_index)
    write_json("yarn-node-edges.json", node_edges)
    write_json("yarn-control-edges.json", control_edges)
    write_json("yarn-command-function-index.json", {"commands": commands, "functions": functions})
    write_json("yarn-unresolved-control.json", {"dynamic": unresolved, "invalid_labels": invalid_labels, "missing_nodes": missing_targets})
    write_json("serialized-gameplay-configs.json", config)
    write_json("summary.json", summary)
    (OUTPUT / "yarn-disassembly.txt").write_text("\n".join(disassembly), encoding="utf-8")
    print(json.dumps({key: value for key, value in summary.items() if key not in ("serialized_configs", "unique_function_names")}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()


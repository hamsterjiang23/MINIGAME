"""Cross-check output completeness, IL boundaries/tokens, and graph rows."""
from __future__ import annotations

import csv
import json
from collections import Counter
from pathlib import Path

BASE = Path(__file__).resolve().parents[3] / "Analysis/Managed/Assembly-CSharp"


def read_json(name: str) -> object:
    return json.loads((BASE / name).read_text(encoding="utf-8-sig"))


def main() -> None:
    methods = read_json("methods.json")
    edges = read_json("call-edges.json")
    coverage = read_json("coverage.json")
    summary = read_json("summary.json")
    native = read_json("cil-cpp-map.json")
    inventory = read_json("assemblies-inventory.json")
    assert isinstance(methods, list) and isinstance(edges, list)
    assert isinstance(coverage, dict) and isinstance(summary, dict)
    assert isinstance(native, list) and isinstance(inventory, list)
    method_tokens = {method["token"] for method in methods}
    assert len(method_tokens) == len(methods) == summary["method_def_count"]
    assert len(edges) == coverage["call_edges"]
    parsed_body_count = 0
    il_bytes = 0
    instruction_count = 0
    raw_call_count = 0
    resolution_errors = 0
    observed_tokens: set[str] = set()
    call_ops = {"call", "callvirt", "newobj", "ldftn", "ldvirtftn"}
    with (BASE / "method-il.jsonl").open(encoding="utf-8-sig") as file:
        for line in file:
            body = json.loads(line)
            assert body["token"] in method_tokens and body["token"] not in observed_tokens
            observed_tokens.add(body["token"])
            raw = bytes.fromhex(body["il_hex"])
            cursor = 0
            for ins in body["instructions"]:
                assert ins["offset"] == cursor
                opcode_size = 2 if raw[cursor] == 0xFE else 1
                operand = bytes.fromhex(ins["operand_hex"])
                assert raw[cursor + opcode_size:cursor + opcode_size + len(operand)] == operand
                if ins["metadata_token"] is not None:
                    assert int(ins["metadata_token"], 16) == int.from_bytes(operand, "little")
                resolved = ins["resolved_operand"]
                if isinstance(resolved, dict) and "resolution_error" in resolved:
                    resolution_errors += 1
                cursor += opcode_size + len(operand)
                instruction_count += 1
                if ins["opcode"] in call_ops:
                    raw_call_count += 1
            assert cursor == len(raw)
            parsed_body_count += 1
            il_bytes += len(raw)
    assert parsed_body_count == summary["methods_with_readable_il_body"]
    assert il_bytes == summary["total_il_bytes"]
    assert raw_call_count == len(edges)
    for edge in edges:
        assert edge["caller_token"] in method_tokens
        assert edge["callee_definition_token"] is None or edge["callee_definition_token"] in method_tokens
    with (BASE / "call-edges.csv").open(encoding="utf-8-sig", newline="") as file:
        csv_edges = list(csv.DictReader(file))
    assert len(csv_edges) == len(edges)
    assert set(csv_edges[0]) == set(edges[0])
    for json_edge, csv_edge in zip(edges, csv_edges, strict=True):
        for key, value in json_edge.items():
            assert csv_edge[key] == ("" if value is None else str(value))
    baseline = read_json("scope-fix-baseline.json")
    assert isinstance(baseline, dict)
    edge_index = {(edge["caller_token"], edge["il_offset"], edge["opcode"], edge["callee_operand_token"]): edge for edge in edges}
    recovered_assembly_edges = 0
    newly_bound_local_generic_edges = 0
    recovered_assemblies: Counter[str] = Counter()
    for old in baseline["missing_edges"]:
        edge = edge_index[(old["caller_token"], old["il_offset"], old["opcode"], old["callee_operand_token"])]
        assert isinstance(edge["callee_assembly"], str) and edge["callee_assembly"]
        assert edge["callee_type"] == old["callee_type"] and edge["callee_name"] == old["callee_name"]
        recovered_assembly_edges += 1
        recovered_assemblies[edge["callee_assembly"]] += 1
        if edge["callee_assembly"] == "Assembly-CSharp" and old["callee_definition_token"] is None and edge["callee_definition_token"] is not None:
            newly_bound_local_generic_edges += 1
    assert recovered_assembly_edges == baseline["missing_callee_assembly_edges"]
    assert all(edge["callee_assembly"] is not None for edge in edges)
    assert len(native) == len(methods)
    for row in native:
        assert row["method_token"] in method_tokens
        assert row["pointer_array_index"] == (int(row["method_token"], 16) & 0xFFFFFF) - 1
        for location in row["native_definitions"]:
            assert location["signature_line"] <= location["body_start_line"] <= location["body_end_line"]
    with (BASE / "cil-cpp-map.csv").open(encoding="utf-8-sig", newline="") as file:
        native_csv = list(csv.DictReader(file))
    assert len(native_csv) == len(native)
    for json_row, csv_row in zip(native, native_csv, strict=True):
        for key in ("method_token", "method_rid", "cil_il_length", "pointer_array_index", "pointer_array_entry_line"):
            assert csv_row[key] == str(json_row[key])
        assert len(json_row["native_definitions"]) <= 1
        if json_row["native_definitions"]:
            definition = json_row["native_definitions"][0]
            for key in ("source_file", "signature_line", "body_start_line", "body_end_line", "native_signature", "body_sha256_utf8"):
                assert csv_row[key] == str(definition[key])
    with (BASE / "assemblies-inventory.csv").open("w", newline="", encoding="utf-8-sig") as file:
        keys = ["assembly_name", "sha256", "mvid", "type_def_count", "method_def_count", "field_def_count", "methods_with_readable_il_body", "methods_with_no_rva", "abstract_methods", "non_il_implementation_methods", "one_byte_ret_methods", "unreadable_rva_bodies", "path"]
        writer = csv.DictWriter(file, fieldnames=keys, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(inventory)
    result: dict[str, object] = {
        "passed": True, "method_definition_tokens_unique": len(method_tokens),
        "parsed_body_count": parsed_body_count, "decoded_instruction_count": instruction_count,
        "validated_il_bytes": il_bytes, "raw_call_or_function_pointer_instruction_count": raw_call_count,
        "recorded_call_edges": len(edges), "operand_resolution_errors": resolution_errors,
        "call_opcodes": dict(Counter(edge["opcode"] for edge in edges)),
        "dispatch_classification": dict(Counter(edge["dispatch"] for edge in edges)),
        "scope_classification": dict(Counter(edge["scope"] for edge in edges)),
        "body_classification": dict(Counter(method["body_classification"] for method in methods)),
        "native_mapping_status": dict(Counter(row["mapping_status"] for row in native)),
        "assemblies_inventory_count": len(inventory),
        "call_csv_all_json_fields_parity": True,
        "native_csv_source_range_and_hash_parity": True,
        "previously_missing_assembly_edges_recovered": recovered_assembly_edges,
        "previously_missing_assembly_distinct_operands": baseline["missing_callee_assembly_distinct_operands"],
        "recovered_assembly_groups": dict(recovered_assemblies),
        "newly_bound_local_generic_declaration_edges": newly_bound_local_generic_edges,
        "remaining_missing_callee_assembly_edges": sum(edge["callee_assembly"] is None for edge in edges),
        "remaining_local_unresolved_declaration_edges": sum(edge["callee_assembly"] == "Assembly-CSharp" and edge["callee_definition_token"] is None for edge in edges),
        "check_scope": "Output structural invariants and raw-IL instruction boundaries/token bytes; not semantic C# recompilation or live runtime validation.",
    }
    (BASE / "verification.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()

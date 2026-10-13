"""Correlate MethodDef RIDs with observed generated C++ registration entries.

Reads copied build artifacts only. It does not load or run the target assembly.
"""
from __future__ import annotations

import csv
import hashlib
import json
import re
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parents[3]
MANAGED = BASE / "Analysis/Managed/Assembly-CSharp"
CPP = BASE / "Analysis/Inputs/GameCopy/NoImNotAHuman_BackUpThisFolder_ButDontShipItWithYourGame/il2cppOutput"


def json_rows(path: Path) -> list[dict[str, object]]:
    value = json.loads(path.read_text(encoding="utf-8-sig"))
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"Expected row list: {path}")
    return value


def text_field(row: dict[str, object], key: str) -> str:
    value = row[key]
    if not isinstance(value, str):
        raise ValueError(f"Expected string {key}: {value!r}")
    return value


def save_json(path: Path, value: object) -> None:
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def file_hash(path: Path) -> str:
    with path.open("rb") as file:
        return hashlib.file_digest(file, "sha256").hexdigest()


def closing_brace(text: str, opening: int) -> int:
    """Find a C++ function boundary without counting comment/string braces."""
    depth = 0
    index = opening
    state = "code"
    while index < len(text):
        ch = text[index]
        next_ch = text[index + 1] if index + 1 < len(text) else ""
        if state == "line_comment":
            if ch == "\n":
                state = "code"
        elif state == "block_comment":
            if ch == "*" and next_ch == "/":
                state = "code"
                index += 1
        elif state in {"string", "char"}:
            if ch == "\\":
                index += 1
            elif ch == ('"' if state == "string" else "'"):
                state = "code"
        elif ch == "/" and next_ch == "/":
            state = "line_comment"
            index += 1
        elif ch == "/" and next_ch == "*":
            state = "block_comment"
            index += 1
        elif ch == '"':
            state = "string"
        elif ch == "'":
            state = "char"
        elif ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0:
                return index
        index += 1
    raise ValueError(f"Unclosed function at char {opening}")


def cpp_definitions() -> tuple[dict[str, list[dict[str, object]]], list[dict[str, object]]]:
    definitions: dict[str, list[dict[str, object]]] = {}
    inventory: list[dict[str, object]] = []
    pattern = re.compile(r"^IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR (?P<signature>[^\n;]*?\b(?P<symbol>[A-Za-z_][A-Za-z_0-9]*_m[0-9A-F]{40})\s*\([^\n;]*\))\s*\n(?P<opening>\{)", re.MULTILINE)
    for path in sorted(CPP.glob("Assembly-CSharp*.cpp")):
        if path.name.startswith("Assembly-CSharp-firstpass"):
            continue
        text = path.read_text(encoding="utf-8-sig")
        count = 0
        for match in pattern.finditer(text):
            symbol = match.group("symbol")
            opening = match.start("opening")
            end = closing_brace(text, opening)
            start_line = text.count("\n", 0, match.start()) + 1
            end_line = text.count("\n", 0, end) + 1
            body = text[opening:end + 1]
            location: dict[str, object] = {
                "source_file": str(path), "signature_line": start_line,
                "body_start_line": start_line + 1, "body_end_line": end_line,
                "native_signature": match.group("signature"),
                "body_sha256_utf8": hashlib.sha256(body.encode("utf-8")).hexdigest(),
            }
            definitions.setdefault(symbol, []).append(location)
            count += 1
        inventory.append({"file": str(path), "sha256": file_hash(path), "bytes": path.stat().st_size, "matched_method_definitions": count})
    return definitions, inventory


def dot_quote(value: str) -> str:
    return json.dumps(value, ensure_ascii=False)


def write_method_graph() -> None:
    methods = json_rows(MANAGED / "methods.json")
    edges = json_rows(MANAGED / "call-edges.json")
    with (MANAGED / "method-call-graph.dot").open("w", encoding="utf-8") as file:
        file.write("digraph MethodCalls {\n  rankdir=LR;\n")
        for method in methods:
            token = text_field(method, "token")
            label = text_field(method, "full_signature")
            file.write(f"  {dot_quote(token)} [label={dot_quote(token + ' ' + label)}];\n")
        external_nodes: set[str] = set()
        for edge in edges:
            caller = text_field(edge, "caller_token")
            definition = edge["callee_definition_token"]
            if isinstance(definition, str):
                callee = definition
            else:
                callee = "operand:" + text_field(edge, "callee_operand_token")
                if callee not in external_nodes:
                    label = text_field(edge, "callee_signature")
                    file.write(f"  {dot_quote(callee)} [shape=box,label={dot_quote(callee + ' ' + label)}];\n")
                    external_nodes.add(callee)
            label = text_field(edge, "opcode") + "@IL_" + f"{int(str(edge['il_offset'])):04X}"
            file.write(f"  {dot_quote(caller)} -> {dot_quote(callee)} [label={dot_quote(label)}];\n")
        file.write("}\n")


def main() -> None:
    methods = json_rows(MANAGED / "methods.json")
    codegen = CPP / "Assembly-CSharp_CodeGen.c"
    source = codegen.read_text(encoding="utf-8-sig")
    match = re.search(r"static Il2CppMethodPointer s_methodPointers\[(\d+)\]\s*=\s*\{(?P<entries>.*?)\n\};", source, re.DOTALL)
    if match is None:
        raise ValueError("s_methodPointers table not found")
    entries = [part.strip() for part in match.group("entries").split(",") if part.strip()]
    expected = int(match.group(1))
    if len(entries) != expected or len(methods) != expected:
        raise ValueError(f"Count mismatch: entries={len(entries)}, advertised={expected}, methods={len(methods)}")
    table_start_line = source.count("\n", 0, match.start("entries")) + 1
    native, cpp_inventory = cpp_definitions()
    rows: list[dict[str, object]] = []
    null_count = 0
    matched_count = 0
    candidate_count = 0
    for method in methods:
        token = text_field(method, "token")
        rid = int(token, 16) & 0xFFFFFF
        raw = entries[rid - 1]
        pointer = None if raw == "NULL" else raw
        definitions = native.get(raw, [])
        if pointer is None:
            null_count += 1
        if definitions:
            matched_count += 1
        if pointer is not None and not definitions:
            candidate_count += 1
        rows.append({
            "method_token": token, "method_rid": rid, "cil_signature": method["full_signature"],
            "cil_body_classification": method["body_classification"], "cil_il_length": method["il_length"],
            "pointer_array_index": rid - 1, "pointer_array_source_file": str(codegen),
            "pointer_array_entry_line": table_start_line + rid,
            "native_symbol": pointer, "native_definitions": definitions,
            "mapping_status": "positional_table_and_symbol_definition" if definitions else "null_pointer_slot" if pointer is None else "pointer_symbol_without_definition_in_scanned_cpp",
            "mapping_basis": "Observed s_methodPointers[5855] registered as Assembly-CSharp.dll; MethodDef RID-1 positional mapping. This does not recover an RVA or VA in GameAssembly.dll.",
        })
    save_json(MANAGED / "cil-cpp-map.json", rows)
    save_json(MANAGED / "cpp-source-inventory.json", cpp_inventory)
    with (MANAGED / "cil-cpp-map.csv").open("w", newline="", encoding="utf-8-sig") as file:
        keys = ["method_token", "method_rid", "cil_signature", "cil_body_classification", "cil_il_length", "pointer_array_index", "pointer_array_source_file", "pointer_array_entry_line", "native_symbol", "mapping_status", "source_file", "signature_line", "body_start_line", "body_end_line", "native_signature", "body_sha256_utf8"]
        writer = csv.DictWriter(file, fieldnames=keys)
        writer.writeheader()
        for row in rows:
            exported = {key: row.get(key) for key in keys}
            definitions = row["native_definitions"]
            if isinstance(definitions, list) and definitions:
                exported.update({key: definitions[0].get(key) for key in keys if key in definitions[0]})
            writer.writerow(exported)
    result = {
        "method_count": expected, "pointer_table_entries": len(entries), "null_pointer_slots": null_count,
        "slots_with_cpp_definition": matched_count, "nonnull_slots_without_scanned_cpp_definition": candidate_count,
        "cpp_definition_symbols": len(native), "scanned_cpp_files": len(cpp_inventory),
        "codegen_source": str(codegen), "codegen_sha256": file_hash(codegen),
        "limits": ["Index mapping uses observed matching registration count and MethodDef order; no shipped GameAssembly symbol RVA or runtime observation.", "NULL includes abstract/generic method slots; it is not proof of absence of an implementation.", "Generic instantiations/sharing and adjustor thunks are not independently reconstructed by this mapping.", "C++ text body hashes are source spans, not machine-code hashes."],
    }
    save_json(MANAGED / "cil-cpp-map-summary.json", result)
    write_method_graph()
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    if "--graph-only" in sys.argv:
        write_method_graph()
    else:
        main()

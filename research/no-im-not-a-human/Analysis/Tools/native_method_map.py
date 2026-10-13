"""Read shipped PE bytes and correlate an observed IL2CPP module pointer table."""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import struct
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Section:
    name: str
    rva: int
    raw_offset: int
    raw_size: int
    flags: int


def read_pe(data: bytes) -> tuple[int, list[Section]]:
    if data[:2] != b"MZ":
        raise ValueError("Missing DOS signature")
    pe_offset = struct.unpack_from("<I", data, 0x3C)[0]
    if data[pe_offset:pe_offset + 4] != b"PE\0\0":
        raise ValueError("Missing PE signature")
    machine, count = struct.unpack_from("<HH", data, pe_offset + 4)
    optional_size = struct.unpack_from("<H", data, pe_offset + 20)[0]
    optional_offset = pe_offset + 24
    if machine != 0x8664 or struct.unpack_from("<H", data, optional_offset)[0] != 0x20B:
        raise ValueError("Expected admitted x64 PE32+ input")
    image_base = struct.unpack_from("<Q", data, optional_offset + 24)[0]
    sections: list[Section] = []
    for index in range(count):
        position = optional_offset + optional_size + index * 40
        name = data[position:position + 8].rstrip(b"\0").decode("ascii", errors="replace")
        rva, raw_size, raw_offset = struct.unpack_from("<III", data, position + 12)
        flags = struct.unpack_from("<I", data, position + 36)[0]
        if raw_offset + raw_size > len(data):
            raise ValueError(f"Section outside file: {name}")
        sections.append(Section(name, rva, raw_offset, raw_size, flags))
    return image_base, sections


def offset_to_rva(offset: int, sections: list[Section]) -> int | None:
    for section in sections:
        if section.raw_offset <= offset < section.raw_offset + section.raw_size:
            return section.rva + offset - section.raw_offset
    return None


def rva_to_offset(rva: int, size: int, sections: list[Section]) -> int | None:
    for section in sections:
        if section.rva <= rva and rva + size <= section.rva + section.raw_size:
            return section.raw_offset + rva - section.rva
    return None


def occurrences(data: bytes, needle: bytes) -> list[int]:
    positions: list[int] = []
    start = 0
    while (position := data.find(needle, start)) != -1:
        positions.append(position)
        start = position + 1
    return positions


def native_section(rva: int, sections: list[Section]) -> Section | None:
    for section in sections:
        if section.rva <= rva < section.rva + section.raw_size:
            return section
    return None


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--binary", type=Path, required=True)
    parser.add_argument("--cpp-map", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    data = args.binary.read_bytes()
    image_base, sections = read_pe(data)
    with args.cpp_map.open(encoding="utf-8-sig", newline="") as stream:
        cpp_rows = list(csv.DictReader(stream))
    expected_count = len(cpp_rows)
    if expected_count != 5855:
        raise ValueError(f"Expected complete 5855-row CIL/C++ index, got {expected_count}")
    candidates: list[dict[str, int]] = []
    for string_offset in occurrences(data, b"Assembly-CSharp.dll\0"):
        string_rva = offset_to_rva(string_offset, sections)
        if string_rva is None:
            continue
        name_va = image_base + string_rva
        prefix = struct.pack("<QQ", name_va, expected_count)
        for module_offset in occurrences(data, prefix):
            if module_offset + 40 > len(data):
                continue
            pointer_va, adjustor_count = struct.unpack_from("<QQ", data, module_offset + 16)
            if adjustor_count != 475:
                continue
            array_offset = rva_to_offset(pointer_va - image_base, expected_count * 8, sections)
            module_rva = offset_to_rva(module_offset, sections)
            if array_offset is None or module_rva is None:
                continue
            candidates.append({"module_offset": module_offset, "module_rva": module_rva,
                "module_name_offset": string_offset, "module_name_va": name_va,
                "pointer_array_va": pointer_va, "pointer_array_offset": array_offset})
    if len(candidates) != 1:
        raise ValueError(f"Require exactly one observed module/count/adjustor match, got {len(candidates)}")
    candidate = candidates[0]
    values = struct.unpack_from(f"<{expected_count}Q", data, candidate["pointer_array_offset"])
    native_rows: list[dict[str, str | int | None]] = []
    null_count = 0
    for index, (cpp_row, value) in enumerate(zip(cpp_rows, values, strict=True)):
        if int(cpp_row["pointer_array_index"]) != index or int(cpp_row["method_rid"]) != index + 1:
            raise ValueError(f"Index order mismatch at slot {index}")
        expected_null = not cpp_row["native_symbol"] or cpp_row["native_symbol"] == "NULL"
        if expected_null != (value == 0):
            raise ValueError(f"Native/generated NULL-mask mismatch at slot {index}")
        rva = value - image_base if value else None
        code_offset = rva_to_offset(rva, 16, sections) if rva is not None else None
        section = native_section(rva, sections) if rva is not None else None
        if value and (code_offset is None or section is None or not section.flags & 0x20000000):
            raise ValueError(f"Nonzero method pointer outside executable file-backed section at {index}")
        null_count += value == 0
        native_rows.append({"method_token": cpp_row["method_token"], "method_rid": index + 1,
            "cil_signature": cpp_row["cil_signature"], "native_symbol": cpp_row["native_symbol"],
            "pointer_slot_offset": candidate["pointer_array_offset"] + index * 8,
            "preferred_image_va": f"0x{value:X}" if value else None,
            "rva": f"0x{rva:X}" if rva is not None else None,
            "file_offset": code_offset, "section": section.name if section is not None else None,
            "first_16_bytes": data[code_offset:code_offset + 16].hex() if code_offset is not None else None,
            "cpp_source_file": cpp_row["source_file"], "cpp_signature_line": cpp_row["signature_line"],
            "status": "verified_static_pointer_slot" if value else "null_slot_unresolved_or_abstract"})
    if null_count != 856:
        raise ValueError(f"Unexpected NULL count {null_count}")
    args.output.mkdir(parents=True, exist_ok=True)
    (args.output / "native-method-map.json").write_text(json.dumps(native_rows, ensure_ascii=False, indent=2), encoding="utf-8")
    with (args.output / "native-method-map.csv").open("w", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=list(native_rows[0]))
        writer.writeheader()
        writer.writerows(native_rows)
    summary = {"binary_path": str(args.binary), "binary_sha256": hashlib.sha256(data).hexdigest(),
        "image_base": f"0x{image_base:X}", "module": "Assembly-CSharp.dll", "module_prefix": candidate,
        "slots": expected_count, "nonzero_static_addresses": expected_count - null_count,
        "null_slots": null_count, "unique_nonzero_addresses": len({value for value in values if value}),
        "null_mask_matches_generated_cpp": True, "all_nonzero_targets_file_backed_executable": True,
        "target_execution": False,
        "structure_reference": "https://raw.githubusercontent.com/SamboyCoding/Cpp2IL/development/LibCpp2IL/BinaryStructures/Il2CppCodeGenModule.cs",
        "limitations": ["Addresses use the PE preferred image base; live ASLR load base is unknown.",
            "Pointer-table alignment is static structural evidence, not machine-code/CIL behavioral equivalence.",
            "No Ghidra disassembly, function boundaries, inlining or runtime execution was established.",
            "NULL slots may be abstract or generic/shared; generic instantiation tables are not mapped here."]}
    (args.output / "native-map-verification.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False))


if __name__ == "__main__":
    main()

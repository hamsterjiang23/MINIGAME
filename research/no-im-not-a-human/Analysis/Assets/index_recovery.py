"""Index exported Unity data and merge only matching recovered C# identities."""

from __future__ import annotations

import csv
import hashlib
import json
import re
import shutil
from collections import Counter, defaultdict
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / "RecoveredUnityProject/ExportedProject/Assets"
OUTPUT = ROOT / "Analysis/Assets"
SCRIPT_SOURCE = ROOT / "DecompiledCode/UnityCompatible"
GUID_RE = re.compile(r"\bguid:\s*([0-9a-fA-F]{32})")
YAML_DOCUMENT_RE = re.compile(r"^--- !u!(\d+) &(-?\d+)", re.MULTILINE)
SCRIPT_RE = re.compile(r"m_Script: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: \d+\}")
NAMESPACE_RE = re.compile(r"\bnamespace\s+([A-Za-z_][\w.]*)\s*[;{]")


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_json(name: str, value: object) -> None:
    (OUTPUT / name).write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def script_identity(text: str, type_name: str) -> tuple[str, ...] | None:
    namespaces = tuple(NAMESPACE_RE.findall(text))
    declaration = re.search(r"\b(?:class|struct|interface|enum|record)\s+" + re.escape(type_name) + r"\b", text)
    if not declaration:
        return None
    return namespaces


def merge_scripts() -> dict[str, object]:
    backups = OUTPUT / "OriginalDummyScripts"
    merged: list[dict[str, object]] = []
    unmatched: list[dict[str, str]] = []
    targets = (
        ("Assembly-CSharp", ASSETS / "Scripts/Assembly-CSharp"),
        ("Assembly-CSharp-firstpass", ASSETS / "Plugins/Assembly-CSharp-firstpass"),
    )
    for assembly, destination in targets:
      for target in sorted(destination.rglob("*.cs")):
        relative = target.relative_to(destination)
        source = SCRIPT_SOURCE / assembly / relative
        if not source.is_file():
            unmatched.append({"assembly": assembly, "path": str(relative), "reason": "No matching recovered-source path"})
            continue
        target_text = target.read_text(encoding="utf-8-sig")
        source_text = source.read_text(encoding="utf-8-sig")
        target_identity = script_identity(target_text, target.stem)
        source_identity = script_identity(source_text, source.stem)
        if target_identity is None or target_identity != source_identity:
            unmatched.append({"assembly": assembly, "path": str(relative), "reason": "Namespace/type declaration did not match"})
            continue
        meta = Path(str(target) + ".meta")
        meta_before = sha256(meta) if meta.exists() else None
        backup = backups / assembly / relative
        backup.parent.mkdir(parents=True, exist_ok=True)
        if not backup.exists():
            shutil.copyfile(target, backup)
        old_hash = sha256(target)
        shutil.copyfile(source, target)
        meta_after = sha256(meta) if meta.exists() else None
        if meta_before != meta_after:
            raise RuntimeError(f"Unexpected .meta mutation: {meta}")
        merged.append({
            "assembly": assembly, "relativePath": str(relative), "typeName": source.stem,
            "namespaces": source_identity, "source": str(source),
            "previousSha256": old_hash, "sourceSha256": sha256(source),
            "destinationSha256": sha256(target), "metaSha256Before": meta_before,
            "metaSha256After": meta_after, "metaPreserved": meta_before == meta_after,
        })
    result: dict[str, object] = {"languageVersion": "CSharp9_0", "mergedCount": len(merged), "mergedByAssembly": dict(Counter(str(record["assembly"]) for record in merged)), "unmatchedCount": len(unmatched), "merged": merged, "unmatched": unmatched}
    write_json("script-merge-csharp9.json", result)
    return result


def inventory() -> dict[str, object]:
    files = sorted(p for p in ASSETS.rglob("*") if p.is_file())
    guid_to_paths: dict[str, list[str]] = defaultdict(list)
    for meta in files:
        if meta.suffix == ".meta":
            match = GUID_RE.search(meta.read_text(encoding="utf-8-sig", errors="replace"))
            if match:
                guid_to_paths[match.group(1).lower()].append(str(meta.with_suffix("").relative_to(ASSETS)))
    yaml_classes: Counter[str] = Counter()
    folder_counts: Counter[str] = Counter()
    extensions: Counter[str] = Counter()
    unresolved: Counter[str] = Counter()
    script_references: list[dict[str, object]] = []
    edges: list[dict[str, object]] = []
    records: list[dict[str, object]] = []
    for path in files:
        if path.suffix == ".meta":
            continue
        relative = str(path.relative_to(ASSETS))
        extensions[path.suffix.lower() or "(none)"] += 1
        folder_counts[path.relative_to(ASSETS).parts[0]] += 1
        record: dict[str, object] = {"relativePath": relative, "bytes": path.stat().st_size, "extension": path.suffix.lower()}
        if path.suffix.lower() in {".asset", ".prefab", ".unity", ".mat", ".controller", ".anim", ".playable", ".rendertexture", ".sprite", ".cubemap", ".vfx"}:
            text = path.read_text(encoding="utf-8-sig", errors="replace")
            documents = YAML_DOCUMENT_RE.findall(text)
            if documents:
                classes = Counter(class_id for class_id, _ in documents)
                yaml_classes.update(classes)
                record["yamlClassCounts"] = dict(classes)
                record["yamlDocumentCount"] = len(documents)
                references = Counter(g.lower() for g in GUID_RE.findall(text))
                record["externalGuidCount"] = len(references)
                for guid, count in references.items():
                    targets = guid_to_paths.get(guid, [])
                    builtin = guid.startswith("0000000000000000")
                    if not targets and not builtin:
                        unresolved[guid] += count
                    edges.append({"source": relative, "guid": guid, "targets": targets, "occurrences": count, "builtinCandidate": builtin, "resolvedToExportedFile": bool(targets)})
                for file_id, guid in SCRIPT_RE.findall(text):
                    script_references.append({"asset": relative, "fileID": int(file_id), "guid": guid, "targets": guid_to_paths.get(guid, [])})
        records.append(record)
    with (OUTPUT / "exported-assets.jsonl").open("w", encoding="utf-8") as stream:
        for record in records:
            stream.write(json.dumps(record, ensure_ascii=False) + "\n")
    with (OUTPUT / "asset-references.jsonl").open("w", encoding="utf-8") as stream:
        for edge in edges:
            stream.write(json.dumps(edge, ensure_ascii=False) + "\n")
    write_json("script-references.json", script_references)
    write_json("guid-index.json", guid_to_paths)
    write_json("unresolved-references.json", {guid: {"occurrences": count, "sources": sorted({str(edge["source"]) for edge in edges if edge["guid"] == guid})} for guid, count in unresolved.items()})
    summaries: dict[str, object] = {
        "projectRoot": str(ASSETS.parent), "assetFileCountExcludingMeta": len(records),
        "metaFileCount": sum(p.suffix == ".meta" for p in files),
        "assetBytesExcludingMeta": sum(int(r["bytes"]) for r in records),
        "folderCounts": dict(folder_counts), "extensionCounts": dict(extensions),
        "yamlClassCounts": dict(sorted(yaml_classes.items(), key=lambda pair: int(pair[0]))),
        "externalReferenceEdges": len(edges), "uniqueExportedGuids": len(guid_to_paths),
        "unresolvedNonBuiltinGuids": len(unresolved),
        "scriptReferenceCount": len(script_references),
        "unresolvedScriptReferenceCount": sum(not r["targets"] for r in script_references),
        "duplicateGuids": {g: paths for g, paths in guid_to_paths.items() if len(paths) > 1},
        "scenes": [str(p.relative_to(ASSETS)) for p in files if p.suffix == ".unity"],
        "limits": ["Counts describe exported assets, not original-author project counts.", "GUID resolution checks target-file existence only; local fileID and component compatibility require an Editor import.", "Built-in Unity GUIDs are heuristically recognized by their zero prefix.", "Missing serialized MonoBehaviour fields remain missing even if their script GUID resolves."],
    }
    write_json("recovery-summary.json", summaries)
    with (OUTPUT / "yaml-class-counts.csv").open("w", newline="", encoding="utf-8") as stream:
        writer = csv.writer(stream)
        writer.writerow(["classID", "exportedDocumentCount"])
        writer.writerows(sorted(yaml_classes.items(), key=lambda pair: int(pair[0])))
    return summaries


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    merge = merge_scripts()
    summary = inventory()
    print(json.dumps({"mergedScripts": merge["mergedCount"], "unmatchedScripts": merge["unmatchedCount"], "summary": summary}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()

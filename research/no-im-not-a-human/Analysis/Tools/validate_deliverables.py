from __future__ import annotations

import json
import re
from pathlib import Path


def main() -> None:
    root = Path(__file__).resolve().parents[2]
    missing: list[dict[str, str]] = []
    checked = 0
    reports = [root / "README.md", *sorted((root / "Reports").glob("*.md"))]
    for report in reports:
        content = report.read_text(encoding="utf-8-sig")
        for match in re.finditer(r"\]\((<[^>]+>|[^)]+)\)", content):
            destination = match.group(1).strip("<>")
            if destination.startswith(("http:", "https:", "app:", "codex:", "#")):
                continue
            destination = re.sub(r":\d+$", "", destination)
            destination = destination.split("#", 1)[0]
            path = Path(destination)
            if not path.is_absolute():
                path = report.parent / path
            checked += 1
            if not path.exists():
                missing.append({"report": str(report), "link": destination, "resolved": str(path)})
    managed = json.loads((root / "Analysis/Managed/Assembly-CSharp/verification.json").read_text(encoding="utf-8-sig"))
    native = json.loads((root / "Analysis/Native/native-map-verification.json").read_text(encoding="utf-8-sig"))
    syntax = json.loads((root / "Analysis/syntax-csharp9-verification.json").read_text(encoding="utf-8-sig"))
    source = json.loads((root / "Analysis/Environment/source-immutability-verification.json").read_text(encoding="utf-8-sig"))
    assets = json.loads((root / "Analysis/Assets/recovery-summary.json").read_text(encoding="utf-8-sig"))
    yarn = json.loads((root / "Analysis/Gameplay/summary.json").read_text(encoding="utf-8-sig"))
    scripts = json.loads((root / "Analysis/Assets/script-merge-csharp9.json").read_text(encoding="utf-8-sig"))
    plugins = json.loads((root / "Analysis/Assets/plugin-merge.json").read_text(encoding="utf-8-sig"))
    script_rows = scripts["merged"]
    plugin_rows = [row for row in plugins if row["status"] == "merged"]
    merged_hashes_match = all(row["sourceSha256"] == row["destinationSha256"] and row["metaSha256Before"] == row["metaSha256After"] for row in script_rows)
    plugin_hashes_match = all(row["sourceHash"] == row["destinationHash"] and row["metaSha256Before"] == row["metaSha256After"] for row in plugin_rows)
    result = {
        "reports_checked": len(reports), "local_links_checked": checked, "missing_links": missing,
        "managed_index_checks_passed": managed["passed"],
        "managed_call_edges": managed["recorded_call_edges"],
        "native_slots": native["slots"], "native_nonzero_pointer_entries": native["nonzero_static_addresses"],
        "native_null_mask_matches_cpp": native["null_mask_matches_generated_cpp"],
        "csharp_sources_parsed": syntax["inspected_source_files"],
        "parser_diagnostics_errors": syntax["syntax_error_count"],
        "comprehensive_language_feature_validation": syntax["performed_comprehensive_language_feature_compatibility_validation"],
        "unity_import_verified": syntax["performed_unity_import"],
        "source_files_checked": source["FilesChecked"], "source_files_changed": source["ChangedFiles"],
        "exported_scenes": assets["extensionCounts"][".unity"],
        "exported_prefabs": assets["extensionCounts"][".prefab"],
        "merged_csharp9_implementations": len(script_rows),
        "script_source_and_meta_hashes_match": merged_hashes_match,
        "merged_plugins": len(plugin_rows),
        "plugin_source_and_meta_hashes_match": plugin_hashes_match,
        "yarn_nodes": yarn["nodes"], "yarn_instructions": yarn["instructions"],
        "yarn_missing_constant_targets": yarn["missing_constant_node_targets"],
        "yarn_unresolved_stack_jumps": yarn["unresolved_stack_jump_records"],
        "limits": ["This validator checks artifact existence and previously recorded invariants; it does not execute a game or prove Unity import/playability."]
    }
    result["passed_with_recorded_limits"] = not missing and managed["passed"] and source["ChangedFiles"] == 0 and syntax["syntax_error_count"] == 0 and merged_hashes_match and plugin_hashes_match
    target = root / "Analysis/deliverable-verification.json"
    target.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    if not result["passed_with_recorded_limits"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()

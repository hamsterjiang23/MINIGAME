"""Apply reviewed prose-only edits with pinned sources and exact old-text guards."""
from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path

MANIFEST = Path(__file__).with_name("approved-text-edits.json")
TEXT_FIELDS = {"open", "option", "response", "actionResponse"}


def load_manifest(path: Path = MANIFEST) -> dict:
    manifest = json.loads(path.read_text(encoding="utf-8"))
    if manifest.get("version") != 1:
        raise ValueError("Unsupported approved text-edit manifest")
    preserved = set(manifest["preservedIssueIds"])
    for book, spec in manifest["books"].items():
        if not re.fullmatch(r"[a-f0-9]{64}", spec["sourceSha256"]):
            raise ValueError(f"Invalid text-edit source hash: {book}")
        seen = set()
        for edit in spec["edits"]:
            key = (edit["id"], edit["field"])
            if (key in seen or edit["field"] not in TEXT_FIELDS or not edit["expectedText"]
                    or not edit["newText"] or edit["expectedText"] == edit["newText"]
                    or not edit["issueIds"] or preserved.intersection(edit["issueIds"])
                    or any(not re.fullmatch(r"p[012]\.\d+", issue) for issue in edit["issueIds"])):
                raise ValueError(f"Invalid, duplicate or protected text edit: {book}/{key}")
            seen.add(key)
    return manifest


def manifest_hash() -> str:
    return hashlib.sha256(MANIFEST.read_bytes()).hexdigest()


def content_revision(book_id: str, source_hash: str) -> str:
    spec = load_manifest()["books"].get(book_id)
    if spec is None:
        return source_hash
    canonical = json.dumps(spec, ensure_ascii=False, sort_keys=True, separators=(",", ":"))
    return source_hash + ":text-edits:" + hashlib.sha256(canonical.encode("utf-8")).hexdigest()


def apply_text_edits(book_id: str, source_hash: str, rows: list[dict[str, str]],
                     id_column: str = "id", columns: dict[str, str] | None = None,
                     manifest_path: Path = MANIFEST) -> tuple[list[dict[str, str]], list[dict]]:
    spec = load_manifest(manifest_path)["books"].get(book_id)
    result = [dict(row) for row in rows]
    if spec is None:
        return result, []
    if spec["sourceSha256"] != source_hash:
        raise ValueError(f"Text-edit source drift: {book_id}")
    edits = []
    for edit in spec["edits"]:
        matches = [row for row in result if row.get(id_column) == edit["id"]]
        if len(matches) != 1:
            raise ValueError(f"Text-edit target is missing or ambiguous: {book_id}/{edit['id']}")
        row = matches[0]
        field = (columns or {}).get(edit["field"], edit["field"])
        if row.get(field) != edit["expectedText"]:
            raise ValueError(f"Text-edit precondition failed: {book_id}/{edit['id']}/{field}")
        row[field] = edit["newText"]
        edits.append({"kind": "user-approved-text-edit", "bookId": book_id,
                      "sourceId": book_id + "__" + edit["id"], "row": int(row["row"]),
                      "field": edit["field"], "before": edit["expectedText"],
                      "after": edit["newText"], "issueIds": edit["issueIds"]})
    return result, edits

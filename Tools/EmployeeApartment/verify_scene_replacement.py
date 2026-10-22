"""Check protected Unity scene objects without loading Unity or changing the scene.

Only Unity's text-serialized document boundaries, local fileID references and
hierarchy fields are parsed. Document order and line endings are normalized;
field/list order is retained because component and child order can be meaningful.
This verifies serialized scene preservation, not imported assets or playability.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
from dataclasses import dataclass
from pathlib import Path


DEFAULT_ROOTS = (
    "Reference exterior corridor",
    "Entrance hinge",
    "Entrance frame",
    "Door camera alignment",
    "Door camera outside",
)
GAMEPLAY_OBJECTS = (
    "Player",
    "Player spawn",
    "MiniGame Bootstrap",
    "Computer approach",
    "Bed approach",
    "Computer interaction collider",
    "Bed interaction collider",
)
HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$", re.MULTILINE)
COMPONENT = re.compile(r"^  - component: \{fileID: (-?\d+)\}\s*$", re.MULTILINE)
CHILD = re.compile(r"^  - \{fileID: (-?\d+)\}\s*$", re.MULTILINE)


@dataclass(frozen=True)
class Document:
    file_id: int
    class_id: int
    text: str

    def reference(self, field: str) -> int | None:
        match = re.search(
            rf"^  {re.escape(field)}: \{{fileID: (-?\d+)\}}\s*$",
            self.text,
            re.MULTILINE,
        )
        return int(match[1]) if match else None

    def name(self) -> str | None:
        match = re.search(r"^  m_Name: ([^\n]*)$", self.text, re.MULTILINE)
        if not match:
            return None
        value = match[1].strip()
        if value.startswith('"'):
            return str(json.loads(value))
        if value.startswith("'") and value.endswith("'"):
            return value[1:-1].replace("''", "'")
        return value


class Scene:
    def __init__(self, path: Path) -> None:
        self.path = path.resolve()
        text = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
        matches = list(HEADER.finditer(text))
        if not matches:
            raise ValueError(f"No Unity YAML documents found: {path}")
        self.documents: dict[int, Document] = {}
        for index, match in enumerate(matches):
            end = matches[index + 1].start() if index + 1 < len(matches) else len(text)
            document = Document(
                int(match[2]),
                int(match[1]),
                "\n".join(line.rstrip() for line in text[match.start():end].splitlines())
                .rstrip()
                + "\n",
            )
            if document.file_id in self.documents:
                raise ValueError(f"Duplicate fileID {document.file_id}: {path}")
            self.documents[document.file_id] = document

    def named_object(self, name: str) -> Document:
        found = [
            document for document in self.documents.values()
            if document.class_id == 1 and document.name() == name
        ]
        if len(found) != 1:
            raise ValueError(f"Expected one GameObject named {name!r}, found {len(found)}")
        return found[0]

    def selection(self, name: str, descendants: bool) -> dict[int, Document]:
        root = self.named_object(name)
        objects = {root.file_id}
        transforms = {
            document.file_id: document
            for document in self.documents.values()
            if document.class_id in (4, 224)
        }
        selected_transforms: set[int] = set()
        changed = True
        while changed:
            changed = False
            for file_id, transform in transforms.items():
                owner = transform.reference("m_GameObject")
                parent = transform.reference("m_Father")
                if owner in objects or (descendants and parent in selected_transforms):
                    if file_id not in selected_transforms:
                        selected_transforms.add(file_id)
                        changed = True
                    if owner is not None:
                        objects.add(owner)

        selected = objects | selected_transforms
        for document in self.documents.values():
            if document.reference("m_GameObject") in objects:
                selected.add(document.file_id)
        for object_id in objects:
            if object_id not in self.documents:
                raise ValueError(f"Transform points to missing GameObject {object_id}")
            for match in COMPONENT.finditer(self.documents[object_id].text):
                component_id = int(match[1])
                if component_id not in self.documents:
                    raise ValueError(f"GameObject {object_id} has missing component {component_id}")
                selected.add(component_id)

        # Prefab children can omit m_Father in stripped Transform documents.
        # Include each attached prefab's override document and stripped records.
        if descendants:
            prefab_ids = {
                document.file_id
                for document in self.documents.values()
                if document.class_id == 1001
                and re.search(
                    r"^    m_TransformParent: \{fileID: (-?\d+)\}\s*$",
                    document.text,
                    re.MULTILINE,
                )
                and self.prefab_parent(document) in selected_transforms
            }
            selected.update(prefab_ids)
            for document in self.documents.values():
                if document.reference("m_PrefabInstance") in prefab_ids:
                    selected.add(document.file_id)
            for transform_id in selected_transforms:
                transform = transforms[transform_id]
                for match in CHILD.finditer(transform.text):
                    child_id = int(match[1])
                    if child_id not in selected:
                        raise ValueError(
                            f"Protected Transform {transform_id} has unresolvable child {child_id}"
                        )
        return {file_id: self.documents[file_id] for file_id in sorted(selected)}

    @staticmethod
    def prefab_parent(document: Document) -> int | None:
        match = re.search(
            r"^    m_TransformParent: \{fileID: (-?\d+)\}\s*$",
            document.text,
            re.MULTILINE,
        )
        return int(match[1]) if match else None


def digest(documents: dict[int, Document]) -> str:
    payload = "".join(documents[file_id].text for file_id in sorted(documents))
    return hashlib.sha256(payload.encode("utf-8")).hexdigest()


def counts(documents: dict[int, Document]) -> dict[str, int]:
    return {
        "documents": len(documents),
        "game_objects": sum(document.class_id == 1 for document in documents.values()),
        "components": sum(
            document.class_id != 1 and document.reference("m_GameObject") is not None
            for document in documents.values()
        ),
        "prefab_instances": sum(
            document.class_id == 1001 for document in documents.values()
        ),
    }


def compare(before: Scene, after: Scene, name: str, descendants: bool) -> dict[str, object]:
    report: dict[str, object] = {"name": name, "include_descendants": descendants}
    try:
        original = before.selection(name, descendants)
        current = after.selection(name, descendants)
        missing = sorted(original.keys() - current.keys())
        added = sorted(current.keys() - original.keys())
        changed = sorted(
            file_id for file_id in original.keys() & current.keys()
            if original[file_id].text != current[file_id].text
        )
        report.update({
            "preserved": not (missing or added or changed),
            "before_root_file_id": before.named_object(name).file_id,
            "after_root_file_id": after.named_object(name).file_id,
            "before_counts": counts(original),
            "after_counts": counts(current),
            "before_sha256": digest(original),
            "after_sha256": digest(current),
            "missing_file_ids": missing,
            "added_file_ids": added,
            "changed_file_ids": changed,
        })
    except ValueError as error:
        report.update({"preserved": False, "error": str(error)})
    return report


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--before", type=Path, required=True)
    parser.add_argument("--after", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument(
        "--root", action="append", default=[],
        help="Additional exact GameObject name whose complete subtree must be preserved.",
    )
    parser.add_argument(
        "--object", action="append", default=[],
        help="Additional GameObject and all its component documents, without descendants.",
    )
    parser.add_argument(
        "--include-gameplay", action="store_true",
        help="Also require unchanged player/bootstrap and computer/bed anchor components.",
    )
    args = parser.parse_args()
    report: dict[str, object] = {
        "before": str(args.before.resolve()),
        "after": str(args.after.resolve()),
        "normalization": "LF line endings, trimmed trailing whitespace, documents sorted by fileID",
        "scope": "Scene serialized objects/components only; external assets and runtime are not checked.",
    }
    try:
        before, after = Scene(args.before), Scene(args.after)
        roots = list(dict.fromkeys((*DEFAULT_ROOTS, *args.root)))
        objects = list(dict.fromkeys((*args.object, *(GAMEPLAY_OBJECTS if args.include_gameplay else ()))))
        checks = [compare(before, after, name, True) for name in roots]
        checks.extend(compare(before, after, name, False) for name in objects if name not in roots)
        report.update({"preserved": all(check["preserved"] for check in checks), "checks": checks})
    except (OSError, ValueError) as error:
        report.update({"preserved": False, "error": str(error)})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report["preserved"] else 1


if __name__ == "__main__":
    raise SystemExit(main())

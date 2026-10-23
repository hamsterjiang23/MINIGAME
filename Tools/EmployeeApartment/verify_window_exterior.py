"""Verify every pre-existing Unity document with a narrow window-exterior allowance."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path

from verify_scene_replacement import Scene


OLD_BACKDROP = {
    "B_Wall__3_.003", "B_Wall__3_.012", "B_Wall__3_.013",
    "B_立方体.003", "B_立方体.016", "B_立方体.017", "B_立方体.019",
    "B_立方体.020", "B_立方体.021", "B_立方体.022", "B_立方体.024",
    "B_立方体.025", "B_立方体.026", "B_立方体.027", "B_立方体.028",
}


def verify(before_path: Path, after_path: Path) -> dict[str, object]:
    before, after = Scene(before_path), Scene(after_path)
    unexpected: list[dict[str, object]] = []
    unchanged: list[int] = []
    hidden: list[str] = []
    camera_changes = 0
    root_appended = False
    model_meta = Path(__file__).resolve().parents[2] / "UnityProject/Assets/MiniGame/Art/WindowExterior/MainRoom.fbx.meta"
    guid_match = re.search(r"^guid: ([0-9a-f]{32})$", model_meta.read_text(encoding="utf-8"), re.MULTILINE)
    if guid_match is None:
        raise ValueError("Exterior model metadata has no GUID")
    expected_guid = guid_match[1]
    added = set(after.documents) - set(before.documents)
    for file_id, original in before.documents.items():
        current = after.documents.get(file_id)
        if current is None:
            unexpected.append({"fileID": file_id, "reason": "removed"})
            continue
        if current.text == original.text:
            unchanged.append(file_id)
            continue
        owner = before.documents.get(original.reference("m_GameObject"))
        name = owner.name() if owner else None
        if original.class_id == 23 and name in OLD_BACKDROP and current.text == original.text.replace(
                "  m_Enabled: 1\n", "  m_Enabled: 0\n", 1):
            hidden.append(name)
        elif original.class_id == 20 and name == "Main Camera" and current.text == original.text.replace(
                "  far clip plane: 45\n", "  far clip plane: 300\n", 1):
            camera_changes += 1
        elif original.class_id == 1660057539:
            old_lines, new_lines = original.text.splitlines(), current.text.splitlines()
            extra = new_lines[len(old_lines):]
            valid_append = len(extra) == 1 and new_lines[:len(old_lines)] == old_lines
            match = re.fullmatch(r"  - \{fileID: (-?\d+)\}", extra[0]) if valid_append else None
            if not match or int(match[1]) not in added:
                unexpected.append({"fileID": file_id, "reason": "existing scene roots changed"})
            else:
                root_appended = True
        else:
            unexpected.append({"fileID": file_id, "classID": original.class_id,
                               "name": name, "reason": "unapproved document change"})
    exterior_instance = len(added) == 1 and all(
        after.documents[file_id].class_id == 1001
        and "      value: Window exterior - Employee Apartment 1.0\n" in after.documents[file_id].text
        and re.search(rf"^  m_SourcePrefab: \{{fileID: 100100000, guid: {expected_guid}, type: 3\}}$",
                      after.documents[file_id].text, re.MULTILINE) is not None
        for file_id in added)
    complete = not unexpected and set(hidden) == OLD_BACKDROP and camera_changes == 1 and exterior_instance and root_appended
    return {"passed": complete, "beforeDocuments": len(before.documents), "afterDocuments": len(after.documents),
            "unchangedOriginalDocuments": len(unchanged), "hiddenBackdrop": sorted(hidden),
            "cameraFarChanges": camera_changes, "singleExteriorPrefabAdded": exterior_instance,
            "exteriorRootAppended": root_appended, "exteriorModelGuid": expected_guid,
            "unexpectedChanges": unexpected,
            "scope": "All original room, light, collider and gameplay documents are identical except 15 exterior renderer flags and the camera far plane; one exterior prefab and its root reference are added.",
            "beforeSha256": hashlib.sha256(before_path.read_bytes()).hexdigest(),
            "afterSha256": hashlib.sha256(after_path.read_bytes()).hexdigest()}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--before", type=Path, required=True)
    parser.add_argument("--after", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    report = verify(args.before, args.after)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())

"""Cheap source/hash/geometry checks independent of Blender and Unity."""
from __future__ import annotations

import hashlib
import json
import struct
from pathlib import Path


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    folder = Path(__file__).parent
    report = json.loads((folder / "geometry-report.json").read_text(encoding="utf-8"))
    layout = json.loads((folder / "layout.json").read_text(encoding="utf-8"))
    original = {item["object"]: item for item in report["originalTransforms"]}
    exported = {item["name"]: item for item in report["exportedObjects"]}
    unchanged = ["B_SM_bed_1", "B_SM_mattress_1", "B_SM_pillow_1", "B_SM_rug_2",
                 "B_obj_01", "B_HT01", "B_HT01.001", "B_SM_CupboardChunk"]
    for name in unchanged:
        for source, target in zip(original[name]["bounds"], exported[name]["bounds"]):
            assert max(abs(a - b) for a, b in zip(source, target)) < .00002, name
    concrete = [name for name in exported if name.startswith("B_ExteriorConcrete_")]
    assert len(concrete) == 2
    for name in concrete:
        source_name = "B_" + name.removeprefix("B_ExteriorConcrete_")
        for source, target in zip(original[source_name]["bounds"], exported[name]["bounds"]):
            assert max(abs(a - b) for a, b in zip(source, target)) < .00002, name
    raw = (folder / "MainRoomReference.glb").read_bytes()
    json_size = struct.unpack_from("<I", raw, 12)[0]
    doc = json.loads(raw[20:20 + json_size])
    assert len(doc["meshes"]) == report["meshCount"]
    assert not any("human" in item.get("name", "").lower() for item in doc["nodes"])
    assert not any("HAIBAO" in item.get("name", "") for item in doc["materials"])
    assert any("Reference WoodFlooring14" in item.get("name", "") for item in doc["materials"])
    for item in report["inputs"]:
        assert hashlib.sha256((root / item["path"]).read_bytes()).hexdigest() == item["sha256"]
    # Original source hash also equals its existing Git-LFS object, independently of this report.
    assert report["inputs"][1]["sha256"] == "a0ff5836d10c188f8fac7496772762ed745ac5a2e1aff2c073ec6a3b45b2dd35"
    assert all(abs(a - b) < .00001 for a, b in zip(layout["interiorSize"], [9.70078, 4.41176, 3.12066]))
    floor = exported["B_Floor"]["bounds"]
    assert abs(floor[1][0] - floor[0][0] - layout["interiorSize"][0]) < .0001
    assert abs(floor[1][1] - floor[0][1] - layout["interiorSize"][1]) < .0001
    door = layout["door"]
    centre = layout["markers"]["DoorCentre"]
    sill = layout["interiorMinimum"][2]
    for name, item in exported.items():
        if not name.startswith("B_ReferenceDoorWall"):
            continue
        lower, upper = item["bounds"]
        overlaps_opening = (lower[1] < centre[1] + door["width"] / 2 - .001
                            and upper[1] > centre[1] - door["width"] / 2 + .001
                            and lower[2] < sill + door["height"] - .001
                            and upper[2] > sill + .001)
        assert not overlaps_opening, name
    result = {"passed": True, "meshCount": report["meshCount"], "triangles": report["triangles"],
              "interiorSize": layout["interiorSize"], "unchangedFurnitureBounds": unchanged,
              "unchangedExteriorConcreteBounds": concrete, "originalSourceHashMatchesGitLfs": True,
              "floorUsesOriginalNestedTexture": True, "doorwayClear": True,
              "scope": "Static art/hash validation; Unity import/gameplay checks run separately"}
    (folder / "verification.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps(result))


if __name__ == "__main__":
    main()

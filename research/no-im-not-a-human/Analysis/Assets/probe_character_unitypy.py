"""Bounded, read-only probe of one shipped CharacterSOData object."""

from __future__ import annotations

import hashlib
import importlib.metadata
import json
import time
import traceback
from pathlib import Path
from typing import Any

import UnityPy
from UnityPy.helpers import TypeTreeHelper
from UnityPy.helpers.TypeTreeGenerator import TypeTreeGenerator


ROOT = Path(__file__).resolve().parents[2]
INPUT = ROOT / "Analysis/Inputs/GameCopy"
OUTPUT = ROOT / "Analysis/Assets"
TARGET_NAME = "Alkonost"


def save_status(record: dict[str, object]) -> None:
    (OUTPUT / "unitypy-status.json").write_text(json.dumps(record, ensure_ascii=True, indent=2), encoding="utf-8")


def main() -> None:
    started = time.monotonic()
    record: dict[str, object] = {
        "status": "probe-started", "readOnly": True,
        "packages": {"UnityPy": importlib.metadata.version("UnityPy"), "TypeTreeGeneratorAPI": importlib.metadata.version("TypeTreeGeneratorAPI")},
        "targetName": TARGET_NAME, "unityVersion": "6000.3.10f1",
        "officialDocumentation": "Analysis/Assets/UnityPy-README.md",
        "attemptLimit": "One MonoBehaviour from one asset file; no source mutation",
    }
    save_status(record)
    try:
        TypeTreeHelper.read_typetree_boost = False
        asset_path = INPUT / "NoImNotAHuman_Data/sharedassets0.assets"
        env = UnityPy.load(str(asset_path))
        target: Any | None = None
        visited = 0
        for obj in env.objects:
            if obj.type.name != "MonoBehaviour":
                continue
            visited += 1
            header = obj.parse_monobehaviour_head()
            if header.m_Name == TARGET_NAME:
                target = obj
                break
        record["monoBehaviourHeadersChecked"] = visited
        if target is None:
            record["status"] = "target-not-found-in-selected-asset"
            return
        raw = target.get_raw_data()
        raw_path = OUTPUT / "Alkonost.original.raw.bin"
        raw_path.write_bytes(raw)
        record["raw"] = {"path": str(raw_path), "bytes": len(raw), "sha256": hashlib.sha256(raw).hexdigest()}
        record["source"] = {"assetFile": str(asset_path), "pathID": target.path_id, "byteStart": target.byte_start, "byteSize": target.byte_size, "unityVersionObserved": target.assets_file.unity_version}
        save_status(record)
        generator = TypeTreeGenerator("6000.3.10f1")
        generator.load_local_dll_folder(str(INPUT / "NoImNotAHuman_BackUpThisFolder_ButDontShipItWithYourGame/Managed"))
        env.typetree_generator = generator
        node = generator.get_nodes_up("Assembly-CSharp.dll", "_Code.Characters.CharacterSOData")
        record["generatedTypeTree"] = {"type": node.m_Type, "rootName": node.m_Name, "directChildren": [child.m_Name for child in node.m_Children]}
        original_ref_resolver = TypeTreeHelper.get_ref_type_node
        fallback_types: list[dict[str, object]] = []

        def generated_reference_type(ref_object: dict[str, Any], assetfile: Any) -> Any:
            try:
                return original_ref_resolver(ref_object, assetfile)
            except ValueError:
                reference_type = ref_object["type"]
                name = reference_type["class"]
                namespace = reference_type["ns"]
                assembly = reference_type["asm"]
                if not name:
                    return None
                qualified_name = f"{namespace}.{name}" if namespace else name
                generated = generator.get_nodes_up(assembly, qualified_name)
                fallback_types.append({"assembly": assembly, "qualifiedName": qualified_name, "children": [child.m_Name for child in generated.m_Children]})
                return generated

        TypeTreeHelper.get_ref_type_node = generated_reference_type
        record["referenceTypeResolution"] = "Use original embedded ref_types first; only if absent generate exact serialized assembly/namespace/class from original Managed DLLs"
        record["fallbackTypes"] = fallback_types
        parsed = target.parse_as_dict(node, check_read=True)
        parsed_path = OUTPUT / "Alkonost.unitypy.parsed.json"
        parsed_path.write_text(json.dumps(parsed, ensure_ascii=True, indent=2), encoding="utf-8")
        record["status"] = "parsed-with-exact-byte-consumption"
        record["fieldEvidence"] = {"path": str(parsed_path), "keys": list(parsed)}
    except Exception as exc:
        record["status"] = "probe-failed"
        record["error"] = {"type": type(exc).__name__, "message": str(exc), "traceback": traceback.format_exc()}
    finally:
        record["elapsedSeconds"] = time.monotonic() - started
        save_status(record)
        print(json.dumps(record, ensure_ascii=True, indent=2))


if __name__ == "__main__":
    main()

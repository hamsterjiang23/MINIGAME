"""Record and verify a local research copy without modifying either input tree."""

from __future__ import annotations

import argparse
import hashlib
import json
import time
from pathlib import Path
from typing import TypedDict, cast


class FileRecord(TypedDict):
    path: str
    bytes: int
    mtime_ns: int
    sha256: str


class Manifest(TypedDict):
    source: str
    directories: list[str]
    files: list[FileRecord]
    file_count: int
    total_bytes: int


def digest(path: Path) -> str:
    result = hashlib.sha256()
    with path.open("rb") as stream:
        while chunk := stream.read(1024 * 1024):
            result.update(chunk)
    return result.hexdigest()


def snapshot(root: Path) -> Manifest:
    root = root.resolve(strict=True)
    directories: list[str] = []
    records: list[FileRecord] = []
    started = time.monotonic()
    for path in sorted(root.rglob("*")):
        if path.is_symlink() or path.is_junction():
            raise RuntimeError(f"Refusing reparse point: {path}")
        relative = path.relative_to(root).as_posix()
        if path.is_dir():
            directories.append(relative)
            continue
        before = path.stat()
        checksum = digest(path)
        after = path.stat()
        if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns):
            raise RuntimeError(f"Input changed while hashing: {path}")
        records.append({"path": relative, "bytes": after.st_size,
                        "mtime_ns": after.st_mtime_ns, "sha256": checksum})
        if len(records) % 2000 == 0:
            print(f"Hashed {len(records)} files in {time.monotonic() - started:.1f}s", flush=True)
    return {"source": str(root), "directories": directories, "files": records,
            "file_count": len(records), "total_bytes": sum(row["bytes"] for row in records)}


def write_json(path: Path, record: object) -> None:
    path.write_text(json.dumps(record, ensure_ascii=False, indent=2), encoding="utf-8")


def verify(manifest: Manifest, source: Path, destination: Path, changes: dict[str, str]) -> dict[str, object]:
    source = source.resolve(strict=True)
    destination = destination.resolve(strict=True)
    if source == destination or source.is_relative_to(destination) or destination.is_relative_to(source):
        raise RuntimeError("Source and destination must be separate trees")
    if str(source).casefold() != manifest["source"].casefold():
        raise RuntimeError("Manifest source does not match the selected source")
    source_entries = list(source.rglob("*"))
    destination_entries = list(destination.rglob("*"))
    for path in [*source_entries, *destination_entries]:
        if path.is_symlink() or path.is_junction():
            raise RuntimeError(f"Refusing reparse point during final verification: {path}")
    source_paths = {path.relative_to(source).as_posix() for path in source_entries if path.is_file()}
    expected_paths = {row["path"] for row in manifest["files"]}
    destination_paths = {path.relative_to(destination).as_posix() for path in destination_entries if path.is_file()}
    issues: list[dict[str, str]] = []
    if source_paths != expected_paths:
        issues.append({"reason": "source file set changed"})
    source_directories = {path.relative_to(source).as_posix() for path in source_entries if path.is_dir()}
    if source_directories != set(manifest["directories"]):
        issues.append({"reason": "source directory set changed"})
    destination_directories = {path.relative_to(destination).as_posix() for path in destination_entries if path.is_dir()}
    extra_directories = destination_directories - set(manifest["directories"])
    for directory in sorted(extra_directories):
        if directory != "Analysis/Relocation" and not directory.startswith("Analysis/Relocation/"):
            issues.append({"path": directory, "reason": "unexpected destination directory"})
    for directory in manifest["directories"]:
        if not (source / directory).is_dir() or not (destination / directory).is_dir():
            issues.append({"path": directory, "reason": "missing directory"})
    for index, row in enumerate(manifest["files"], start=1):
        relative = row["path"]
        old_path, new_path = source / relative, destination / relative
        if not old_path.is_file() or not new_path.is_file():
            issues.append({"path": relative, "reason": "missing file"})
            continue
        old_stat = old_path.stat()
        if old_stat.st_size != row["bytes"] or digest(old_path) != row["sha256"]:
            issues.append({"path": relative, "reason": "source content changed"})
        expected_hash = changes.get(relative, row["sha256"])
        if digest(new_path) != expected_hash:
            issues.append({"path": relative, "reason": "destination SHA256 mismatch"})
        if relative not in changes and new_path.stat().st_size != row["bytes"]:
            issues.append({"path": relative, "reason": "destination size mismatch"})
        if index % 2000 == 0:
            print(f"Verified {index}/{len(manifest['files'])} files", flush=True)
    extras = sorted(destination_paths - expected_paths)
    unknown_extras = [path for path in extras if not path.startswith("Analysis/Relocation/")]
    if unknown_extras:
        issues.extend({"path": path, "reason": "unexpected destination file"} for path in unknown_extras)
    return {"source": str(source), "destination": str(destination),
            "files_checked": manifest["file_count"], "original_bytes": manifest["total_bytes"],
            "directories_checked": len(manifest["directories"]), "algorithm": "SHA256",
            "source_file_and_directory_sets_checked": True, "reparse_points": 0,
            "changed_destination_files": sorted(changes), "migration_files": extras,
            "issues": issues, "passed": not issues}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=["manifest", "verify"])
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--destination", type=Path)
    parser.add_argument("--manifest", type=Path, required=True)
    parser.add_argument("--output", type=Path)
    parser.add_argument("--changes", type=Path)
    args = parser.parse_args()
    if args.mode == "manifest":
        record = snapshot(args.source)
        write_json(args.manifest, record)
        print(json.dumps({key: record[key] for key in ("source", "file_count", "total_bytes")}))
        return
    if args.destination is None or args.output is None:
        parser.error("verify requires --destination and --output")
    manifest = cast(Manifest, json.loads(args.manifest.read_text(encoding="utf-8")))
    changes = cast(dict[str, str], json.loads(args.changes.read_text(encoding="utf-8"))) if args.changes else {}
    result = verify(manifest, args.source, args.destination, changes)
    write_json(args.output, result)
    print(json.dumps({key: result[key] for key in ("files_checked", "original_bytes", "passed", "issues")}))
    if not result["passed"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()

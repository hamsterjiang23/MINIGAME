"""Import selected v0.8.26 display assets and the five authored dossier records."""
from __future__ import annotations

import argparse
import hashlib
import json
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MAPPINGS = (
    ("小孩", "child", "ML01"),
    ("神父", "priest", "JN02"),
    ("女性执政官", "former-ruler", "VV01"),
    ("外交官", "diplomat", "AD01"),
    ("战地医生", "battlefield-doctor", "ED02"),
)


def extract_profiles(source: Path) -> list[dict[str, str]]:
    ns = {"w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main"}
    with zipfile.ZipFile(source) as archive:
        document = ET.fromstring(archive.read("word/document.xml"))
    paragraphs = ["".join(node.text or "" for node in paragraph.findall(".//w:t", ns))
                  for paragraph in document.findall(".//w:p", ns)]
    records = []
    for name, patient_id, portrait_id in MAPPINGS:
        if paragraphs.count(name) != 1:
            raise ValueError(f"Expected one heading for {name}")
        start = paragraphs.index(name)
        fields = paragraphs[start + 1:start + 4]
        prefixes = ("年龄：", "职业：", "登记摘要：")
        if len(fields) != 3 or not all(text.startswith(prefix) for text, prefix in zip(fields, prefixes)):
            raise ValueError(f"Unexpected dossier fields after {name}")
        age, occupation, summary = (text[len(prefix):] for text, prefix in zip(fields, prefixes))
        records.append(dict(patientId=patient_id, portraitId=portrait_id, age=age,
                            occupation=occupation, summary=summary))
    records.extend([
        dict(patientId="self-review", portraitId="SJ01", age="未登记", occupation="未登记",
             summary="第三安置所值班审查员。本人复核记录。"),
        dict(patientId="tutorial", portraitId="", age="未登记", occupation="未登记",
             summary="身份尚未登记的收容者。等待本次审查。"),
    ])
    return records


def encoded(value: object) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def import_sources(package: Path, document: Path, html: Path, output: Path, check: bool) -> None:
    def install(relative: str, data: bytes) -> None:
        target = output / relative
        if check:
            if not target.is_file() or target.read_bytes() != data:
                raise ValueError(f"Source or generated file differs: {relative}")
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(data)

    records = extract_profiles(document)
    prefix = "Assets/PMSAmberUI_v0826/"
    selected = ["Portraits/Native/" + key + "_Amber.png" for key in ("ML01", "JN02", "VV01", "AD01", "ED02", "SJ01")]
    selected += ["Portraits/Portraits.json", "UI/Frame_Directory_800x600.png",
                 "Fonts/DossierQuota_00_99.png", "Fonts/DossierQuota.json", "Documentation/Layout.json",
                 "Documentation/ThirdPartyNotices.md", "README.md"]
    asset_hashes = {}
    with zipfile.ZipFile(package) as archive:
        if archive.read("Reference/PMS_v0.8.26.html") != html.read_bytes():
            raise ValueError("The provided HTML differs from the resource package reference")
        for relative in selected:
            data = archive.read(prefix + relative)
            install("UnityProject/" + prefix + relative, data)
            asset_hashes[relative] = hashlib.sha256(data).hexdigest()
    profiles = dict(formatVersion=1, sourceSha256=hashlib.sha256(document.read_bytes()).hexdigest(), entries=records)
    install("UnityProject/Assets/MiniGame/Content/TerminalDossier/profiles.json", encoded(profiles))
    install("Art/TerminalDossier/Source/角色信息缩略版.docx", document.read_bytes())
    install("docs/terminal-htmlmatch/reference-v0.8.26.html", html.read_bytes())
    manifest = dict(version="0.8.26", packageSha256=hashlib.sha256(package.read_bytes()).hexdigest(),
                    documentSha256=profiles["sourceSha256"], htmlSha256=hashlib.sha256(html.read_bytes()).hexdigest(),
                    sources=dict(package=str(package), document=str(document), html=str(html)),
                    importedAssets=asset_hashes, profiles=records)
    install("docs/terminal-htmlmatch/dossier-v0.8.26/source-manifest.json", encoded(manifest))
    print(json.dumps(dict(checked=check, importedAssets=len(selected), profiles=len(records),
                          authoredProfiles=len(MAPPINGS)), ensure_ascii=False))


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--package", type=Path, required=True)
    parser.add_argument("--document", type=Path, required=True)
    parser.add_argument("--html", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=ROOT)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    import_sources(args.package, args.document, args.html, args.output, args.check)


if __name__ == "__main__":
    main()

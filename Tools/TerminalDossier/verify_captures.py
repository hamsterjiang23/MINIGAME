# /// script
# requires-python = ">=3.10"
# dependencies = ["Pillow==12.3.0"]
# ///
"""Verify dossier Player/Editor captures against imported source art and authored metadata."""
from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image, ImageChops

ROOT = Path(__file__).resolve().parents[2]


def verify(folder: Path) -> dict[str, object]:
    report = json.loads((folder / "report.json").read_text(encoding="utf-8-sig"))
    if report["result"] != "Passed":
        raise ValueError(f"Probe did not pass: {folder}")
    source = json.loads((ROOT / "UnityProject/Assets/MiniGame/Content/TerminalDossier/profiles.json").read_text(encoding="utf-8"))
    profiles = {entry["patientId"]: entry for entry in source["entries"]}
    observed: set[str] = set()
    portraits: list[dict[str, object]] = []
    screen_captures_visible = True
    for record in report["captures"]:
        patient = record["patient"]
        observed.add(patient)
        profile = profiles[patient]
        for key in ("age", "occupation"):
            if record[key] != profile[key]:
                raise ValueError(f"{record['name']}: {key} mismatch")
        if record["fullSummary"] != profile["summary"]:
            raise ValueError(f"{record['name']}: full summary mismatch")
        native = Image.open(folder / (record["name"] + "-native.png")).convert("RGB")
        screen = Image.open(folder / (record["name"] + "-screen.png")).convert("RGB")
        if native.size != (800, 600) or screen.size != (record["width"], record["height"]):
            raise ValueError(f"{record['name']}: capture dimensions mismatch")
        visible_screen = screen.getbbox() is not None
        screen_captures_visible = screen_captures_visible and visible_screen
        if report["environment"] == "Development Player" and not visible_screen:
            raise ValueError(f"{record['name']}: Player screen capture is blank")
        if patient == "tutorial":
            continue
        original = Image.open(ROOT / "UnityProject/Assets/PMSAmberUI_v0826/Portraits/Native" / (profile["portraitId"] + "_Amber.png")).convert("RGB")
        actual = native.crop((31, 323, 127, 451))
        maximum = max(high for low, high in ImageChops.difference(original, actual).getextrema())
        portraits.append(dict(capture=record["name"], portrait=profile["portraitId"], maximumChannelDifference=maximum))
        if maximum > 1:
            raise ValueError(f"{record['name']}: portrait differs from original by {maximum}/255")
    if observed != set(profiles):
        raise ValueError(f"Missing patient coverage: {set(profiles) - observed}")
    return dict(folder=str(folder), result="Passed", environment=report["environment"],
                screenSize=[report["width"], report["height"]], nativeSize=[800, 600],
                screenCapturesVisible=screen_captures_visible,
                patients=sorted(observed), captureCount=len(report["captures"]),
                wheelEvents=report["wheelEvents"], arrowClicks=report["arrowClicks"],
                readonlyChecks=report["readonlyChecks"], portraitComparisons=portraits,
                nativeWheelDown=report.get("nativeWheelDown", False), nativeWheelUp=report.get("nativeWheelUp", False),
                nativeDownInputFrames=report.get("nativeDownInputFrames", 0), nativeUpInputFrames=report.get("nativeUpInputFrames", 0))


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("folders", nargs="+", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    results = [verify(folder) for folder in args.folders]
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(dict(result="Passed", runs=results), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(result="Passed", runs=len(results), captures=sum(int(run["captureCount"]) for run in results)), ensure_ascii=False))


if __name__ == "__main__":
    main()

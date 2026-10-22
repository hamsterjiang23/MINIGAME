"""Package Blender's Ocean visibility pass as a native Unity Cubemap."""

import json
import re

from package_cubemap import FACES, OUTPUT, ROOT, UNITY, meta
from PIL import Image, ImageFilter, ImageStat


def main() -> None:
    size = 512
    payload = bytearray()
    metrics = []
    contact = Image.new("RGB", (3 * 256, 2 * 256))
    for index, name in enumerate(FACES):
        with Image.open(OUTPUT / "WaterMask" / (name + ".png")) as image:
            if image.size != (size, size):
                raise ValueError("Unexpected mask size")
            mask = image.convert("L").filter(ImageFilter.MinFilter(3))
        contact.paste(mask.resize((256, 256)), ((index % 3) * 256, (index // 3) * 256))
        rgba = Image.merge("RGBA", (mask, mask, mask, Image.new("L", mask.size, 255)))
        payload.extend(rgba.tobytes())
        metrics.append(
            {"face": name, "mean_water_coverage": ImageStat.Stat(mask).mean[0] / 255}
        )
    contact.save(OUTPUT / "water-mask-contact.png")
    template = (UNITY / "Art/DreamEnding/DoorwayBeachPanorama.asset").read_text(
        encoding="utf-8"
    )
    header, remainder = template.split("  _typelessdata: ", 1)
    _, separator, suffix = remainder.partition("\n")
    header = header.replace(
        "m_Name: DoorwayBeachPanorama", "m_Name: SeaMidpointWaterMask"
    )
    header = header.replace("m_Width: 1024", "m_Width: 512").replace(
        "m_Height: 1024", "m_Height: 512"
    )
    header = header.replace(
        "m_CompleteImageSize: 4194304", "m_CompleteImageSize: 1048576"
    )
    header = header.replace("image data: 25165824", f"image data: {len(payload)}")
    path = UNITY / "Art/DreamEnding/SeaMidpointWaterMask.asset"
    path.write_text(
        header + "  _typelessdata: " + payload.hex() + separator + suffix,
        encoding="utf-8",
    )
    guid = meta(path, 8900000)
    profile = UNITY / "Resources/MiniGame/DreamSequenceDefault.asset"
    text = profile.read_text(encoding="utf-8")
    capture = json.loads((OUTPUT / "capture-settings.json").read_text(encoding="utf-8"))
    fields = {
        "EnableEndingRetro": "1",
        "EndingRetro": "{fileID: 11400000, guid: b53af644d95a439c8f45888661bf2ff2, type: 2}",
        "AnimateRoomWater": "1",
        "PanoramaWaterMask": f"{{fileID: 8900000, guid: {guid}, type: 2}}",
        "PanoramaSeaLevel": str(capture.get("panorama_sea_level", -6.5)),
        "PanoramaShoreDistance": "12",
    }
    for key, value in fields.items():
        pattern = rf"^  {key}:.*$"
        if re.search(pattern, text, re.MULTILINE):
            if key in {"PanoramaWaterMask", "PanoramaSeaLevel"}:
                text = re.sub(pattern, f"  {key}: {value}", text, flags=re.MULTILINE)
        else:
            text += f"  {key}: {value}\n"
    profile.write_text(text, encoding="utf-8")
    (OUTPUT / "water-mask-verification.json").write_text(
        json.dumps(
            {
                "face_size": size,
                "mask_guid": guid,
                "faces": metrics,
                "unity_mask": str(path.relative_to(ROOT)),
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    print(json.dumps(metrics))


if __name__ == "__main__":
    main()

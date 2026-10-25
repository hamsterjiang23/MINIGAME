"""Bake the streaming ocean clip's smooth loudness envelope. Requires numpy.

Run from any directory with Python; the default source and output are repository assets.
"""
import argparse
import hashlib
import json
import wave
from pathlib import Path

import numpy as np


def main():
    root = Path(__file__).resolve().parents[2]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path,
                        default=root / "UnityProject/Assets/MiniGame/Art/DreamEnding/OceanWaves.wav")
    parser.add_argument("--output", type=Path,
                        default=root / "UnityProject/Assets/MiniGame/Resources/MiniGame/OceanWaveMotion.json")
    args = parser.parse_args()
    with wave.open(str(args.source)) as audio:
        if audio.getsampwidth() != 2 or audio.getframerate() % 20:
            raise ValueError("Expected PCM16 audio with a sample rate divisible by 20")
        rate, channels, frames = audio.getframerate(), audio.getnchannels(), audio.getnframes()
        pcm = np.frombuffer(audio.readframes(frames), dtype="<i2").astype(np.float64).reshape(-1, channels) / 32768
    step = rate // 20
    if frames % step:
        pcm = np.pad(pcm, ((0, step - frames % step), (0, 0)), mode="wrap")
    rms = np.sqrt(np.mean(pcm.reshape(-1, step, channels) ** 2, axis=(1, 2)))
    # Circular smoothing preserves the recording's loop. Sigma is 0.65 seconds.
    offsets = np.arange(-52, 53)
    kernel = np.exp(-.5 * (offsets / 13) ** 2)
    kernel /= kernel.sum()
    smooth = sum(np.roll(rms, int(offset)) * weight for offset, weight in zip(offsets, kernel))
    low, high = np.percentile(smooth, [2, 98])
    lift = np.clip(2 * (smooth - low) / (high - low) - 1, -1, 1) if high > low else np.zeros_like(smooth)
    data = {"ClipName": args.source.stem, "SampleRate": rate, "PcmFrames": frames, "EnvelopeHz": 20,
            "SourceSha256": hashlib.sha256(args.source.read_bytes()).hexdigest(),
            "Lift": [round(float(value), 6) for value in lift]}
    args.output.write_text(json.dumps(data, ensure_ascii=False, separators=(",", ":")), encoding="utf-8")
    print(f"Baked {len(lift)} samples from {frames / rate:.2f}s into {args.output}")


if __name__ == "__main__":
    main()

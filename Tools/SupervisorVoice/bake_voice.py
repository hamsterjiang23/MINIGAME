"""Legacy oscillator throat grains, retained to reproduce the rejected first timbre.

The current bank is built with render_speech.ps1 + process_speech.py. This legacy
generator requires --legacy to prevent accidentally replacing the speech-based bank.
"""
import argparse
import json
import math
from pathlib import Path
import uuid
import wave

import numpy as np

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / "UnityProject/Assets/MiniGame/Resources/MiniGame/SupervisorVoice"
RATE = 24000
NAMESPACE = uuid.UUID("a60c390d-28ea-43a1-a649-03c193123ce8")


def guid(name):
    return uuid.uuid5(NAMESPACE, name).hex


def resonator(source, frequencies, bandwidth):
    radius = math.exp(-math.pi * bandwidth / RATE)
    coefficients = 2 * radius * np.cos(2 * np.pi * frequencies / RATE)
    result = np.zeros(len(source))
    y1 = y2 = 0.0
    for i, value in enumerate(source):
        y = (1 - radius) * value + coefficients[i] * y1 - radius * radius * y2
        result[i] = y
        y2, y1 = y1, y
    return result / max(1e-8, np.sqrt(np.mean(result ** 2)))


def grain(number):
    rng = np.random.default_rng(721504 + number)
    time = np.arange(round(RATE * .85)) / RATE
    wobble = np.interp(time, np.linspace(0, time[-1], 24), rng.normal(0, 1, 24))
    fundamental = 62 + 5 * math.sin(number * 2.1)
    pitch = fundamental * (1 + .045 * wobble + .065 * np.sin(2 * np.pi * 2.9 * time + number))
    phase = np.cumsum(pitch) * 2 * np.pi / RATE
    # Asymmetric glottal excitation with period-doubling/creak and turbulent breath.
    pulse = np.maximum(0, np.sin(phase)) ** 1.7 - .27 * np.maximum(0, -np.sin(phase))
    pulse *= .78 + .22 * np.sin(phase / 2 + .6)
    pulse += rng.normal(0, .075, len(time))
    pulse -= pulse.mean()
    mouth = np.sin(2 * np.pi * (1.7 + number * .08) * time + number)
    throat = (resonator(pulse, 270 + 65 * mouth, 105) * .53
              + resonator(pulse, 610 + 120 * np.sin(time * 9 + number), 160) * .28
              + resonator(pulse, 1210 + 180 * mouth, 260) * .11)
    throat += .21 * np.sin(phase / 2) + .13 * np.sin(phase)
    # Wet closed-mouth gulps, deliberately quieter than the vocal body.
    bubbles = np.zeros(len(time))
    for center in rng.uniform(.04, .8, 8):
        dt = time - center
        envelope = np.where(dt >= 0, np.exp(-np.maximum(0, dt) * 65), 0)
        chirp_phase = 2 * np.pi * (190 * dt + 310 * .009 * (1 - np.exp(-np.maximum(0, dt) / .009)))
        bubbles += .22 * envelope * np.sin(chirp_phase)
    modulation = .64 + .22 * np.sin(2 * np.pi * 4.1 * time + number) + .1 * wobble
    voice = np.tanh((throat * modulation + bubbles) * .85)
    # Remove DC/rumble, and add a small enclosed-room reflection without a long tail.
    filtered = np.zeros(len(voice))
    previous = output = 0.0
    pole = math.exp(-2 * math.pi * 48 / RATE)
    for i, value in enumerate(voice):
        output = pole * (output + value - previous)
        filtered[i] = output
        previous = value
    result = filtered.copy()
    for seconds, gain in ((.021, .08), (.047, -.055), (.089, .035)):
        delay = round(seconds * RATE)
        result[delay:] += filtered[:-delay] * gain
    fade = np.minimum(1, np.minimum(time / .025, (time[-1] - time) / .035))
    result *= np.maximum(0, fade)
    result *= .58 / max(.0001, np.max(np.abs(result)))
    return result


def write_wav(path, samples):
    with wave.open(str(path), "wb") as stream:
        stream.setnchannels(1)
        stream.setsampwidth(2)
        stream.setframerate(RATE)
        stream.writeframes(np.round(np.clip(samples, -1, 1) * 32767).astype("<i2").tobytes())


def bake():
    DEST.mkdir(parents=True, exist_ok=True)
    report = {"sampleRate": RATE, "origin": "original procedural glottal source + moving formants + wet gulp transients", "grains": []}
    for i in range(10):
        name = f"throat-{i + 1:02d}.wav"
        pcm = grain(i)
        write_wav(DEST / name, pcm)
        (DEST / (name + ".meta")).write_text(f"""fileFormatVersion: 2
guid: {guid(name)}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 24000
    compressionFormat: 0
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {{}}
  forceToMono: 1
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
""", encoding="utf-8")
        report["grains"].append({"file": name, "seconds": len(pcm) / RATE,
                                 "peak": float(np.max(np.abs(pcm))), "rms": float(np.sqrt(np.mean(pcm ** 2)))})
    (DEST / "bake-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"grains": len(report["grains"]), "durationEach": .85, "output": str(DEST)}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--legacy', action='store_true', help='Explicitly restore the old oscillator bank')
    if not parser.parse_args().legacy:
        parser.error('Use render_speech.ps1 + process_speech.py for the current speech bank, or --legacy for the rejected first timbre.')
    bake()

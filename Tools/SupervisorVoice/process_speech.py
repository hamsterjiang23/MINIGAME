"""Build muffled/lowered speech grains from render_speech.ps1 output.

Requires numpy and an FFmpeg build with rubberband. Source is synthesized speech,
not an actor recording. No pure oscillators or sub-bass layer are added.
"""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import wave

import numpy as np

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'UnityProject/Assets/MiniGame/Resources/MiniGame/SupervisorVoice'
RATE = 24000


def compact_pauses(pcm):
    """Join voiced regions, retaining 10 ms articulation edges and 12 ms crossfades."""
    hop = 240
    frames = np.pad(pcm, (0, (-len(pcm)) % hop)).reshape(-1, hop)
    rms = np.sqrt(np.mean(frames ** 2, axis=1))
    active = rms > max(.0015, np.percentile(rms, 80) * .12)
    active = np.convolve(active.astype(float), np.ones(3), mode='same') > 0
    intervals = []
    start = None
    for index in range(len(active) + 1):
        if index < len(active) and active[index]:
            if start is None:
                start = index * hop
        elif start is not None:
            intervals.append(pcm[start:min(len(pcm), index * hop)])
            start = None
    if not intervals:
        raise ValueError('No voiced regions to compact')
    output = intervals[0].copy()
    for part in intervals[1:]:
        overlap = min(round(.012 * RATE), len(output), len(part))
        ramp = np.linspace(0, 1, overlap)
        output[-overlap:] = output[-overlap:] * (1 - ramp) + part[:overlap] * ramp
        output = np.concatenate((output, part[overlap:]))
    return output


def process(ffmpeg, source):
    result = subprocess.run([ffmpeg, '-hide_banner', '-loglevel', 'error', '-i', str(source),
        '-af', 'rubberband=pitch=0.60:formant=shifted:transients=smooth,highpass=f=90,lowpass=f=950:p=2',
        '-ar', str(RATE), '-ac', '1', '-f', 'f32le', '-'], capture_output=True, check=True)
    pcm = np.frombuffer(result.stdout, dtype='<f4').astype(np.float64)
    # Retain the breath and consonant attacks, without the long TTS leading/trailing silence.
    energy = np.convolve(pcm ** 2, np.ones(240) / 240, mode='same')
    active = np.flatnonzero(energy > max(1e-7, energy.max() * .003))
    if not len(active):
        raise ValueError(f'No speech in {source}')
    pcm = pcm[max(0, active[0] - 480):min(len(pcm), active[-1] + 720)]
    pcm = compact_pauses(pcm)
    # Irregularly sized reversed syllable fragments remove recognizable articulation.
    # Varying their lengths avoids the fixed retrigger rhythm of the rejected oscillator.
    rng = np.random.default_rng(8317 + int(source.stem.split('-')[-1]))
    blurred = pcm.copy()
    begin = 0
    while begin < len(pcm):
        end = min(len(pcm), begin + round(rng.uniform(.075, .16) * RATE))
        window = np.minimum(1, np.minimum(np.arange(end - begin), np.arange(end - begin)[::-1]) / (RATE * .01))
        blurred[begin:end] = pcm[begin:end] * (1 - .85 * window) + pcm[begin:end][::-1] * .85 * window
        begin = end
    # A quiet, offset speech layer fills residual syllable valleys without synthetic hum.
    blurred = .82 * blurred + .18 * np.roll(blurred[::-1], round(.137 * RATE))
    # Quiet, irregular moving reflections give a wet gulp quality while retaining the
    # approved dense speech texture. This is delayed voice, with no added bass oscillator.
    position = np.arange(len(blurred), dtype=float)
    time = position / RATE
    delay = (.006 + .003 * np.sin(2 * np.pi * 3.8 * time)
             + .0018 * np.sin(2 * np.pi * 6.1 * time + .8)) * RATE
    wet = np.interp(position - delay, position, blurred, left=0, right=0)
    blurred = .84 * blurred + .16 * wet
    output = blurred.copy()
    for delay, gain in ((.019, .18), (.043, .12), (.079, .075)):
        frames = round(delay * RATE)
        output[frames:] += blurred[:-frames] * gain
    output -= output.mean()
    rms = np.sqrt(np.mean(output ** 2))
    # Spoken consonants have much larger peaks than the old steady oscillator. Soft
    # compression keeps the audible body near the prior level without clipping peaks.
    output = .55 * np.tanh(output / max(rms, 1e-8) * .6)
    fade = min(240, len(output) // 2)
    output[:fade] *= np.linspace(0, 1, fade)
    output[-fade:] *= np.linspace(1, 0, fade)
    return output


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--ffmpeg', required=True)
    parser.add_argument('--source', type=Path, default=ROOT / 'Artifacts/Audio/SupervisorSpeechSource')
    args = parser.parse_args()
    manifest = json.loads((args.source / 'speech-manifest.json').read_text(encoding='utf-8-sig'))
    report = {'sampleRate': RATE, 'origin': 'Dense, strongly lowered and obscured speech texture',
              'provider': manifest['provider'], 'voice': manifest['voice'], 'pitchRatio': .60,
              'highPassHz': 90, 'lowPassHz': 950, 'reversedArticulationMix': .85, 'wetMovingReflectionMix': .16,
              'pauseCompaction': True, 'reverseFragmentSeconds': [.075, .16], 'speechValleyFillMix': .18, 'grains': []}
    for index, line in enumerate(manifest['lines']):
        source = args.source / line['file']
        pcm = process(args.ffmpeg, source)
        dest = DEST / f'throat-{index + 1:02d}.wav'
        with wave.open(str(dest), 'wb') as stream:
            stream.setnchannels(1); stream.setsampwidth(2); stream.setframerate(RATE)
            stream.writeframes(np.round(pcm * 32767).astype('<i2').tobytes())
        report['grains'].append({'file': dest.name, 'sourceId': line['sourceId'],
            'sourceSha256': hashlib.sha256(source.read_bytes()).hexdigest(),
            'seconds': len(pcm) / RATE, 'peak': float(np.max(np.abs(pcm))),
            'rms': float(np.sqrt(np.mean(pcm ** 2)))})
    (DEST / 'bake-report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'grains': len(report['grains']), 'origin': report['origin'],
                     'totalSeconds': sum(item['seconds'] for item in report['grains'])}))


if __name__ == '__main__':
    main()

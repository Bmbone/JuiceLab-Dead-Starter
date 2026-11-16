"""Rebuild this checkpoint's original mono PCM cues with Python's standard library.

Run from any directory: python3 tools/generate_castle_audio.py
Only the eight WAV files in CastleAudio/Clips are replaced; Unity metadata is retained.
No external samples, sound fonts, synthesis packages or gameplay random state are used.
"""
from array import array
import math
from pathlib import Path
import random
import sys
import wave

RATE = 44100
DESTINATION = Path(__file__).resolve().parents[1] / "Assets/_Project/CastleAudio/Clips"
TAU = 2 * math.pi


def modal(data, start, duration, frequency, level, partials, decay=0.09):
    """A struck/plucked body with a smooth attack and a short release at the boundary."""
    first = round(start * RATE)
    count = min(round(duration * RATE), len(data) - first)
    for i in range(max(0, count)):
        t = i / RATE
        attack = 1 - math.exp(-t / 0.0018)
        release = min(1, max(0, (count - 1 - i) / (RATE * 0.018)))
        release = release * release * (3 - 2 * release)
        value = sum(weight * math.sin(TAU * frequency * ratio * t) * math.exp(-t * falloff / decay)
                    for ratio, weight, falloff in partials)
        data[first + i] += level * value * attack * release


def grit(data, start, duration, level, seed, dark=0.14, decay=0.045):
    """Short filtered stone/wood noise with its own fixed offline seed."""
    rng = random.Random(seed)
    first = round(start * RATE)
    count = min(round(duration * RATE), len(data) - first)
    low = 0.0
    for i in range(max(0, count)):
        t = i / RATE
        noise = rng.uniform(-1, 1)
        low += dark * (noise - low)
        attack = min(1, t / 0.0015)
        release = min(1, max(0, (count - 1 - i) / (RATE * 0.012)))
        data[first + i] += level * (low * 0.8 + (noise - low) * 0.20) * attack * release * math.exp(-t / decay)


PLUCK = [(1, 1, 1), (2, .28, 1.5), (3, .10, 2.4)]
BELL = [(1, 1, 1), (2.01, .22, 1.6), (2.76, .12, 2), (4.08, .035, 3)]
STONE = [(1, 1, 1), (1.43, .48, 1.4), (2.17, .25, 2), (3.13, .12, 3)]


def make_cues():
    cues = {}
    data = [0.0] * round(.18 * RATE)
    modal(data, 0, .17, 172, .54, STONE, .028)
    modal(data, .002, .17, 440, .30, PLUCK, .045)
    modal(data, .004, .15, 1140, .065, BELL, .035)
    grit(data, 0, .055, .33, 40201, .10, .012)
    cues["PaddleWood"] = data

    data = [0.0] * round(.14 * RATE)
    modal(data, 0, .13, 116, .61, STONE, .025)
    modal(data, .001, .10, 760, .085, BELL, .020)
    grit(data, 0, .06, .48, 40202, .09, .014)
    cues["WallStone"] = data

    for variant, frequency in enumerate([232, 251, 218]):
        data = [0.0] * round(.24 * RATE)
        modal(data, 0, .19, frequency, .48, STONE, .031)
        grit(data, 0, .19, .90, 40210 + variant, .20, .038)
        grit(data, .018, .15, .35, 40220 + variant, .12, .032)
        modal(data, .009, .22, [1175, 1319, 1047][variant], .065, BELL, .060)
        cues["BrickBreak" + str(variant + 1)] = data

    data = [0.0] * round(.68 * RATE)
    for start, hz, level in [(0, 659.25, .42), (.13, 587.33, .37), (.28, 440, .36)]:
        modal(data, start, .38, hz, level, PLUCK, .11)
    grit(data, .02, .38, .09, 40230, .035, .12)
    cues["LifeLost"] = data

    data = [0.0] * round(1.85 * RATE)
    for start, hz in [(0, 293.66), (.16, 369.99), (.32, 440), (.50, 587.33)]:
        modal(data, start, .65, hz, .30, PLUCK, .19)
        modal(data, start + .006, .65, hz * 2, .075, BELL, .18)
    for hz, level in [(293.66, .16), (369.99, .12), (440, .12), (587.33, .17)]:
        modal(data, .74, 1.10, hz, level, BELL, .35)
    cues["VictoryFanfare"] = data

    data = [0.0] * round(1.78 * RATE)
    for start, hz, level in [(0, 293.66, .31), (.23, 261.63, .28), (.48, 207.65, .28), (.76, 146.83, .37)]:
        modal(data, start, min(.94, 1.77 - start), hz, level, BELL, .24)
    modal(data, .76, .98, 220, .075, PLUCK, .22)
    cues["GameOverToll"] = data
    return cues


def write_cues():
    DESTINATION.mkdir(parents=True, exist_ok=True)
    for name, samples in make_cues().items():
        peak = max(abs(value) for value in samples)
        gain = .72 / max(peak, 1e-9)
        pcm = array("h", (round(max(-1, min(1, value * gain)) * 32767) for value in samples))
        if sys.byteorder != "little":
            pcm.byteswap()
        path = DESTINATION / (name + ".wav")
        with wave.open(str(path), "wb") as stream:
            stream.setnchannels(1)
            stream.setsampwidth(2)
            stream.setframerate(RATE)
            stream.writeframes(pcm.tobytes())
        print(f"{name}: {len(samples) / RATE:.2f}s, mono 44.1kHz PCM16")


if __name__ == "__main__":
    write_cues()

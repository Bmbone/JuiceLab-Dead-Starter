"""Compose the original two-part Brickkeep score; requires Python 3 and FFmpeg/libvorbis.

Run: python3 tools/generate_castle_music.py
The two 15-second arrangements share one 96 BPM, 3/4, eight-bar phrase and sample clock.
Tails wrap into the next loop; no external recordings, MIDI files or sound fonts are used.
Only these two OGG files are replaced. Unity metadata and existing SFX are retained.
"""
from array import array
import math
from pathlib import Path
import random
import subprocess
import sys
import tempfile
import wave

RATE = 44100
BPM = 96
BEAT = 60 / BPM
BARS = 8
SECONDS = BARS * 3 * BEAT
COUNT = round(SECONDS * RATE)
DESTINATION = Path(__file__).resolve().parents[1] / 'Assets/_Project/CastleMusic/Clips'
TAU = 2 * math.pi
LUTE = [(1, 1, 1), (2, .22, 1.7), (3, .10, 2.2), (4, .025, 3)]
BELL = [(1, 1, 1), (2.01, .13, 1.6), (2.76, .045, 2.2)]


def hz(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def pluck(data, beat, note, level, length=.7, decay=.18, partials=LUTE):
    first = round(beat * BEAT * RATE)
    count = round(length * RATE)
    frequency = hz(note)
    for i in range(count):
        t = i / RATE
        attack = 1 - math.exp(-t / .003)
        release = min(1, max(0, (count - 1 - i) / (RATE * .035)))
        release = release * release * (3 - 2 * release)
        signal = sum(weight * math.sin(TAU * frequency * ratio * t) * math.exp(-t * falloff / decay)
                     for ratio, weight, falloff in partials)
        data[(first + i) % COUNT] += signal * level * attack * release


def drum(data, beat, level, seed, high=False):
    rng = random.Random(seed)
    first = round(beat * BEAT * RATE)
    count = round((.10 if high else .20) * RATE)
    low = 0.0
    phase = 0.0
    for i in range(count):
        t = i / RATE
        phase += TAU * (150 + 50 * math.exp(-t * 45) if high else 78 + 65 * math.exp(-t * 35)) / RATE
        white = rng.uniform(-1, 1)
        low += .11 * (white - low)
        envelope = min(1, t / .002) * math.exp(-t / (.018 if high else .045))
        tail = min(1, max(0, (count - 1 - i) / (RATE * .015)))
        signal = math.sin(phase) * (.18 if high else .66) + (white - low) * (.16 if high else .045)
        data[(first + i) % COUNT] += signal * level * envelope * tail


def compose():
    menu = [0.0] * COUNT
    # An integer-cycle open fifth gives a quiet continuous drone with an exact periodic boundary.
    for i in range(COUNT):
        t = i / RATE
        drone = .012 * math.sin(TAU * round(hz(38) * SECONDS) / SECONDS * t)
        drone += .008 * math.sin(TAU * round(hz(45) * SECONDS) / SECONDS * t)
        menu[i] = drone
    chords = [(50, 57, 62, 65), (48, 55, 60, 64), (43, 50, 59, 62), (50, 57, 62, 65),
              (53, 60, 65, 69), (48, 55, 60, 64), (45, 52, 57, 60), (50, 57, 62, 65)]
    melody = [(0, 74, 1), (1, 77, .5), (1.5, 76, .5), (2, 74, 1),
              (3, 72, 1.5), (4.5, 76, .5), (5, 79, 1),
              (6, 79, 1), (7, 77, .5), (7.5, 76, .5), (8, 74, 1),
              (9, 77, 1), (10, 76, 1), (11, 74, 1),
              (12, 77, 1.5), (13.5, 81, .5), (14, 79, 1),
              (15, 76, 1), (16, 74, .5), (16.5, 72, .5), (17, 76, 1),
              (18, 76, 1), (19, 72, 1), (20, 69, 1),
              (21, 74, 2), (23, 69, 1)]
    for bar, chord in enumerate(chords):
        for offset, index in [(0, 0), (.5, 2), (1, 1), (1.5, 3), (2, 2), (2.5, 1)]:
            pluck(menu, bar * 3 + offset, chord[index], .11 if index == 0 else .075, .85, .25)
    for beat, note, duration in melody:
        pluck(menu, beat, note, .125, duration * BEAT + .3, .23)
        if beat in [0, 6, 12, 21]:
            pluck(menu, beat + .02, note + 12, .016, 1.4, .40, BELL)
    play = menu.copy()
    for bar, chord in enumerate(chords):
        for beat in range(3):
            absolute = bar * 3 + beat
            drum(play, absolute, .17 if beat == 0 else .09, 40900 + absolute)
            drum(play, absolute + .5, .07, 41000 + absolute, high=True)
            pluck(play, absolute, chord[0] - 12 if beat == 0 else chord[1] - 12, .10, .5, .13)
        for offset, note in [(0, chord[2]), (.5, chord[1]), (1.5, chord[3]), (2.5, chord[1])]:
            pluck(play, bar * 3 + offset, note + 12, .031, .40, .10)
    # Use one gain for both arrangements, preserving their shared material while crossfading.
    gain = .62 / max(abs(value) for track in [menu, play] for value in track)
    return {'CourtyardMenu': [value * gain for value in menu],
            'CourtyardPlay': [value * gain for value in play]}


def write_tracks():
    DESTINATION.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix='brickkeep-music-') as temporary:
        for name, samples in compose().items():
            pcm = array('h', (round(max(-1, min(1, sample)) * 32767) for sample in samples))
            if sys.byteorder != 'little':
                pcm.byteswap()
            source = Path(temporary) / (name + '.wav')
            with wave.open(str(source), 'wb') as stream:
                stream.setnchannels(1)
                stream.setsampwidth(2)
                stream.setframerate(RATE)
                stream.writeframes(pcm.tobytes())
            target = DESTINATION / (name + '.ogg')
            subprocess.run(['ffmpeg', '-hide_banner', '-loglevel', 'error', '-y', '-i', str(source),
                            '-map_metadata', '-1', '-fflags', '+bitexact', '-flags:a', '+bitexact',
                            '-c:a', 'libvorbis', '-q:a', '6', '-serial_offset', '0', str(target)], check=True)
            print(f'{name}: {SECONDS:g}s / {COUNT} samples / mono {RATE}Hz / original OGG Vorbis')


if __name__ == '__main__':
    write_tracks()

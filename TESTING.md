# Validation report

This report records source and asset validation for the consolidated project. `Assets/_Project/Scenes/After.unity` is the final game and only Build Settings entry. `Before.unity` provides the original starter for editor comparison. The scene consolidation replaces the previous multi-scene test assumptions; earlier milestone results remain historical evidence.

## Current automated results

| Check | Scope | Recorded result |
|---|---|---|
| `tests/verify_project.py` | After build entry with Before comparison, gameplay/feedback wiring, singleton isolation, feedback lifecycle and score/trail isolation | 6 tests passed |
| `tests/verify_polish.py` | Unique asset IDs, metadata targets, scene/prefab references, final scene fields, materials and WAV payloads | 7 tests passed |
| C# syntax parsing | All 63 scripts under Assets | No syntax errors detected |
| Serialized-data parsing | 2 scenes, 6 prefabs, 22 materials, 1 mixer and 9 serialized assets | Parsing passed |
| Final scene preservation | Consolidated After scene content | Unchanged by the documentation/configuration cleanup |
| Historical shader-mask sampling | Procedural alpha masks and numeric bounds | Finite, bounded sample values |
| Historical text and layout calculations | Status-label glyph advances, court gutters and Settings clearance | Within calculated bounds at 1920×1080 and 1280×720 |
| Audio payload inspection | Eight PCM WAV files | Valid headers, formats and complete payloads |

The current automated total is **13 project/asset tests**. The earlier 19-test result applied before the intermediate scenes were removed. Syntax and numeric checks are additional analyses, not Unity compilation or gameplay execution. Audio payload inspection establishes file integrity, not listening quality.

## Runtime verification status

Unity and .NET were unavailable during the latest implementation pass.

| Area | Status |
|---|---|
| Unity import and C# semantic compilation | Not verified |
| URP shader compilation and final rendering | Not verified |
| Executable Arcade/Ability/Level rule harnesses | Not rerun in the latest pass |
| Mouse, keyboard and hotbar input | Not verified in Unity |
| Physics, victory/defeat, retry and progression | Not verified in Unity |
| Pause, resets, component re-enable and scene cleanup | Not verified in Unity |
| Sound balance, music transitions and device output | Not verified by listening |
| Build compatibility and performance | Not verified |
| Qualitative peer-playtest feedback | A friend suggested levels and difficulty selection; the implemented revision is documented in README.md |

The [demonstration video](https://youtu.be/BF2wYM0IkJU) accompanies the project.

## Level and difficulty matrix

All nine combinations are implemented. Their engine playthrough status is recorded below.

| Level | Easy | Medium | Hard |
|---|---|---|---|
| Courtyard | Not verified | Not verified | Not verified |
| Twin Towers | Not verified | Not verified | Not verified |
| Crown | Not verified | Not verified | Not verified |

## Technical considerations

- Before contains the original starter, and After contains the final game. Only After is included in Build Settings; intermediate versions remain in Git history.
- Rune Shield feedback observes the native line-width flash. Deflection detection during low-frame-rate execution remains an engine-validation item.
- Giant's Grip visuals derive the current world-space box from the collider's local shape and transform, avoiding a cached physics-bounds dependency with automatic transform syncing disabled.
- Status badges use calculated court gutters and hide when the viewport does not provide enough space. Final TMP and alpha rendering remain engine-dependent.
- Temporary world effects use scaled ages. Existing UI, TrailRenderer and pause/input interactions remain part of runtime verification.

## Reproducibility

The Python checks use the standard library and cover After plus the original Before comparison, with After as the only build scene. The three console rule harnesses target .NET 8. The display font uses Git LFS and is downloaded separately from its tracked pointer.

```sh
python tests/verify_project.py
python tests/verify_polish.py
dotnet run --project tests/ArcadeRoundChecks.csproj
dotnet run --project tests/AbilityChecks.csproj
dotnet run --project tests/LevelChecks.csproj
```

The Unity project targets **6000.3.2f1** with a 16:9 reference Game view. Source checks and console rules do not replace editor compilation or play-mode testing.

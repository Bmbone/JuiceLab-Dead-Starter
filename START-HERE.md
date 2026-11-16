# Project structure

After uses Unity 6000.3.2f1, C# and the Universal Render Pipeline 2D renderer. The Unity project root contains `Assets`, `Packages` and `ProjectSettings`.

## Entry point

[Assets/_Project/Scenes/After.unity](Assets/_Project/Scenes/After.unity) is the final polished game and sole Build Settings entry. It includes menus, levels, difficulty selection, spells, audio and visual feedback. [Before.unity](Assets/_Project/Scenes/Before.unity) is the restored original starter, kept separately for comparison in the Unity Editor. Intermediate development scenes remain in Git history.

## Modules

All module paths below are relative to `Assets/_Project`.

| Module | Responsibility |
|---|---|
| `Scripts/Core` | Starter events, game state, impact data and HUD ownership. |
| `Scripts/Gameplay` | Original ball, paddle, board, brick and death-zone behavior. |
| `Feedback` | Original five feedback components and their shared rendering assets. |
| `Arcade` | Independent difficulty selection and complete-round flow. |
| `Abilities` | Spell timers, expanded paddle, shield protection, projectile presentation and hotbar. |
| `Levels` | Layout catalog, level selection, progression, score/lives and final scene adapters. |
| `Presentation` | Medieval menu styling, display typography, button motion and animated score. |
| `StoneCourt` / `BreakEffects` / `MasonryParticles` | Stone surfaces, castle decoration, break effects and pooled debris. |
| `ImpactMotion` / `RelicGear` | Restrained shake, brick reactions, orb/paddle surfaces and contact feedback. |
| `CastleAudio` / `CastleMusic` | Event/result sounds, original audio assets and music transitions. |
| `CourtSettings` / `ChapterPresentation` | Pause/options, volume/motion preferences, chapter banners and result decoration. |
| `FireballVfx` / `RuneShieldVfx` / `GiantGripVfx` | Final ability visuals that observe existing gameplay state. |
| `Prefabs` / `Settings` / `Scenes` | Brick prefabs, rendering configuration and the final After scene and original Before comparison. |

## Runtime responsibilities

`LevelSession` coordinates the selected level, round state, board, menus, score and lives. `LevelAbilityController` owns spell execution and connects the ability rules to the scene. Feedback components respond to `GameEvents` or observe the existing ability presentation; they do not own the score.

Visual feedback uses separate renderers, child transforms, material property blocks and fixed pools. Event subscriptions are removed when components are disabled. Cosmetic random variation uses independent random streams.

The gameplay extensions use separate adapters and prefabs. The current branch retains the final After scene and the original Before comparison. Intermediate milestones remain available through Git history. The development history is documented in [DEVELOPMENT-HISTORY.md](DEVELOPMENT-HISTORY.md).

## Repository documentation

| File | Content |
|---|---|
| [README.md](README.md) | Project overview, demonstration, setup, controls, features and credits. |
| [TESTING.md](TESTING.md) | Recorded validation results, test scope and runtime verification status. |
| [DEVELOPMENT-HISTORY.md](DEVELOPMENT-HISTORY.md) | Implementation milestones and scene versions. |
| [SOURCE.md](SOURCE.md) | Starter provenance and licensing. |

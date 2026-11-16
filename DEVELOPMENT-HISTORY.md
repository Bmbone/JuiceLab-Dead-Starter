# Development history

After was developed through separate scene milestones and then consolidated into **Assets/_Project/Scenes/After.unity**, the only scene retained in the current branch. The scene names below describe historical Git revisions; they are not additional scene files in the current project.

## Gameplay foundation

Historical scene paths in both tables were relative to `Assets/_Project/Scenes`.

| Scene | Implementation |
|---|---|
| `Before.unity` | Original unpolished course starter. |
| `After.unity` | Brick particles, paddle squash, event audio, score pulse and ball trail, with the original gameplay rules. |
| `Arcade.unity` | Difficulty selection, complete-round state, victory/defeat screens and retry/menu flow. |
| `Abilities.unity` | Fireball, Giant's Grip, Rune Shield and the cooldown hotbar. |
| `Levels.unity` | Courtyard, Twin Towers and Crown layouts, each supporting all three difficulties. |
| `Medieval.unity` | Initial medieval presentation. |

## Presentation milestones

| Milestone | Scene | Main change |
|---:|---|---|
| 01 | `MedievalUI01_Score.unity` | Framed score plaque, color flash, upward pulse and reusable gain popup. |
| 02 | `MedievalUI02_RoyalInterface.unity` | Larger typography, horizontal level/difficulty cards, board previews and menu/HUD styling. |
| 03 | `Medieval03_StoneCourt.unity` | Procedural stone bricks, castle walls, pillars and courtyard decoration. |
| 04 | `Medieval04_BrickBreak.unity` | Break animation, recoil and impact score feedback. |
| 05 | `Medieval05_MasonryParticles.unity` | Layered masonry debris and pooled destruction particles. |
| 06 | `Medieval06_ImpactMotion.unity` | Restrained life-loss/impact shake and neighboring brick reactions. |
| 07 | `Medieval07_BallPaddle.unity` | Wood-and-brass paddle, enchanted orb, halo and contact feedback. |
| 08 | `Medieval08_CastleAudio.unity` | Event-specific impacts, life-loss and result cues. |
| 09 | `Medieval09_CastleMusic.unity` | Original menu/play music and round transitions. |
| 10 | `Medieval10_CourtSettings.unity` | Pause/settings, music/SFX sliders, motion switches and mixer routing. |
| 11 | `Medieval11_ChapterPresentation.unity` | Level/difficulty banners and decorated result presentation. |
| 12 | `Medieval12_FireballVFX.unity` | Flame core, halo, ribbon, cast ring and pooled embers. |
| 13 | `Medieval13_RuneShieldVFX.unity` | Runic ward, end anchors, state badge and deflection shards. |
| 14 | `Medieval14_GiantGripVFX.unity` | Expanded-paddle aura, gold runes, activation/expiry motes and status badge. |

## Implementation approach

The original feedback layer uses the starter's event hooks. Subsequent gameplay extensions use independent rules, adapters and prefabs. Later presentation passes add renderers, child transforms, material property blocks and reusable visual pools with earlier scene versions now retained in Git history.

The selected gameplay extension is level and difficulty selection. Courtyard, Twin Towers and Crown provide different formations, and Easy/Medium/Hard use speeds of 9/12/15 world units per second. This extends the game beyond the controlled Before/After feedback comparison.

## Validation history

Before scene consolidation, milestone 14 included 12 existing source checks and 7 additional asset/scene checks, all passing. After consolidation, the Python checks were updated to target only `After.unity`; 6 project checks and 7 asset/scene checks pass for the current structure. Source analysis also identified a potential dependency on stale physics bounds for the new paddle decoration; its implementation uses the current collider shape and transform instead.

Unity compilation, runtime behavior, device performance and recorded peer-playtest results are outside the completed source-validation scope. The detailed status is maintained in [TESTING.md](TESTING.md).

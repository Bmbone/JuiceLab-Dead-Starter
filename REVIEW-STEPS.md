# Review checkpoints

Each stage is pushed separately to main. Use GitHub commit history to inspect a stage; in Unity, stop Play before pulling and open the After scene.

## 1. Starter repairs

- Restored the actual EmojiOne PNG; the previous commit stored a Git LFS pointer.
- Store small game media as regular Git blobs so a fresh clone contains playable assets.
- Register After first in Build Settings, with Before still available.
- Center only After's camera and restore its Canvas scale. Physics objects and Before remain unchanged.
- Validation: seven source checks; Unity import/play mode still requires the editor.

## 2. HUD pointer fix

- Disabled Raycast Target only on After's score and lives labels. The paddle's existing UI-hover guard otherwise stops mouse following over these display-only labels.
- Review: stop Play, pull main, open `Assets/_Project/Scenes/After.unity`, and play in 16:9. Move the mouse horizontally over the score and lives labels; the paddle should keep following. Confirm both labels still update during scoring and ball loss.
- Validation: all seven source checks pass; Before and shared gameplay remain unchanged. Unity runtime verification is still pending.
- Next small checkpoint: medieval castle background. Later presentation work covers textures, UI, VFX, sound effects, and music; gameplay abilities, including the brick-piercing fireball, belong in a separate optional mode to preserve the assignment comparison.

# World Order phase plan

## Phase 1: playable RTS foundation, completed in this repository

Delivered systems:

- Main menu with Play, Settings and Credits.
- World list with empty state and persisted world index.
- World creation screen with name, seed, difficulty, map preset, enemy factions and ally factions.
- Loading screen with progress states.
- RTS game screen with preset procedural maps, roads, lakes, rocks and props.
- Unit selection, move orders, attack orders, Android tap controls and desktop controls.
- Player economy with supplies and tank production.
- Enemy AI with faction bases, spawned attack waves and target selection.
- Ally AI that assists against enemy forces.
- Tank and boat sprites, turret rotation, projectile simulation, explosion/muzzle effects, health bars and minimap.
- Windows and Android projects using MonoGame packages.
- GitHub Actions for Windows x64/win-arm64 zips and signed Android APK output named by commit hash.

## Phase 2: production RTS depth

- Add deterministic lockstep simulation boundaries so multiplayer can be introduced later without rewriting core logic.
- Replace steering-only movement with tile-aware A* or flow-field pathfinding.
- Add base construction, harvesters, refineries, repair pads and tech tree progression.
- Add fog of war, radar range, scouting and revealed-map persistence.
- Add proper unit command groups, queued commands, patrol and attack-move.
- Add balance data in external JSON so units can be tuned without recompiling.
- Add audio: UI sounds, engine loops, cannon fire, explosions and music.
- Add accessibility pass: scalable UI, touch size tuning, colorblind-friendly faction cues.

## Phase 3: campaign and world layer

- Add overworld campaign map, persistent faction control and generated operations.
- Add save files per campaign, mission results and unit veterancy.
- Add naval/land mixed maps with amphibious objectives.
- Add scripted tutorial missions and skirmish presets.
- Add localization-ready text tables.

## Phase 4: release hardening

- Automated smoke tests for map generation and simulation invariants.
- Crash logging and replay snapshots.
- Android device farm tests for input, resolution and APK compatibility.
- Replace default private keystore with production signing flow.
- Store-ready icons, screenshots, privacy policy and release notes.
- Performance pass for batching, texture atlases, memory pressure and Android thermal behavior.

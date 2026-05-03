# World Order phase plan

## Phase 1: playable RTS foundation, completed

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

## Phase 2: production RTS depth, completed in this patch

Delivered systems:

- Replaced the broken HUD bitmap font with a clean generated UI font atlas.
- Fixed Android touch release hit-testing so menu and HUD buttons register at the tapped position.
- Added tile-aware A* pathfinding with road movement preference and passability checks.
- Added spice resource fields to the procedural maps.
- Added Harvester units that automatically find resource fields, mine cargo and return it to command centers.
- Added a build button for harvesters and visible cargo bars.
- Added fog of war with visible/explored tile state.
- Hidden enemy units from world view and minimap when outside player/ally vision.
- Added resource indicators to the world and minimap.
- Updated UI copy, settings text and project documentation for Phase 2.

## Phase 3: command depth and base building

Recommended next systems:

- Add base construction placement, power, refineries, repair pads and tech structures.
- Add command groups, queued commands, patrol, guard and attack-move.
- Add deterministic simulation boundaries so multiplayer can be introduced later without rewriting core logic.
- Add unit/building balance data in external JSON so tuning does not require recompilation.
- Add audio: UI sounds, engine loops, cannon fire, explosions and music.
- Add accessibility pass: scalable UI, touch-size tuning and colorblind-friendly faction cues.

## Phase 4: campaign and world layer

- Add overworld campaign map, persistent faction control and generated operations.
- Add save files per campaign, mission results and unit veterancy.
- Add naval/land mixed maps with amphibious objectives.
- Add scripted tutorial missions and skirmish presets.
- Add localization-ready text tables.

## Phase 5: release hardening

- Automated smoke tests for map generation and simulation invariants.
- Crash logging and replay snapshots.
- Android device farm tests for input, resolution and APK compatibility.
- Replace default private keystore with production signing flow.
- Store-ready icons, screenshots, privacy policy and release notes.
- Performance pass for batching, texture atlases, memory pressure and Android thermal behavior.

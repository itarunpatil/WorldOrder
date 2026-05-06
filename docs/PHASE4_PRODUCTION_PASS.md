# Phase 4 Production Pass

This pass rebuilds the parts that were still making World Order feel rough despite compiling and running.

## World generation

The old generator mixed detailed atlas tiles directly into base terrain, which caused repeated road stripes, wall fragments, and rubble patterns to appear as if they were normal ground. Phase 4 replaces that with a parcel-based generator:

- roads and sidewalks are generated first from a deterministic city grid;
- city cells are split into parcels;
- each parcel chooses a stable role such as park, plaza, parking, ruined lot, industrial yard, wilderness, or buildable block;
- buildings are generated inside eligible parcels with door gaps and deterministic ruin damage;
- base terrain stays simple and readable while environmental detail is pushed into resource and decoration passes.

## UI and controls

- Added a hotbar-style inventory with selectable slots.
- Added desktop/touch crafting overlay.
- Added touch `INV` button.
- Kept gameplay touch controls outside the world-placement region so build placement remains explicit.

## Gameplay systems

- Added crafting recipes in `Gameplay/Definitions.cs`.
- Added build placement preview with green/red validity feedback.
- Zombie death now drops loot entities instead of directly editing inventory.
- Game-over freezes gameplay simulation and displays a death banner.

## Asset usage

- Corrected road tile selection to clean asphalt bases.
- Corrected building/wall tile selection to avoid using edge fragments as repeated floor.
- Added UI icon bindings for hotbar/crafting presentation.

## Notes for future agents

Do not reintroduce noisy tile-by-tile world mixing. The art pack contains many decorative tiles, but base terrain must be semantically clean. Use decoration nodes and renderer overlays for visual variety.

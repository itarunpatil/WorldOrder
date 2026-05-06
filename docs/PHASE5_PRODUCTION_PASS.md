# Phase 5 Production Pass

Phase 5 replaces the unstable open-world prototype generator with curated survival maps. The goal of this pass is to make the game coherent immediately: readable maps, meaningful routes, deliberate loot placement, cleaner menus, and inventory that reflects what the player actually owns.

## Major changes

- Replaced procedural city/noise generation with `WorldMapCatalog` and hand-authored map definitions.
- Added three selectable maps during world creation:
  - Overgrown Crossroads
  - Industrial Yard
  - Downtown Breakout
- Added map IDs to saves and load-screen display.
- New worlds now spawn at authored safe starting positions for the selected map.
- Zombie waves now use authored map spawn points instead of random off-screen placement.
- Resources, wrecks, containers, caches, benches, roadblocks, fences, doors, vents, bins, trees, and debris are placed intentionally per map.
- Fixed inventory/hotbar presentation so the hotbar draws the player's real item stacks and leaves empty slots empty.
- Switched hotbar to six slots to match the asset-pack quick-access style.
- Removed control-instruction text above the hotbar.
- Removed the mobile in-game name keyboard. Android now uses MonoGame platform keyboard input for naming.
- Re-centered the main menu title/subtitle and removed the asset-pack integration subtitle from the menu.
- Reworked the menu background into a subdued apocalyptic scene instead of random placeholder geometry.
- Added player punch/pistol attack sprite sheets from the pack so attack animation is no longer the idle/run sprite with a slash overlay.
- Cleaned terrain tile selection so roads and pavements no longer get random stripe/curb variants baked into base ground.

## Map architecture

The old generator is intentionally not deleted from history, but the runtime path now uses authored maps:

- `WorldMapCatalog` owns map metadata and all map construction.
- `WorldMapDefinition` owns the tile layer, resource placements, decoration placements, and zombie spawns.
- `WorldGenerator` is now a thin adapter from chunk requests to the selected authored map.
- `ChunkManager` still streams chunks, but chunks are slices of the authored map rather than infinite noise.

This keeps the code modular while stopping the visual chaos caused by random parcel/tile selection.

## Next recommended pass

- Add explicit story objectives per map.
- Add map completion/extraction points.
- Add weapon-specific combat rules and real ammunition UI.
- Add building sockets instead of free placement in curated maps.
- Add authored night events and map-specific zombie pressure.

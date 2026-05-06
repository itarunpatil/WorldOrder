# AGENTS.md - World Order engineering guide

## Product intent

World Order is a modular C# MonoGame post-apocalypse zombie survival game. It must remain a real playable game at every phase, not a throwaway prototype. Keep systems deterministic where possible, keep constants named, and avoid hardcoded one-off behavior hidden inside update loops.

## Non-negotiable constraints

1. Keep the supplied PostApocalypse art integrated under `GameAssets/PostApocalypse` for this private repo. Do not publish this repo or repackage the raw art publicly unless the asset license holder confirms that is allowed.
2. Keep Windows and Android projects compiling from shared source under `src/WorldOrder.Shared`.
3. Keep the Android package id as `com.world.order` unless the product owner explicitly changes it.
4. GitHub Actions must trigger on every push to every branch and produce commit-hash-named artifacts.
5. Any new gameplay system needs save/load consideration before it is merged.
6. Avoid magic numbers in gameplay logic. Put tunables in `Core/Balance.cs` or a clearly named definition table.
7. Keep rendering compatible with `SamplerState.PointClamp` and pixel art scaling.
8. Prefer deterministic systems over random runtime-only behavior. When randomness is needed, derive it from the world seed and named salts.

## Architecture

- `GameRoot` owns MonoGame services, input, art, UI, camera, and screen transitions.
- `Screens/` owns menus, loading, and the active play loop.
- `World/` owns deterministic generation, chunks, saves, placed blocks, and resource nodes.
- `Entities/` owns player, zombies, pickups, and entity lifecycle.
- `Gameplay/` owns inventory, definitions, buildable costs, and loot tables.
- `Rendering/` owns world and HUD drawing.
- `Assets/` owns PostApocalypse asset stream loading plus procedural safety fallback art.
- `UI/` owns the rectangle UI and 5x7 pixel font.

## Asset workflow

- Direct runtime art bindings live in `Assets/ArtLibrary.cs` and `tools/asset_manifest.txt`.
- Windows packaging is handled by `WorldOrder.Desktop.csproj` with `CopyToOutputDirectory`.
- Android packaging is handled by `WorldOrder.Android.csproj` with `AndroidAsset` items.
- The shared `AssetStorage` loader checks disk first, Android assets second, and embedded resources last.
- To refresh assets from the original zip, run `tools/import-post-apocalypse-assets.py <zip> --clean`.

## Development workflow

1. Run the desktop project first for fast iteration.
2. Add gameplay through small, testable classes in shared code.
3. Add new save fields with safe defaults so existing saves continue to load.
4. For new art references, add them to `tools/asset_manifest.txt` and `ArtLibrary.LoadPostApocalypsePack`.
5. Keep Android-specific code only in `src/WorldOrder.Android`; keep Desktop-specific code only in `src/WorldOrder.Desktop`.
6. For CI changes, test the command locally if possible and keep artifact names commit-hash-based.

## Current phase 4 systems

- Deterministic endless chunks with parcel-based city blocks, clean roads with renderer-owned markings, sidewalks, district types, parks, plazas, parking lots, abandoned/ruined buildings, wilderness pockets, resource rules, and decoration passes.
- Resource gathering and loot drops.
- Hotbar inventory, crafting recipes, and material costs.
- Build mode for walls, reinforced walls, floors, and campfires with placement preview.
- Player vitals and consumables.
- Zombies with day escalation, deterministic movement states, hurt/death states, knockback, damage numbers, and impact feedback.
- Save/load, autosave, and loading screen.
- Integrated PostApocalypse character, zombie, object, buildable, resource, tile, decoration, road prop, and building prop art.
- Procedural art remains only as a startup-safe fallback when an asset is missing.
- Android has immersive landscape mode, guarded touch hit-testing, soft-keyboard world-name entry, fallback touch keypad, and explicit gameplay touch UI.
- Desktop supports resizable windows, maximize, F11/Alt+Enter fullscreen toggling, and cursor-directed melee attacks.

## Next high-value phases

1. Add map/minimap with discovered-chunk persistence.
2. Add proper doors/gates and repair/upgrade interactions for placed blocks.
3. Add ranged weapon reload states and projectile simulation.
4. Add biome-specific hazards and weather.
5. Add crafting bench recipes and larger base-defense waves.
6. Add audio and music with settings.
7. Add automated compile smoke tests once the build runner has .NET installed.

## Quality bar

- No hidden prototype-only shortcuts.
- No public asset-pack redistribution from this private source tree.
- No broken saves after schema changes.
- No frame-wide iteration over unloaded world data.
- No platform-specific API usage in shared code unless guarded and documented.

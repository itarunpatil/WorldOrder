# World Order architecture

## Runtime flow

`GameRoot` initializes MonoGame, creates the art library, pixel font, UI renderer, input, camera, and screen manager. The main menu routes to world creation or loading. The loading screen creates or reads a `WorldState`, constructs a `WorldSession`, preloads chunks around spawn, saves the initial state, and enters `PlayScreen`.

`PlayScreen` updates the session unless paused. It then renders the chunked world with a camera transform and draws the HUD without the transform.

## World generation

The world is divided into 32x32-tile chunks. Tile generation is deterministic from world tile coordinates and the world seed. Phase 2 uses a seeded city-cell model: road corridors, sidewalks, parcel buildings, spawn-plaza safety, wilderness/water noise, resource nodes, and a separate decoration pass. Loaded chunks are kept inside a radius around the player and removed outside a larger keep radius. This keeps memory and draw costs bounded while preserving an endless world.

## Saving

Save files are JSON under the user's local app data folder. The save contains seed, time, day, player position, vitals, inventory, placed blocks, and depleted resource node ids. The world terrain itself is not stored; it is regenerated deterministically from the seed.

## Art pipeline

The game does not require the MonoGame content pipeline. Real PostApocalypse PNG art is integrated under `GameAssets/PostApocalypse` and loaded at runtime with `Texture2D.FromStream`. Phase 2 binds more ground variants, environmental props, road props, and building props. Procedural art exists only as a startup-safe fallback if a required PNG is missing.

Packaging paths:

- Windows: the desktop project copies PNG assets to the output folder.
- Android: the Android project packages PNG assets as Android assets.
- Shared code: `AssetStorage` checks disk, then Android assets, then embedded resources.

## CI

Windows and Android are separate workflows. Android uses the committed development keystore. Windows uses a committed development PFX and `signtool` on the Windows runner. Both artifact names include the short commit hash.

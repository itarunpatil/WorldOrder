# World Order

World Order is a C# MonoGame post-apocalypse zombie survival game. The repository is set up for Windows DesktopGL and Android builds, with separate GitHub Actions workflows that trigger on every push to every branch.

## Phase 4 playable feature set

- Main menu, world creation, save loading, settings/credits, pause menu, and loading screen.
- Endless deterministic open world with chunk streaming, a parcel-based city/wasteland generator, clean road surfaces, sidewalks, districts, parks, plazas, parking lots, abandoned/ruined buildings, safer spawn sanctuary, resources, and environmental decoration.
- Survival loop: health, hunger, thirst, stamina, infection pressure, day/night cycle, autosave, manual save.
- Zombies with deterministic state rules, sight tracking, attacks, escalation by day, soft cap, hurt flashes, knockback, death states, and chunk-distance cleanup.
- Gathering, salvage, pickups, drops, hotbar inventory, crafting recipes, food/water consumption, bandage healing, melee/firearm combat, damage numbers, hit sparks, slash effects, blood decals, and loot tables.
- Building mode with placement preview, wooden walls, reinforced walls, floors, and campfires with material costs.
- Integrated PostApocalypse pixel art from `GameAssets/PostApocalypse`, with procedural fallback only as a safety net.
- Android package name: `com.world.order`.
- Android immersive landscape rendering with strict touch hit-testing, smaller movement pad, touch buttons, and system keyboard/world-name fallback controls.
- Desktop windows are resizable/maximizable; press `F11` or `Alt+Enter` for fullscreen.

## Controls

- Move: `WASD` or arrow keys
- Sprint: `Left Shift`
- Gather/interact: `E` or right click
- Attack: `Space`, `F`, or left click toward cursor
- Build mode: `B`
- Crafting/inventory: `I` or `C`
- Select hotbar: `1`-`8` outside build mode
- Select buildable: `1`-`4`, or `Tab` while building
- Eat/drink: `Q`
- Heal: `H`
- Save: `R`
- Pause: `Esc`, then tap/click Save and Menu or press `M`
- Fullscreen/window toggle: `F11` or `Alt+Enter`

## Required SDKs

- .NET SDK 8.0 or later with `rollForward` enabled by `global.json`
- Android workload for Android builds: `dotnet workload install android`

## Local Windows run

```bash
dotnet restore src/WorldOrder.Desktop/WorldOrder.Desktop.csproj
dotnet run --project src/WorldOrder.Desktop/WorldOrder.Desktop.csproj
```

## Android touch controls

- Left side: movement pad
- ATK: attack
- GET: gather/interact
- BLD: toggle build mode
- EAT: consume food/water
- MED: use bandage
- INV: open crafting/inventory overlay
- II: pause

## Local Android publish

```bash
dotnet workload install android
dotnet publish src/WorldOrder.Android/WorldOrder.Android.csproj \
  -c Release \
  -f net8.0-android \
  -p:AndroidPackageFormat=apk \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=android/worldorder-dev.keystore \
  -p:AndroidSigningStorePass=worldorder \
  -p:AndroidSigningKeyAlias=worldorder \
  -p:AndroidSigningKeyPass=worldorder \
  -o artifacts/android
```

## Integrated asset pack

The supplied PostApocalypse pack is integrated into the private game repository under:

```text
GameAssets/PostApocalypse/
```

The source projects package those PNG assets automatically:

- Windows copies them to the publish output under `GameAssets/PostApocalypse`.
- Android packages them as Android assets and the shared loader opens them through the Android asset manager.

The pack's `LICENSE.txt` is kept in the same folder. Keep this repository private unless the asset license you hold permits public redistribution of the art inside source/build packages. The game code does not require MonoGame content pipeline processing; assets are loaded from PNG streams at runtime.

To refresh the integrated art from the original zip:

```bash
python tools/import-post-apocalypse-assets.py /path/to/PostApocalypse_AssetPack_v1.1.2.zip --clean
```

PowerShell:

```powershell
./tools/import-post-apocalypse-assets.ps1 -AssetZip C:\path\to\PostApocalypse_AssetPack_v1.1.2.zip -Clean
```

## CI artifacts

- `.github/workflows/build-windows.yml` publishes a Windows x64 single-file app, signs it with the repository development certificate, zips it, and names it `WorldOrder-<commit>-windows-x64.zip`.
- `.github/workflows/build-android.yml` publishes a signed Android APK using `android/worldorder-dev.keystore` and names it `WorldOrder-<commit>-android.apk`.

The bundled Android keystore and Windows PFX are development credentials for this private repository. Replace them before public distribution.

## Repository layout

```text
src/WorldOrder.Shared/   Cross-platform game code
src/WorldOrder.Desktop/  Windows/DesktopGL host
src/WorldOrder.Android/  Android host
GameAssets/PostApocalypse/ Integrated source art from the supplied pack
.github/workflows/       CI builds
tools/                   Asset refresh helper and manifest
docs/                    Architecture and asset notes
android/                 Default Android dev keystore
windows/                 Default Windows dev code-signing certificate
```


## Phase 3 production-pass notes

Phase 3 focuses on control correctness and world readability:

- Menu buttons now activate only on explicit button taps/clicks or keyboard confirm. A random click elsewhere no longer triggers the selected menu item.
- Text entry no longer treats Backspace as screen-back while the world name field is active.
- Android world-name entry opens MonoGame's platform keyboard and also includes a built-in touch fallback keypad.
- Desktop attack is now directed by the mouse cursor. Left-click attacks; right-click gathers/interacts.
- Build mode consumes the pointer for block placement instead of accidentally attacking.
- The road/tile atlas selection was corrected so road lane paint is drawn intentionally instead of every road tile repeating striping art.
- Zombie spawning has a short new-world grace period and starts farther away.


## Phase 4 production-pass notes

Phase 4 focuses on making the game feel like a coherent survival game instead of a tile stress-test:

- World generation is now parcel-based: roads, sidewalks, lots, parks, plazas, parking lots, ruined lots, industrial yards, and buildings are generated from deterministic city parcels instead of noisy per-tile patchwork.
- Road and building tile atlas usage was corrected again: base road tiles are clean, lane/crosswalk paint is drawn by the renderer, and decorative rubble is no longer used as random ground everywhere.
- The HUD now uses a proper quickbar layout with selectable hotbar slots and item icons.
- Crafting/inventory overlay is available on desktop and Android through `I`, `C`, or the touch `INV` button.
- Zombie death now drops loot into the world instead of silently adding it to inventory.
- Build mode has a green/red placement preview so the player can understand range and blockage before spending materials.
- Game-over state now freezes simulation updates and shows a clear death banner.

## Phase 5 production pass

- Runtime world generation now uses hand-authored selectable maps from `WorldMapCatalog` instead of the earlier noisy open-world generator.
- Keep new maps authored, named, and reviewed; do not reintroduce random tile-noise as the primary gameplay map path.
- Inventory/hotbar UI must show real player-owned item stacks; empty slots stay empty.
- Android world naming must use platform keyboard input, not an in-game keyboard.
- Main menu must remain centered, clean, and free from asset-debug/status text.

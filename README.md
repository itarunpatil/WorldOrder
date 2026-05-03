# World Order

World Order is a Phase 2 top-down 2D RTS built with C# and MonoGame. The goal of this repository is to be a real playable foundation for a long-running RTS project: menus, world creation, loading, preset map generation, unit selection, touch-safe orders, faction AI, A* pathfinding, fog of war, harvesters, resource fields, combat, projectiles, explosions, minimap, economy and Windows/Android build automation are included.

## Phase 2 gameplay

Start the game, select **Play**, create a world, choose map preset/difficulty/enemy factions/allies, wait for the loading screen, then enter the tactical map.

Current playable loop:

- Player command center generates baseline supplies.
- Spice fields now spawn on the map; harvesters mine them automatically and return cargo to command centers.
- Build light tanks, heavy tanks and harvesters from the bottom command bar.
- Select units with left drag on Windows, then right-click to move or attack.
- On Android, tap a player unit to select, then tap ground to move or tap an enemy to attack.
- Enemy factions spawn tanks from their command centers and use pathing to attack player/ally forces.
- Ally factions patrol and attack enemies.
- Fog of war hides unexplored terrain and enemy units outside vision.
- Win by destroying all enemy command centers.
- Lose if your command center is destroyed.

## Controls

Desktop:

- Left drag: select units.
- Right click: move selected units or attack an enemy.
- WASD / arrow keys: pan camera.
- Mouse wheel: zoom.
- Escape: return to the world list.
- F11: toggle fullscreen.

Android:

- Tap a player unit or button to select/activate it.
- Tap destination to move selected units. Android touch release coordinates are preserved so menu and HUD buttons register reliably.
- Tap an enemy to order selected units to attack.
- Drag/tap near screen edges to pan.

## Repository layout

```text
Content/                       Raw PNG assets loaded at runtime.
src/WorldOrder.Shared/         Cross-platform game code.
src/WorldOrder.Desktop/        Windows DesktopGL MonoGame launcher.
src/WorldOrder.Android/        Android MonoGame activity/project.
build/signing/                 Default private-repo Android keystore.
.github/workflows/             Windows and Android CI build pipelines.
docs/                          Asset review sheets and phase plan.
```

## Build locally

Install the .NET 8 SDK. For Android, install the Android workload:

```bash
dotnet workload install android
```

Run Windows desktop development build:

```bash
dotnet run --project src/WorldOrder.Desktop/WorldOrder.Desktop.csproj
```

Publish Windows x64:

```bash
dotnet publish src/WorldOrder.Desktop/WorldOrder.Desktop.csproj -c Release -r win-x64 --self-contained true -o artifacts/windows/win-x64 /p:PublishSingleFile=true
```

Publish a signed Android APK. The Android project declares RuntimeIdentifiers for android-arm, android-arm64, android-x86 and android-x64 in the csproj:

```bash
dotnet publish src/WorldOrder.Android/WorldOrder.Android.csproj \
  -f net8.0-android34.0 \
  -c Release \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=build/signing/worldorder.keystore \
  -p:AndroidSigningKeyAlias=worldorder \
  -p:AndroidSigningKeyPass=worldorder \
  -p:AndroidSigningStorePass=worldorder \
  -p:AndroidPackageFormats=apk
```

## GitHub Actions outputs

The workflows create downloadable artifacts named with the commit hash:

- `WorldOrder-<commit>-win-x64.zip`
- `WorldOrder-<commit>-win-arm64.zip`
- `WorldOrder-<commit>-android-universal-signed.apk`

The Android APK is signed with `build/signing/worldorder.keystore`. The default credentials are intentionally in the repo for this private-repo Phase 1 workflow:

- alias: `worldorder`
- store password: `worldorder`
- key password: `worldorder`

Replace this keystore before any public or store release.

## Asset credits

Assets are redistributed as game content for this private development build. Keep these credits visible in any public distribution.

- Tank sprites, tank weapons, effects, boats and RPG desert objects: CraftPix free asset packs. License file in source packs points to CraftPix file licenses.
- Free Desert Top-Down Tileset: Franco Giachetti / LudicArts.com, Creative Commons Attribution 3.0 International.
- Bitmap UI font atlas generated as a raster PNG from a system font for this project. No font files are included.

## Production notes

This Phase 2 still avoids MonoGame Content Builder/XNB dependencies by loading raw PNGs with `Texture2D.FromStream`. That keeps the build simple for CI and Android asset packaging. Later phases can move large assets into a formal content pipeline once animations, audio, localization and streaming are finalized.

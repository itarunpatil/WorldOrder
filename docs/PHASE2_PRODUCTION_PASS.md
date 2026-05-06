# Phase 2 production pass

This pass fixes the issues found after the first playable build and pushes the game toward a more robust survival loop.

## Fixed build issue

- Added `using WorldOrder.Gameplay;` to `WorldSaveSystem.cs` so `Inventory.CreateStarter()` resolves on Android and Desktop builds.

## World generation rewrite

- Replaced the first generator's visually noisy ground selection with a seeded city-cell model.
- Added a safe spawn plaza so the player starts in a readable area instead of inside repeated rubble/building tiles.
- Roads, sidewalks, abandoned parcel buildings, wilderness pockets, water pockets, resources, and decorations are produced from deterministic world coordinates and seed salts.
- Moved garbage/debris art out of the ground layer and into decoration/resource layers to stop object tiles from repeating as terrain.
- Added deterministic tile variants so large areas no longer repeat one bad crop.

## Android pass

- Added explicit touch support to menu buttons and gameplay controls.
- Added an immersive fullscreen landscape activity setup.
- Added touch gameplay buttons: move, attack, gather, build, eat, heal, pause.
- Create/load screens now have tappable actions so Android can start a world without a keyboard.

## Desktop pass

- Desktop window can now be resized/maximized.
- Fullscreen toggles with `F11` or `Alt+Enter`.

## Combat and feedback

- Zombie damage now shows hit sparks, floating damage numbers, hurt tint, knockback, death state, and blood decals.
- Player attacks now produce visible slash effects and misses show feedback.
- Gathering now shows dust/loot feedback and resource durability bars.
- Zombies show health bars after being damaged.

## Asset usage expanded

Additional bound assets include grass tufts, bushes, tires, cardboard, garbage bins, hydrants, manholes, benches, containers, vents, doors, destroyed walls, brick debris, roof holes, and fence/gate art.

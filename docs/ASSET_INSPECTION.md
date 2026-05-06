# PostApocalypse asset pack inspection

Uploaded file inspected: `PostApocalypse_AssetPack_v1.1.2.zip`

## Summary

- Total files integrated: 1,387
- PNG files: 1,113
- GIF files: 273
- Text/license files: 1
- Main categories found: `Character`, `Enemies`, `Objects`, `Tiles`, `UI`

## License handling

`LICENSE.txt` is included in `GameAssets/PostApocalypse`. The private repo contains the supplied art so the game and CI builds package the real visuals. Keep the repository private unless the asset license holder confirms public redistribution of source/build packages containing the art is allowed.

## Runtime mappings currently bound in code

| Runtime use | Asset path | Dimensions |
|---|---:|---:|
| Player idle down | `Character/Main/Idle/Character_down_idle-Sheet6.png` | 78x16 |
| Player run down | `Character/Main/Run/Character_down_run-Sheet6.png` | 78x17 |
| Player idle side | `Character/Main/Idle/Character_side_idle-Sheet6.png` | 72x16 |
| Player run side | `Character/Main/Run/Character_side_run-Sheet6.png` | 84x17 |
| Player idle up | `Character/Main/Idle/Character_up_idle-Sheet6.png` | 66x16 |
| Player run up | `Character/Main/Run/Character_up_run-Sheet6.png` | 78x17 |
| Zombie small walk down | `Enemies/Zombie_Small/Zombie_Small_Down_walk-Sheet6.png` | 72x16 |
| Zombie small walk side | `Enemies/Zombie_Small/Zombie_Small_Side_Walk-Sheet6.png` | 78x15 |
| Zombie small walk up | `Enemies/Zombie_Small/Zombie_Small_Up_Walk-Sheet6.png` | 78x16 |
| Canned food | `Objects/Pickable/Canned-food.png` | 7x7 |
| Canned soup / water stand-in | `Objects/Pickable/Canned-soup.png` | 5x7 |
| Bandage | `Objects/Pickable/Bandage.png` | 5x7 |
| Pistol | `Objects/Pickable/Pistol.png` | 8x7 |
| Ammo crate / generic cache | `Objects/Pickable/Ammo-crate_Green.png` | 11x10 |
| Scrap pickup | `Objects/Vehicles/Rust/Car_6_Rust_Scrap/Car_6_Rust_Blue_Scrap.png` | 23x26 |
| Wooden wall | `Objects/Buildable/Wooden/Wooden-wall_Horizontal.png` | 16x14 |
| Reinforced wall | `Objects/Buildable/Reinforced/Reinforced_wooden-wall_Horizontal.png` | 16x14 |
| Tree | `Objects/Nature/Green/Tree_1_Spruce_Green.png` | 16x29 |
| Rust car | `Objects/Vehicles/Rust/Car_1_Rust/Car_1_Rust_Red.png` | 25x37 |
| Rust barrel | `Objects/Barrel_rust_red_1.png` | 12x16 |
| Grass/dry ground/asphalt/pavement | `Tiles/Background_*_TileSet.png` crops | 16x16 |
| Rubble | Procedural cracked-rubble safety tile; garbage/debris art now renders as decorations instead of repeated ground | 16x16 |
| Building floor | `Tiles/Background_Green_TileSet.png` concrete/floor crops | 16x16 |
| Building wall | `Tiles/Brick-Wall_TileSet.png` crop | 16x16 |

## Visual inspection notes

The inspected contact sheets show coherent top-down pixel art across player, zombie, post-apocalypse tiles, vehicles, nature props, buildable walls, and UI inventory icons. Phase 2 binds animated player/zombies, safer tile crops, salvage resources, pickups, buildables, grass/bush props, tires, cardboard, bins, hydrants, manholes, benches, containers, vents, doors, destroyed walls, brick debris, roof holes, and fence/gate art. Remaining integrated art is available for future systems such as UI skins, weapon overlays, gates, vehicles, and building interiors.

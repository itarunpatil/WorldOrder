# Phase 3 Production Pass

## Goals

Phase 3 addresses the control, touch, menu, name-entry, and world readability problems discovered after Phase 2 runtime testing.

## Input and UI fixes

- Removed mouse/touch from the global confirm action. Menus now require an explicit hit inside a button rectangle, or keyboard confirm with Enter/Space.
- Reset input edges while the game is inactive so title-bar/minimize/maximize clicks do not leak into the game as menu actions.
- World creation no longer uses the generic cancel action while the text box is active. Backspace edits text; Escape unfocuses first, then backs out.
- Android name input now requests MonoGame `KeyboardInput.Show` and includes a custom touch keypad fallback for devices where the platform keyboard is unavailable.
- Main menu, save list, settings, and credits screens use explicit back buttons instead of arbitrary click-anywhere navigation.

## Gameplay control fixes

- Desktop left-click now attacks toward the mouse cursor.
- Desktop right-click gathers/interacts with nearby resource nodes.
- Build mode consumes left-click for placement and prevents accidental attacks while building.
- Android movement pad hit area is no longer the whole left half of the screen; it is now centered on the visible joystick region.
- Touch buttons retain explicit hit rectangles for attack, gather, build, eat, heal, and pause.

## World/visual pass

- The road atlas mapping was corrected. Asphalt ground now uses plain road tiles and renderer-owned lane/crosswalk paint so roads do not become full-screen repeated stripe tiles.
- Building floor atlas mapping was changed away from noisy vent tiles toward clean interior tiles.
- Rubble uses the garbage tile set intentionally instead of procedural placeholder art.
- The generator now separates districts into city, ruins, industrial, and wild zones with lower decoration spam and cleaner spawn surroundings.
- Resource and decoration rendering now picks deterministic texture variants from the integrated asset pack: multiple trees, vehicles, barrels, benches, containers, bins, doors, vents, debris, and roof holes.

## Difficulty/readability changes

- New worlds get a short grace period before zombie spawning.
- Zombies spawn farther from the player and the initial soft cap is lower.
- This keeps the first minutes focused on learning movement, gathering, and shelter instead of immediate death loops.

## Follow-up priorities

- Add a real settings screen with remappable controls and touch-size sliders.
- Add door/gate placement and repair interactions.
- Add minimap/discovered chunks.
- Add audio, weapon reload states, and base-defense events.

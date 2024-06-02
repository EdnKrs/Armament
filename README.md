# Comprehensive Mod Pack for GTFO

## Overview
This mod pack utilizes **BepInEx** and **Harmony** to introduce gameplay enhancements, graphical improvements, and new features to GTFO.

### BepInEx
- A plugin framework for Unity games built with C#/Mono scripting.
- Injects code (plugins) into the game.

### Harmony
- A patching library working alongside BepInEx.
- Allows targeting specific parts of the base game assembly for custom code injection.

## Combat Mechanics
- Introduced **Armor Penetration** and **Fragmentation** mechanics for manned and sentry weapon systems.
- Adjusted weapon shooting trajectories based on muzzle orientation for enhanced realism.

## Weapon Models and Mechanics
- Changed weapon models to more realistic versions.
- Enhanced reloading mechanic: retains one bullet in the chamber for closed-bolt weapons when reloading before emptying the magazine.

## Quality of Life Features
- Implemented **resource sharing** between team members.
- Added a mechanic to prevent ammunition loss during reloads.
- Balanced ammunition distribution to mitigate ammunition loss for players with unbalanced weapon usage.

## Bug Fixes and Enhancements
- Fixed sentry bug to correctly eject shell casings and display muzzle flashes.
- Corrected functionality for sentries with piercing traits.
- Extended the lifetime of spent casing and bullet hole decals for greater immersion.

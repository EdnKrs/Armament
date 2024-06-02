This is a comprehensive mod pack for GTFO. 
Utilizing BepInEx (Bepis Injector eXtendable) - a plugin framework for Unity games built with C#/Mono scripting. It essentially injects code (plugins) into the game, enabling gameplay, graphics modifications, or new features introductions.
Utilizing Harmony - a patching library that works in conjunction with BepInEx, allowing specific parts of the base game assembly targeting, and inject custom code at these points.
Combat Mechanics:
Introduced Armor Penetration and Fragmentation mechanics to the manned and sentry weapon systems, features not present in the base game.
Adjusted weapon shooting trajectories to be based on the muzzle orientation rather than being normalized to the center of the screen, enhancing realism.
Weapon Models and Mechanics:
Changed weapon models to more realistic versions.
Enhanced the reloading mechanic: for closed-bolt weapons, retained one bullet in the chamber when reloading before emptying the magazine, a feature absent in the base game.
Quality of Life Features:
Implemented resource sharing between team members to reduce wasteful resource usage.
Added a mechanic to prevent ammunition loss: if weapons are not reloaded before receiving ammunition, the remaining rounds (MagazineSize - CurrentAmmoInMag) are added to the ReserveAmmo.
Balanced ammunition distribution: if a player with an unbalanced weapon usage (e.g., 3%/95%) receives an AmmoPack, the ammunition transfer helps mitigate ammunition loss.
Bug Fixes and Enhancements:
Fixed the base game’s sentry bug to eject the correct shell casing models and display appropriate muzzle flashes.
Corrected a bug where sentries with piercing traits (bullets capable of piercing multiple enemies) did not function due to a development oversight. Implemented the missing code to enable this feature.
Extended the lifetime of spent casing and bullet hole decals in the game world for greater immersion.

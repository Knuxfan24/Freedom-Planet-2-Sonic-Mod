# Freedom Planet 2 Sonic Mod

![](./readme_imgs/main.png)

A mod for Freedom Planet 2 (using [FP2Lib](https://github.com/Kuborros/FP2Lib)) that adds Sonic The Hedgehog as a playable character.

## Features

### Moveset

#### Spin Jump

![](./readme_imgs/spin_jump.png)

A replacement for the default jump, causing Sonic to curl into a ball that can cause weak, but consistent damage to enemies.

#### Double Jump

![](./readme_imgs/double_jump.png)

Press the jump button again while Spin Jumping to Double Jump, gaining extra height at the cost of uncurling from the spin.

#### Roll

![](./readme_imgs/roll.png)

Hold down while running to roll.

#### Spin Dash

![](./readme_imgs/spin_dash.png)

While on the ground, hold down and press the jump button to begin charging a Spin Dash. Repeatedly tap the button to increase the power of the Spin Dash, then release down to blast off at high speed.

#### Homing Attack

![](./readme_imgs/homing_attack.png)

While airborne, a lock-on cursor will appear on the closest enemy or Item Box. Press the attack button to launch a Homing Attack which zeroes in on the target's position.

#### Sweep Kick/Windmill
![](./readme_imgs/sweep_kick.png)
![](./readme_imgs/windmill.png)

Press the attack button while on the ground to perform a Sweep Kick attack. If running or rolling, then the Sweep Kick is replaced with the faster Windmill attack.

#### Sonic Updraft

![](./readme_imgs/sonic_updraft.png)

Hold up and press attack to perform a Sonic Updraft. If used while airborne without having used a Double Jump, then this attack will also give a tiny bit of upwards momentum.

#### Sonic Rocket

![](./readme_imgs/sonic_rocket.png)

Hold down and press attack while airborne to perform a Sonic Rocket, a small downwards jab which also limits vertical momentum and can prolong air time.

#### Humming Top

![](./readme_imgs/humming_top.png)

While bouncing off a spring, hold left or right and press the attack button to fire a Humming Top for a quick burst of speed.

#### Hop Jump

![](./readme_imgs/hop_jump.png)

While bouncing off a spring, hold up and press the attack button to perform a Hop Jump for a bit of extra height.

#### Stomp

![](./readme_imgs/stomp.png)

While airborne, press the special button to perform a Stomp that sends Sonic down to the ground, damagining enemies along the way. Holding the special button upon landing will cause a Stomp Bounce, gaining extra height.

#### Air Dash

![](./readme_imgs/air_dash.png)

While in the air from a Spin Jump, double tap either left or right to Air Dash in that direction.

#### Anti-Gravity

![](./readme_imgs/slide.png)

Press the special button while running to perform a Slide, fixing Sonic's speed for three seconds and allowing him to turn on the spot while also damaging enemies.

### Wisps

Certain stages are impacted by Sonic's inability to swim. To compensate, three types of Wisps are included and placed at certain positions in specific stages.

#### Rocket

![](./readme_imgs/rocket_wisp.png)

Turns Sonic into the Orange Rocket, launching him upwards until the energy gague is depleted.

#### Drill

![](./readme_imgs/drill_wisp.png)

Turns Sonic into the Yellow Drill, allowing free movement underwater until the energy gague is depleted or the water is left. Hitting a wall will bounce the Drill Wisp off of it.

#### Laser

![](./readme_imgs/laser_wisp.png)

Turns Sonic into the Cyan Laser, allowing the player to aim and launch by pressing any action button. The Laser Wisp will bounce off of any surface, continuing to move forward until the energy gauge is depleted.

### Stages

#### Custom Tutorial

![](./readme_imgs/tutorial.png)

A custom stage based on the theming of Green Hill Zone, with Omochao NPCs describing Sonic's various moves.

#### Green Hill Zone

![](./readme_imgs/green_hill.png)

A recreation of the original Green Hill Zone Act 1, unlocked exclusively for Sonic after completing Weapon's Core.

### Items

#### Power Sneakers

![](./readme_imgs/power_sneakers.png)

Sonic's personal power up item, granting increased speed and acceleration for roughly 18 seconds.

#### Chaos Emeralds

![](./readme_imgs/chaos_emeralds.png)
![](./readme_imgs/super_sonic.png)
![](./readme_imgs/super_milla.png)

An item unlocked for any character after completing Weapon's Core. Gathering 50 Crystal Shards then performing a guard will enable a Super Form, granting increased speed, jump height and invincibility while draining a Crystal Shard every second.

Sonic transforms into Super Sonic when using the Chaos Emeralds and can detransform at will by guarding.

### Achievements

![](./readme_imgs/achievements.png)

Sonic adds eight new achievements, seven of which are exclusive to him and one that can be unlocked by any character. These achievements are:

- Blue Blur: Clear the game as Sonic.
- Greased Lightning: Beat any stage's part time as Sonic.
- Sonic Boom: Beat any stage as Sonic in less than half of the par time.
- Fastest Thing Alive: Beat the par times in all stages as Sonic.
- The Servers Are...: Clear Weapon's Core and obtain the Chaos Emeralds. (this achievement can be unlocked by any character)
- Problem Solved, Story Over: Defeat Merga in Palace Courtyard as Super Sonic.
- Oversaturated: Beat Gravity Bubble as Sonic without using any Wisps.
- Home Sweet Home: Unlock and complete Green Hill Zone.

### Vinyls

Sonic adds thirteen new Vinyls for purchase in the Vinyl Shop, these tracks being:

- Power Sneakers: The jingle used when Sonic's Power Sneakers item is active, taken from SONIC THE HEDGEHOG (2006).
- Stage Clear - Sonic: The jingle used when clearing a stage as Sonic, taken from SONIC THE HEDGEHOG (2006).
- Results - Sonic: The theme used during the results screen for Sonic, taken from SONIC THE HEDGEHOG (2006).
- His World (Sonic's Theme): The theme used during the credits for Sonic, taken from SONIC THE HEDGEHOG (2006).
- Super Sonic: The theme used when transforming into Super Sonic, taken from Super Smash Brothers Brawl.
- Drowning: The jingle used when the oxygen timer starts flashing while underwater, taken from Sonic Colours.
- Colour Power - Orange Rocket: The jingle used when activating the Rocket Wisp, taken from Sonic Colours.
- Color Power - Yellow Drill (Submarine Ver.): The jingle used when activating the Drill Wisp, taken from Sonic Lost.
- Color Power - Cyan Laser: The jingle used when activating the Laser Wisp, taken from Sonic Lost.
- Map - Green Hill: The theme used when highlighting Green Hill Zone on the map, taken from Sonic Generations.
- Green Hill Zone: The theme used in Sonic's tutorial and Green Hill Zone, taken from Sonic The Hedgehog (1991).
- Stage Clear - Green Hill: The jingle used when clearing Green Hill Zone, taken from Sonic The Hedgehog (1991).
- Super Form: The theme used when any character other than Sonic activates their Super Form, taken from Sonic Superstars.

## Building

First off, ensure that your system has a modern version of [Visual Studio](https://visualstudio.microsoft.com/) installed alongside the `.NET Framework 3.5 development tools`, as well as [Unity 5.6.3](https://unity.com/releases/editor/whats-new/5.6.3#installs) and [FP2Lib](https://github.com/Kuborros/FP2Lib) (at least Version 0.6.1. The [Freedom Manager](https://github.com/Kuborros/FreedomManager) program should install this automatically if used.).

Open the solution file in Visual Studio then go to `Tools > Options` and select `Package Sources` under the `NuGet Package Manager` category. Then add a package source called `BepInEx` with the source url set to `https://nuget.bepinex.dev/v3/index.json`.

Next, go to the `Assemblies` category in the `Dependencies` for the project, then delete the `Assembly-CSharp` and `FP2Lib` references. Right click on the Assemblies category and click `Add Assembly Reference...`, then click `Browse...` and navigate to Freedom Planet 2's install directory. Open the `FP2_Data` directory, then the `Managed` directory and select the `Assembly-CSharp.dll` file. Click Add, then Browse again and navigate to the location that FP2Lib's DLL is installed to (likely `BepInEx\Plugins\lib`) and select the `FP2Lib.dll` file. Click Add, then click OK.

You should now be able to right click the solution and choose `Rebuild` to build the mod. Though it is recommended to change the build configuration from `Debug` to `Release`, as the debug build prints a lot of console messages that are useless to the average player.

## Installing

Navigate to `BepInEx/plugins` and create a new folder with whatever name you want. Then copy the `FP2_Sonic_Mod.dll` file from the build (`bin/Debug/net35` or `bin/Release/net35`) into it. If using Freedom Manager, you may also want to copy the included `modinfo.json` file to give the mod a proper entry in the manager, although this is not strictly required.

## Customisation

![](./readme_imgs/config.png)

This mod has two customisable options (those being Sonic's voice and jump sound effect). To customise these, go to the `BepInEx/config` and open `K24_FP2_Sonic.cfg` in a text editor. What value corresponds to what is listed within the configuration file itself.

## Asset Credits

Sonic Sprites - Sonic Advance 1, 2, 3 and Battle, ripped by [AshuraMoon](https://www.spriters-resource.com/game_boy_advance/sonicadv/sheet/6583/), [Ice](https://www.spriters-resource.com/game_boy_advance/sonicadv2/sheet/154243/), [Ren "Foxx" Ramos](https://www.spriters-resource.com/game_boy_advance/sonicadv3/sheet/7143/) and [QuadFactor](https://www.spriters-resource.com/game_boy_advance/sonicbattle/sheet/10039/).

Bakunawa Chase Tornado Sprites - Sonic Advance, ripped by [Daniel Sidney](https://www.spriters-resource.com/game_boy_advance/sonicadv/sheet/7090/).

Custom Super Sonic Sprites - Ripped by Kevin Huff, edited by [Moe](https://www.spriters-resource.com/custom_edited/sonicthehedgehogcustoms/sheet/113731/). Sonic Battle style sprites by [Joe T.E.](https://www.spriters-resource.com/custom_edited/sonicthehedgehogcustoms/asset/18231/). 

Character Select Portrait - Sonic Advance 3, ripped by [HXC](https://www.spriters-resource.com/game_boy_advance/sonicadv3/sheet/7151/).

File Icon - Sonic Advance 3, ripped by [EternalLight](https://www.spriters-resource.com/game_boy_advance/sonicadv3/sheet/7156/).

Life Icon - Sonic Advance, ripped by [EternalLight](https://www.spriters-resource.com/game_boy_advance/sonicadv/sheet/6600/).

Power Sneakers Icon - Sonic Advance, ripped by [Spitfya](https://www.spriters-resource.com/game_boy_advance/sonicadv/sheet/234248/).

Wisp Capsules - Sonic Colours DS, ripped by [Trish Rowdy](https://www.spriters-resource.com/ds_dsi/soniccolors/sheet/35327/).

Rocket Wisp and Laser Wisp - Sonic Colours DS, screencapped and edited by [HogeezHoagies](https://www.youtube.com/@HogeezHoagies).

Homing Attack Cursor - Sonic 4 Episode 2, ripped by [thegameexplorer](https://www.textures-resource.com/pc_computer/sonic4ep2/texture/3865/).

Tutorial Omochao - Sonic Advance 3, ripped by [Gussprint](https://www.spriters-resource.com/game_boy_advance/sonicadv3/asset/27437/).
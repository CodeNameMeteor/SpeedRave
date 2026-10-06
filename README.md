# SpeedRave
 A Speedrun Mod for sewer rave

# Features
 * Trainer including Room Picker. Press ``insert`` in game (a controller button can be added as an alt bind). Includes:
   * Room selector and next/previous room hotkeys (``J`` / ``K``)
   * Room Lock: every door leads back to the current room (``L``)
   * Store / restore position (``Z`` / ``X``)
   * Add / remove cheese and fruit (``U`` / ``I`` / ``O`` / ``P``)
   * A Config UI for every setting below. Changes are saved when you close the trainer or press Save.
 * Autosplitter. Start TCP Server in Livesplit (LiveSplit Server on its default port, 16834). The mod only connects to LiveSplit on this computer (127.0.0.1), so a firewall only needs to allow local connections on that port.
   * Starts the timer when a run starts, pauses game time during room loads (loadless), and splits on the ending.
   * Optional splits: 20 resources, 20 fruit, key, each item.
   * Categories: by default the run ends at the first ending (Plaguending, Spaceending or Truending). Turn on **All Endings** to keep going until all three are reached.
   * Optional auto-reconnect if LiveSplit is restarted.
 * On-Screen Timer (Disabled By Default): a loadless timer in the top-right corner, matching LiveSplit's game time. Optionally shows LOADING / FINISHED text under it.
 * Inventory Overlay (Disabled By Default): cheese, fruit and collected items in the bottom-left corner. Icon mode needs ``cheese.png`` and ``fruit.png`` in ``BepInEx/CustomTextures``; without them it falls back to text.
 * Modified Seed Generation Allowing For Set Seed Runs (Thanks To <a href="https://github.com/Som1Lse">Som1Lse</a>). Choose a set seed or random seeds in the trainer; the seed is shown on screen after Instant Restart.

# Patches & Options
 * Pressing E and ESC will return the player back to the title screen (Enabled By Default)
 * Pressing Space in the main menu will start the game (Enabled By Default)
 * Remove Music (Disabled By Default). Mutes all looping audio, including looping ambience.
 * Clear Save on New Game and Instant Restart (Enabled By Default, so every run starts from a fresh save)
 * Instant Restart Run (Default hotkey: ``F6``)
 * Accessibility: UI Scale for the trainer, plain-font option, seed flash duration
 * Performance: V-Sync on/off and a target FPS cap

# Key names
Binds accept Unity key names in any case: letters, digits (``1``), ``F1``-``F12``, ``insert``, ``space``, ``left shift``, ``right ctrl``, ``up``/``down``/``left``/``right``, keypad keys (``[1]``, ``[+]``), and controller buttons (``joystick button 0``-``joystick button 19``). Leave a bind empty to unbind it. The Config UI flags unknown names.

# Installation
* Download [SpeedRave.zip](https://github.com/CodeNameMeteor/SpeedRave/releases).
* Download [BepInEx 5 x64](https://github.com/BepInEx/BepInEx/releases/) (the `BepInEx_win_x64_5.4.x` zip). BepInEx 6 is not supported.
* Optional: check the download. Run `Get-FileHash SpeedRave.zip` in PowerShell and compare the result with the SHA-256 listed in the release notes.
* Extract Bepinex in the game directory ``<game folder>/SewerRaveWindows``
* Start The Game once
* Extract SpeedRave.zip to ``<game folder>/SewerRaveWindows/BepInEx``
* To Edit the config go to ``<game folder>/SewerRaveWindows/BepInEx/config`` and edit ``SpeedRave.cfg``.

# Building and testing
* Build `Source/SpeedRave.sln` in Visual Studio. If the game isn't in the default Steam folder, copy `Source/SpeedRave.csproj.user.example` to `Source/SpeedRave.csproj.user` and set `GameDir`.
* `dotnet test Tests/SpeedRave.Tests` runs the unit tests (no game needed).
* `Tests/CompileCheck` compiles the whole mod against stand-ins for the game's types, so CI can catch build errors without the game. CI downloads BepInEx for it; locally, extract a BepInEx 5 x64 zip into `Tests/CompileCheck/bepinex` and run `dotnet build Tests/CompileCheck`.

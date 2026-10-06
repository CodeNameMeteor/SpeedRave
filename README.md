# SpeedRave
 A Speedrun Mod for sewer rave

# Features
 * Trainer including Room Picker. press ``insert`` in game
 * Autosplitter. Start TCP Server in Livesplit (LiveSplit Server on its default port, 16834). The mod only connects to LiveSplit on this computer (127.0.0.1), so a firewall only needs to allow local connections on that port.
 * Inventory Overlay
 * Modified Seed Generation Allowing For Set Seed Runs (Thanks To <a href="https://github.com/Som1Lse">Som1Lse</a>)
   
# Patches & Options
 * Pressing E and ESC will return the player back to the title screen (Enabled By Default)
 * Pressing Space in the main menu will start the game (Enabled By Default)
 * Remove Music (Disabled By Default)
 * Clear Save on New Game and Instant Restart (Enabled By Default, so every run starts from a fresh save)
 * Instant Restart Run (Default hotkey: ``F6``)

# Installation
* Download [SpeedRave.zip](https://github.com/CodeNameMeteor/SpeedRave/releases).
* Download [BepInEx 5 x64](https://github.com/BepInEx/BepInEx/releases/) (the `BepInEx_win_x64_5.4.x` zip). BepInEx 6 is not supported.
* Optional: check the download. Run `Get-FileHash SpeedRave.zip` in PowerShell and compare the result with the SHA-256 listed in the release notes.
* Extract Bepinex in the game directory ``<game folder>/SewerRaveWindows``
* Start The Game once
* Extract SpeedRave.zip to ``<game folder>/SewerRaveWindows/BepInEx``
* To Edit the config go to ``<game folder>/SewerRaveWindows/BepInEx/config`` and edit ``SpeedRave.cfg``.

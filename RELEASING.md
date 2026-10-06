# Releasing SpeedRave

1. Update `modVersion` in `Source/Plugin.cs`. The DLL's assembly and file versions are taken from it automatically.
2. Build in **Release** configuration (see the build notes in `README.md`).
3. Zip the release so it extracts into `<game folder>/SewerRaveWindows/BepInEx`, as the README describes:
   - `plugins/SpeedRave.dll`
   - `CustomTextures/cheese.png` and `CustomTextures/fruit.png` (used by the inventory overlay icons)
4. Compute the zip's SHA-256:
   - Windows (PowerShell): `Get-FileHash SpeedRave.zip -Algorithm SHA256`
   - Linux/macOS: `sha256sum SpeedRave.zip`
5. Create the GitHub release with the tag `vX.Y.Z`, attach `SpeedRave.zip`, and put the SHA-256 in the release notes so players can check their download.
6. Say which BepInEx version the release was tested with (currently BepInEx 5.4.x, x64).

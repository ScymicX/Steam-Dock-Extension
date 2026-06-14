# WinGet submission

WinGet distribution uses the unpackaged EXE installers from a GitHub release.
The Microsoft Store `.msixbundle` is a separate publication artifact.

## 1. Create the GitHub release

1. Open the repository's **Actions** tab.
2. Select **Release WinGet installers**.
3. Choose **Run workflow** on `main`.
4. Enter version `0.1.0` and the release notes.
5. Wait for release `v0.1.0` to contain:
   - `SteamDockExtension-Setup-0.1.0-x64.exe`
   - `SteamDockExtension-Setup-0.1.0-arm64.exe`
   - `SHA256SUMS.txt`

## 2. Install WinGetCreate

```powershell
winget install Microsoft.WingetCreate
wingetcreate --version
```

## 3. Generate the first manifests

Run this with the permanent GitHub release URLs:

```powershell
wingetcreate new `
  --out .\winget-manifests `
  "https://github.com/ScymicX/Steam-Dock-Extension/releases/download/v0.1.0/SteamDockExtension-Setup-0.1.0-x64.exe" `
  "https://github.com/ScymicX/Steam-Dock-Extension/releases/download/v0.1.0/SteamDockExtension-Setup-0.1.0-arm64.exe"
```

Use these values when prompted:

- Package identifier: `DennieZorg.SteamDockExtension`
- Package version: `0.1.0`
- Publisher: `DennieZorg`
- Package name: `Steam Dock Extension`
- License: `MIT`
- License URL: `https://github.com/ScymicX/Steam-Dock-Extension/blob/main/LICENSE`
- Publisher URL: `https://github.com/ScymicX`
- Package URL: `https://github.com/ScymicX/Steam-Dock-Extension`
- Privacy URL: `https://github.com/ScymicX/Steam-Dock-Extension/blob/main/PRIVACY.md`
- Installer type: `inno`
- Scope: `user`

Answer **No** when WinGetCreate first asks whether it should submit the
manifests. We need to check the Command Palette discovery tag first.

## 4. Check the generated manifests

Open the generated `.locale.en-US.yaml` file and make sure it contains:

```yaml
Tags:
- steam
- games
- powertoys
- windows-commandpalette-extension
```

This project does not use Windows App SDK, so a
`Microsoft.WindowsAppRuntime` package dependency is not required.

Validate the folder containing the generated manifests:

```powershell
winget validate .\winget-manifests
```

## 5. Submit the pull request

```powershell
wingetcreate submit `
  --prtitle "New package: DennieZorg.SteamDockExtension version 0.1.0" `
  .\winget-manifests
```

WinGetCreate opens the GitHub login flow, forks `microsoft/winget-pkgs`, and
creates the pull request. Test the package after the pull request checks pass:

```powershell
winget install DennieZorg.SteamDockExtension
```

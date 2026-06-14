# Publishing assets

## Microsoft Store

- `Store/StoreListingTile.png` - 300x300 Store app tile icon
- `Store/01-dock-library-and-friends.png` - 1920x1080 desktop screenshot
- `Store/02-steam-library.png` - 1920x1080 desktop screenshot
- `Store/03-extension-settings.png` - 1920x1080 desktop screenshot
- `Store/04-steam-chat.png` - 1080x1920 portrait desktop screenshot

Suggested captions:

1. Search and launch installed Steam games directly from the Command Palette Dock.
2. Browse your local Steam library with cover artwork and recently played information.
3. Configure Steam friends, library detection, and refresh settings.
4. Select an online friend in the Dock to open a Steam chat.

## GitHub and WinGet

- `GitHub/SteamDockExtension-Banner.png` - 1280x640 repository and release banner

WinGet does not require separate screenshots or logos in its manifests.

## Regeneration

Run `PreparePublishingAssets.ps1` after replacing source images in
`Afbeelding Ontwikkeling`.

## Store package

Run `Build-StorePackage.ps1` from a Windows machine with the .NET 10 SDK and
Windows SDK installed. It creates an unsigned x64/ARM64 `.msixbundle` under
`artifacts/store` for upload to Partner Center.

See `RELEASE_CHECKLIST.md` and `STORE_LISTING.md` for the remaining submission
steps and prepared listing text.

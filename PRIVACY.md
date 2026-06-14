# Privacy policy

Steam Dock Extension does not operate a developer-owned server, include
analytics, or collect telemetry.

## Data used by the extension

- Installed-game information is read locally from Steam configuration,
  manifest, and artwork cache files.
- The optional SteamID64, Steam installation path, and display preferences are
  stored in the extension's local application data.
- A Steam Web API key entered in the settings is stored for the current Windows
  user in Windows Credential Manager. It is not written to the settings file.
- When online friends are enabled, the extension sends the configured API key,
  SteamID64, and friend Steam IDs directly to Valve's Steam Web API over HTTPS.
- Selecting a friend or Store action opens a `steam://` or Steam web address in
  the locally installed Steam client or default browser.

The extension does not transmit this data to the extension developer.

## Removing data

Enter `CLEAR` in the Steam Web API key setting to remove the stored credential.
Other settings can be removed by uninstalling the extension and deleting its
local application data through Windows.

## Third parties

Use of Steam services is subject to Valve's own terms and privacy policy. Steam
Dock Extension is an independent project and is not affiliated with or endorsed
by Valve Corporation or Microsoft.

Last updated: June 14, 2026.

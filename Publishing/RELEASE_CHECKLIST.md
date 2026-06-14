# Release checklist

## Repository

- [x] Source, assets, scripts, privacy policy, security policy, and MIT license
- [x] Build validation for x64 and ARM64
- [x] Generated output, certificates, user settings, and IDE files ignored
- [x] Create the public GitHub repository
- [ ] Enable private security advisories in the GitHub repository settings
- [x] Replace the repository placeholders in Store and release metadata
- [x] Tag the exact reviewed commit as `v0.1.0`

## Microsoft Store

- [x] Partner Center product identity reserved
- [x] Manifest identity and publisher match Partner Center
- [x] Store tile and four screenshots prepared
- [x] Privacy policy available in the repository
- [x] Reproducible unsigned x64/ARM64 Store bundle script
- [ ] Rotate the Steam Web API key visible in development screenshots
- [ ] Capture a fresh settings screenshot after deploying the secure-key build
- [ ] Confirm consent for, or anonymize, friend names shown in Store screenshots
- [x] Publish the repository so the privacy policy has a public HTTPS URL
- [ ] Run `Publishing/Build-StorePackage.ps1`
- [ ] Complete age ratings, properties, pricing/availability, and Store listing
- [ ] Upload the generated `.msixbundle` and submit for certification

## WinGet

- [ ] Publish a versioned installer or package as a permanent GitHub release asset
- [ ] Generate and validate WinGet manifests with `wingetcreate`
- [ ] Submit the manifests to `microsoft/winget-pkgs`
- [ ] Repeat the manifest update for every released version

Once the Store listing is live, users can also install it through the `msstore`
source using its Store ID. A separate `winget-pkgs` submission is only needed
for an independent GitHub-hosted distribution channel.

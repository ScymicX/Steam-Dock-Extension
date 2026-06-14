#define AppName "Steam Dock Extension"
#define AppPublisher "DennieZorg"
#define AppExeName "SteamDockExtension.exe"

[Setup]
AppId={{F560E91B-9A9B-4FD0-8EBA-5194745879E9}
AppName={#AppName}
AppVersion=__VERSION__
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/ScymicX/Steam-Dock-Extension
AppSupportURL=https://github.com/ScymicX/Steam-Dock-Extension/issues
AppUpdatesURL=https://github.com/ScymicX/Steam-Dock-Extension/releases
DefaultDirName={localappdata}\Programs\SteamDockExtension
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=__OUTPUT_DIRECTORY__
OutputBaseFilename=SteamDockExtension-Setup-__VERSION__-__PLATFORM__
SetupIconFile=__ICON_PATH__
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=__ARCHITECTURES_ALLOWED__
ArchitecturesInstallIn64BitMode=__ARCHITECTURES_64BIT__
MinVersion=10.0.19041
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "__PUBLISH_DIRECTORY__\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Classes\CLSID\{{389FA345-561B-4898-A7D6-11528EC27454}"; ValueType: string; ValueName: ""; ValueData: "Steam Dock Extension"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\CLSID\{{389FA345-561B-4898-A7D6-11528EC27454}\LocalServer32"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExeName}"" -RegisterProcessAsComServer"; Flags: uninsdeletekey

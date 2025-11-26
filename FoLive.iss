; FoLive Windows Installer Script
; Inno Setup Script for creating professional Windows installer

#define AppName "FoLive"
#define AppVersion "1.0.0"
#define AppPublisher "FoLive Team"
#define AppURL "https://github.com/maitrithanh/FoLive"
#define AppExeName "FoLive.exe"
#define OutputDir "dist"
#define OutputBaseFilename "FoLive-Setup"

[Setup]
; App info
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
; LicenseFile=
; InfoBeforeFile=
; InfoAfterFile=
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
; SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "vietnamese"; MessagesFile: "compiler:Languages\Vietnamese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "startmenu"; Description: "Create Start Menu shortcut"; GroupDescription: "Shortcuts"; Flags: checkedonce

[Files]
Source: "dist\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "env.example"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion; IsReadme: yes
; Note: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard;
begin
  // Check for FFmpeg
  if not Exec('ffmpeg', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if MsgBox('FFmpeg is not installed. Do you want to install it now?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      // Try to install via winget
      if Exec('winget', 'install ffmpeg', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      begin
        MsgBox('FFmpeg installed successfully!', mbInformation, MB_OK);
      end
      else
      begin
        MsgBox('Could not install FFmpeg automatically. Please install it manually from https://ffmpeg.org/download.html', mbError, MB_OK);
      end;
    end;
  end;
end;


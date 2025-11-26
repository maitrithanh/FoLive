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

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "startmenu"; Description: "Create Start Menu shortcut"; GroupDescription: "Shortcuts"; Flags: checkedonce

[Files]
Source: "dist\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "install_dependencies.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion; IsReadme: yes
; Note: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\install_dependencies.ps1"" -Silent"; Description: "Install dependencies (FFmpeg, yt-dlp)"; Flags: runhidden nowait skipifnotsilent
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DependenciesPage: TOutputProgressWizardPage;
  DependenciesInstalled: Boolean;

procedure InitializeWizard;
var
  FFmpegInstalled: Boolean;
  ResultCode: Integer;
begin
  // Check if FFmpeg is already installed
  FFmpegInstalled := Exec('ffmpeg', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  
  if not FFmpegInstalled then
  begin
    // Create dependencies installation page
    DependenciesPage := CreateOutputProgressPage('Installing Dependencies', 'Please wait while we install required components...');
    DependenciesPage.Show;
    
    try
      DependenciesPage.SetText('Installing FFmpeg...', '');
      DependenciesPage.SetProgress(0, 100);
      
      // Try winget first (most reliable on Windows 10/11)
      if Exec('winget', 'install --id Gyan.FFmpeg -e --accept-source-agreements --accept-package-agreements --silent', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
        begin
          DependenciesPage.SetText('FFmpeg installed successfully!', '');
          DependenciesPage.SetProgress(50, 100);
          Sleep(1000);
        end;
      end
      // Try Chocolatey
      else if Exec('choco', 'install ffmpeg -y --no-progress', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
        begin
          DependenciesPage.SetText('FFmpeg installed successfully!', '');
          DependenciesPage.SetProgress(50, 100);
          Sleep(1000);
        end;
      end;
      
      // Try to install yt-dlp (optional but recommended)
      DependenciesPage.SetText('Installing yt-dlp (optional)...', '');
      DependenciesPage.SetProgress(75, 100);
      
      if Exec('winget', 'install --id yt-dlp.yt-dlp -e --accept-source-agreements --accept-package-agreements --silent', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
        begin
          DependenciesPage.SetText('yt-dlp installed successfully!', '');
          DependenciesPage.SetProgress(100, 100);
          Sleep(1000);
        end;
      end
      else if Exec('choco', 'install yt-dlp -y --no-progress', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
        begin
          DependenciesPage.SetText('yt-dlp installed successfully!', '');
          DependenciesPage.SetProgress(100, 100);
          Sleep(1000);
        end;
      end;
      
      DependenciesInstalled := True;
      
    finally
      DependenciesPage.Hide;
    end;
    
    // Verify FFmpeg installation
    if not Exec('ffmpeg', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      if MsgBox('FFmpeg could not be installed automatically.' + #13#10 + #13#10 +
                'FoLive requires FFmpeg to function.' + #13#10 + #13#10 +
                'Would you like to:' + #13#10 +
                '1. Continue installation (you can install FFmpeg manually later)' + #13#10 +
                '2. Cancel and install FFmpeg first', mbConfirmation, MB_YESNO) = IDNO then
      begin
        Abort;
      end;
    end
    else
    begin
      MsgBox('All dependencies installed successfully!' + #13#10 + #13#10 +
             'FFmpeg is ready to use.', mbInformation, MB_OK);
    end;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  // If we're on the ready page and dependencies were installed, show message
  if (CurPageID = wpReady) and DependenciesInstalled then
  begin
    // Dependencies already installed in InitializeWizard
  end;
end;


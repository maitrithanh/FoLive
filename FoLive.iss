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
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
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
  FFmpegInstalled: Boolean;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  FFmpegPath: String;
begin
  Result := True;
  FFmpegInstalled := False;
  
  // Check if FFmpeg is already in PATH
  if Exec('ffmpeg', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
    begin
      FFmpegInstalled := True;
      Exit;
    end;
  end;
  
  // Check common installation paths
  FFmpegPath := ExpandConstant('{pf}\ffmpeg\bin\ffmpeg.exe');
  if FileExists(FFmpegPath) then
  begin
    FFmpegInstalled := True;
    Exit;
  end;
  
  FFmpegPath := ExpandConstant('{pf32}\ffmpeg\bin\ffmpeg.exe');
  if FileExists(FFmpegPath) then
  begin
    FFmpegInstalled := True;
    Exit;
  end;
end;

procedure InitializeWizard;
var
  ResultCode: Integer;
  InstallSuccess: Boolean;
begin
  // Skip if already installed
  if FFmpegInstalled then
    Exit;
  
  // Automatically install FFmpeg without asking
  // Create progress page
  DependenciesPage := CreateOutputProgressPage('Installing FFmpeg', 'FoLive is installing FFmpeg automatically...');
  DependenciesPage.Show;
  
  InstallSuccess := False;
  
  try
    DependenciesPage.SetText('Installing FFmpeg via winget...', 'This may take a few minutes.');
    DependenciesPage.SetProgress(10, 100);
    
    // Try winget first (Windows 10/11)
    if Exec('winget', 'install --id Gyan.FFmpeg -e --accept-source-agreements --accept-package-agreements --silent', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      if ResultCode = 0 then
      begin
        DependenciesPage.SetText('FFmpeg installation completed. Verifying...', '');
        DependenciesPage.SetProgress(80, 100);
        Sleep(3000); // Wait for PATH to update
        
        // Verify installation
        if Exec('ffmpeg', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
        begin
          if ResultCode = 0 then
          begin
            InstallSuccess := True;
            FFmpegInstalled := True;
          end;
        end;
      end;
    end;
    
    // Try Chocolatey if winget failed
    if not InstallSuccess then
    begin
      DependenciesPage.SetText('Trying Chocolatey...', '');
      DependenciesPage.SetProgress(30, 100);
      
      if Exec('choco', 'install ffmpeg -y --no-progress', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
        begin
          DependenciesPage.SetText('FFmpeg installation completed. Verifying...', '');
          DependenciesPage.SetProgress(80, 100);
          Sleep(3000);
          
          if Exec('ffmpeg', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
          begin
            if ResultCode = 0 then
            begin
              InstallSuccess := True;
              FFmpegInstalled := True;
            end;
          end;
        end;
      end;
    end;
    
    DependenciesPage.SetProgress(100, 100);
    
  finally
    DependenciesPage.Hide;
  end;
  
  // Show result only if failed
  if InstallSuccess then
  begin
    // Silent success - no message needed
  end
  else
  begin
    // Only ask if automatic installation failed
    if MsgBox('FFmpeg could not be installed automatically.' + #13#10 + #13#10 +
              'Possible reasons:' + #13#10 +
              '- winget or Chocolatey is not available' + #13#10 +
              '- Administrator privileges required' + #13#10 +
              '- Network connection issue' + #13#10 + #13#10 +
              'You can install FFmpeg manually:' + #13#10 +
              '1. Visit: https://ffmpeg.org/download.html' + #13#10 +
              '2. Or run: winget install Gyan.FFmpeg' + #13#10 + #13#10 +
              'Would you like to:' + #13#10 +
              '1. Continue installation (install FFmpeg manually later)' + #13#10 +
              '2. Cancel and install FFmpeg first', 
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Abort;
    end;
  end;
end;


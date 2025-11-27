; FoLive Windows Installer Script
; Inno Setup Script for creating professional Windows installer

#define AppName "FoLive"
#define AppVersion "3.0.13"
#define AppPublisher "FoLive Team"
#define AppURL "https://github.com/maitrithanh/FoLive"
#define AppExeName "FoLive.exe"
#define OutputDir "dist"
#define OutputBaseFilename "FoLive-Setup"

[Setup]
; App info
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}}
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
Source: "dist\ffmpeg.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: FFmpegZipExists
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
; Note: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  DependenciesPage: TOutputProgressWizardPage;
  FFmpegInstalled: Boolean;

// Note: PATH changes are written to registry
// New processes will automatically pick up the updated PATH
// Existing processes may need to be restarted

function FFmpegZipExists(): Boolean;
begin
  Result := FileExists(ExpandConstant('{tmp}\ffmpeg.zip'));
end;

function AddFFmpegBinToPath(BinPath: String): Boolean;
forward;

procedure AddFFmpegToPath();
forward;

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

procedure CurStepChanged(CurStep: TSetupStep);
var
  FFmpegZipPath: String;
  FFmpegExtractPath: String;
  FFmpegBinPath: String;
  ResultCode: Integer;
  UnzipTool: String;
begin
  // Extract FFmpeg after files are installed
  if CurStep = ssPostInstall then
  begin
    // Skip if already installed
    if FFmpegInstalled then
      Exit;
    
    // Check if FFmpeg zip exists
    FFmpegZipPath := ExpandConstant('{tmp}\ffmpeg.zip');
    if not FileExists(FFmpegZipPath) then
    begin
      // No bundled FFmpeg, try to add existing FFmpeg to PATH
      AddFFmpegToPath();
      Exit;
    end;
    
    // Show progress
    DependenciesPage := CreateOutputProgressPage('Installing FFmpeg', 'Extracting and configuring FFmpeg...');
    DependenciesPage.Show;
    
    try
      DependenciesPage.SetText('Extracting FFmpeg...', '');
      DependenciesPage.SetProgress(20, 100);
      
      // Extract to app directory
      FFmpegExtractPath := ExpandConstant('{app}\ffmpeg');
      FFmpegBinPath := FFmpegExtractPath + '\bin';
      
      // Use PowerShell to extract (built-in, no need for 7zip)
      DependenciesPage.SetText('Extracting FFmpeg archive...', 'This may take a moment.');
      DependenciesPage.SetProgress(40, 100);
      
      if Exec('powershell.exe', '-Command "Expand-Archive -Path ''' + FFmpegZipPath + ''' -DestinationPath ''' + FFmpegExtractPath + ''' -Force"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        if ResultCode = 0 then
        begin
          DependenciesPage.SetText('Adding FFmpeg to PATH...', '');
          DependenciesPage.SetProgress(80, 100);
          
          // Add to PATH
          if AddFFmpegBinToPath(FFmpegBinPath) then
          begin
            DependenciesPage.SetText('FFmpeg installed successfully!', '');
            DependenciesPage.SetProgress(100, 100);
            Sleep(1000);
            
            // Verify
            if Exec(FFmpegBinPath + '\ffmpeg.exe', '-version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
            begin
              if ResultCode = 0 then
              begin
                FFmpegInstalled := True;
              end;
            end;
          end;
        end;
      end;
      
    finally
      DependenciesPage.Hide;
    end;
  end;
end;

function AddFFmpegBinToPath(BinPath: String): Boolean;
var
  Paths: String;
  NewPath: String;
begin
  Result := False;
  // Get current PATH
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', Paths) then
    Paths := '';
  
  // Check if already in PATH
  if Pos(BinPath, Paths) > 0 then
  begin
    Result := True;
    Exit;
  end;
  
  // Add to PATH
  NewPath := Paths;
  if NewPath <> '' then
    NewPath := NewPath + ';';
  NewPath := NewPath + BinPath;
  
  // Write to registry
  if RegWriteStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', NewPath) then
  begin
    Result := True;
    // PATH is now updated in registry
    // New processes will automatically use the updated PATH
    // Existing processes may need to be restarted to see the change
  end;
end;

procedure AddFFmpegToPath();
var
  FFmpegPath: String;
begin
  // Check common installation paths and add to PATH if found
  FFmpegPath := ExpandConstant('{pf}\ffmpeg\bin');
  if DirExists(FFmpegPath) then
  begin
    AddFFmpegBinToPath(FFmpegPath);
    Exit;
  end;
  
  FFmpegPath := ExpandConstant('{pf32}\ffmpeg\bin');
  if DirExists(FFmpegPath) then
  begin
    AddFFmpegBinToPath(FFmpegPath);
    Exit;
  end;
end;

procedure InitializeWizard;
begin
  // Just check, installation will happen in CurStepChanged
end;


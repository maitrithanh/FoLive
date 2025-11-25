@echo off
setlocal enabledelayedexpansion

echo FoLive - Auto Installer
echo ========================
echo.

set "INSTALL_DIR=%USERPROFILE%\.folive"
set "BIN_DIR=%INSTALL_DIR%\bin"
set "APP_DIR=%INSTALL_DIR%\app"

echo Creating directories...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%BIN_DIR%" mkdir "%BIN_DIR%"
if not exist "%APP_DIR%" mkdir "%APP_DIR%"
if not exist "%INSTALL_DIR%\temp" mkdir "%INSTALL_DIR%\temp"
if not exist "%INSTALL_DIR%\output" mkdir "%INSTALL_DIR%\output"

echo.

:: Find executable
set "EXECUTABLE="
if exist "FoLive.exe" (
    set "EXECUTABLE=FoLive.exe"
) else if exist "dist\FoLive.exe" (
    set "EXECUTABLE=dist\FoLive.exe"
)

if "!EXECUTABLE!"=="" (
    echo ERROR: Khong tim thay executable!
    echo    Vui long chay build truoc: python build_installer.py
    pause
    exit /b 1
)

:: Copy executable
echo Copying files...
copy /Y "!EXECUTABLE!" "%BIN_DIR%\FoLive.exe"

:: Copy templates
if exist "templates" (
    xcopy /E /I /Y "templates" "%APP_DIR%\templates\"
)

:: Copy config
if exist "env.example" (
    if not exist "%APP_DIR%\.env" (
        copy /Y "env.example" "%APP_DIR%\.env"
        echo Created .env file
    )
)

:: Create launcher script
echo @echo off > "%BIN_DIR%\folive.bat"
echo cd /d "%%~dp0.." >> "%BIN_DIR%\folive.bat"
echo set FOLIVE_HOME=%%CD%% >> "%BIN_DIR%\folive.bat"
echo set TEMP_DIR=%%FOLIVE_HOME%%\temp >> "%BIN_DIR%\folive.bat"
echo set OUTPUT_DIR=%%FOLIVE_HOME%%\output >> "%BIN_DIR%\folive.bat"
echo bin\FoLive.exe %%* >> "%BIN_DIR%\folive.bat"

:: Add to PATH
setx PATH "%PATH%;%BIN_DIR%" >nul 2>&1
set "PATH=%PATH%;%BIN_DIR%"

:: Check FFmpeg
echo.
echo Checking FFmpeg...
where ffmpeg >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo FFmpeg not found!
    echo.
    echo Installing FFmpeg...
    
    :: Try winget
    where winget >nul 2>&1
    if %ERRORLEVEL% EQU 0 (
        echo Using winget...
        winget install ffmpeg
    ) else (
        echo Please install FFmpeg manually:
        echo   1. Download from https://ffmpeg.org/download.html
        echo   2. Or use: choco install ffmpeg
        pause
    )
) else (
    echo FFmpeg is installed!
    ffmpeg -version | findstr /C:"ffmpeg version"
)

echo.
echo ========================================
echo Installation complete!
echo ========================================
echo.
echo Installation directory: %INSTALL_DIR%
echo.
echo Run: folive
echo Or: %BIN_DIR%\FoLive.exe
echo.
echo Access: http://localhost:5000
echo.
pause



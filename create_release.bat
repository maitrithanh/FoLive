@echo off
REM FoLive - Create Release Package for Windows
REM This script builds and packages the release

echo ========================================
echo FoLive - Create Release Package
echo ========================================
echo.

set "RELEASE_DIR=release"
set "VERSION=%1"

if "%VERSION%"=="" (
    echo Usage: create_release.bat [version]
    echo Example: create_release.bat 1.0.0
    exit /b 1
)

echo Version: %VERSION%
echo.

REM Clean previous release
if exist "%RELEASE_DIR%" rmdir /s /q "%RELEASE_DIR%"
mkdir "%RELEASE_DIR%"

echo [1/5] Checking dependencies...
python --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Python not found!
    exit /b 1
)

pip show pyinstaller >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Installing PyInstaller...
    pip install pyinstaller
)

echo [2/5] Building executable...
python -m PyInstaller --clean --noconfirm FoLive.spec
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed!
    exit /b 1
)

echo [3/5] Creating installer bundle...
python build_installer.py
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: build_installer.py failed, continuing...
)

echo [4/5] Preparing release files...

REM Copy executable
if exist "dist\FoLive.exe" (
    copy /Y "dist\FoLive.exe" "%RELEASE_DIR%\"
    echo   - Copied FoLive.exe
)

REM Copy installer
if exist "installer\install.bat" (
    copy /Y "installer\install.bat" "%RELEASE_DIR%\"
    echo   - Copied install.bat
)

REM Copy bundle if exists
if exist "FoLive-Windows-Bundle" (
    xcopy /E /I /Y "FoLive-Windows-Bundle" "%RELEASE_DIR%\FoLive-Windows-Bundle\"
    echo   - Copied bundle
)

REM Copy zip if exists
if exist "*.zip" (
    copy /Y "*.zip" "%RELEASE_DIR\" 2>nul
    echo   - Copied zip archive
)

REM Copy documentation
copy /Y "README.md" "%RELEASE_DIR\" 2>nul
copy /Y "INSTALL.md" "%RELEASE_DIR\" 2>nul
copy /Y "env.example" "%RELEASE_DIR\" 2>nul
echo   - Copied documentation

REM Create version file
echo %VERSION% > "%RELEASE_DIR%\VERSION.txt"

echo [5/5] Creating release archive...
cd "%RELEASE_DIR%"
powershell -Command "Compress-Archive -Path * -DestinationPath ..\FoLive-Windows-%VERSION%.zip -Force"
cd ..

if exist "FoLive-Windows-%VERSION%.zip" (
    echo.
    echo ========================================
    echo Release package created successfully!
    echo ========================================
    echo.
    echo File: FoLive-Windows-%VERSION%.zip
    echo Directory: %RELEASE_DIR%\
    echo.
    echo Next steps:
    echo   1. Test the release package
    echo   2. Create GitHub release
    echo   3. Upload FoLive-Windows-%VERSION%.zip
    echo.
) else (
    echo ERROR: Failed to create archive!
    exit /b 1
)

pause


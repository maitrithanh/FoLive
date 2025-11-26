@echo off
REM FoLive - Push Code and Create Release
REM This script pushes code to GitHub and creates a release

setlocal enabledelayedexpansion

echo ========================================
echo FoLive - Push and Release
echo ========================================
echo.

REM Get version from user
set /p VERSION="Enter version number (e.g., 1.0.0): "
if "%VERSION%"=="" (
    echo ERROR: Version is required!
    exit /b 1
)

REM Remove 'v' prefix if exists
set TAG_NAME=%VERSION%
if "%TAG_NAME:~0,1%"=="v" (
    set TAG_NAME=%TAG_NAME:~1%
)
set TAG_NAME=v%TAG_NAME%

echo.
echo Version: %VERSION%
echo Tag: %TAG_NAME%
echo.

REM Check git status
echo [1/4] Checking git status...
git status --short
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Not a git repository!
    exit /b 1
)

REM Ask for commit message
set /p COMMIT_MSG="Enter commit message (or press Enter for default): "
if "%COMMIT_MSG%"=="" (
    set COMMIT_MSG=Update code and prepare release %TAG_NAME%
)

echo.
echo [2/4] Committing changes...
git add .
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Nothing to commit or commit failed
)

echo.
echo [3/4] Pushing to GitHub...
git push origin main
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Push failed!
    exit /b 1
)

echo.
echo [4/4] Creating and pushing tag...
git tag -a %TAG_NAME% -m "Release %TAG_NAME%"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Tag might already exist
)

git push origin %TAG_NAME%
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to push tag!
    exit /b 1
)

echo.
echo ========================================
echo [OK] Success!
echo ========================================
echo.
echo Code pushed to: origin/main
echo Tag created: %TAG_NAME%
echo.
echo GitHub Actions will automatically:
echo   1. Build C# application
echo   2. Create Windows installer
echo   3. Create GitHub Release
echo   4. Upload FoLive.exe and FoLive-Setup.exe
echo.
echo Check progress at:
echo   https://github.com/maitrithanh/FoLive/actions
echo.
echo Release will be available at:
echo   https://github.com/maitrithanh/FoLive/releases
echo.
echo Waiting 5 seconds before opening browser...
timeout /t 5 /nobreak >nul

start https://github.com/maitrithanh/FoLive/actions



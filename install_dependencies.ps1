# FoLive - Auto-install Dependencies Script
# This script installs FFmpeg and yt-dlp automatically

param(
    [switch]$Silent = $false
)

$ErrorActionPreference = "Continue"

function Write-Log {
    param([string]$Message)
    if (-not $Silent) {
        Write-Host $Message
    }
}

function Install-FFmpeg {
    Write-Log "Checking FFmpeg..."
    
    # Check if already installed
    try {
        $process = Start-Process -FilePath "ffmpeg" -ArgumentList "-version" -NoNewWindow -Wait -PassThru -ErrorAction SilentlyContinue
        if ($process.ExitCode -eq 0) {
            Write-Log "[OK] FFmpeg is already installed"
            return $true
        }
    } catch {
        Write-Log "[INFO] FFmpeg not found, installing..."
    }
    
    # Try winget first (Windows 10/11)
    # According to https://www.gyan.dev/ffmpeg/builds/
    # release full: winget install ffmpeg
    # release essentials: winget install "FFmpeg (Essentials Build)"
    try {
        Write-Log "Attempting to install FFmpeg via winget (full build)..."
        $process = Start-Process -FilePath "winget" -ArgumentList "install ffmpeg --accept-source-agreements --accept-package-agreements --silent" -NoNewWindow -Wait -PassThru -ErrorAction SilentlyContinue
        if ($process.ExitCode -eq 0) {
            Write-Log "[OK] FFmpeg installed via winget"
            
            # Refresh PATH
            $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
            
            # Verify installation
            Start-Sleep -Seconds 3
            try {
                $verifyProcess = Start-Process -FilePath "ffmpeg" -ArgumentList "-version" -NoNewWindow -Wait -PassThru -ErrorAction SilentlyContinue
                if ($verifyProcess.ExitCode -eq 0) {
                    Write-Log "[OK] FFmpeg verified successfully"
                    return $true
                } else {
                    Write-Log "[WARNING] FFmpeg installed but not in PATH yet"
                    return $true
                }
            } catch {
                Write-Log "[WARNING] FFmpeg installed but not in PATH yet"
                return $true
            }
        } else {
            Write-Log "[WARNING] winget full build installation failed, trying essentials build..."
            # Try essentials build as fallback
            $process = Start-Process -FilePath "winget" -ArgumentList "install `"FFmpeg (Essentials Build)`" --accept-source-agreements --accept-package-agreements --silent" -NoNewWindow -Wait -PassThru -ErrorAction SilentlyContinue
            if ($process.ExitCode -eq 0) {
                Write-Log "[OK] FFmpeg (Essentials) installed via winget"
                $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
                Start-Sleep -Seconds 3
                try {
                    $verifyProcess = Start-Process -FilePath "ffmpeg" -ArgumentList "-version" -NoNewWindow -Wait -PassThru -ErrorAction SilentlyContinue
                    if ($verifyProcess.ExitCode -eq 0) {
                        Write-Log "[OK] FFmpeg verified successfully"
                        return $true
                    }
                } catch { }
                return $true
            } else {
                Write-Log "[WARNING] winget installation failed with exit code: $($process.ExitCode)"
            }
        }
    } catch {
        Write-Log "[WARNING] winget installation failed: $_"
    }
    
    # Try Chocolatey
    try {
        Write-Log "Attempting to install FFmpeg via Chocolatey..."
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            choco install ffmpeg -y --no-progress
            if ($LASTEXITCODE -eq 0) {
                Write-Log "[OK] FFmpeg installed via Chocolatey"
                return $true
            }
        }
    } catch {
        Write-Log "[WARNING] Chocolatey installation failed"
    }
    
    # Try Scoop
    try {
        Write-Log "Attempting to install FFmpeg via Scoop..."
        if (Get-Command scoop -ErrorAction SilentlyContinue) {
            scoop install ffmpeg
            if ($LASTEXITCODE -eq 0) {
                Write-Log "[OK] FFmpeg installed via Scoop"
                return $true
            }
        }
    } catch {
        Write-Log "[WARNING] Scoop installation failed"
    }
    
    Write-Log "[ERROR] Could not install FFmpeg automatically"
    Write-Log "Please install FFmpeg manually from: https://ffmpeg.org/download.html"
    return $false
}

function Install-YtDlp {
    Write-Log "Checking yt-dlp..."
    
    # Check if already installed
    try {
        $null = yt-dlp --version 2>&1
        Write-Log "[OK] yt-dlp is already installed"
        return $true
    } catch {
        Write-Log "[INFO] yt-dlp not found, installing..."
    }
    
    # Try winget
    try {
        Write-Log "Attempting to install yt-dlp via winget..."
        $process = Start-Process -FilePath "winget" -ArgumentList "install yt-dlp --accept-source-agreements --accept-package-agreements --silent" -NoNewWindow -Wait -PassThru -ErrorAction SilentlyContinue
        if ($process.ExitCode -eq 0) {
            Write-Log "[OK] yt-dlp installed via winget"
            return $true
        } else {
            Write-Log "[WARNING] winget installation failed with exit code: $($process.ExitCode)"
        }
    } catch {
        Write-Log "[WARNING] winget installation failed: $_"
    }
    
    # Try Chocolatey
    try {
        Write-Log "Attempting to install yt-dlp via Chocolatey..."
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            choco install yt-dlp -y --no-progress
            if ($LASTEXITCODE -eq 0) {
                Write-Log "[OK] yt-dlp installed via Chocolatey"
                return $true
            }
        }
    } catch {
        Write-Log "[WARNING] Chocolatey installation failed"
    }
    
    # Try Scoop
    try {
        Write-Log "Attempting to install yt-dlp via Scoop..."
        if (Get-Command scoop -ErrorAction SilentlyContinue) {
            scoop install yt-dlp
            if ($LASTEXITCODE -eq 0) {
                Write-Log "[OK] yt-dlp installed via Scoop"
                return $true
            }
        }
    } catch {
        Write-Log "[WARNING] Scoop installation failed"
    }
    
    Write-Log "[WARNING] Could not install yt-dlp automatically"
    Write-Log "yt-dlp is optional but recommended for YouTube/Facebook streaming"
    return $false
}

# Main installation
Write-Log "========================================"
Write-Log "FoLive - Installing Dependencies"
Write-Log "========================================"
Write-Log ""

$ffmpegOk = Install-FFmpeg
Write-Log ""
$ytdlpOk = Install-YtDlp

Write-Log ""
Write-Log "========================================"
if ($ffmpegOk) {
    Write-Log "[OK] All dependencies installed successfully!"
} else {
    Write-Log "[WARNING] Some dependencies may need manual installation"
}
Write-Log "========================================"

if (-not $Silent) {
    Write-Log ""
    Write-Log "Press any key to continue..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

exit 0


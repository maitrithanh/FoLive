#!/usr/bin/env python3
"""
Script build installer đầy đủ cho FoLive
"""
import os
import sys
import subprocess
import shutil
import platform
from pathlib import Path

def check_pyinstaller():
    """Kiểm tra PyInstaller đã cài chưa"""
    try:
        import PyInstaller
        return True
    except ImportError:
        return False

def install_pyinstaller():
    """Cài đặt PyInstaller"""
    print("📦 Đang cài đặt PyInstaller...")
    subprocess.run([sys.executable, "-m", "pip", "install", "pyinstaller"], check=True)

def build_executable():
    """Build executable với PyInstaller"""
    print("🔨 Đang build executable...")
    
    # Tạo spec file nếu chưa có
    spec_content = """# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['run.py'],
    pathex=[],
    binaries=[],
    datas=[
        ('env.example', '.'),
    ],
    hiddenimports=[
        'tkinter',
        'tkinter.ttk',
        'tkinter.filedialog',
        'tkinter.messagebox',
        'tkinter.scrolledtext',
        'yt_dlp',
        'pydub',
        'dotenv',
        'requests',
        'psutil',
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='FoLive',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
"""
    
    with open('FoLive.spec', 'w') as f:
        f.write(spec_content)
    
    # Build
    subprocess.run([
        sys.executable, "-m", "PyInstaller",
        "--clean",
        "--noconfirm",
        "FoLive.spec"
    ], check=True)
    
    print("✅ Build executable thành công!")

def create_installer_script():
    """Tạo script cài đặt tự động"""
    system = platform.system()
    
    if system == "Darwin":  # macOS
        create_macos_installer()
    elif system == "Windows":
        create_windows_installer()
    elif system == "Linux":
        create_linux_installer()
    else:
        print(f"⚠️  Hệ điều hành {system} chưa được hỗ trợ")

def create_macos_installer():
    """Tạo installer cho macOS"""
    print("🍎 Đang tạo macOS installer...")
    
    installer_script = """#!/bin/bash

echo "🎥 FoLive Installer"
echo "==================="
echo ""

APP_NAME="FoLive"
INSTALL_DIR="/Applications/FoLive"
BIN_DIR="$INSTALL_DIR/bin"
APP_DIR="$INSTALL_DIR/app"

# Tạo thư mục
mkdir -p "$BIN_DIR"
mkdir -p "$APP_DIR"
mkdir -p "$INSTALL_DIR/temp"
mkdir -p "$INSTALL_DIR/output"

# Copy files
echo "📦 Đang cài đặt files..."
cp -r dist/FoLive "$BIN_DIR/"
cp -r templates "$APP_DIR/" 2>/dev/null || true
cp env.example "$APP_DIR/.env.example" 2>/dev/null || true
cp README.md "$APP_DIR/" 2>/dev/null || true

# Tạo launcher script
cat > "$BIN_DIR/folive" << 'EOF'
#!/bin/bash
cd "$(dirname "$0")"
./FoLive
EOF

chmod +x "$BIN_DIR/folive"
chmod +x "$BIN_DIR/FoLive"

# Kiểm tra FFmpeg
if ! command -v ffmpeg &> /dev/null; then
    echo "⚠️  FFmpeg chưa được cài đặt"
    echo "   Đang kiểm tra Homebrew..."
    
    if command -v brew &> /dev/null; then
        read -p "Cài đặt FFmpeg qua Homebrew? (y/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            brew install ffmpeg
        fi
    else
        echo "   Vui lòng cài đặt FFmpeg: brew install ffmpeg"
    fi
fi

# Tạo symlink
if [ -d "/usr/local/bin" ]; then
    ln -sf "$BIN_DIR/folive" /usr/local/bin/folive
    echo "✅ Đã tạo symlink: /usr/local/bin/folive"
fi

echo ""
echo "✅ Cài đặt thành công!"
echo ""
echo "🚀 Chạy ứng dụng:"
echo "   folive"
echo "   hoặc"
echo "   $BIN_DIR/folive"
echo ""
"""
    
    with open('install_macos.sh', 'w') as f:
        f.write(installer_script)
    
    os.chmod('install_macos.sh', 0o755)
    print("✅ Đã tạo install_macos.sh")

def create_windows_installer():
    """Tạo installer cho Windows"""
    print("🪟 Đang tạo Windows installer...")
    
    installer_script = """@echo off
echo FoLive Installer
echo =================
echo.

set "INSTALL_DIR=%ProgramFiles%\\FoLive"
set "BIN_DIR=%INSTALL_DIR%\\bin"
set "APP_DIR=%INSTALL_DIR%\\app"

echo Creating directories...
mkdir "%INSTALL_DIR%" 2>nul
mkdir "%BIN_DIR%" 2>nul
mkdir "%APP_DIR%" 2>nul
mkdir "%INSTALL_DIR%\\temp" 2>nul
mkdir "%INSTALL_DIR%\\output" 2>nul

echo Copying files...
xcopy /E /I /Y "dist\\FoLive.exe" "%BIN_DIR%\\"
xcopy /E /I /Y "templates" "%APP_DIR%\\templates\\" 2>nul
copy /Y "env.example" "%APP_DIR%\\.env.example" 2>nul
copy /Y "README.md" "%APP_DIR%\\" 2>nul

echo.
echo Checking FFmpeg...
where ffmpeg >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo FFmpeg not found!
    echo Please install FFmpeg from https://ffmpeg.org/download.html
    echo Or use: winget install ffmpeg
    pause
)

echo.
echo Installation complete!
echo.
echo Run: %BIN_DIR%\\FoLive.exe
echo.
pause
"""
    
    with open('install_windows.bat', 'w') as f:
        f.write(installer_script)
    
    print("✅ Đã tạo install_windows.bat")

def create_linux_installer():
    """Tạo installer cho Linux"""
    print("🐧 Đang tạo Linux installer...")
    
    installer_script = """#!/bin/bash

echo "🎥 FoLive Installer"
echo "==================="
echo ""

INSTALL_DIR="/opt/folive"
BIN_DIR="$INSTALL_DIR/bin"
APP_DIR="$INSTALL_DIR/app"

# Check root
if [ "$EUID" -ne 0 ]; then 
    echo "⚠️  Cần quyền root để cài đặt"
    echo "   Chạy: sudo bash install_linux.sh"
    exit 1
fi

# Tạo thư mục
mkdir -p "$BIN_DIR"
mkdir -p "$APP_DIR"
mkdir -p "$INSTALL_DIR/temp"
mkdir -p "$INSTALL_DIR/output"

# Copy files
echo "📦 Đang cài đặt files..."
cp dist/FoLive "$BIN_DIR/"
cp -r templates "$APP_DIR/" 2>/dev/null || true
cp env.example "$APP_DIR/.env.example" 2>/dev/null || true
cp README.md "$APP_DIR/" 2>/dev/null || true

chmod +x "$BIN_DIR/FoLive"

# Tạo launcher script
cat > "$BIN_DIR/folive" << 'EOF'
#!/bin/bash
cd "$(dirname "$0")"
./FoLive
EOF

chmod +x "$BIN_DIR/folive"

# Tạo symlink
ln -sf "$BIN_DIR/folive" /usr/local/bin/folive

# Kiểm tra FFmpeg
if ! command -v ffmpeg &> /dev/null; then
    echo "⚠️  FFmpeg chưa được cài đặt"
    echo "   Đang cài đặt FFmpeg..."
    
    if command -v apt-get &> /dev/null; then
        apt-get update
        apt-get install -y ffmpeg
    elif command -v yum &> /dev/null; then
        yum install -y ffmpeg
    elif command -v dnf &> /dev/null; then
        dnf install -y ffmpeg
    else
        echo "   Vui lòng cài đặt FFmpeg thủ công"
    fi
fi

echo ""
echo "✅ Cài đặt thành công!"
echo ""
echo "🚀 Chạy ứng dụng:"
echo "   folive"
echo ""
"""
    
    with open('install_linux.sh', 'w') as f:
        f.write(installer_script)
    
    os.chmod('install_linux.sh', 0o755)
    print("✅ Đã tạo install_linux.sh")

def create_bundle():
    """Tạo bundle đầy đủ"""
    system = platform.system()
    print(f"📦 Đang tạo bundle cho {system}...")
    
    bundle_dir = f"FoLive-{system}-Bundle"
    if os.path.exists(bundle_dir):
        shutil.rmtree(bundle_dir)
    
    os.makedirs(bundle_dir, exist_ok=True)
    
    # Copy executable
    if os.path.exists("dist/FoLive"):
        shutil.copy("dist/FoLive", bundle_dir)
    elif os.path.exists("dist/FoLive.exe"):
        shutil.copy("dist/FoLive.exe", bundle_dir)
    
    # Copy templates
    if os.path.exists("templates"):
        shutil.copytree("templates", os.path.join(bundle_dir, "templates"))
    
    # Copy config files
    if os.path.exists("env.example"):
        shutil.copy("env.example", bundle_dir)
    
    if os.path.exists("README.md"):
        shutil.copy("README.md", bundle_dir)
    
    # Copy installer script
    if system == "Darwin" and os.path.exists("install_macos.sh"):
        shutil.copy("install_macos.sh", bundle_dir)
    elif system == "Windows" and os.path.exists("install_windows.bat"):
        shutil.copy("install_windows.bat", bundle_dir)
    elif system == "Linux" and os.path.exists("install_linux.sh"):
        shutil.copy("install_linux.sh", bundle_dir)
    
    # Tạo README cho bundle
    bundle_readme = f"""# FoLive {system} Bundle

## Cài đặt

### Cách 1: Tự động (Khuyến nghị)

"""
    
    if system == "Darwin":
        bundle_readme += """```bash
chmod +x install_macos.sh
./install_macos.sh
```

### Cách 2: Thủ công

1. Copy file `FoLive` vào thư mục bạn muốn
2. Chạy: `./FoLive`
"""
    elif system == "Windows":
        bundle_readme += """```cmd
install_windows.bat
```

### Cách 2: Thủ công

1. Copy file `FoLive.exe` vào thư mục bạn muốn
2. Chạy: `FoLive.exe`
"""
    else:
        bundle_readme += """```bash
sudo bash install_linux.sh
```

### Cách 2: Thủ công

1. Copy file `FoLive` vào thư mục bạn muốn
2. Chạy: `./FoLive`
"""
    
    bundle_readme += """
## Yêu cầu

- FFmpeg (sẽ được cài đặt tự động nếu có thể)
- Hệ điều hành: """ + system + """

## Sử dụng

Sau khi cài đặt, chạy:
```bash
folive
```

Hoặc truy cập: http://localhost:5000
"""
    
    with open(os.path.join(bundle_dir, "README.txt"), 'w') as f:
        f.write(bundle_readme)
    
    # Tạo archive
    archive_name = f"FoLive-{system}-{platform.machine()}.tar.gz"
    if system == "Windows":
        archive_name = f"FoLive-{system}-{platform.machine()}.zip"
        import zipfile
        with zipfile.ZipFile(archive_name, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for root, dirs, files in os.walk(bundle_dir):
                for file in files:
                    file_path = os.path.join(root, file)
                    arcname = os.path.relpath(file_path, bundle_dir)
                    zipf.write(file_path, arcname)
    else:
        subprocess.run(["tar", "-czf", archive_name, bundle_dir], check=True)
    
    print(f"✅ Đã tạo bundle: {archive_name}")
    return archive_name

def main():
    """Hàm main"""
    print("=" * 50)
    print("🎥 FoLive - Build Installer")
    print("=" * 50)
    print()
    
    # Kiểm tra PyInstaller
    if not check_pyinstaller():
        print("⚠️  PyInstaller chưa được cài đặt")
        install_pyinstaller()
    
    # Build executable
    if not build_executable():
        print("❌ Không thể build executable, dừng lại")
        sys.exit(1)
    
    # Tạo installer scripts
    try:
        create_installer_script()
    except Exception as e:
        print(f"⚠️  Lỗi tạo installer script: {e}")
    
    # Tạo bundle
    try:
        archive = create_bundle()
        print()
        print("=" * 50)
        print("✅ Hoàn tất!")
        print("=" * 50)
        if archive:
            print(f"📦 Bundle: {archive}")
        print()
        print("🚀 Để cài đặt:")
        system = platform.system()
        if system == "Windows":
            print("   install.bat")
            print("   hoặc chạy trực tiếp: dist\\FoLive.exe")
        else:
            print("   Ứng dụng này chỉ hỗ trợ Windows")
    except Exception as e:
        print(f"⚠️  Lỗi tạo bundle: {e}")
        print("✅ Executable đã được build tại: dist\\FoLive.exe")

if __name__ == '__main__':
    main()



#!/usr/bin/env python3
"""
Script để build Windows Setup Installer (.exe) bằng Inno Setup
"""
import os
import sys
import subprocess
import shutil
from pathlib import Path

def check_inno_setup():
    """Kiểm tra Inno Setup đã cài chưa"""
    # Inno Setup thường ở Program Files
    inno_paths = [
        r"C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        r"C:\Program Files\Inno Setup 6\ISCC.exe",
        r"C:\Program Files (x86)\Inno Setup 5\ISCC.exe",
        r"C:\Program Files\Inno Setup 5\ISCC.exe",
    ]
    
    for path in inno_paths:
        if os.path.exists(path):
            return path
    
    # Kiểm tra trong PATH
    try:
        result = subprocess.run(
            ["where", "iscc"],
            capture_output=True,
            text=True,
            shell=True
        )
        if result.returncode == 0:
            return result.stdout.strip().split('\n')[0]
    except:
        pass
    
    return None

def install_inno_setup_guide():
    """Hướng dẫn cài đặt Inno Setup"""
    print("=" * 60)
    print("⚠️  Inno Setup chưa được cài đặt!")
    print("=" * 60)
    print()
    print("Để tạo installer chuyên nghiệp, bạn cần cài Inno Setup:")
    print()
    print("1. Tải Inno Setup từ: https://jrsoftware.org/isdl.php")
    print("2. Cài đặt Inno Setup (miễn phí)")
    print("3. Chạy lại script này")
    print()
    print("Hoặc bạn có thể:")
    print("- Dùng winget: winget install JRSoftware.InnoSetup")
    print("- Dùng Chocolatey: choco install innosetup")
    print()
    print("=" * 60)

def build_setup():
    """Build setup installer"""
    print("🔨 Đang build Windows Setup Installer...")
    print()
    
    # Kiểm tra Inno Setup
    iscc_path = check_inno_setup()
    if not iscc_path:
        install_inno_setup_guide()
        return False
    
    print(f"✅ Tìm thấy Inno Setup: {iscc_path}")
    print()
    
    # Kiểm tra file .iss
    iss_file = "FoLive.iss"
    if not os.path.exists(iss_file):
        print(f"❌ Không tìm thấy file {iss_file}")
        return False
    
    # Kiểm tra executable đã build chưa
    exe_file = "dist/FoLive.exe"
    if not os.path.exists(exe_file):
        print(f"❌ Không tìm thấy {exe_file}")
        print("   Hãy build executable trước: python -m PyInstaller --clean --noconfirm FoLive.spec")
        return False
    
    print(f"✅ Tìm thấy executable: {exe_file}")
    print()
    
    # Build installer
    print("📦 Đang compile installer...")
    try:
        result = subprocess.run(
            [iscc_path, iss_file],
            check=True,
            capture_output=True,
            text=True
        )
        
        # Tìm file setup đã build
        setup_file = "dist/FoLive-Setup.exe"
        if os.path.exists(setup_file):
            size_mb = os.path.getsize(setup_file) / (1024 * 1024)
            print()
            print("=" * 60)
            print("✅ Build installer thành công!")
            print("=" * 60)
            print(f"📦 File: {setup_file}")
            print(f"📊 Size: {size_mb:.2f} MB")
            print()
            print("🚀 Bạn có thể:")
            print(f"   1. Chạy file: {setup_file}")
            print("   2. Upload lên GitHub Releases")
            print("   3. Chia sẻ với người dùng")
            print()
            return True
        else:
            print("⚠️  Không tìm thấy file setup sau khi build")
            print("   Kiểm tra logs ở trên để biết lỗi")
            return False
            
    except subprocess.CalledProcessError as e:
        print(f"❌ Lỗi khi build installer:")
        print(e.stdout)
        print(e.stderr)
        return False
    except Exception as e:
        print(f"❌ Lỗi: {e}")
        return False

def main():
    """Hàm main"""
    print("=" * 60)
    print("🎥 FoLive - Build Windows Setup Installer")
    print("=" * 60)
    print()
    
    if build_setup():
        print("✅ Hoàn tất!")
    else:
        print("❌ Build thất bại!")
        sys.exit(1)

if __name__ == '__main__':
    main()


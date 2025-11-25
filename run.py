#!/usr/bin/env python3
"""
Script khởi chạy FoLive - Windows Desktop Application
"""
import os
import sys
import platform

def check_dependencies():
    """Kiểm tra các dependencies cần thiết"""
    print("🔍 Đang kiểm tra dependencies...")
    
    # Check Windows
    if platform.system() != "Windows":
        print("⚠️  Cảnh báo: Ứng dụng này được thiết kế cho Windows")
        response = input("Bạn có muốn tiếp tục? (y/n): ")
        if response.lower() != 'y':
            return False
    
    # Kiểm tra FFmpeg
    from ffmpeg_handler import FFmpegHandler
    ffmpeg = FFmpegHandler()
    if not ffmpeg.check_ffmpeg():
        print("❌ FFmpeg không được tìm thấy!")
        print("   Vui lòng cài đặt FFmpeg:")
        print("   winget install ffmpeg")
        print("   hoặc tải từ: https://ffmpeg.org/download.html")
        return False
    
    print("✅ FFmpeg: OK")
    
    # Kiểm tra tkinter (built-in với Python trên Windows)
    try:
        import tkinter
        print("✅ GUI library: OK")
    except ImportError:
        print("❌ Tkinter không được tìm thấy!")
        print("   Tkinter thường được cài sẵn với Python trên Windows")
        return False
    
    # Kiểm tra Python packages
    try:
        import yt_dlp
        print("✅ Python packages: OK")
    except ImportError as e:
        print(f"❌ Thiếu package: {e}")
        print("   Chạy: pip install -r requirements.txt")
        return False
    
    return True

def main():
    """Hàm main"""
    import argparse
    
    parser = argparse.ArgumentParser(description='FoLive - Livestream 24/7 Manager')
    parser.add_argument('--host', default=None, help='Host to bind (default: from config)')
    parser.add_argument('--port', type=int, default=None, help='Port to bind (default: from config)')
    parser.add_argument('--debug', action='store_true', help='Enable debug mode')
    parser.add_argument('--check', action='store_true', help='Check dependencies and exit')
    
    args = parser.parse_args()
    
    print("=" * 50)
    print("🎥 FoLive - Livestream 24/7 Manager")
    print("=" * 50)
    print()
    
    if args.check:
        if check_dependencies():
            print("✅ All dependencies OK!")
            sys.exit(0)
        else:
            sys.exit(1)
    
    if not check_dependencies():
        sys.exit(1)
    
    print()
    print("🚀 Đang khởi động ứng dụng...")
    
    # Import và chạy GUI
    try:
        from gui import main as gui_main
        print("✅ Khởi động GUI...")
        gui_main()
    except ImportError as e:
        print(f"❌ Không thể import GUI: {e}")
        print("   Đang khởi động web interface...")
        from app import app
        from config import HOST, PORT, DEBUG
        
        host = args.host or HOST
        port = args.port or PORT
        debug = args.debug or DEBUG
        
        print(f"📡 Truy cập: http://{host}:{port}")
        print()
        
        app.run(host=host, port=port, debug=debug)

if __name__ == '__main__':
    main()


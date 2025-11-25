#!/usr/bin/env python3
"""
Script khởi chạy FoLive
"""
import os
import sys
from ffmpeg_handler import FFmpegHandler

def check_dependencies():
    """Kiểm tra các dependencies cần thiết"""
    print("🔍 Đang kiểm tra dependencies...")
    
    # Kiểm tra FFmpeg
    ffmpeg = FFmpegHandler()
    if not ffmpeg.check_ffmpeg():
        print("❌ FFmpeg không được tìm thấy!")
        print("   Vui lòng cài đặt FFmpeg:")
        print("   macOS: brew install ffmpeg")
        print("   Ubuntu: sudo apt-get install ffmpeg")
        return False
    
    print("✅ FFmpeg: OK")
    
    # Kiểm tra Python packages
    try:
        import flask
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
    print("🚀 Đang khởi động server...")
    
    # Import và chạy app
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


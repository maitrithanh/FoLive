#!/bin/bash

echo "🎥 FoLive Setup Script"
echo "======================"
echo ""

# Check Python
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 không được tìm thấy!"
    exit 1
fi

echo "✅ Python: $(python3 --version)"

# Check FFmpeg
if ! command -v ffmpeg &> /dev/null; then
    echo "⚠️  FFmpeg không được tìm thấy!"
    echo "   Cài đặt FFmpeg:"
    echo "   macOS: brew install ffmpeg"
    echo "   Ubuntu: sudo apt-get install ffmpeg"
    exit 1
fi

echo "✅ FFmpeg: $(ffmpeg -version | head -n 1)"

# Create virtual environment
if [ ! -d "venv" ]; then
    echo "📦 Tạo virtual environment..."
    python3 -m venv venv
fi

# Activate virtual environment
echo "🔧 Kích hoạt virtual environment..."
source venv/bin/activate

# Install dependencies
echo "📥 Cài đặt dependencies..."
pip install --upgrade pip
pip install -r requirements.txt

# Create .env file if not exists
if [ ! -f ".env" ]; then
    echo "📝 Tạo file .env..."
    cp env.example .env
    echo "⚠️  Vui lòng chỉnh sửa file .env với thông tin của bạn!"
fi

# Create directories
echo "📁 Tạo thư mục cần thiết..."
mkdir -p temp output

echo ""
echo "✅ Setup hoàn tất!"
echo ""
echo "🚀 Chạy ứng dụng:"
echo "   python run.py"
echo "   hoặc"
echo "   python app.py"
echo ""



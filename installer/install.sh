#!/bin/bash

# FoLive Auto Installer
# Tự động cài đặt và cấu hình FoLive

set -e

echo "🎥 FoLive - Auto Installer"
echo "=========================="
echo ""

# Detect OS
OS="$(uname -s)"
ARCH="$(uname -m)"

echo "📋 System: $OS $ARCH"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Installation directory
INSTALL_DIR="$HOME/.folive"
BIN_DIR="$INSTALL_DIR/bin"
APP_DIR="$INSTALL_DIR/app"

# Create directories
echo "📁 Creating directories..."
mkdir -p "$INSTALL_DIR"
mkdir -p "$BIN_DIR"
mkdir -p "$APP_DIR"
mkdir -p "$INSTALL_DIR/temp"
mkdir -p "$INSTALL_DIR/output"

# Find executable
EXECUTABLE=""
if [ -f "FoLive" ]; then
    EXECUTABLE="FoLive"
elif [ -f "dist/FoLive" ]; then
    EXECUTABLE="dist/FoLive"
elif [ -f "FoLive.exe" ]; then
    EXECUTABLE="FoLive.exe"
fi

if [ -z "$EXECUTABLE" ]; then
    echo -e "${RED}❌ Không tìm thấy executable!${NC}"
    echo "   Vui lòng chạy build trước: python build_installer.py"
    exit 1
fi

# Copy executable
echo "📦 Copying files..."
cp "$EXECUTABLE" "$BIN_DIR/FoLive"
chmod +x "$BIN_DIR/FoLive"

# Copy templates if exists
if [ -d "templates" ]; then
    cp -r templates "$APP_DIR/"
fi

# Copy config
if [ -f "env.example" ]; then
    if [ ! -f "$APP_DIR/.env" ]; then
        cp env.example "$APP_DIR/.env"
        echo "✅ Đã tạo file .env"
    fi
fi

# Create launcher script
cat > "$BIN_DIR/folive" << 'LAUNCHER_EOF'
#!/bin/bash
cd "$(dirname "$0")/.."
export FOLIVE_HOME="$(pwd)"
export TEMP_DIR="$FOLIVE_HOME/temp"
export OUTPUT_DIR="$FOLIVE_HOME/output"
./bin/FoLive "$@"
LAUNCHER_EOF

chmod +x "$BIN_DIR/folive"

# Add to PATH
add_to_path() {
    local shell_rc=""
    if [ -n "$ZSH_VERSION" ]; then
        shell_rc="$HOME/.zshrc"
    elif [ -n "$BASH_VERSION" ]; then
        shell_rc="$HOME/.bashrc"
    fi
    
    if [ -n "$shell_rc" ]; then
        if ! grep -q "$BIN_DIR" "$shell_rc" 2>/dev/null; then
            echo "" >> "$shell_rc"
            echo "# FoLive" >> "$shell_rc"
            echo "export PATH=\"$BIN_DIR:\$PATH\"" >> "$shell_rc"
            echo -e "${GREEN}✅ Đã thêm vào PATH${NC}"
            echo "   Chạy: source $shell_rc"
        fi
    fi
    
    # Also add to current session
    export PATH="$BIN_DIR:$PATH"
}

add_to_path

# Check and install FFmpeg
check_ffmpeg() {
    if command -v ffmpeg &> /dev/null; then
        echo -e "${GREEN}✅ FFmpeg đã được cài đặt${NC}"
        ffmpeg -version | head -n 1
        return 0
    fi
    
    echo -e "${YELLOW}⚠️  FFmpeg chưa được cài đặt${NC}"
    
    if [ "$OS" = "Darwin" ]; then
        if command -v brew &> /dev/null; then
            echo "🍺 Đang cài đặt FFmpeg qua Homebrew..."
            brew install ffmpeg
            return 0
        else
            echo "   Cài đặt Homebrew: /bin/bash -c \"\$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)\""
            echo "   Sau đó chạy: brew install ffmpeg"
        fi
    elif [ "$OS" = "Linux" ]; then
        if command -v apt-get &> /dev/null; then
            echo "📦 Đang cài đặt FFmpeg..."
            sudo apt-get update
            sudo apt-get install -y ffmpeg
            return 0
        elif command -v yum &> /dev/null; then
            echo "📦 Đang cài đặt FFmpeg..."
            sudo yum install -y ffmpeg
            return 0
        elif command -v dnf &> /dev/null; then
            echo "📦 Đang cài đặt FFmpeg..."
            sudo dnf install -y ffmpeg
            return 0
        fi
    fi
    
    echo "   Vui lòng cài đặt FFmpeg thủ công từ: https://ffmpeg.org/download.html"
    return 1
}

check_ffmpeg

# Create desktop entry (Linux)
if [ "$OS" = "Linux" ] && [ -d "$HOME/.local/share/applications" ]; then
    mkdir -p "$HOME/.local/share/applications"
    cat > "$HOME/.local/share/applications/folive.desktop" << DESKTOP_EOF
[Desktop Entry]
Name=FoLive
Comment=Livestream 24/7 Manager
Exec=$BIN_DIR/folive
Icon=applications-multimedia
Terminal=true
Type=Application
Categories=AudioVideo;Network;
DESKTOP_EOF
    echo "✅ Đã tạo desktop entry"
fi

echo ""
echo -e "${GREEN}✅ Cài đặt thành công!${NC}"
echo ""
echo "📍 Installation directory: $INSTALL_DIR"
echo ""
echo "🚀 Chạy ứng dụng:"
echo "   folive"
echo ""
echo "🌐 Hoặc truy cập: http://localhost:5000"
echo ""
echo "💡 Tip: Nếu lệnh 'folive' không hoạt động, chạy:"
echo "   source ~/.bashrc  # hoặc ~/.zshrc"
echo ""



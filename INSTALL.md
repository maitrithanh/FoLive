# 📦 Hướng dẫn cài đặt FoLive

## 🎯 Cài đặt nhanh (Khuyến nghị)

### Bước 1: Download

Tải file cài đặt từ [GitHub Releases](https://github.com/YOUR_USERNAME/FoLive/releases) phù hợp với hệ điều hành của bạn.

### Bước 2: Cài đặt

#### Linux/macOS

```bash
# Giải nén file
tar -xzf FoLive-Linux-*.tar.gz
# hoặc
tar -xzf FoLive-Darwin-*.tar.gz

# Chạy installer
chmod +x install.sh
./install.sh
```

#### Windows

```cmd
# Giải nén file
# Sử dụng WinRAR hoặc 7-Zip

# Chạy installer
install.bat
```

### Bước 3: Chạy

```bash
folive
```

Truy cập: http://localhost:5000

## ✅ Đã xong!

Không cần cài đặt Python hay bất kỳ package nào khác. Tất cả đã được bundle sẵn!

## 🔧 Cài đặt thủ công

Nếu muốn cài đặt thủ công:

### Linux/macOS

1. Giải nén bundle
2. Copy file `FoLive` vào thư mục bạn muốn (ví dụ: `/usr/local/bin/`)
3. Chạy: `chmod +x FoLive && ./FoLive`

### Windows

1. Giải nén bundle
2. Copy file `FoLive.exe` vào thư mục bạn muốn
3. Chạy: `FoLive.exe`

## 📋 Yêu cầu

- **FFmpeg**: Sẽ được cài đặt tự động nếu có thể
  - Linux: `apt-get install ffmpeg` hoặc `yum install ffmpeg`
  - macOS: `brew install ffmpeg`
  - Windows: Tải từ [ffmpeg.org](https://ffmpeg.org/download.html) hoặc `winget install ffmpeg`

## 🐛 Troubleshooting

### Lỗi "command not found: folive"

**Linux/macOS:**
```bash
# Thêm vào PATH
export PATH="$HOME/.folive/bin:$PATH"

# Hoặc thêm vào ~/.bashrc hoặc ~/.zshrc
echo 'export PATH="$HOME/.folive/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

**Windows:**
- Restart terminal sau khi cài đặt
- Hoặc chạy trực tiếp: `%USERPROFILE%\.folive\bin\FoLive.exe`

### FFmpeg không được tìm thấy

Installer sẽ tự động cài đặt FFmpeg nếu có thể. Nếu không:

- **Linux**: `sudo apt-get install ffmpeg`
- **macOS**: `brew install ffmpeg`
- **Windows**: Tải từ [ffmpeg.org](https://ffmpeg.org/download.html)

### Port 5000 đã được sử dụng

Thay đổi port trong file `.env`:
```
PORT=5001
```

## 📍 Vị trí cài đặt

- **Linux/macOS**: `~/.folive/`
- **Windows**: `%USERPROFILE%\.folive\`

## 🔄 Gỡ cài đặt

### Linux/macOS

```bash
rm -rf ~/.folive
# Xóa khỏi PATH trong ~/.bashrc hoặc ~/.zshrc
```

### Windows

```cmd
rmdir /s %USERPROFILE%\.folive
# Xóa khỏi PATH trong System Environment Variables
```

## 💡 Tips

- Sử dụng `folive --help` để xem các tùy chọn
- File cấu hình: `~/.folive/app/.env`
- Logs: Kiểm tra terminal output



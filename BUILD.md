# 🔨 Hướng dẫn Build FoLive

## Build Standalone Installer

### Yêu cầu

- Python 3.8+
- PyInstaller: `pip install pyinstaller`
- FFmpeg (để test)

### Cách build

```bash
# Cài đặt dependencies
pip install -r requirements.txt
pip install pyinstaller

# Build installer
python build_installer.py
```

Kết quả sẽ có:
- `dist/FoLive` hoặc `dist/FoLive.exe` - Standalone executable
- `install.sh` / `install.bat` - Auto installer scripts
- `FoLive-*-Bundle/` - Complete bundle với tất cả files
- `*.tar.gz` / `*.zip` - Archive files

## Build cho từng platform

### Linux

```bash
python -m PyInstaller --clean --noconfirm FoLive.spec
```

### macOS

```bash
python -m PyInstaller --clean --noconfirm FoLive.spec
```

### Windows

```bash
python -m PyInstaller --clean --noconfirm FoLive.spec
```

## Cấu trúc Bundle

```
FoLive-Platform-Bundle/
├── FoLive (hoặc FoLive.exe)
├── templates/
│   └── index.html
├── env.example
├── README.md
├── install.sh (Linux/macOS)
└── install.bat (Windows)
```

## Test installer

### Linux/macOS

```bash
cd FoLive-Linux-Bundle
chmod +x install.sh
./install.sh
folive
```

### Windows

```cmd
cd FoLive-Windows-Bundle
install.bat
folive
```

## Build trên GitHub Actions

Workflows tự động build khi:
- Push code lên main/master
- Tạo release tag

Xem kết quả trong tab **Actions** > **Artifacts**

## Troubleshooting

### PyInstaller không tìm thấy modules

Thêm vào `FoLive.spec`:
```python
hiddenimports=[
    'module_name',
]
```

### File quá lớn

- Sử dụng UPX để compress: `upx=True` trong spec
- Loại bỏ các dependencies không cần thiết

### Lỗi khi chạy executable

- Kiểm tra logs trong terminal
- Test trên clean system
- Đảm bảo FFmpeg đã được cài đặt

## Advanced Options

### Custom spec file

Chỉnh sửa `FoLive.spec` để tùy chỉnh:
- Dependencies
- Data files
- Icons
- Version info

### Code signing (macOS/Windows)

```bash
# macOS
codesign --sign "Developer ID" dist/FoLive

# Windows
signtool sign /f certificate.pfx dist/FoLive.exe
```


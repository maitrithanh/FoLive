# 📦 Hướng dẫn Build Windows Setup Installer

## 🎯 Mục đích

Tạo file **FoLive-Setup.exe** - Windows Installer chuyên nghiệp giống các phần mềm Windows thông thường, không chỉ là file .exe portable.

## ✨ Tính năng Setup Installer

- ✅ **Wizard cài đặt** - Giao diện cài đặt chuyên nghiệp
- ✅ **Cài vào Program Files** - Giống các app Windows thông thường
- ✅ **Shortcut Start Menu** - Dễ dàng tìm và chạy
- ✅ **Shortcut Desktop** - Tùy chọn
- ✅ **Gỡ cài đặt** - Có trong Control Panel
- ✅ **Kiểm tra FFmpeg** - Tự động kiểm tra và hướng dẫn cài đặt
- ✅ **Multi-language** - Hỗ trợ tiếng Anh và tiếng Việt

## 🔧 Yêu cầu

### Để build local:

1. **Inno Setup** (miễn phí)
   - Tải từ: https://jrsoftware.org/isdl.php
   - Hoặc: `winget install JRSoftware.InnoSetup`
   - Hoặc: `choco install innosetup`

2. **Python & PyInstaller** (đã có sẵn)

### Trên GitHub Actions:

- Tự động cài Inno Setup
- Không cần cài đặt gì thêm

## 🚀 Cách Build

### Cách 1: Build trên GitHub Actions (Khuyến nghị)

1. **Push tag lên GitHub:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Workflow tự động:**
   - Build executable
   - Build setup installer
   - Tạo release với cả 2 file

3. **Download từ Releases:**
   - `FoLive-Setup.exe` - Setup installer
   - `FoLive.exe` - Portable version

### Cách 2: Build local

**Bước 1: Build executable**
```bash
python -m PyInstaller --clean --noconfirm FoLive.spec
```

**Bước 2: Build setup installer**
```bash
python build_setup.py
```

**Kết quả:**
- `dist/FoLive-Setup.exe` - Setup installer
- `dist/FoLive.exe` - Portable executable

## 📝 Cấu hình

### File `FoLive.iss`

File này chứa cấu hình cho Inno Setup installer:

- **AppName**: Tên ứng dụng
- **AppVersion**: Phiên bản
- **DefaultDirName**: Thư mục cài đặt mặc định
- **Tasks**: Các tùy chọn (Desktop shortcut, Start Menu, etc.)
- **Files**: Files cần copy vào thư mục cài đặt
- **Icons**: Shortcuts cần tạo
- **Run**: Chạy app sau khi cài đặt

### Tùy chỉnh

Chỉnh sửa `FoLive.iss` để:
- Thay đổi thư mục cài đặt
- Thêm/bớt shortcuts
- Thay đổi icon
- Thêm license file
- Thêm thông tin version

## 🎨 Giao diện Installer

Setup installer sẽ có:
- ✅ Wizard cài đặt đẹp mắt
- ✅ Chọn thư mục cài đặt
- ✅ Tùy chọn shortcuts
- ✅ Progress bar khi cài đặt
- ✅ Tự động kiểm tra FFmpeg
- ✅ Hướng dẫn cài FFmpeg nếu thiếu

## 📦 Release Package

Sau khi build, release sẽ có:

1. **FoLive-Setup.exe** (Khuyến nghị)
   - Windows Installer chuyên nghiệp
   - Cài đặt như app bình thường
   - Có trong Control Panel

2. **FoLive.exe** (Portable)
   - Không cần cài đặt
   - Chạy trực tiếp
   - Phù hợp cho USB

## 🔍 Troubleshooting

### Lỗi: "Inno Setup not found"

**Giải pháp:**
- Cài Inno Setup: https://jrsoftware.org/isdl.php
- Hoặc dùng GitHub Actions (tự động cài)

### Lỗi: "Setup build failed"

**Kiểm tra:**
- File `dist/FoLive.exe` đã được build chưa?
- File `FoLive.iss` có đúng syntax không?
- Inno Setup đã được cài đặt chưa?

### Setup file quá lớn

**Giải pháp:**
- Setup file thường lớn hơn portable .exe (do có installer engine)
- Có thể tối ưu bằng cách:
  - Dùng compression tốt hơn
  - Loại bỏ files không cần thiết

## 💡 Tips

1. **Test installer:**
   - Cài đặt trên máy sạch
   - Kiểm tra shortcuts
   - Kiểm tra gỡ cài đặt

2. **Version number:**
   - Tự động lấy từ tag hoặc workflow input
   - Có thể chỉnh trong `FoLive.iss`

3. **Icon:**
   - Thêm file `.ico` vào project
   - Cập nhật `SetupIconFile` trong `FoLive.iss`

## 📚 Tài liệu tham khảo

- Inno Setup: https://jrsoftware.org/isinfo.php
- Inno Setup Script: https://jrsoftware.org/ishelp/
- PyInstaller: https://pyinstaller.org/

---

**Bây giờ bạn đã có cả 2 phiên bản:**
- ✅ **Setup Installer** - Giống app Windows thông thường
- ✅ **Portable** - Chạy trực tiếp không cần cài đặt


# 🚀 Hướng dẫn Push và Release

## ⚡ Cách nhanh nhất

### Windows:
```cmd
push_and_release.bat
```

### Linux/macOS:
```bash
./push_and_release.sh
```

Script sẽ:
1. ✅ Hỏi version (ví dụ: `1.0.0`)
2. ✅ Commit và push code
3. ✅ Tạo tag và push lên GitHub
4. ✅ GitHub Actions tự động build và tạo release

## 📋 Các bước chi tiết

### Bước 1: Chạy script

**Windows:**
```cmd
push_and_release.bat
```

**Linux/macOS:**
```bash
./push_and_release.sh
```

### Bước 2: Nhập version

Script sẽ hỏi:
```
Enter version number (e.g., 1.0.0):
```

Nhập version (ví dụ: `1.0.1`)

### Bước 3: Nhập commit message (tùy chọn)

Script sẽ hỏi:
```
Enter commit message (or press Enter for default):
```

Nhấn Enter để dùng default hoặc nhập message tùy chỉnh

### Bước 4: Chờ script chạy

Script sẽ tự động:
- ✅ Commit changes
- ✅ Push code lên GitHub
- ✅ Tạo tag `v1.0.1`
- ✅ Push tag lên GitHub

### Bước 5: GitHub Actions tự động build

Sau khi push tag, GitHub Actions sẽ tự động:
1. Build C# application
2. Build Windows installer (Inno Setup)
3. Tạo GitHub Release
4. Upload `FoLive.exe` và `FoLive-Setup.exe`

### Bước 6: Kiểm tra release

1. Vào **Actions**: https://github.com/maitrithanh/FoLive/actions
2. Chờ workflow "Release Build" chạy xong (5-10 phút)
3. Vào **Releases**: https://github.com/maitrithanh/FoLive/releases
4. Download `FoLive-Setup.exe` hoặc `FoLive.exe`

## 🔧 Manual (Nếu không dùng script)

### Cách 1: Push tag

```bash
# 1. Commit và push code
git add .
git commit -m "Update for release v1.0.1"
git push origin main

# 2. Tạo và push tag
git tag v1.0.1
git push origin v1.0.1
```

### Cách 2: GitHub Actions UI

1. Vào **Actions** > **Release Build**
2. Click **"Run workflow"**
3. Nhập version: `1.0.1`
4. Click **"Run workflow"**

## 📝 Version Numbering

Sử dụng [Semantic Versioning](https://semver.org/):
- **MAJOR.MINOR.PATCH** (ví dụ: 1.0.0)
- **MAJOR**: Breaking changes
- **MINOR**: New features
- **PATCH**: Bug fixes

Ví dụ:
- `1.0.0` - Initial release
- `1.0.1` - Bug fix
- `1.1.0` - New features
- `2.0.0` - Major update

## ⚠️ Lưu ý

1. **Đảm bảo code đã test** trước khi release
2. **Version phải tăng** mỗi lần release
3. **Tag không được trùng** - nếu trùng sẽ báo lỗi
4. **Chờ workflow chạy xong** trước khi release tiếp

## 🐛 Troubleshooting

### Lỗi: "Tag already exists"

**Giải pháp:**
- Xóa tag cũ: `git tag -d v1.0.1` và `git push origin :refs/tags/v1.0.1`
- Hoặc dùng version mới

### Lỗi: "Push failed"

**Giải pháp:**
- Kiểm tra kết nối internet
- Kiểm tra quyền trên repository
- Pull trước: `git pull origin main`

### Workflow không chạy

**Giải pháp:**
- Kiểm tra file `.github/workflows/release.yml` có tồn tại
- Kiểm tra tag đã được push chưa
- Xem logs trong Actions

---

**Chạy script và release sẽ tự động!** 🚀


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

### Cách 1: Push tag (Khuyến nghị)

```bash
# 1. Kiểm tra trạng thái
git status

# 2. Thêm tất cả thay đổi
git add .

# 3. Commit (thay v1.0.1 bằng version của bạn)
git commit -m "Update for release v1.0.1"

# 4. Push code lên GitHub
git push origin main

# 5. Tạo tag (thay v1.0.1 bằng version của bạn)
git tag v1.0.1

# 6. Push tag lên GitHub
git push origin v1.0.1
```

### Cách 2: Tạo tag với message

```bash
# Tạo tag với message mô tả
git tag -a v1.0.1 -m "Release v1.0.1: Add new features"
git push origin v1.0.1
```

### Cách 3: Xóa tag nếu cần (nếu tag đã tồn tại)

```bash
# Xóa tag local
git tag -d v1.0.1

# Xóa tag trên GitHub
git push origin :refs/tags/v1.0.1

# Sau đó tạo lại tag
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

## 🚀 Quick Git Commands

### Kiểm tra trạng thái
```bash
git status
```

### Xem các thay đổi
```bash
git diff
```

### Thêm tất cả thay đổi
```bash
git add .
```

### Commit với message
```bash
git commit -m "Your commit message here"
```

### Push code
```bash
git push origin main
```

### Tạo và push tag (1 lệnh)
```bash
# Tạo tag
git tag v1.0.1

# Push tag
git push origin v1.0.1
```

### Tạo tag với message và push (1 lệnh)
```bash
git tag -a v1.0.1 -m "Release v1.0.1" && git push origin v1.0.1
```

### Xem tất cả tags
```bash
git tag
```

### Xem tag cụ thể
```bash
git show v1.0.1
```

### Xóa tag local
```bash
git tag -d v1.0.1
```

### Xóa tag trên GitHub
```bash
git push origin :refs/tags/v1.0.1
```

### Pull code mới nhất
```bash
git pull origin main
```

### Xem log commits
```bash
git log --oneline
```

### Xem log với tags
```bash
git log --oneline --decorate
```

## ⚠️ Lưu ý

1. **Đảm bảo code đã test** trước khi release
2. **Version phải tăng** mỗi lần release
3. **Tag không được trùng** - nếu trùng sẽ báo lỗi
4. **Chờ workflow chạy xong** trước khi release tiếp
5. **Luôn pull trước khi push** để tránh conflict: `git pull origin main`

## 📋 Checklist trước khi release

- [ ] Code đã được test
- [ ] Không có lỗi compile
- [ ] Đã commit tất cả thay đổi
- [ ] Đã pull code mới nhất: `git pull origin main`
- [ ] Version number đã được tăng
- [ ] Tag chưa tồn tại: `git tag | grep v1.0.1`
- [ ] Sẵn sàng push: `git push origin main && git push origin v1.0.1`

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

## 📚 Tài liệu tham khảo

- **Git Commands chi tiết**: Xem [GIT_COMMANDS.md](./GIT_COMMANDS.md)
- **GitHub Actions**: https://github.com/maitrithanh/FoLive/actions
- **Releases**: https://github.com/maitrithanh/FoLive/releases

---

**Chạy script và release sẽ tự động!** 🚀

**Hoặc copy commands từ [GIT_COMMANDS.md](./GIT_COMMANDS.md) để thao tác thủ công!**


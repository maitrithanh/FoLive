# 🚀 Hướng dẫn tạo Release

## ⚡ Cách nhanh nhất: Tạo Release qua GitHub Actions

### 📍 Bước 1: Vào GitHub Actions
1. Mở repository trên GitHub (ví dụ: `https://github.com/YOUR_USERNAME/FoLive`)
2. Click tab **"Actions"** (ở trên cùng, bên cạnh Code, Issues, Pull requests...)
3. Ở sidebar bên trái, tìm và click **"Release Build"**

### ▶️ Bước 2: Chạy workflow
1. Bạn sẽ thấy nút **"Run workflow"** (dropdown màu xanh ở góc trên bên phải)
2. Click vào dropdown đó
3. Chọn branch: `main` (hoặc `master` - branch chính của bạn)
4. Nhập version: `1.0.0` (chỉ số, KHÔNG cần chữ "v")
5. Click nút **"Run workflow"** (màu xanh)

### ⏳ Bước 3: Chờ workflow hoàn thành
- Workflow sẽ tự động:
  - ✅ Build file `.exe` (mất 5-10 phút)
  - ✅ Tạo GitHub Release
  - ✅ Upload file `FoLive.exe` lên release

**Làm sao biết đang chạy?**
- Vào tab **Actions**
- Bạn sẽ thấy workflow run với status "In progress" (màu vàng)
- Khi xong sẽ chuyển thành "Success" (màu xanh) hoặc "Failed" (màu đỏ)

### ✅ Bước 4: Kiểm tra Release
1. Click tab **"Releases"** trên GitHub (bên cạnh tab Actions)
2. Bạn sẽ thấy release mới với tên "Release v1.0.0"
3. Trong phần **Assets**, bạn sẽ thấy file **FoLive.exe**
4. Click vào **FoLive.exe** để download!

## 🎯 Nếu không thấy nút "Run workflow"

**Có thể do:**
- Bạn chưa vào đúng workflow → Đảm bảo đã chọn "Release Build" ở sidebar
- Bạn không có quyền → Cần quyền write trên repository
- Workflow file chưa được commit → Push code lên GitHub trước

## 🔍 Nếu workflow chạy nhưng không có release

1. **Vào Actions** > Click vào workflow run vừa chạy
2. **Xem các steps:**
   - Step "Build executable" phải có dấu ✅
   - Step "Verify executable exists" phải có dấu ✅
   - Step "Create GitHub Release" phải có dấu ✅
3. **Nếu có step failed:**
   - Click vào step đó để xem logs
   - Tìm dòng có "[ERROR]" để biết lỗi

## Cách 2: Tạo Release bằng Git Tag

### Bước 1: Tạo tag
```bash
git tag v1.0.0
git push origin v1.0.0
```

### Bước 2: Workflow tự động chạy
- Khi push tag `v*`, workflow sẽ tự động:
  - Build executable
  - Tạo release với tag name
  - Upload file `.exe`

## Cách 3: Tạo Release thủ công trên GitHub

Nếu workflow không chạy, bạn có thể tạo release thủ công:

1. Vào **Releases** > **"Draft a new release"**
2. Chọn hoặc tạo tag mới (ví dụ: `v1.0.0`)
3. Điền release title và notes
4. Download file `.exe` từ **Actions** > **Artifacts**
5. Upload file `.exe` vào release
6. Click **"Publish release"**

## Lưu ý

- **Version format**: Nên dùng format `v1.0.0` (v + số)
- **File size**: File `.exe` có thể lớn (50-200MB), cần thời gian upload
- **Build time**: Build có thể mất 5-10 phút
- **Permissions**: Đảm bảo GITHUB_TOKEN có quyền tạo release

## Troubleshooting

### Workflow không chạy
- ✅ Kiểm tra file `.github/workflows/release.yml` có tồn tại
- ✅ Kiểm tra branch có workflow file
- ✅ Xem logs trong tab Actions

### Release không có file .exe
1. **Kiểm tra workflow đã chạy chưa:**
   - Vào **Actions** > Xem workflow run có thành công không
   - Step "Build executable" phải có dấu ✅
   - Step "Create GitHub Release" phải có dấu ✅

2. **Kiểm tra logs:**
   - Click vào workflow run
   - Xem step "Build executable" - phải thấy "[OK] Build successful!"
   - Xem step "Verify executable exists" - phải thấy "[OK] FoLive.exe ready"
   - Xem step "Create GitHub Release" - phải không có lỗi

3. **Nếu vẫn không thấy file:**
   - Kiểm tra release có được tạo không (vào tab Releases)
   - Nếu có release nhưng không có file → có thể file quá lớn hoặc upload failed
   - Thử tạo release lại với workflow

### Build failed
- Kiểm tra Python dependencies đã cài đầy đủ
- Kiểm tra FFmpeg đã được cài (có thể skip)
- Xem error logs trong Actions để biết lỗi cụ thể

### File .exe quá lớn
- GitHub có giới hạn 2GB cho mỗi file
- Nếu file > 2GB, cần optimize build hoặc split file

## Quick Start

**Cách nhanh nhất:**
1. Vào **Actions** > **Release Build** > **Run workflow**
2. Nhập version: `1.0.0`
3. Click **Run workflow**
4. Chờ 5-10 phút
5. Vào **Releases** để download file `.exe`


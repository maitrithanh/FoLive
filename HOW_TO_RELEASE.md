# 🚀 Hướng dẫn tạo Release

## Cách 1: Tạo Release qua GitHub Actions (Khuyến nghị)

### Bước 1: Vào GitHub Actions
1. Vào repository trên GitHub
2. Click tab **Actions**
3. Chọn workflow **"Release Build"** ở sidebar bên trái

### Bước 2: Chạy workflow
1. Click nút **"Run workflow"** (góc trên bên phải)
2. Chọn branch: `main` hoặc `master`
3. Nhập version: `1.0.0` (hoặc version bạn muốn)
4. Đảm bảo **"Create GitHub Release"** được bật (mặc định là true)
5. Click **"Run workflow"**

### Bước 3: Chờ workflow hoàn thành
- Workflow sẽ tự động:
  - Build file `.exe`
  - Tạo GitHub Release
  - Upload file `.exe` lên release

### Bước 4: Kiểm tra Release
1. Vào tab **Releases** trên GitHub
2. Bạn sẽ thấy release mới với file `FoLive.exe`

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
- Kiểm tra file `.github/workflows/release.yml` có tồn tại
- Kiểm tra branch có workflow file
- Xem logs trong tab Actions

### Release không có file .exe
- Kiểm tra step "Build executable" có thành công
- Kiểm tra step "Verify files before release"
- Xem logs để biết file có được tạo không

### Build failed
- Kiểm tra Python dependencies
- Kiểm tra FFmpeg đã được cài
- Xem error logs trong Actions

## Quick Start

**Cách nhanh nhất:**
1. Vào **Actions** > **Release Build** > **Run workflow**
2. Nhập version: `1.0.0`
3. Click **Run workflow**
4. Chờ 5-10 phút
5. Vào **Releases** để download file `.exe`


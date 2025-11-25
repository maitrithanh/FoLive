# GitHub Actions Workflows

Dự án này sử dụng GitHub Actions để tự động build, test và deploy.

## Workflows

### 1. CI/CD Pipeline (`.github/workflows/ci.yml`)

Workflow chính chạy trên mỗi push và pull request:

- **Test**: Chạy tests trên nhiều OS (Ubuntu, macOS, Windows) và Python versions (3.8-3.11)
- **Lint**: Kiểm tra code style với flake8 và black
- **Build**: Tạo executable với PyInstaller
- **Docker**: Build và push Docker image
- **Release**: Tự động tạo release khi có tag mới

### 2. Quick Test (`.github/workflows/test.yml`)

Workflow nhanh chạy trên mỗi push:

- Kiểm tra dependencies
- Test imports
- Kiểm tra FFmpeg

### 3. Release Build (`.github/workflows/release.yml`)

Workflow tạo release package:

- Build executable
- Tạo archive (tar.gz, zip)
- Tạo GitHub Release

## Cách sử dụng

### Chạy tests tự động

Chỉ cần push code lên GitHub, tests sẽ tự động chạy:

```bash
git add .
git commit -m "Add new feature"
git push origin main
```

### Tạo Release

1. Tạo tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. Hoặc sử dụng GitHub UI:
   - Vào Actions > Release Build
   - Click "Run workflow"
   - Nhập version number

### Xem kết quả

- Vào tab **Actions** trên GitHub repository
- Xem logs và kết quả của từng workflow run
- Download artifacts nếu có

## Secrets (Optional)

Để push Docker image, cần set secrets:

- `DOCKER_USERNAME`: Docker Hub username
- `DOCKER_PASSWORD`: Docker Hub password/token

Settings > Secrets and variables > Actions > New repository secret

## Badges

Thêm badges vào README.md (thay YOUR_USERNAME):

```markdown
![CI](https://github.com/YOUR_USERNAME/FoLive/workflows/CI%2FCD%20Pipeline/badge.svg)
![Tests](https://github.com/YOUR_USERNAME/FoLive/workflows/Quick%20Test/badge.svg)
```



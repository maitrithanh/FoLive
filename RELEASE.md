# 📦 Hướng dẫn tạo Release

## Tạo Release trên GitHub

### Cách 1: Tự động qua GitHub Actions (Khuyến nghị)

1. **Tạo tag:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Hoặc dùng GitHub UI:**
   - Vào **Actions** > **Release Build**
   - Click **Run workflow**
   - Nhập version (ví dụ: `1.0.0`)
   - Click **Run workflow**

3. **Workflow sẽ tự động:**
   - Build executable
   - Tạo installer bundle
   - Tạo GitHub Release
   - Upload files

### Cách 2: Tạo Release thủ công

1. **Build local:**
   ```cmd
   create_release.bat 1.0.0
   ```

2. **Tạo release trên GitHub:**
   - Vào **Releases** > **Draft a new release**
   - Chọn tag hoặc tạo tag mới
   - Upload file `FoLive-Windows-1.0.0.zip`
   - Điền release notes
   - Publish release

## Cấu trúc Release Package

```
FoLive-Windows-1.0.0.zip
├── FoLive.exe              # Standalone executable
├── install.bat             # Auto installer
├── FoLive-Windows-Bundle/  # Complete bundle
│   ├── FoLive.exe
│   ├── install.bat
│   ├── README.md
│   └── env.example
├── README.md               # Documentation
├── INSTALL.md              # Installation guide
└── env.example             # Config template
```

## Release Checklist

Trước khi tạo release:

- [ ] Code đã được test kỹ
- [ ] Version number đã được cập nhật
- [ ] README.md đã được cập nhật
- [ ] Changelog đã được viết
- [ ] Build thành công trên local
- [ ] Executable chạy được trên clean Windows
- [ ] FFmpeg auto-install hoạt động

## Version Numbering

Sử dụng [Semantic Versioning](https://semver.org/):

- **MAJOR.MINOR.PATCH** (ví dụ: 1.0.0)
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

## Release Notes Template

```markdown
## FoLive v1.0.0

### ✨ New Features
- Feature 1
- Feature 2

### 🐛 Bug Fixes
- Fix 1
- Fix 2

### 📝 Changes
- Change 1
- Change 2

### 📥 Installation
Download `FoLive-Windows-1.0.0.zip` và giải nén.
Chạy `install.bat` để cài đặt tự động.
```

## Troubleshooting

### Build fails trên GitHub Actions

- Kiểm tra logs trong Actions tab
- Đảm bảo tất cả dependencies đã được cài
- Test build local trước

### Executable không chạy

- Kiểm tra Windows Defender/Antivirus
- Test trên clean Windows VM
- Kiểm tra logs trong terminal

### Release không được tạo

- Kiểm tra GITHUB_TOKEN permissions
- Đảm bảo workflow có quyền tạo release
- Kiểm tra tag format (phải bắt đầu bằng `v`)


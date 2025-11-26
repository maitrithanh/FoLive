# 🚀 Hướng dẫn nhanh tạo Release

## ⚡ Cách tạo Release đầu tiên (3 bước)

### Bước 1: Vào GitHub Actions
1. Mở repository trên GitHub
2. Click tab **"Actions"** (ở trên cùng, bên cạnh Code, Issues...)

### Bước 2: Chạy workflow
1. Ở sidebar bên trái, tìm và click **"Release Build"**
2. Click nút **"Run workflow"** (dropdown màu xanh ở góc trên bên phải)
3. Chọn branch: `main` (hoặc `master`)
4. Nhập version: `1.0.0` (chỉ số, không cần chữ "v")
5. Click nút **"Run workflow"** (màu xanh)

### Bước 3: Chờ và kiểm tra
1. Chờ 5-10 phút (workflow đang build)
2. Vào tab **"Releases"** (bên cạnh tab Actions)
3. Bạn sẽ thấy release mới với file **FoLive.exe**

## 📸 Hình ảnh minh họa

```
GitHub Repository
├── Code (tab)
├── Issues (tab)
├── Actions (tab) ← Vào đây
│   └── Release Build (workflow) ← Chọn cái này
│       └── Run workflow (button) ← Click đây
└── Releases (tab) ← Kiểm tra kết quả ở đây
```

## ❓ FAQ

### Q: Không thấy nút "Run workflow"?
**A:** Đảm bảo bạn đã:
- Vào đúng tab **Actions**
- Chọn workflow **"Release Build"** ở sidebar
- Có quyền write trên repository

### Q: Workflow chạy nhưng không có release?
**A:** Kiểm tra:
- Vào **Actions** > Click vào workflow run vừa chạy
- Xem các steps có dấu ✅ không
- Step "Create GitHub Release" phải thành công
- Xem logs để biết lỗi cụ thể

### Q: Release có nhưng không có file .exe?
**A:** Có thể:
- Build failed → Xem step "Build executable"
- File quá lớn → GitHub giới hạn 2GB
- Upload failed → Xem logs step "Create GitHub Release"

### Q: Làm sao biết workflow đang chạy?
**A:** 
- Vào **Actions** tab
- Bạn sẽ thấy workflow run với status "In progress" (màu vàng)
- Khi xong sẽ chuyển thành "Success" (màu xanh) hoặc "Failed" (màu đỏ)

## 🎯 Checklist

Trước khi chạy workflow, đảm bảo:
- [ ] Code đã được push lên GitHub
- [ ] File `.github/workflows/release.yml` có trong repository
- [ ] Bạn có quyền write trên repository
- [ ] Đã commit file `FoLive.spec` (hoặc để workflow tự tạo)

## 🔧 Nếu vẫn không được

1. **Kiểm tra workflow file:**
   - Vào repository
   - Xem file `.github/workflows/release.yml` có tồn tại không
   - Kiểm tra syntax có đúng không

2. **Xem logs:**
   - Vào **Actions** > Click vào workflow run
   - Xem từng step có lỗi gì không
   - Copy error message để debug

3. **Test local:**
   - Clone repository về máy
   - Chạy: `python -m PyInstaller --clean --noconfirm FoLive.spec`
   - Xem có build được không

## 💡 Tip

Nếu muốn test nhanh, có thể:
1. Tạo tag local: `git tag v1.0.0`
2. Push tag: `git push origin v1.0.0`
3. Workflow sẽ tự động chạy và tạo release


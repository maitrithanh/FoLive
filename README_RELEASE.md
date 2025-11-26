# 🚨 HƯỚNG DẪN TẠO RELEASE - BẮT BUỘC ĐỌC!

## ⚠️ QUAN TRỌNG: Release KHÔNG tự động tạo!

**Workflow chỉ chạy khi bạn:**
1. ✅ **Chạy thủ công** qua GitHub Actions (khuyến nghị)
2. ✅ **Push tag** lên GitHub

**Release KHÔNG tự động xuất hiện!** Bạn phải chạy workflow trước!

---

## 🎯 CÁCH TẠO RELEASE (3 BƯỚC ĐƠN GIẢN)

### 📍 BƯỚC 1: Vào GitHub Actions

1. Mở repository trên GitHub (ví dụ: `https://github.com/YOUR_USERNAME/FoLive`)
2. Click tab **"Actions"** (ở trên cùng, bên cạnh Code, Issues...)
3. Ở sidebar bên trái, tìm và click **"Release Build"**

**Nếu không thấy "Release Build":**
- Đảm bảo file `.github/workflows/release.yml` đã được commit và push lên GitHub
- Refresh trang GitHub

### ▶️ BƯỚC 2: Chạy workflow

1. Bạn sẽ thấy nút **"Run workflow"** (dropdown màu xanh ở góc trên bên phải)
2. Click vào dropdown đó
3. Chọn branch: `main` (hoặc `master` - branch chính của bạn)
4. Nhập version: `1.0.0` (chỉ số, KHÔNG cần chữ "v")
5. Click nút **"Run workflow"** (màu xanh)

**Làm sao biết đang chạy?**
- Vào tab **Actions**
- Bạn sẽ thấy workflow run với status "In progress" (màu vàng)
- Khi xong sẽ chuyển thành "Success" (màu xanh) hoặc "Failed" (màu đỏ)

### ⏳ BƯỚC 3: Chờ và kiểm tra

1. **Chờ 5-10 phút** (workflow đang build file .exe)
2. Click tab **"Releases"** trên GitHub (bên cạnh tab Actions)
3. Bạn sẽ thấy release mới với tên "Release v1.0.0"
4. Trong phần **Assets**, bạn sẽ thấy file **FoLive.exe**
5. Click vào **FoLive.exe** để download!

---

## 🔍 KIỂM TRA NẾU KHÔNG THẤY RELEASE

### 1. Kiểm tra workflow đã chạy chưa?

**Vào Actions tab:**
- Bạn có thấy workflow run nào không?
- Nếu không có → Workflow chưa được chạy → Làm theo BƯỚC 1-2 ở trên

### 2. Kiểm tra workflow có thành công không?

**Vào Actions > Click vào workflow run vừa chạy:**
- Tất cả steps phải có dấu ✅ (màu xanh)
- Nếu có step ❌ (màu đỏ) → Click vào step đó để xem lỗi

**Các steps quan trọng:**
- ✅ "Build executable" → Phải thấy "[OK] Build successful!"
- ✅ "Verify executable exists" → Phải thấy "[OK] FoLive.exe ready"
- ✅ "Create GitHub Release" → Phải không có lỗi

### 3. Kiểm tra release có được tạo không?

**Vào Releases tab:**
- Bạn có thấy release nào không?
- Nếu có release nhưng không có file .exe → Xem phần "Release không có file .exe" bên dưới

### 4. Kiểm tra logs chi tiết

**Vào Actions > Click vào workflow run > Click vào step "Create GitHub Release":**
- Tìm dòng có "[ERROR]" hoặc "Error"
- Copy error message để debug

---

## ❓ FAQ - CÁC VẤN ĐỀ THƯỜNG GẶP

### Q: Không thấy nút "Run workflow"?

**A:** Có thể do:
- Bạn chưa vào đúng workflow → Đảm bảo đã chọn "Release Build" ở sidebar
- Bạn không có quyền write → Cần quyền write trên repository
- Workflow file chưa được commit → Push code lên GitHub trước

### Q: Workflow chạy nhưng không có release?

**A:** Kiểm tra:
1. Vào **Actions** > Click vào workflow run vừa chạy
2. Xem các steps có dấu ✅ không
3. Step "Create GitHub Release" phải thành công
4. Xem logs để biết lỗi cụ thể

### Q: Release có nhưng không có file .exe?

**A:** Có thể:
- Build failed → Xem step "Build executable"
- File quá lớn → GitHub giới hạn 2GB
- Upload failed → Xem logs step "Create GitHub Release"
- File path sai → Kiểm tra `files: dist/FoLive.exe` trong workflow

### Q: Làm sao biết workflow đang chạy?

**A:** 
- Vào **Actions** tab
- Bạn sẽ thấy workflow run với status "In progress" (màu vàng)
- Khi xong sẽ chuyển thành "Success" (màu xanh) hoặc "Failed" (màu đỏ)

### Q: Workflow failed với lỗi gì?

**A:** 
- Click vào workflow run
- Click vào step failed (màu đỏ)
- Xem logs để biết lỗi cụ thể
- Thường gặp:
  - Build failed → Thiếu dependencies
  - File not found → Path sai
  - Permission denied → Không có quyền tạo release

---

## 🎯 CÁCH 2: TẠO RELEASE BẰNG GIT TAG

Nếu không muốn dùng GitHub Actions, bạn có thể tạo release bằng Git tag:

```bash
# 1. Tạo tag local
git tag v1.0.0

# 2. Push tag lên GitHub
git push origin v1.0.0
```

**Workflow sẽ tự động chạy** khi bạn push tag `v*` lên GitHub!

---

## ✅ CHECKLIST TRƯỚC KHI CHẠY WORKFLOW

Trước khi chạy workflow, đảm bảo:
- [ ] Code đã được push lên GitHub
- [ ] File `.github/workflows/release.yml` có trong repository
- [ ] Bạn có quyền write trên repository
- [ ] Đã commit file `FoLive.spec` (hoặc để workflow tự tạo)
- [ ] File `requirements.txt` có đầy đủ dependencies

---

## 🔧 NẾU VẪN KHÔNG ĐƯỢC

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

4. **Kiểm tra permissions:**
   - Đảm bảo `GITHUB_TOKEN` có quyền tạo release
   - Mặc định GitHub Actions có quyền này, nhưng nếu dùng custom token thì cần check

---

## 💡 TIP

**Cách nhanh nhất để test:**
1. Vào **Actions** > **Release Build** > **Run workflow**
2. Nhập version: `1.0.0`
3. Click **Run workflow**
4. Chờ 5-10 phút
5. Vào **Releases** để download file `.exe`

---

## 📞 CẦN GIÚP ĐỠ?

Nếu vẫn không được, hãy:
1. Copy error message từ logs
2. Tạo issue trên GitHub với error message đó
3. Hoặc check lại các bước ở trên

---

**NHỚ: Release KHÔNG tự động tạo! Bạn phải chạy workflow trước!** 🚀


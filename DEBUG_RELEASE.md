# 🔍 Debug Release Workflow

## Vấn đề: Workflow chạy OK nhưng không có release

Nếu workflow chạy thành công (tất cả steps đều ✅) nhưng không thấy release, hãy làm theo các bước sau:

## 🔎 Bước 1: Kiểm tra logs chi tiết

1. Vào **Actions** > Click vào workflow run vừa chạy
2. Click vào step **"Create GitHub Release"**
3. Tìm các dòng quan trọng:
   - `Creating release...`
   - `Uploading asset...`
   - `Release created successfully!`
   - Hoặc bất kỳ error message nào

## 🔎 Bước 2: Kiểm tra step "Verify release created"

Step này sẽ kiểm tra xem release đã được tạo chưa:

- ✅ Nếu thấy `[OK] Release created successfully!` → Release đã được tạo
- ❌ Nếu thấy `[ERROR] Failed to verify release` → Có lỗi xảy ra

**Xem logs để biết:**
- Tag name là gì?
- Release URL là gì?
- Có assets không?

## 🔎 Bước 3: Kiểm tra tag name

Tag name phải đúng format:
- ✅ `v1.0.0` (có chữ "v" ở đầu)
- ❌ `1.0.0` (không có "v")

**Workflow sẽ tự động thêm "v" nếu bạn nhập `1.0.0`**

## 🔎 Bước 4: Kiểm tra permissions

Workflow cần quyền `contents: write` để tạo release. Đã được thêm vào workflow:

```yaml
permissions:
  contents: write
```

## 🔎 Bước 5: Kiểm tra file path

File `.exe` phải ở đúng path: `dist/FoLive.exe`

**Kiểm tra trong step "Verify executable exists":**
- Phải thấy `[OK] FoLive.exe ready for release`
- Phải có path và size

## 🔎 Bước 6: Kiểm tra GitHub API

Nếu vẫn không thấy release, có thể kiểm tra trực tiếp qua GitHub API:

```bash
# Thay YOUR_USERNAME và YOUR_REPO
curl -H "Authorization: token YOUR_TOKEN" \
  https://api.github.com/repos/YOUR_USERNAME/YOUR_REPO/releases
```

Hoặc vào trực tiếp trên GitHub:
- Vào repository
- Click tab **Releases**
- Xem có release nào không (kể cả draft)

## 🔎 Bước 7: Kiểm tra tag đã được tạo chưa

1. Vào repository trên GitHub
2. Click vào phần **"X tags"** (bên cạnh branches)
3. Xem tag có được tạo không

**Nếu tag không có → Release sẽ không được tạo**

## 🔎 Bước 8: Thử tạo release thủ công

Nếu workflow vẫn không tạo được release, có thể thử:

1. Vào **Releases** > **"Draft a new release"**
2. Chọn tag (hoặc tạo tag mới)
3. Upload file `.exe` từ **Actions** > **Artifacts**
4. Click **"Publish release"**

## 🐛 Các lỗi thường gặp

### Lỗi 1: "Tag already exists"

**Nguyên nhân:** Tag đã tồn tại từ lần chạy trước

**Giải pháp:**
- Xóa tag cũ trên GitHub
- Hoặc dùng version mới (ví dụ: `1.0.1`)

### Lỗi 2: "Permission denied"

**Nguyên nhân:** GITHUB_TOKEN không có quyền

**Giải pháp:**
- Đảm bảo workflow có `permissions: contents: write`
- Kiểm tra repository settings > Actions > General > Workflow permissions

### Lỗi 3: "File not found"

**Nguyên nhân:** File `.exe` không được build thành công

**Giải pháp:**
- Kiểm tra step "Build executable" có thành công không
- Xem logs để biết lỗi cụ thể

### Lỗi 4: "Release created but no assets"

**Nguyên nhân:** File upload failed

**Giải pháp:**
- Kiểm tra file size (GitHub giới hạn 2GB)
- Kiểm tra file path có đúng không
- Xem logs step "Create GitHub Release"

## ✅ Checklist Debug

- [ ] Workflow đã chạy thành công (tất cả steps ✅)
- [ ] Step "Verify executable exists" có `[OK] FoLive.exe ready`
- [ ] Step "Determine tag name" có `[OK] Tag name: vX.X.X`
- [ ] Step "Create GitHub Release" không có error
- [ ] Step "Verify release created" có `[OK] Release created successfully!`
- [ ] Tag đã được tạo trên GitHub
- [ ] Release đã được tạo trên GitHub (tab Releases)
- [ ] File `.exe` có trong Assets của release

## 📞 Nếu vẫn không được

1. Copy toàn bộ logs từ step "Create GitHub Release"
2. Copy logs từ step "Verify release created"
3. Tạo issue trên GitHub với logs đó
4. Hoặc check lại các bước ở trên

---

**Lưu ý:** Workflow mới đã được cập nhật với:
- ✅ Permissions đúng
- ✅ Tag name logic đúng
- ✅ Verify step để kiểm tra release
- ✅ Logging chi tiết hơn

Hãy chạy lại workflow và xem logs để biết chính xác lỗi ở đâu!


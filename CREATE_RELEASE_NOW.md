# 🚀 TẠO RELEASE NGAY BÂY GIỜ

## ⚠️ VẤN ĐỀ HIỆN TẠI

Repository của bạn **KHÔNG CÓ RELEASE NÀO** trên GitHub!

## ✅ GIẢI PHÁP: 2 CÁCH TẠO RELEASE

### CÁCH 1: Push Tag (Đơn giản nhất - Giống snapvideo) ⭐

**Bước 1: Push code lên GitHub (nếu chưa push)**
```bash
git add .
git commit -m "Update release workflow"
git push origin main
```

**Bước 2: Tạo và push tag**
```bash
git tag v1.0.0
git push origin v1.0.0
```

**Bước 3: Chờ workflow chạy**
- Vào **Actions** trên GitHub
- Bạn sẽ thấy workflow "Release Build" tự động chạy
- Chờ 5-10 phút
- Vào **Releases** → Sẽ thấy release mới với file **FoLive.exe**!

### CÁCH 2: Chạy Workflow Thủ Công

**Bước 1: Push code lên GitHub (nếu chưa push)**
```bash
git add .
git commit -m "Update release workflow"
git push origin main
```

**Bước 2: Chạy workflow trên GitHub**
1. Vào https://github.com/maitrithanh/FoLive
2. Click tab **Actions**
3. Ở sidebar bên trái, tìm và click **"Release Build"**
4. Click nút **"Run workflow"** (dropdown màu xanh)
5. Chọn branch: `main`
6. Nhập version: `1.0.0` (chỉ số, không cần "v")
7. Click **"Run workflow"**

**Bước 3: Chờ workflow chạy**
- Workflow sẽ:
  1. Build executable
  2. Tạo tag `v1.0.0`
  3. Push tag lên GitHub
  4. Tag push sẽ trigger workflow lại
  5. Lần chạy thứ 2 sẽ tạo release

**Bước 4: Kiểm tra release**
- Vào tab **Releases** trên GitHub
- Sẽ thấy release mới với file **FoLive.exe**!

## 🔍 KIỂM TRA WORKFLOW ĐÃ ĐƯỢC PUSH CHƯA

Chạy lệnh này để kiểm tra:
```bash
git log origin/main --oneline -- .github/workflows/release.yml | head -5
```

Nếu không thấy gì → Workflow chưa được push → Cần push code!

## 📋 CHECKLIST

Trước khi tạo release, đảm bảo:
- [ ] Code đã được push lên GitHub
- [ ] File `.github/workflows/release.yml` có trong repository
- [ ] Bạn có quyền write trên repository
- [ ] Đã commit file `FoLive.spec` (hoặc để workflow tự tạo)

## 🎯 KHUYẾN NGHỊ

**Dùng CÁCH 1 (Push Tag)** vì:
- ✅ Đơn giản hơn
- ✅ Giống cách snapvideo làm
- ✅ Chắc chắn hoạt động
- ✅ Không cần chờ workflow chạy 2 lần

## 🐛 NẾU VẪN KHÔNG THẤY RELEASE

1. **Kiểm tra workflow đã chạy chưa:**
   - Vào **Actions** → Xem có workflow run nào không
   - Nếu không có → Workflow chưa được trigger

2. **Kiểm tra workflow có thành công không:**
   - Click vào workflow run
   - Xem các steps có dấu ✅ không
   - Step "Create GitHub Release" phải thành công

3. **Kiểm tra tag đã được tạo chưa:**
   - Vào repository → Click "X tags"
   - Xem tag có được tạo không

4. **Kiểm tra logs:**
   - Click vào step "Create GitHub Release"
   - Xem có lỗi gì không

## 💡 TIP

**Cách nhanh nhất:**
```bash
# 1. Push code (nếu chưa)
git push origin main

# 2. Tạo và push tag
git tag v1.0.0
git push origin v1.0.0

# 3. Chờ 5-10 phút
# 4. Vào Releases trên GitHub → Sẽ thấy release!
```

---

**Hãy thử ngay và cho tôi biết kết quả!** 🚀


# 🔧 Git Commands Quick Reference

## 📦 Release Workflow

### Full Release Process (Copy & Paste)

```bash
# 1. Kiểm tra trạng thái
git status

# 2. Pull code mới nhất
git pull origin main

# 3. Thêm tất cả thay đổi
git add .

# 4. Commit (thay v1.0.1 bằng version của bạn)
git commit -m "Update for release v1.0.1"

# 5. Push code
git push origin main

# 6. Tạo tag
git tag v1.0.1

# 7. Push tag
git push origin v1.0.1
```

## 🏷️ Tag Management

### Tạo tag
```bash
# Tag đơn giản
git tag v1.0.1

# Tag với message
git tag -a v1.0.1 -m "Release v1.0.1: Add new features"
```

### Push tag
```bash
# Push 1 tag
git push origin v1.0.1

# Push tất cả tags
git push origin --tags
```

### Xem tags
```bash
# Xem tất cả tags
git tag

# Xem tags với pattern
git tag -l "v1.0.*"

# Xem chi tiết tag
git show v1.0.1
```

### Xóa tag
```bash
# Xóa tag local
git tag -d v1.0.1

# Xóa tag trên GitHub
git push origin :refs/tags/v1.0.1

# Xóa cả local và remote
git tag -d v1.0.1 && git push origin :refs/tags/v1.0.1
```

## 📝 Commit & Push

### Commit
```bash
# Commit với message
git commit -m "Your message here"

# Commit tất cả (không cần add)
git commit -am "Your message here"

# Commit với message dài
git commit -m "Title" -m "Description line 1" -m "Description line 2"
```

### Push
```bash
# Push lên main branch
git push origin main

# Push và set upstream
git push -u origin main

# Force push (cẩn thận!)
git push --force origin main
```

## 🔍 Kiểm tra & Xem

### Status
```bash
# Trạng thái hiện tại
git status

# Trạng thái ngắn gọn
git status -s
```

### Diff
```bash
# Xem thay đổi chưa staged
git diff

# Xem thay đổi đã staged
git diff --staged

# Xem thay đổi của 1 file
git diff path/to/file.cs
```

### Log
```bash
# Log đơn giản
git log

# Log 1 dòng
git log --oneline

# Log với graph
git log --oneline --graph

# Log với tags
git log --oneline --decorate

# Log của 1 file
git log path/to/file.cs
```

## 🔄 Pull & Fetch

### Pull
```bash
# Pull từ main
git pull origin main

# Pull và rebase
git pull --rebase origin main
```

### Fetch
```bash
# Fetch tất cả
git fetch

# Fetch từ remote cụ thể
git fetch origin

# Fetch tags
git fetch --tags
```

## 🧹 Cleanup

### Xóa file chưa track
```bash
# Xem file sẽ bị xóa
git clean -n

# Xóa file chưa track
git clean -f

# Xóa cả thư mục
git clean -fd
```

### Reset
```bash
# Unstage tất cả
git reset

# Unstage 1 file
git reset path/to/file.cs

# Reset về commit trước (giữ thay đổi)
git reset --soft HEAD~1

# Reset về commit trước (xóa thay đổi)
git reset --hard HEAD~1
```

## 🌿 Branch (Nếu cần)

### Tạo branch
```bash
# Tạo branch mới
git branch feature-name

# Tạo và chuyển sang branch
git checkout -b feature-name

# Hoặc dùng switch (Git 2.23+)
git switch -c feature-name
```

### Chuyển branch
```bash
# Chuyển branch
git checkout branch-name

# Hoặc dùng switch
git switch branch-name
```

### Xóa branch
```bash
# Xóa branch local
git branch -d branch-name

# Xóa branch trên GitHub
git push origin --delete branch-name
```

## 🔗 Remote

### Xem remote
```bash
# Xem remote URLs
git remote -v
```

### Thêm remote
```bash
git remote add origin https://github.com/maitrithanh/FoLive.git
```

## ⚡ One-liners

### Quick release (thay v1.0.1)
```bash
git add . && git commit -m "Release v1.0.1" && git push origin main && git tag v1.0.1 && git push origin v1.0.1
```

### Quick commit & push
```bash
git add . && git commit -m "Update" && git push origin main
```

### Check tag exists
```bash
git tag | grep v1.0.1
```

### Last commit message
```bash
git log -1 --pretty=%B
```

---

**💡 Tip:** Copy các commands vào terminal và thay `v1.0.1` bằng version của bạn!




#!/bin/bash
# Script để tạo release nhanh

echo "🚀 Tạo release cho FoLive..."
echo ""

# Kiểm tra đã commit chưa
if [ -n "$(git status --porcelain)" ]; then
    echo "⚠️  Có thay đổi chưa commit!"
    echo "   Hãy commit trước:"
    echo "   git add ."
    echo "   git commit -m 'Update release workflow'"
    exit 1
fi

# Push code lên GitHub
echo "📤 Pushing code to GitHub..."
git push origin main

# Tạo và push tag
echo ""
echo "🏷️  Creating tag v1.0.0..."
git tag v1.0.0
git push origin v1.0.0

echo ""
echo "✅ Done!"
echo ""
echo "📋 Tiếp theo:"
echo "   1. Vào https://github.com/maitrithanh/FoLive/actions"
echo "   2. Chờ workflow 'Release Build' chạy xong (5-10 phút)"
echo "   3. Vào https://github.com/maitrithanh/FoLive/releases"
echo "   4. Sẽ thấy release mới với file FoLive.exe!"


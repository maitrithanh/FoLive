#!/bin/bash
# FoLive - Push Code and Create Release
# This script pushes code to GitHub and creates a release

set -e

echo "========================================"
echo "FoLive - Push and Release"
echo "========================================"
echo ""

# Get version from user
read -p "Enter version number (e.g., 1.0.0): " VERSION
if [ -z "$VERSION" ]; then
    echo "ERROR: Version is required!"
    exit 1
fi

# Remove 'v' prefix if exists
TAG_NAME="$VERSION"
if [[ "$TAG_NAME" == v* ]]; then
    TAG_NAME="${TAG_NAME#v}"
fi
TAG_NAME="v$TAG_NAME"

echo ""
echo "Version: $VERSION"
echo "Tag: $TAG_NAME"
echo ""

# Check git status
echo "[1/4] Checking git status..."
git status --short

# Ask for commit message
read -p "Enter commit message (or press Enter for default): " COMMIT_MSG
if [ -z "$COMMIT_MSG" ]; then
    COMMIT_MSG="Update code and prepare release $TAG_NAME"
fi

echo ""
echo "[2/4] Committing changes..."
git add .
if git diff --staged --quiet; then
    echo "WARNING: Nothing to commit"
else
    git commit -m "$COMMIT_MSG"
fi

echo ""
echo "[3/4] Pushing to GitHub..."
git push origin main || {
    echo "ERROR: Push failed!"
    exit 1
}

echo ""
echo "[4/4] Creating and pushing tag..."
if git rev-parse "$TAG_NAME" >/dev/null 2>&1; then
    echo "WARNING: Tag $TAG_NAME already exists"
    read -p "Delete and recreate? (y/n): " RECREATE
    if [ "$RECREATE" = "y" ]; then
        git tag -d "$TAG_NAME"
        git push origin :refs/tags/"$TAG_NAME" || true
    else
        echo "Skipping tag creation"
        exit 0
    fi
fi

git tag -a "$TAG_NAME" -m "Release $TAG_NAME"
git push origin "$TAG_NAME" || {
    echo "ERROR: Failed to push tag!"
    exit 1
}

echo ""
echo "========================================"
echo "[OK] Success!"
echo "========================================"
echo ""
echo "Code pushed to: origin/main"
echo "Tag created: $TAG_NAME"
echo ""
echo "GitHub Actions will automatically:"
echo "  1. Build C# application"
echo "  2. Create Windows installer"
echo "  3. Create GitHub Release"
echo "  4. Upload FoLive.exe and FoLive-Setup.exe"
echo ""
echo "Check progress at:"
echo "  https://github.com/maitrithanh/FoLive/actions"
echo ""
echo "Release will be available at:"
echo "  https://github.com/maitrithanh/FoLive/releases"
echo ""

# Open browser (macOS/Linux)
if command -v open >/dev/null 2>&1; then
    sleep 2
    open "https://github.com/maitrithanh/FoLive/actions"
elif command -v xdg-open >/dev/null 2>&1; then
    sleep 2
    xdg-open "https://github.com/maitrithanh/FoLive/actions"
fi



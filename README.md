# 🎥 FoLive - Công cụ Livestream 24/7 đa luồng

Phần mềm livestream đa luồng lên YouTube với nhiều tính năng mạnh mẽ.

**🆕 Đã chuyển đổi sang C# (.NET 8) với WPF GUI - Ổn định và hiệu năng tốt hơn!**

## ✨ Tính năng

### 📹 Nguồn livestream đa dạng
- ✅ Hỗ trợ nhiều định dạng: **mp4, mov, mkv, avi, flv, webm, m4v, wmv**
- ✅ Livestream từ **file video** có sẵn
- ✅ Livestream từ **video YouTube**
- ✅ Livestream từ **playlist YouTube**
- ✅ Livestream từ **Facebook** video
- ✅ Livestream **quay màn hình** máy tính
- ✅ **Hỗ trợ 1000+ nguồn** qua yt-dlp (YouTube, Facebook, TikTok, Instagram, Twitter, và nhiều hơn nữa)
- ✅ Xem danh sách đầy đủ: [yt-dlp Supported Sites](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md)

### 🎬 Xử lý video nâng cao
- ✅ **Tăng tốc video** gốc (0.5x - 3.0x)
- ✅ Thêm **intro, outro** video
- ✅ **Tăng âm lượng** (0% - 200%)
- ✅ Điều chỉnh **độ sáng** (-100 đến +100)
- ✅ **Chèn chữ** (text overlay) lên video
- ✅ Hỗ trợ **render FFmpeg** khi livestream (bật effects)
- ✅ Livestream **không render** siêu nhẹ (direct copy)
- ✅ **Lặp lại vô hạn** video (24/7 mode)

### 🔄 Livestream 24/7
- ✅ **Lặp lại vô hạn** livestream 24/24 (loop mode)
- ✅ **Không giới hạn** số luồng
- ✅ **Livestream song song** nhiều luồng (multi-threading)
- ✅ Hiển thị **tình trạng luồng** real-time (auto-refresh mỗi 3s)
- ✅ Quản lý **unlimited streams** cùng lúc

### 🖥️ Giao diện Desktop
- ✅ Giao diện Windows Desktop **trực quan, dễ sử dụng** (WPF)
- ✅ Quản lý nhiều stream cùng lúc
- ✅ Theo dõi trạng thái real-time
- ✅ Cấu hình nâng cao cho từng stream
- ✅ Không cần web browser

## 🚀 Cài đặt

### ⚡ Download từ GitHub Releases (Khuyến nghị)

**Download từ GitHub Releases:**

1. Vào [Releases](https://github.com/maitrithanh/FoLive/releases)
2. Download `FoLive-Setup.exe` (Windows Installer)
3. Chạy installer và làm theo wizard

✅ **Không cần cài .NET runtime!** (Self-contained)  
✅ **Windows Desktop Application** - Chạy như ứng dụng thông thường

### 📦 Build từ source (Development)

**Yêu cầu:**
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** (khuyến nghị) hoặc **VS Code**
- **FFmpeg** - `winget install ffmpeg`

**Build:**

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project FoLive.GUI/FoLive.GUI.csproj

# Publish (tạo .exe)
dotnet publish FoLive.GUI/FoLive.GUI.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

**Output:** `FoLive.GUI/bin/Release/net8.0-windows/win-x64/publish/FoLive.exe`

## 📖 Sử dụng

### Khởi chạy ứng dụng

**Windows:**
- Double-click `FoLive.exe` (nếu đã build)
- Hoặc từ Start Menu (nếu đã cài đặt)

**Development:**
```bash
dotnet run --project FoLive.GUI/FoLive.GUI.csproj
```

### Tạo stream mới

1. Mở ứng dụng FoLive
2. Click nút **"➕ Add New Stream"**
3. Điền thông tin cơ bản:
   - **Stream ID**: Tên định danh cho stream
   - **Source Type**: Chọn File, YouTube, Playlist, Facebook, URL, hoặc Screen
   - **Source**: Đường dẫn file hoặc URL
   - **Stream URL**: RTMP URL (ví dụ: `rtmp://a.rtmp.youtube.com/live2`)
   - **Stream Key**: Key từ YouTube Studio
4. (Tùy chọn) Mở **Advanced Settings** để cấu hình:
   - Loop video (24/7)
   - Speed, Volume, Brightness
   - Text overlay
   - Intro/Outro videos
   - Bitrate
5. Click **"Add"**

### Quản lý streams

- **Start**: Click nút "Start" trên stream
- **Stop**: Click nút "Stop" trên stream đang chạy
- **Delete**: Click nút "Delete" để xóa stream

### Real-time Status

- Stream status tự động cập nhật mỗi 3 giây
- Hiển thị trạng thái: Idle, Starting, Running, Stopping, Stopped, Error

## 🎛️ Cấu hình nâng cao

### Video Processing

- **Bitrate**: Độ phân giải bitrate (mặc định: 2500k)
- **Tốc độ**: Tăng tốc video (1.0 = bình thường, 2.0 = nhanh gấp đôi)
- **Âm lượng**: Điều chỉnh âm lượng (1.0 = 100%)
- **Độ sáng**: Điều chỉnh độ sáng (-100 đến 100)
- **Chèn chữ**: Thêm text overlay lên video

### Stream Settings

- **Lặp lại vô hạn**: Bật để stream 24/7
- **Preset**: FFmpeg preset (veryfast, fast, medium, slow)
- **Resolution**: Độ phân giải output

## 🏗️ Cấu trúc Project

```
FoLive/
├── FoLive.sln                    # Solution file
├── FoLive.Core/                  # Core business logic
│   ├── Models/
│   │   ├── Stream.cs
│   │   └── StreamStatus.cs
│   └── Services/
│       ├── StreamManager.cs
│       ├── FFmpegService.cs
│       └── SourceHandlerService.cs
├── FoLive.GUI/                   # WPF Application
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   └── AddStreamDialog.xaml
│   └── App.xaml
└── FoLive.iss                    # Inno Setup installer
```

## 🔧 Troubleshooting

### FFmpeg không tìm thấy
- Đảm bảo FFmpeg đã được cài đặt và có trong PATH
- Cài đặt: `winget install ffmpeg` hoặc `choco install ffmpeg`

### Stream không kết nối được
- Kiểm tra Stream Key và Stream URL
- Đảm bảo firewall không chặn kết nối RTMP
- Kiểm tra log trong ứng dụng

### YouTube không nhận stream
- Đảm bảo stream key còn hiệu lực
- Kiểm tra bitrate không quá cao (khuyến nghị: 2500-4000k)
- Đảm bảo video có audio track

### Build errors
- Kiểm tra .NET 8 SDK đã được cài: `dotnet --version`
- Restore packages: `dotnet restore`
- Clear cache: `dotnet nuget locals all --clear`

## 📄 License

MIT License

## 🤝 Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng tạo issue hoặc pull request.

## 🔄 CI/CD

Dự án sử dụng GitHub Actions để tự động build và release:

- **Release Build**: Tự động build khi push tag `v*`
- **Workflow**: Build C# application, tạo installer, upload lên Releases

### Tạo Release

1. **Push tag:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Hoặc chạy workflow thủ công:**
   - Vào **Actions** > **Release Build** > **Run workflow**
   - Nhập version: `1.0.0`
   - Workflow sẽ tự động build và tạo release

## 📧 Liên hệ

Nếu có vấn đề hoặc câu hỏi, vui lòng tạo issue trên GitHub.

---

**FoLive** - Công cụ livestream 24/7 mạnh mẽ và dễ sử dụng! 🚀

**Tech Stack:** C# (.NET 8), WPF, FFmpeg

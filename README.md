# 🎥 FoLive - Công cụ Livestream 24/7 đa luồng

Phần mềm livestream đa luồng lên YouTube với nhiều tính năng mạnh mẽ.

## ✨ Tính năng

### 📹 Nguồn livestream đa dạng
- ✅ Hỗ trợ nhiều định dạng: **mp4, mov, mkv, avi, flv, webm**
- ✅ Livestream từ **file video** có sẵn
- ✅ Livestream từ **video YouTube**
- ✅ Livestream từ **playlist YouTube**
- ✅ Livestream **quay màn hình** máy tính
- ✅ Hỗ trợ tất cả nguồn được liệt kê

### 🎬 Xử lý video nâng cao
- ✅ **Tăng tốc video** gốc
- ✅ Thêm **intro, outro**
- ✅ **Tăng âm lượng**, điều chỉnh độ sáng
- ✅ **Chèn chữ** (text overlay)
- ✅ Hỗ trợ **render FFmpeg** khi livestream
- ✅ Livestream **không render** siêu nhẹ

### 🔄 Livestream 24/7
- ✅ **Lặp lại vô hạn** livestream 24/24
- ✅ **Không giới hạn** số luồng
- ✅ **Livestream song song** nhiều luồng
- ✅ Hiển thị **tình trạng luồng** real-time

### 🖥️ Giao diện Desktop
- ✅ Giao diện Windows Desktop **trực quan, dễ sử dụng**
- ✅ Quản lý nhiều stream cùng lúc
- ✅ Theo dõi trạng thái real-time
- ✅ Cấu hình nâng cao cho từng stream
- ✅ Không cần web browser

## 🚀 Cài đặt

### ⚡ Cài đặt tự động (Khuyến nghị - Không cần Python!)

**Download từ GitHub Releases:**
1. Vào [Releases](https://github.com/YOUR_USERNAME/FoLive/releases)
2. Download `FoLive-Windows-*.zip`
3. Giải nén và chạy `install.bat`

**Windows:**
```cmd
# Giải nén file zip
# Chạy installer
install.bat

# Hoặc chạy trực tiếp
FoLive.exe
```

✅ **Không cần cài Python!** Tất cả dependencies đã được bundle sẵn.
✅ **Windows Desktop Application** - Chạy như ứng dụng thông thường

### 📦 Build từ source (Development)

Nếu muốn build từ source:

```bash
# Cài đặt dependencies
pip install -r requirements.txt
pip install pyinstaller

# Build installer
python build_installer.py
```

### Yêu cầu hệ thống (chỉ khi build từ source)

1. **Windows 10/11**
2. **Python 3.8+**
3. **FFmpeg** - Cài đặt:
   ```cmd
   # Windows
   winget install ffmpeg
   # hoặc
   choco install ffmpeg
   # hoặc tải từ https://ffmpeg.org/download.html
   ```

### Cài đặt dependencies

```bash
# Clone hoặc tải project
cd FoLive

# Tạo virtual environment (khuyến nghị)
python3 -m venv venv
source venv/bin/activate  # Trên Windows: venv\Scripts\activate

# Cài đặt packages
pip install -r requirements.txt
```

### Cấu hình

1. Copy file `.env.example` thành `.env`:
   ```bash
   cp .env.example .env
   ```

2. Chỉnh sửa file `.env` với thông tin của bạn:
   ```env
   YOUTUBE_STREAM_KEY=your_youtube_stream_key_here
   YOUTUBE_STREAM_URL=rtmp://a.rtmp.youtube.com/live2
   ```

## 📖 Sử dụng

### Khởi chạy ứng dụng

```bash
python app.py
```

Ứng dụng sẽ chạy tại: `http://localhost:5000`

### Tạo stream mới

1. Mở trình duyệt và truy cập `http://localhost:5000`
2. Điền thông tin:
   - **Stream ID**: Tên định danh cho stream
   - **Loại nguồn**: Chọn file, YouTube, playlist, hoặc screen
   - **Nguồn**: Đường dẫn file hoặc URL
   - **Stream URL**: RTMP URL (ví dụ: `rtmp://a.rtmp.youtube.com/live2`)
   - **Stream Key**: Key từ YouTube Studio
3. Cấu hình nâng cao (tùy chọn):
   - Bitrate, tốc độ phát, âm lượng, độ sáng
   - Chèn chữ, lặp lại vô hạn
4. Click **"Tạo Stream"**

### Quản lý streams

- **Bắt đầu**: Click nút "Bắt đầu" trên stream
- **Dừng**: Click nút "Dừng" trên stream đang chạy
- **Xóa**: Click nút "Xóa" để xóa stream

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

## 📝 API Endpoints

### Streams
- `GET /api/streams` - Lấy danh sách streams
- `POST /api/streams` - Tạo stream mới
- `GET /api/streams/<id>` - Lấy thông tin stream
- `POST /api/streams/<id>/start` - Bắt đầu stream
- `POST /api/streams/<id>/stop` - Dừng stream
- `DELETE /api/streams/<id>` - Xóa stream
- `PUT /api/streams/<id>/config` - Cập nhật config

### Utilities
- `POST /api/sources/validate` - Validate source
- `POST /api/video/info` - Lấy thông tin video
- `GET /api/system/check` - Kiểm tra hệ thống

## 🔧 Troubleshooting

### FFmpeg không tìm thấy
- Đảm bảo FFmpeg đã được cài đặt và có trong PATH
- Hoặc chỉ định đường dẫn trong file `.env`:
  ```
  FFMPEG_PATH=/usr/local/bin/ffmpeg
  ```

### Stream không kết nối được
- Kiểm tra Stream Key và Stream URL
- Đảm bảo firewall không chặn kết nối RTMP
- Kiểm tra log trong terminal để xem lỗi chi tiết

### YouTube không nhận stream
- Đảm bảo stream key còn hiệu lực
- Kiểm tra bitrate không quá cao (khuyến nghị: 2500-4000k)
- Đảm bảo video có audio track

## 📄 License

MIT License

## 🤝 Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng tạo issue hoặc pull request.

## 🔄 CI/CD và Testing

Dự án sử dụng GitHub Actions để tự động build và test:

- **CI Pipeline**: Tự động chạy tests trên mỗi push/PR
- **Multi-platform**: Test trên Ubuntu, macOS, Windows
- **Multi-version**: Test với Python 3.8, 3.9, 3.10, 3.11
- **Docker Build**: Tự động build Docker image
- **Release**: Tự động tạo release khi tag version

### Chạy tests locally

```bash
# Cài đặt test dependencies
pip install pytest pytest-cov flake8 black

# Chạy tests
pytest tests/ -v

# Check code style
flake8 .
black --check .
```

### GitHub Actions Workflows

- `.github/workflows/ci.yml` - Full CI/CD pipeline
- `.github/workflows/test.yml` - Quick test on push
- `.github/workflows/release.yml` - Build release packages

## 📧 Liên hệ

Nếu có vấn đề hoặc câu hỏi, vui lòng tạo issue trên GitHub.

## 🏗️ Build Status

![CI](https://github.com/YOUR_USERNAME/FoLive/workflows/CI%2FCD%20Pipeline/badge.svg)
![Tests](https://github.com/YOUR_USERNAME/FoLive/workflows/Quick%20Test/badge.svg)

---

**FoLive** - Công cụ livestream 24/7 mạnh mẽ và dễ sử dụng! 🚀


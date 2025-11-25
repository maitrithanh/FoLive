"""
Source Handlers - Xử lý các nguồn video khác nhau
"""
import os
import subprocess
import yt_dlp
from typing import Dict, Optional, List
from config import TEMP_DIR, SUPPORTED_FORMATS

class SourceHandler:
    """Base class cho các source handlers"""
    
    def __init__(self):
        self.temp_dir = TEMP_DIR
        os.makedirs(self.temp_dir, exist_ok=True)
    
    def validate(self, source: str) -> bool:
        """Kiểm tra source có hợp lệ không"""
        raise NotImplementedError
    
    def get_stream_url(self, source: str) -> Optional[str]:
        """Lấy stream URL từ source"""
        raise NotImplementedError

class FileSourceHandler(SourceHandler):
    """Xử lý file video"""
    
    def validate(self, source: str) -> bool:
        """Kiểm tra file có tồn tại và đúng format không"""
        if not os.path.exists(source):
            return False
        
        ext = os.path.splitext(source)[1].lower()
        return ext in SUPPORTED_FORMATS
    
    def get_stream_url(self, source: str) -> Optional[str]:
        """Trả về đường dẫn file"""
        if self.validate(source):
            return source
        return None
    
    def get_info(self, source: str) -> Dict:
        """Lấy thông tin file"""
        if not self.validate(source):
            return {'error': 'Invalid file'}
        
        stat = os.stat(source)
        return {
            'path': source,
            'size': stat.st_size,
            'extension': os.path.splitext(source)[1]
        }

class YouTubeSourceHandler(SourceHandler):
    """Xử lý YouTube video"""
    
    def __init__(self):
        super().__init__()
        self.ydl_opts = {
            'format': 'best[ext=mp4]/best',
            'outtmpl': os.path.join(self.temp_dir, '%(id)s.%(ext)s'),
            'quiet': True,
            'no_warnings': True,
        }
    
    def validate(self, source: str) -> bool:
        """Kiểm tra URL YouTube có hợp lệ không"""
        return 'youtube.com' in source or 'youtu.be' in source
    
    def get_stream_url(self, source: str) -> Optional[str]:
        """Download và trả về đường dẫn file hoặc stream URL"""
        if not self.validate(source):
            return None
        
        try:
            # Thử lấy stream URL trực tiếp (không download)
            ydl_opts_stream = {
                'format': 'best[ext=mp4]/best',
                'quiet': True,
                'no_warnings': True,
            }
            
            with yt_dlp.YoutubeDL(ydl_opts_stream) as ydl:
                info = ydl.extract_info(source, download=False)
                # Lấy URL stream trực tiếp
                if 'url' in info:
                    return info['url']
                
                # Nếu không có, download file
                ydl_opts_download = self.ydl_opts.copy()
                with yt_dlp.YoutubeDL(ydl_opts_download) as ydl_dl:
                    info = ydl_dl.extract_info(source, download=True)
                    filename = ydl_dl.prepare_filename(info)
                    return filename
        except Exception as e:
            print(f"Error getting YouTube stream: {e}")
            return None
    
    def get_info(self, source: str) -> Dict:
        """Lấy thông tin video YouTube"""
        if not self.validate(source):
            return {'error': 'Invalid YouTube URL'}
        
        try:
            ydl_opts = {'quiet': True, 'no_warnings': True}
            with yt_dlp.YoutubeDL(ydl_opts) as ydl:
                info = ydl.extract_info(source, download=False)
                return {
                    'title': info.get('title'),
                    'duration': info.get('duration'),
                    'view_count': info.get('view_count'),
                    'uploader': info.get('uploader')
                }
        except Exception as e:
            return {'error': str(e)}

class PlaylistSourceHandler(YouTubeSourceHandler):
    """Xử lý YouTube playlist"""
    
    def validate(self, source: str) -> bool:
        """Kiểm tra playlist URL"""
        return 'playlist' in source.lower() or 'list=' in source
    
    def get_videos(self, source: str) -> List[str]:
        """Lấy danh sách video trong playlist"""
        if not self.validate(source):
            return []
        
        try:
            ydl_opts = {'quiet': True, 'extract_flat': True}
            with yt_dlp.YoutubeDL(ydl_opts) as ydl:
                info = ydl.extract_info(source, download=False)
                videos = []
                for entry in info.get('entries', []):
                    video_id = entry.get('id')
                    if video_id:
                        videos.append(f"https://www.youtube.com/watch?v={video_id}")
                return videos
        except Exception as e:
            print(f"Error getting playlist: {e}")
            return []

class ScreenSourceHandler(SourceHandler):
    """Xử lý screen capture"""
    
    def validate(self, source: str) -> bool:
        """Screen capture luôn hợp lệ"""
        return True
    
    def get_stream_url(self, source: str) -> Optional[str]:
        """Trả về screen identifier"""
        # macOS: "1:0" là screen:audio
        return "1:0"
    
    def get_available_screens(self) -> List[Dict]:
        """Lấy danh sách màn hình có sẵn"""
        # Trên macOS, sử dụng avfoundation
        try:
            result = subprocess.run(
                ['ffmpeg', '-f', 'avfoundation', '-list_devices', 'true', '-i', '""'],
                capture_output=True,
                text=True
            )
            # Parse output để lấy danh sách devices
            return [{'id': '1:0', 'name': 'Screen Capture'}]
        except:
            return []

class SourceHandlerFactory:
    """Factory để tạo source handlers"""
    
    @staticmethod
    def get_handler(source_type: str) -> SourceHandler:
        """Lấy handler theo loại source"""
        handlers = {
            'file': FileSourceHandler,
            'youtube': YouTubeSourceHandler,
            'playlist': PlaylistSourceHandler,
            'screen': ScreenSourceHandler
        }
        
        handler_class = handlers.get(source_type)
        if handler_class:
            return handler_class()
        raise ValueError(f"Unknown source type: {source_type}")


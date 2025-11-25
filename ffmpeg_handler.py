"""
FFmpeg Handler - Xử lý video và livestream với FFmpeg
"""
import subprocess
import os
import json
from typing import Dict, List, Optional
from config import FFMPEG_PATH, FFPROBE_PATH, TEMP_DIR, SUPPORTED_FORMATS

class FFmpegHandler:
    """Xử lý các thao tác FFmpeg"""
    
    def __init__(self):
        self.ffmpeg_path = FFMPEG_PATH
        self.ffprobe_path = FFPROBE_PATH
        self.temp_dir = TEMP_DIR
        
        # Tạo thư mục temp nếu chưa có
        os.makedirs(self.temp_dir, exist_ok=True)
    
    def build_stream_command(self, source: str, source_type: str,
                            stream_url: str, stream_key: str, config: Dict) -> List[str]:
        """Xây dựng FFmpeg command cho livestream"""
        cmd = [self.ffmpeg_path]
        
        # Input source
        if source_type == 'file':
            cmd.extend(self._build_file_input(source, config))
        elif source_type == 'youtube':
            cmd.extend(self._build_youtube_input(source, config))
        elif source_type == 'screen':
            cmd.extend(self._build_screen_input(config))
        elif source_type == 'playlist':
            cmd.extend(self._build_playlist_input(source, config))
        
        # Video filters
        filters = self._build_video_filters(config)
        if filters:
            cmd.extend(['-vf', filters])
        
        # Audio filters
        audio_filters = self._build_audio_filters(config)
        if audio_filters:
            cmd.extend(['-af', audio_filters])
        
        # Video encoding settings
        cmd.extend([
            '-c:v', 'libx264',
            '-preset', config.get('preset', 'veryfast'),
            '-tune', 'zerolatency',
            '-b:v', str(config.get('bitrate', '2500k')),
            '-maxrate', str(config.get('maxrate', '2500k')),
            '-bufsize', str(config.get('bufsize', '5000k')),
            '-g', '50',
            '-c:a', 'aac',
            '-b:a', str(config.get('audio_bitrate', '128k')),
            '-ar', '44100',
            '-f', 'flv'
        ])
        
        # Output RTMP URL
        rtmp_url = f"{stream_url}/{stream_key}"
        cmd.append(rtmp_url)
        
        return cmd
    
    def _build_file_input(self, file_path: str, config: Dict) -> List[str]:
        """Xây dựng input cho file video"""
        cmd = ['-re']  # Read input at native frame rate
        
        # Loop nếu cần
        if config.get('loop', True):
            cmd.extend(['-stream_loop', '-1'])
        
        # Intro/Outro - sử dụng filter_complex để concat
        inputs = []
        if config.get('intro'):
            inputs.append(config['intro'])
        inputs.append(file_path)
        if config.get('outro'):
            inputs.append(config['outro'])
        
        # Nếu có intro/outro, cần xử lý khác
        if len(inputs) > 1:
            # Tạm thời chỉ dùng file chính, intro/outro sẽ được xử lý sau
            cmd.extend(['-i', file_path])
        else:
            cmd.extend(['-i', file_path])
        
        return cmd
    
    def _build_youtube_input(self, youtube_url: str, config: Dict) -> List[str]:
        """Xây dựng input cho YouTube video"""
        # Sử dụng yt-dlp để lấy stream URL
        # Tạm thời sử dụng file đã download
        # Trong thực tế cần tích hợp yt-dlp
        cmd = ['-re', '-i', youtube_url]
        if config.get('loop', True):
            cmd.extend(['-stream_loop', '-1'])
        return cmd
    
    def _build_screen_input(self, config: Dict) -> List[str]:
        """Xây dựng input cho screen capture"""
        # macOS sử dụng avfoundation
        # Format: screen_index:audio_index (1:0 = screen 1, no audio)
        screen = config.get('screen', '1:0')
        fps = config.get('fps', 30)
        resolution = config.get('resolution', '1920x1080')
        
        cmd = [
            '-f', 'avfoundation',
            '-framerate', str(fps),
            '-video_size', resolution,
            '-i', screen
        ]
        
        # Capture audio nếu có
        if config.get('capture_audio', True) and ':' in screen:
            # Audio đã được chỉ định trong screen parameter
            pass
        
        return cmd
    
    def _build_playlist_input(self, playlist_url: str, config: Dict) -> List[str]:
        """Xây dựng input cho playlist"""
        # Tương tự YouTube nhưng xử lý nhiều video
        cmd = ['-re', '-i', playlist_url]
        if config.get('loop', True):
            cmd.extend(['-stream_loop', '-1'])
        return cmd
    
    def _build_video_filters(self, config: Dict) -> Optional[str]:
        """Xây dựng video filters"""
        filters = []
        
        # Speed
        if config.get('speed', 1.0) != 1.0:
            filters.append(f"setpts=PTS/{config['speed']}")
        
        # Brightness
        if config.get('brightness', 0) != 0:
            filters.append(f"eq=brightness={config['brightness']/100}")
        
        # Text overlay
        if config.get('text'):
            text = config['text'].replace(':', '\\:').replace("'", "\\'")
            filters.append(f"drawtext=text='{text}':fontsize={config.get('text_size', 24)}:"
                          f"fontcolor={config.get('text_color', 'white')}:"
                          f"x={config.get('text_x', 10)}:y={config.get('text_y', 10)}")
        
        # Scale
        if config.get('scale'):
            filters.append(f"scale={config['scale']}")
        
        return ','.join(filters) if filters else None
    
    def _build_audio_filters(self, config: Dict) -> Optional[str]:
        """Xây dựng audio filters"""
        filters = []
        
        # Volume
        if config.get('volume', 1.0) != 1.0:
            filters.append(f"volume={config['volume']}")
        
        return ','.join(filters) if filters else None
    
    def get_video_info(self, file_path: str) -> Dict:
        """Lấy thông tin video"""
        cmd = [
            self.ffprobe_path,
            '-v', 'quiet',
            '-print_format', 'json',
            '-show_format',
            '-show_streams',
            file_path
        ]
        
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, check=True)
            return json.loads(result.stdout)
        except Exception as e:
            return {'error': str(e)}
    
    def process_video(self, input_file: str, output_file: str, config: Dict) -> bool:
        """Xử lý video (render)"""
        cmd = [self.ffmpeg_path, '-i', input_file]
        
        # Video filters
        filters = self._build_video_filters(config)
        if filters:
            cmd.extend(['-vf', filters])
        
        # Audio filters
        audio_filters = self._build_audio_filters(config)
        if audio_filters:
            cmd.extend(['-af', audio_filters])
        
        # Output settings
        cmd.extend(['-c:v', 'libx264', '-c:a', 'aac', '-y', output_file])
        
        try:
            subprocess.run(cmd, check=True, capture_output=True)
            return True
        except Exception as e:
            print(f"Error processing video: {e}")
            return False
    
    def check_ffmpeg(self) -> bool:
        """Kiểm tra FFmpeg có sẵn không"""
        try:
            subprocess.run([self.ffmpeg_path, '-version'], 
                         capture_output=True, check=True)
            return True
        except:
            return False


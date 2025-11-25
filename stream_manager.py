"""
Stream Manager - Quản lý nhiều luồng livestream
"""
import subprocess
import threading
import time
import os
import json
from datetime import datetime
from typing import Dict, List, Optional
from enum import Enum

class StreamStatus(Enum):
    IDLE = "idle"
    STARTING = "starting"
    RUNNING = "running"
    STOPPING = "stopping"
    STOPPED = "stopped"
    ERROR = "error"

class Stream:
    """Đại diện cho một luồng livestream"""
    def __init__(self, stream_id: str, source: str, source_type: str, 
                 stream_key: str, stream_url: str, config: Dict = None):
        self.stream_id = stream_id
        self.source = source
        self.source_type = source_type  # 'file', 'youtube', 'screen', 'playlist'
        self.stream_key = stream_key
        self.stream_url = stream_url
        self.config = config or {}
        self.status = StreamStatus.IDLE
        self.process = None
        self.thread = None
        self.start_time = None
        self.error_message = None
        self.stats = {
            'duration': 0,
            'frames': 0,
            'bitrate': 0
        }
        
    def start(self):
        """Bắt đầu stream"""
        if self.status == StreamStatus.RUNNING:
            return False
        
        self.status = StreamStatus.STARTING
        self.thread = threading.Thread(target=self._run_stream, daemon=True)
        self.thread.start()
        return True
    
    def stop(self):
        """Dừng stream"""
        if self.status in [StreamStatus.STOPPED, StreamStatus.IDLE]:
            return False
        
        self.status = StreamStatus.STOPPING
        if self.process:
            try:
                self.process.terminate()
                self.process.wait(timeout=5)
            except:
                self.process.kill()
        self.status = StreamStatus.STOPPED
        return True
    
    def _run_stream(self):
        """Chạy stream trong thread riêng"""
        try:
            from ffmpeg_handler import FFmpegHandler
            handler = FFmpegHandler()
            
            # Tạo FFmpeg command dựa trên source type
            cmd = handler.build_stream_command(
                self.source, 
                self.source_type,
                self.stream_url,
                self.stream_key,
                self.config
            )
            
            self.process = subprocess.Popen(
                cmd,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                universal_newlines=True
            )
            
            self.status = StreamStatus.RUNNING
            self.start_time = datetime.now()
            
            # Monitor process
            while self.status == StreamStatus.RUNNING:
                if self.process.poll() is not None:
                    break
                time.sleep(1)
            
            if self.process.returncode != 0:
                error = self.process.stderr.read() if self.process.stderr else "Unknown error"
                self.error_message = error
                self.status = StreamStatus.ERROR
            else:
                self.status = StreamStatus.STOPPED
                
        except Exception as e:
            self.error_message = str(e)
            self.status = StreamStatus.ERROR
    
    def get_info(self) -> Dict:
        """Lấy thông tin stream"""
        duration = 0
        if self.start_time:
            duration = (datetime.now() - self.start_time).total_seconds()
        
        return {
            'stream_id': self.stream_id,
            'source': self.source,
            'source_type': self.source_type,
            'status': self.status.value,
            'duration': duration,
            'start_time': self.start_time.isoformat() if self.start_time else None,
            'error_message': self.error_message,
            'stats': self.stats,
            'config': self.config
        }

class StreamManager:
    """Quản lý tất cả các luồng livestream"""
    def __init__(self):
        self.streams: Dict[str, Stream] = {}
        self.lock = threading.Lock()
    
    def create_stream(self, stream_id: str, source: str, source_type: str,
                     stream_key: str, stream_url: str, config: Dict = None) -> Stream:
        """Tạo stream mới"""
        with self.lock:
            if stream_id in self.streams:
                raise ValueError(f"Stream {stream_id} already exists")
            
            stream = Stream(stream_id, source, source_type, stream_key, stream_url, config)
            self.streams[stream_id] = stream
            return stream
    
    def start_stream(self, stream_id: str) -> bool:
        """Bắt đầu một stream"""
        with self.lock:
            if stream_id not in self.streams:
                return False
            return self.streams[stream_id].start()
    
    def stop_stream(self, stream_id: str) -> bool:
        """Dừng một stream"""
        with self.lock:
            if stream_id not in self.streams:
                return False
            return self.streams[stream_id].stop()
    
    def remove_stream(self, stream_id: str) -> bool:
        """Xóa một stream"""
        with self.lock:
            if stream_id not in self.streams:
                return False
            
            stream = self.streams[stream_id]
            if stream.status == StreamStatus.RUNNING:
                stream.stop()
            
            del self.streams[stream_id]
            return True
    
    def get_stream(self, stream_id: str) -> Optional[Stream]:
        """Lấy stream theo ID"""
        with self.lock:
            return self.streams.get(stream_id)
    
    def get_all_streams(self) -> List[Dict]:
        """Lấy thông tin tất cả streams"""
        with self.lock:
            return [stream.get_info() for stream in self.streams.values()]
    
    def get_stream_status(self, stream_id: str) -> Optional[Dict]:
        """Lấy trạng thái stream"""
        stream = self.get_stream(stream_id)
        if stream:
            return stream.get_info()
        return None



"""
Configuration file for FoLive
"""
import os
from dotenv import load_dotenv

load_dotenv()

# YouTube API Configuration
YOUTUBE_STREAM_KEY = os.getenv('YOUTUBE_STREAM_KEY', '')
YOUTUBE_STREAM_URL = os.getenv('YOUTUBE_STREAM_URL', '')

# FFmpeg Configuration
FFMPEG_PATH = os.getenv('FFMPEG_PATH', 'ffmpeg')
FFPROBE_PATH = os.getenv('FFPROBE_PATH', 'ffprobe')

# Application Configuration
HOST = os.getenv('HOST', '0.0.0.0')
PORT = int(os.getenv('PORT', 5000))
DEBUG = os.getenv('DEBUG', 'False').lower() == 'true'

# Video Processing
TEMP_DIR = os.getenv('TEMP_DIR', './temp')
OUTPUT_DIR = os.getenv('OUTPUT_DIR', './output')
MAX_CONCURRENT_STREAMS = int(os.getenv('MAX_CONCURRENT_STREAMS', '10'))

# Supported video formats
SUPPORTED_FORMATS = ['.mp4', '.mov', '.mkv', '.avi', '.flv', '.webm']



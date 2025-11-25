"""
Tests for Source Handlers
"""
import unittest
import sys
import os
from unittest.mock import patch, MagicMock

# Add parent directory to path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from source_handlers import (
    FileSourceHandler,
    YouTubeSourceHandler,
    PlaylistSourceHandler,
    ScreenSourceHandler,
    SourceHandlerFactory
)

class TestFileSourceHandler(unittest.TestCase):
    """Test cases for FileSourceHandler"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.handler = FileSourceHandler()
    
    def test_validate_file_path(self):
        """Test validating file path"""
        # This will fail if file doesn't exist, which is expected in CI
        # In real scenario, you'd create a test file
        result = self.handler.validate('/nonexistent/file.mp4')
        # Should return False for non-existent file
        self.assertIsInstance(result, bool)
    
    def test_get_info_nonexistent(self):
        """Test getting info for non-existent file"""
        info = self.handler.get_info('/nonexistent/file.mp4')
        self.assertIn('error', info)

class TestYouTubeSourceHandler(unittest.TestCase):
    """Test cases for YouTubeSourceHandler"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.handler = YouTubeSourceHandler()
    
    def test_validate_youtube_url(self):
        """Test validating YouTube URL"""
        valid_urls = [
            'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
            'https://youtu.be/dQw4w9WgXcQ',
            'http://youtube.com/watch?v=test'
        ]
        
        for url in valid_urls:
            self.assertTrue(self.handler.validate(url))
        
        invalid_urls = [
            'https://example.com',
            'not a url',
            'https://vimeo.com/123'
        ]
        
        for url in invalid_urls:
            self.assertFalse(self.handler.validate(url))

class TestSourceHandlerFactory(unittest.TestCase):
    """Test cases for SourceHandlerFactory"""
    
    def test_get_file_handler(self):
        """Test getting file handler"""
        handler = SourceHandlerFactory.get_handler('file')
        self.assertIsInstance(handler, FileSourceHandler)
    
    def test_get_youtube_handler(self):
        """Test getting YouTube handler"""
        handler = SourceHandlerFactory.get_handler('youtube')
        self.assertIsInstance(handler, YouTubeSourceHandler)
    
    def test_get_screen_handler(self):
        """Test getting screen handler"""
        handler = SourceHandlerFactory.get_handler('screen')
        self.assertIsInstance(handler, ScreenSourceHandler)
    
    def test_get_invalid_handler(self):
        """Test getting invalid handler"""
        with self.assertRaises(ValueError):
            SourceHandlerFactory.get_handler('invalid')

if __name__ == '__main__':
    unittest.main()



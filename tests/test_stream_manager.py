"""
Tests for Stream Manager
"""
import unittest
from unittest.mock import Mock, patch
import sys
import os

# Add parent directory to path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from stream_manager import StreamManager, Stream, StreamStatus

class TestStreamManager(unittest.TestCase):
    """Test cases for StreamManager"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.manager = StreamManager()
    
    def test_create_stream(self):
        """Test creating a new stream"""
        stream = self.manager.create_stream(
            'test-stream-1',
            '/path/to/video.mp4',
            'file',
            'test-key',
            'rtmp://test.com',
            {'bitrate': '2500k'}
        )
        
        self.assertIsNotNone(stream)
        self.assertEqual(stream.stream_id, 'test-stream-1')
        self.assertEqual(stream.status, StreamStatus.IDLE)
    
    def test_duplicate_stream_id(self):
        """Test creating duplicate stream ID"""
        self.manager.create_stream(
            'test-stream-1',
            '/path/to/video.mp4',
            'file',
            'test-key',
            'rtmp://test.com'
        )
        
        with self.assertRaises(ValueError):
            self.manager.create_stream(
                'test-stream-1',
                '/path/to/video2.mp4',
                'file',
                'test-key',
                'rtmp://test.com'
            )
    
    def test_get_stream(self):
        """Test getting a stream"""
        self.manager.create_stream(
            'test-stream-1',
            '/path/to/video.mp4',
            'file',
            'test-key',
            'rtmp://test.com'
        )
        
        stream = self.manager.get_stream('test-stream-1')
        self.assertIsNotNone(stream)
        self.assertEqual(stream.stream_id, 'test-stream-1')
    
    def test_get_nonexistent_stream(self):
        """Test getting non-existent stream"""
        stream = self.manager.get_stream('non-existent')
        self.assertIsNone(stream)
    
    def test_get_all_streams(self):
        """Test getting all streams"""
        self.manager.create_stream('stream-1', '/path/1', 'file', 'key', 'url')
        self.manager.create_stream('stream-2', '/path/2', 'file', 'key', 'url')
        
        streams = self.manager.get_all_streams()
        self.assertEqual(len(streams), 2)
    
    def test_remove_stream(self):
        """Test removing a stream"""
        self.manager.create_stream('stream-1', '/path/1', 'file', 'key', 'url')
        
        result = self.manager.remove_stream('stream-1')
        self.assertTrue(result)
        
        stream = self.manager.get_stream('stream-1')
        self.assertIsNone(stream)
    
    def test_remove_nonexistent_stream(self):
        """Test removing non-existent stream"""
        result = self.manager.remove_stream('non-existent')
        self.assertFalse(result)

class TestStream(unittest.TestCase):
    """Test cases for Stream"""
    
    def test_stream_creation(self):
        """Test stream creation"""
        stream = Stream(
            'test-id',
            '/path/to/video.mp4',
            'file',
            'test-key',
            'rtmp://test.com',
            {'bitrate': '2500k'}
        )
        
        self.assertEqual(stream.stream_id, 'test-id')
        self.assertEqual(stream.source, '/path/to/video.mp4')
        self.assertEqual(stream.source_type, 'file')
        self.assertEqual(stream.status, StreamStatus.IDLE)
    
    def test_stream_info(self):
        """Test getting stream info"""
        stream = Stream(
            'test-id',
            '/path/to/video.mp4',
            'file',
            'test-key',
            'rtmp://test.com'
        )
        
        info = stream.get_info()
        self.assertIn('stream_id', info)
        self.assertIn('status', info)
        self.assertIn('source', info)

if __name__ == '__main__':
    unittest.main()


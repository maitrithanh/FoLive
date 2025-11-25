"""
FoLive - Ứng dụng livestream 24/7 đa luồng
"""
from flask import Flask, render_template, request, jsonify
from flask_cors import CORS
import os
from stream_manager import StreamManager, StreamStatus
from source_handlers import SourceHandlerFactory
from ffmpeg_handler import FFmpegHandler
from config import HOST, PORT, DEBUG

app = Flask(__name__)
CORS(app)

# Khởi tạo Stream Manager
stream_manager = StreamManager()
ffmpeg_handler = FFmpegHandler()

@app.route('/')
def index():
    """Trang chủ"""
    return render_template('index.html')

@app.route('/api/streams', methods=['GET'])
def get_streams():
    """Lấy danh sách tất cả streams"""
    streams = stream_manager.get_all_streams()
    return jsonify({'success': True, 'streams': streams})

@app.route('/api/streams', methods=['POST'])
def create_stream():
    """Tạo stream mới"""
    data = request.json
    
    required_fields = ['stream_id', 'source', 'source_type', 'stream_key', 'stream_url']
    for field in required_fields:
        if field not in data:
            return jsonify({'success': False, 'error': f'Missing field: {field}'}), 400
    
    try:
        # Validate source
        handler = SourceHandlerFactory.get_handler(data['source_type'])
        if not handler.validate(data['source']):
            return jsonify({'success': False, 'error': 'Invalid source'}), 400
        
        # Get actual stream URL from source
        stream_source = handler.get_stream_url(data['source'])
        if not stream_source:
            return jsonify({'success': False, 'error': 'Failed to get stream source'}), 400
        
        # Create stream
        config = data.get('config', {})
        stream = stream_manager.create_stream(
            data['stream_id'],
            stream_source,
            data['source_type'],
            data['stream_key'],
            data['stream_url'],
            config
        )
        
        return jsonify({'success': True, 'stream': stream.get_info()})
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)}), 500

@app.route('/api/streams/<stream_id>', methods=['GET'])
def get_stream(stream_id):
    """Lấy thông tin một stream"""
    stream_info = stream_manager.get_stream_status(stream_id)
    if stream_info:
        return jsonify({'success': True, 'stream': stream_info})
    return jsonify({'success': False, 'error': 'Stream not found'}), 404

@app.route('/api/streams/<stream_id>/start', methods=['POST'])
def start_stream(stream_id):
    """Bắt đầu stream"""
    success = stream_manager.start_stream(stream_id)
    if success:
        return jsonify({'success': True, 'message': 'Stream started'})
    return jsonify({'success': False, 'error': 'Failed to start stream'}), 400

@app.route('/api/streams/<stream_id>/stop', methods=['POST'])
def stop_stream(stream_id):
    """Dừng stream"""
    success = stream_manager.stop_stream(stream_id)
    if success:
        return jsonify({'success': True, 'message': 'Stream stopped'})
    return jsonify({'success': False, 'error': 'Failed to stop stream'}), 400

@app.route('/api/streams/<stream_id>', methods=['DELETE'])
def delete_stream(stream_id):
    """Xóa stream"""
    success = stream_manager.remove_stream(stream_id)
    if success:
        return jsonify({'success': True, 'message': 'Stream deleted'})
    return jsonify({'success': False, 'error': 'Stream not found'}), 404

@app.route('/api/streams/<stream_id>/config', methods=['PUT'])
def update_stream_config(stream_id):
    """Cập nhật config của stream"""
    data = request.json
    stream = stream_manager.get_stream(stream_id)
    
    if not stream:
        return jsonify({'success': False, 'error': 'Stream not found'}), 404
    
    # Update config
    stream.config.update(data.get('config', {}))
    
    return jsonify({'success': True, 'stream': stream.get_info()})

@app.route('/api/sources/validate', methods=['POST'])
def validate_source():
    """Validate source"""
    data = request.json
    source_type = data.get('source_type')
    source = data.get('source')
    
    if not source_type or not source:
        return jsonify({'success': False, 'error': 'Missing parameters'}), 400
    
    try:
        handler = SourceHandlerFactory.get_handler(source_type)
        is_valid = handler.validate(source)
        
        if is_valid and hasattr(handler, 'get_info'):
            info = handler.get_info(source)
            return jsonify({'success': True, 'valid': True, 'info': info})
        
        return jsonify({'success': True, 'valid': is_valid})
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)}), 500

@app.route('/api/video/info', methods=['POST'])
def get_video_info():
    """Lấy thông tin video file"""
    data = request.json
    file_path = data.get('file_path')
    
    if not file_path or not os.path.exists(file_path):
        return jsonify({'success': False, 'error': 'File not found'}), 404
    
    info = ffmpeg_handler.get_video_info(file_path)
    return jsonify({'success': True, 'info': info})

@app.route('/api/system/check', methods=['GET'])
def check_system():
    """Kiểm tra hệ thống"""
    ffmpeg_available = ffmpeg_handler.check_ffmpeg()
    
    return jsonify({
        'success': True,
        'ffmpeg_available': ffmpeg_available,
        'active_streams': len([s for s in stream_manager.get_all_streams() 
                              if s['status'] == 'running'])
    })

if __name__ == '__main__':
    # Kiểm tra FFmpeg
    if not ffmpeg_handler.check_ffmpeg():
        print("Warning: FFmpeg not found. Please install FFmpeg.")
    
    app.run(host=HOST, port=PORT, debug=DEBUG)


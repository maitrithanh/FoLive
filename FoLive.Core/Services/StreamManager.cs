using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoLive.Core.Models;
using StreamModel = FoLive.Core.Models.Stream;

namespace FoLive.Core.Services;

public class StreamManager
{
    private readonly Dictionary<string, StreamModel> _streams = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly FFmpegService _ffmpegService;
    private readonly YtDlpService _ytDlpService;
    private readonly ConfigService _configService;
    private readonly LogService? _logger;

    public StreamManager(ConfigService? configService = null, LogService? logger = null)
    {
        _logger = logger;
        _ffmpegService = new FFmpegService(logger: _logger);
        _ytDlpService = new YtDlpService();
        _configService = configService ?? new ConfigService(logger: _logger);
    }

    public event EventHandler<StreamEventArgs>? StreamStatusChanged;
    
    public async Task LoadConfigAsync()
    {
        var configs = await _configService.LoadStreamsAsync();
        var streams = _configService.ConvertConfigsToStreams(configs);
        
        await _lock.WaitAsync();
        try
        {
            foreach (var stream in streams)
            {
                _streams[stream.StreamId] = stream;
            }
        }
        finally
        {
            _lock.Release();
        }
    }
    
    private async Task SaveConfigAsync()
    {
        // Don't lock here - caller should already have the lock
        try
        {
            _logger?.LogInfo("Bắt đầu lưu config...");
            var streams = _streams.Values.ToList();
            _logger?.LogInfo($"Đang lưu {streams.Count} stream(s) vào config");
            await _configService.SaveStreamsAsync(streams);
            _logger?.LogInfo("Đã lưu config thành công");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Lỗi khi lưu config: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<bool> AddStreamAsync(StreamModel stream)
    {
        await _lock.WaitAsync();
        try
        {
            if (_streams.ContainsKey(stream.StreamId))
            {
                _logger?.LogWarning($"Stream với ID '{stream.StreamId}' đã tồn tại");
                return false;
            }

            _streams[stream.StreamId] = stream;
            OnStreamStatusChanged(stream);
            await SaveConfigAsync();
            _logger?.LogInfo($"Đã thêm stream '{stream.StreamId}' thành công vào StreamManager");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Lỗi khi thêm stream '{stream.StreamId}': {ex.Message}", ex);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateStreamAsync(StreamModel updatedStream)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_streams.TryGetValue(updatedStream.StreamId, out var existingStream))
            {
                _logger?.LogWarning($"Không tìm thấy stream với ID '{updatedStream.StreamId}' để cập nhật");
                return false;
            }

            _logger?.LogInfo($"Đang cập nhật stream '{updatedStream.StreamId}'");

            // If stream is running, stop it first
            if (existingStream.Status == StreamStatus.Running)
            {
                _logger?.LogInfo($"Stream '{updatedStream.StreamId}' đang chạy, đang dừng trước khi cập nhật...");
                await StopStreamAsync(updatedStream.StreamId);
            }

            // Update stream data (keep runtime data if needed)
            existingStream.Source = updatedStream.Source;
            existingStream.SourceType = updatedStream.SourceType;
            existingStream.StreamKey = updatedStream.StreamKey;
            existingStream.StreamUrl = updatedStream.StreamUrl;
            existingStream.Config = updatedStream.Config;
            
            // Reset status to Idle
            existingStream.Status = StreamStatus.Idle;
            existingStream.ErrorMessage = null;

            OnStreamStatusChanged(existingStream);
            await SaveConfigAsync();
            _logger?.LogInfo($"Đã cập nhật stream '{updatedStream.StreamId}' thành công");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Lỗi khi cập nhật stream '{updatedStream.StreamId}': {ex.Message}", ex);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveStreamAsync(string streamId)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_streams.TryGetValue(streamId, out var stream))
            {
                _logger?.LogWarning($"Không tìm thấy stream với ID '{streamId}' để xóa");
                return false;
            }

            if (stream.Status == StreamStatus.Running)
            {
                _logger?.LogInfo($"Stream '{streamId}' đang chạy, đang dừng trước khi xóa...");
                await StopStreamAsync(streamId);
            }

            _streams.Remove(streamId);
            await SaveConfigAsync();
            _logger?.LogInfo($"Đã xóa stream '{streamId}' thành công");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Lỗi khi xóa stream '{streamId}': {ex.Message}", ex);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task SaveConfigNowAsync()
    {
        await SaveConfigAsync();
    }

    public async Task<bool> StartStreamAsync(string streamId)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_streams.TryGetValue(streamId, out var stream))
            {
                return false;
            }

            if (stream.Status == StreamStatus.Running)
            {
                return false;
            }

            stream.Status = StreamStatus.Starting;
            stream.ErrorMessage = null;
            OnStreamStatusChanged(stream);
            _logger?.LogInfo($"Stream '{streamId}' đang chuyển sang trạng thái Starting");

            try
            {
                _logger?.LogInfo($"Đang build FFmpeg command cho stream '{streamId}'");
                // Build FFmpeg command
                var command = await _ffmpegService.BuildStreamCommandAsync(
                    stream.Source,
                    stream.SourceType,
                    stream.StreamUrl,
                    stream.StreamKey,
                    stream.Config,
                    _ytDlpService
                );

                if (string.IsNullOrEmpty(command))
                {
                    stream.Status = StreamStatus.Error;
                    stream.ErrorMessage = "Không thể build FFmpeg command. Vui lòng kiểm tra nguồn và cấu hình.";
                    OnStreamStatusChanged(stream);
                    _logger?.LogError($"Không thể build FFmpeg command cho stream '{streamId}'");
                    return false;
                }

                // Log command for debugging
                _logger?.LogInfo($"FFmpeg command cho stream '{streamId}': {command}");

                // Start FFmpeg process
                _logger?.LogInfo($"Đang khởi động FFmpeg process cho stream '{streamId}'");
                try
                {
                    stream.Process = _ffmpegService.StartStream(command);
                    
                    if (stream.Process == null)
                    {
                        stream.Status = StreamStatus.Error;
                        stream.ErrorMessage = "Không thể khởi động FFmpeg process. Vui lòng kiểm tra FFmpeg đã được cài đặt và có trong PATH.";
                        OnStreamStatusChanged(stream);
                        _logger?.LogError($"Không thể khởi động FFmpeg process cho stream '{streamId}' - StartStream trả về null");
                        return false;
                    }
                }
                catch (FileNotFoundException ex)
                {
                    stream.Status = StreamStatus.Error;
                    stream.ErrorMessage = $"FFmpeg không được tìm thấy: {ex.Message}\n\nVui lòng:\n1. Cài đặt FFmpeg\n2. Thêm FFmpeg vào PATH\n3. Hoặc đặt FFmpeg tại: C:\\ffmpeg\\bin\\ffmpeg.exe";
                    OnStreamStatusChanged(stream);
                    _logger?.LogError($"FFmpeg không được tìm thấy cho stream '{streamId}': {ex.Message}", ex);
                    return false;
                }
                catch (Exception ex)
                {
                    stream.Status = StreamStatus.Error;
                    stream.ErrorMessage = $"Lỗi khi khởi động FFmpeg: {ex.Message}";
                    OnStreamStatusChanged(stream);
                    _logger?.LogError($"Lỗi khi khởi động FFmpeg cho stream '{streamId}': {ex.Message}", ex);
                    return false;
                }

                // Check if process started successfully
                if (stream.Process.HasExited)
                {
                    stream.Status = StreamStatus.Error;
                    stream.ErrorMessage = $"FFmpeg process thoát ngay lập tức với mã lỗi {stream.Process.ExitCode}. Vui lòng kiểm tra Stream URL và Key.";
                    OnStreamStatusChanged(stream);
                    _logger?.LogError($"FFmpeg process thoát ngay với mã lỗi {stream.Process.ExitCode} cho stream '{streamId}'");
                    return false;
                }

                stream.Status = StreamStatus.Running;
                stream.StartTime = DateTime.Now;
                stream.ErrorMessage = null;
                OnStreamStatusChanged(stream);
                _logger?.LogInfo($"Stream '{streamId}' đã bắt đầu chạy thành công");

                // Monitor process
                _ = Task.Run(async () => await MonitorStreamAsync(stream));

                return true;
            }
            catch (Exception ex)
            {
                stream.Status = StreamStatus.Error;
                stream.ErrorMessage = $"Error starting stream: {ex.Message}";
                OnStreamStatusChanged(stream);
                return false;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> StopStreamAsync(string streamId)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_streams.TryGetValue(streamId, out var stream))
            {
                return false;
            }

            if (stream.Status != StreamStatus.Running)
            {
                return false;
            }

            stream.Status = StreamStatus.Stopping;
            OnStreamStatusChanged(stream);

            if (stream.Process != null && !stream.Process.HasExited)
            {
                stream.Process.Kill();
                await stream.Process.WaitForExitAsync();
            }

            stream.Status = StreamStatus.Stopped;
            stream.Process = null;
            OnStreamStatusChanged(stream);

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<StreamModel>> GetAllStreamsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _streams.Values.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<StreamModel?> GetStreamAsync(string streamId)
    {
        await _lock.WaitAsync();
        try
        {
            return _streams.TryGetValue(streamId, out var stream) ? stream : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task MonitorStreamAsync(StreamModel stream)
    {
        if (stream.Process == null) return;

        try
        {
            // Monitor process status
            while (!stream.Process.HasExited && stream.Status == StreamStatus.Running)
            {
                await Task.Delay(1000);
                
                // Update stats if possible
                if (stream.StartTime.HasValue)
                {
                    // Stats will be updated by UI refresh
                }
            }

            await stream.Process.WaitForExitAsync();

            if (stream.Process.ExitCode != 0 && stream.Status == StreamStatus.Running)
            {
                stream.Status = StreamStatus.Error;
                stream.ErrorMessage = $"FFmpeg process exited with code {stream.Process.ExitCode}. Check console for details.";
                OnStreamStatusChanged(stream);
            }
            else if (stream.Status == StreamStatus.Running)
            {
                // Process exited normally but status is still Running
                stream.Status = StreamStatus.Stopped;
                OnStreamStatusChanged(stream);
            }
        }
        catch (Exception ex)
        {
            stream.Status = StreamStatus.Error;
            stream.ErrorMessage = $"Monitor error: {ex.Message}";
            OnStreamStatusChanged(stream);
        }
    }

    protected virtual void OnStreamStatusChanged(StreamModel stream)
    {
        StreamStatusChanged?.Invoke(this, new StreamEventArgs(stream));
    }
}

public class StreamEventArgs : EventArgs
{
    public StreamModel Stream { get; }

    public StreamEventArgs(StreamModel stream)
    {
        Stream = stream;
    }
}


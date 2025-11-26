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

    public StreamManager()
    {
        _ffmpegService = new FFmpegService();
        _ytDlpService = new YtDlpService();
    }

    public event EventHandler<StreamEventArgs>? StreamStatusChanged;

    public async Task<bool> AddStreamAsync(StreamModel stream)
    {
        await _lock.WaitAsync();
        try
        {
            if (_streams.ContainsKey(stream.StreamId))
            {
                return false;
            }

            _streams[stream.StreamId] = stream;
            OnStreamStatusChanged(stream);
            return true;
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
                return false;
            }

            if (stream.Status == StreamStatus.Running)
            {
                await StopStreamAsync(streamId);
            }

            _streams.Remove(streamId);
            return true;
        }
        finally
        {
            _lock.Release();
        }
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
            OnStreamStatusChanged(stream);

            // Build FFmpeg command
            var command = await _ffmpegService.BuildStreamCommandAsync(
                stream.Source,
                stream.SourceType,
                stream.StreamUrl,
                stream.StreamKey,
                stream.Config,
                _ytDlpService
            );

            // Start FFmpeg process
            stream.Process = _ffmpegService.StartStream(command);
            
            if (stream.Process == null)
            {
                stream.Status = StreamStatus.Error;
                stream.ErrorMessage = "Failed to start FFmpeg process";
                OnStreamStatusChanged(stream);
                return false;
            }

            stream.Status = StreamStatus.Running;
            stream.StartTime = DateTime.Now;
            OnStreamStatusChanged(stream);

            // Monitor process
            _ = Task.Run(async () => await MonitorStreamAsync(stream));

            return true;
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
            await stream.Process.WaitForExitAsync();

            if (stream.Process.ExitCode != 0 && stream.Status == StreamStatus.Running)
            {
                stream.Status = StreamStatus.Error;
                stream.ErrorMessage = $"Process exited with code {stream.Process.ExitCode}";
                OnStreamStatusChanged(stream);
            }
        }
        catch (Exception ex)
        {
            stream.Status = StreamStatus.Error;
            stream.ErrorMessage = ex.Message;
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


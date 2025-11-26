using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using FoLive.Core.Models;
using StreamModel = FoLive.Core.Models.Stream;

namespace FoLive.ViewModels;

public class StreamViewModel : INotifyPropertyChanged
{
    private StreamModel _stream;
    private int _index;

    public StreamViewModel(StreamModel stream, int index)
    {
        _stream = stream;
        _index = index;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int Index
    {
        get => _index;
        set => _index = value;
    }

    public string StreamId => _stream.StreamId;
    public StreamModel Stream => _stream;

    public string SourceDisplay
    {
        get
        {
            if (_stream.SourceType.ToLower() == "file")
            {
                var fileName = System.IO.Path.GetFileName(_stream.Source);
                return string.IsNullOrEmpty(fileName) ? _stream.Source : fileName;
            }
            if (_stream.SourceType.ToLower() == "youtube" || 
                _stream.SourceType.ToLower() == "playlist" ||
                _stream.SourceType.ToLower() == "facebook" ||
                _stream.SourceType.ToLower() == "url")
            {
                return "Link " + _stream.SourceType;
            }
            if (_stream.SourceType.ToLower() == "screen")
            {
                return "Quay màn hình";
            }
            return _stream.Source;
        }
    }

    public string OutputDisplay
    {
        get
        {
            if (_stream.StreamUrl.Contains("youtube.com") || _stream.StreamUrl.Contains("youtu.be"))
            {
                return "Youtube";
            }
            if (_stream.StreamUrl.Contains("facebook.com"))
            {
                return "Facebook";
            }
            if (_stream.StreamUrl.Contains("twitch.tv"))
            {
                return "Twitch";
            }
            return "Custom";
        }
    }

    public string TimeDisplay
    {
        get
        {
            if (_stream.Status == StreamStatus.Running && _stream.StartTime.HasValue)
            {
                var duration = DateTime.Now - _stream.StartTime.Value;
                return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
            return "";
        }
    }

    public string BitrateDisplay
    {
        get
        {
            if (_stream.Status == StreamStatus.Running && _stream.Stats.Bitrate > 0)
            {
                return $"{_stream.Stats.Bitrate:F1}kbits/s";
            }
            if (_stream.Config.TryGetValue("bitrate", out var bitrate) && bitrate is string bitrateStr)
            {
                return bitrateStr;
            }
            return "";
        }
    }

    public string FPSDisplay
    {
        get
        {
            if (_stream.Status == StreamStatus.Running && _stream.Stats.Frames > 0)
            {
                // Calculate FPS from frames and duration
                if (_stream.StartTime.HasValue)
                {
                    var duration = DateTime.Now - _stream.StartTime.Value;
                    if (duration.TotalSeconds > 0)
                    {
                        var fps = _stream.Stats.Frames / duration.TotalSeconds;
                        return fps.ToString("F0", CultureInfo.InvariantCulture);
                    }
                }
            }
            return "30"; // Default FPS
        }
    }

    public string SpeedDisplay
    {
        get
        {
            if (_stream.Config.TryGetValue("speed", out var speed) && speed is double speedValue)
            {
                return $"{speedValue:F1}x";
            }
            return "1x";
        }
    }

    public string StatusDisplay
    {
        get
        {
            return _stream.Status switch
            {
                StreamStatus.Running => "Tốt!",
                StreamStatus.Starting => "Đang khởi động...",
                StreamStatus.Stopping => "Đang dừng...",
                StreamStatus.Stopped => "Đã dừng",
                StreamStatus.Error => !string.IsNullOrEmpty(_stream.ErrorMessage) 
                    ? $"Lỗi: {_stream.ErrorMessage}" 
                    : "Lỗi",
                _ => "Chờ"
            };
        }
    }

    public Brush StatusColor
    {
        get
        {
            return _stream.Status switch
            {
                StreamStatus.Running => new SolidColorBrush(Colors.Green),
                StreamStatus.Starting => new SolidColorBrush(Colors.Orange),
                StreamStatus.Stopping => new SolidColorBrush(Colors.Orange),
                StreamStatus.Stopped => new SolidColorBrush(Colors.Gray),
                StreamStatus.Error => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Black)
            };
        }
    }

    public void Update(StreamModel stream)
    {
        _stream = stream;
        // Notify all properties changed
        OnPropertyChanged(nameof(SourceDisplay));
        OnPropertyChanged(nameof(OutputDisplay));
        OnPropertyChanged(nameof(TimeDisplay));
        OnPropertyChanged(nameof(BitrateDisplay));
        OnPropertyChanged(nameof(FPSDisplay));
        OnPropertyChanged(nameof(SpeedDisplay));
        OnPropertyChanged(nameof(StatusDisplay));
        OnPropertyChanged(nameof(StatusColor));
    }
}


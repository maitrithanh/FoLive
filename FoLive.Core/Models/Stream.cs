using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FoLive.Core.Models;

public class Stream
{
    public string StreamId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty; // 'file', 'youtube', 'screen', 'playlist'
    public string StreamKey { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public Dictionary<string, object> Config { get; set; } = new();
    
    public StreamStatus Status { get; set; } = StreamStatus.Idle;
    public Process? Process { get; set; }
    public DateTime? StartTime { get; set; }
    public string? ErrorMessage { get; set; }
    
    public StreamStats Stats { get; set; } = new();
}

public class StreamStats
{
    public TimeSpan Duration { get; set; }
    public long Frames { get; set; }
    public double Bitrate { get; set; }
}



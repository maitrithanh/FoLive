using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FoLive.Core.Services;

/// <summary>
/// Service to handle video effects like intro/outro concatenation
/// This creates a temporary processed video file that can be streamed
/// </summary>
public class VideoEffectsService
{
    private readonly FFmpegService _ffmpegService;

    public VideoEffectsService(FFmpegService ffmpegService)
    {
        _ffmpegService = ffmpegService;
    }

    public Task<string?> ProcessVideoWithIntroOutroAsync(
        string mainVideo,
        string? introPath,
        string? outroPath,
        Dictionary<string, object> config)
    {
        // For now, return main video
        // Full intro/outro support would require:
        // 1. Use FFmpeg concat filter
        // 2. Create temporary processed file
        // 3. Stream from that file
        
        // This is a placeholder - can be enhanced to actually process video
        return Task.FromResult<string?>(mainVideo);
    }

    public string BuildConcatCommand(string introPath, string mainVideo, string outroPath, string outputPath)
    {
        // Create concat file list
        var concatFile = Path.Combine(Path.GetTempPath(), $"concat_{Guid.NewGuid()}.txt");
        var concatContent = new StringBuilder();
        
        if (File.Exists(introPath))
        {
            concatContent.AppendLine($"file '{introPath.Replace("'", "'\\''")}'");
        }
        
        concatContent.AppendLine($"file '{mainVideo.Replace("'", "'\\''")}'");
        
        if (File.Exists(outroPath))
        {
            concatContent.AppendLine($"file '{outroPath.Replace("'", "'\\''")}'");
        }
        
        File.WriteAllText(concatFile, concatContent.ToString());

        // Build FFmpeg command to concat videos
        var args = new StringBuilder();
        args.Append($"-f concat -safe 0 -i \"{concatFile}\"");
        args.Append($"-c copy \"{outputPath}\"");

        return args.ToString();
    }
}


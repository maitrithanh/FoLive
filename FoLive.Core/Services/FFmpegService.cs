using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace FoLive.Core.Services;

public class FFmpegService
{
    private readonly string _ffmpegPath;

    public FFmpegService()
    {
        _ffmpegPath = FindFFmpeg();
    }

    private string FindFFmpeg()
    {
        // Check common paths
        var paths = new[]
        {
            "ffmpeg.exe",
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
            Environment.GetEnvironmentVariable("PATH")?.Split(';')
        };

        // Try to find in PATH
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(1000);
            if (process.ExitCode == 0)
            {
                return "ffmpeg";
            }
        }
        catch { }

        return "ffmpeg"; // Default, will fail if not found
    }

    public bool CheckFFmpeg()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(2000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> BuildStreamCommandAsync(
        string source,
        string sourceType,
        string streamUrl,
        string streamKey,
        Dictionary<string, object> config,
        YtDlpService? ytDlpService = null)
    {
        var args = new StringBuilder();
        var useRender = config.TryGetValue("use_render", out var render) && render is bool renderValue && renderValue;
        var loop = config.TryGetValue("loop", out var loopValue) && loopValue is bool loopBool && loopBool;

        // Input source
        string inputSource = source;
        
        switch (sourceType.ToLower())
        {
            case "file":
                if (loop)
                {
                    args.Append($"-re -stream_loop -1 -i \"{source}\"");
                }
                else
                {
                    args.Append($"-re -i \"{source}\"");
                }
                break;
            case "youtube":
            case "playlist":
            case "facebook":
            case "url":
                // Use yt-dlp to get stream URL
                if (ytDlpService != null)
                {
                    var streamUrlFromYtDlp = await ytDlpService.GetStreamUrlAsync(source);
                    if (!string.IsNullOrEmpty(streamUrlFromYtDlp))
                    {
                        inputSource = streamUrlFromYtDlp;
                        args.Append($"-re -i \"{inputSource}\"");
                    }
                    else
                    {
                        // Fallback to direct URL
                        args.Append($"-re -i \"{source}\"");
                    }
                }
                else
                {
                    args.Append($"-re -i \"{source}\"");
                }
                break;
            case "screen":
                args.Append($"-f gdigrab -framerate 30 -i desktop");
                break;
            default:
                // Generic URL - try yt-dlp first
                if (ytDlpService != null && ytDlpService.IsSupportedUrl(source))
                {
                    var streamUrlFromYtDlp = await ytDlpService.GetStreamUrlAsync(source);
                    if (!string.IsNullOrEmpty(streamUrlFromYtDlp))
                    {
                        args.Append($"-re -i \"{streamUrlFromYtDlp}\"");
                    }
                    else
                    {
                        args.Append($"-re -i \"{source}\"");
                    }
                }
                else
                {
                    args.Append($"-re -i \"{source}\"");
                }
                break;
        }

        // Video filters
        var filters = new List<string>();

        if (config.TryGetValue("speed", out var speed) && speed is double speedValue && speedValue != 1.0)
        {
            filters.Add($"setpts={1.0 / speedValue}*PTS");
        }

        if (config.TryGetValue("brightness", out var brightness) && brightness is int brightnessValue)
        {
            filters.Add($"eq=brightness={brightnessValue / 100.0}");
        }

        if (config.TryGetValue("text", out var text) && text is string textValue && !string.IsNullOrEmpty(textValue))
        {
            filters.Add($"drawtext=text='{textValue}':fontsize=24:fontcolor=white:x=10:y=10");
        }

        // Apply filters
        if (filters.Count > 0)
        {
            args.Append($"-vf \"{string.Join(",", filters)}\"");
        }

        // Audio filters
        if (config.TryGetValue("volume", out var volume) && volume is double volumeValue && volumeValue != 1.0)
        {
            args.Append($"-af volume={volumeValue}");
        }

        // Use render or direct stream
        if (useRender)
        {
            // Output settings with encoding
            var bitrate = config.TryGetValue("bitrate", out var br) && br is string bitrateValue 
                ? bitrateValue 
                : "2500k";
            
            var preset = config.TryGetValue("preset", out var presetValue) && presetValue is string presetStr
                ? presetStr
                : "veryfast";

            args.Append($"-c:v libx264 -preset {preset} -b:v {bitrate}");
            args.Append($"-c:a aac -b:a 128k");
        }
        else
        {
            // Direct stream - no encoding (lighter)
            args.Append($"-c copy");
        }

        args.Append($"-f flv");
        args.Append($"\"{streamUrl}/{streamKey}\"");

        return args.ToString();
    }

    public Process? StartStream(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting FFmpeg: {ex.Message}");
            return null;
        }
    }
}


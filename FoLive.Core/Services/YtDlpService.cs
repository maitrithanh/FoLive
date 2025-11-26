using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoLive.Core.Services;

public class YtDlpService
{
    private readonly string _ytDlpPath;

    public YtDlpService()
    {
        _ytDlpPath = FindYtDlp();
    }

    private string FindYtDlp()
    {
        // Check common paths
        var paths = new[]
        {
            "yt-dlp.exe",
            "yt-dlp",
            @"C:\yt-dlp\yt-dlp.exe",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp")
        };

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Try to find in PATH
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(2000);
            if (process.ExitCode == 0)
            {
                return "yt-dlp";
            }
        }
        catch { }

        return "yt-dlp"; // Default
    }

    public bool CheckYtDlp()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = "--version",
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

    public async Task<bool> ValidateUrlAsync(string url)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = $"--dump-json --no-playlist \"{url}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetStreamUrlAsync(string url, string? quality = null)
    {
        try
        {
            var args = new StringBuilder();
            args.Append("--get-url");
            
            if (!string.IsNullOrEmpty(quality))
            {
                args.Append($" -f \"{quality}\"");
            }
            else
            {
                args.Append(" -f \"best[ext=mp4]/best\"");
            }
            
            args.Append($" \"{url}\"");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = args.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var output = new StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                var result = output.ToString().Trim();
                // yt-dlp may return multiple URLs (video + audio), take first one
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                return lines.Length > 0 ? lines[0] : null;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting stream URL: {ex.Message}");
            return null;
        }
    }

    public async Task<VideoInfo?> GetVideoInfoAsync(string url)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = $"--dump-json \"{url}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var output = new StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                var json = output.ToString().Trim();
                return JsonSerializer.Deserialize<VideoInfo>(json);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting video info: {ex.Message}");
            return null;
        }
    }

    public bool IsSupportedUrl(string url)
    {
        // yt-dlp supports 1000+ sites
        // Just check if it's a URL (starts with http:// or https://)
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}

public class VideoInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("duration")]
    public double? Duration { get; set; }
    
    [JsonPropertyName("extractor")]
    public string? Extractor { get; set; }
    
    [JsonPropertyName("extractor_key")]
    public string? ExtractorKey { get; set; }
}


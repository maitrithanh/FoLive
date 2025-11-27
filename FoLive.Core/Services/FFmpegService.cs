using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace FoLive.Core.Services;

public class FFmpegService
{
    private readonly string _ffmpegPath;
    private readonly LogService? _logger;

    public FFmpegService(LogService? logger = null)
    {
        _logger = logger;
        _ffmpegPath = FindFFmpeg();
        _logger?.LogInfo($"FFmpegService khởi tạo. FFmpeg path: {_ffmpegPath}");
        
        // Kiểm tra FFmpeg ngay khi khởi tạo
        if (!CheckFFmpeg())
        {
            _logger?.LogWarning($"FFmpeg không được tìm thấy hoặc không thể chạy tại: {_ffmpegPath}");
        }
        else
        {
            _logger?.LogInfo($"FFmpeg đã được xác nhận hoạt động tại: {_ffmpegPath}");
        }
    }

    private string FindFFmpeg()
    {
        // Check common paths
        var paths = new List<string>
        {
            "ffmpeg.exe",
            @"C:\ffmpeg\bin\ffmpeg.exe",
            @"C:\Program Files\ffmpeg\bin\ffmpeg.exe"
        };

        // Add paths from PATH environment variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var pathEntries = pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pathEntry in pathEntries)
            {
                var fullPath = Path.Combine(pathEntry, "ffmpeg.exe");
                if (!paths.Contains(fullPath))
                {
                    paths.Add(fullPath);
                }
            }
        }

        // Check each path
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Try to find in PATH by running ffmpeg command
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
            // Nếu _ffmpegPath là "ffmpeg" (từ PATH), thử chạy trực tiếp
            if (_ffmpegPath == "ffmpeg" || _ffmpegPath == "ffmpeg.exe")
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(3000);
                var result = process.ExitCode == 0;
                _logger?.LogInfo($"CheckFFmpeg (PATH): {result}");
                return result;
            }
            
            // Nếu là đường dẫn file cụ thể, kiểm tra file tồn tại
            if (File.Exists(_ffmpegPath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(3000);
                var result = process.ExitCode == 0;
                _logger?.LogInfo($"CheckFFmpeg (file): {result} - {_ffmpegPath}");
                return result;
            }
            
            _logger?.LogWarning($"FFmpeg file không tồn tại: {_ffmpegPath}");
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Lỗi khi kiểm tra FFmpeg: {ex.Message}", ex);
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
        // Validate required parameters
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(streamUrl))
        {
            throw new ArgumentException("Stream URL cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(streamKey))
        {
            throw new ArgumentException("Stream Key cannot be empty");
        }

        var args = new StringBuilder();
        var useRender = config.TryGetValue("use_render", out var render) && render is bool renderValue && renderValue;
        var loop = config.TryGetValue("loop", out var loopValue) && loopValue is bool loopBool && loopBool;

        // Add log level at the beginning
        args.Append($"-loglevel warning ");

        // Input source
        string inputSource = source;
        
        switch (sourceType.ToLower())
        {
            case "file":
                if (!File.Exists(source))
                {
                    throw new FileNotFoundException($"Source file not found: {source}");
                }
                if (loop)
                {
                    args.Append($"-re -stream_loop -1 -i \"{source}\" ");
                }
                else
                {
                    args.Append($"-re -i \"{source}\" ");
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
                        args.Append($"-re -i \"{inputSource}\" ");
                    }
                    else
                    {
                        // Fallback to direct URL
                        args.Append($"-re -i \"{source}\" ");
                    }
                }
                else
                {
                    args.Append($"-re -i \"{source}\" ");
                }
                break;
            case "screen":
                args.Append($"-f gdigrab -framerate 30 -i desktop ");
                // Note: Audio capture may require additional setup
                // For now, stream without audio or add audio source separately
                break;
            default:
                // Generic URL - try yt-dlp first
                if (ytDlpService != null && ytDlpService.IsSupportedUrl(source))
                {
                    var streamUrlFromYtDlp = await ytDlpService.GetStreamUrlAsync(source);
                    if (!string.IsNullOrEmpty(streamUrlFromYtDlp))
                    {
                        args.Append($"-re -i \"{streamUrlFromYtDlp}\" ");
                    }
                    else
                    {
                        args.Append($"-re -i \"{source}\" ");
                    }
                }
                else
                {
                    args.Append($"-re -i \"{source}\" ");
                }
                break;
        }

        // Video filters
        var filters = new List<string>();

        // Resolution/Scale filter (should be first or early in the filter chain)
        if (config.TryGetValue("resolution", out var resolution) && resolution is string resolutionValue && !string.IsNullOrEmpty(resolutionValue))
        {
            // Validate resolution format (e.g., "1920x1080")
            if (System.Text.RegularExpressions.Regex.IsMatch(resolutionValue, @"^\d+x\d+$"))
            {
                filters.Add($"scale={resolutionValue}");
            }
        }

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
            // Escape single quotes in text
            var escapedText = textValue.Replace("'", "\\'");
            filters.Add($"drawtext=text='{escapedText}':fontsize=24:fontcolor=white:x=10:y=10");
        }

        // Apply filters
        if (filters.Count > 0)
        {
            args.Append($"-vf \"{string.Join(",", filters)}\" ");
        }
        else if (useRender)
        {
            // If using render but no filters, check if resolution is set
            // Resolution should already be in filters if specified, but double-check
            if (config.TryGetValue("resolution", out var res) && res is string resValue && !string.IsNullOrEmpty(resValue))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(resValue, @"^\d+x\d+$"))
                {
                    args.Append($"-vf \"scale={resValue}\" ");
                }
            }
        }

        // Audio filters
        if (config.TryGetValue("volume", out var volume) && volume is double volumeValue && volumeValue != 1.0)
        {
            args.Append($"-af volume={volumeValue} ");
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

            // Video encoding
            args.Append($"-c:v libx264 ");
            args.Append($"-preset {preset} ");
            args.Append($"-b:v {bitrate} ");
            args.Append($"-maxrate {bitrate} ");
            args.Append($"-bufsize {bitrate} ");
            args.Append($"-g 50 ");
            args.Append($"-keyint_min 50 ");
            args.Append($"-sc_threshold 0 ");
            args.Append($"-profile:v baseline ");
            args.Append($"-level 3.0 ");
            args.Append($"-pix_fmt yuv420p ");
            
            // Audio encoding
            args.Append($"-c:a aac ");
            args.Append($"-b:a 128k ");
            args.Append($"-ar 44100 ");
            args.Append($"-ac 2 ");
        }
        else
        {
            // Direct stream - no encoding (lighter)
            // Note: This only works if input format matches output
            // For direct copy, we still need to ensure format compatibility
            args.Append($"-c copy ");
            
            // If resolution is specified but no render, we can't scale with copy
            // So we need to use render mode or skip resolution
            if (config.TryGetValue("resolution", out var res) && res is string resValue && !string.IsNullOrEmpty(resValue))
            {
                // With -c copy, we can't scale, so warn or force render
                Console.WriteLine($"[FFmpeg] Warning: Resolution specified but using -c copy. Resolution will be ignored.");
            }
        }

        // Output format
        args.Append($"-f flv ");
        
        // Add timeout and reconnect options for RTMP
        args.Append($"-rtmp_live live ");
        args.Append($"-rtmp_conn_timeout 10 ");
        
        // Output URL (must be last)
        var outputUrl = $"{streamUrl.TrimEnd('/')}/{streamKey}";
        args.Append($"\"{outputUrl}\"");

        var command = args.ToString().Trim();
        
        // Log command for debugging
        Console.WriteLine($"[FFmpeg] Command: {_ffmpegPath} {command}");
        
        return command;
    }

    public Process? StartStream(string command)
    {
        try
        {
            _logger?.LogInfo($"Đang khởi động FFmpeg với command: {command}");
            
            // Check if FFmpeg exists
            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                var errorMsg = "FFmpeg path trống. Vui lòng đảm bảo FFmpeg đã được cài đặt.";
                _logger?.LogError(errorMsg);
                throw new FileNotFoundException(errorMsg);
            }
            
            // Kiểm tra FFmpeg có thể chạy không
            if (!CheckFFmpeg())
            {
                var errorMsg = $"FFmpeg không được tìm thấy hoặc không thể chạy tại: {_ffmpegPath}. Vui lòng đảm bảo FFmpeg đã được cài đặt và có trong PATH.";
                _logger?.LogError(errorMsg);
                throw new FileNotFoundException(errorMsg);
            }
            
            _logger?.LogInfo($"FFmpeg đã được xác nhận tại: {_ffmpegPath}");

            var errorOutput = new StringBuilder();
            var outputData = new StringBuilder();
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                },
                EnableRaisingEvents = true
            };
            
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                    _logger?.LogError($"[FFmpeg Error] {e.Data}");
                }
            };
            
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputData.AppendLine(e.Data);
                    _logger?.LogInfo($"[FFmpeg Output] {e.Data}");
                }
            };
            
            process.Exited += (sender, e) =>
            {
                _logger?.LogWarning($"[FFmpeg] Process exited with code: {process.ExitCode}");
            };
            
            _logger?.LogInfo($"[FFmpeg] Starting process: {_ffmpegPath}");
            _logger?.LogInfo($"[FFmpeg] Arguments: {command}");
            
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            
            // Give process time to initialize
            Thread.Sleep(2000);
            
            if (process.HasExited)
            {
                var error = errorOutput.ToString();
                var output = outputData.ToString();
                
                var errorMsg = $"FFmpeg thoát với mã lỗi {process.ExitCode}";
                if (!string.IsNullOrEmpty(error))
                {
                    errorMsg += $"\n\nLỗi:\n{error}";
                }
                if (!string.IsNullOrEmpty(output))
                {
                    errorMsg += $"\n\nOutput:\n{output}";
                }
                
                _logger?.LogError($"[FFmpeg] {errorMsg}");
                throw new Exception(errorMsg);
            }
            
            _logger?.LogInfo($"[FFmpeg] Process started successfully. PID: {process.Id}");
            return process;
        }
        catch (FileNotFoundException ex)
        {
            _logger?.LogError($"FFmpeg không được tìm thấy: {ex.Message}", ex);
            throw; // Re-throw để caller biết
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Lỗi khi khởi động FFmpeg: {ex.Message}", ex);
            return null;
        }
    }
}


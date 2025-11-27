using System;
using System.IO;
using System.Threading.Tasks;

namespace FoLive.Core.Services;

public class LogService
{
    private readonly string _logPath;
    private readonly object _lock = new object();

    public LogService(string? logPath = null)
    {
        _logPath = logPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FoLive",
            "logs",
            $"folive_{DateTime.Now:yyyyMMdd}.log"
        );

        // Ensure log directory exists
        var logDirectory = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        try
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            
            // Write to console (for debugging)
            Console.WriteLine(logEntry);
            
            // Write to file
            lock (_lock)
            {
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            // If logging fails, at least try to write to console
            Console.WriteLine($"[LogService] Failed to write log: {ex.Message}");
        }
    }

    public void LogInfo(string message) => Log(message, LogLevel.Info);
    public void LogWarning(string message) => Log(message, LogLevel.Warning);
    public void LogError(string message) => Log(message, LogLevel.Error);
    public void LogError(string message, Exception ex) => Log($"{message}: {ex.Message}\nStack trace: {ex.StackTrace}", LogLevel.Error);

    public string GetLogPath() => _logPath;
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}


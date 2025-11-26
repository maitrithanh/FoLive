using System;
using System.IO;
using System.Threading.Tasks;

namespace FoLive.Core.Services;

public class SourceHandlerService
{
    private readonly YtDlpService _ytDlpService;

    public SourceHandlerService()
    {
        _ytDlpService = new YtDlpService();
    }

    public async Task<bool> ValidateSourceAsync(string source, string sourceType)
    {
        return sourceType.ToLower() switch
        {
            "file" => ValidateFileSource(source),
            "youtube" => await _ytDlpService.ValidateUrlAsync(source),
            "playlist" => await _ytDlpService.ValidateUrlAsync(source),
            "facebook" => await _ytDlpService.ValidateUrlAsync(source),
            "url" => await _ytDlpService.ValidateUrlAsync(source),
            "screen" => true, // Screen capture is always valid
            _ => _ytDlpService.IsSupportedUrl(source) && await _ytDlpService.ValidateUrlAsync(source)
        };
    }

    private bool ValidateFileSource(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // Check if file exists and is a supported video format
        var supportedFormats = new[] { ".mp4", ".mov", ".mkv", ".avi", ".flv", ".webm", ".m4v", ".wmv" };
        var extension = Path.GetExtension(path).ToLower();
        return File.Exists(path) && Array.Exists(supportedFormats, ext => ext == extension);
    }

    public string GetSourceInfo(string source, string sourceType)
    {
        return sourceType.ToLower() switch
        {
            "file" => $"File: {Path.GetFileName(source)}",
            "youtube" => $"YouTube: {source}",
            "playlist" => $"Playlist: {source}",
            "facebook" => $"Facebook: {source}",
            "url" => $"URL: {source}",
            "screen" => "Screen Capture",
            _ => $"Source: {source}"
        };
    }

    public bool IsVideoFile(string path)
    {
        var supportedFormats = new[] { ".mp4", ".mov", ".mkv", ".avi", ".flv", ".webm", ".m4v", ".wmv" };
        var extension = Path.GetExtension(path).ToLower();
        return Array.Exists(supportedFormats, ext => ext == extension);
    }
}


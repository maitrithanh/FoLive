using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FoLive.Core.Models;
using StreamModel = FoLive.Core.Models.Stream;

namespace FoLive.Core.Services;

public class ConfigService
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigService(string? configPath = null)
    {
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FoLive",
            "config.json"
        );
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public async Task<List<StreamConfig>> LoadStreamsAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return new List<StreamConfig>();
            }

            var json = await File.ReadAllTextAsync(_configPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<StreamConfig>();
            }

            var configs = JsonSerializer.Deserialize<List<StreamConfig>>(json, _jsonOptions);
            return configs ?? new List<StreamConfig>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return new List<StreamConfig>();
        }
    }

    public async Task SaveStreamsAsync(List<StreamModel> streams)
    {
        try
        {
            // Convert streams to configs (exclude runtime data)
            var configs = streams.Select(s => new StreamConfig
            {
                StreamId = s.StreamId,
                Source = s.Source,
                SourceType = s.SourceType,
                StreamKey = s.StreamKey,
                StreamUrl = s.StreamUrl,
                Config = s.Config
            }).ToList();

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(configs, _jsonOptions);
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }

    public List<StreamModel> ConvertConfigsToStreams(List<StreamConfig> configs)
    {
        return configs.Select(c => new StreamModel
        {
            StreamId = c.StreamId,
            Source = c.Source,
            SourceType = c.SourceType,
            StreamKey = c.StreamKey,
            StreamUrl = c.StreamUrl,
            Config = c.Config,
            Status = StreamStatus.Idle,
            Process = null,
            StartTime = null,
            ErrorMessage = null,
            Stats = new StreamStats()
        }).ToList();
    }
}

public class StreamConfig
{
    public string StreamId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string StreamKey { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public Dictionary<string, object> Config { get; set; } = new();
}


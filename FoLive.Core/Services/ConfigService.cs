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
    private readonly LogService? _logger;

    public ConfigService(string? configPath = null, LogService? logger = null)
    {
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FoLive",
            "config.json"
        );
        
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Gets the config file path
    /// </summary>
    public string GetConfigPath()
    {
        return _configPath;
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
            _logger?.LogError($"Error loading config: {ex.Message}", ex);
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
                _logger?.LogInfo($"Created directory: {directory}");
            }

            var json = JsonSerializer.Serialize(configs, _jsonOptions);
            await File.WriteAllTextAsync(_configPath, json);
            _logger?.LogInfo($"Saved config to: {_configPath}");
            _logger?.LogInfo($"Saved {configs.Count} stream(s)");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error saving config to {_configPath}: {ex.Message}";
            _logger?.LogError(errorMsg, ex);
            // Don't throw - log the error but don't break the flow
            // The caller can check if file exists to verify save was successful
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

    /// <summary>
    /// Gets the API base URL from config or environment variable
    /// </summary>
    public string GetApiBaseUrl()
    {
        // First check environment variable
        var envUrl = Environment.GetEnvironmentVariable("FOLIVE_API_BASE_URL");
        if (!string.IsNullOrWhiteSpace(envUrl))
        {
            return envUrl;
        }

        // Then check config file
        try
        {
            var configDir = Path.GetDirectoryName(_configPath);
            var apiConfigPath = Path.Combine(configDir ?? "", "api_config.json");
            if (File.Exists(apiConfigPath))
            {
                var json = File.ReadAllText(apiConfigPath);
                var apiConfig = JsonSerializer.Deserialize<ApiConfig>(json, _jsonOptions);
                if (apiConfig != null && !string.IsNullOrWhiteSpace(apiConfig.BaseUrl))
                {
                    return apiConfig.BaseUrl;
                }
            }
        }
        catch
        {
            // Ignore errors, use default
        }

        // Default fallback
        return "https://folive-web.vercel.app";
    }

    /// <summary>
    /// Saves the API base URL to config file
    /// </summary>
    public async Task SaveApiBaseUrlAsync(string baseUrl)
    {
        try
        {
            var configDir = Path.GetDirectoryName(_configPath);
            if (string.IsNullOrEmpty(configDir))
                return;

            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var apiConfigPath = Path.Combine(configDir, "api_config.json");
            var apiConfig = LoadApiConfig();
            apiConfig.BaseUrl = baseUrl;
            var json = JsonSerializer.Serialize(apiConfig, _jsonOptions);
            await File.WriteAllTextAsync(apiConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving API config: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the Software API Key from config or environment variable
    /// </summary>
    public string GetSoftwareApiKey()
    {
        // First check environment variable
        var envKey = Environment.GetEnvironmentVariable("FOLIVE_SOFTWARE_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            return envKey;
        }

        // Then check config file
        try
        {
            var apiConfig = LoadApiConfig();
            if (!string.IsNullOrWhiteSpace(apiConfig.SoftwareApiKey))
            {
                return apiConfig.SoftwareApiKey;
            }
        }
        catch
        {
            // Ignore errors, return empty
        }

        // Default Software API Key (fallback)
        return "w5UnHtnKYAwa3k06PJIhF1u0nA9FBS8bZQ8iHSE";
    }

    /// <summary>
    /// Saves the Software API Key to config file
    /// </summary>
    public async Task SaveSoftwareApiKeyAsync(string apiKey)
    {
        try
        {
            var configDir = Path.GetDirectoryName(_configPath);
            if (string.IsNullOrEmpty(configDir))
                return;

            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var apiConfigPath = Path.Combine(configDir, "api_config.json");
            var apiConfig = LoadApiConfig();
            apiConfig.SoftwareApiKey = apiKey;
            var json = JsonSerializer.Serialize(apiConfig, _jsonOptions);
            await File.WriteAllTextAsync(apiConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving API key: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads API configuration from file (synchronous version for use in synchronous methods)
    /// </summary>
    private ApiConfig LoadApiConfig()
    {
        try
        {
            var configDir = Path.GetDirectoryName(_configPath);
            var apiConfigPath = Path.Combine(configDir ?? "", "api_config.json");
            if (File.Exists(apiConfigPath))
            {
                var json = File.ReadAllText(apiConfigPath);
                var apiConfig = JsonSerializer.Deserialize<ApiConfig>(json, _jsonOptions);
                if (apiConfig != null)
                {
                    return apiConfig;
                }
            }
        }
        catch
        {
            // Ignore errors, return default
        }

        return new ApiConfig();
    }
}

public class ApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string SoftwareApiKey { get; set; } = string.Empty;
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


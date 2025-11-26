using System;
using System.Text.Json.Serialization;

namespace FoLive.Core.Models;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("userType")]
    public string UserType { get; set; } = string.Empty; // "promonth", etc.
    
    [JsonPropertyName("accountStatus")]
    public string AccountStatus { get; set; } = string.Empty; // "active", etc.
    
    [JsonPropertyName("isExpired")]
    public bool IsExpired { get; set; }
    
    [JsonPropertyName("packageDetails")]
    public PackageDetails? PackageDetails { get; set; }
    
    // Internal fields
    public string? Token { get; set; }
    public DateTime? TokenExpiry { get; set; }
}

public class PackageDetails
{
    [JsonPropertyName("plan")]
    public string Plan { get; set; } = string.Empty;
    
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
    
    [JsonPropertyName("maxStreams")]
    public int MaxStreams { get; set; } = 1;
    
    [JsonPropertyName("canUseAdvancedFeatures")]
    public bool CanUseAdvancedFeatures { get; set; }
    
    [JsonPropertyName("canUseScreenCapture")]
    public bool CanUseScreenCapture { get; set; }
    
    [JsonPropertyName("canUseMultipleSources")]
    public bool CanUseMultipleSources { get; set; }
}

public class AuthResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    
    [JsonPropertyName("user")]
    public User? User { get; set; }
}

public class StatusResponse
{
    [JsonPropertyName("isExpired")]
    public bool IsExpired { get; set; }
    
    [JsonPropertyName("isExpiringSoon")]
    public bool IsExpiringSoon { get; set; }
    
    [JsonPropertyName("daysUntilExpiry")]
    public int? DaysUntilExpiry { get; set; }
    
    [JsonPropertyName("user")]
    public User? User { get; set; }
}

// Legacy Subscription class for backward compatibility
public class Subscription
{
    public string Plan { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public int MaxStreams { get; set; } = 1;
    public bool CanUseAdvancedFeatures { get; set; } = false;
    public bool CanUseScreenCapture { get; set; } = false;
    public bool CanUseMultipleSources { get; set; } = false;
}




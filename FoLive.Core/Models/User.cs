using System;

namespace FoLive.Core.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? TokenExpiry { get; set; }
    public Subscription? Subscription { get; set; }
}

public class Subscription
{
    public string Plan { get; set; } = string.Empty; // "free", "monthly", "yearly", "lifetime"
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public int MaxStreams { get; set; } = 1; // Default free plan: 1 stream
    public bool CanUseAdvancedFeatures { get; set; } = false;
    public bool CanUseScreenCapture { get; set; } = false;
    public bool CanUseMultipleSources { get; set; } = false;
}




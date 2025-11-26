using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FoLive.Core.Models;

namespace FoLive.Core.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;
    private User? _currentUser;

    public AuthService(string? baseUrl = null)
    {
        _baseUrl = baseUrl ?? "https://folive-web.vercel.app";
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FoLive/1.0");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public User? CurrentUser => _currentUser;

    /// <summary>
    /// Sanitizes email input by trimming and converting to lowercase
    /// </summary>
    private string SanitizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;
        
        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Authenticates user with email and password
    /// POST /api/software/auth
    /// </summary>
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            var sanitizedEmail = SanitizeEmail(email);
            
            var requestBody = new
            {
                email = sanitizedEmail,
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/software/auth", requestBody, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
            
            if (authResponse?.Success == true && authResponse.User != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                authResponse.User.Token = authResponse.Token;
                _currentUser = authResponse.User;
            }

            return authResponse ?? new AuthResponse { Success = false };
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Authentication failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Authentication request timed out");
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error during authentication: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Verifies token and returns user information
    /// GET /api/software/auth?token=xxx
    /// </summary>
    public async Task<AuthResponse> VerifyTokenAsync(string token)
    {
        try
        {
            var encodedToken = Uri.EscapeDataString(token);
            var response = await _httpClient.GetAsync($"/api/software/auth?token={encodedToken}");
            response.EnsureSuccessStatusCode();

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
            
            if (authResponse?.Success == true && authResponse.User != null)
            {
                authResponse.User.Token = token;
                _currentUser = authResponse.User;
            }

            return authResponse ?? new AuthResponse { Success = false };
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Token verification failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Token verification request timed out");
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error during token verification: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks package status by email
    /// GET /api/software/status?email=xxx
    /// </summary>
    public async Task<StatusResponse> GetStatusByEmailAsync(string email)
    {
        try
        {
            var sanitizedEmail = SanitizeEmail(email);
            var encodedEmail = Uri.EscapeDataString(sanitizedEmail);
            var response = await _httpClient.GetAsync($"/api/software/status?email={encodedEmail}");
            response.EnsureSuccessStatusCode();

            var statusResponse = await response.Content.ReadFromJsonAsync<StatusResponse>(_jsonOptions);
            return statusResponse ?? new StatusResponse();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Status check failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Status check request timed out");
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error during status check: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks package status by token
    /// GET /api/software/status?token=xxx
    /// </summary>
    public async Task<StatusResponse> GetStatusByTokenAsync(string token)
    {
        try
        {
            var encodedToken = Uri.EscapeDataString(token);
            var response = await _httpClient.GetAsync($"/api/software/status?token={encodedToken}");
            response.EnsureSuccessStatusCode();

            var statusResponse = await response.Content.ReadFromJsonAsync<StatusResponse>(_jsonOptions);
            return statusResponse ?? new StatusResponse();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Status check failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Status check request timed out");
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error during status check: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks package status by email (POST method)
    /// POST /api/software/status
    /// </summary>
    public async Task<StatusResponse> CheckStatusByEmailPostAsync(string email)
    {
        try
        {
            var sanitizedEmail = SanitizeEmail(email);
            var requestBody = new { email = sanitizedEmail };
            
            var response = await _httpClient.PostAsJsonAsync("/api/software/status", requestBody, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var statusResponse = await response.Content.ReadFromJsonAsync<StatusResponse>(_jsonOptions);
            return statusResponse ?? new StatusResponse();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Status check failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Status check request timed out");
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error during status check: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks package status by token (POST method)
    /// POST /api/software/status
    /// </summary>
    public async Task<StatusResponse> CheckStatusByTokenPostAsync(string token)
    {
        try
        {
            var requestBody = new { token = token };
            
            var response = await _httpClient.PostAsJsonAsync("/api/software/status", requestBody, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var statusResponse = await response.Content.ReadFromJsonAsync<StatusResponse>(_jsonOptions);
            return statusResponse ?? new StatusResponse();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Status check failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Status check request timed out");
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error during status check: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks status for current user
    /// </summary>
    public async Task<StatusResponse> CheckCurrentUserStatusAsync()
    {
        if (_currentUser == null || string.IsNullOrEmpty(_currentUser.Token))
        {
            throw new InvalidOperationException("No user is currently logged in");
        }

        return await GetStatusByTokenAsync(_currentUser.Token);
    }

    /// <summary>
    /// Logs out the current user
    /// </summary>
    public void Logout()
    {
        _currentUser = null;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

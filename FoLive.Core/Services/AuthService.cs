using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FoLive.Core.Models;

namespace FoLive.Core.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private User? _currentUser;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(string? baseUrl = null)
    {
        // Default API base URL: https://folive-web.vercel.app
        // Endpoints sẽ là: /api/auth/login, /api/user/profile, etc.
        _baseUrl = baseUrl ?? "https://folive-web.vercel.app";
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public User? CurrentUser => _currentUser;

    public bool IsLoggedIn => _currentUser != null && 
                             !string.IsNullOrEmpty(_currentUser.Token) &&
                             (_currentUser.TokenExpiry == null || _currentUser.TokenExpiry > DateTime.Now);

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new
            {
                username = username,
                password = password
            };

            var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
                if (loginResponse != null && loginResponse.Success)
                {
                    _currentUser = new User
                    {
                        Id = loginResponse.User?.Id ?? "",
                        Username = loginResponse.User?.Username ?? username,
                        Email = loginResponse.User?.Email ?? "",
                        Token = loginResponse.Token,
                        TokenExpiry = loginResponse.ExpiresAt,
                        Subscription = loginResponse.User?.Subscription
                    };

                    // Set authorization header for future requests
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_currentUser.Token}");

                    return new LoginResult { Success = true, Message = "Đăng nhập thành công" };
                }
            }

            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
            return new LoginResult
            {
                Success = false,
                Message = errorResponse?.Message ?? "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin."
            };
        }
        catch (HttpRequestException ex)
        {
            return new LoginResult
            {
                Success = false,
                Message = $"Lỗi kết nối: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                Success = false,
                Message = $"Lỗi: {ex.Message}"
            };
        }
    }

    public async Task<bool> RefreshUserInfoAsync()
    {
        if (!IsLoggedIn) return false;

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_currentUser!.Token}");

            var response = await _httpClient.GetAsync("/api/user/profile");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<UserResponse>(content, _jsonOptions);
                
                if (userResponse != null && userResponse.User != null)
                {
                    _currentUser.Id = userResponse.User.Id;
                    _currentUser.Username = userResponse.User.Username;
                    _currentUser.Email = userResponse.User.Email;
                    _currentUser.Subscription = userResponse.User.Subscription;
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing user info: {ex.Message}");
        }

        return false;
    }

    public void Logout()
    {
        _currentUser = null;
        _httpClient.DefaultRequestHeaders.Clear();
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
}

public class UserResponse
{
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Subscription? Subscription { get; set; }
}

public class ErrorResponse
{
    public string? Message { get; set; }
}



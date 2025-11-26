using System;
using System.Windows;
using FoLive.Core.Services;
using FoLive.Views;

namespace FoLive;

public partial class App : Application
{
    public static AuthService AuthService { get; private set; } = null!;
    public static SubscriptionService SubscriptionService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Lấy API base URL từ environment variable hoặc sử dụng default
        // Có thể set: export FOLIVE_API_URL=http://localhost:3000
        // Default: https://folive-web.vercel.app
        var apiUrl = Environment.GetEnvironmentVariable("FOLIVE_API_URL");
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            // Nếu không có env var, sử dụng production API
            // Production: https://folive-web.vercel.app
            // Local dev: http://localhost:3000 (set FOLIVE_API_URL env var)
            apiUrl = "https://folive-web.vercel.app"; // Default production API base URL
        }
        
        // Initialize services với API URL
        AuthService = new AuthService(apiUrl);
        SubscriptionService = new SubscriptionService(AuthService);
        
        // Show login window first
        var loginWindow = new LoginWindow(AuthService);
        if (loginWindow.ShowDialog() == true)
        {
            // Login successful, show main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        else
        {
            // Login cancelled or failed, exit app
            Shutdown();
        }
    }
}


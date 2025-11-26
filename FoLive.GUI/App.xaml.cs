using System;
using System.Windows;
using FoLive.Core.Services;
using FoLive.Views;

namespace FoLive
{
    public partial class App : Application
    {
        private AuthService? _authService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize AuthService with API base URL and API key
            var configService = new ConfigService();
            var apiBaseUrl = configService.GetApiBaseUrl();
            var softwareApiKey = configService.GetSoftwareApiKey();
            _authService = new AuthService(apiBaseUrl, softwareApiKey);
            
            // Show login window first
            var loginWindow = new LoginWindow(_authService);
            if (loginWindow.ShowDialog() == true && loginWindow.AuthenticatedUser != null)
            {
                // User authenticated successfully, show main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // User cancelled login or authentication failed
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _authService?.Dispose();
            base.OnExit(e);
        }
    }
}

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
            
            try
            {
                // Show login window first
                var loginWindow = new LoginWindow(_authService);
                var dialogResult = loginWindow.ShowDialog();
                
                if (dialogResult == true && loginWindow.AuthenticatedUser != null)
                {
                    // User authenticated successfully, show main window
                    try
                    {
                        var mainWindow = new MainWindow();
                        MainWindow = mainWindow; // Set as main window of application
                        
                        // Change shutdown mode to OnMainWindowClose so app closes when main window closes
                        ShutdownMode = ShutdownMode.OnMainWindowClose;
                        
                        mainWindow.Show();
                        mainWindow.Activate(); // Bring window to front
                    }
                    catch (Exception mainWindowEx)
                    {
                        MessageBox.Show($"Error creating main window: {mainWindowEx.Message}\n\n{mainWindowEx.StackTrace}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Shutdown();
                    }
                }
                else
                {
                    // User cancelled login or authentication failed
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}\n\n{ex.StackTrace}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

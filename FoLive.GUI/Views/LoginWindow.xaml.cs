using System;
using System.Windows;
using FoLive.Core.Models;
using FoLive.Core.Services;

namespace FoLive.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;
        public User? AuthenticatedUser { get; private set; }

        public LoginWindow(AuthService? authService = null)
        {
            InitializeComponent();
            _authService = authService ?? new AuthService();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var email = UsernameTextBox.Text;
                var password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please enter your email address.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter your password.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Disable UI during login
                UsernameTextBox.IsEnabled = false;
                PasswordBox.IsEnabled = false;
                var loginButton = sender as System.Windows.Controls.Button;
                if (loginButton != null)
                {
                    loginButton.IsEnabled = false;
                    loginButton.Content = "Logging in...";
                }

                var response = await _authService.LoginAsync(email, password);

                if (response.Success && response.User != null)
                {
                    AuthenticatedUser = response.User;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid email or password. Please try again.", "Login Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Re-enable UI
                    UsernameTextBox.IsEnabled = true;
                    PasswordBox.IsEnabled = true;
                    if (loginButton != null)
                    {
                        loginButton.IsEnabled = true;
                        loginButton.Content = "Login";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Re-enable UI
                UsernameTextBox.IsEnabled = true;
                PasswordBox.IsEnabled = true;
                var loginButton = sender as System.Windows.Controls.Button;
                if (loginButton != null)
                {
                    loginButton.IsEnabled = true;
                    loginButton.Content = "Login";
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

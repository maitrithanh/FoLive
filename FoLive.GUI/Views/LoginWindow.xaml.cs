using System;
using System.Windows;
using System.Windows.Input;
using FoLive.Core.Services;

namespace FoLive.Views;

public partial class LoginWindow : Window
{
    private readonly AuthService _authService;

    public LoginWindow(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        UsernameTextBox.Focus();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        await PerformLogin();
    }

    private async void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PasswordBox.Focus();
        }
    }

    private async void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await PerformLogin();
        }
    }

    private async System.Threading.Tasks.Task PerformLogin()
    {
        var username = UsernameTextBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Vui lòng nhập tên đăng nhập hoặc email.");
            UsernameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Vui lòng nhập mật khẩu.");
            PasswordBox.Focus();
            return;
        }

        // Disable UI during login
        UsernameTextBox.IsEnabled = false;
        PasswordBox.IsEnabled = false;
        ErrorMessageText.Visibility = Visibility.Collapsed;

        try
        {
            var result = await _authService.LoginAsync(username, password);
            
            if (result.Success)
            {
                // Refresh user info to get subscription
                await _authService.RefreshUserInfoAsync();
                
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError(result.Message);
                UsernameTextBox.IsEnabled = true;
                PasswordBox.IsEnabled = true;
                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Lỗi: {ex.Message}");
            UsernameTextBox.IsEnabled = true;
            PasswordBox.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessageText.Text = message;
        ErrorMessageText.Visibility = Visibility.Visible;
    }

    private void Register_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show(
            "Vui lòng đăng ký tại website: https://folive-web.vercel.app/register\n\n" +
            "Hoặc liên hệ: dev@fotech.pro",
            "Đăng ký tài khoản",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show(
            "Vui lòng khôi phục mật khẩu tại: https://folive-web.vercel.app/forgot-password\n\n" +
            "Hoặc liên hệ: dev@fotech.pro",
            "Quên mật khẩu",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}



using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FoLive.Core.Models;
using FoLive.Core.Services;
using FoLive.ViewModels;
using StreamModel = FoLive.Core.Models.Stream;

namespace FoLive.Views;

public partial class MainWindow : Window
{
    private readonly StreamManager _streamManager;
    private readonly FFmpegService _ffmpegService;
    private readonly SubscriptionService _subscriptionService;
    private readonly ObservableCollection<StreamViewModel> _streams;
    private readonly DispatcherTimer _refreshTimer;
    private readonly DispatcherTimer _authCheckTimer;

    public MainWindow()
    {
        InitializeComponent();
        
        _streamManager = new StreamManager();
        _ffmpegService = new FFmpegService();
        _subscriptionService = App.SubscriptionService;
        _streams = new ObservableCollection<StreamViewModel>();
        
        StreamsDataGrid.ItemsSource = _streams;
        
        _streamManager.StreamStatusChanged += OnStreamStatusChanged;
        
        // Setup refresh timer
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();
        
        // Setup auth check timer (kiểm tra token expiry mỗi phút)
        _authCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _authCheckTimer.Tick += AuthCheckTimer_Tick;
        _authCheckTimer.Start();
        
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Update user info in footer
        UpdateUserInfo();
        
        // Load saved configuration
        await _streamManager.LoadConfigAsync();
        await RefreshStreams();
        await CheckSystemStatus();
    }

    private void UpdateUserInfo()
    {
        var user = App.AuthService.CurrentUser;
        if (user != null)
        {
            var planName = _subscriptionService.GetPlanName();
            UserInfoText.Text = $"{user.Username} - {planName}";
        }
        else
        {
            UserInfoText.Text = "Chưa đăng nhập";
        }
    }

    private Task CheckSystemStatus()
    {
        var ffmpegOk = _ffmpegService.CheckFFmpeg();
        // Status will be shown in footer or removed if not needed
        return Task.CompletedTask;
    }

    private async Task RefreshStreams()
    {
        var streams = await _streamManager.GetAllStreamsAsync();
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Update existing or add new
            var existingIds = _streams.Select(s => s.StreamId).ToList();
            var newIds = streams.Select(s => s.StreamId).ToList();
            
            // Remove deleted streams
            var toRemove = _streams.Where(s => !newIds.Contains(s.StreamId)).ToList();
            foreach (var item in toRemove)
            {
                _streams.Remove(item);
            }
            
            // Update or add streams
            int index = 1;
            foreach (var stream in streams)
            {
                var existing = _streams.FirstOrDefault(s => s.StreamId == stream.StreamId);
                if (existing != null)
                {
                    existing.Update(stream);
                    if (existing.Index != index)
                    {
                        existing.Index = index;
                    }
                }
                else
                {
                    _streams.Add(new StreamViewModel(stream, index));
                }
                index++;
            }
            
            // Sort by index
            var sorted = _streams.OrderBy(s => s.Index).ToList();
            _streams.Clear();
            foreach (var item in sorted)
            {
                _streams.Add(item);
            }
        });
    }

    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        _ = RefreshStreams();
        _ = CheckSystemStatus();
    }

    private async void AuthCheckTimer_Tick(object? sender, EventArgs e)
    {
        // Kiểm tra token expiry
        if (!App.AuthService.IsLoggedIn)
        {
            // Token đã hết hạn hoặc chưa đăng nhập
            var result = MessageBox.Show(
                "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                "Phiên đăng nhập hết hạn",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.OK)
            {
                // Đóng ứng dụng để user đăng nhập lại
                Application.Current.Shutdown();
            }
        }
        else
        {
            // Cập nhật thông tin user định kỳ
            await App.AuthService.RefreshUserInfoAsync();
            UpdateUserInfo();
        }
    }

    private void OnStreamStatusChanged(object? sender, StreamEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _streams.FirstOrDefault(s => s.StreamId == e.Stream.StreamId);
            if (existing != null)
            {
                existing.Update(e.Stream);
                // Trigger property change notification
                var index = _streams.IndexOf(existing);
                _streams[index] = existing;
                
                // Show error message if status is Error
                if (e.Stream.Status == StreamStatus.Error && !string.IsNullOrEmpty(e.Stream.ErrorMessage))
                {
                    MessageBox.Show(
                        $"Stream '{e.Stream.StreamId}' Error:\n\n{e.Stream.ErrorMessage}",
                        "Stream Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        });
    }

    private async void AddStream_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Check subscription limit
        var streams = await _streamManager.GetAllStreamsAsync();
        if (!_subscriptionService.CanAddStream(streams.Count))
        {
            var maxStreams = _subscriptionService.GetMaxStreams();
            MessageBox.Show(
                _subscriptionService.GetFeatureRestrictionMessage("add_stream") + 
                $"\n\nGiới hạn hiện tại: {maxStreams} luồng",
                "Giới hạn gói dịch vụ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var dialog = new AddStreamDialog(_subscriptionService);
        if (dialog.ShowDialog() == true)
        {
            var stream = dialog.GetStream();
            if (stream != null)
            {
                await _streamManager.AddStreamAsync(stream);
                await RefreshStreams();
            }
        }
    }

    private async void StartStream_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            try
            {
                var success = await _streamManager.StartStreamAsync(streamId);
                await RefreshStreams();
                
                // Get updated stream to check for errors
                var stream = await _streamManager.GetStreamAsync(streamId);
                
                // Status will be updated via OnStreamStatusChanged
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error starting stream '{streamId}':\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private async void StopStream_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            await _streamManager.StopStreamAsync(streamId);
            await RefreshStreams();
        }
    }

    private async void DeleteStream_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete stream '{streamId}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await _streamManager.RemoveStreamAsync(streamId);
                await RefreshStreams();
            }
        }
    }

    private async void AddScreenStream_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Check if screen capture is allowed
        if (!_subscriptionService.CanUseScreenCapture())
        {
            MessageBox.Show(
                _subscriptionService.GetFeatureRestrictionMessage("screen_capture"),
                "Tính năng không khả dụng",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Check subscription limit
        var streams = await _streamManager.GetAllStreamsAsync();
        if (!_subscriptionService.CanAddStream(streams.Count))
        {
            var maxStreams = _subscriptionService.GetMaxStreams();
            MessageBox.Show(
                _subscriptionService.GetFeatureRestrictionMessage("add_stream") + 
                $"\n\nGiới hạn hiện tại: {maxStreams} luồng",
                "Giới hạn gói dịch vụ",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var dialog = new AddStreamDialog(_subscriptionService);
        // Pre-select screen capture type
        if (dialog.ShowDialog() == true)
        {
            var stream = dialog.GetStream();
            if (stream != null)
            {
                stream.SourceType = "screen";
                stream.Source = "desktop";
                await _streamManager.AddStreamAsync(stream);
                await RefreshStreams();
            }
        }
    }

    private async void StartAllStreams_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var streams = await _streamManager.GetAllStreamsAsync();
        foreach (var stream in streams)
        {
            if (stream.Status != StreamStatus.Running)
            {
                await _streamManager.StartStreamAsync(stream.StreamId);
            }
        }
        await RefreshStreams();
    }

    private async void StopAllStreams_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var streams = await _streamManager.GetAllStreamsAsync();
        foreach (var stream in streams)
        {
            if (stream.Status == StreamStatus.Running)
            {
                await _streamManager.StopStreamAsync(stream.StreamId);
            }
        }
        await RefreshStreams();
    }

    private void ViewLog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            MessageBox.Show($"Log viewer for stream '{streamId}' - To be implemented", "View Log", MessageBoxButton.OK);
        }
    }

    private async void EditStream_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            var stream = await _streamManager.GetStreamAsync(streamId);
            if (stream != null)
            {
                var dialog = new AddStreamDialog(_subscriptionService);
                // TODO: Pre-fill dialog with stream data
                if (dialog.ShowDialog() == true)
                {
                    var updatedStream = dialog.GetStream();
                    if (updatedStream != null)
                    {
                        // Update stream (will save config automatically)
                        await _streamManager.UpdateStreamAsync(updatedStream);
                        await RefreshStreams();
                    }
                }
            }
        }
    }

    private void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Change password - To be implemented", "Change Password", MessageBoxButton.OK);
    }

    private void Renew_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra đăng nhập trước
        if (!App.AuthService.IsLoggedIn)
        {
            MessageBox.Show(
                "Vui lòng đăng nhập để sử dụng tính năng này.",
                "Yêu cầu đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var paymentWindow = new PaymentWindow();
        paymentWindow.Owner = this;
        paymentWindow.ShowDialog();
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Bạn có chắc chắn muốn đăng xuất?",
            "Xác nhận đăng xuất",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            App.AuthService.Logout();
            MessageBox.Show(
                "Đã đăng xuất thành công. Ứng dụng sẽ đóng.",
                "Đăng xuất",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            // Đóng ứng dụng hoặc hiển thị lại login window
            Application.Current.Shutdown();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer?.Stop();
        _authCheckTimer?.Stop();
        
        // Save config before closing
        _ = Task.Run(async () =>
        {
            try
            {
                await _streamManager.SaveConfigNowAsync();
                Console.WriteLine("[MainWindow] Config saved on exit");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainWindow] Error saving config on exit: {ex.Message}");
            }
        });
        
        base.OnClosed(e);
    }
}


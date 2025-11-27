using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FoLive.Core.Models;
using FoLive.Core.Services;
using FoLive.ViewModels;

namespace FoLive.Views
{
    public partial class MainWindow : Window
    {
        private readonly StreamManager _streamManager;
        private readonly ObservableCollection<StreamViewModel> _streams;
        private System.Windows.Threading.DispatcherTimer? _refreshTimer;

        private readonly LogService _logger;

        public MainWindow()
        {
            InitializeComponent();
            _logger = new LogService();
            _streamManager = new StreamManager(logger: _logger);
            _streams = new ObservableCollection<StreamViewModel>();
            StreamsDataGrid.ItemsSource = _streams;
            
            // Log startup
            _logger.LogInfo("FoLive application started");
            _logger.LogInfo($"Log file: {_logger.GetLogPath()}");

            // Subscribe to stream status changes
            _streamManager.StreamStatusChanged += StreamManager_StreamStatusChanged;

            // Load streams on startup
            Loaded += MainWindow_Loaded;

            // Setup refresh timer to update UI every 3 seconds
            _refreshTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStreamsAsync();
            
            // Show config file path and log path in status bar
            var configService = new ConfigService(logger: _logger);
            var configPath = configService.GetConfigPath();
            var logPath = _logger.GetLogPath();
            StatusTextBlock.Text = $"Config: {configPath} | Log: {logPath} | Đã tải {_streams.Count} stream(s)";
            _logger.LogInfo($"Đã tải {_streams.Count} stream(s) từ config");
        }

        private async Task LoadStreamsAsync()
        {
            try
            {
                await _streamManager.LoadConfigAsync();
                var allStreams = await _streamManager.GetAllStreamsAsync();
                
                _streams.Clear();
                int index = 0;
                foreach (var stream in allStreams)
                {
                    _streams.Add(new StreamViewModel(stream, index++));
                }

                StatusTextBlock.Text = $"Đã tải {_streams.Count} stream(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải streams: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StreamManager_StreamStatusChanged(object? sender, StreamEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var viewModel = _streams.FirstOrDefault(vm => vm.StreamId == e.Stream.StreamId);
                if (viewModel != null)
                {
                    viewModel.Update(e.Stream);
                }
            });
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            // Refresh all stream view models to update time display, etc.
            foreach (var viewModel in _streams)
            {
                viewModel.Update(viewModel.Stream);
            }
        }

        private async void AddStream_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddStreamDialog();
            if (dialog.ShowDialog() == true && dialog.CreatedStream != null)
            {
                try
                {
                    _logger?.LogInfo($"Đang thêm stream: {dialog.CreatedStream.StreamId}");
                    var success = await _streamManager.AddStreamAsync(dialog.CreatedStream);
                    if (success)
                    {
                        // Reload streams from manager to ensure sync
                        await RefreshStreamsListAsync();
                        
                        // Show config path in status
                        var configService = new ConfigService(logger: _logger);
                        var configPath = configService.GetConfigPath();
                        StatusTextBlock.Text = $"Đã thêm stream '{dialog.CreatedStream.StreamId}' thành công | Config: {configPath}";
                        _logger?.LogInfo($"Đã thêm stream '{dialog.CreatedStream.StreamId}' thành công");
                    }
                    else
                    {
                        var errorMsg = $"Stream với ID '{dialog.CreatedStream.StreamId}' đã tồn tại. Vui lòng chọn ID khác.";
                        _logger?.LogWarning(errorMsg);
                        MessageBox.Show(errorMsg, "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Lỗi khi thêm stream: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMsg += $"\n\nChi tiết: {ex.InnerException.Message}";
                    }
                    _logger?.LogError(errorMsg, ex);
                    
                    var configService = new ConfigService(logger: _logger);
                    var configPath = configService.GetConfigPath();
                    var logPath = _logger?.GetLogPath();
                    
                    var fullErrorMsg = $"{errorMsg}\n\nĐường dẫn config: {configPath}";
                    if (!string.IsNullOrEmpty(logPath))
                    {
                        fullErrorMsg += $"\nĐường dẫn log: {logPath}";
                    }
                    
                    MessageBox.Show(fullErrorMsg, "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task RefreshStreamsListAsync()
        {
            try
            {
                var allStreams = await _streamManager.GetAllStreamsAsync();
                
                // Update on UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    // Update existing view models or add new ones
                    var existingIds = _streams.Select(vm => vm.StreamId).ToHashSet();
                    var managerIds = allStreams.Select(s => s.StreamId).ToHashSet();
                    
                    // Remove streams that no longer exist in manager
                    var toRemove = _streams.Where(vm => !managerIds.Contains(vm.StreamId)).ToList();
                    foreach (var vm in toRemove)
                    {
                        _streams.Remove(vm);
                    }
                    
                    // Update or add streams
                    int index = 0;
                    foreach (var stream in allStreams)
                    {
                        var existingViewModel = _streams.FirstOrDefault(vm => vm.StreamId == stream.StreamId);
                        if (existingViewModel != null)
                        {
                            // Update existing view model
                            existingViewModel.Update(stream);
                        }
                        else
                        {
                            // Add new view model
                            _streams.Add(new StreamViewModel(stream, index));
                        }
                        index++;
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Lỗi khi làm mới danh sách streams: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async void StartAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allStreams = await _streamManager.GetAllStreamsAsync();
                var streamsToStart = allStreams.Where(s => s.Status != StreamStatus.Running).ToList();
                
                foreach (var stream in streamsToStart)
                {
                    await _streamManager.StartStreamAsync(stream.StreamId);
                }

                StatusTextBlock.Text = $"Đang bắt đầu {streamsToStart.Count} stream(s)...";
            }
            catch (Exception ex)
            {
                    MessageBox.Show($"Lỗi khi bắt đầu streams: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StopAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allStreams = await _streamManager.GetAllStreamsAsync();
                var streamsToStop = allStreams.Where(s => s.Status == StreamStatus.Running).ToList();
                
                foreach (var stream in streamsToStop)
                {
                    await _streamManager.StopStreamAsync(stream.StreamId);
                }

                StatusTextBlock.Text = $"Đang dừng {streamsToStop.Count} stream(s)...";
            }
            catch (Exception ex)
            {
                    MessageBox.Show($"Lỗi khi dừng streams: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StartStream_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is StreamViewModel viewModel)
            {
                try
                {
                    await _streamManager.StartStreamAsync(viewModel.StreamId);
                    StatusTextBlock.Text = $"Đang bắt đầu stream '{viewModel.StreamId}'...";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi bắt đầu stream: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void StopStream_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is StreamViewModel viewModel)
            {
                try
                {
                    await _streamManager.StopStreamAsync(viewModel.StreamId);
                    StatusTextBlock.Text = $"Đang dừng stream '{viewModel.StreamId}'...";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi dừng stream: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FoLive - Quản lý Stream\nPhiên bản 3.0.10", "Giới thiệu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Stop();
            base.OnClosed(e);
        }
    }
}

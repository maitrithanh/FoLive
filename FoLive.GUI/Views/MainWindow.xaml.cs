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

        public MainWindow()
        {
            InitializeComponent();
            _streamManager = new StreamManager();
            _streams = new ObservableCollection<StreamViewModel>();
            StreamsDataGrid.ItemsSource = _streams;

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
            
            // Show config file path in status bar
            var configService = new ConfigService();
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FoLive",
                "config.json"
            );
            StatusTextBlock.Text = $"Config: {configPath} | {_streams.Count} stream(s) loaded";
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

                StatusTextBlock.Text = $"Loaded {_streams.Count} stream(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading streams: {ex.Message}", "Error", 
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
                    var success = await _streamManager.AddStreamAsync(dialog.CreatedStream);
                    if (success)
                    {
                        // Reload streams from manager to ensure sync
                        await RefreshStreamsListAsync();
                        StatusTextBlock.Text = $"Stream '{dialog.CreatedStream.StreamId}' added successfully";
                    }
                    else
                    {
                        MessageBox.Show($"Stream with ID '{dialog.CreatedStream.StreamId}' already exists.", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding stream: {ex.Message}", "Error", 
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
                    MessageBox.Show($"Error refreshing streams: {ex.Message}", "Error", 
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

                StatusTextBlock.Text = $"Starting {streamsToStart.Count} stream(s)...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting streams: {ex.Message}", "Error", 
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

                StatusTextBlock.Text = $"Stopping {streamsToStop.Count} stream(s)...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping streams: {ex.Message}", "Error", 
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
                    StatusTextBlock.Text = $"Starting stream '{viewModel.StreamId}'...";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error starting stream: {ex.Message}", "Error", 
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
                    StatusTextBlock.Text = $"Stopping stream '{viewModel.StreamId}'...";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error stopping stream: {ex.Message}", "Error", 
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
            MessageBox.Show("FoLive - Stream Manager\nVersion 3.0.9", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Stop();
            base.OnClosed(e);
        }
    }
}

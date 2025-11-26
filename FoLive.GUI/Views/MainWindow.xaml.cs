using System;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FoLive.Core.Models;
using FoLive.Core.Services;

namespace FoLive.Views;

public partial class MainWindow : Window
{
    private readonly StreamManager _streamManager;
    private readonly FFmpegService _ffmpegService;
    private readonly ObservableCollection<Stream> _streams;
    private readonly DispatcherTimer _refreshTimer;

    public MainWindow()
    {
        InitializeComponent();
        
        _streamManager = new StreamManager();
        _ffmpegService = new FFmpegService();
        _streams = new ObservableCollection<Stream>();
        
        StreamsDataGrid.ItemsSource = _streams;
        
        _streamManager.StreamStatusChanged += OnStreamStatusChanged;
        
        // Setup refresh timer
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();
        
        // Setup time display timer
        var timeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timeTimer.Tick += (s, e) => TimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        timeTimer.Start();
        
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await CheckSystemStatus();
        await RefreshStreams();
    }

    private async Task CheckSystemStatus()
    {
        var ffmpegOk = _ffmpegService.CheckFFmpeg();
        FFmpegStatus.Text = ffmpegOk ? "FFmpeg: ✅ OK" : "FFmpeg: ❌ Not Found";
        
        var streams = await _streamManager.GetAllStreamsAsync();
        StreamsCount.Text = $"Streams: {streams.Count}";
    }

    private async Task RefreshStreams()
    {
        var streams = await _streamManager.GetAllStreamsAsync();
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            _streams.Clear();
            foreach (var stream in streams)
            {
                _streams.Add(stream);
            }
        });
    }

    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        _ = RefreshStreams();
        _ = CheckSystemStatus();
    }

    private void OnStreamStatusChanged(object? sender, StreamEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = _streams.FirstOrDefault(s => s.StreamId == e.Stream.StreamId);
            if (existing != null)
            {
                var index = _streams.IndexOf(existing);
                _streams[index] = e.Stream;
            }
        });
    }

    private async void AddStream_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddStreamDialog();
        if (dialog.ShowDialog() == true)
        {
            var stream = dialog.GetStream();
            if (stream != null)
            {
                await _streamManager.AddStreamAsync(stream);
                await RefreshStreams();
                StatusText.Text = $"Stream {stream.StreamId} added";
            }
        }
    }

    private async void StartStream_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            await _streamManager.StartStreamAsync(streamId);
            await RefreshStreams();
            StatusText.Text = $"Stream {streamId} started";
        }
    }

    private async void StopStream_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string streamId)
        {
            await _streamManager.StopStreamAsync(streamId);
            await RefreshStreams();
            StatusText.Text = $"Stream {streamId} stopped";
        }
    }

    private async void DeleteStream_Click(object sender, RoutedEventArgs e)
    {
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
                StatusText.Text = $"Stream {streamId} deleted";
            }
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Settings dialog - To be implemented", "Settings", MessageBoxButton.OK);
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer?.Stop();
        base.OnClosed(e);
    }
}


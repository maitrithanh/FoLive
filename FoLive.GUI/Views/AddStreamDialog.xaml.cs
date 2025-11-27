using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using FoLive.Core.Models;

namespace FoLive.Views
{
    public partial class AddStreamDialog : Window
    {
        public Stream? CreatedStream { get; private set; }
        private Stream? _existingStream;

        public AddStreamDialog(Stream? existingStream = null)
        {
            InitializeComponent();
            _existingStream = existingStream;
            
            // Nếu có existing stream, load data vào form
            if (existingStream != null)
            {
                Title = "Sửa Stream";
                TitleTextBlock.Text = "Sửa Stream";
                LoadStreamData(existingStream);
            }
            else
            {
                Title = "Thêm Stream Mới";
                TitleTextBlock.Text = "Thêm Stream Mới";
            }
            
            // Wire up slider value changed events
            SpeedSlider.ValueChanged += (s, e) => SpeedValueText.Text = $"{SpeedSlider.Value:F1}x";
            VolumeSlider.ValueChanged += (s, e) => VolumeValueText.Text = $"{(VolumeSlider.Value * 100):F0}%";
            BrightnessSlider.ValueChanged += (s, e) => BrightnessValueText.Text = BrightnessSlider.Value.ToString();
            
            // Wire up resolution combo box
            ResolutionComboBox.SelectionChanged += (s, e) =>
            {
                CustomResolutionPanel.Visibility = ResolutionComboBox.SelectedIndex == 4 
                    ? System.Windows.Visibility.Visible 
                    : System.Windows.Visibility.Collapsed;
            };
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4;*.avi;*.mkv;*.mov;*.flv)|*.mp4;*.avi;*.mkv;*.mov;*.flv|All files (*.*)|*.*"
            };
            
            if (dialog.ShowDialog() == true)
            {
                SourceTextBox.Text = dialog.FileName;
            }
        }

        private void BrowseIntro_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4;*.avi;*.mkv;*.mov;*.flv)|*.mp4;*.avi;*.mkv;*.mov;*.flv|All files (*.*)|*.*"
            };
            
            if (dialog.ShowDialog() == true)
            {
                IntroTextBox.Text = dialog.FileName;
            }
        }

        private void BrowseOutro_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4;*.avi;*.mkv;*.mov;*.flv)|*.mp4;*.avi;*.mkv;*.mov;*.flv|All files (*.*)|*.*"
            };
            
            if (dialog.ShowDialog() == true)
            {
                OutroTextBox.Text = dialog.FileName;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(StreamIdTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập Stream ID.", "Lỗi xác thực", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập nguồn (đường dẫn file hoặc URL).", "Lỗi xác thực", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(StreamUrlTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập Stream URL.", "Lỗi xác thực", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(StreamKeyTextBox.Text))
                {
                    MessageBox.Show("Vui lòng nhập Stream Key.", "Lỗi xác thực", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get source type
                var sourceTypeItems = new[] { "File", "YouTube", "Playlist", "Facebook", "URL (yt-dlp supported)", "Screen" };
                var selectedSourceType = SourceTypeComboBox.SelectedIndex >= 0 && SourceTypeComboBox.SelectedIndex < sourceTypeItems.Length
                    ? sourceTypeItems[SourceTypeComboBox.SelectedIndex]
                    : "File";

                // Normalize source type
                var normalizedSourceType = selectedSourceType.ToLower();
                if (normalizedSourceType.Contains("youtube"))
                    normalizedSourceType = "youtube";
                else if (normalizedSourceType.Contains("playlist"))
                    normalizedSourceType = "playlist";
                else if (normalizedSourceType.Contains("facebook"))
                    normalizedSourceType = "facebook";
                else if (normalizedSourceType.Contains("url"))
                    normalizedSourceType = "url";
                else if (normalizedSourceType.Contains("screen"))
                    normalizedSourceType = "screen";
                else
                    normalizedSourceType = "file";

                // Get resolution
                var resolutionItems = new[] { "1920x1080", "1280x720", "854x480", "640x360", "Custom" };
                string resolution = "1280x720";
                if (ResolutionComboBox.SelectedIndex == 4 && !string.IsNullOrWhiteSpace(CustomResolutionTextBox.Text))
                {
                    resolution = CustomResolutionTextBox.Text;
                }
                else if (ResolutionComboBox.SelectedIndex >= 0 && ResolutionComboBox.SelectedIndex < 4)
                {
                    resolution = resolutionItems[ResolutionComboBox.SelectedIndex];
                }

                // Parse resolution
                var resolutionParts = resolution.Split('x');
                var width = 1280;
                var height = 720;
                if (resolutionParts.Length == 2 && 
                    int.TryParse(resolutionParts[0], out var w) && 
                    int.TryParse(resolutionParts[1], out var h))
                {
                    width = w;
                    height = h;
                }

                // Create config dictionary
                var config = new Dictionary<string, object>
                {
                    ["loop"] = LoopCheckBox.IsChecked == true,
                    ["useRender"] = UseRenderCheckBox.IsChecked == true,
                    ["speed"] = SpeedSlider.Value,
                    ["volume"] = VolumeSlider.Value,
                    ["brightness"] = BrightnessSlider.Value,
                    ["width"] = width,
                    ["height"] = height,
                    ["bitrate"] = string.IsNullOrWhiteSpace(BitrateTextBox.Text) ? "2500k" : BitrateTextBox.Text
                };

                if (!string.IsNullOrWhiteSpace(TextOverlayTextBox.Text))
                {
                    config["textOverlay"] = TextOverlayTextBox.Text;
                }

                if (!string.IsNullOrWhiteSpace(IntroTextBox.Text))
                {
                    config["intro"] = IntroTextBox.Text;
                }

                if (!string.IsNullOrWhiteSpace(OutroTextBox.Text))
                {
                    config["outro"] = OutroTextBox.Text;
                }

                // Create stream object
                CreatedStream = new Stream
                {
                    StreamId = StreamIdTextBox.Text.Trim(),
                    Source = SourceTextBox.Text.Trim(),
                    SourceType = normalizedSourceType,
                    StreamUrl = StreamUrlTextBox.Text.Trim(),
                    StreamKey = StreamKeyTextBox.Text.Trim(),
                    Config = config,
                    Status = StreamStatus.Idle
                };

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var errorMsg = $"Lỗi khi tạo stream: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n\nChi tiết: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMsg, "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LoadStreamData(Stream stream)
        {
            StreamIdTextBox.Text = stream.StreamId;
            StreamIdTextBox.IsEnabled = false; // Không cho sửa ID khi edit
            
            // Set source type
            var sourceTypeItems = new[] { "File", "YouTube", "Playlist", "Facebook", "URL (yt-dlp supported)", "Screen" };
            var sourceTypeIndex = stream.SourceType.ToLower() switch
            {
                "youtube" => 1,
                "playlist" => 2,
                "facebook" => 3,
                "url" => 4,
                "screen" => 5,
                _ => 0
            };
            SourceTypeComboBox.SelectedIndex = sourceTypeIndex;
            
            SourceTextBox.Text = stream.Source;
            StreamUrlTextBox.Text = stream.StreamUrl;
            StreamKeyTextBox.Text = stream.StreamKey;
            
            // Load config
            if (stream.Config != null)
            {
                if (stream.Config.TryGetValue("loop", out var loop) && loop is bool loopValue)
                    LoopCheckBox.IsChecked = loopValue;
                
                if (stream.Config.TryGetValue("useRender", out var useRender) && useRender is bool renderValue)
                    UseRenderCheckBox.IsChecked = renderValue;
                
                if (stream.Config.TryGetValue("speed", out var speed) && speed is double speedValue)
                    SpeedSlider.Value = speedValue;
                
                if (stream.Config.TryGetValue("volume", out var volume) && volume is double volumeValue)
                    VolumeSlider.Value = volumeValue;
                
                if (stream.Config.TryGetValue("brightness", out var brightness) && brightness is double brightnessValue)
                    BrightnessSlider.Value = brightnessValue;
                
                if (stream.Config.TryGetValue("textOverlay", out var textOverlay) && textOverlay is string textOverlayValue)
                    TextOverlayTextBox.Text = textOverlayValue;
                
                if (stream.Config.TryGetValue("intro", out var intro) && intro is string introValue)
                    IntroTextBox.Text = introValue;
                
                if (stream.Config.TryGetValue("outro", out var outro) && outro is string outroValue)
                    OutroTextBox.Text = outroValue;
                
                if (stream.Config.TryGetValue("bitrate", out var bitrate) && bitrate is string bitrateValue)
                    BitrateTextBox.Text = bitrateValue;
                
                // Load resolution
                if (stream.Config.TryGetValue("width", out var width) && stream.Config.TryGetValue("height", out var height))
                {
                    var widthValue = width is int w ? w : 1280;
                    var heightValue = height is int h ? h : 720;
                    var resolution = $"{widthValue}x{heightValue}";
                    
                    var resolutionItems = new[] { "1920x1080", "1280x720", "854x480", "640x360", "Custom" };
                    var resolutionIndex = Array.IndexOf(resolutionItems, resolution);
                    if (resolutionIndex >= 0)
                    {
                        ResolutionComboBox.SelectedIndex = resolutionIndex;
                    }
                    else
                    {
                        ResolutionComboBox.SelectedIndex = 4; // Custom
                        CustomResolutionTextBox.Text = resolution;
                    }
                }
            }
        }
    }
}

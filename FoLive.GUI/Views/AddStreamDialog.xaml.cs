using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using FoLive.Core.Models;
using FoLive.Core.Services;
using StreamModel = FoLive.Core.Models.Stream;

namespace FoLive.Views;

public partial class AddStreamDialog : Window
{
    private readonly SubscriptionService? _subscriptionService;

    public AddStreamDialog(SubscriptionService? subscriptionService = null)
    {
        InitializeComponent();
        
        _subscriptionService = subscriptionService ?? App.SubscriptionService;
        
        // Setup slider value changed handlers
        SpeedSlider.ValueChanged += (s, e) => SpeedValueText.Text = $"{SpeedSlider.Value:F1}x";
        VolumeSlider.ValueChanged += (s, e) => VolumeValueText.Text = $"{(VolumeSlider.Value * 100):F0}%";
        BrightnessSlider.ValueChanged += (s, e) => BrightnessValueText.Text = $"{(int)BrightnessSlider.Value}";
        
        // Setup resolution combo box changed handler
        ResolutionComboBox.SelectionChanged += (s, e) =>
        {
            if (ResolutionComboBox.SelectedItem is ComboBoxItem item && item.Content is string content)
            {
                CustomResolutionPanel.Visibility = content == "Custom" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        };

        // Disable features based on subscription
        UpdateFeatureAvailability();
    }

    private void UpdateFeatureAvailability()
    {
        if (_subscriptionService == null) return;

        // Disable screen capture if not allowed
        if (!_subscriptionService.CanUseScreenCapture())
        {
            // Find and disable Screen option
            foreach (ComboBoxItem item in SourceTypeComboBox.Items)
            {
                if (item.Content is string content && content == "Screen")
                {
                    item.IsEnabled = false;
                    break;
                }
            }
        }

        // Disable multiple sources if not allowed
        if (!_subscriptionService.CanUseMultipleSources())
        {
            // Disable YouTube, Playlist, Facebook, URL options
            foreach (ComboBoxItem item in SourceTypeComboBox.Items)
            {
                if (item.Content is string content)
                {
                    var lowerContent = content.ToLower();
                    if (lowerContent.Contains("youtube") || 
                        lowerContent.Contains("playlist") || 
                        lowerContent.Contains("facebook") || 
                        lowerContent.Contains("url"))
                    {
                        item.IsEnabled = false;
                    }
                }
            }
        }

        // Disable advanced features if not allowed
        if (!_subscriptionService.CanUseAdvancedFeatures())
        {
            UseRenderCheckBox.IsEnabled = false;
            UseRenderCheckBox.ToolTip = "Tính năng này chỉ dành cho gói trả phí";
        }
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.mov;*.mkv;*.avi;*.flv;*.webm;*.m4v;*.wmv|All Files|*.*",
            Title = "Select Video File"
        };

        if (dialog.ShowDialog() == true)
        {
            SourceTextBox.Text = dialog.FileName;
            SourceTypeComboBox.SelectedIndex = 0; // File
        }
    }

    private void BrowseIntro_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Video Files|*.mp4;*.mov;*.mkv;*.avi;*.flv;*.webm;*.m4v;*.wmv|All Files|*.*",
            Title = "Select Intro Video"
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
            Filter = "Video Files|*.mp4;*.mov;*.mkv;*.avi;*.flv;*.webm;*.m4v;*.wmv|All Files|*.*",
            Title = "Select Outro Video"
        };

        if (dialog.ShowDialog() == true)
        {
            OutroTextBox.Text = dialog.FileName;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
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

        if (string.IsNullOrWhiteSpace(StreamIdTextBox.Text) ||
            string.IsNullOrWhiteSpace(SourceTextBox.Text) ||
            string.IsNullOrWhiteSpace(StreamKeyTextBox.Text))
        {
            MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc.", "Lỗi xác thực", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check if selected source type is allowed
        if (SourceTypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content is string sourceType)
        {
            var lowerType = sourceType.ToLower();
            
            // Check screen capture
            if (lowerType == "screen" && _subscriptionService != null && !_subscriptionService.CanUseScreenCapture())
            {
                MessageBox.Show(
                    _subscriptionService.GetFeatureRestrictionMessage("screen_capture"),
                    "Tính năng không khả dụng",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Check multiple sources
            if ((lowerType.Contains("youtube") || lowerType.Contains("playlist") || 
                 lowerType.Contains("facebook") || lowerType.Contains("url")) &&
                _subscriptionService != null && !_subscriptionService.CanUseMultipleSources())
            {
                MessageBox.Show(
                    _subscriptionService.GetFeatureRestrictionMessage("multiple_sources"),
                    "Tính năng không khả dụng",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
        }

        DialogResult = true;
        Close();
    }

    public StreamModel? GetStream()
    {
        string sourceType = "file";
        if (SourceTypeComboBox.SelectedItem is ComboBoxItem item && item.Content is string content)
        {
            sourceType = content.ToLower();
            // Map display names to internal types
            sourceType = sourceType switch
            {
                "url (yt-dlp supported)" => "url",
                _ => sourceType
            };
        }
        
        // Get resolution
        string resolution = "1280x720"; // Default
        if (ResolutionComboBox.SelectedItem is ComboBoxItem resItem && resItem.Content is string resContent)
        {
            if (resContent == "Custom")
            {
                resolution = CustomResolutionTextBox.Text.Trim();
            }
            else
            {
                // Extract resolution from text like "1920x1080 (Full HD)"
                var match = System.Text.RegularExpressions.Regex.Match(resContent, @"(\d+x\d+)");
                if (match.Success)
                {
                    resolution = match.Groups[1].Value;
                }
            }
        }

        var config = new Dictionary<string, object>
        {
            { "loop", LoopCheckBox.IsChecked == true },
            { "use_render", UseRenderCheckBox.IsChecked == true },
            { "speed", SpeedSlider.Value },
            { "volume", VolumeSlider.Value },
            { "brightness", (int)BrightnessSlider.Value },
            { "bitrate", BitrateTextBox.Text },
            { "resolution", resolution }
        };

        if (!string.IsNullOrWhiteSpace(TextOverlayTextBox.Text))
        {
            config["text"] = TextOverlayTextBox.Text;
        }

        if (!string.IsNullOrWhiteSpace(IntroTextBox.Text))
        {
            config["intro"] = IntroTextBox.Text;
        }

        if (!string.IsNullOrWhiteSpace(OutroTextBox.Text))
        {
            config["outro"] = OutroTextBox.Text;
        }
        
        return new StreamModel
        {
            StreamId = StreamIdTextBox.Text,
            Source = SourceTextBox.Text,
            SourceType = sourceType,
            StreamUrl = StreamUrlTextBox.Text,
            StreamKey = StreamKeyTextBox.Text,
            Config = config
        };
    }
}


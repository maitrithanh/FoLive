using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using FoLive.Core.Models;
using StreamModel = FoLive.Core.Models.Stream;

namespace FoLive.Views;

public partial class AddStreamDialog : Window
{
    public AddStreamDialog()
    {
        InitializeComponent();
        
        // Setup slider value changed handlers
        SpeedSlider.ValueChanged += (s, e) => SpeedValueText.Text = $"{SpeedSlider.Value:F1}x";
        VolumeSlider.ValueChanged += (s, e) => VolumeValueText.Text = $"{(VolumeSlider.Value * 100):F0}%";
        BrightnessSlider.ValueChanged += (s, e) => BrightnessValueText.Text = $"{(int)BrightnessSlider.Value}";
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
        if (string.IsNullOrWhiteSpace(StreamIdTextBox.Text) ||
            string.IsNullOrWhiteSpace(SourceTextBox.Text) ||
            string.IsNullOrWhiteSpace(StreamKeyTextBox.Text))
        {
            MessageBox.Show("Please fill in all required fields.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
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
        
        var config = new Dictionary<string, object>
        {
            { "loop", LoopCheckBox.IsChecked == true },
            { "use_render", UseRenderCheckBox.IsChecked == true },
            { "speed", SpeedSlider.Value },
            { "volume", VolumeSlider.Value },
            { "brightness", (int)BrightnessSlider.Value },
            { "bitrate", BitrateTextBox.Text }
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


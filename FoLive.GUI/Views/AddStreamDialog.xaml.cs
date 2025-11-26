using System.Windows;
using Microsoft.Win32;

namespace FoLive.Views
{
    public partial class AddStreamDialog : Window
    {
        public AddStreamDialog()
        {
            InitializeComponent();
            
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
            // TODO: Validate inputs and create stream
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

using System.Windows;
using FoLive.Views;

namespace FoLive.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddStream_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddStreamDialog();
            if (dialog.ShowDialog() == true)
            {
                // TODO: Implement add stream logic
            }
        }

        private void StartAll_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement start all streams logic
        }

        private void StopAll_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement stop all streams logic
        }

        private void StartStream_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement start stream logic
        }

        private void StopStream_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement stop stream logic
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FoLive - Stream Manager\nVersion 1.0", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

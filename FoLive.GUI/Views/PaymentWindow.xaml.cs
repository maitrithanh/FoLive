using System.Windows;

namespace FoLive.Views
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow()
        {
            InitializeComponent();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement payment logic
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

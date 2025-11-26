using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoLive.Views;

public partial class PaymentWindow : Window
{
    private string _selectedPlan = "yearly";
    private decimal _selectedPrice = 2150000;

    public PaymentWindow()
    {
        InitializeComponent();
        UpdateSelectedPlan("yearly", 2150000, "Gói Năm - 2.150.000₫");
    }

    private void MonthlyPlan_Click(object sender, MouseButtonEventArgs e)
    {
        UpdateSelectedPlan("monthly", 299000, "Gói Tháng - 299.000₫");
        UpdateBadges("monthly");
    }

    private void YearlyPlan_Click(object sender, MouseButtonEventArgs e)
    {
        UpdateSelectedPlan("yearly", 2150000, "Gói Năm - 2.150.000₫");
        UpdateBadges("yearly");
    }

    private void LifetimePlan_Click(object sender, MouseButtonEventArgs e)
    {
        UpdateSelectedPlan("lifetime", 5990000, "Gói Vĩnh viễn - 5.990.000₫");
        UpdateBadges("lifetime");
    }

    private void UpdateSelectedPlan(string plan, decimal price, string displayText)
    {
        _selectedPlan = plan;
        _selectedPrice = price;
        SelectedPlanText.Text = displayText;
        TotalAmountText.Text = $"{price:N0}₫";
    }

    private void UpdateBadges(string selectedPlan)
    {
        MonthlySelectedBadge.Visibility = selectedPlan == "monthly" ? Visibility.Visible : Visibility.Collapsed;
        YearlySelectedBadge.Visibility = selectedPlan == "yearly" ? Visibility.Visible : Visibility.Collapsed;
        LifetimeSelectedBadge.Visibility = selectedPlan == "lifetime" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PayNow_Click(object sender, RoutedEventArgs e)
    {
        // Validate form
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Vui lòng nhập họ và tên.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
        {
            MessageBox.Show("Vui lòng nhập email hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            EmailTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
        {
            MessageBox.Show("Vui lòng nhập số điện thoại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            PhoneTextBox.Focus();
            return;
        }

        // Get payment method
        var paymentMethod = "Thẻ tín dụng/Ghi nợ";
        if (PaymentMethodComboBox.SelectedItem is ComboBoxItem item && item.Content is string content)
        {
            paymentMethod = content.Replace("💳 ", "").Replace("🏦 ", "").Replace("📱 ", "").Replace("💵 ", "");
        }

        // Show confirmation
        var result = MessageBox.Show(
            $"Xác nhận thanh toán:\n\n" +
            $"Gói: {SelectedPlanText.Text}\n" +
            $"Họ tên: {NameTextBox.Text}\n" +
            $"Email: {EmailTextBox.Text}\n" +
            $"SĐT: {PhoneTextBox.Text}\n" +
            $"Phương thức: {paymentMethod}\n" +
            $"Tổng tiền: {TotalAmountText.Text}\n\n" +
            $"Bạn có muốn tiếp tục?",
            "Xác nhận thanh toán",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // TODO: Integrate with payment gateway
            MessageBox.Show(
                "Thanh toán đã được gửi thành công!\n\n" +
                "Chúng tôi sẽ liên hệ với bạn trong vòng 24 giờ để xác nhận thanh toán.\n" +
                "Cảm ơn bạn đã sử dụng dịch vụ của FoLive!",
                "Thanh toán thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ContactSupport_Click(object sender, MouseButtonEventArgs e)
    {
        MessageBox.Show(
            "Liên hệ hỗ trợ:\n\n" +
            "Email: dev@fotech.pro\n" +
            "Hotline: +84 032557029\n" +
            "Website: https://folive-web.vercel.app\n" +
            "Thời gian: 8:00 - 22:00 hàng ngày",
            "Liên hệ hỗ trợ",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}



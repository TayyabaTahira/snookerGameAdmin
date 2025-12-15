using System.Windows;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class EndGameBillingDialog : Window
    {
        public EndGameBillingDialog(EndGameBillingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void EndGame_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as EndGameBillingViewModel;
            if (viewModel == null) return;

            // Validate partial payment
            if (viewModel.PayStatus == SnookerGameManagementSystem.Models.PayStatus.PARTIAL)
            {
                if (viewModel.PartialPaymentAmount <= 0)
                {
                    MessageBox.Show(
                        "Please enter a valid partial payment amount greater than 0.",
                        "Invalid Partial Payment",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (viewModel.PartialPaymentAmount >= viewModel.TotalAmount)
                {
                    MessageBox.Show(
                        $"Partial payment amount (PKR {viewModel.PartialPaymentAmount:N2}) must be less than the total amount (PKR {viewModel.TotalAmount:N2}).\n\n" +
                        "Please either reduce the partial payment amount or select 'Paid Now' status.",
                        "Invalid Partial Payment",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
    }
}

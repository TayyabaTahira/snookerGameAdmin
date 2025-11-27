using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class PaymentEntryDialog : Window
    {
        private readonly PaymentEntryViewModel _viewModel;

        public PaymentEntryDialog(PaymentEntryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // Subscribe to payment processed event
            _viewModel.PaymentProcessed += OnPaymentProcessed;
        }

        private void OnPaymentProcessed(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                try
                {
                    DragMove();
                }
                catch
                {
                    // Ignore if window is already moving
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.PaymentProcessed -= OnPaymentProcessed;
            if (!DialogResult.HasValue)
            {
                DialogResult = false;
            }
            base.OnClosed(e);
        }
    }
}

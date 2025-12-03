using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class AllocationPreviewItem
    {
        public Guid ChargeId { get; set; }
        public string FrameDescription { get; set; } = string.Empty;
        public decimal ChargeAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public class PaymentEntryViewModel : ViewModelBase
    {
        private readonly Guid _customerId;
        private readonly LedgerService _ledgerService;
        private readonly CustomerService _customerService;
        private decimal _paymentAmount;
        private string? _selectedPaymentMethod;
        private string _customerName = string.Empty;
        private decimal _currentBalance;
        private RelayCommand? _processPaymentCommand;

        public event EventHandler? PaymentProcessed;

        public PaymentEntryViewModel(Guid customerId, LedgerService ledgerService, CustomerService customerService)
        {
            _customerId = customerId;
            _ledgerService = ledgerService;
            _customerService = customerService;

            PaymentMethods = new ObservableCollection<string>
            {
                "Cash", "Card", "Bank Transfer", "Other"
            };

            _selectedPaymentMethod = "Cash";

            // Load customer info
            LoadCustomerInfoAsync();
        }

        private async void LoadCustomerInfoAsync()
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(_customerId);
                if (customer != null)
                {
                    CustomerName = customer.FullName;
                }

                CurrentBalance = await _ledgerService.GetCustomerBalanceAsync(_customerId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading customer info: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public decimal CurrentBalance
        {
            get => _currentBalance;
            set => SetProperty(ref _currentBalance, value);
        }

        public ObservableCollection<string> PaymentMethods { get; set; }

        public ObservableCollection<AllocationPreviewItem> AllocationPreview { get; set; } = new();

        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                if (SetProperty(ref _paymentAmount, value))
                {
                    OnPropertyChanged(nameof(CanProcessPayment));
                    ProcessPaymentCommand?.RaiseCanExecuteChanged();
                    _ = UpdateAllocationPreviewAsync();
                }
            }
        }

        public string? SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set => SetProperty(ref _selectedPaymentMethod, value);
        }

        public bool CanProcessPayment => PaymentAmount > 0;

        public RelayCommand ProcessPaymentCommand
        {
            get
            {
                if (_processPaymentCommand == null)
                {
                    _processPaymentCommand = new RelayCommand(
                        async _ => await ProcessPaymentAsync(),
                        _ => CanProcessPayment);
                }
                return _processPaymentCommand;
            }
        }

        public bool DialogResult { get; set; }

        private async Task UpdateAllocationPreviewAsync()
        {
            try
            {
                // Get unpaid charges for customer (FIFO order)
                var unpaidCharges = await _ledgerService.GetUnpaidChargesAsync(_customerId);

                AllocationPreview.Clear();
                decimal remainingPayment = PaymentAmount;

                foreach (var chargeInfo in unpaidCharges)
                {
                    if (remainingPayment <= 0) break;

                    decimal toAllocate = Math.Min(remainingPayment, chargeInfo.RemainingAmount);

                    AllocationPreview.Add(new AllocationPreviewItem
                    {
                        ChargeId = chargeInfo.Charge.Id,
                        FrameDescription = chargeInfo.Charge.Description,
                        ChargeAmount = chargeInfo.Charge.AmountPk,
                        AllocatedAmount = toAllocate,
                        RemainingAmount = chargeInfo.RemainingAmount - toAllocate
                    });

                    remainingPayment -= toAllocate;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error updating allocation preview: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task ProcessPaymentAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Starting payment processing...");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Customer ID: {_customerId}");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Amount: {PaymentAmount}");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Method: {_selectedPaymentMethod}");
                
                var result = await _ledgerService.ProcessPaymentAsync(
                    _customerId,
                    PaymentAmount,
                    _selectedPaymentMethod ?? "Cash");

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Payment processed successfully!");
                    
                    MessageBox.Show(
                        $"Payment of PKR {PaymentAmount:N2} processed successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    PaymentProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Payment processing returned false");
                    
                    MessageBox.Show(
                        "Failed to process payment. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Payment processing exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Inner exception: {ex.InnerException.Message}");
                }
                
                MessageBox.Show(
                    $"Error processing payment:\n\n{ex.Message}\n\n{(ex.InnerException != null ? "Inner: " + ex.InnerException.Message : "")}",
                    "Payment Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

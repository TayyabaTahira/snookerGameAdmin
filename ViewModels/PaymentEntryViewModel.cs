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
        public bool IsInitialCredit { get; set; } = false;
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
        private decimal _initialCredit;
        private RelayCommand? _processPaymentCommand;
        private string _errorMessage = string.Empty;

        public event EventHandler? PaymentProcessed;

        public PaymentEntryViewModel(Guid customerId, LedgerService ledgerService, CustomerService customerService)
        {
            _customerId = customerId;
            _ledgerService = ledgerService;
            _customerService = customerService;

            PaymentMethods = new ObservableCollection<string>(LedgerPayment.AvailablePaymentMethods);

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
                    _initialCredit = customer.InitialCreditPk;
                }

                CurrentBalance = await _ledgerService.GetCustomerBalanceAsync(_customerId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading customer info: {ex.Message}";
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
                    ErrorMessage = string.Empty;
                    ValidatePaymentAmount();
                    OnPropertyChanged(nameof(CanProcessPayment));
                    ProcessPaymentCommand?.RaiseCanExecuteChanged();
                    _ = UpdateAllocationPreviewAsync();
                }
            }
        }

        public string? SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                if (SetProperty(ref _selectedPaymentMethod, value))
                {
                    ErrorMessage = string.Empty;
                    ValidatePaymentMethod();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool CanProcessPayment => PaymentAmount > 0 && !string.IsNullOrWhiteSpace(SelectedPaymentMethod) && string.IsNullOrEmpty(ErrorMessage);

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

        private void ValidatePaymentAmount()
        {
            if (PaymentAmount <= 0)
            {
                ErrorMessage = "Payment amount must be greater than zero";
            }
            else if (PaymentAmount > CurrentBalance)
            {
                ErrorMessage = $"Payment amount (PKR {PaymentAmount:N2}) exceeds current balance (PKR {CurrentBalance:N2})";
            }
        }

        private void ValidatePaymentMethod()
        {
            if (string.IsNullOrWhiteSpace(SelectedPaymentMethod))
            {
                ErrorMessage = "Please select a payment method";
            }
        }

        private async Task UpdateAllocationPreviewAsync()
        {
            try
            {
                AllocationPreview.Clear();
                decimal remainingPayment = PaymentAmount;

                // First, show allocation to initial credit if it exists
                if (_initialCredit > 0)
                {
                    // Calculate how much of initial credit has already been paid
                    var totalPayments = await _ledgerService.GetCustomerBalanceAsync(_customerId); // This will be recalculated
                    
                    // Get unpaid charges to calculate what went to them
                    var unpaidCharges = await _ledgerService.GetUnpaidChargesAsync(_customerId);
                    var totalUnpaidCharges = unpaidCharges.Sum(c => c.RemainingAmount);
                    
                    // Initial credit remaining = current balance - unpaid charges
                    var initialCreditRemaining = Math.Max(0, CurrentBalance - totalUnpaidCharges);
                    
                    if (initialCreditRemaining > 0 && remainingPayment > 0)
                    {
                        decimal toAllocate = Math.Min(remainingPayment, initialCreditRemaining);

                        AllocationPreview.Add(new AllocationPreviewItem
                        {
                            ChargeId = Guid.Empty,
                            FrameDescription = "Initial Outstanding Balance",
                            ChargeAmount = _initialCredit,
                            AllocatedAmount = toAllocate,
                            RemainingAmount = initialCreditRemaining - toAllocate,
                            IsInitialCredit = true
                        });

                        remainingPayment -= toAllocate;
                    }
                }

                // Then show allocation to unpaid charges (FIFO order)
                var charges = await _ledgerService.GetUnpaidChargesAsync(_customerId);

                foreach (var chargeInfo in charges)
                {
                    if (remainingPayment <= 0) break;

                    decimal toAllocate = Math.Min(remainingPayment, chargeInfo.RemainingAmount);

                    AllocationPreview.Add(new AllocationPreviewItem
                    {
                        ChargeId = chargeInfo.Charge.Id,
                        FrameDescription = chargeInfo.Charge.Description,
                        ChargeAmount = chargeInfo.Charge.AmountPk,
                        AllocatedAmount = toAllocate,
                        RemainingAmount = chargeInfo.RemainingAmount - toAllocate,
                        IsInitialCredit = false
                    });

                    remainingPayment -= toAllocate;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating allocation preview: {ex.Message}";
            }
        }

        private async Task ProcessPaymentAsync()
        {
            try
            {
                // Final validation
                if (PaymentAmount <= 0)
                {
                    ErrorMessage = "Payment amount must be greater than zero";
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedPaymentMethod))
                {
                    ErrorMessage = "Please select a payment method";
                    return;
                }

                if (PaymentAmount > CurrentBalance)
                {
                    var result = MessageBox.Show(
                        $"Payment amount (PKR {PaymentAmount:N2}) exceeds current balance (PKR {CurrentBalance:N2}).\n\nDo you want to continue?",
                        "Confirm Overpayment",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Starting payment processing...");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Customer ID: {_customerId}");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Amount: {PaymentAmount}");
                System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Method: {SelectedPaymentMethod}");
                
                var result2 = await _ledgerService.ProcessPaymentAsync(
                    _customerId,
                    PaymentAmount,
                    SelectedPaymentMethod ?? "Cash");

                if (result2)
                {
                    System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Payment processed successfully!");
                    
                    MessageBox.Show(
                        $"Payment of PKR {PaymentAmount:N2} via {SelectedPaymentMethod} processed successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    PaymentProcessed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PaymentEntryViewModel] Payment processing returned false");
                    ErrorMessage = "Failed to process payment. Please try again.";
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
                
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }
}

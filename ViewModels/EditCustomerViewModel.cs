using System.Windows;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class EditCustomerViewModel : ViewModelBase
    {
        private readonly Customer? _existingCustomer;
        private readonly CustomerService _customerService;
        private string _fullName = string.Empty;
        private string _phone = string.Empty;
        private decimal _initialCredit = 0;

        public EditCustomerViewModel(Customer? customer, CustomerService customerService)
        {
            _existingCustomer = customer;
            _customerService = customerService;

            if (_existingCustomer != null)
            {
                _fullName = _existingCustomer.FullName;
                _phone = _existingCustomer.Phone ?? string.Empty;
                _initialCredit = _existingCustomer.InitialCreditPk;
            }
        }

        public string DialogTitle => _existingCustomer == null ? "Add Customer" : "Edit Customer";
        public string SaveButtonText => _existingCustomer == null ? "Add" : "Save";
        public bool IsInitialCreditVisible => _existingCustomer == null; // Only show for new customers

        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public decimal InitialCredit
        {
            get => _initialCredit;
            set => SetProperty(ref _initialCredit, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(_fullName);

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_existingCustomer == null)
                {
                    // Create new customer with initial credit
                    var newCustomer = new Customer
                    {
                        FullName = _fullName,
                        Phone = _phone,
                        InitialCreditPk = _initialCredit
                    };
                    await _customerService.CreateCustomerAsync(newCustomer.FullName, newCustomer.Phone, newCustomer.InitialCreditPk);
                }
                else
                {
                    // Update existing customer (don't update initial credit)
                    _existingCustomer.FullName = _fullName;
                    _existingCustomer.Phone = _phone;
                    await _customerService.UpdateCustomerAsync(_existingCustomer);
                }

                MessageBox.Show(
                    "Customer saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving customer: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}

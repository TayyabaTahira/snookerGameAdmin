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

        public EditCustomerViewModel(Customer? customer, CustomerService customerService)
        {
            _existingCustomer = customer;
            _customerService = customerService;

            if (_existingCustomer != null)
            {
                _fullName = _existingCustomer.FullName;
                _phone = _existingCustomer.Phone ?? string.Empty;
            }
        }

        public string DialogTitle => _existingCustomer == null ? "Add Customer" : "Edit Customer";
        public string SaveButtonText => _existingCustomer == null ? "Add" : "Save";

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

        public bool CanSave => !string.IsNullOrWhiteSpace(_fullName);

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_existingCustomer == null)
                {
                    // Create new customer
                    await _customerService.CreateCustomerAsync(_fullName, _phone);
                }
                else
                {
                    // Update existing customer
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

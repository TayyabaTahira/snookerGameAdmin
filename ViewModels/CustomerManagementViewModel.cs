using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;
using SnookerGameManagementSystem.Views;

namespace SnookerGameManagementSystem.ViewModels
{
    public class CustomerManagementViewModel : ViewModelBase
    {
        private readonly CustomerService _customerService;
        private ObservableCollection<Customer> _customers = new();

        public CustomerManagementViewModel(CustomerService customerService)
        {
            _customerService = customerService;

            AddCustomerCommand = new RelayCommand(async _ => await AddCustomer());
            EditCustomerCommand = new RelayCommand(async param => await EditCustomer(param as Customer));
            DeleteCustomerCommand = new RelayCommand(async param => await DeleteCustomer(param as Customer));

            _ = LoadCustomers();
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        private async Task LoadCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                
                // Load balance for each customer
                foreach (var customer in customers)
                {
                    customer.Balance = await _customerService.GetCustomerBalanceAsync(customer.Id);
                }
                
                Customers = new ObservableCollection<Customer>(customers);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading customers: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task AddCustomer()
        {
            try
            {
                var dialogViewModel = new EditCustomerViewModel(null, _customerService);
                var dialog = new Views.EditCustomerDialog(dialogViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true)
                {
                    await LoadCustomers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding customer: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task EditCustomer(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var dialogViewModel = new EditCustomerViewModel(customer, _customerService);
                var dialog = new Views.EditCustomerDialog(dialogViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true)
                {
                    await LoadCustomers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error editing customer: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DeleteCustomer(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{customer.FullName}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _customerService.DeleteCustomerAsync(customer.Id);
                    await LoadCustomers();

                    MessageBox.Show(
                        "Customer deleted successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting customer: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

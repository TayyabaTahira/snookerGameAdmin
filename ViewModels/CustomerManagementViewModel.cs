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
        private ObservableCollection<Customer> _filteredCustomers = new();
        private string _searchText = string.Empty;

        public CustomerManagementViewModel(CustomerService customerService)
        {
            _customerService = customerService;

            AddCustomerCommand = new RelayCommand(async _ => await AddCustomer());
            EditCustomerCommand = new RelayCommand(async param => await EditCustomer(param as Customer));
            DeleteCustomerCommand = new RelayCommand(async param => await DeleteCustomer(param as Customer));
            MakePaymentCommand = new RelayCommand(async param => await MakePayment(param as Customer));
            ViewHistoryCommand = new RelayCommand(param => ViewHistory(param as Customer));

            _ = LoadCustomers();
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set => SetProperty(ref _filteredCustomers, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterCustomers();
                }
            }
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand MakePaymentCommand { get; }
        public ICommand ViewHistoryCommand { get; }

        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = Customers.Where(c =>
                    c.FullName.ToLower().Contains(searchLower) ||
                    (c.Phone != null && c.Phone.Contains(searchLower))
                ).ToList();
                FilteredCustomers = new ObservableCollection<Customer>(filtered);
            }
        }

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
                FilterCustomers();
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

        private async Task MakePayment(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var ledgerService = new LedgerService(App.GetDbContext());
                var viewModel = new PaymentEntryViewModel(customer.Id, ledgerService, _customerService);
                var dialog = new PaymentEntryDialog(viewModel)
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
                    $"Error processing payment: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ViewHistory(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var ledgerService = new LedgerService(App.GetDbContext());
                var viewModel = new CustomerHistoryViewModel(customer, ledgerService);
                var window = new CustomerHistoryWindow(viewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error viewing history: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

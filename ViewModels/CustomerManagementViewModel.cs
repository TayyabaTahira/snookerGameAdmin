using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;
using SnookerGameManagementSystem.Views;

namespace SnookerGameManagementSystem.ViewModels
{
    public class CustomerWithBalance
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal Balance { get; set; }
    }

    public class CustomerManagementViewModel : ViewModelBase
    {
        private readonly CustomerService _customerService;
        private ObservableCollection<CustomerWithBalance> _customers = new();

        public CustomerManagementViewModel(CustomerService customerService)
        {
            _customerService = customerService;
            
            AddCustomerCommand = new RelayCommand(async _ => await AddCustomer());
            ViewCustomerCommand = new RelayCommand(param => ViewCustomer(param as CustomerWithBalance));
            
            LoadCustomers();
        }

        public ObservableCollection<CustomerWithBalance> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand ViewCustomerCommand { get; }

        private async void LoadCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var customersWithBalance = new List<CustomerWithBalance>();

                foreach (var customer in customers)
                {
                    var balance = await _customerService.GetCustomerBalanceAsync(customer.Id);
                    customersWithBalance.Add(new CustomerWithBalance
                    {
                        Id = customer.Id,
                        FullName = customer.FullName,
                        Phone = customer.Phone,
                        Balance = balance
                    });
                }

                Customers = new ObservableCollection<CustomerWithBalance>(customersWithBalance);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddCustomer()
        {
            var dialogViewModel = new SelectCustomerViewModel(_customerService);
            var dialog = new SelectCustomerDialog(dialogViewModel)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                LoadCustomers();
            }
        }

        private void ViewCustomer(CustomerWithBalance? customer)
        {
            if (customer != null)
            {
                MessageBox.Show($"Customer details for {customer.FullName}\n\nBalance: PKR {customer.Balance:N2}\n\n(Full detail view TODO)", 
                    "Customer Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

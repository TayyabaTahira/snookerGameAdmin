using System.Collections.ObjectModel;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class SelectCustomerViewModel : ViewModelBase
    {
        private readonly CustomerService _customerService;
        private ObservableCollection<Customer> _allCustomers = new();
        private ObservableCollection<Customer> _filteredCustomers = new();
        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private string _newCustomerName = string.Empty;
        private string _newCustomerPhone = string.Empty;
        private RelayCommand? _createCustomerCommand;

        public SelectCustomerViewModel(CustomerService customerService)
        {
            _customerService = customerService;
            
            SearchCommand = new RelayCommand(_ => FilterCustomers());
            
            LoadCustomers();
        }

        public event EventHandler? CustomerCreatedAndSelected;

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set => SetProperty(ref _filteredCustomers, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                }
            }
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

        public string NewCustomerName
        {
            get => _newCustomerName;
            set
            {
                if (SetProperty(ref _newCustomerName, value))
                {
                    OnPropertyChanged(nameof(CanCreateCustomer));
                    CreateCustomerCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewCustomerPhone
        {
            get => _newCustomerPhone;
            set => SetProperty(ref _newCustomerPhone, value);
        }

        public bool HasSelection => SelectedCustomer != null;
        public bool CanCreateCustomer => !string.IsNullOrWhiteSpace(NewCustomerName);

        public ICommand SearchCommand { get; }
        
        public RelayCommand CreateCustomerCommand
        {
            get
            {
                if (_createCustomerCommand == null)
                {
                    _createCustomerCommand = new RelayCommand(
                        async _ => await CreateCustomer(),
                        _ => CanCreateCustomer);
                }
                return _createCustomerCommand;
            }
        }

        private async void LoadCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                _allCustomers = new ObservableCollection<Customer>(customers);
                FilteredCustomers = new ObservableCollection<Customer>(_allCustomers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
        }

        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(_allCustomers);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredCustomers = new ObservableCollection<Customer>(
                    _allCustomers.Where(c => 
                        c.FullName.ToLower().Contains(searchLower) ||
                        (c.Phone?.ToLower().Contains(searchLower) ?? false)));
            }
        }

        private async Task CreateCustomer()
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(
                    NewCustomerName.Trim(),
                    string.IsNullOrWhiteSpace(NewCustomerPhone) ? null : NewCustomerPhone.Trim());

                _allCustomers.Add(customer);
                FilterCustomers();
                SelectedCustomer = customer;

                // Clear inputs
                NewCustomerName = string.Empty;
                NewCustomerPhone = string.Empty;

                // Notify that customer was created and selected
                CustomerCreatedAndSelected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating customer: {ex.Message}");
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class CreateSessionViewModel : ViewModelBase
    {
        private readonly CustomerService _customerService;
        private string _tableName = string.Empty;
        private GameType? _selectedGameType;
        private ObservableCollection<GameType> _gameTypes = new();
        private ObservableCollection<Customer> _selectedCustomers = new();

        public CreateSessionViewModel(CustomerService customerService)
        {
            _customerService = customerService;
            
            AddPlayerCommand = new RelayCommand(async _ => await AddPlayer());
            RemovePlayerCommand = new RelayCommand(param => RemovePlayer(param as Customer));
        }

        public string TableName
        {
            get => _tableName;
            set
            {
                if (SetProperty(ref _tableName, value))
                {
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public GameType? SelectedGameType
        {
            get => _selectedGameType;
            set
            {
                if (SetProperty(ref _selectedGameType, value))
                {
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public ObservableCollection<GameType> GameTypes
        {
            get => _gameTypes;
            set => SetProperty(ref _gameTypes, value);
        }

        public ObservableCollection<Customer> SelectedCustomers
        {
            get => _selectedCustomers;
            set
            {
                if (SetProperty(ref _selectedCustomers, value))
                {
                    OnPropertyChanged(nameof(HasNoPlayers));
                }
            }
        }

        public bool CanCreate => !string.IsNullOrWhiteSpace(TableName) && SelectedGameType != null;

        public bool HasNoPlayers => SelectedCustomers.Count == 0;

        public ICommand AddPlayerCommand { get; }
        public ICommand RemovePlayerCommand { get; }

        private async Task AddPlayer()
        {
            try
            {
                // Show customer selection dialog
                var dialogViewModel = new SelectCustomerViewModel(_customerService);
                var dialog = new Views.SelectCustomerDialog(dialogViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true && dialogViewModel.SelectedCustomer != null)
                {
                    var customer = dialogViewModel.SelectedCustomer;

                    // Check if already added
                    if (SelectedCustomers.Any(c => c.Id == customer.Id))
                    {
                        MessageBox.Show(
                            $"{customer.FullName} is already added.",
                            "Already Added",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }

                    // Add to list
                    SelectedCustomers.Add(customer);
                    OnPropertyChanged(nameof(HasNoPlayers));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding player: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RemovePlayer(Customer? customer)
        {
            if (customer != null && SelectedCustomers.Contains(customer))
            {
                SelectedCustomers.Remove(customer);
                OnPropertyChanged(nameof(HasNoPlayers));
            }
        }
    }
}

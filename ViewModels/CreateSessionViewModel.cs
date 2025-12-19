using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class CreateSessionViewModel : ViewModelBase
    {
        private readonly CustomerService _customerService;
        private string _tableName = string.Empty;
        private bool _isTableNameReadOnly = false;
        private GameType? _selectedGameType;
        private ObservableCollection<GameType> _gameTypes = new();
        private ObservableCollection<Customer> _selectedCustomers = new();
        private string _errorMessage = string.Empty;

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
                    ValidatePlayerCount();
                }
            }
        }

        public bool IsTableNameReadOnly
        {
            get => _isTableNameReadOnly;
            set => SetProperty(ref _isTableNameReadOnly, value);
        }

        public GameType? SelectedGameType
        {
            get => _selectedGameType;
            set
            {
                if (SetProperty(ref _selectedGameType, value))
                {
                    OnPropertyChanged(nameof(CanCreate));
                    OnPropertyChanged(nameof(PlayerLimitInfo));
                    ValidatePlayerCount();
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
                    OnPropertyChanged(nameof(PlayerCountInfo));
                    ValidatePlayerCount();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string PlayerLimitInfo
        {
            get
            {
                if (SelectedGameType == null) return string.Empty;
                var min = SelectedGameType.MinPlayers ?? 2;
                var max = SelectedGameType.MaxPlayers ?? 4;
                return $"This game type requires {min} to {max} players";
            }
        }

        public string PlayerCountInfo
        {
            get
            {
                if (SelectedGameType == null) return $"{SelectedCustomers.Count} player(s) selected";
                var min = SelectedGameType.MinPlayers ?? 2;
                var max = SelectedGameType.MaxPlayers ?? 4;
                return $"{SelectedCustomers.Count} of {min}-{max} required players selected";
            }
        }

        public bool CanCreate => !string.IsNullOrWhiteSpace(TableName) && SelectedGameType != null && string.IsNullOrEmpty(ErrorMessage);

        public bool HasNoPlayers => SelectedCustomers.Count == 0;

        public ICommand AddPlayerCommand { get; }
        public ICommand RemovePlayerCommand { get; }

        private void ValidatePlayerCount()
        {
            ErrorMessage = string.Empty;

            if (SelectedGameType == null) return;

            var minPlayers = SelectedGameType.MinPlayers ?? 2;
            var maxPlayers = SelectedGameType.MaxPlayers ?? 4;

            // Only validate if player limits are set (backward compatibility)
            if (minPlayers > 0 && maxPlayers > 0)
            {
                if (SelectedCustomers.Count < minPlayers)
                {
                    ErrorMessage = $"At least {minPlayers} players required for {SelectedGameType.Name}";
                }
                else if (SelectedCustomers.Count > maxPlayers)
                {
                    ErrorMessage = $"Maximum {maxPlayers} players allowed for {SelectedGameType.Name}";
                }
            }
        }

        private async Task AddPlayer()
        {
            try
            {
                var maxPlayers = SelectedGameType?.MaxPlayers ?? 4;
                
                // Check max player limit first (only if set)
                if (SelectedGameType != null && maxPlayers > 0 && SelectedCustomers.Count >= maxPlayers)
                {
                    MessageBox.Show(
                        $"Cannot add more players. Maximum {maxPlayers} players allowed for {SelectedGameType.Name}.",
                        "Player Limit Reached",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

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

                    // Check if customer has active session (optional check)
                    try
                    {
                        var hasActiveSession = await CheckCustomerHasActiveSession(customer.Id);
                        if (hasActiveSession)
                        {
                            var result = MessageBox.Show(
                                $"{customer.FullName} is already playing in another active game.\n\nDo you want to add them anyway?",
                                "Player Already Active",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);
                            
                            if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // If check fails, continue anyway (backward compatibility)
                    }

                    // Add to list
                    SelectedCustomers.Add(customer);
                    OnPropertyChanged(nameof(HasNoPlayers));
                    OnPropertyChanged(nameof(PlayerCountInfo));
                    ValidatePlayerCount();
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

        private async Task<bool> CheckCustomerHasActiveSession(Guid customerId)
        {
            try
            {
                using (var context = App.GetDbContext())
                {
                    // Check if customer is in any active session's frames
                    var hasActiveSession = await context.FrameParticipants
                        .Where(fp => fp.CustomerId == customerId)
                        .Join(
                            context.Frames,
                            fp => fp.FrameId,
                            f => f.Id,
                            (fp, f) => f
                        )
                        .Join(
                            context.Sessions,
                            f => f.SessionId,
                            s => s.Id,
                            (f, s) => s
                        )
                        .AnyAsync(s => s.Status == SessionStatus.IN_PROGRESS);

                    return hasActiveSession;
                }
            }
            catch
            {
                return false;
            }
        }

        private void RemovePlayer(Customer? customer)
        {
            if (customer != null && SelectedCustomers.Contains(customer))
            {
                SelectedCustomers.Remove(customer);
                OnPropertyChanged(nameof(HasNoPlayers));
                OnPropertyChanged(nameof(PlayerCountInfo));
                ValidatePlayerCount();
            }
        }
    }
}

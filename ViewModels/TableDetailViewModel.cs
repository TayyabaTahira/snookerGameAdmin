using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class PlayerInfo
    {
        public string Name { get; set; } = string.Empty;
        public int WinStreak { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class TableDetailViewModel : ViewModelBase
    {
        private readonly Session _session;
        private readonly SessionService _sessionService;
        private readonly CustomerService _customerService;
        private ObservableCollection<PlayerInfo> _players = new();
        private bool _isClosed = false;

        public event EventHandler? SessionEnded;
        public event EventHandler? SessionDeleted;

        public TableDetailViewModel(Session session, SessionService sessionService, CustomerService customerService)
        {
            _session = session;
            _sessionService = sessionService;
            _customerService = customerService;
            
            // Load players
            LoadPlayers();
            
            // Commands
            AddPlayerCommand = new RelayCommand(async _ => await AddPlayer());
            RemovePlayerCommand = new RelayCommand(param => RemovePlayer(param as PlayerInfo));
            EndGameCommand = new RelayCommand(async _ => await EndGame(), _ => CanEndGame);
            NextFrameCommand = new RelayCommand(async _ => await NextFrame(), _ => CanNextFrame);
            QuitSessionCommand = new RelayCommand(async _ => await QuitSession());
            DeleteTableCommand = new RelayCommand(async _ => await DeleteTable());
        }

        public string TableName => _session.Name;
        public string GameTypeName => _session.GameType?.Name ?? "Not Set";
        public int FrameCount => _session.Frames.Count;
        public string StartedAt => _session.StartedAt.ToString("g");
        
        public TimeSpan ElapsedTime => DateTime.Now - _session.StartedAt;
        
        public string ElapsedTimeDisplay => 
            $"{(int)ElapsedTime.TotalHours:D2}:{ElapsedTime.Minutes:D2}:{ElapsedTime.Seconds:D2}";

        public ObservableCollection<PlayerInfo> Players
        {
            get => _players;
            set
            {
                if (SetProperty(ref _players, value))
                {
                    OnPropertyChanged(nameof(HasNoPlayers));
                    OnPropertyChanged(nameof(HasPlayers));
                    OnPropertyChanged(nameof(CanEndGame));
                    OnPropertyChanged(nameof(CanNextFrame));
                }
            }
        }

        public bool HasNoPlayers => Players.Count == 0;
        public bool HasPlayers => Players.Count > 0;
        public bool CanEndGame => Players.Count > 0 && FrameCount > 0;
        public bool CanNextFrame => Players.Count >= 2;

        public ICommand AddPlayerCommand { get; }
        public ICommand RemovePlayerCommand { get; }
        public ICommand EndGameCommand { get; }
        public ICommand NextFrameCommand { get; }
        public ICommand QuitSessionCommand { get; }
        public ICommand DeleteTableCommand { get; }

        private void LoadPlayers()
        {
            // Load from the last frame if exists
            var lastFrame = _session.Frames.LastOrDefault();
            if (lastFrame != null)
            {
                foreach (var participant in lastFrame.Participants)
                {
                    if (participant.Customer != null)
                    {
                        Players.Add(new PlayerInfo
                        {
                            Name = participant.Customer.FullName,
                            CustomerId = participant.Customer.Id,
                            WinStreak = 0 // TODO: Calculate actual win streak
                        });
                    }
                }
            }
        }

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
                    if (Players.Any(p => p.CustomerId == customer.Id))
                    {
                        MessageBox.Show(
                            $"{customer.FullName} is already added to this session.",
                            "Already Added",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }

                    // Add to list
                    Players.Add(new PlayerInfo
                    {
                        Name = customer.FullName,
                        CustomerId = customer.Id,
                        WinStreak = 0
                    });
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

        private void RemovePlayer(PlayerInfo? player)
        {
            if (player != null)
            {
                var result = MessageBox.Show(
                    $"Remove {player.Name} from this session?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Players.Remove(player);
                }
            }
        }

        private async Task EndGame()
        {
            try
            {
                // TODO: Show billing dialog
                MessageBox.Show(
                    "Billing dialog will be shown here.\n\n" +
                    "Features to implement:\n" +
                    "- Calculate base rate + overtime\n" +
                    "- Apply discounts\n" +
                    "- Select payer (Loser/Split/Custom)\n" +
                    "- Pay now or Credit\n" +
                    "- Create ledger charges",
                    "End Game - TODO",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error ending game: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task NextFrame()
        {
            try
            {
                if (Players.Count < 2)
                {
                    MessageBox.Show(
                        "At least 2 players are required to start a frame.",
                        "Not Enough Players",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // TODO: Show winner selection dialog
                // TODO: Create new frame
                MessageBox.Show(
                    "Winner selection dialog will be shown here.\n\n" +
                    "Features to implement:\n" +
                    "- Select winner from players\n" +
                    "- Create new frame\n" +
                    "- Update win streaks\n" +
                    "- Refresh frame count",
                    "Next Frame - TODO",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error starting next frame: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task QuitSession()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to quit this session?\n\n" +
                    $"Table: {TableName}\n" +
                    $"Frames played: {FrameCount}\n\n" +
                    $"This will end the session and return to the dashboard.",
                    "Confirm Quit Session",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // End the session
                    await _sessionService.EndSessionAsync(_session.Id);

                    _isClosed = true;
                    SessionEnded?.Invoke(this, EventArgs.Empty);

                    // Close the window
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var window = Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w.DataContext == this);
                        window?.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error quitting session: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DeleteTable()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to DELETE this table?\n\n" +
                    $"Table: {TableName}\n" +
                    $"Frames: {FrameCount}\n" +
                    $"Players: {Players.Count}\n\n" +
                    $"?? WARNING: This will permanently end the session.\n" +
                    $"All data will be marked as ended in the database.",
                    "Confirm Delete Table",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // End the session (we don't actually delete, just end it)
                    await _sessionService.EndSessionAsync(_session.Id);

                    _isClosed = true;
                    SessionDeleted?.Invoke(this, EventArgs.Empty);

                    MessageBox.Show(
                        "Table session has been ended successfully.",
                        "Table Deleted",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Close the window
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var window = Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w.DataContext == this);
                        window?.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting table: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

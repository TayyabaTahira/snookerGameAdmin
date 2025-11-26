using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public partial class PlayerInfo
    {
        public string Name { get; set; } = string.Empty;
        public int WinStreak { get; set; }
        public Guid CustomerId { get; set; }
        public string WinStreakDisplay => WinStreak > 0 ? $"Win Streak: {WinStreak}" : "No current streak";
    }

    public class TableDetailViewModel : ViewModelBase
    {
        private readonly Session _session;
        private readonly SessionService _sessionService;
        private readonly CustomerService _customerService;
        private readonly FrameService _frameService;
        private readonly GameRuleService _gameRuleService;
        private ObservableCollection<PlayerInfo> _players = new();
        private bool _isClosed = false;

        public event EventHandler? SessionEnded;
        public event EventHandler? SessionDeleted;

        public TableDetailViewModel(Session session, SessionService sessionService, CustomerService customerService)
        {
            _session = session;
            _sessionService = sessionService;
            _customerService = customerService;
            _frameService = new FrameService(App.GetDbContext());
            _gameRuleService = new GameRuleService(App.GetDbContext());
            
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
                            WinStreak = CalculateWinStreak(participant.Customer.Id)
                        });
                    }
                }
            }
        }

        private int CalculateWinStreak(Guid customerId)
        {
            int streak = 0;
            var frames = _session.Frames.OrderByDescending(f => f.StartedAt).ToList();
            
            foreach (var frame in frames)
            {
                if (frame.WinnerCustomerId == customerId)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }
            
            return streak;
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
                // Get base rate from game type rule
                decimal baseRate = 0;
                if (_session.GameTypeId != null)
                {
                    var rule = await _gameRuleService.GetRuleByGameTypeIdAsync(_session.GameTypeId.Value);
                    baseRate = rule?.BaseRate ?? 0;
                }

                // Show billing dialog
                var billingViewModel = new EndGameBillingViewModel(_session, baseRate);
                var billingDialog = new Views.EndGameBillingDialog(billingViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (billingDialog.ShowDialog() == true)
                {
                    // Get the last frame to update with billing info
                    var lastFrame = _session.Frames.LastOrDefault();
                    if (lastFrame != null)
                    {
                        lastFrame.OvertimeMinutes = billingViewModel.OvertimeMinutes;
                        lastFrame.OvertimeAmountPk = billingViewModel.OvertimeAmount;
                        lastFrame.LumpSumFinePk = billingViewModel.LumpSumFine;
                        lastFrame.DiscountPk = billingViewModel.Discount;
                        lastFrame.TotalAmountPk = billingViewModel.TotalAmount;
                        lastFrame.PayerMode = billingViewModel.PayerMode;
                        lastFrame.PayStatus = billingViewModel.PayStatus;
                        lastFrame.EndedAt = DateTime.Now;

                        // Create ledger charges based on payer mode
                        await CreateLedgerCharges(lastFrame, billingViewModel);
                    }

                    // End the session
                    await _sessionService.EndSessionAsync(_session.Id);

                    _isClosed = true;
                    SessionEnded?.Invoke(this, EventArgs.Empty);

                    MessageBox.Show(
                        $"Game ended successfully!\n\n" +
                        $"Total Amount: PKR {billingViewModel.TotalAmount:N2}\n" +
                        $"Payment Status: {billingViewModel.PayStatus}",
                        "Game Ended",
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
                    $"Error ending game: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task CreateLedgerCharges(Frame frame, EndGameBillingViewModel billing)
        {
            var context = App.GetDbContext();
            
            switch (billing.PayerMode)
            {
                case PayerMode.LOSER:
                    if (frame.LoserCustomerId != null)
                    {
                        var charge = new LedgerCharge
                        {
                            CustomerId = frame.LoserCustomerId.Value,
                            FrameId = frame.Id,
                            Amount = billing.TotalAmount,
                            Description = $"Game charge - {_session.Name}",
                            ChargedAt = DateTime.Now
                        };
                        context.LedgerCharges.Add(charge);
                    }
                    break;

                case PayerMode.SPLIT:
                    var splitAmount = billing.TotalAmount / Players.Count;
                    foreach (var player in Players)
                    {
                        var charge = new LedgerCharge
                        {
                            CustomerId = player.CustomerId,
                            FrameId = frame.Id,
                            Amount = splitAmount,
                            Description = $"Game charge (split) - {_session.Name}",
                            ChargedAt = DateTime.Now
                        };
                        context.LedgerCharges.Add(charge);
                    }
                    break;

                case PayerMode.EACH:
                    foreach (var player in Players)
                    {
                        var charge = new LedgerCharge
                        {
                            CustomerId = player.CustomerId,
                            FrameId = frame.Id,
                            Amount = billing.TotalAmount,
                            Description = $"Game charge (each) - {_session.Name}",
                            ChargedAt = DateTime.Now
                        };
                        context.LedgerCharges.Add(charge);
                    }
                    break;
            }

            await context.SaveChangesAsync();
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

                // Show winner selection dialog
                var winnerViewModel = new SelectWinnerViewModel(Players);
                var winnerDialog = new Views.SelectWinnerDialog(winnerViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (winnerDialog.ShowDialog() == true && winnerViewModel.SelectedWinner != null)
                {
                    var winner = winnerViewModel.SelectedWinner;

                    // Get base rate
                    decimal baseRate = 0;
                    if (_session.GameTypeId != null)
                    {
                        var rule = await _gameRuleService.GetRuleByGameTypeIdAsync(_session.GameTypeId.Value);
                        baseRate = rule?.BaseRate ?? 0;
                    }

                    // End the current frame if it exists
                    var currentFrame = _session.Frames.LastOrDefault();
                    if (currentFrame != null && currentFrame.EndedAt == null)
                    {
                        // Determine loser (first player who is not winner)
                        var loserId = Players.FirstOrDefault(p => p.CustomerId != winner.CustomerId)?.CustomerId;

                        await _frameService.EndFrameAsync(
                            currentFrame.Id,
                            winner.CustomerId,
                            loserId);
                    }

                    // Create new frame
                    var playerIds = Players.Select(p => p.CustomerId).ToList();
                    var newFrame = await _frameService.CreateFrameAsync(
                        _session.Id,
                        playerIds,
                        baseRate);

                    // Reload session to get updated frame count
                    _session.Frames.Add(newFrame);

                    // Update win streaks
                    foreach (var player in Players)
                    {
                        player.WinStreak = CalculateWinStreak(player.CustomerId);
                    }

                    OnPropertyChanged(nameof(FrameCount));
                    OnPropertyChanged(nameof(CanEndGame));

                    MessageBox.Show(
                        $"Frame {FrameCount} started!\n\nWinner: {winner.Name}",
                        "Next Frame",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
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

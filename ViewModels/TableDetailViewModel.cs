using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
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
        private readonly Guid _sessionId;
        private Session _session;
        private readonly SessionService _sessionService;
        private readonly CustomerService _customerService;
        private readonly FrameService _frameService;
        private readonly GameRuleService _gameRuleService;
        private ObservableCollection<PlayerInfo> _players = new();
        private bool _isClosed = false;
        private System.Windows.Threading.DispatcherTimer? _elapsedTimer;
        private int _frameCount;

        public event EventHandler? SessionEnded;
        public event EventHandler? SessionDeleted;

        public TableDetailViewModel(Session session, SessionService sessionService, CustomerService customerService)
        {
            _sessionId = session.Id;
            _session = session;
            _sessionService = sessionService;
            _customerService = customerService;
            _frameService = new FrameService(App.GetDbContext());
            _gameRuleService = new GameRuleService(App.GetDbContext());
            _frameCount = session.Frames.Count;
            
            // Load players
            LoadPlayers();
            
            // Setup timer to update elapsed time every second
            _elapsedTimer = new System.Windows.Threading.DispatcherTimer();
            _elapsedTimer.Interval = TimeSpan.FromSeconds(1);
            _elapsedTimer.Tick += (s, e) =>
            {
                OnPropertyChanged(nameof(ElapsedTime));
                OnPropertyChanged(nameof(ElapsedTimeDisplay));
            };
            _elapsedTimer.Start();
            
            // Commands
            AddPlayerCommand = new RelayCommand(async _ => await AddPlayer());
            RemovePlayerCommand = new RelayCommand(param => RemovePlayer(param as PlayerInfo));
            EndGameCommand = new RelayCommand(async _ => await EndGame(), _ => CanEndGame);
            NextFrameCommand = new RelayCommand(async _ => await NextFrame(), _ => CanNextFrame);
            QuitSessionCommand = new RelayCommand(async _ => await QuitSession());
        }

        public string TableName => _session.Name;
        public string GameTypeName => _session.GameType?.Name ?? "Not Set";
        public int FrameCount
        {
            get => _frameCount;
            private set => SetProperty(ref _frameCount, value);
        }
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
                    UpdatePlayerRelatedProperties();
                }
            }
        }

        private void UpdatePlayerRelatedProperties()
        {
            OnPropertyChanged(nameof(HasNoPlayers));
            OnPropertyChanged(nameof(HasPlayers));
            OnPropertyChanged(nameof(CanEndGame));
            OnPropertyChanged(nameof(CanNextFrame));
            
            System.Diagnostics.Debug.WriteLine($"[TableDetailViewModel] UpdatePlayerRelatedProperties - Players: {Players.Count}, Frames: {FrameCount}, CanEndGame: {CanEndGame}");
            
            // Force command re-evaluation
            ((RelayCommand)AddPlayerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)EndGameCommand).RaiseCanExecuteChanged();
            ((RelayCommand)NextFrameCommand).RaiseCanExecuteChanged();
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

        private async Task RefreshSessionData()
        {
            // Reload session with fresh data from database
            var refreshedSession = await _sessionService.GetSessionByIdAsync(_sessionId);
            if (refreshedSession != null)
            {
                _session = refreshedSession;
                FrameCount = _session.Frames.Count;
                OnPropertyChanged(nameof(TableName));
                OnPropertyChanged(nameof(GameTypeName));
                OnPropertyChanged(nameof(StartedAt));
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
                    
                    // If this is the first frame and we now have players, create a frame to persist them
                    if (FrameCount == 0 && Players.Count >= 1)
                    {
                        // Get base rate from game type rule
                        decimal baseRate = 0;
                        if (_session.GameTypeId != null)
                        {
                            var rule = await _gameRuleService.GetRuleByGameTypeIdAsync(_session.GameTypeId.Value);
                            baseRate = rule?.BaseRate ?? 0;
                        }

                        // Create the first frame with current players
                        var playerIds = Players.Select(p => p.CustomerId).ToList();
                        await _frameService.CreateFrameAsync(_sessionId, playerIds, baseRate);
                        
                        // Refresh session data to update frame count
                        await RefreshSessionData();
                    }
                    // If frame already exists, update participants for existing frame
                    else if (FrameCount > 0)
                    {
                        var currentFrame = _session.Frames.LastOrDefault();
                        if (currentFrame != null)
                        {
                            using (var context = App.GetDbContext())
                            {
                                // Check if participant already exists
                                var existingParticipant = await context.FrameParticipants
                                    .FirstOrDefaultAsync(fp => fp.FrameId == currentFrame.Id && fp.CustomerId == customer.Id);
                                
                                if (existingParticipant == null)
                                {
                                    // Add new participant to current frame
                                    var participant = new FrameParticipant
                                    {
                                        Id = Guid.NewGuid(),
                                        FrameId = currentFrame.Id,
                                        CustomerId = customer.Id,
                                        Team = null,
                                        IsWinner = false,
                                        SharePk = null
                                    };
                                    context.FrameParticipants.Add(participant);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    
                    // Update UI properties
                    UpdatePlayerRelatedProperties();
                    
                    MessageBox.Show(
                        $"{customer.FullName} has been added to the session!",
                        "Player Added",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
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

        private async void RemovePlayer(PlayerInfo? player)
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
                    // Remove from UI
                    Players.Remove(player);
                    
                    // Remove from database if frame exists
                    if (FrameCount > 0)
                    {
                        var currentFrame = _session.Frames.LastOrDefault();
                        if (currentFrame != null)
                        {
                            try
                            {
                                using (var context = App.GetDbContext())
                                {
                                    var participant = await context.FrameParticipants
                                        .FirstOrDefaultAsync(fp => fp.FrameId == currentFrame.Id && fp.CustomerId == player.CustomerId);
                                    
                                    if (participant != null)
                                    {
                                        context.FrameParticipants.Remove(participant);
                                        await context.SaveChangesAsync();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    $"Error removing player from database: {ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                    }
                    
                    UpdatePlayerRelatedProperties();
                }
            }
        }

        private async Task EndGame()
        {
            try
            {
                // Refresh session data to get latest frame information
                await RefreshSessionData();

                // Check if there's an unfinished frame - need to finish it first
                var currentFrame = _session.Frames.LastOrDefault();
                if (currentFrame != null && currentFrame.EndedAt == null)
                {
                    // Show winner selection dialog for the final frame
                    var winnerViewModel = new SelectWinnerViewModel(Players);
                    var winnerDialog = new Views.SelectWinnerDialog(winnerViewModel)
                    {
                        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    };

                    if (winnerDialog.ShowDialog() != true || winnerViewModel.SelectedWinner == null)
                    {
                        // User cancelled - don't end the game
                        return;
                    }

                    var winner = winnerViewModel.SelectedWinner;
                    var loserId = Players.FirstOrDefault(p => p.CustomerId != winner.CustomerId)?.CustomerId;

                    // End the frame using a fresh context
                    using (var context = App.GetDbContext())
                    {
                        var frameToUpdate = await context.Frames.FindAsync(currentFrame.Id);
                        if (frameToUpdate != null)
                        {
                            frameToUpdate.WinnerCustomerId = winner.CustomerId;
                            frameToUpdate.LoserCustomerId = loserId;
                            frameToUpdate.EndedAt = DateTime.Now;
                            await context.SaveChangesAsync();
                        }
                    }

                    // Refresh after ending frame
                    await RefreshSessionData();
                    currentFrame = _session.Frames.LastOrDefault();
                    
                    // Get base rate from game type rule
                    decimal baseRate = 0;
                    if (_session.GameTypeId != null)
                    {
                        var rule = await _gameRuleService.GetRuleByGameTypeIdAsync(_session.GameTypeId.Value);
                        baseRate = rule?.BaseRate ?? 0;
                    }

                    // Show billing dialog for the final frame if it doesn't have billing yet
                    if (currentFrame != null && currentFrame.TotalAmountPk == 0)
                    {
                        var billingViewModel = new EndGameBillingViewModel(_session, baseRate);
                        var billingDialog = new Views.EndGameBillingDialog(billingViewModel)
                        {
                            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
                            Title = "Final Frame Billing"
                        };

                        if (billingDialog.ShowDialog() == true)
                        {
                            // Update the last frame with billing info using a fresh context
                            using (var context = App.GetDbContext())
                            {
                                var frameToUpdate = await context.Frames.FindAsync(currentFrame.Id);
                                if (frameToUpdate != null)
                                {
                                    frameToUpdate.OvertimeMinutes = billingViewModel.OvertimeMinutes;
                                    frameToUpdate.OvertimeAmountPk = billingViewModel.OvertimeAmount;
                                    frameToUpdate.LumpSumFinePk = billingViewModel.LumpSumFine;
                                    frameToUpdate.DiscountPk = billingViewModel.Discount;
                                    frameToUpdate.TotalAmountPk = billingViewModel.TotalAmount;
                                    frameToUpdate.PayerMode = billingViewModel.PayerMode;
                                    frameToUpdate.PayStatus = billingViewModel.PayStatus;

                                    // Create ledger charges based on payer mode
                                    await CreateLedgerCharges(context, frameToUpdate, billingViewModel);
                                    
                                    await context.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            // User cancelled billing - don't end the game
                            return;
                        }
                    }
                }

                // End the session
                await _sessionService.EndSessionAsync(_sessionId);

                _isClosed = true;
                SessionEnded?.Invoke(this, EventArgs.Empty);

                // Calculate total amount for all frames
                decimal totalSessionAmount = _session.Frames.Sum(f => f.TotalAmountPk);
                
                MessageBox.Show(
                    $"Game ended successfully!\n\n" +
                    $"Total Frames: {_session.Frames.Count}\n" +
                    $"Total Amount: PKR {totalSessionAmount:N2}",
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
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error ending game: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task CreateLedgerCharges(Data.SnookerDbContext context, Frame frame, EndGameBillingViewModel billing)
        {
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

                    // Refresh session to get latest frame data
                    await RefreshSessionData();

                    // End the current frame if it exists and is not ended
                    var currentFrame = _session.Frames.LastOrDefault();
                    if (currentFrame != null && currentFrame.EndedAt == null)
                    {
                        // Determine loser (first player who is not winner)
                        var loserId = Players.FirstOrDefault(p => p.CustomerId != winner.CustomerId)?.CustomerId;

                        // End frame with winner/loser
                        using (var context = App.GetDbContext())
                        {
                            var frameToUpdate = await context.Frames.FindAsync(currentFrame.Id);
                            if (frameToUpdate != null)
                            {
                                frameToUpdate.WinnerCustomerId = winner.CustomerId;
                                frameToUpdate.LoserCustomerId = loserId;
                                frameToUpdate.EndedAt = DateTime.Now;
                                await context.SaveChangesAsync();
                            }
                        }

                        // Refresh to get updated frame
                        await RefreshSessionData();
                        currentFrame = _session.Frames.FirstOrDefault(f => f.Id == currentFrame.Id);

                        // Show billing dialog for the completed frame
                        if (currentFrame != null)
                        {
                            var billingViewModel = new EndGameBillingViewModel(_session, baseRate);
                            var billingDialog = new Views.EndGameBillingDialog(billingViewModel)
                            {
                                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
                                Title = $"Frame {FrameCount} Billing"
                            };

                            if (billingDialog.ShowDialog() == true)
                            {
                                // Update frame with billing info and create ledger charges
                                using (var context = App.GetDbContext())
                                {
                                    var frameToUpdate = await context.Frames.FindAsync(currentFrame.Id);
                                    if (frameToUpdate != null)
                                    {
                                        frameToUpdate.OvertimeMinutes = billingViewModel.OvertimeMinutes;
                                        frameToUpdate.OvertimeAmountPk = billingViewModel.OvertimeAmount;
                                        frameToUpdate.LumpSumFinePk = billingViewModel.LumpSumFine;
                                        frameToUpdate.DiscountPk = billingViewModel.Discount;
                                        frameToUpdate.TotalAmountPk = billingViewModel.TotalAmount;
                                        frameToUpdate.PayerMode = billingViewModel.PayerMode;
                                        frameToUpdate.PayStatus = billingViewModel.PayStatus;

                                        // Create ledger charges based on payer mode
                                        await CreateLedgerCharges(context, frameToUpdate, billingViewModel);
                                        
                                        await context.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                // User cancelled billing - revert frame end
                                using (var context = App.GetDbContext())
                                {
                                    var frameToRevert = await context.Frames.FindAsync(currentFrame.Id);
                                    if (frameToRevert != null)
                                    {
                                        frameToRevert.WinnerCustomerId = null;
                                        frameToRevert.LoserCustomerId = null;
                                        frameToRevert.EndedAt = null;
                                        await context.SaveChangesAsync();
                                    }
                                }
                                return; // Don't create next frame
                            }
                        }
                    }

                    // Create new frame
                    var playerIds = Players.Select(p => p.CustomerId).ToList();
                    var newFrame = await _frameService.CreateFrameAsync(
                        _sessionId,
                        playerIds,
                        baseRate);

                    // Refresh session data to get accurate frame count
                    await RefreshSessionData();

                    // Update win streaks
                    foreach (var player in Players)
                    {
                        player.WinStreak = CalculateWinStreak(player.CustomerId);
                    }

                    OnPropertyChanged(nameof(CanEndGame));
                    ((RelayCommand)EndGameCommand).RaiseCanExecuteChanged();

                    MessageBox.Show(
                        $"Frame {FrameCount} started!\n\nPrevious winner: {winner.Name}",
                        "Next Frame",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error starting next frame: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
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
                    await _sessionService.EndSessionAsync(_sessionId);

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
    }
}

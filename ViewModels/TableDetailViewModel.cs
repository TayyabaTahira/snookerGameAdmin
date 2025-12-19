using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public partial class PlayerInfo : ViewModelBase
    {
        private string _name = string.Empty;
        private int _winStreak;
        private Guid _customerId;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int WinStreak
        {
            get => _winStreak;
            set
            {
                if (SetProperty(ref _winStreak, value))
                {
                    OnPropertyChanged(nameof(WinStreakDisplay));
                }
            }
        }

        public Guid CustomerId
        {
            get => _customerId;
            set => SetProperty(ref _customerId, value);
        }

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
            // Order frames by most recent first
            var frames = _session.Frames.OrderByDescending(f => f.StartedAt).ToList();
            
            System.Diagnostics.Debug.WriteLine($"[CalculateWinStreak] Customer: {customerId}, Total frames: {frames.Count}");
            
            // Count consecutive wins from most recent frame backwards
            foreach (var frame in frames)
            {
                System.Diagnostics.Debug.WriteLine($"[CalculateWinStreak]   Frame started: {frame.StartedAt}, Ended: {frame.EndedAt}, Winner: {frame.WinnerCustomerId}");
                
                // Only count finished frames with a winner
                if (frame.EndedAt != null && frame.WinnerCustomerId == customerId)
                {
                    streak++;
                    System.Diagnostics.Debug.WriteLine($"[CalculateWinStreak]   WIN - Streak now: {streak}");
                }
                else if (frame.EndedAt != null)
                {
                    // Frame finished but this player didn't win, streak ends
                    System.Diagnostics.Debug.WriteLine($"[CalculateWinStreak]   LOSS - Streak ends at: {streak}");
                    break;
                }
                // Skip unfinished frames (current frame)
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CalculateWinStreak]   UNFINISHED - Skipping");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[CalculateWinStreak] Final streak for customer {customerId}: {streak}");
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
                
                // Update win streaks for existing players
                foreach (var player in Players)
                {
                    var newStreak = CalculateWinStreak(player.CustomerId);
                    player.WinStreak = newStreak;
                    System.Diagnostics.Debug.WriteLine($"[RefreshSessionData] Player {player.Name} win streak updated to: {newStreak}");
                }
                
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

                    // Check if customer has active session
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

                    // Check game type player limits if set
                    if (_session.GameTypeId != null)
                    {
                        using (var context = App.GetDbContext())
                        {
                            var gameType = await context.GameTypes.FindAsync(_session.GameTypeId.Value);
                            if (gameType != null && Players.Count >= gameType.MaxPlayers)
                            {
                                MessageBox.Show(
                                    $"Cannot add more players. Maximum {gameType.MaxPlayers} players allowed for {gameType.Name}.",
                                    "Player Limit Reached",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                                return;
                            }
                        }
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

        private async Task<bool> CheckCustomerHasActiveSession(Guid customerId)
        {
            try
            {
                using (var context = App.GetDbContext())
                {
                    // Check if customer is in any active session's frames (excluding current session)
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
                        .AnyAsync(s => s.Status == SessionStatus.IN_PROGRESS && s.Id != _sessionId);

                    return hasActiveSession;
                }
            }
            catch
            {
                return false;
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

                // ALWAYS require winner selection for the last frame if not set
                var currentFrame = _session.Frames.LastOrDefault();
                if (currentFrame == null || FrameCount == 0)
                {
                    MessageBox.Show(
                        "No frames have been played. Cannot end the game.",
                        "No Frames",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // If the last frame doesn't have a winner, ask for it
                if (currentFrame.WinnerCustomerId == null)
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

                    // End the last frame with winner/loser
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

                    // CRITICAL: Refresh session data AGAIN to reload with winner/loser customer data
                    await RefreshSessionData();
                }

                // Get base rate from game type rule
                decimal baseRate = 0;
                if (_session.GameTypeId != null)
                {
                    var rule = await _gameRuleService.GetRuleByGameTypeIdAsync(_session.GameTypeId.Value);
                    baseRate = rule?.BaseRate ?? 0;
                }

                // IMPORTANT: Refresh one more time right before billing to ensure all data is fresh
                await RefreshSessionData();

                // Debug: Log frame data to see what we have
                System.Diagnostics.Debug.WriteLine($"[EndGame] Session has {_session.Frames.Count} frames");
                foreach (var frame in _session.Frames.OrderBy(f => f.StartedAt))
                {
                    System.Diagnostics.Debug.WriteLine($"  Frame: WinnerId={frame.WinnerCustomerId}, LoserId={frame.LoserCustomerId}");
                    System.Diagnostics.Debug.WriteLine($"    WinnerCustomer null? {frame.WinnerCustomer == null}");
                    System.Diagnostics.Debug.WriteLine($"    LoserCustomer null? {frame.LoserCustomer == null}");
                    System.Diagnostics.Debug.WriteLine($"    Participants: {frame.Participants.Count}");
                    foreach (var p in frame.Participants)
                    {
                        System.Diagnostics.Debug.WriteLine($"      Participant ID: {p.CustomerId}, Customer null: {p.Customer == null}, Name: {p.Customer?.FullName ?? "NULL"}");
                    }
                }

                // Load fresh session data directly for billing to avoid EF tracking issues
                Session billingSession;
                using (var context = App.GetDbContext())
                {
                    billingSession = await context.Sessions
                        .Include(s => s.GameType)
                        .Include(s => s.Frames)
                            .ThenInclude(f => f.Participants)
                            .ThenInclude(p => p.Customer)
                        .AsNoTracking() // Use AsNoTracking to get fresh data
                        .FirstOrDefaultAsync(s => s.Id == _sessionId);
                    
                    if (billingSession == null)
                    {
                        MessageBox.Show("Error loading session data for billing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    // Manually load winner/loser names to avoid EF tracking conflicts
                    foreach (var frame in billingSession.Frames)
                    {
                        if (frame.WinnerCustomerId != null)
                        {
                            var winner = await context.Customers
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == frame.WinnerCustomerId.Value);
                            if (winner != null)
                            {
                                frame.WinnerCustomer = winner;
                            }
                        }
                        
                        if (frame.LoserCustomerId != null)
                        {
                            var loser = await context.Customers
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == frame.LoserCustomerId.Value);
                            if (loser != null)
                            {
                                frame.LoserCustomer = loser;
                            }
                        }
                    }
                }

                // Show comprehensive session billing dialog with fresh untracked session data
                var billingViewModel = new EndGameBillingViewModel(billingSession, baseRate);
                var billingDialog = new Views.EndGameBillingDialog(billingViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
                    Title = "Session Billing - End Game"
                };

                if (billingDialog.ShowDialog() != true)
                {
                    // User cancelled billing - don't end the game
                    return;
                }

                // Apply session-level billing info and create ledger charges PER FRAME
                // Use RAW SQL to bypass EF tracking issues completely
                
                List<Guid> frameIds;
                int frameCount;
                decimal perFrameAmount;
                List<(Guid chargeId, Guid customerId, decimal amount)> createdCharges = new();
                
                using (var context = App.GetDbContext())
                {
                    // Load only frame IDs and the data we need for charges
                    var sessionFrames = await context.Frames
                        .Where(f => f.SessionId == _sessionId)
                        .OrderBy(f => f.StartedAt)
                        .Select(f => new 
                        { 
                            f.Id, 
                            f.WinnerCustomerId, 
                            f.LoserCustomerId,
                            ParticipantIds = f.Participants.Select(p => p.CustomerId).Distinct().ToList()
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    if (!sessionFrames.Any())
                    {
                        MessageBox.Show("Error: No frames found for this session.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Calculate per-frame charges
                    frameCount = sessionFrames.Count;
                    decimal sessionTotal = billingViewModel.TotalAmount;
                    perFrameAmount = frameCount > 0 ? sessionTotal / frameCount : 0;
                    frameIds = sessionFrames.Select(f => f.Id).ToList();

                    System.Diagnostics.Debug.WriteLine($"[EndGame] Creating charges for {frameCount} frames");
                    System.Diagnostics.Debug.WriteLine($"[EndGame] Session total: {sessionTotal}, Per frame: {perFrameAmount}");

                    // Create ledger charges using RAW SQL to avoid EF tracking issues
                    var now = DateTime.Now;
                    foreach (var frame in sessionFrames)
                    {
                        System.Diagnostics.Debug.WriteLine($"[EndGame] Processing frame {frame.Id}");
                        System.Diagnostics.Debug.WriteLine($"[EndGame]   Winner: {frame.WinnerCustomerId}, Loser: {frame.LoserCustomerId}");
                        
                        switch (billingViewModel.PayerMode)
                        {
                            case PayerMode.LOSER:
                                if (frame.LoserCustomerId != null)
                                {
                                    var chargeId = Guid.NewGuid();
                                    await context.Database.ExecuteSqlRawAsync(
                                        "INSERT INTO ledger_charge (id, customer_id, frame_id, amount_pk, description, created_at) " +
                                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                                        chargeId, frame.LoserCustomerId.Value, frame.Id, perFrameAmount,
                                        $"Frame charge - {_session.Name}", now);
                                    System.Diagnostics.Debug.WriteLine($"[EndGame]   Created LOSER charge: {perFrameAmount} for customer {frame.LoserCustomerId.Value}");
                                    createdCharges.Add((chargeId, frame.LoserCustomerId.Value, perFrameAmount));
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[EndGame]   WARNING: Frame has no loser, skipping charge");
                                }
                                break;

                            case PayerMode.WINNER:
                                if (frame.WinnerCustomerId != null)
                                {
                                    var chargeId = Guid.NewGuid();
                                    await context.Database.ExecuteSqlRawAsync(
                                        "INSERT INTO ledger_charge (id, customer_id, frame_id, amount_pk, description, created_at) " +
                                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                                        chargeId, frame.WinnerCustomerId.Value, frame.Id, perFrameAmount,
                                        $"Frame charge - {_session.Name}", now);
                                    System.Diagnostics.Debug.WriteLine($"[EndGame]   Created WINNER charge: {perFrameAmount} for customer {frame.WinnerCustomerId.Value}");
                                    createdCharges.Add((chargeId, frame.WinnerCustomerId.Value, perFrameAmount));
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[EndGame]   WARNING: Frame has no winner, skipping charge");
                                }
                                break;

                            case PayerMode.SPLIT:
                                var playersInFrame = frame.ParticipantIds;
                                var splitPerPlayer = playersInFrame.Count > 0 ? perFrameAmount / playersInFrame.Count : 0;
                                
                                System.Diagnostics.Debug.WriteLine($"[EndGame]   SPLIT mode: {playersInFrame.Count} players, {splitPerPlayer} each");
                                
                                foreach (var playerId in playersInFrame)
                                {
                                    var chargeId = Guid.NewGuid();
                                    await context.Database.ExecuteSqlRawAsync(
                                        "INSERT INTO ledger_charge (id, customer_id, frame_id, amount_pk, description, created_at) " +
                                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                                        chargeId, playerId, frame.Id, splitPerPlayer,
                                        $"Frame charge (split) - {_session.Name}", now);
                                    System.Diagnostics.Debug.WriteLine($"[EndGame]     Created SPLIT charge: {splitPerPlayer} for customer {playerId}");
                                    createdCharges.Add((chargeId, playerId, splitPerPlayer));
                                }
                                break;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[EndGame] All charges created successfully via SQL");
                }

                // Step 2: If payment status is PAID, create payment records and allocations
                if (billingViewModel.PayStatus == PayStatus.PAID)
                {
                    System.Diagnostics.Debug.WriteLine($"[EndGame] PayStatus is PAID - Creating payment records and allocations");
                    System.Diagnostics.Debug.WriteLine($"[EndGame] Payment Method: {billingViewModel.SelectedPaymentMethod}");
                    
                    using (var paymentContext = App.GetDbContext())
                    {
                        var now = DateTime.Now;
                        var paymentMethod = billingViewModel.SelectedPaymentMethod ?? "Cash";
                        
                        // Group charges by customer
                        var chargesByCustomer = createdCharges
                            .GroupBy(c => c.customerId)
                            .ToList();
                        
                        foreach (var customerGroup in chargesByCustomer)
                        {
                            var customerId = customerGroup.Key;
                            var customerCharges = customerGroup.ToList();
                            var totalCustomerAmount = customerCharges.Sum(c => c.amount);
                            
                            System.Diagnostics.Debug.WriteLine($"[EndGame]   Creating payment for customer {customerId}: {totalCustomerAmount} via {paymentMethod}");
                            
                            // Create payment record with selected method
                            var paymentId = Guid.NewGuid();
                            await paymentContext.Database.ExecuteSqlRawAsync(
                                "INSERT INTO ledger_payment (id, customer_id, amount_pk, method, received_at) " +
                                "VALUES ({0}, {1}, {2}, {3}, {4})",
                                paymentId, customerId, totalCustomerAmount, paymentMethod, now);
                            
                            // Create payment allocations for each charge
                            foreach (var charge in customerCharges)
                            {
                                var allocationId = Guid.NewGuid();
                                await paymentContext.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO payment_allocation (id, payment_id, charge_id, allocated_amount_pk, created_at) " +
                                    "VALUES ({0}, {1}, {2}, {3}, {4})",
                                    allocationId, paymentId, charge.chargeId, charge.amount, now);
                                
                                System.Diagnostics.Debug.WriteLine($"[EndGame]     Created allocation: {charge.amount} for charge {charge.chargeId}");
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[EndGame] All payment records and allocations created");
                    }
                }
                else if (billingViewModel.PayStatus == PayStatus.PARTIAL && billingViewModel.PartialPaymentAmount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[EndGame] PayStatus is PARTIAL - Creating partial payment records and allocations");
                    System.Diagnostics.Debug.WriteLine($"[EndGame] Partial payment amount: {billingViewModel.PartialPaymentAmount}");
                    System.Diagnostics.Debug.WriteLine($"[EndGame] Payment Method: {billingViewModel.SelectedPaymentMethod}");
                    
                    using (var paymentContext = App.GetDbContext())
                    {
                        var now = DateTime.Now;
                        var paymentMethod = billingViewModel.SelectedPaymentMethod ?? "Cash";
                        
                        // Group charges by customer
                        var chargesByCustomer = createdCharges
                            .GroupBy(c => c.customerId)
                            .ToList();
                        
                        // Calculate total charges across all customers
                        decimal totalAllCharges = createdCharges.Sum(c => c.amount);
                        
                        // Distribute the partial payment proportionally across customers
                        foreach (var customerGroup in chargesByCustomer)
                        {
                            var customerId = customerGroup.Key;
                            var customerCharges = customerGroup.ToList();
                            var totalCustomerCharges = customerCharges.Sum(c => c.amount);
                            
                            // Calculate proportional payment for this customer
                            var customerPaymentAmount = totalAllCharges > 0 
                                ? (totalCustomerCharges / totalAllCharges) * billingViewModel.PartialPaymentAmount 
                                : 0;
                            

                            System.Diagnostics.Debug.WriteLine($"[EndGame]   Creating partial payment for customer {customerId}: {customerPaymentAmount} (of total {totalCustomerCharges}) via {paymentMethod}");
                            System.Diagnostics.Debug.WriteLine($"[EndGame]   MATH CHECK: Charge={totalCustomerCharges:F2}, Payment={customerPaymentAmount:F2}, Remaining={totalCustomerCharges - customerPaymentAmount:F2}");
                            
                            // Create payment record for this customer's portion with selected method
                            var paymentId = Guid.NewGuid();
                            await paymentContext.Database.ExecuteSqlRawAsync(
                                "INSERT INTO ledger_payment (id, customer_id, amount_pk, method, received_at) " +
                                "VALUES ({0}, {1}, {2}, {3}, {4})",
                                paymentId, customerId, customerPaymentAmount, paymentMethod, now);
                            
                            // Allocate payment to charges (FIFO)
                            decimal remainingPayment = customerPaymentAmount;
                            foreach (var charge in customerCharges.OrderBy(c => c.chargeId))
                            {
                                if (remainingPayment <= 0) break;
                                
                                var toAllocate = Math.Min(remainingPayment, charge.amount);
                                
                                var allocationId = Guid.NewGuid();
                                await paymentContext.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO payment_allocation (id, payment_id, charge_id, allocated_amount_pk, created_at) " +
                                    "VALUES ({0}, {1}, {2}, {3}, {4})",
                                    allocationId, paymentId, charge.chargeId, toAllocate, now);
                                
                                System.Diagnostics.Debug.WriteLine($"[EndGame]     Created partial allocation: {toAllocate} for charge {charge.chargeId}");
                                
                                remainingPayment -= toAllocate;
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[EndGame] All partial payment records and allocations created");
                    }
                }

                // Step 3: Update frame properties using RAW SQL as well
                using (var frameContext = App.GetDbContext())
                {
                    // Update each frame's payment info using RAW SQL
                    foreach (var frameId in frameIds)
                    {
                        await frameContext.Database.ExecuteSqlRawAsync(
                            "UPDATE frame SET total_amount_pk = {0}, payer_mode = {1}, pay_status = {2} WHERE id = {3}",
                            perFrameAmount, billingViewModel.PayerMode.ToString(), billingViewModel.PayStatus.ToString(), frameId);
                    }

                    // Store session-level billing info on the last frame using RAW SQL
                    var lastFrameId = frameIds.Last();
                    await frameContext.Database.ExecuteSqlRawAsync(
                        "UPDATE frame SET overtime_minutes = {0}, overtime_amount_pk = {1}, lump_sum_fine_pk = {2}, discount_pk = {3} WHERE id = {4}",
                        billingViewModel.OvertimeMinutes, billingViewModel.OvertimeAmount, 
                        billingViewModel.LumpSumFine, billingViewModel.Discount, lastFrameId);

                    System.Diagnostics.Debug.WriteLine($"[EndGame] Frame updates completed via SQL");
                }

                // End the session
                await _sessionService.EndSessionAsync(_sessionId);

                _isClosed = true;
                SessionEnded?.Invoke(this, EventArgs.Empty);

                MessageBox.Show(
                    $"Session ended successfully!\n\n" +
                    $"Total Frames: {_session.Frames.Count}\n" +
                    $"Total Amount: PKR {billingViewModel.TotalAmount:N2}\n\n" +
                    $"Payment: {billingViewModel.PayStatus}",
                    "Session Ended",
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

                        // End frame with winner/loser (NO BILLING - that happens at session end)
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
                    }

                    // Create new frame with current players
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

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class GameHistoryItem
    {
        public DateTime Timestamp { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string GameType { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public int WinStreak { get; set; }
        public string Duration { get; set; } = string.Empty;
        public decimal TotalCharged { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IsInitialCredit { get; set; } = false;
    }

    public class CustomerHistoryViewModel : ViewModelBase
    {
        private readonly Customer _customer;
        private readonly LedgerService _ledgerService;
        private ObservableCollection<GameHistoryItem> _gameHistory = new();
        private int _totalGames;
        private int _totalWins;
        private int _totalLosses;
        private decimal _totalCharged;
        private decimal _totalPaid;
        private decimal _balanceDue;
        private decimal _initialCredit;

        public CustomerHistoryViewModel(Customer customer, LedgerService ledgerService)
        {
            _customer = customer;
            _ledgerService = ledgerService;
            _initialCredit = customer.InitialCreditPk;
            
            LoadHistory();
        }

        public string CustomerName => _customer.FullName;

        public ObservableCollection<GameHistoryItem> GameHistory
        {
            get => _gameHistory;
            set => SetProperty(ref _gameHistory, value);
        }

        public int TotalGames
        {
            get => _totalGames;
            set => SetProperty(ref _totalGames, value);
        }

        public int TotalWins
        {
            get => _totalWins;
            set => SetProperty(ref _totalWins, value);
        }

        public int TotalLosses
        {
            get => _totalLosses;
            set => SetProperty(ref _totalLosses, value);
        }

        public decimal TotalCharged
        {
            get => _totalCharged;
            set => SetProperty(ref _totalCharged, value);
        }

        public decimal TotalPaid
        {
            get => _totalPaid;
            set => SetProperty(ref _totalPaid, value);
        }

        public decimal BalanceDue
        {
            get => _balanceDue;
            set => SetProperty(ref _balanceDue, value);
        }

        public decimal InitialCredit
        {
            get => _initialCredit;
            set => SetProperty(ref _initialCredit, value);
        }

        public double WinRate => TotalGames > 0 ? (TotalWins * 100.0 / TotalGames) : 0;
        public string WinRateDisplay => $"{WinRate:F1}%";

        // Public method to refresh history data
        public async Task RefreshHistoryAsync()
        {
            await LoadHistoryAsync();
        }

        private async void LoadHistory()
        {
            await LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                using var context = App.GetDbContext();
                
                // Get all frames this customer participated in
                var participantFrames = await context.FrameParticipants
                    .Where(fp => fp.CustomerId == _customer.Id)
                    .Select(fp => fp.FrameId)
                    .ToListAsync();

                var frames = await context.Frames
                    .Where(f => participantFrames.Contains(f.Id) && f.EndedAt != null)
                    .OrderByDescending(f => f.EndedAt)
                    .ToListAsync();

                var historyItems = new List<GameHistoryItem>();
                int totalWins = 0;
                int totalLosses = 0;

                foreach (var frame in frames)
                {
                    // Get session info
                    var session = await context.Sessions.FirstOrDefaultAsync(s => s.Id == frame.SessionId);
                    var gameType = session?.GameTypeId != null 
                        ? await context.Set<GameType>().FirstOrDefaultAsync(gt => gt.Id == session.GameTypeId)
                        : null;

                    // Calculate frame duration
                    var frameEndTime = frame.EndedAt ?? DateTime.Now;
                    var frameDuration = frameEndTime - frame.StartedAt;
                    var durationStr = $"{(int)frameDuration.TotalHours:D2}:{frameDuration.Minutes:D2}:{frameDuration.Seconds:D2}";

                    // Determine if win or loss
                    bool isWin = frame.WinnerCustomerId == _customer.Id;
                    if (isWin) totalWins++;
                    else totalLosses++;

                    // Get charges for this customer on this frame
                    var charges = await context.LedgerCharges
                        .Where(c => c.CustomerId == _customer.Id && c.FrameId == frame.Id)
                        .ToListAsync();

                    var chargeAmount = charges.Sum(c => c.AmountPk);

                    // Get payments allocated to these charges WITH payment method
                    var chargeIds = charges.Select(c => c.Id).ToList();

                    // Get payment allocations with payment details
                    var paymentAllocationsWithMethod = await context.PaymentAllocations
                        .Where(pa => chargeIds.Contains(pa.ChargeId))
                        .Include(pa => pa.Payment) // Eagerly load payment to get method
                        .ToListAsync();

                    var paidAmount = paymentAllocationsWithMethod.Sum(pa => pa.AllocatedAmountPk);

                    // Get unique payment methods used for this frame's charges
                    var paymentMethods = paymentAllocationsWithMethod
                        .Where(pa => pa.Payment != null)
                        .Select(pa => pa.Payment!.Method ?? "Unknown")
                        .Distinct()
                        .ToList();

                    var paymentMethodStr = paymentMethods.Any() ? string.Join(", ", paymentMethods) : "-";
                    
                    // Calculate win streak at this point in time
                    var sessionFrames = await context.Frames
                        .Where(f => f.SessionId == frame.SessionId && f.StartedAt <= frame.StartedAt)
                        .OrderByDescending(f => f.StartedAt)
                        .ToListAsync();
                    
                    int winStreak = 0;
                    foreach (var f in sessionFrames)
                    {
                        if (f.WinnerCustomerId == _customer.Id)
                        {
                            winStreak++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    historyItems.Add(new GameHistoryItem
                    {
                        Timestamp = frame.EndedAt ?? frame.StartedAt,
                        TableName = session?.Name ?? "Unknown",
                        GameType = gameType?.Name ?? "Unknown",
                        Result = isWin ? "Win" : "Lose",
                        WinStreak = winStreak,
                        Duration = durationStr,
                        TotalCharged = chargeAmount,
                        AmountDue = chargeAmount - paidAmount,
                        AmountPaid = paidAmount,
                        PaymentStatus = chargeAmount == 0 ? "No Charge" : 
                                       paidAmount >= chargeAmount ? "Paid" : 
                                       paidAmount > 0 ? "Partial" : "Unpaid",
                        PaymentMethod = paymentMethodStr,
                        IsInitialCredit = false
                    });
                }

                // Calculate how much of initial credit has been paid
                decimal initialCreditPaid = 0;
                decimal initialCreditDue = _initialCredit;
                
                if (_initialCredit > 0)
                {
                    // Get all payments for this customer
                    var totalPayments = await context.LedgerPayments
                        .Where(p => p.CustomerId == _customer.Id)
                        .SumAsync(p => p.AmountPk);

                    // Get all payment allocations to charges
                    var totalAllocatedToCharges = await context.PaymentAllocations
                        .Where(pa => pa.Charge.CustomerId == _customer.Id)
                        .SumAsync(pa => pa.AllocatedAmountPk);

                    // The difference went to initial credit
                    initialCreditPaid = Math.Min(_initialCredit, Math.Max(0, totalPayments - totalAllocatedToCharges));
                    initialCreditDue = _initialCredit - initialCreditPaid;

                    // Get payment methods that were applied to initial credit
                    var allPaymentMethods = await context.LedgerPayments
                        .Where(p => p.CustomerId == _customer.Id)
                        .Select(p => p.Method ?? "Unknown")
                        .Distinct()
                        .ToListAsync();
                    
                    var initialCreditPaymentMethod = initialCreditPaid > 0 
                        ? string.Join(", ", allPaymentMethods)
                        : "-";

                    historyItems.Add(new GameHistoryItem
                    {
                        Timestamp = _customer.CreatedAt,
                        TableName = "-",
                        GameType = "Initial Balance",
                        Result = "-",
                        WinStreak = 0,
                        Duration = "-",
                        TotalCharged = _initialCredit,
                        AmountDue = initialCreditDue,
                        AmountPaid = initialCreditPaid,
                        PaymentStatus = initialCreditDue <= 0 ? "Paid" :
                                       initialCreditPaid > 0 ? "Partial" : "Unpaid",
                        PaymentMethod = initialCreditPaymentMethod,
                        IsInitialCredit = true
                    });
                }

                // Sort by timestamp descending (most recent first)
                historyItems = historyItems.OrderByDescending(h => h.Timestamp).ToList();

                GameHistory = new ObservableCollection<GameHistoryItem>(historyItems);
                TotalGames = historyItems.Where(h => !h.IsInitialCredit).Count();
                TotalWins = totalWins;
                TotalLosses = totalLosses;
                
                // Calculate totals including initial credit
                TotalCharged = historyItems.Sum(h => h.TotalCharged);
                TotalPaid = historyItems.Sum(h => h.AmountPaid);
                BalanceDue = historyItems.Sum(h => h.AmountDue);
                
                OnPropertyChanged(nameof(WinRate));
                OnPropertyChanged(nameof(WinRateDisplay));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Error: {ex.Message}");
            }
        }
    }
}

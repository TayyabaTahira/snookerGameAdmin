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
        public decimal TotalCharged { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class CustomerHistoryViewModel : ViewModelBase
    {
        private readonly Customer _customer;
        private readonly LedgerService _ledgerService;
        private ObservableCollection<GameHistoryItem> _gameHistory = new();
        private int _totalGames;
        private decimal _totalCharged;
        private decimal _totalPaid;
        private decimal _balanceDue;

        public CustomerHistoryViewModel(Customer customer, LedgerService ledgerService)
        {
            _customer = customer;
            _ledgerService = ledgerService;
            
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

        private async void LoadHistory()
        {
            try
            {
                var context = App.GetDbContext();
                
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Loading history for customer: {_customer.FullName} (ID: {_customer.Id})");
                
                // Get all frames this customer participated in
                var participantFrames = await context.FrameParticipants
                    .Where(fp => fp.CustomerId == _customer.Id)
                    .Select(fp => fp.FrameId)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Found {participantFrames.Count} frames for customer");

                var frames = await context.Frames
                    .Where(f => participantFrames.Contains(f.Id) && f.EndedAt != null)
                    .OrderByDescending(f => f.EndedAt)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Found {frames.Count} completed frames");

                var historyItems = new List<GameHistoryItem>();

                foreach (var frame in frames)
                {
                    // Get session info
                    var session = await context.Sessions.FirstOrDefaultAsync(s => s.Id == frame.SessionId);
                    var gameType = session?.GameTypeId != null 
                        ? await context.Set<GameType>().FirstOrDefaultAsync(gt => gt.Id == session.GameTypeId)
                        : null;

                    // Get charges for this customer on this frame
                    var charges = await context.LedgerCharges
                        .Where(c => c.CustomerId == _customer.Id && c.FrameId == frame.Id)
                        .ToListAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Frame {frame.Id}: Found {charges.Count} charges");
                    
                    var chargeAmount = charges.Sum(c => c.AmountPk);
                    System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Frame {frame.Id}: Total charge amount = {chargeAmount}");

                    // Get payments allocated to these charges
                    var chargeIds = charges.Select(c => c.Id).ToList();
                    var paidAmount = await context.PaymentAllocations
                        .Where(pa => chargeIds.Contains(pa.ChargeId))
                        .SumAsync(pa => (decimal?)pa.AllocatedAmountPk) ?? 0;
                    
                    System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Frame {frame.Id}: Paid amount = {paidAmount}");
                    
                    // Calculate win streak at this point in time
                    // Get all frames in this session up to and including current frame
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
                        Result = frame.WinnerCustomerId == _customer.Id ? "Win" : "Lose",
                        WinStreak = winStreak,
                        TotalCharged = chargeAmount,
                        AmountDue = chargeAmount - paidAmount,  // This shows the remaining balance
                        AmountPaid = paidAmount,
                        PaymentStatus = chargeAmount == 0 ? "No Charge" : 
                                       paidAmount >= chargeAmount ? "Paid" : 
                                       paidAmount > 0 ? "Partial" : "Unpaid"
                    });
                }

                GameHistory = new ObservableCollection<GameHistoryItem>(historyItems);
                TotalGames = historyItems.Count;
                
                // Calculate totals
                TotalCharged = historyItems.Sum(h => h.TotalCharged);  // Total of all charges
                TotalPaid = historyItems.Sum(h => h.AmountPaid);        // Total of all payments
                BalanceDue = historyItems.Sum(h => h.AmountDue);        // Sum of remaining balances (already deducted)
                
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Summary:");
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory]   Total Games: {TotalGames}");
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory]   Total Charged: {TotalCharged}");
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory]   Total Paid: {TotalPaid}");
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory]   Balance Due: {BalanceDue}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Error loading customer history: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CustomerHistory] Stack trace: {ex.StackTrace}");
            }
        }
    }
}

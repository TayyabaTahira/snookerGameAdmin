using System.Collections.ObjectModel;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.ViewModels
{
    public class PayerModeOption
    {
        public PayerMode Value { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class PayStatusOption
    {
        public PayStatus Value { get; set; }
        public string Display { get; set; } = string.Empty;
    }

    public class FrameSummary
    {
        public int FrameNumber { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public string Loser { get; set; } = string.Empty;
        public decimal BaseRate { get; set; }
        public string BaseRateDisplay => $"PKR {BaseRate:N2}";
    }

    public class EndGameBillingViewModel : ViewModelBase
    {
        private readonly Session _session;
        private readonly decimal _baseRatePerFrame;
        private int _overtimeMinutes;
        private decimal _overtimeAmount;
        private decimal _lumpSumFine;
        private decimal _discount;
        private decimal _partialPaymentAmount;
        private PayerModeOption? _selectedPayerMode;
        private PayStatusOption? _selectedPayStatus;

        public EndGameBillingViewModel(Session session, decimal baseRate)
        {
            _session = session;
            _baseRatePerFrame = baseRate;

            // Build frame summaries
            FrameSummaries = new ObservableCollection<FrameSummary>();
            int frameNumber = 1;
            foreach (var frame in _session.Frames.OrderBy(f => f.StartedAt))
            {
                var duration = frame.EndedAt.HasValue 
                    ? (frame.EndedAt.Value - frame.StartedAt) 
                    : (DateTime.Now - frame.StartedAt);
                
                // Try to get winner name from navigation property first, then fallback to participants
                var winnerName = "Not Set";
                if (frame.WinnerCustomer != null)
                {
                    winnerName = frame.WinnerCustomer.FullName;
                    System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: Got winner from WinnerCustomer: {winnerName}");
                }
                else if (frame.WinnerCustomerId != null)
                {
                    // Fallback: Try to find in participants
                    var winnerParticipant = frame.Participants.FirstOrDefault(p => p.CustomerId == frame.WinnerCustomerId.Value);
                    if (winnerParticipant?.Customer != null)
                    {
                        winnerName = winnerParticipant.Customer.FullName;
                        System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: Got winner from Participants: {winnerName}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: Winner participant not found or has no customer");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: No WinnerCustomerId set");
                }

                // Try to get loser name from navigation property first, then fallback to participants
                var loserName = "Not Set";
                if (frame.LoserCustomer != null)
                {
                    loserName = frame.LoserCustomer.FullName;
                    System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: Got loser from LoserCustomer: {loserName}");
                }
                else if (frame.LoserCustomerId != null)
                {
                    // Fallback: Try to find in participants
                    var loserParticipant = frame.Participants.FirstOrDefault(p => p.CustomerId == frame.LoserCustomerId.Value);
                    if (loserParticipant?.Customer != null)
                    {
                        loserName = loserParticipant.Customer.FullName;
                        System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: Got loser from Participants: {loserName}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: Loser participant not found or has no customer");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}: No LoserCustomerId set");
                }

                // Debug output
                System.Diagnostics.Debug.WriteLine($"[EndGameBillingViewModel] Frame {frameNumber}:");
                System.Diagnostics.Debug.WriteLine($"  WinnerCustomerId: {frame.WinnerCustomerId}");
                System.Diagnostics.Debug.WriteLine($"  WinnerCustomer is null: {frame.WinnerCustomer == null}");
                System.Diagnostics.Debug.WriteLine($"  LoserCustomerId: {frame.LoserCustomerId}");
                System.Diagnostics.Debug.WriteLine($"  LoserCustomer is null: {frame.LoserCustomer == null}");
                System.Diagnostics.Debug.WriteLine($"  Participants count: {frame.Participants.Count}");
                foreach (var p in frame.Participants)
                {
                    System.Diagnostics.Debug.WriteLine($"    Participant: {p.CustomerId}, Customer null? {p.Customer == null}, Name: {p.Customer?.FullName}");
                }
                System.Diagnostics.Debug.WriteLine($"  Winner name resolved: {winnerName}");
                System.Diagnostics.Debug.WriteLine($"  Loser name resolved: {loserName}");

                FrameSummaries.Add(new FrameSummary
                {
                    FrameNumber = frameNumber++,
                    Duration = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}",
                    Winner = winnerName,
                    Loser = loserName,
                    BaseRate = frame.BaseRatePk
                });
            }

            // Initialize collections
            PayerModes = new ObservableCollection<PayerModeOption>
            {
                new() { Value = PayerMode.LOSER, Display = "Loser Pays" },
                new() { Value = PayerMode.WINNER, Display = "Winner Pays" },
                new() { Value = PayerMode.SPLIT, Display = "Split Equally" }
            };

            PayStatuses = new ObservableCollection<PayStatusOption>
            {
                new() { Value = PayStatus.PAID, Display = "Paid Now" },
                new() { Value = PayStatus.UNPAID, Display = "Credit (Unpaid)" },
                new() { Value = PayStatus.PARTIAL, Display = "Partial Payment" }
            };

            // Set defaults
            _selectedPayerMode = PayerModes[0];
            _selectedPayStatus = PayStatuses[1]; // Default to Credit
        }

        public ObservableCollection<FrameSummary> FrameSummaries { get; }

        public int TotalFrames => _session.Frames.Count;

        public string SessionDuration
        {
            get
            {
                var duration = DateTime.Now - _session.StartedAt;
                return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
        }

        public decimal BaseRateTotal => _session.Frames.Sum(f => f.BaseRatePk);
        
        public string BaseRateTotalDisplay => $"PKR {BaseRateTotal:N2}";

        public int OvertimeMinutes
        {
            get => _overtimeMinutes;
            set
            {
                if (SetProperty(ref _overtimeMinutes, value))
                {
                    OnPropertyChanged(nameof(TotalAmountDisplay));
                }
            }
        }

        public decimal OvertimeAmount
        {
            get => _overtimeAmount;
            set
            {
                if (SetProperty(ref _overtimeAmount, value))
                {
                    OnPropertyChanged(nameof(TotalAmountDisplay));
                }
            }
        }

        public decimal LumpSumFine
        {
            get => _lumpSumFine;
            set
            {
                if (SetProperty(ref _lumpSumFine, value))
                {
                    OnPropertyChanged(nameof(TotalAmountDisplay));
                }
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                if (SetProperty(ref _discount, value))
                {
                    OnPropertyChanged(nameof(TotalAmountDisplay));
                }
            }
        }

        public decimal PartialPaymentAmount
        {
            get => _partialPaymentAmount;
            set => SetProperty(ref _partialPaymentAmount, value);
        }

        public bool IsPartialPaymentVisible => _selectedPayStatus?.Value == PayStatus.PARTIAL;

        public decimal TotalAmount => BaseRateTotal + _overtimeAmount + _lumpSumFine - _discount;

        public string TotalAmountDisplay => $"PKR {TotalAmount:N2}";

        public ObservableCollection<PayerModeOption> PayerModes { get; }

        public PayerModeOption? SelectedPayerMode
        {
            get => _selectedPayerMode;
            set
            {
                if (SetProperty(ref _selectedPayerMode, value))
                {
                    OnPropertyChanged(nameof(WhoWillPayDisplay));
                }
            }
        }

        public ObservableCollection<PayStatusOption> PayStatuses { get; }

        public PayStatusOption? SelectedPayStatus
        {
            get => _selectedPayStatus;
            set
            {
                if (SetProperty(ref _selectedPayStatus, value))
                {
                    OnPropertyChanged(nameof(IsPartialPaymentVisible));
                }
            }
        }

        public PayerMode PayerMode => _selectedPayerMode?.Value ?? PayerMode.LOSER;
        public PayStatus PayStatus => _selectedPayStatus?.Value ?? PayStatus.UNPAID;

        public string WhoWillPayDisplay
        {
            get
            {
                var mode = _selectedPayerMode?.Value ?? PayerMode.LOSER;
                var lastFrame = _session.Frames.LastOrDefault();
                var playerCount = lastFrame?.Participants.Count ?? 0;
                
                return mode switch
                {
                    PayerMode.LOSER => "The losing player of the final frame will be charged the full session amount",
                    PayerMode.WINNER => "The winning player of the final frame will be charged the full session amount",
                    PayerMode.SPLIT => $"Session amount will be split equally among {playerCount} players",
                    _ => "Payment mode not selected"
                };
            }
        }
    }
}

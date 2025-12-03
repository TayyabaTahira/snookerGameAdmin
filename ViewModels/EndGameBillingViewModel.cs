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

    public class EndGameBillingViewModel : ViewModelBase
    {
        private readonly Session _session;
        private readonly decimal _baseRate;
        private int _overtimeMinutes;
        private decimal _overtimeAmount;
        private decimal _lumpSumFine;
        private decimal _discount;
        private PayerModeOption? _selectedPayerMode;
        private PayStatusOption? _selectedPayStatus;

        public EndGameBillingViewModel(Session session, decimal baseRate)
        {
            _session = session;
            _baseRate = baseRate;

            // Initialize collections
            PayerModes = new ObservableCollection<PayerModeOption>
            {
                new() { Value = PayerMode.LOSER, Display = "Loser Pays" },
                new() { Value = PayerMode.SPLIT, Display = "Split Between Players" },
                new() { Value = PayerMode.EACH, Display = "Each Player Pays" }
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

        public int FrameCount => _session.Frames.Count;

        public string DurationDisplay
        {
            get
            {
                var duration = DateTime.Now - _session.StartedAt;
                return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
        }

        public string BaseRateDisplay => $"PKR {_baseRate:N2}";

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

        public decimal TotalAmount => _baseRate + _overtimeAmount + _lumpSumFine - _discount;

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
            set => SetProperty(ref _selectedPayStatus, value);
        }

        public PayerMode PayerMode => _selectedPayerMode?.Value ?? PayerMode.LOSER;
        public PayStatus PayStatus => _selectedPayStatus?.Value ?? PayStatus.UNPAID;

        public string WhoWillPayDisplay
        {
            get
            {
                var mode = _selectedPayerMode?.Value ?? PayerMode.LOSER;
                var playerCount = _session.Frames.LastOrDefault()?.Participants.Count ?? 0;
                
                return mode switch
                {
                    PayerMode.LOSER => "The losing player will be charged the full amount",
                    PayerMode.SPLIT => $"Amount will be split equally among {playerCount} players",
                    PayerMode.EACH => $"Each of the {playerCount} players will be charged the full amount",
                    _ => "Payment mode not selected"
                };
            }
        }
    }
}

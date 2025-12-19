using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly LedgerService _ledgerService;
        private readonly SessionService _sessionService;
        private readonly CustomerService _customerService;
        
        private DateTime _fromDate = DateTime.Today.AddMonths(-1);
        private DateTime _toDate = DateTime.Today;
        private decimal _todayRevenue;
        private decimal _rangeRevenue;
        private decimal _outstandingCredit;
        private int _todayGames;
        private int _rangeGames;
        private ObservableCollection<TopPlayerViewModel> _topPlayers = new();
        private ObservableCollection<RecentGameViewModel> _recentGames = new();
        private bool _isLoading;

        public ReportsViewModel(
            LedgerService ledgerService,
            SessionService sessionService,
            CustomerService customerService)
        {
            _ledgerService = ledgerService;
            _sessionService = sessionService;
            _customerService = customerService;
            
            RefreshCommand = new RelayCommand(async _ => await LoadReportsAsync());
            
            _ = LoadReportsAsync();
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value))
                {
                    OnPropertyChanged(nameof(TodayRevenueLabel));
                    OnPropertyChanged(nameof(RangeRevenueLabel));
                    OnPropertyChanged(nameof(TodayGamesLabel));
                    OnPropertyChanged(nameof(RangeGamesLabel));
                    OnPropertyChanged(nameof(AverageLabel));
                    _ = LoadReportsAsync();
                }
            }
        }

        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value))
                {
                    OnPropertyChanged(nameof(TodayRevenueLabel));
                    OnPropertyChanged(nameof(RangeRevenueLabel));
                    OnPropertyChanged(nameof(TodayGamesLabel));
                    OnPropertyChanged(nameof(RangeGamesLabel));
                    OnPropertyChanged(nameof(AverageLabel));
                    _ = LoadReportsAsync();
                }
            }
        }

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set => SetProperty(ref _todayRevenue, value);
        }

        public decimal RangeRevenue
        {
            get => _rangeRevenue;
            set
            {
                if (SetProperty(ref _rangeRevenue, value))
                {
                    OnPropertyChanged(nameof(AveragePerGame));
                }
            }
        }

        public decimal OutstandingCredit
        {
            get => _outstandingCredit;
            set => SetProperty(ref _outstandingCredit, value);
        }

        public int TodayGames
        {
            get => _todayGames;
            set
            {
                if (SetProperty(ref _todayGames, value))
                {
                    OnPropertyChanged(nameof(TodayGamesLabel));
                }
            }
        }

        public int RangeGames
        {
            get => _rangeGames;
            set
            {
                if (SetProperty(ref _rangeGames, value))
                {
                    OnPropertyChanged(nameof(AveragePerGame));
                    OnPropertyChanged(nameof(RangeGamesLabel));
                }
            }
        }

        public ObservableCollection<TopPlayerViewModel> TopPlayers
        {
            get => _topPlayers;
            set => SetProperty(ref _topPlayers, value);
        }

        public ObservableCollection<RecentGameViewModel> RecentGames
        {
            get => _recentGames;
            set => SetProperty(ref _recentGames, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public decimal AveragePerGame => RangeGames > 0 ? RangeRevenue / RangeGames : 0;

        // Dynamic Labels
        public string TodayRevenueLabel
        {
            get
            {
                if (ToDate.Date == DateTime.Today)
                    return "Today's Revenue";
                else
                    return $"{ToDate:M/dd/yyyy} Revenue";
            }
        }

        public string RangeRevenueLabel
        {
            get
            {
                var monthAgo = DateTime.Today.AddMonths(-1);
                if (FromDate.Date == monthAgo.Date && ToDate.Date == DateTime.Today)
                    return "This Month Revenue";
                else
                    return "Selected Range Revenue";
            }
        }

        public string TodayGamesLabel => $"{TodayGames} games";

        public string RangeGamesLabel => $"{RangeGames} games";

        public string AverageLabel
        {
            get
            {
                if (ToDate.Date == DateTime.Today && FromDate.Date == DateTime.Today.AddMonths(-1))
                    return "This month average";
                else
                    return "Range average";
            }
        }

        public ICommand RefreshCommand { get; }

        private async Task LoadReportsAsync()
        {
            IsLoading = true;
            try
            {
                // Today's revenue (based on To Date)
                var todayStart = ToDate.Date;
                var todayEnd = todayStart.AddDays(1);
                TodayRevenue = await _ledgerService.GetTotalRevenueAsync(todayStart, todayEnd);
                
                // Range revenue (from From Date to To Date)
                var rangeStart = FromDate.Date;
                var rangeEnd = ToDate.Date.AddDays(1);
                RangeRevenue = await _ledgerService.GetTotalRevenueAsync(rangeStart, rangeEnd);
                
                // Outstanding credit (always total)
                OutstandingCredit = await _ledgerService.GetTotalOutstandingCreditAsync();
                
                // Today's games count (based on To Date)
                var todayFrames = await _sessionService.GetFramesByDateRangeAsync(todayStart, todayEnd);
                TodayGames = todayFrames.Count();
                
                // Range games count
                var rangeFrames = await _sessionService.GetFramesByDateRangeAsync(rangeStart, rangeEnd);
                RangeGames = rangeFrames.Count();
                
                // Top players - based on date range
                var customers = await _customerService.GetAllCustomersAsync();
                var topPlayersList = customers
                    .Select(c => new
                    {
                        Customer = c,
                        GamesPlayed = rangeFrames.Count(f =>
                            f.Participants.Any(p => p.CustomerId == c.Id)),
                        GamesWon = rangeFrames.Count(f => f.WinnerCustomerId == c.Id)
                    })
                    .Where(x => x.GamesPlayed > 0)
                    .OrderByDescending(x => x.GamesWon)
                    .ThenByDescending(x => x.GamesPlayed)
                    .Take(10)
                    .Select(x => new TopPlayerViewModel
                    {
                        Name = x.Customer.FullName,
                        GamesPlayed = x.GamesPlayed,
                        GamesWon = x.GamesWon,
                        WinRate = x.GamesPlayed > 0 ? (x.GamesWon * 100.0 / x.GamesPlayed) : 0
                    })
                    .ToList();
                
                TopPlayers = new ObservableCollection<TopPlayerViewModel>(topPlayersList);
                
                // Recent games - based on date range (last 20 in range) with correct frame times
                var recentFrames = rangeFrames
                    .Where(f => f.EndedAt != null) // Only show completed frames
                    .OrderByDescending(f => f.EndedAt)
                    .Take(20)
                    .Select(f =>
                    {
                        var frameEndTime = f.EndedAt ?? DateTime.Now;
                        var frameDuration = frameEndTime - f.StartedAt;
                        var durationStr = $"{(int)frameDuration.TotalHours:D2}:{frameDuration.Minutes:D2}:{frameDuration.Seconds:D2}";
                        
                        return new RecentGameViewModel
                        {
                            Date = f.EndedAt.Value.ToString("MMM dd, HH:mm"),
                            Players = string.Join(" vs ", f.Participants.Select(p => p.Customer?.FullName ?? "Unknown")),
                            Winner = f.WinnerCustomerId != null ?
                                f.Participants.FirstOrDefault(p => p.CustomerId == f.WinnerCustomerId)?.Customer?.FullName ?? "Unknown" :
                                "-",
                            Duration = durationStr,
                            Amount = $"PKR {f.TotalAmountPk:N2}"
                        };
                    })
                    .ToList();
                
                RecentGames = new ObservableCollection<RecentGameViewModel>(recentFrames);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reports: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class TopPlayerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public double WinRate { get; set; }
        public string WinRateDisplay => $"{WinRate:F1}%";
    }

    public class RecentGameViewModel
    {
        public string Date { get; set; } = string.Empty;
        public string Players { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
    }
}

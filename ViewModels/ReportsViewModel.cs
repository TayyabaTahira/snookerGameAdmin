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
        
        private DateTime _selectedDate = DateTime.Today;
        private decimal _todayRevenue;
        private decimal _monthRevenue;
        private decimal _outstandingCredit;
        private int _todayGames;
        private int _monthGames;
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

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    _ = LoadReportsAsync();
                }
            }
        }

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set
            {
                if (SetProperty(ref _todayRevenue, value))
                {
                    OnPropertyChanged(nameof(AveragePerGame));
                }
            }
        }

        public decimal MonthRevenue
        {
            get => _monthRevenue;
            set => SetProperty(ref _monthRevenue, value);
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
                    OnPropertyChanged(nameof(AveragePerGame));
                }
            }
        }

        public int MonthGames
        {
            get => _monthGames;
            set => SetProperty(ref _monthGames, value);
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

        public decimal AveragePerGame => TodayGames > 0 ? TodayRevenue / TodayGames : 0;

        public ICommand RefreshCommand { get; }

        private async Task LoadReportsAsync()
        {
            IsLoading = true;
            try
            {
                // Today's revenue
                var todayStart = _selectedDate.Date;
                var todayEnd = todayStart.AddDays(1);
                TodayRevenue = await _ledgerService.GetTotalRevenueAsync(todayStart, todayEnd);
                
                // Month's revenue
                var monthStart = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                MonthRevenue = await _ledgerService.GetTotalRevenueAsync(monthStart, monthEnd);
                
                // Outstanding credit
                OutstandingCredit = await _ledgerService.GetTotalOutstandingCreditAsync();
                
                // Today's games count
                var todayFrames = await _sessionService.GetFramesByDateRangeAsync(todayStart, todayEnd);
                TodayGames = todayFrames.Count();
                
                // Month's games count
                var monthFrames = await _sessionService.GetFramesByDateRangeAsync(monthStart, monthEnd);
                MonthGames = monthFrames.Count();
                
                // Top players (by games played this month)
                var customers = await _customerService.GetAllCustomersAsync();
                var topPlayersList = customers
                    .Select(c => new
                    {
                        Customer = c,
                        GamesPlayed = monthFrames.Count(f =>
                            f.Participants.Any(p => p.CustomerId == c.Id)),
                        GamesWon = monthFrames.Count(f => f.WinnerCustomerId == c.Id)
                    })
                    .Where(x => x.GamesPlayed > 0)
                    .OrderByDescending(x => x.GamesPlayed)
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
                
                // Recent games (last 20)
                var recentFrames = monthFrames
                    .OrderByDescending(f => f.EndedAt ?? f.StartedAt)
                    .Take(20)
                    .Select(f => new RecentGameViewModel
                    {
                        Date = (f.EndedAt ?? f.StartedAt).ToString("MMM dd, HH:mm"),
                        Players = string.Join(" vs ", f.Participants.Select(p => p.Customer?.FullName ?? "Unknown")),
                        Winner = f.WinnerCustomerId != null ?
                            f.Participants.FirstOrDefault(p => p.CustomerId == f.WinnerCustomerId)?.Customer?.FullName ?? "Unknown" :
                            "-",
                        Amount = $"PKR {f.TotalAmountPk:N2}"
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
        public string Amount { get; set; } = string.Empty;
    }
}

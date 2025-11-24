using System.Collections.ObjectModel;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly SessionService _sessionService;
        private readonly GameRuleService _gameRuleService;
        private ObservableCollection<SessionTileViewModel> _sessions = new();
        private bool _isLoading;

        public DashboardViewModel(SessionService sessionService, GameRuleService gameRuleService)
        {
            _sessionService = sessionService;
            _gameRuleService = gameRuleService;
            
            AddTableCommand = new RelayCommand(async _ => await AddTableAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadSessionsAsync());
            
            _ = LoadSessionsAsync();
        }

        public ObservableCollection<SessionTileViewModel> Sessions
        {
            get => _sessions;
            set => SetProperty(ref _sessions, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand AddTableCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadSessionsAsync()
        {
            IsLoading = true;
            try
            {
                var sessions = await _sessionService.GetActiveSessionsAsync();
                
                Sessions.Clear();
                foreach (var session in sessions)
                {
                    Sessions.Add(new SessionTileViewModel(session));
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading sessions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddTableAsync()
        {
            try
            {
                int tableNumber = await _sessionService.GetNextTableNumberAsync();
                string tableName = $"Table #{tableNumber}";
                
                var session = await _sessionService.CreateSessionAsync(tableName);
                Sessions.Add(new SessionTileViewModel(session));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding table: {ex.Message}");
            }
        }
    }

    public class SessionTileViewModel : ViewModelBase
    {
        private readonly Session _session;

        public SessionTileViewModel(Session session)
        {
            _session = session;
        }

        public string Id => _session.Id.ToString();
        public string Name => _session.Name;
        public string GameTypeName => _session.GameType?.Name ?? "Not Set";
        public int FrameCount => _session.Frames.Count;
        
        public TimeSpan ElapsedTime => DateTime.Now - _session.StartedAt;
        
        public string ElapsedTimeDisplay => 
            $"{(int)ElapsedTime.TotalHours:D2}:{ElapsedTime.Minutes:D2}:{ElapsedTime.Seconds:D2}";

        public string PlayersDisplay
        {
            get
            {
                var lastFrame = _session.Frames.LastOrDefault();
                if (lastFrame == null) return "No players";
                
                var players = lastFrame.Participants.Select(p => p.Customer?.FullName ?? "Unknown");
                return string.Join(" vs ", players);
            }
        }
    }
}

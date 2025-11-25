using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;
using SnookerGameManagementSystem.Views;

namespace SnookerGameManagementSystem.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly SessionService _sessionService;
        private readonly GameRuleService _gameRuleService;
        private readonly GameTypeService _gameTypeService;
        private readonly CustomerService _customerService;
        private ObservableCollection<SessionTileViewModel> _sessions = new();
        private bool _isLoading;

        public DashboardViewModel(
            SessionService sessionService, 
            GameRuleService gameRuleService, 
            GameTypeService gameTypeService,
            CustomerService customerService)
        {
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Constructor started");
            
            _sessionService = sessionService;
            _gameRuleService = gameRuleService;
            _gameTypeService = gameTypeService;
            _customerService = customerService;
            
            AddTableCommand = new RelayCommand(async _ => await AddTableAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadSessionsAsync());
            OpenTableCommand = new RelayCommand(param => OpenTable(param as SessionTileViewModel));
            CustomersCommand = new RelayCommand(_ => OpenCustomers());
            ReportsCommand = new RelayCommand(_ => OpenReports());
            
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Commands initialized");
            
            // Load sessions asynchronously on the UI thread to ensure proper initialization
            Application.Current.Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Starting initial LoadSessionsAsync");
                    await LoadSessionsAsync();
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Initial LoadSessionsAsync completed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Error in initial load: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Stack: {ex.StackTrace}");
                    
                    // Show error to user
                    MessageBox.Show(
                        $"Error loading dashboard data: {ex.Message}",
                        "Dashboard Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            });
            
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Constructor completed");
        }

        public ObservableCollection<SessionTileViewModel> Sessions
        {
            get => _sessions;
            set => SetProperty(ref _sessions, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set 
            { 
                if (SetProperty(ref _isLoading, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] IsLoading changed to: {value}");
                }
            }
        }

        public ICommand AddTableCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenTableCommand { get; }
        public ICommand CustomersCommand { get; }
        public ICommand ReportsCommand { get; }

        private async Task LoadSessionsAsync()
        {
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] LoadSessionsAsync started");
            IsLoading = true;
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Fetching active sessions...");
                var sessions = await _sessionService.GetActiveSessionsAsync();
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Retrieved {sessions?.Count() ?? 0} sessions");
                
                // Update UI on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Sessions.Clear();
                    foreach (var session in sessions)
                    {
                        var tileViewModel = new SessionTileViewModel(session)
                        {
                            OpenCommand = OpenTableCommand
                        };
                        Sessions.Add(tileViewModel);
                    }
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Added {Sessions.Count} session tiles to UI");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Error loading sessions: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Stack: {ex.StackTrace}");
                
                // Show error to user
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        $"Error loading sessions: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] LoadSessionsAsync completed, IsLoading = false");
            }
        }

        private async Task AddTableAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] ========== AddTableAsync started ==========");
                
                // Get next table number
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Getting next table number...");
                int tableNumber = await _sessionService.GetNextTableNumberAsync();
                string defaultTableName = $"Table #{tableNumber}";
                
                // Load game types for dialog
                var gameTypes = await _gameTypeService.GetAllGameTypesAsync();
                
                // Create and show dialog
                var dialogViewModel = new CreateSessionViewModel
                {
                    TableName = defaultTableName,
                    GameTypes = new ObservableCollection<GameType>(gameTypes)
                };
                
                var dialog = new CreateSessionDialog(dialogViewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Showing CreateSessionDialog...");
                
                bool? result = dialog.ShowDialog();
                
                if (result == true && dialogViewModel.SelectedGameType != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Creating session: {dialogViewModel.TableName}");
                    
                    // Create session with selected game type
                    var session = await _sessionService.CreateSessionAsync(
                        dialogViewModel.TableName, 
                        dialogViewModel.SelectedGameType.Id);
                    
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Session created with ID: {session.Id}");
                    
                    // Add to UI on the UI thread
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var tileViewModel = new SessionTileViewModel(session)
                        {
                            OpenCommand = OpenTableCommand
                        };
                        Sessions.Add(tileViewModel);
                        System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Session tile added to UI. Total sessions: {Sessions.Count}");
                    });
                    
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] ========== AddTableAsync completed successfully ==========");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Dialog cancelled or invalid selection");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] ========== ERROR in AddTableAsync ==========");
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Exception Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Error message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Inner stack: {ex.InnerException.StackTrace}");
                }
                
                // Show error to user on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        $"Error adding table:\n\n{ex.Message}\n\nPlease check the Output window for details.",
                        "Error Creating Table",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }

        private void OpenTable(SessionTileViewModel? tileViewModel)
        {
            if (tileViewModel == null) return;

            try
            {
                var detailViewModel = new TableDetailViewModel(
                    tileViewModel.Session,
                    _sessionService,
                    _customerService);

                // Subscribe to events
                detailViewModel.SessionEnded += async (s, e) => await LoadSessionsAsync();
                detailViewModel.SessionDeleted += async (s, e) => await LoadSessionsAsync();

                var detailWindow = new TableDetailWindow(detailViewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                detailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening table details:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenCustomers()
        {
            try
            {
                var viewModel = new CustomerManagementViewModel(_customerService);
                var window = new Views.CustomerManagementWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening customer management:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenReports()
        {
            MessageBox.Show(
                "Reports window will be implemented here.\n\n" +
                "Features:\n" +
                "- Daily/Monthly revenue\n" +
                "- Outstanding balances\n" +
                "- Top players\n" +
                "- Frame statistics",
                "Reports - TODO",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    public class SessionTileViewModel : ViewModelBase
    {
        public Session Session { get; }

        public SessionTileViewModel(Session session)
        {
            Session = session;
        }

        public string Id => Session.Id.ToString();
        public string Name => Session.Name;
        public string GameTypeName => Session.GameType?.Name ?? "Not Set";
        public int FrameCount => Session.Frames.Count;
        
        public TimeSpan ElapsedTime => DateTime.Now - Session.StartedAt;
        
        public string ElapsedTimeDisplay => 
            $"{(int)ElapsedTime.TotalHours:D2}:{ElapsedTime.Minutes:D2}:{ElapsedTime.Seconds:D2}";

        public string PlayersDisplay
        {
            get
            {
                var lastFrame = Session.Frames.LastOrDefault();
                if (lastFrame == null) return "No players";
                
                var players = lastFrame.Participants.Select(p => p.Customer?.FullName ?? "Unknown");
                return string.Join(" vs ", players);
            }
        }

        public ICommand? OpenCommand { get; set; }
    }
}

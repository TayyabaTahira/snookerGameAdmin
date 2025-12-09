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
        private readonly TableService _tableService;
        private readonly GameRuleService _gameRuleService;
        private readonly GameTypeService _gameTypeService;
        private readonly CustomerService _customerService;
        private ObservableCollection<TableTileViewModel> _tables = new();
        private bool _isLoading;
        private System.Windows.Threading.DispatcherTimer? _timerUpdateTimer;

        public DashboardViewModel(
            SessionService sessionService,
            TableService tableService,
            GameRuleService gameRuleService, 
            GameTypeService gameTypeService,
            CustomerService customerService)
        {
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Constructor started");
            
            _sessionService = sessionService;
            _tableService = tableService;
            _gameRuleService = gameRuleService;
            _gameTypeService = gameTypeService;
            _customerService = customerService;
            
            StartSessionCommand = new RelayCommand(async param => await StartSessionAsync(param as TableTileViewModel));
            ViewSessionCommand = new RelayCommand(param => ViewSession(param as TableTileViewModel));
            RefreshCommand = new RelayCommand(async _ => await LoadTablesAsync());
            TablesManagementCommand = new RelayCommand(_ => OpenTablesManagement());
            GameTypesCommand = new RelayCommand(_ => OpenGameTypes());
            CustomersCommand = new RelayCommand(_ => OpenCustomers());
            ReportsCommand = new RelayCommand(_ => OpenReports());
            
            // Setup timer to update elapsed time every second
            _timerUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            _timerUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            _timerUpdateTimer.Tick += (s, e) => UpdateSessionTimers();
            _timerUpdateTimer.Start();
            
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Commands initialized");
            
            // Load tables asynchronously on the UI thread to ensure proper initialization
            Application.Current.Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Starting initial LoadTablesAsync");
                    await LoadTablesAsync();
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Initial LoadTablesAsync completed");
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

        private void UpdateSessionTimers()
        {
            foreach (var table in Tables)
            {
                if (table.HasActiveSession)
                {
                    table.RefreshElapsedTime();
                }
            }
        }

        public ObservableCollection<TableTileViewModel> Tables
        {
            get => _tables;
            set => SetProperty(ref _tables, value);
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

        public ICommand StartSessionCommand { get; }
        public ICommand ViewSessionCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand TablesManagementCommand { get; }
        public ICommand GameTypesCommand { get; }
        public ICommand CustomersCommand { get; }
        public ICommand ReportsCommand { get; }

        private async Task LoadTablesAsync()
        {
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] LoadTablesAsync started");
            IsLoading = true;
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] Fetching active tables...");
                var tables = await _tableService.GetActiveTablesAsync();
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Retrieved {tables?.Count() ?? 0} tables");
                
                // Update UI on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    Tables.Clear();
                    foreach (var table in tables)
                    {
                        var hasActiveSession = await _tableService.HasActiveSessionAsync(table.Id);
                        var activeSession = hasActiveSession ? await _tableService.GetActiveSessionForTableAsync(table.Id) : null;
                        
                        var tileViewModel = new TableTileViewModel(table, hasActiveSession, activeSession);
                        Tables.Add(tileViewModel);
                    }
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Added {Tables.Count} table tiles to UI");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Error loading tables: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Stack: {ex.StackTrace}");
                
                // Show error to user
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        $"Error loading tables: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("[DashboardViewModel] LoadTablesAsync completed, IsLoading = false");
            }
        }

        private async Task StartSessionAsync(TableTileViewModel? tableTile)
        {
            if (tableTile == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Starting session on table: {tableTile.Name}");
                
                // Load game types for dialog
                var gameTypes = await _gameTypeService.GetAllGameTypesAsync();
                
                // Create and show dialog
                var dialogViewModel = new CreateSessionViewModel(_customerService)
                {
                    TableName = tableTile.Name, // Pre-filled from table
                    IsTableNameReadOnly = true, // Make it read-only
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
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Creating session on table: {tableTile.Name}");
                    
                    // Create session with selected game type and table ID
                    using (var context = App.GetDbContext())
                    {
                        var session = new Session
                        {
                            TableId = tableTile.Id,
                            Name = tableTile.Name,
                            GameTypeId = dialogViewModel.SelectedGameType.Id,
                            StartedAt = DateTime.Now,
                            Status = SessionStatus.IN_PROGRESS
                        };
                        
                        context.Sessions.Add(session);
                        await context.SaveChangesAsync();
                        
                        System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Session created with ID: {session.Id}");
                        
                        // If players were selected, create first frame
                        if (dialogViewModel.SelectedCustomers.Count >= 2)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Creating first frame with {dialogViewModel.SelectedCustomers.Count} players");
                            
                            // Get base rate from game rule
                            var rule = await _gameRuleService.GetRuleByGameTypeIdAsync(dialogViewModel.SelectedGameType.Id);
                            decimal baseRate = rule?.BaseRate ?? 0;
                            
                            var frameService = new FrameService(context);
                            var playerIds = dialogViewModel.SelectedCustomers.Select(c => c.Id).ToList();
                            await frameService.CreateFrameAsync(session.Id, playerIds, baseRate);
                            
                            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] First frame created");
                        }
                    }
                    
                    // Reload tables to show updated status
                    await LoadTablesAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DashboardViewModel] StartSessionAsync cancelled or no game type selected");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Error in StartSessionAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Stack: {ex.StackTrace}");
                
                // Show error to user
                MessageBox.Show(
                    $"Error starting session: {ex.Message}",
                    "Session Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ViewSession(TableTileViewModel? tableTile)
        {
            if (tableTile == null || !tableTile.HasActiveSession || tableTile.ActiveSession == null) return;

            try
            {
                var detailViewModel = new TableDetailViewModel(
                    tableTile.ActiveSession,
                    _sessionService,
                    _customerService);

                // Subscribe to events
                detailViewModel.SessionEnded += async (s, e) => await LoadTablesAsync();
                detailViewModel.SessionDeleted += async (s, e) => await LoadTablesAsync();

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

        private void OpenTablesManagement()
        {
            try
            {
                var window = new Views.TableManagementWindow(_tableService)
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
                
                // Reload tables after closing management window
                _ = LoadTablesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening tables management:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenGameTypes()
        {
            try
            {
                var viewModel = new GameTypeManagementViewModel(_gameTypeService);
                var window = new Views.GameTypeManagementWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening game type management:\n\n{ex.Message}",
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
            try
            {
                var ledgerService = new LedgerService(App.GetDbContext());
                var viewModel = new ReportsViewModel(ledgerService, _sessionService, _customerService);
                var window = new Views.ReportsWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening reports:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    public class TableTileViewModel : ViewModelBase
    {
        private readonly Table _table;
        private bool _hasActiveSession;
        private Session? _activeSession;

        public TableTileViewModel(Table table, bool hasActiveSession, Session? activeSession)
        {
            _table = table;
            _hasActiveSession = hasActiveSession;
            _activeSession = activeSession;
        }

        public Guid Id => _table.Id;
        public string Name => _table.Name;
        public int DisplayOrder => _table.DisplayOrder;
        public bool IsActive => _table.IsActive;
        
        public bool HasActiveSession
        {
            get => _hasActiveSession;
            set => SetProperty(ref _hasActiveSession, value);
        }

        public Session? ActiveSession
        {
            get => _activeSession;
            set
            {
                if (SetProperty(ref _activeSession, value))
                {
                    OnPropertyChanged(nameof(GameTypeName));
                    OnPropertyChanged(nameof(FrameCount));
                    OnPropertyChanged(nameof(PlayersDisplay));
                }
            }
        }

        public string GameTypeName => ActiveSession?.GameType?.Name ?? "N/A";
        public int FrameCount => ActiveSession?.Frames.Count ?? 0;
        
        public string StatusBadge => HasActiveSession ? "In Use" : "Available";
        public string StatusColor => HasActiveSession ? "#e94560" : "#4caf50";
        public bool ShowSessionInfo => HasActiveSession && ActiveSession != null;
        
        public TimeSpan ElapsedTime => HasActiveSession && ActiveSession != null 
            ? DateTime.Now - ActiveSession.StartedAt 
            : TimeSpan.Zero;
        
        public string ElapsedTimeDisplay => 
            $"{(int)ElapsedTime.TotalHours:D2}:{ElapsedTime.Minutes:D2}:{ElapsedTime.Seconds:D2}";

        public string PlayersDisplay
        {
            get
            {
                if (ActiveSession == null) return "No players";
                
                var lastFrame = ActiveSession.Frames.LastOrDefault();
                if (lastFrame == null) return "No players";
                
                var players = lastFrame.Participants.Select(p => p.Customer?.FullName ?? "Unknown");
                return string.Join(" vs ", players);
            }
        }

        public Table GetTable() => _table;

        public void RefreshElapsedTime()
        {
            OnPropertyChanged(nameof(ElapsedTime));
            OnPropertyChanged(nameof(ElapsedTimeDisplay));
        }
    }
}

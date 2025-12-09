using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class TableManagementViewModel : ViewModelBase
    {
        private readonly TableService _tableService;
        private ObservableCollection<TableInfoViewModel> _tables = new();
        private bool _isLoading;

        public TableManagementViewModel(TableService tableService)
        {
            _tableService = tableService;
            
            AddTableCommand = new RelayCommand(async _ => await AddTable());
            EditTableCommand = new RelayCommand(async param => await EditTable(param as TableInfoViewModel));
            DeleteTableCommand = new RelayCommand(async param => await DeleteTable(param as TableInfoViewModel));
            RefreshCommand = new RelayCommand(async _ => await LoadTablesAsync());
            
            _ = LoadTablesAsync();
        }

        public ObservableCollection<TableInfoViewModel> Tables
        {
            get => _tables;
            set => SetProperty(ref _tables, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand AddTableCommand { get; }
        public ICommand EditTableCommand { get; }
        public ICommand DeleteTableCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadTablesAsync()
        {
            IsLoading = true;
            try
            {
                var tables = await _tableService.GetAllTablesAsync();
                var tableTiles = new List<TableInfoViewModel>();

                foreach (var table in tables)
                {
                    var hasActiveSession = await _tableService.HasActiveSessionAsync(table.Id);
                    var activeSession = hasActiveSession ? await _tableService.GetActiveSessionForTableAsync(table.Id) : null;
                    
                    tableTiles.Add(new TableInfoViewModel(table, hasActiveSession, activeSession));
                }

                Tables = new ObservableCollection<TableInfoViewModel>(tableTiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading tables: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddTable()
        {
            try
            {
                var dialog = new Views.EditTableDialog(null, _tableService)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true)
                {
                    await LoadTablesAsync();
                    MessageBox.Show(
                        "Table created successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error creating table: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task EditTable(TableInfoViewModel? tableTile)
        {
            if (tableTile == null) return;

            try
            {
                var dialog = new Views.EditTableDialog(tableTile.GetTable(), _tableService)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true)
                {
                    await LoadTablesAsync();
                    MessageBox.Show(
                        "Table updated successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error updating table: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DeleteTable(TableInfoViewModel? tableTile)
        {
            if (tableTile == null) return;

            try
            {
                if (tableTile.HasActiveSession)
                {
                    MessageBox.Show(
                        "Cannot delete a table with an active session. Please end the session first.",
                        "Cannot Delete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{tableTile.Name}'?\n\nThis will deactivate the table, but historical sessions will remain.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _tableService.DeleteTableAsync(tableTile.Id);
                    await LoadTablesAsync();
                    
                    MessageBox.Show(
                        "Table deleted successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting table: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public async Task ReorderTablesAsync(TableInfoViewModel draggedItem, TableInfoViewModel targetItem)
        {
            try
            {
                // Get current indices
                int draggedIndex = Tables.IndexOf(draggedItem);
                int targetIndex = Tables.IndexOf(targetItem);

                if (draggedIndex == -1 || targetIndex == -1 || draggedIndex == targetIndex)
                    return;

                // Update UI immediately for smooth feedback
                Tables.Move(draggedIndex, targetIndex);

                // Update display orders in database
                var orderUpdates = new List<(Guid Id, int Order)>();
                for (int i = 0; i < Tables.Count; i++)
                {
                    Tables[i].UpdateDisplayOrder(i);
                    orderUpdates.Add((Tables[i].Id, i));
                }

                await _tableService.UpdateDisplayOrdersAsync(orderUpdates);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error reordering tables: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                // Reload on error to restore correct order
                await LoadTablesAsync();
            }
        }
    }

    // Separate ViewModel for table management list items
    public class TableInfoViewModel : ViewModelBase
    {
        private readonly Table _table;
        private readonly bool _hasActiveSession;
        private readonly Session? _activeSession;

        public TableInfoViewModel(Table table, bool hasActiveSession, Session? activeSession)
        {
            _table = table;
            _hasActiveSession = hasActiveSession;
            _activeSession = activeSession;
        }

        public Guid Id => _table.Id;
        public string Name => _table.Name;
        public int DisplayOrder => _table.DisplayOrder;
        public bool IsActive => _table.IsActive;
        public bool HasActiveSession => _hasActiveSession;
        
        public string StatusBadge => HasActiveSession ? "In Use" : "Available";
        public string StatusColor => HasActiveSession ? "#e94560" : "#4caf50";
        public bool ShowSessionInfo => HasActiveSession && _activeSession != null;
        
        public string GameTypeName => _activeSession?.GameType?.Name ?? "";
        public int PlayerCount => _activeSession?.Frames.LastOrDefault()?.Participants.Count ?? 0;
        public int FrameCount => _activeSession?.Frames.Count ?? 0;
        
        public Table GetTable() => _table;
        
        public void UpdateDisplayOrder(int newOrder)
        {
            _table.DisplayOrder = newOrder;
            OnPropertyChanged(nameof(DisplayOrder));
        }
    }
}

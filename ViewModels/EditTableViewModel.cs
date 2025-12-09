using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class EditTableViewModel : ViewModelBase
    {
        private readonly Table? _existingTable;
        private readonly TableService _tableService;
        private string _name = string.Empty;
        private bool _isActive = true;
        private bool _isEditMode;

        public EditTableViewModel(Table? existingTable, TableService tableService)
        {
            _existingTable = existingTable;
            _tableService = tableService;
            _isEditMode = existingTable != null;

            if (existingTable != null)
            {
                _name = existingTable.Name;
                _isActive = existingTable.IsActive;
            }

            SaveCommand = new RelayCommand(async _ => await Save(), _ => CanSave);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string WindowTitle => IsEditMode ? "Edit Table" : "Create New Table";
        public string SaveButtonText => IsEditMode ? "Update" : "Create";

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(Name);

        public ICommand SaveCommand { get; }

        private async Task Save()
        {
            try
            {
                if (IsEditMode && _existingTable != null)
                {
                    // Update existing table - keep its display order
                    await _tableService.UpdateTableAsync(
                        _existingTable.Id,
                        Name.Trim(),
                        _existingTable.DisplayOrder, // Keep existing order
                        IsActive);
                }
                else
                {
                    // Create new table - auto-assign next display order
                    int nextOrder = await _tableService.GetNextDisplayOrderAsync();
                    await _tableService.CreateTableAsync(
                        Name.Trim(),
                        nextOrder);
                }

                // Close dialog with success
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.DataContext == this);
                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving table: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

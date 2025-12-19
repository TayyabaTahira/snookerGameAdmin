using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class GameTypeViewModel : ViewModelBase
    {
        private readonly GameType _gameType;

        public GameTypeViewModel(GameType gameType)
        {
            _gameType = gameType;
        }

        public GameType GameType => _gameType;
        public Guid Id => _gameType.Id;
        public string Name => _gameType.Name;
        public ICollection<GameRule> GameRules => _gameType.GameRules;
        
        public int RuleCount => _gameType.GameRules?.Count ?? 0;
        
        public bool HasNoRules => _gameType.GameRules == null || !_gameType.GameRules.Any();
    }

    public class GameTypeManagementViewModel : ViewModelBase
    {
        private readonly GameTypeService _gameTypeService;
        private ObservableCollection<GameTypeViewModel> _gameTypes = new();

        public GameTypeManagementViewModel(GameTypeService gameTypeService)
        {
            _gameTypeService = gameTypeService;

            AddGameTypeCommand = new RelayCommand(async _ => await AddGameType());
            EditGameTypeCommand = new RelayCommand(async param => await EditGameType(param as GameTypeViewModel));
            DeleteGameTypeCommand = new RelayCommand(async param => await DeleteGameType(param as GameTypeViewModel));

            _ = LoadGameTypes();
        }

        public ObservableCollection<GameTypeViewModel> GameTypes
        {
            get => _gameTypes;
            set => SetProperty(ref _gameTypes, value);
        }

        public ICommand AddGameTypeCommand { get; }
        public ICommand EditGameTypeCommand { get; }
        public ICommand DeleteGameTypeCommand { get; }

        private async Task LoadGameTypes()
        {
            try
            {
                var gameTypes = await _gameTypeService.GetAllGameTypesAsync();
                GameTypes = new ObservableCollection<GameTypeViewModel>(
                    gameTypes.Select(gt => new GameTypeViewModel(gt)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading game types: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task AddGameType()
        {
            try
            {
                var dialogViewModel = new EditGameTypeViewModel(null);
                var dialog = new Views.EditGameTypeDialog(dialogViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true)
                {
                    await LoadGameTypes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding game type: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task EditGameType(GameTypeViewModel? gameTypeViewModel)
        {
            if (gameTypeViewModel == null) return;

            try
            {
                var dialogViewModel = new EditGameTypeViewModel(gameTypeViewModel.GameType);
                var dialog = new Views.EditGameTypeDialog(dialogViewModel)
                {
                    Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                if (dialog.ShowDialog() == true)
                {
                    await LoadGameTypes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error editing game type: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DeleteGameType(GameTypeViewModel? gameTypeViewModel)
        {
            if (gameTypeViewModel == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{gameTypeViewModel.Name}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _gameTypeService.DeleteGameTypeAsync(gameTypeViewModel.Id);
                    await LoadGameTypes();

                    MessageBox.Show(
                        "Game type deleted successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting game type: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

using System.Windows;
using SnookerGameManagementSystem.Models;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class EditGameTypeViewModel : ViewModelBase
    {
        private readonly GameType? _existingGameType;
        private readonly GameTypeService _gameTypeService;
        private readonly GameRuleService _gameRuleService;
        private string _gameTypeName = string.Empty;
        private decimal _baseRate;
        private decimal _overtimeRate;

        public EditGameTypeViewModel(GameType? gameType)
        {
            _existingGameType = gameType;
            _gameTypeService = new GameTypeService(App.GetDbContext());
            _gameRuleService = new GameRuleService(App.GetDbContext());

            if (_existingGameType != null)
            {
                _gameTypeName = _existingGameType.Name;
                
                // Load first rule if exists
                var firstRule = _existingGameType.GameRules?.FirstOrDefault();
                if (firstRule != null)
                {
                    _baseRate = firstRule.BaseRate;
                    _overtimeRate = firstRule.OvertimeRate;
                }
            }
        }

        public string DialogTitle => _existingGameType == null ? "Add Game Type" : "Edit Game Type";
        public string SaveButtonText => _existingGameType == null ? "Create" : "Save";

        public string GameTypeName
        {
            get => _gameTypeName;
            set
            {
                if (SetProperty(ref _gameTypeName, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public decimal BaseRate
        {
            get => _baseRate;
            set
            {
                if (SetProperty(ref _baseRate, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public decimal OvertimeRate
        {
            get => _overtimeRate;
            set
            {
                if (SetProperty(ref _overtimeRate, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool CanSave =>
            !string.IsNullOrWhiteSpace(_gameTypeName) &&
            _baseRate > 0 &&
            _overtimeRate >= 0;

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_existingGameType == null)
                {
                    // Create new game type
                    var gameType = await _gameTypeService.CreateGameTypeAsync(_gameTypeName);

                    // Create default rule
                    await _gameRuleService.CreateGameRuleAsync(
                        gameType.Id,
                        _baseRate,
                        _overtimeRate);
                }
                else
                {
                    // Update existing game type
                    _existingGameType.Name = _gameTypeName;
                    await _gameTypeService.UpdateGameTypeAsync(_existingGameType);

                    // Update first rule or create if doesn't exist
                    var firstRule = _existingGameType.GameRules?.FirstOrDefault();
                    if (firstRule != null)
                    {
                        firstRule.BaseRate = _baseRate;
                        firstRule.OvertimeRate = _overtimeRate;
                        await _gameRuleService.UpdateGameRuleAsync(firstRule);
                    }
                    else
                    {
                        await _gameRuleService.CreateGameRuleAsync(
                            _existingGameType.Id,
                            _baseRate,
                            _overtimeRate);
                    }
                }

                MessageBox.Show(
                    "Game type saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving game type: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}

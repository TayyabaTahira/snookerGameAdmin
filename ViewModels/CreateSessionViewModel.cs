using System.Collections.ObjectModel;
using System.Windows.Input;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.ViewModels
{
    public class CreateSessionViewModel : ViewModelBase
    {
        private string _tableName = string.Empty;
        private GameType? _selectedGameType;
        private ObservableCollection<GameType> _gameTypes = new();
        private ObservableCollection<string> _players = new();
        private string _newPlayerName = string.Empty;

        public CreateSessionViewModel()
        {
            AddPlayerCommand = new RelayCommand(_ => AddPlayer(), _ => CanAddPlayer);
            RemovePlayerCommand = new RelayCommand(param => RemovePlayer(param as string));
        }

        public string TableName
        {
            get => _tableName;
            set
            {
                if (SetProperty(ref _tableName, value))
                {
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public GameType? SelectedGameType
        {
            get => _selectedGameType;
            set
            {
                if (SetProperty(ref _selectedGameType, value))
                {
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public ObservableCollection<GameType> GameTypes
        {
            get => _gameTypes;
            set => SetProperty(ref _gameTypes, value);
        }

        public ObservableCollection<string> Players
        {
            get => _players;
            set
            {
                if (SetProperty(ref _players, value))
                {
                    OnPropertyChanged(nameof(HasNoPlayers));
                }
            }
        }

        public string NewPlayerName
        {
            get => _newPlayerName;
            set
            {
                if (SetProperty(ref _newPlayerName, value))
                {
                    OnPropertyChanged(nameof(CanAddPlayer));
                }
            }
        }

        public bool CanCreate => !string.IsNullOrWhiteSpace(TableName) && SelectedGameType != null;

        public bool CanAddPlayer => !string.IsNullOrWhiteSpace(NewPlayerName);

        public bool HasNoPlayers => Players.Count == 0;

        public ICommand AddPlayerCommand { get; }
        public ICommand RemovePlayerCommand { get; }

        private void AddPlayer()
        {
            if (!string.IsNullOrWhiteSpace(NewPlayerName) && !Players.Contains(NewPlayerName.Trim()))
            {
                Players.Add(NewPlayerName.Trim());
                NewPlayerName = string.Empty;
                OnPropertyChanged(nameof(HasNoPlayers));
            }
        }

        private void RemovePlayer(string? playerName)
        {
            if (playerName != null && Players.Contains(playerName))
            {
                Players.Remove(playerName);
                OnPropertyChanged(nameof(HasNoPlayers));
            }
        }
    }
}

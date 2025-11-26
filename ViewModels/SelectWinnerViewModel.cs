using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.ViewModels
{
    public class SelectWinnerViewModel : ViewModelBase
    {
        private PlayerInfo? _selectedWinner;

        public SelectWinnerViewModel(ObservableCollection<PlayerInfo> players)
        {
            Players = players;
            SelectWinnerCommand = new RelayCommand(param => SelectWinner(param as PlayerInfo));
        }

        public ObservableCollection<PlayerInfo> Players { get; }

        public PlayerInfo? SelectedWinner
        {
            get => _selectedWinner;
            private set => SetProperty(ref _selectedWinner, value);
        }

        public ICommand SelectWinnerCommand { get; }

        private void SelectWinner(PlayerInfo? player)
        {
            if (player != null)
            {
                SelectedWinner = player;
                
                // Close the dialog
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
        }
    }
}

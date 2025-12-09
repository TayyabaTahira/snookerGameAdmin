using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardWindow(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            
            // Debug logging for window lifecycle
            Loaded += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Loaded event fired");
            Activated += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Activated event fired");
            Deactivated += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Deactivated event fired");
            Closing += DashboardWindow_Closing;
            Closed += (s, e) => System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Closed event fired");
            
            System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Constructor completed: {this.GetHashCode()}");
        }

        private void DashboardWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Closing event fired");
            System.Diagnostics.Debug.WriteLine($"[DashboardWindow] IsLoading: {_viewModel.IsLoading}");
            System.Diagnostics.Debug.WriteLine($"[DashboardWindow] StackTrace: {Environment.StackTrace}");

            // Prevent closing while loading
            if (_viewModel.IsLoading)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardWindow] Closing cancelled - loading in progress");
                e.Cancel = true;
                MessageBox.Show(
                    "Please wait while data is loading...",
                    "Loading",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void TableTile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is TableTileViewModel tableTile)
            {
                // If table has active session, view it; otherwise start new session
                if (tableTile.HasActiveSession)
                {
                    _viewModel.ViewSessionCommand.Execute(tableTile);
                }
                else
                {
                    _viewModel.StartSessionCommand.Execute(tableTile);
                }
            }
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Stop the event from bubbling up to the Border's MouseLeftButtonUp
            e.Handled = true;
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SnookerGameManagementSystem.ViewModels;

namespace SnookerGameManagementSystem.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel? _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
            
            viewModel.LoginSuccessful += OnLoginSuccessful;
            
            // Set focus to username
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void OnLoginSuccessful(object? sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LoginWindow] OnLoginSuccessful called!");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Current MainWindow: {Application.Current.MainWindow?.GetType().Name ?? "NULL"}");
                
                // Use Dispatcher to ensure we're on the UI thread
                Dispatcher.Invoke(() =>
                {
                    var dashboardWindow = App.ServiceProvider?.GetService(typeof(DashboardWindow)) as DashboardWindow;
                    if (dashboardWindow != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[LoginWindow] Opening dashboard...");
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] Dashboard window created: {dashboardWindow.GetHashCode()}");
                        
                        // Transfer MainWindow to dashboard BEFORE showing it
                        Application.Current.MainWindow = dashboardWindow;
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] MainWindow transferred to dashboard: {Application.Current.MainWindow?.GetType().Name}");
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] ShutdownMode: {Application.Current.ShutdownMode}");
                        
                        // Now show dashboard
                        dashboardWindow.Show();
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] Dashboard shown. IsVisible: {dashboardWindow.IsVisible}");
                        
                        // Close login window (this will not close the app since MainWindow is now Dashboard)
                        System.Diagnostics.Debug.WriteLine("[LoginWindow] About to close login window...");
                        this.Close();
                        
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] Login window closed successfully");
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] Current MainWindow after close: {Application.Current.MainWindow?.GetType().Name ?? "NULL"}");
                        System.Diagnostics.Debug.WriteLine($"[LoginWindow] Dashboard still visible: {dashboardWindow.IsVisible}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[LoginWindow] ERROR: DashboardWindow is null!");
                        MessageBox.Show("Error: Could not open dashboard window", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] ERROR in OnLoginSuccessful: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error opening dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LoginWindow] ========== LOGIN BUTTON CLICKED ==========");
                
                if (_viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] ERROR: ViewModel is NULL!");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Username: '{_viewModel.Username}'");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Password length: {_viewModel.Password?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] IsLoading: {_viewModel.IsLoading}");
                
                if (_viewModel.LoginCommand.CanExecute(null))
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] Executing login command...");
                    _viewModel.LoginCommand.Execute(null);
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] Command executed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] ERROR: Command cannot execute!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Stack: {ex.StackTrace}");
                MessageBox.Show($"An error occurred during login. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                System.Diagnostics.Debug.WriteLine("[LoginWindow] Enter key pressed");
                LoginButton_Click(sender, e);
            }
        }
    }
}

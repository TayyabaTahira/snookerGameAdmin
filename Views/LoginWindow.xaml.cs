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
                
                var dashboardWindow = App.ServiceProvider?.GetService(typeof(DashboardWindow)) as DashboardWindow;
                if (dashboardWindow != null)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] Opening dashboard...");
                    dashboardWindow.Show();
                    this.Close();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] ERROR: DashboardWindow is null!");
                    MessageBox.Show("Error: Could not open dashboard window", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] ERROR in OnLoginSuccessful: {ex.Message}");
                MessageBox.Show($"Error opening dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LoginWindow] ========== LOGIN BUTTON CLICKED ==========");
                MessageBox.Show("Login button clicked! Check Output window.", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                
                if (_viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] ERROR: ViewModel is NULL!");
                    MessageBox.Show("Error: ViewModel is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Username: '{_viewModel.Username}'");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Password length: {_viewModel.Password?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] IsLoading: {_viewModel.IsLoading}");
                
                if (_viewModel.LoginCommand.CanExecute(null))
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] Executing login command...");
                    MessageBox.Show("Executing login command...", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                    _viewModel.LoginCommand.Execute(null);
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] Command executed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoginWindow] ERROR: Command cannot execute!");
                    MessageBox.Show($"Command cannot execute!\nUsername: {_viewModel.Username}\nPassword length: {_viewModel.Password?.Length ?? 0}", 
                        "Cannot Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoginWindow] Stack: {ex.StackTrace}");
                MessageBox.Show($"Exception during login:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", 
                    "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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

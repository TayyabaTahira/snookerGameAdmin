using System.Windows;
using System.Windows.Input;
using SnookerGameManagementSystem.Services;

namespace SnookerGameManagementSystem.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private RelayCommand? _loginCommand;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            System.Diagnostics.Debug.WriteLine("[LoginViewModel] Constructor called");
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ErrorMessage = string.Empty;
                    LoginCommand.RaiseCanExecuteChanged();
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Username set to: '{value}'");
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ErrorMessage = string.Empty;
                    LoginCommand.RaiseCanExecuteChanged();
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Password set (length: {value?.Length ?? 0})");
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    LoginCommand.RaiseCanExecuteChanged();
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] IsLoading set to: {value}");
                }
            }
        }

        public RelayCommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(
                        async _ => await LoginAsync(),
                        _ => CanLogin()
                    );
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] LoginCommand created");
                }
                return _loginCommand;
            }
        }

        public event EventHandler? LoginSuccessful;

        private bool CanLogin()
        {
            var canLogin = !string.IsNullOrWhiteSpace(Username) && 
                          !string.IsNullOrWhiteSpace(Password) && 
                          !IsLoading;
            
            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] CanLogin: {canLogin} (User: '{Username}', Pass: {Password?.Length ?? 0} chars, Loading: {IsLoading})");
            
            return canLogin;
        }

        private async Task LoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("[LoginViewModel] ========== LoginAsync STARTED ==========");
            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Username: '{Username}', Password length: {Password?.Length ?? 0}");
            
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Validate inputs before authentication
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required";
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Calling AuthService.AuthenticateAsync...");
                MessageBox.Show($"About to authenticate:\nUser: {Username}\nPass length: {Password.Length}", 
                    "Debug - Before Auth", MessageBoxButton.OK, MessageBoxImage.Information);
                
                var user = await _authService.AuthenticateAsync(Username, Password);
                
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] AuthService returned: {(user != null ? "USER OBJECT" : "NULL")}");
                MessageBox.Show($"Auth result: {(user != null ? $"SUCCESS (User: {user.Username})" : "FAILED (null)")}", 
                    "Debug - After Auth", MessageBoxButton.OK, MessageBoxImage.Information);
                
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Login successful! User: {user.Username}, ID: {user.Id}");
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Raising LoginSuccessful event...");
                    
                    MessageBox.Show($"Login successful!\nAbout to raise LoginSuccessful event.\nUser: {user.Username}", 
                        "Debug - Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                    
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] LoginSuccessful event raised. Subscribers: {LoginSuccessful?.GetInvocationList().Length ?? 0}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] Login failed: User is null");
                    ErrorMessage = "Invalid username or password";
                    MessageBox.Show("Authentication failed!\nInvalid username or password", 
                        "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Stack: {ex.StackTrace}");
                ErrorMessage = $"Login failed: {ex.Message}";
                MessageBox.Show($"Exception during authentication:\n{ex.Message}\n\nType: {ex.GetType().Name}", 
                    "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] LoginAsync COMPLETED - setting IsLoading = false");
                IsLoading = false;
                Password = string.Empty;
            }
            
            System.Diagnostics.Debug.WriteLine("[LoginViewModel] ========== LoginAsync ENDED ==========");
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine("[RelayCommand] Execute called");
            _execute(parameter);
            System.Diagnostics.Debug.WriteLine("[RelayCommand] Execute completed");
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

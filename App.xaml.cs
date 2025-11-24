using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Services;
using SnookerGameManagementSystem.ViewModels;
using SnookerGameManagementSystem.Views;

namespace SnookerGameManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        private IConfiguration? _configuration;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                System.Diagnostics.Debug.WriteLine($"[App] Current directory: {Directory.GetCurrentDirectory()}");
                
                // Build configuration
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                
                _configuration = builder.Build();
                System.Diagnostics.Debug.WriteLine("[App] Configuration loaded");

                // Verify connection string exists
                var connectionString = _configuration.GetConnectionString("SnookerDb");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'SnookerDb' not found in appsettings.json");
                }
                System.Diagnostics.Debug.WriteLine($"[App] Connection string: {connectionString?.Split(';')[0]}...");

                // Setup dependency injection
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                ServiceProvider = serviceCollection.BuildServiceProvider();
                System.Diagnostics.Debug.WriteLine("[App] Services configured");

                // Test database connection
                TestDatabaseConnection();

                // Show login window
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] FileNotFoundException: {ex.Message}");
                MessageBox.Show(
                    "Configuration file 'appsettings.json' not found!\n\n" +
                    "Please ensure appsettings.json exists in the application directory.\n\n" +
                    $"Current directory: {Directory.GetCurrentDirectory()}\n\n" +
                    $"File: {ex.FileName}",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
            catch (NullReferenceException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] NullReferenceException: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[App] Stack: {ex.StackTrace}");
                MessageBox.Show(
                    "A null reference error occurred during startup.\n\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"Stack Trace:\n{ex.StackTrace}\n\n" +
                    "This usually means:\n" +
                    "- appsettings.json is missing or invalid\n" +
                    "- Connection string is not configured\n" +
                    "- Database context is not properly initialized",
                    "Null Reference Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[App] Stack: {ex.StackTrace}");
                MessageBox.Show(
                    $"Application startup failed:\n\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"Type: {ex.GetType().Name}\n\n" +
                    $"Current directory: {Directory.GetCurrentDirectory()}\n\n" +
                    $"Stack Trace:\n{ex.StackTrace}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void TestDatabaseConnection()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[App] Testing database connection...");
                
                if (ServiceProvider == null)
                {
                    throw new InvalidOperationException("ServiceProvider is null");
                }
                
                using var scope = ServiceProvider.CreateScope();
                System.Diagnostics.Debug.WriteLine("[App] Scope created");
                
                var context = scope.ServiceProvider.GetRequiredService<SnookerDbContext>();
                System.Diagnostics.Debug.WriteLine("[App] DbContext retrieved");
                
                if (context == null)
                {
                    throw new InvalidOperationException("SnookerDbContext is null");
                }
                
                // Additional null check for DbSet
                if (context.AppUsers == null)
                {
                    throw new InvalidOperationException("AppUsers DbSet is null - DbContext may not be properly initialized");
                }
                
                // Test connection
                System.Diagnostics.Debug.WriteLine("[App] Testing CanConnect...");
                var canConnect = context.Database.CanConnect();
                System.Diagnostics.Debug.WriteLine($"[App] CanConnect result: {canConnect}");
                
                if (!canConnect)
                {
                    throw new Exception("Unable to connect to database. Please check if MySQL is running and the database exists.");
                }
                
                // Check if app_user table exists and has data
                System.Diagnostics.Debug.WriteLine("[App] Counting users...");
                var userCount = context.AppUsers.Count();
                System.Diagnostics.Debug.WriteLine($"[App] Database connection successful! Found {userCount} users.");
                
                if (userCount == 0)
                {
                    MessageBox.Show(
                        "Database connected but no users found!\n\n" +
                        "Please import the database schema:\n" +
                        "mysql -u root -proot snooker_club_db < Database\\SnookerDB_Schema.sql",
                        "Database Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                var connectionString = _configuration?.GetConnectionString("SnookerDb") ?? "Not configured";
                
                System.Diagnostics.Debug.WriteLine($"[App] Database connection failed: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[App] Inner exception: {ex.InnerException?.Message ?? "None"}");
                System.Diagnostics.Debug.WriteLine($"[App] Stack: {ex.StackTrace}");
                
                MessageBox.Show(
                    "❌ Database Connection Failed\n\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"Type: {ex.GetType().Name}\n\n" +
                    (ex.InnerException != null ? $"Inner: {ex.InnerException.Message}\n\n" : "") +
                    "Please ensure:\n" +
                    "1. MySQL is installed and running (sc query MySQL80)\n" +
                    "2. Database 'snooker_club_db' exists\n" +
                    "3. Connection string in appsettings.json is correct\n" +
                    "4. Password is correct\n" +
                    "5. Schema has been imported\n\n" +
                    "Run diagnose_login.bat for detailed diagnostics.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[App] ConfigureServices started");
                
                // Configuration
                if (_configuration == null)
                {
                    throw new InvalidOperationException("Configuration is null");
                }
                services.AddSingleton(_configuration);
                System.Diagnostics.Debug.WriteLine("[App] Configuration added");

                // DbContext
                var connectionString = _configuration.GetConnectionString("SnookerDb");
                System.Diagnostics.Debug.WriteLine($"[App] Connection string retrieved: {!string.IsNullOrEmpty(connectionString)}");
                
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Database connection string 'SnookerDb' not found in appsettings.json");
                }
                
                services.AddDbContext<SnookerDbContext>(options =>
                {
                    System.Diagnostics.Debug.WriteLine("[App] Configuring DbContext...");
                    
                    // Use explicit MySQL version instead of AutoDetect to avoid connection issues during startup
                    var serverVersion = new MySqlServerVersion(new Version(8, 0, 44));
                    
                    options.UseMySql(connectionString, 
                        serverVersion,
                        mySqlOptions => {
                            mySqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(5),
                                errorNumbersToAdd: null);
                        });
                    
                    System.Diagnostics.Debug.WriteLine("[App] DbContext configured");
                });
                System.Diagnostics.Debug.WriteLine("[App] DbContext service added");

                // Services
                services.AddScoped<AuthService>();
                services.AddScoped<SessionService>();
                services.AddScoped<CustomerService>();
                services.AddScoped<GameRuleService>();
                System.Diagnostics.Debug.WriteLine("[App] Business services added");

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<DashboardViewModel>();
                System.Diagnostics.Debug.WriteLine("[App] ViewModels added");

                // Views
                services.AddTransient<LoginWindow>();
                services.AddTransient<DashboardWindow>();
                System.Diagnostics.Debug.WriteLine("[App] Views added");
                
                System.Diagnostics.Debug.WriteLine("[App] ConfigureServices completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] ConfigureServices error: {ex.Message}");
                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }
}

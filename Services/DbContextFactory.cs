using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using System.Diagnostics;

namespace SnookerGameManagementSystem.Services
{
    public class DbContextFactory
    {
        private readonly DatabaseSyncService _syncService;
        private string? _currentConnectionString;

        public DbContextFactory(DatabaseSyncService syncService)
        {
            _syncService = syncService;
        }

        public async Task<SnookerDbContext> CreateDbContextAsync()
        {
            try
            {
                // Get the active connection string
                _currentConnectionString = await _syncService.GetActiveConnectionStringAsync();

                var optionsBuilder = new DbContextOptionsBuilder<SnookerDbContext>();
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 44));

                optionsBuilder.UseMySql(_currentConnectionString, serverVersion, options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });

                var context = new SnookerDbContext(optionsBuilder.Options);

                Debug.WriteLine($"[DbContextFactory] Created context with {_syncService.CurrentMode} connection");
                
                return context;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DbContextFactory] Error creating context: {ex.Message}");
                throw;
            }
        }

        public SnookerDbContext CreateDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SnookerDbContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 44));

            optionsBuilder.UseMySql(connectionString, serverVersion, options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            return new SnookerDbContext(optionsBuilder.Options);
        }
    }
}

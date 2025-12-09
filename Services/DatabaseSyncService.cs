using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using System.Diagnostics;

namespace SnookerGameManagementSystem.Services
{
    public enum DatabaseConnectionMode
    {
        Remote,
        Local,
        Offline
    }

    public class DatabaseSyncService
    {
        private readonly string _remoteConnectionString;
        private readonly string _localConnectionString;
        private DatabaseConnectionMode _currentMode;
        private bool _isSyncing = false;

        public DatabaseConnectionMode CurrentMode => _currentMode;
        public bool IsOnline => _currentMode != DatabaseConnectionMode.Offline;

        public DatabaseSyncService(string remoteConnectionString, string localConnectionString)
        {
            _remoteConnectionString = remoteConnectionString;
            _localConnectionString = localConnectionString;
            _currentMode = DatabaseConnectionMode.Remote;
        }

        /// <summary>
        /// Determines which database connection to use based on connectivity
        /// </summary>
        public async Task<string> GetActiveConnectionStringAsync()
        {
            try
            {
                // First try remote database
                if (await TestConnectionAsync(_remoteConnectionString))
                {
                    Debug.WriteLine("[DatabaseSync] Remote database is accessible");
                    _currentMode = DatabaseConnectionMode.Remote;
                    
                    // If we were offline, sync local changes to remote
                    if (_currentMode == DatabaseConnectionMode.Local && !_isSyncing)
                    {
                        _ = Task.Run(() => SyncLocalToRemoteAsync());
                    }
                    
                    return _remoteConnectionString;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseSync] Remote connection failed: {ex.Message}");
            }

            // Fallback to local database
            try
            {
                if (await TestConnectionAsync(_localConnectionString))
                {
                    Debug.WriteLine("[DatabaseSync] Falling back to local database");
                    _currentMode = DatabaseConnectionMode.Local;
                    return _localConnectionString;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseSync] Local connection failed: {ex.Message}");
            }

            // Both failed
            _currentMode = DatabaseConnectionMode.Offline;
            throw new InvalidOperationException("Cannot connect to either remote or local database");
        }

        /// <summary>
        /// Tests if a database connection is working
        /// </summary>
        private async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<SnookerDbContext>();
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 44));
                
                optionsBuilder.UseMySql(connectionString, serverVersion, options =>
                {
                    options.CommandTimeout(5); // 5 second timeout for testing
                });

                using var context = new SnookerDbContext(optionsBuilder.Options);
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Synchronizes local database changes to remote database
        /// This is a placeholder - you would need to implement proper sync logic
        /// </summary>
        private async Task SyncLocalToRemoteAsync()
        {
            if (_isSyncing)
                return;

            _isSyncing = true;
            try
            {
                Debug.WriteLine("[DatabaseSync] Starting sync from local to remote...");
                
                // TODO: Implement proper synchronization logic
                // This would involve:
                // 1. Track changes in local database (add a sync_status column to tables)
                // 2. Read unsynced records from local database
                // 3. Write them to remote database
                // 4. Mark records as synced
                
                await Task.Delay(100); // Placeholder
                
                Debug.WriteLine("[DatabaseSync] Sync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseSync] Sync failed: {ex.Message}");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        /// <summary>
        /// Manually trigger synchronization
        /// </summary>
        public async Task ManualSyncAsync()
        {
            if (_currentMode == DatabaseConnectionMode.Local)
            {
                await SyncLocalToRemoteAsync();
            }
        }

        /// <summary>
        /// Check connectivity and switch databases if needed
        /// </summary>
        public async Task<bool> CheckAndSwitchConnectionAsync()
        {
            try
            {
                var newConnectionString = await GetActiveConnectionStringAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

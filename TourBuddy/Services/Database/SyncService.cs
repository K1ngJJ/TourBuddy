using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Models;
using System.Diagnostics;

namespace TourBuddy.Services.Database
{
  public   class SyncService
    {
        private readonly ISQLiteService _sqliteService;
        private readonly ISQLServerService _sqlServerService;
        private readonly DatabaseSettings _settings;

        private bool _isSyncing;
        private DateTime _lastSyncTime;

        public event EventHandler<bool> SyncCompleted;

        public SyncService(ISQLiteService sqliteService, ISQLServerService sqlServerService, DatabaseSettings settings)
        {
            _sqliteService = sqliteService;
            _sqlServerService = sqlServerService;
            _settings = settings;
            _lastSyncTime = DateTime.UtcNow.AddDays(-30); // Default to sync last 30 days data
        }

        public async Task StartAutoSyncIfEnabledAsync()
        {
            if (!_settings.AutoSyncEnabled)
                return;

            // Start the auto-sync process in a background task
            await Task.Run(async () =>
            {
                while (_settings.AutoSyncEnabled)
                {
                    try
                    {
                        await SyncDataAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Auto sync error: {ex.Message}");
                    }

                    // Wait for the next sync interval
                    await Task.Delay(TimeSpan.FromMinutes(_settings.SyncIntervalMinutes));
                }
            });
        }

        public async Task SyncDataAsync()
        {
            if (_isSyncing)
                return;

            _isSyncing = true;

            try
            {
                // Check connection to SQL Server
                try
                {
                    await _sqlServerService.InitializeConnectionAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SQL Server connection error: {ex.Message}");
                    SyncCompleted?.Invoke(this, false);
                    return;
                }

                // 1. Get all local users
                var localUsers = await _sqliteService.GetAllAsync<User>();

                // 2. Get all server users modified since last sync
                var serverUsers = await _sqlServerService.GetUsersModifiedSinceAsync(_lastSyncTime);

                // 3. Sync from server to local
                foreach (var serverUser in serverUsers)
                {
                    var localUser = localUsers.FirstOrDefault(u => u.Id == serverUser.Id);

                    if (localUser == null)
                    {
                        // New user on server, add to local
                        await _sqliteService.InsertAsync(serverUser);
                    }
                    else
                    {
                        // User exists locally, update
                        localUser.Username = serverUser.Username;
                        localUser.Email = serverUser.Email;
                        localUser.PasswordHash = serverUser.PasswordHash;
                        localUser.ResetToken = serverUser.ResetToken;
                        localUser.ResetTokenExpiry = serverUser.ResetTokenExpiry;
                        localUser.LastSyncedAt = DateTime.UtcNow;
                        localUser.IsSynced = true;

                        await _sqliteService.UpdateAsync(localUser);
                    }
                }

                // 4. Sync from local to server
                foreach (var localUser in localUsers.Where(u => !u.IsSynced || u.LastSyncedAt == null))
                {
                    var serverUser = await _sqlServerService.GetUserByIdAsync(localUser.Id);

                    if (serverUser == null)
                    {
                        // New user locally, add to server
                        int serverId = await _sqlServerService.InsertUserAsync(localUser);
                        if (serverId > 0)
                        {
                            localUser.Id = serverId;
                            localUser.LastSyncedAt = DateTime.UtcNow;
                            localUser.IsSynced = true;
                            await _sqliteService.UpdateAsync(localUser);
                        }
                    }
                    else
                    {
                        // User exists on server, update
                        await _sqlServerService.UpdateUserAsync(localUser);

                        localUser.LastSyncedAt = DateTime.UtcNow;
                        localUser.IsSynced = true;
                        await _sqliteService.UpdateAsync(localUser);
                    }
                }

                // Update last sync time
                _lastSyncTime = DateTime.UtcNow;

                // Notify that sync is complete
                SyncCompleted?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sync error: {ex.Message}");
                SyncCompleted?.Invoke(this, false);
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
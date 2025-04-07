using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Models;

namespace TourBuddy.Helpers
{
    public static class AppSettings
    {
        private const string DatabaseSettingsKey = "database_settings";
        private const string EmailSettingsKey = "email_settings";

        public static async Task SaveDatabaseSettingsAsync(DatabaseSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            await SecureStorage.SetAsync(DatabaseSettingsKey, json);
        }

        public static async Task<DatabaseSettings> GetDatabaseSettingsAsync()
        {
            var json = await SecureStorage.GetAsync(DatabaseSettingsKey);

            if (string.IsNullOrEmpty(json))
            {
                // Return default settings
                return new DatabaseSettings
                {
                    DatabaseFilename = "authapp.db3",
                    DatabasePath = FileSystem.AppDataDirectory,
                    ServerConnectionString = "Server=yourserver.database.windows.net;Database=YourDB;User Id=YourUser;Password=YourPassword;",
                    AutoSyncEnabled = true,
                    SyncIntervalMinutes = 30
                };
            }

            return JsonConvert.DeserializeObject<DatabaseSettings>(json);
        }

        public static async Task SaveEmailSettingsAsync(EmailSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings);
            await SecureStorage.SetAsync(EmailSettingsKey, json);
        }

        public static async Task<EmailSettings> GetEmailSettingsAsync()
        {
            var json = await SecureStorage.GetAsync(EmailSettingsKey);

            if (string.IsNullOrEmpty(json))
            {
                // Return default settings
                return new EmailSettings
                {
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    SmtpUsername = "your-email@gmail.com",
                    SmtpPassword = "your-app-password",
                    FromEmail = "your-email@gmail.com",
                    FromName = "MauiAuthApp",
                    EnableSsl = true
                };
            }

            return JsonConvert.DeserializeObject<EmailSettings>(json);
        }
    }
}

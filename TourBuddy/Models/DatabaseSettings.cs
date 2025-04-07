using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourBuddy.Models
{
  public class DatabaseSettings
    {
        // SQLite Settings
        public string DatabasePath { get; set; }
        public string DatabaseFilename { get; set; }

        // SQL Server Settings
        public string ServerConnectionString { get; set; }

        // Sync Settings
        public bool AutoSyncEnabled { get; set; }
        public int SyncIntervalMinutes { get; set; }
    }
}
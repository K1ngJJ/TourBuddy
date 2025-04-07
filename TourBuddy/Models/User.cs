using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace TourBuddy.Models
{
   public  class User
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [SQLite.MaxLength(50)]
        public string Username { get; set; }

        [SQLite.MaxLength(100)]
        public string Email { get; set; }

        [SQLite.MaxLength(100)]
        public string PasswordHash { get; set; }

        [SQLite.MaxLength(36)]
        public string ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastSyncedAt { get; set; }

        [Ignore]
        public bool IsSynced { get; set; }

        public User()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
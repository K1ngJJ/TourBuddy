using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Models;

namespace TourBuddy.Services.Database
{
    public interface  ISQLServerService
    {
        Task InitializeConnectionAsync();
        Task<int> InsertUserAsync(User user);
        Task<int> UpdateUserAsync(User user);
        Task<int> DeleteUserAsync(int userId);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersModifiedSinceAsync(DateTime lastSyncTime);
        Task<bool> UserExistsAsync(string email);
    }
}
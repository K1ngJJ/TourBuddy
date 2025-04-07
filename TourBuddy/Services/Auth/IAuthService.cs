using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Models;

namespace TourBuddy.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(string username, string email, string password);
        Task<User> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<User> GetCurrentUserAsync();
        Task LogoutAsync();
    }
}

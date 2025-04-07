using TourBuddy.Helpers;
using TourBuddy.Models;
using TourBuddy.Services.Database;
using TourBuddy.Services.Email;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

namespace TourBuddy.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ISQLiteService _sqliteService;
        private readonly IEmailService _emailService;
        private User _currentUser;

        public AuthService(ISQLiteService sqliteService, IEmailService emailService)
        {
            _sqliteService = sqliteService;
            _emailService = emailService;
        }

        public async Task<bool> RegisterUserAsync(string username, string email, string password)
        {
            try
            {
                // Check if user with the same email already exists
                var existingUser = await _sqliteService.GetAsync<User>(u => u.Email.ToLower() == email.ToLower());

                if (existingUser != null)
                    return false;

                // Create new user
                var newUser = new User
                {
                    Username = username,
                    Email = email.ToLower(),
                    PasswordHash = PasswordHasher.HashPassword(password),
                    CreatedAt = DateTime.UtcNow,
                    IsSynced = false
                };

                // Save to database
                await _sqliteService.InsertAsync(newUser);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                // Find user by email
                var user = await _sqliteService.GetAsync<User>(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                    return null;

                // Verify password
                if (PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    _currentUser = user;
                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                // Find user by ID
                var user = await _sqliteService.GetByIdAsync<User>(userId);

                if (user == null)
                    return false;

                // Verify current password
                if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                    return false;

                // Update with new password
                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                user.IsSynced = false;

                await _sqliteService.UpdateAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Change password error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            try
            {
                // Find user by email
                var user = await _sqliteService.GetAsync<User>(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                    return false;

                // Generate reset token
                string token = Guid.NewGuid().ToString();
                DateTime expiry = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours

                // Save token to database
                user.ResetToken = token;
                user.ResetTokenExpiry = expiry;
                user.IsSynced = false;

                await _sqliteService.UpdateAsync(user);

                // Send reset email
                string resetLink = $"?email={Uri.EscapeDataString(email)}&token={token}";
                string subject = "Reset Password";
                string body = $@"
                    <h2>Reset Password</h2>
                    <p>Hello {user.Username},</p>
                    <p>You have requested to reset your password. Please enter the following code in the app:</p>
                    <p><strong>{token}</strong></p>
                    <p>This code will expire in 24 hours.</p>
                    <p>If you did not request a password reset, please ignore this email.</p>
                ";

                await _emailService.SendEmailAsync(user.Email, subject, body);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Password reset request error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                // Find user by email
                var user = await _sqliteService.GetAsync<User>(u =>
                    u.Email.ToLower() == email.ToLower() &&
                    u.ResetToken == token &&
                    u.ResetTokenExpiry > DateTime.UtcNow);

                if (user == null)
                    return false;

                // Update password and clear reset token
                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                user.ResetToken = null;
                user.ResetTokenExpiry = null;
                user.IsSynced = false;

                await _sqliteService.UpdateAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Password reset error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            // You might want to implement a more robust way to persist the current user
            // such as using SecureStorage to store the user ID between app sessions
            return _currentUser;
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }
    }
}

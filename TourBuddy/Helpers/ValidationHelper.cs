using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TourBuddy.Helpers
{
    class ValidationHelper
    {
        // Email validation regex pattern
        private static readonly Regex _emailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        // Password must be at least 8 characters, include at least one uppercase, one lowercase, one number
        private static readonly Regex _passwordRegex = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            RegexOptions.Compiled);

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return _emailRegex.IsMatch(email);
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return _passwordRegex.IsMatch(password);
        }

        public static bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.Length >= 3 && username.Length <= 50;
        }

        public static (bool IsValid, string Message) ValidateRegisterInput(string username, string email, string password, string confirmPassword)
        {
            if (!IsValidUsername(username))
                return (false, "Username must be between 3 and 50 characters.");

            if (!IsValidEmail(email))
                return (false, "Please enter a valid email address.");

            if (!IsValidPassword(password))
                return (false, "Password must be at least 8 characters, and include at least one uppercase letter, one lowercase letter, and one number.");

            if (password != confirmPassword)
                return (false, "Passwords do not match.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string Message) ValidateLoginInput(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Please enter an email address.");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Please enter a password.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string Message) ValidatePasswordResetRequest(string email)
        {
            if (!IsValidEmail(email))
                return (false, "Please enter a valid email address.");

            return (true, string.Empty);
        }

        public static (bool IsValid, string Message) ValidatePasswordReset(string password, string confirmPassword)
        {
            if (!IsValidPassword(password))
                return (false, "Password must be at least 8 characters, and include at least one uppercase letter, one lowercase letter, and one number.");

            if (password != confirmPassword)
                return (false, "Passwords do not match.");

            return (true, string.Empty);
        }
    }
}

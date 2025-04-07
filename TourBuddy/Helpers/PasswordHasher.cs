using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TourBuddy.Helpers
{
   public class PasswordHasher
    {
        // In a real-world application, you would use a more secure hashing algorithm
        // like BCrypt, Argon2, or PBKDF2. This is a simplified version for demonstration.

        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{salt}:{Iterations}:{key}";
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split(':', 3);

            if (parts.Length != 3)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var iterations = int.Parse(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(KeySize);
                return keyToCheck.SequenceEqual(key);
            }
        }
    }
}
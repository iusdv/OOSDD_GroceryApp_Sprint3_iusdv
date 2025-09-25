using System.Security.Cryptography;
using System.Text;

namespace Grocery.Core.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations: 100_000,
                HashAlgorithmName.SHA256,
                outputLength: 32);

            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }
        
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            try
            {
                var salt = FromB64Safe(parts[0]);
                var hash = FromB64Safe(parts[1]);

                var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations: 100_000,
                    HashAlgorithmName.SHA256,
                    outputLength: 32);

                return CryptographicOperations.FixedTimeEquals(hash, inputHash);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] FromB64Safe(string s)
        {
            s = (s ?? string.Empty).Trim().Replace('-', '+').Replace('_', '/');
            int mod = s.Length % 4;
            if (mod == 2) s += "==";
            else if (mod == 3) s += "=";
            return Convert.FromBase64String(s);
        }
    }
}
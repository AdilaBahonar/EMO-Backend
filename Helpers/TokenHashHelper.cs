using System.Security.Cryptography;
using System.Text;

namespace EMO.Helpers
{
    public static class TokenHashHelper
    {
        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);

            // Convert to readable hex string (64 chars)
            return Convert.ToHexString(hash);
        }
    }
}

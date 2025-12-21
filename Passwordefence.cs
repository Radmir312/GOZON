using System;
using System.Security.Cryptography;
using System.Text;
using System.Data.SQLite;

namespace GOZON
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

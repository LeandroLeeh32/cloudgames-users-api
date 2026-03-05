using System.Security.Cryptography;
using System.Text;
using Users.Application.Interfaces.Repositories;

namespace Users.Infrastructure.Security
{
    public class PasswordHashService : IPasswordHashService
    {
        public string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        public bool Verify(string password, string hash)
            => Hash(password) == hash;
    }
}

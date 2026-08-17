using System.Security.Cryptography;
using System.Text;
using EventOrchestrationService.Application.Interfaces;

namespace EventOrchestrationService.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool VerifyHashedPassword(string hashedPassword, string plainPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(plainPassword))
            return false;

        var newHash = HashPassword(plainPassword);
        return string.Equals(newHash, hashedPassword, StringComparison.Ordinal);
    }
}
using System.Security.Cryptography;
using System.Text;

namespace ToDoApi.Helpers;

public static class HashPassword
{
    public static string Password(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
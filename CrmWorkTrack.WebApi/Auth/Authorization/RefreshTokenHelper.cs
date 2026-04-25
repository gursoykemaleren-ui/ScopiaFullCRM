using System.Security.Cryptography;
using System.Text;

namespace CrmWorkTrack.WebApi.Auth;

public static class RefreshTokenHelpers
{
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static (string hash, string salt) HashToken(string token)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(32);
        var salt = Convert.ToBase64String(saltBytes);

        var hash = Sha256Base64(token + salt);
        return (hash, salt);
    }

    public static bool Verify(string token, string salt, string expectedHash)
        => Sha256Base64(token + salt) == expectedHash;

    private static string Sha256Base64(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
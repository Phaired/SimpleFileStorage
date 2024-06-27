using System.Security.Cryptography;

namespace FileStorage.Helpers;

public static class HashHelper
{
    public static string ComputeHash(Stream inputStream)
    {
        using (var md5 = MD5.Create())
        {
            var hashBytes = md5.ComputeHash(inputStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
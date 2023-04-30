using System.Security.Cryptography;

namespace AIdentities.UI.Features.Core.Services.Authorization;

public static class CryptographyHelper
{
   public static string SecurePassword(string password)
   {
      var salt = new byte[16];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(salt);

      using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
      var hash = deriveBytes.GetBytes(20);
      var inArray = new byte[36];
      Buffer.BlockCopy(salt, 0, inArray, 0, 16);
      Buffer.BlockCopy(hash, 0, inArray, 16, 20);

      return Convert.ToBase64String(inArray);
   }

   public static bool ComparePassword(string password, string storedpassword)
   {
      var buffer = Convert.FromBase64String(storedpassword);
      var salt = new byte[16];
      Buffer.BlockCopy(buffer, 0, salt, 0, 16);
      using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
      var a = deriveBytes.GetBytes(20);
      for (var i = 0; i < 20; i++)
      {
         if (buffer[i + 16] != a[i])
            return false;
      }
      return true;

      //explain the code above:

   }
}

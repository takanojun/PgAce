using System;
using System.Security.Cryptography;
using System.Text;

namespace PgAce.App.Services
{
    public static class EncryptionService
    {
        private const string Prefix = "dpapi:";

        public static string Protect(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return plaintext;
            var data = Encoding.UTF8.GetBytes(plaintext);
            var protectedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(protectedData);
        }

        public static string Unprotect(string cipher)
        {
            if (string.IsNullOrEmpty(cipher)) return cipher;
            if (!cipher.StartsWith(Prefix)) return cipher;
            var base64 = cipher.Substring(Prefix.Length);
            var protectedData = Convert.FromBase64String(base64);
            try
            {
                var data = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch (CryptographicException)
            {
                return string.Empty; // failed to decrypt
            }
        }
    }
}

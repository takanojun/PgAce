using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PgAce.App.Models;

namespace PgAce.App.Services
{
    public class ConfigService
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public AppConfig Load()
        {
            if (!File.Exists(_path))
            {
                return new AppConfig();
            }

            var json = File.ReadAllText(_path);
            var config = JsonConvert.DeserializeObject<AppConfig>(json);
            return config ?? new AppConfig();
        }

        public void Save(AppConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_path, json);
        }

        public string Protect(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return string.Empty;
            var data = Encoding.UTF8.GetBytes(plain);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return "dpapi:" + Convert.ToBase64String(encrypted);
        }

        public string Unprotect(string cipher)
        {
            if (string.IsNullOrEmpty(cipher) || !cipher.StartsWith("dpapi:")) return string.Empty;
            var data = Convert.FromBase64String(cipher.Substring(6));
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}

using System.Collections.Generic;

namespace PgAce.App.Models
{
    public class ConnectionProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AppSettings
    {
        public bool SavePassword { get; set; } = true;
        public int MaxHistory { get; set; } = 500;
    }

    public class ConfigRoot
    {
        public List<ConnectionProfile> Profiles { get; set; } = new List<ConnectionProfile>();
        public AppSettings Settings { get; set; } = new AppSettings();
    }
}

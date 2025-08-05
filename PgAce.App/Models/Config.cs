using System.Collections.Generic;

namespace PgAce.App.Models
{
    public class AppConfig
    {
        public List<ConnectionProfile> Profiles { get; set; } = new List<ConnectionProfile>();
        public Settings Settings { get; set; } = new Settings();
    }

    public class ConnectionProfile
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; } = 5432;
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Settings
    {
        public bool SavePassword { get; set; } = true;
        public int MaxHistory { get; set; } = 500;
    }
}

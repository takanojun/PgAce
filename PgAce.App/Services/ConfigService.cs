using System.IO;
using Newtonsoft.Json;
using PgAce.App.Models;

namespace PgAce.App.Services
{
    public class ConfigService
    {
        private readonly string _path;

        public ConfigService(string path)
        {
            _path = path;
        }

        public ConfigRoot Load()
        {
            if (!File.Exists(_path))
            {
                return new ConfigRoot();
            }

            var json = File.ReadAllText(_path);
            return JsonConvert.DeserializeObject<ConfigRoot>(json) ?? new ConfigRoot();
        }

        public void Save(ConfigRoot config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_path, json);
        }
    }
}

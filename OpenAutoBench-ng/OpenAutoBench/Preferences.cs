using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel;
using Newtonsoft.Json;

namespace OpenAutoBench_ng.OpenAutoBench
{
    public class Preferences
    {
        private readonly string _filePath;
        private readonly string _vendorPath = "OpenAutoBench";
        private readonly string _appData;

        private string FileName = "settings.json";

        public Preferences()
        {
            _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = GetLocalFilePath(FileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            
            
            return Path.Combine(_appData, _vendorPath, fileName);
        }

        private void CheckCreateDirectory()
        {
            if (!Directory.Exists(Path.Combine(_appData, _vendorPath)))
            {
                Directory.CreateDirectory(Path.Combine(_appData, _vendorPath));
            }
        }

        public Settings Load()
        {
            Settings settings;
            CheckCreateDirectory();
            
            if (File.Exists(_filePath))
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_filePath));
            }
            else
            {
                //TODO: log no file
                settings = new Settings();
            }
            return settings;
        }

        public void Save(Settings settings)
        {
            CheckCreateDirectory();
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(_filePath, json);
        }
    }
}

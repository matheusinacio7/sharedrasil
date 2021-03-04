using System;
using System.IO;
using Newtonsoft.Json;

namespace sharedrasil {
    public class Settings : ConfigFile {
        public override string PATH {
            get {
                return $"{Globals.CONFIG_PATH}/settings.json";
            }
        }

        public static void Initialize() {
            if(File.Exists(Globals.settings.PATH)) {
                Load();
                return;
            }

            Settings initialSettings = new Settings();
            initialSettings.Version = Globals.CURRENT_VERSION;
            Globals.settings = initialSettings;
            Globals.settings.Save();
        }

        public static void Load() {
            string jsonString = File.ReadAllText(Globals.settings.PATH);
            Settings loadedSettings = JsonConvert.DeserializeObject<Settings>(jsonString);

            Globals.settings = loadedSettings;
        }
        
        public Version Version {get; set;}
    }
}
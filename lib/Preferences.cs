using System;
using System.IO;
using Newtonsoft.Json;

namespace sharedrasil {
    public class Preferences : ConfigFile {    
        public override string PATH {get {
            return Path.Combine(Globals.CONFIG_PATH, "preferences.json");
        } }
        public int BackupInterval {get; set;}
        public int MinimumPlayTime {get; set;}
        public Sharebranch SignedInBranch {get; set;}
        public string SteamExePath {get; set;}

        public static void Initialize() {
            if(File.Exists(Globals.preferences.PATH)) {
                Load();
                return;
            }
            Preferences initialPreferences = new Preferences();
            
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string[] paths = {
                programFilesPath,
                "Steam",
                "steam.exe",
            };

            string steamExePath = Path.Combine(paths);
            
            initialPreferences.SteamExePath = steamExePath;
            initialPreferences.BackupInterval = 1200000; // 20 minutes
            initialPreferences.MinimumPlayTime = 600000; // 10 minutes
            initialPreferences.Save();
        }

        public static void Load() {
            string jsonString = File.ReadAllText(Globals.preferences.PATH);
            Preferences preferences = JsonConvert.DeserializeObject<Preferences>(jsonString);

            Globals.preferences = preferences;
        }
    }
}
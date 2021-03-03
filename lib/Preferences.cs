using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace sharedrasil {
    public class Preferences {
        public static string PATH = Path.Combine(Globals.CONFIG_PATH, "preferences.json");
        public Sharebranch SignedInBranch {get; set;}
        public string SteamExePath {get; set;}

        public static void Initialize() {
            if(File.Exists(PATH)) {
                return;
            }
            Preferences initialPreferences = new Preferences();
            
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string[] paths = {
                programFilesPath,
                "Steam",
                "steam.exe"
            };

            string steamExePath = Path.Combine(paths);
            
            initialPreferences.SteamExePath = steamExePath;
            initialPreferences.Save();
        }

        public static void Load() {
            string jsonString = File.ReadAllText(PATH);
            Preferences preferences = JsonConvert.DeserializeObject<Preferences>(jsonString);

            Globals.preferences = preferences;
        }

        public void Save() {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);

            using(StreamWriter sw = new StreamWriter(PATH)) {
                sw.Write(jsonString);
                sw.Close();
            }
        }
    }
}
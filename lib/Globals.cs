using System;
using System.IO;

namespace sharedrasil {

    public static class Globals {
        public static Version CURRENT_VERSION = new Version("0.1.2.9");
        public static string MYDOCS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string LOCALREPO_PATH = Directory.GetParent(
                                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                                                ).ToString()   
                                                + "\\LocalLow\\IronGate\\Valheim";
        public static string CONFIG_PATH = Path.Combine(MYDOCS_PATH, "Sharedrasil");
        public static Boolean firstMenu = true;
        public static User currentUser = new User();
        public static Sharebranch currentBranch = new Sharebranch();
        public static Preferences preferences = new Preferences();
        public static Settings settings = new Settings();

    }
}
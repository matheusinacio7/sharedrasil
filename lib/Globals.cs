using System;
using System.IO;

namespace sharedrasil {
    public static class Globals {
        public static string MYDOCS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string LOCALREPO_PATH = Path.Combine(MYDOCS_PATH, "Sharedrasil");

        public static User currentUser = new User();
    }
}
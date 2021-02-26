using System;
using System.IO;

namespace sharedrasil {
    public static class DotEnv {
        public static void Load(string filePath) {
            if(!File.Exists(filePath))
                return;

            foreach(var line in File.ReadAllLines(filePath)) {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries
                );

                if(parts.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }

        public static void LoadRoot() {
            string root = Directory.GetCurrentDirectory();
            string dotEnvPath = Path.Combine(root, ".env");
            Load(dotEnvPath);
        }
    }
}
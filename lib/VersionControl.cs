using System;
using System.Collections.Generic;

namespace sharedrasil {
    public static class VersionControl {
        public static void Initialize() {
            HashSet<ConfigFile> changedConfigs = GetConfigsWithMissingValuesAndUpdateGlobals();
            InitializeMissingValues(changedConfigs);
            UpdateVersion();
        }
 
        static HashSet<ConfigFile> GetConfigsWithMissingValuesAndUpdateGlobals() {
            Version userVersion = Globals.settings.Version;
            HashSet<ConfigFile> changedClasses = new HashSet<ConfigFile>();

            // Version 0.1.2.1 from 2021.03.04 introduced the "Minimum play time" preference
            if(userVersion.Major < 1 && userVersion.Minor < 2 && userVersion.Build < 3 && userVersion.Revision < 1) {
                Globals.preferences.MinimumPlayTime = 60000; // 10 minutes
                changedClasses.Add(Globals.preferences);
            }

            return changedClasses;
        }

        static void InitializeMissingValues(HashSet<ConfigFile> configs) {
            foreach(ConfigFile config in configs) {
                config.Save();
            }
        }

        static void UpdateVersion() {
            Globals.settings.Version = Globals.CURRENT_VERSION;
            Globals.settings.Save();
        }
    }
}
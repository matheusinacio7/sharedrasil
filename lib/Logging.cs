using System;
using System.IO;

namespace sharedrasil {
    public static class Logging {
        public static void WriteFromStream(StreamReader sr) {
            StreamWriter sw = new StreamWriter($"{Globals.CONFIG_PATH}/logs.md", append: true);
            sw.WriteLine(DateTime.Now);

            string line = sr.ReadLine();
            while(line != null) {
                sw.WriteLine(line);
                line = sr.ReadLine();
            }

            sw.Close();
        }
    }
}
using System.IO;
using Newtonsoft.Json;

namespace sharedrasil {
    public abstract class ConfigFile  {
        public abstract string PATH {get; }

        public void Save() {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);

            using(StreamWriter sw = new StreamWriter(PATH)) {
                sw.Write(jsonString);
                sw.Close();
            }
        }
    }
}
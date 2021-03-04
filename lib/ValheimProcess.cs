using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace sharedrasil {
    public class ValheimProcess {
        public static async Task<Process> Get() {
            DateTime startTime = DateTime.Now;

            do {
                await Task.Delay(10000);

                Process[] processes = Process.GetProcessesByName("valheim");

                TimeSpan tryingTime = DateTime.Now - startTime;

                if(processes.Length > 0) {
                    return processes[0];
                } else if(tryingTime.TotalMinutes > 15) {
                    return null;
                }
            } while (true);
        }                
    }
}
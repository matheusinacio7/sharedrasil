using System.IO;
using System.Diagnostics;

namespace sharedrasil {
    public static class ShellWorker {
        public static void RunCommands(string[] commands) {
            Process p = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            p.StartInfo = startInfo;
            p.Start();

            StreamWriter sw = p.StandardInput;
            StreamReader sr = p.StandardOutput;

            foreach(string command in commands) {
                sw.WriteLine(command);
            }

            // Logging.WriteFromStream(sr);

            sw.Close();
            p.WaitForExit();
        }
    }
}
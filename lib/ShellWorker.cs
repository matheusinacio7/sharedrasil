using System.IO;
using System.Diagnostics;

namespace sharedrasil {
    public class ShellWorker {
        public void RunCommands(string[] commands) {
            Process p = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            p.StartInfo = startInfo;
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

            StreamWriter sw = p.StandardInput;

            foreach(string command in commands) {
                sw.WriteLine(command);
            }

            sw.Close();
            p.WaitForExit();
        }
    }
}
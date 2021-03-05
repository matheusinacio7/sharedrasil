using System;
using System.IO;
using System.Threading;

namespace sharedrasil
{

    public class LocalRepo
    {
        public Boolean Exists
        {
            get
            {
                if (Directory.Exists($"{Globals.LOCALREPO_PATH}/.git"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        

        public static void Backup() {
            Console.WriteLine("\nDoing local backup.");
            string[] commands = {
                $"cd \"{Globals.LOCALREPO_PATH}\"",
                "git add .",
                $"git commit -m \"Local backup\"",
            };

            ShellWorker shellWorker = new ShellWorker();
            shellWorker.RunCommands(commands);
            Console.WriteLine("\nFinished local backup");
        }

        public void Create()
        {
            if(Exists) {
                Console.WriteLine("You already have the local repository.");
                bool wantsToDeleteTheRepository = CLIParser.AskYesOrNo("Do you want to remove it? This will destroy all backups, but not your current files.");
                
                if(!wantsToDeleteTheRepository) {
                    return;
                } else {
                    Console.WriteLine("For now, it's not possible to delete the repository through the command line.");
                    return;
                }

            }

            Console.WriteLine($"\nCreating a new Git repository at {Globals.LOCALREPO_PATH}");
            string[] commands = {
                $"cd \"{Globals.LOCALREPO_PATH}\"",
                $"git config user.name \"{Globals.currentUser.Username}\"",
                $"git config user.email \"{Globals.currentUser.Email}\"",
                "git init -b remote",
                "git checkout -b local",
                "git add .",
                "git commit -m \"First backup\"",
            };

            ShellWorker shellWorker = new ShellWorker();
            shellWorker.RunCommands(commands);

            if(!Exists) {
                Console.WriteLine($"Could not create a Git repository at \"{Globals.LOCALREPO_PATH}\". Please, check if: \n- You have Git installed;\n- The app has permission to access your command prompt, git, and the folder");
                return;
            }

            using(StreamReader sr = new StreamReader("./assets/README.md"))
            using(StreamWriter sw = new StreamWriter(Path.Combine(Globals.LOCALREPO_PATH, "README.md"))) {
                string line = sr.ReadLine();

                while(line != null) {
                    sw.WriteLine(line);
                    line = sr.ReadLine();
                }

                sw.Close();
            }
            
            Console.WriteLine("Successfully created repository.");
        }

        public static void DoRegularBackups(CancellationToken cancellationToken) {
            do {
                Thread.Sleep(Globals.preferences.BackupInterval);
                if(cancellationToken.IsCancellationRequested) {
                    return;
                }
                Backup();
            } while(true);
        }

    }
}
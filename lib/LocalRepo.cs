using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            string[] commands = {
                $"cd \"{Globals.LOCALREPO_PATH}\"",
                "git add .",
                $"git commit -m \"Local backup\"",
            };

            ShellWorker.RunCommands(commands);
            Console.WriteLine("Finished local backup");
        }

        public async Task Create()
        {
            if(Exists) {
                Console.WriteLine("You already have the local repositories.");
                bool wantsToDeleteTheRepository = CLIParser.AskYesOrNo("Do you want to remove them? This will destroy all backups, but not your current files.");
                
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
                "git init",
                $"git config user.name \"{Globals.currentUser.Username}\"",
                $"git config user.email \"{Globals.currentUser.Email}\"",
            };

            ShellWorker.RunCommands(commands);

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
            User user = new User();
            await user.Create();
        }

        public static void Commit() {
            
        }

    }
}
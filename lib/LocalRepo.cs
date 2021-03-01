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
        
        public async Task AddUser() {
            string userPath = Path.Combine(Globals.CONFIG_PATH, "user.json");

            if(File.Exists(userPath)) {
                Console.WriteLine("There is already an user for sharedrasil in your local machine.");
                Boolean wantsToContinue = CLIParser.AskYesOrNo("Would you like to delete it and create another one?");

                if(!wantsToContinue) {
                    return;
                }

                File.Delete(userPath);
            }

            Console.WriteLine("\nPlease, enter your github username and email.");
            string username = CLIParser.AskAnyString("Username:");
            string email = CLIParser.AskAnyString("Email:");

            AccessToken token = await Github.Authenticate();

            User user = new User {
                Username = username,
                Email = email,
                AccessToken = token,
            };

            string json = JsonConvert.SerializeObject(user, Formatting.Indented);

            using(StreamWriter sw = new StreamWriter(userPath)) {
                sw.Write(json);
            }

            if(File.Exists(userPath)) {
               Console.WriteLine($"Successfully created user. You can change your user by editing {userPath}.");
               Console.WriteLine("You can also run 'add user' command at any time to go through this proccess again.");
            }
        }

        public async Task Create()
        {
            if(Exists) {
                Console.WriteLine("You already have the local repositories.");
                return;
            }

            Console.WriteLine($"\nCreating a new Git repository at {Globals.LOCALREPO_PATH}");
            string[] commands = {
                $"cd {Globals.LOCALREPO_PATH}",
                "git init"
            };

            ShellWorker.RunCommands(commands);

            if(!Exists) {
                Console.WriteLine($"Could not create a Git repository at {Globals.LOCALREPO_PATH}. Please, check your permissions.");
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
            await AddUser();
        }

        public static void Commit() {
            
        }

    }
}
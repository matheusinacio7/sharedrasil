using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LibGit2Sharp;

namespace sharedrasil
{

    public class LocalRepo
    {
        public Boolean Exists
        {
            get
            {
                if (Repository.IsValid(Globals.LOCALREPO_PATH))
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
            string userPath = Path.Combine(Globals.LOCALREPO_PATH, "user.json");

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

            Token token = await Github.Authenticate();

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
                Console.WriteLine("You already have a local repository.");
                return;
            }

            Console.WriteLine($"\nCreating a new Git repository at {Globals.LOCALREPO_PATH}");
            Repository.Init(Globals.LOCALREPO_PATH);

            if(!Exists) {
                Console.WriteLine($"Could not create a Git repository at {Globals.LOCALREPO_PATH}. Please, check your permissions.");
            }
            
            Console.WriteLine("Successfully created repository.");
            await AddUser();
        }

    }
}
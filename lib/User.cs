using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace sharedrasil {
    public class AccessToken {
        public string Type {get; set;}
        public string Token {get; set;}
    }

    public class User : ConfigFile {
        public override string PATH {
            get {
                return Path.Combine(Globals.CONFIG_PATH, "user.json");
            }
        }

        public string Username {get; set;}
        public string Email {get; set;}
        public AccessToken AccessToken {get;set;}
        public async Task Authenticate(Boolean save = true) {
            AccessToken token = await Github.Authenticate();
            this.AccessToken = token;

            if(save) {
                this.Save();
            }
        }

        public async Task<Boolean> Create() {
            if(File.Exists(Globals.currentUser.PATH)) {
                Console.WriteLine("There is already an user for sharedrasil in your local machine.");
                Boolean wantsToContinue = CLIParser.AskYesOrNo("Would you like to delete it and create another one?");

                if(!wantsToContinue) {
                    return false;
                }

                File.Delete(Globals.currentUser.PATH);
            }

            Console.WriteLine("\nPlease, enter your github username and email.");
            this.Username = CLIParser.AskAnyString("Username:");
            this.Email = CLIParser.AskAnyString("Email:");
            
            await this.Authenticate(false);

            this.Save();

            if(File.Exists(Globals.currentUser.PATH)) {
               Console.WriteLine($"\nSuccessfully created user. You can change your user by editing {Globals.currentUser.PATH}.");
               Console.WriteLine("You can also run 'add user' command at any time to go through this proccess again.");
                return true;
            }

            return false;
        }

        public async Task GetCredentials() {
            if(!File.Exists(Globals.currentUser.PATH)) {
                Console.WriteLine("You don't have a user created.");
                Boolean wantsToCreate = CLIParser.AskYesOrNo("Would you like to create one now?");

                if(!wantsToCreate) {
                    Console.WriteLine("\nUnfortunately, Sharedrasil needs a local user in order to comunicate with Github");
                    Console.ReadKey();
                    Environment.Exit(-1);
                    return;
                } else {
                    await this.Create();
                }
            }
            
            string jsonString = File.ReadAllText(Globals.currentUser.PATH);
            User user = JsonConvert.DeserializeObject<User>(jsonString);

            this.Username = user.Username;
            this.Email = user.Email;
            this.AccessToken = user.AccessToken;
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace sharedrasil {
    public class AccessToken {
        public string Type {get; set;}
        public string Token {get; set;}
    }

    public class User {
        public static string FilePath = Path.Combine(Globals.CONFIG_PATH, "user.json");

        public string Username {get;set;}
        public string Email {get;set;}
        public AccessToken AccessToken {get;set;}
        public Sharebranch SignedInBranch {get; set;}

        public async Task Create() {

            if(File.Exists(User.FilePath)) {
                Console.WriteLine("There is already an user for sharedrasil in your local machine.");
                Boolean wantsToContinue = CLIParser.AskYesOrNo("Would you like to delete it and create another one?");

                if(!wantsToContinue) {
                    return;
                }

                File.Delete(User.FilePath);
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

            using(StreamWriter sw = new StreamWriter(User.FilePath)) {
                sw.Write(json);
                sw.Close();
            }

            if(File.Exists(User.FilePath)) {
               Console.WriteLine($"Successfully created user. You can change your user by editing {User.FilePath}.");
               Console.WriteLine("You can also run 'add user' command at any time to go through this proccess again.");
            }
        }

        public async Task GetCredentials() {
            if(!File.Exists(User.FilePath)) {
                Console.WriteLine("You don't have a user created.");
                Boolean wantsToCreate = CLIParser.AskYesOrNo("Would you like to create one now?");

                if(!wantsToCreate) {
                    Console.WriteLine("Unfortunately, Sharedrasil needs a local user in order to comunicate with Github");
                    return;
                } else {
                    await this.Create();
                }
            }
            
            string jsonString = File.ReadAllText(User.FilePath);
            User user = JsonConvert.DeserializeObject<User>(jsonString);

            this.Username = user.Username;
            this.Email = user.Email;
            this.AccessToken = user.AccessToken;
        }
    }
}
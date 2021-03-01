using System;
using System.Threading.Tasks;

namespace sharedrasil {
    public static class CLIInterface {

        public static async Task MainMenuLoop() {
            do {
                PrintLogo();
                await CheckForDeps();
                PrintMenu();
                
                CLICommand command = CLIParser.WaitForComand();

                await ValidateCommand(command);

                Console.WriteLine("\nPress any key to go back to the main menu.");
                Console.ReadKey();
            } while(true);
        }

        static async Task ValidateCommand(CLICommand command) {
            switch(command.Command)
            {
                case "create":
                    await CreateCommand(command.Arguments);
                    break;
                case "push":
                    PushCommand(command.Arguments);
                    break;
                case "signIn":
                    await SignInCommand(command.Arguments);
                    break;
                case "addBuddy":
                    await AddBuddyCommand(command.Arguments);
                    break;
                case "checkBuddy":
                    await CheckBuddyCommand(command.Arguments);
                    break;
                default:
                    Console.WriteLine("\nSharedrasil doesn't recognize the command you typed.");
                    break;
            }
        }

        static async Task AddBuddyCommand(string[] args) {
            string user;

            if(args.Length > 0) {
                user = args[0];
            } else {
                user = CLIParser.AskAnyString("What's the GITHUB username of your buddy?");
            }

            string message = await Github.AddSharebuddy(user);
            Console.WriteLine("\n" + message);
        }

        static async Task CheckBuddyCommand(string[] args) {
            string user = CLIParser.AskAnyString("What's your buddy GITHUB username?");
            Boolean isBuddy = await Github.CheckIfUserIsSharebuddy(user);

            if(isBuddy) {
                Console.WriteLine("That user is already your Sharebuddy!");
            } else {
                Console.WriteLine("This user is not your Sharebuddy yet ):");
                Boolean wantsToAdd = CLIParser.AskYesOrNo("Would you like to add said user as your buddy?");

                if(wantsToAdd) {
                    string[] addArgs = { user };
                    await AddBuddyCommand(addArgs);
                }
            }
        }

        static async Task CreateCommand(string[] args) {
            await Github.CreateRepository();
        }

        static void PushCommand(string[] args) {
            Github.Push();
        }

        static async Task SignInCommand(string[] args) {
            string creator;

            if(args.Length > 0) {
                creator = args[0];
            } else {
                creator = CLIParser.AskAnyString("What's the GITHUB username of the creator of the branch?");
            }

            Sharebranch signedInBranch = await Sharebranch.SignIn(creator);

            if(signedInBranch is null) {
                Console.WriteLine("\nCould not sign in. Maybe the branch doesn't exist, or maybe you don't have permission to view it.");
            } else {
                Console.WriteLine($"\nSuccessfully signed in to {signedInBranch.Creator}'s sharebranch.");
            }
        }

        // UI printing methods down below
        
        static async Task CheckForDeps() {
            User user = new User();
            await user.GetCredentials();
            Globals.currentUser = user;

            LocalRepo localRepo = new LocalRepo();

            if(!localRepo.Exists) {
                Console.WriteLine("You don't have the local repositories. Would you like to create one? (Y/N)");
                string createRepo_Q = Console.ReadLine().ToLower();

                if(createRepo_Q == "y" || createRepo_Q == "yes") {
                    await localRepo.Create();
                } else {
                    Console.WriteLine("Unfortunately, Sharedrasil needs local repositories to work with.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                    Environment.Exit(-1);
                }
            }
        }

        static void PrintLogo() {
            Console.WriteLine(@"
            ███████ ██   ██  █████  ██████  ███████ ██████  ██████   █████  ███████ ██ ██      
            ██      ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██ ██      ██ ██      
            ███████ ███████ ███████ ██████  █████   ██   ██ ██████  ███████ ███████ ██ ██      
                 ██ ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██      ██ ██ ██      
            ███████ ██   ██ ██   ██ ██   ██ ███████ ██████  ██   ██ ██   ██ ███████ ██ ███████

                                                                                 version 0.1.0 
            ");
        }

        static void PrintMenu() {
            if(Globals.currentUser.SignedInBranch is null)
            {
                Console.WriteLine("\nYou are not currently signed to any sharebranch.");
            }
            else
            {
                Console.WriteLine($"\nSigned to {Globals.currentUser.SignedInBranch.Creator}'s branch.");
            }

            Console.WriteLine(@"
You can use the following commands:

---- Sharebranch related ----
    -> create
    -> push
    -> signIn

---- Sharebuddy related ----
    -> addBuddy
    -> checkBuddy

You can type help after any command to get info about it. For example:
    -> create help
                ");
        }
    }

}
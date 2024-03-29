using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace sharedrasil {
    public static class CLIInterface {
        public static async Task MainMenuLoop() {
            do {
                PrintLogo();
                await CheckForDeps();
                PrintMenu();
                if(Globals.firstMenu)
                    Globals.firstMenu = false;
                
                CLICommand command = CLIParser.WaitForComand();

                await ValidateCommand(command);

                Console.WriteLine("\nPress any key to go back to the main menu.");
                Console.ReadKey();
            } while(true);
        }

        static async Task ValidateCommand(CLICommand command) {
            switch(command.Command.ToLower())
            {
                case "run":
                    await RunCommand(command.Arguments);
                    break;
                case "create":
                    await CreateCommand(command.Arguments);
                    break;
                case "pull":
                    PullCommand(command.Arguments);
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
                case "addUser":
                    await AddUserCommand(command.Arguments);
                    break;
                case "authenticate":
                    await AuthenticateUserCommand(command.Arguments);
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

            string message = await Github.AddCollaborator(user);
            Console.WriteLine("\n" + message);
        }

        static async Task AddUserCommand(string[] args) {
            User user = new User();
            Boolean created = await user.Create();
            if(created) {
                Globals.currentUser = user;
                Sharebranch.SetCurrent();
            }
        }

        static async Task AuthenticateUserCommand(string[] args) {
            await Globals.currentUser.Authenticate();
            Console.WriteLine("\nYou have been successfully authenticated");
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

        static void PullCommand(string[] args) {
            Github.Pull();
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

        static async Task RunCommand(string[] args) {
            Console.WriteLine("\nPulling changes from the Sharebranch");
            Github.Pull();
            Console.WriteLine("\nTrying to start Valheim.");
            
            string steamPath = Globals.preferences.SteamExePath;

            if(!File.Exists(steamPath)) {
                Console.WriteLine("\nSharedrasil could not find steam executable at the path");
                Console.WriteLine(steamPath);
                Console.WriteLine("If you have steam installed in another directory, please update your preferences file");
                return;
            }

            Process steamGameProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = steamPath;
            startInfo.Arguments = "steam://rungameid/892970";
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            steamGameProcess.StartInfo = startInfo;

            DateTime startTime = DateTime.Now;
            steamGameProcess.Start();

            Process valheimProcess = await ValheimProcess.Get();

            if(valheimProcess is null) {
                Console.WriteLine("\nSharedrasil could not start Valheim. Make sure you have the correct steam path.");
                Console.ReadKey();
                return;
            }

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Thread backupThread = new Thread(() => LocalRepo.DoRegularBackups(token));
            backupThread.Start();

            Console.WriteLine("\nThe game is running..");

            valheimProcess.WaitForExit();
            
            source.Cancel();

            DateTime exitTime = DateTime.Now;
            TimeSpan totalGameTime = exitTime - startTime;
            if(totalGameTime.TotalMilliseconds < Globals.preferences.MinimumPlayTime) {
                Console.WriteLine($"\nYou have played for less than {Globals.preferences.MinimumPlayTime / 60000} minutes.");
                Console.WriteLine("The changes will not be pushed to the sharebranch.");
                return;
            }

            Console.WriteLine("\nThe game has stopped running.");
            Console.WriteLine("\nPushing changes to the sharebranch");
            Github.Push();        
        }

        // UI printing methods down below
        
        static async Task CheckForDeps() {
            User user = new User();
            await user.GetCredentials();
            Globals.currentUser = user;

            Sharebranch.SetCurrent();
            Preferences.Initialize();
            Settings.Initialize();
            VersionControl.Initialize();

            LocalRepo localRepo = new LocalRepo();

            if(!localRepo.Exists) {
                Console.WriteLine("\nYou don't have the local repositories. Would you like to create one? (Y/N)");
                string createRepo_Q = Console.ReadLine().ToLower();

                if(createRepo_Q == "y" || createRepo_Q == "yes") {
                    localRepo.Create();
                    Console.WriteLine("\nPress any key to go back to the main menu.");
                    Console.ReadKey();
                } else {
                    Console.WriteLine("Unfortunately, Sharedrasil needs local repositories to work with.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                    Environment.Exit(-1);
                }
            }
        
            if(Globals.firstMenu)
                CheckConnectionAndToken();
        }

        public static Boolean CheckConnectionAndToken() {
            if(CheckConnection() && CheckToken()) {
                return true;
            } else {
                return false;
            }
        }

        public static Boolean CheckConnection() {
            if(Connection.Check()) {
                return true;
            } else {
                Console.WriteLine("\nIt seems you are not connected to the internet.");
                Console.WriteLine("You will not be able to perform any online operation, such as pushing the sharebranch or pulling it.");
                return false;
            }
        }

        public static Boolean CheckToken() {
            if(Globals.currentUser.AccessToken is null) {
                Console.WriteLine("\nYou don't have a token. Have you authenticated?");
                return false;
            } else {
                return true;
            }
        }

        static void PrintLogo() {
            Console.WriteLine($@"
            ███████ ██   ██  █████  ██████  ███████ ██████  ██████   █████  ███████ ██ ██      
            ██      ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██ ██      ██ ██      
            ███████ ███████ ███████ ██████  █████   ██   ██ ██████  ███████ ███████ ██ ██      
                 ██ ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██      ██ ██ ██      
            ███████ ██   ██ ██   ██ ██   ██ ███████ ██████  ██   ██ ██   ██ ███████ ██ ███████

                                                                             version {Globals.CURRENT_VERSION}
            ");
        }

        static void PrintMenu() {
            if(Globals.preferences.SignedInBranch is null)
            {
                Console.WriteLine("\nYou are not currently signed to any sharebranch.");
            }
            else
            {
                Console.WriteLine($"\nSigned to {Globals.preferences.SignedInBranch.Creator}'s sharebranch.");
            }

            Console.WriteLine(@"
You can use the following commands:

  ===> run

---- Sharebranch related ----
    -> create
    -> pull
    -> push
    -> signIn

---- Sharebuddy related ----
    -> addBuddy
    -> checkBuddy

---- User related ----
    -> addUser
    -> authenticate

You can type 'help' after any command to get info about it. For example:
    -> create help
                ");
        }
    }

}
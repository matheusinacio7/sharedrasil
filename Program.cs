using System;
using System.IO;
using System.Threading.Tasks;

namespace sharedrasil
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DotEnv.LoadRoot();

            Console.WriteLine(@"
            ███████ ██   ██  █████  ██████  ███████ ██████  ██████   █████  ███████ ██ ██      
            ██      ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██ ██      ██ ██      
            ███████ ███████ ███████ ██████  █████   ██   ██ ██████  ███████ ███████ ██ ██      
                 ██ ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██      ██ ██ ██      
            ███████ ██   ██ ██   ██ ██   ██ ███████ ██████  ██   ██ ██   ██ ███████ ██ ███████ 
            ");

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

            User user = new User();
            await user.GetCredentials();
            Globals.currentUser = user;

            await CLIInterface.MainMenuLoop();            
        }
    }
}

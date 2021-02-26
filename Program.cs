using System;
using System.Threading.Tasks;

namespace sharedrasil
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(@"
            ███████ ██   ██  █████  ██████  ███████ ██████  ██████   █████  ███████ ██ ██      
            ██      ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██ ██      ██ ██      
            ███████ ███████ ███████ ██████  █████   ██   ██ ██████  ███████ ███████ ██ ██      
                 ██ ██   ██ ██   ██ ██   ██ ██      ██   ██ ██   ██ ██   ██      ██ ██ ██      
            ███████ ██   ██ ██   ██ ██   ██ ███████ ██████  ██   ██ ██   ██ ███████ ██ ███████ 
            ");

            LocalRepo localRepo = new LocalRepo();

            if(!localRepo.Exists) {
                Console.WriteLine("You have a local repository. Would you like to create one? (Y/N)");
                string createRepo_Q = Console.ReadLine().ToLower();

                if(createRepo_Q == "y" || createRepo_Q == "yes") {
                    localRepo.Create();
                } else {
                    Console.WriteLine("Unfortunately, Sharedrasil needs local repositories to work with.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey(true);
                    Environment.Exit(-1);
                }
            }

            User user = new User();
            user.GetCredentials();
            Globals.currentUser = user;

            await Github.GetAll();
        }
    }
}

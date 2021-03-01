using System;
using System.Threading.Tasks;

namespace sharedrasil {
    public static class CLIInterface {
        public static async Task MainMenuLoop() {
            do {
                Console.WriteLine(@"
You can use the following commands:
    -> create
    -> push

You can type help after any command to get info about the command. For example:
    -> create help
                ");
                
                CLICommand command = CLIParser.WaitForComand();

                switch(command.Command) {
                    case "create":
                        await CreateCommand(command.Arguments);
                        break;
                    case "push":
                        await PushCommand(command.Arguments);
                        break;
                    default:
                        Console.WriteLine("\nSharedrasil doesn't recognize the command you typed.");
                        break;
                }
            } while(true);
        }

        static async Task CreateCommand(string[] args) {
            await Github.Create();
        }

        static async Task PushCommand(string[] args) {
            Github.Push();
        }
    }
}
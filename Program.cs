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
            
            await CLIInterface.MainMenuLoop();            
        }
    }
}

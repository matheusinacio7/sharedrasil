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

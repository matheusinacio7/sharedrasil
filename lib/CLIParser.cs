using System;

namespace sharedrasil {
    public class CLICommand {
        public string Command {get; set;}
        public string[] Arguments {get; set;}
    }

    public static class CLIParser {
        public static string AskAnyString(string question) {
            Console.WriteLine(question);
            Boolean hasAsked = false;

            do {
                string answer = Console.ReadLine();

                if(!String.IsNullOrEmpty(answer)) {
                    return answer;
                } else if(!hasAsked) {
                    Console.WriteLine("Please, provide a non-empty answer, or enter another empty string to close the program.");
                    hasAsked = true;
                } else {
                    Environment.Exit(-1);
                } 
            } while(true);
        }

        public static Boolean AskYesOrNo(string question) {
            Boolean hasAsked = false;

            do {
                Console.WriteLine(question + " (Y/N)");
                string answer = Console.ReadLine().ToLower();;

                if(String.IsNullOrEmpty(answer)) {
                    if(!hasAsked) {
                        Console.WriteLine("Please, provide a non-empty answer, or enter another empty string to close the program.");
                        hasAsked = true; 
                    } else {
                        Environment.Exit(-1);
                    }
                } else if(answer == "y" || answer == "yes") {
                    return true;
                } else if(answer == "n" || answer == "no") {
                    return false;
                } else {
                    Console.Write("Please provide a yes or no answer.");
                }
            } while (true);
        }

        public static CLICommand WaitForComand() {
            Boolean hasAsked = false;

            do {
                string answer = Console.ReadLine();
                if(String.IsNullOrEmpty(answer))
                {
                    if (!hasAsked)
                    {
                        Console.WriteLine("Please, provide a non-empty answer, or enter another empty string to close the program.");
                        hasAsked = true;
                    }
                    else
                    {
                        Environment.Exit(-1);
                    }
                } else {
                    string[] words = answer.Split(' ');

                    string command = words[0];
                    string[] arguments = new string[words.Length - 1];
                    for(int i = 0; i < arguments.Length; i++) {
                        arguments[i] = words[i + 1];
                    }

                    return new CLICommand{
                        Command = command,
                        Arguments = arguments,
                    };
                }
            } while (true);
        }
    }
}
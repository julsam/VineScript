using System;
using System.IO;
using VineScriptLib;
using VineScriptLib.Core;

namespace VineScriptConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            VineStory story = new VineStory();
            try {
                if (args.Length <= 0)
                {
                    StreamReader br = new StreamReader(Console.OpenStandardInput());
                    string input = br.ReadLine();
                    while (!string.IsNullOrWhiteSpace(input))
                    {
                        story.Eval(input);
                        input = br.ReadLine();
                    }
                }
                else if (args.Length == 1)
                {
                    if (args[0] == "-h" || args[0] == "--help")
                    {
                        ShowUsage();
                    }
                    else
                    {
                        string inputFile = args[0];
                        StreamReader istream = File.OpenText(inputFile);
                        story.RunPassage(istream);
                    }
                }
                else
                {
                    ShowUsage();
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private static void ShowUsage()
        {
            string binname = System.AppDomain.CurrentDomain.FriendlyName;
            Console.WriteLine(
                string.Format("Usage: {0} [file]", binname)
            );
            Console.WriteLine(
                "With no file, read from the standard input in expressions mode only."
            );
            Console.WriteLine("-h, --help   : show this help message and exit");
            Console.WriteLine("file         : interpret the script file");
        }
    }
}

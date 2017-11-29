using System;
using System.IO;
using VineScript;
using VineScript.Core;

namespace VineScriptConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                if (args.Length <= 0)
                {
                    VineStory story = new VineStory();
                    StreamReader br = new StreamReader(Console.OpenStandardInput());
                    string input = br.ReadLine();
                    while (!string.IsNullOrWhiteSpace(input))
                    {
                        string output = story.Eval(input);
                        Console.WriteLine(output);
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
                        if (!File.Exists(args[0])) {
                            // Should we let File.GetAttributes handle it or handle it here?
                        }
                        Loader loader = new Loader();
                        FileAttributes attr = File.GetAttributes(args[0]);
                        if (attr.HasFlag(FileAttributes.Directory)) {
                            loader.LoadFromDir(args[0]);
                            // should specify a main file in arg[1] and run it
                            // then, either:
                            //  1. propose a choice between links if there's any
                            //  2. terminate the program
                            throw new NotImplementedException();
                        }
                        else {
                            // Load the given file and name the passage
                            loader.LoadFile(args[0], PassageScript.STDIN);
                        }

                        // Load story
                        VineStory story = new VineStory(loader);

                        // Run the passage loaded
                        PassageResult result = story.RunPassage(PassageScript.STDIN);

                        // Final output
                        Console.WriteLine("### FINAL OUTPUT: ###");
                        if (result.text.Length > 0)
                            Console.WriteLine(result.text);
                        Console.WriteLine("### END ###");
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

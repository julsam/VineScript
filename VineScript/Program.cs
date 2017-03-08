using System;
using System.IO;
using VineScriptLib;
using VineScriptLib.Core;

namespace VineScript
{
    class Program
    {
        static void Main(string[] args)
        {
            VineStory story = new VineStory();
            try {
                //string inputFile = "expressions/test01.expr";
                //string inputFile = "scripts/if.vine";

                //string inputFile = "scripts/print.vine";
                //story.vars["hello_world"] = "Hello, World!";
                //story.vars["my_int"] = 42;

                //string inputFile = "scripts/empty.vine";
                //string inputFile = "scripts/set01.vine";
                //string inputFile = "scripts/print02.vine";
                string inputFile = "scripts/if02.vine";
                //string inputFile = "scripts/line_ending01.vine";
                //string inputFile = "scripts/comments1.vine";
                //string inputFile = "scripts/comments2.vine";
                StreamReader istream = File.OpenText(inputFile);
                //StreamReader istream = new StreamReader(Console.OpenStandardInput());
                story.RunPassage(istream);
                //Calc.Evaluate("foobar(42)");
                //Calc.Evaluate("🍴_11\n_😎2\nπ33424\nµ\n∰\n日本語=42\n");
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            //try {
            //    //string inputFile = "expressions/test01.expr";
            //    string inputFile = "scripts/if.vine";
            //    //string inputFile = "scripts/print.vine";
            //    StreamReader istream = File.OpenText(inputFile);
            //    //StreamReader istream = new StreamReader(Console.OpenStandardInput());
            //    Interpreter.ExecuteEval(istream);
            //    //Calc.Evaluate("foobar(42)");
            //    //Calc.Evaluate("🍴_11\n_😎2\nπ33424\nµ\n∰\n日本語=42\n");
            //} catch (Exception e) {
            //    Console.WriteLine(e.Message);
            //}
        }
    }
}

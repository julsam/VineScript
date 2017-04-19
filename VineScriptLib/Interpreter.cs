using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScriptLib.Core;
using VineScriptLib.Compilers.LinesFormatter;
using VineScriptLib.Compilers.Vine;

namespace VineScriptLib
{
    public class Interpreter
    {                
        private VineStory story;

        public Interpreter(VineStory story)
        {
            this.story = story;
        }

        public string Evaluate(StreamReader istream)
        {
            return Evaluate(istream.ReadToEnd());
        }

        public string Evaluate(string expr)
        {
            // Print input
            Console.WriteLine(expr);

            // Remove whitespace at the start & end of each lines
            string wsRemoved = Compilers.Utils.RemoveWhiteSpace(expr);

            // Start timer
            var watch = System.Diagnostics.Stopwatch.StartNew();

            // Compile Vine code
            var vineCompiler = new VineCompiler();
            string vineOutput = vineCompiler.Parse(wsRemoved, story);

            // Formatting lines (removes empty lines containing Vine code)
            var formatCompiler = new LinesFormatterCompiler();
            string formatOutput = formatCompiler.FormatLines(wsRemoved, vineOutput);
            
            // Print before trimming whitespace
            Console.WriteLine(formatOutput);

            // Remove whitespace at the start & end of each lines (again)
            // TODO: keep only one space between words
            string finalOutput = Compilers.Utils.RemoveWhiteSpace(formatOutput);

            // Stop timer
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            
            // Finale output
            Console.WriteLine("### FORMATTED OUTPUT: ###");
            if (finalOutput.Length > 0)
                Console.WriteLine(finalOutput);
            Console.WriteLine("### END ###");

            // Timer output
            Console.WriteLine(string.Format("Time elapsed: {0} ms", elapsedMs.ToString("0.00")));

            return finalOutput;
        }
    }
}

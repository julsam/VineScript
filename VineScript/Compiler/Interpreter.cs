using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScript.Core;
using VineScript.Compiler.Formatter;

namespace VineScript.Compiler
{
    public class Interpreter
    {                
        private VineStory story;
        private VineCompiler vineCompiler;
        private VineFormatterCompiler formatCompiler;

        public Interpreter(VineStory story)
        {
            this.story = story;
            this.vineCompiler = new VineCompiler(story);
            this.formatCompiler = new VineFormatterCompiler();
        }

        public PassageResult Execute(PassageScript script)
        {
#if GRAMMAR_VERBOSE
            // Print input
            Console.WriteLine(script.SourceCode);
#endif
#if TIME_STATS
            // Start timer
            var watch = System.Diagnostics.Stopwatch.StartNew();
#endif

            // Compile Vine code
            PassageResult compiledResult = script.Run(story);

            // Formatting lines (removes empty lines containing Vine code)
            string formatOutput = formatCompiler.FormatLines(compiledResult.Text, script.Filename);
            
            // Post processing
            // Unescape user input
            string finalOutput = Escape.UnescapeVineSequence(formatOutput);
            // Remove whitespace at the start & end of each lines (again)
            finalOutput = WhiteSpace.Trim(finalOutput);
            // TODO add \r if on windows

            PassageResult finalResult = new PassageResult(script.Name, finalOutput, compiledResult.Links);

#if TIME_STATS
            // Stop timer
            watch.Stop();
            var elapsedMs = watch.Elapsed.TotalMilliseconds;
#endif
#if GRAMMAR_VERBOSE
            // Final output
            Console.WriteLine("### FORMATTED OUTPUT: ###");
            if (finalResult.Text.Length > 0)
                Console.WriteLine(finalResult.Text);
            Console.WriteLine("### END ###");
#endif
#if TIME_STATS
            // Timer output
            Console.WriteLine(string.Format(
                "Parsed & Executed in: {0} ms", elapsedMs.ToString("0.00")
            ));
#endif
            return finalResult;
        }

        public string Eval(StreamReader istream, string sourceName)
        {
            return Eval(istream.ReadToEnd(), sourceName);
        }

        public string Eval(string expr, string sourceName)
        {
#if GRAMMAR_VERBOSE
            // Print input
            Console.WriteLine(expr);
#endif
#if TIME_STATS
            // Start timer
            var watch = System.Diagnostics.Stopwatch.StartNew();
#endif

            // Remove whitespace at the start & end of each lines
            string wsRemoved = WhiteSpace.Trim(expr);

            // Compile Vine code
            string parsed = vineCompiler.Eval(wsRemoved, sourceName);
            
            // Remove whitespace at the start & end of each lines (again)
            // TODO: keep only one space between words
            string finalOutput = WhiteSpace.Trim(parsed);

#if TIME_STATS
            // Stop timer
            watch.Stop();
            var elapsedMs = watch.Elapsed.TotalMilliseconds;
            Console.WriteLine(string.Format("Time elapsed: {0} ms", elapsedMs.ToString("0.00")));
#endif

            return finalOutput;
        }
    }
}

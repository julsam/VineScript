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

        public PassageResult Execute(StreamReader istream, string sourceName)
        {
            return Execute(istream.ReadToEnd(), sourceName);
        }

        public PassageResult Execute(string vinecode, string sourceName)
        {
#if GRAMMAR_VERBOSE
            // Print input
            Console.WriteLine(vinecode);

            // Start timer
            var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
            // Pre processing
            // Remove whitespace at the start & end of each lines
            string wsRemoved = WhiteSpace.Trim(vinecode);

            // Compile Vine code
            PassageResult compiledResult = vineCompiler.Compile(wsRemoved, sourceName);

            // Formatting lines (removes empty lines containing Vine code)
            string formatOutput = formatCompiler.FormatLines(compiledResult.text, sourceName);
            
            // Post processing
            // Unescape user input
            string finalOutput = Escape.UnescapeVineSequence(formatOutput);
            // Remove whitespace at the start & end of each lines (again)
            // TODO: keep only one space between words
            finalOutput = WhiteSpace.Trim(finalOutput);

            PassageResult finalResult = new PassageResult(finalOutput, compiledResult.links);

#if GRAMMAR_VERBOSE
            // Stop timer
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            
            // Final output
            Console.WriteLine("### FORMATTED OUTPUT: ###");
            if (finalResult.text.Length > 0)
                Console.WriteLine(finalResult.text);
            Console.WriteLine("### END ###");

            // Timer output
            Console.WriteLine(string.Format("Time elapsed: {0} ms", elapsedMs.ToString("0.00")));
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

#if GRAMMAR_VERBOSE
            // Stop timer
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(string.Format("Time elapsed: {0} ms", elapsedMs.ToString("0.00")));
#endif

            return finalOutput;
        }
    }
}

using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScriptLib.Core;
using VineScriptLib.Compiler;

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

        static public string WhiteSpaceFormatter(string expr)
        {
            Console.WriteLine(expr);
            var input = new AntlrInputStream(expr);
            var lexer = new WhiteSpaceLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new WhiteSpaceParser(tokens);
            var tree = parser.compileUnit();

            //Console.WriteLine(tree.ToStringTree(parser));
            Console.WriteLine(PrettyPrint(tree.ToStringTree(parser)));
            
            var eval = new WhiteSpaceVisitor();
            eval.Visit(tree);
            eval.printOutput();

            return eval.output;
        }

        static public string LinesFormatter(string expr)
        {
            Console.WriteLine(expr);
            var input = new AntlrInputStream(expr);
            var lexer = new LinesFormatterLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new LinesFormatterParser(tokens);
            var tree = parser.compileUnit();

            //Console.WriteLine(tree.ToStringTree(parser));
            //Console.WriteLine(PrettyPrint(tree.ToStringTree(parser)));
            
            var eval = new LinesFormatterVisitor();
            eval.Visit(tree);
            //eval.printOutput();

            return eval.output;
        }

        static public HashSet<int> GetLinesToKeep(string expr)
        {
            var input = new AntlrInputStream(expr);
            var lexer = new LinesFormatterLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new LinesFormatterParser(tokens);
            var tree = parser.compileUnit();
            
            Console.WriteLine(PrettyPrint(tree.ToStringTree(parser)));
            
            var eval = new LinesFormatterVisitor();
            eval.Visit(tree);
            eval.printOutput();

            return eval.linesToKeep;
        }

        /// <summary>
        /// Removes '\n' in lines marked by LinesFormatter.
        /// Those lines corresponds to blocks of code that are now empty lines
        /// and shouldn't be displayed in the final output.
        /// </summary>
        /// <param name="input">User input to evaluate</param>
        /// <param name="interpreted">Interpreted input, only text should remains</param>
        /// <returns></returns>
        static public string FormatOutput(string input, string interpreted)
        {
            HashSet<int> markedLines = GetLinesToKeep(input);

            string output = "";
            if (interpreted.Length > 0) {
                string[] tmp = interpreted.Replace("\r", "").Split(new char[] { '\n' });
                for (int i = 0; i< tmp.Length; i++) {
                    output += tmp[i];
                    if (markedLines.Contains(i) && i < tmp.Length - 1) {
                        output += Environment.NewLine;
                    }
                }
            }
            return output;
        }

        public string Evaluate(string expr)
        {
            //return WhiteSpaceFormatter(expr);
            //var formatted = LinesFormatter(expr);
            Console.WriteLine(expr);
            Console.WriteLine("--- WS removed: ---");
            
            string wsRemoved = "";
            string tmp = expr.Replace("\r", "");
            string[] lines = tmp.Split(new char[] { '\n'});
            for (int i = 0; i < lines.Length; i++) {
                wsRemoved += lines[i].Trim(new char[] { '\t', ' ' });
                if (i < lines.Length - 1)
                    wsRemoved += Environment.NewLine;
            }
            Console.WriteLine(wsRemoved);
            Console.WriteLine("--- ---");

            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var inputStream = new AntlrInputStream(wsRemoved);
            var lexer = new VineLexer(inputStream);
            //lexer.RemoveErrorListeners();
            //lexer.AddErrorListener(new InterpreterErrorHandler());
            var tokens = new CommonTokenStream(lexer);
            var parser = new VineParser(tokens);
            var tree = parser.passage();

            //Console.WriteLine(tree.ToStringTree(parser));
            Console.WriteLine(PrettyPrint(tree.ToStringTree(parser)));
            
            var eval = new VineVisitor(story);
            eval.Visit(tree);
            eval.printOutput();
            string output = FormatOutput(expr, eval.output);
            Console.WriteLine("### FORMATTED OUTPUT: ###");
            if (output.Length > 0)
                Console.WriteLine(output);
            Console.WriteLine("### END ###");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(string.Format("Time elapsed: {0} ms", elapsedMs.ToString("0.00")));

            return output;
        }

        static public string PrettyPrint(string stringTree)
        {
            string output = "";
            string str = stringTree.Replace(" (", "\n(");
            str = str.Replace(")", ")\n");
            string[] lines = str.Split(new char[] { '\n'}, StringSplitOptions.RemoveEmptyEntries);
            int tab = -1;
            foreach (string line in lines) {
                if (line[0] == '(') {
                    tab++;
                }
                for (int i = 0; i < tab; i++) {
                    output += "  ";
                }
                output += line.Trim() + "\n";
                if (line[line.Length - 1] == ')') {
                    tab--;
                }
            }
            return output;
        }
    }
}

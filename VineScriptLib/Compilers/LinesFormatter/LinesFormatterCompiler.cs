using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScriptLib.Core;
using VineScriptLib.Compilers.Vine;

namespace VineScriptLib.Compilers.LinesFormatter
{
    public class LinesFormatterCompiler
    {
        private AntlrInputStream inputStream;
        private LinesFormatterLexer lexer;
        private CommonTokenStream tokens;
        private LinesFormatterParser parser;
        private LinesFormatterVisitor visitor;

        private void Setup(string text)
        {
            inputStream = new AntlrInputStream(text);
            lexer = new LinesFormatterLexer(inputStream);
            tokens = new CommonTokenStream(lexer);
            parser = new LinesFormatterParser(tokens);
        }

        private void Parse(string text)
        {
            Setup(text);

            var tree = parser.compileUnit();
            
            Console.WriteLine(Utils.PrettyPrint(tree.ToStringTree(parser)));
            
            visitor = new LinesFormatterVisitor();
            visitor.Visit(tree);
            visitor.printOutput();
        }

        /// <summary>
        /// Removes '\n' in lines corresponding to blocks of code 
        /// that are now empty lines and shouldn't be displayed 
        /// in the final output.
        /// </summary>
        /// <param name="source">Original and unterpreted source code</param>
        /// <param name="interpreted_code">Interpreted source, only text
        /// should remains, the code has already been interpreted</param>
        /// <returns>Text without the empty lines</returns>
        public string FormatLines(string source, string interpreted_code)
        {
            // First, parse source code to mark lines to keep
            Parse(source);

            // Get the lines to keep
            HashSet<int> markedLines = visitor.linesToKeep;

            string output = "";
            if (interpreted_code.Length > 0) {
                string[] tmp = interpreted_code.Replace("\r", "").Split(new char[] { '\n' });
                for (int i = 0; i< tmp.Length; i++) {
                    output += tmp[i];
                    if (markedLines.Contains(i) && i < tmp.Length - 1) {
                        output += Environment.NewLine;
                    }
                }
            }

            // In VineVisitor.VisitDisplay, user generated '\n' were replaced
            // by the character control '\u000B' in order to distinguish
            // between line returns presents originally in the source code
            // and the one added by displaying the return of a function
            // returning '\n'.
            // Formatting is done, now we can add back those user generated
            // line returns by replacing '\u000B' with '\n'
            output = output.Replace('\u000B', '\n');

            return output;
        }
    }
}

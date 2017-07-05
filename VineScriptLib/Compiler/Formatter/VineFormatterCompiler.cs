using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScriptLib.Core;

namespace VineScriptLib.Compiler.Formatter
{
    public class VineFormatterCompiler
    {
        private AntlrInputStream inputStream;
        private VineFormatterLexer lexer;
        private CommonTokenStream tokens;
        private VineFormatterParser parser;
        private VineFormatterVisitor visitor;

        private void Setup(string text)
        {
            inputStream = new AntlrInputStream(text);
            lexer = new VineFormatterLexer(inputStream);
            tokens = new CommonTokenStream(lexer);
            parser = new VineFormatterParser(tokens);
        }

        private string Parse(string text)
        {
            Setup(text);

            var tree = parser.passage();
            
            Console.WriteLine(Util.PrettyPrint(tree.ToStringTree(parser)));
            
            visitor = new VineFormatterVisitor();
            visitor.Visit(tree);
            visitor.printOutput();

            return visitor.output;
        }

        /// <summary>
        /// Parse interpreted code to remove code markups
        /// and unwanted line endings
        /// </summary>
        /// <param name="interpreted_code">
        /// Interpreted source code, only text and some markups should remains
        /// </param>
        /// <returns>Text without unwanted empty lines and code markups</returns>
        public string FormatLines(string interpreted_code)
        {
            // First, parse interpreted code to remove code markups
            // and unwanted line endings
            var parsed = Parse(interpreted_code);

            // In VineVisitor.VisitDisplay, user generated '\n' were replaced
            // by the character control '\u000B' in order to distinguish
            // between line returns presents originally in the source code
            // and the one added by displaying the return of a function
            // returning '\n'.
            // Formatting is done, now we can add back those user generated
            // line returns by replacing '\u000B' with '\n'

            var output = parsed.Replace('\u000B', '\n');

            return output;
        }
    }
}

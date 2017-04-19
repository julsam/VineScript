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
        /// Removes '\n' in lines marked by LinesFormatter.
        /// Those lines corresponds to blocks of code that are now empty lines
        /// and shouldn't be displayed in the final output.
        /// </summary>
        /// <param name="originText">User input to evaluate</param>
        /// <param name="vineParsedText">Interpreted input, only text should remains</param>
        /// <returns></returns>
        public string FormatLines(string originText, string vineInterpreted)
        {
            Parse(originText);

            HashSet<int> markedLines = visitor.linesToKeep;

            string output = "";
            if (vineInterpreted.Length > 0) {
                string[] tmp = vineInterpreted.Replace("\r", "").Split(new char[] { '\n' });
                for (int i = 0; i< tmp.Length; i++) {
                    output += tmp[i];
                    if (markedLines.Contains(i) && i < tmp.Length - 1) {
                        output += Environment.NewLine;
                    }
                }
            }
            return output;
        }
    }
}

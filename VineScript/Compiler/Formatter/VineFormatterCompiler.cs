using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScript.Core;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace VineScript.Compiler.Formatter
{
    public class VineFormatterCompiler
    {
        private AntlrInputStream inputStream;
        private VineFormatterLexer lexer;
        private CommonTokenStream tokens;
        private VineFormatterParser parser;
        private VineFormatterVisitor visitor;
        private ParserRuleContext tree;
        private bool parsed = false;
        private bool inited = false;

        private void Init(string vinecode, string sourceName)
        {
            if (!inited)
            {
                inputStream = new AntlrInputStream(vinecode);
                lexer = new VineFormatterLexer(inputStream);
                tokens = new CommonTokenStream(lexer);
                parser = new VineFormatterParser(tokens);
                inited = true;
            }
            else
            {
                inputStream = new AntlrInputStream(vinecode);
                inputStream.name = sourceName;
                lexer.SetInputStream(inputStream);
                tokens = new CommonTokenStream(lexer);
                parser.SetInputStream(tokens);
            }
            parsed = false;
        }

        private List<SyntaxErrorReport> Parse(string vinecode, string sourceName)
        {
            Init(vinecode, sourceName);

            // try with simpler/faster SLL(*)
            parser.Interpreter.PredictionMode = PredictionMode.Sll;

            // we don't want error messages or recovery during first try
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new BailErrorListener());
            parser.ErrorHandler = new BailErrorStrategy();
            
            try
            {
                tree = parser.passage();
                parsed = true;
                // if we get here, there was no syntax error and SLL(*) was enough;
                // there is no need to try full LL(*)
                return null;
            }
            catch (ParseCanceledException) // thrown by BailErrorStrategy
            {
                // rewind input stream
                tokens.Reset();
                parser.Reset();

                // back to standard handler
                //parser.AddErrorListener(ConsoleErrorListener<IToken>.Instance);
                parser.ErrorHandler = new DefaultErrorStrategy();

                // back to error reporting listener
                parser.RemoveErrorListeners();
                SyntaxCheckErrorListener syntaxCheckErrorListener = new SyntaxCheckErrorListener();
                parser.AddErrorListener(syntaxCheckErrorListener);

                // full now with full LL(*)
                parser.Interpreter.PredictionMode = PredictionMode.Ll;
                tree = parser.passage();
                parsed = true;
                
                // returns errors
                return syntaxCheckErrorListener.errorReports;
            }
            
        }

        /// <summary>
        /// Parse interpreted code to remove code markups
        /// and unwanted line endings
        /// </summary>
        /// <param name="interpreted_code">
        /// Interpreted source code, only text and some markups should remains
        /// </param>
        /// <param name="sourceName">
        /// Filename of the source being parsed
        /// </param>
        /// <returns>Text without unwanted empty lines and code markups</returns>
        public string FormatLines(string interpreted_code, string sourceName)
        {
            // First, parse interpreted code to remove code markups
            // and unwanted line endings
            var errorReports = Parse(interpreted_code, sourceName);

            // Check for errors only on debug. Error here should be extremely rare,
            // as most of them are catched by VineCompiler. Even in the case of an error,
            // it will produce a formatting error, which is not that critical.
            if (errorReports?.Count > 0) {
#if DEBUG
                throw new VineParseException(errorReports);
#else
                foreach (var report in errorReports) {
                    Console.WriteLine(report.FullMessage);
                }
#endif
            }

#if GRAMMAR_TREE || GRAMMAR_VERBOSE
            Console.WriteLine(Util.PrettyGrammarTree(tree.ToStringTree(parser)));
#endif

            visitor = new VineFormatterVisitor();
            visitor.Visit(tree);
            var parsed =  visitor.output;

#if GRAMMAR_VERBOSE
            visitor.printOutput();
#endif

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

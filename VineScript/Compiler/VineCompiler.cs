using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScript.Core;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace VineScript.Compiler
{
    public class VineCompiler
    {
        private AntlrInputStream inputStream;
        private VineLexer lexer;
        private CommonTokenStream tokens;
        private VineParser parser;
        private ParserRuleContext tree;
        private bool parsed = false;

        private void Setup(string vinecode)
        {
            inputStream = new AntlrInputStream(vinecode);
            lexer = new VineLexer(inputStream);
            tokens = new CommonTokenStream(lexer);
            parser = new VineParser(tokens);
        }

        public static List<SyntaxErrorReport> CheckSyntax(string vinecode)
        {
            VineCompiler compiler = new VineCompiler();
            compiler.Setup(vinecode);
            
            // Setup error listener
            compiler.parser.RemoveErrorListeners();
            SyntaxCheckErrorListener errorListener = new SyntaxCheckErrorListener();
            compiler.parser.AddErrorListener(errorListener);

            // Parse
            compiler.parser.BuildParseTree = false;
            compiler.parser.passage();

            return errorListener.errorReports;
        }

        public void ParseWithSyntaxCheck(string vinecode)
        {
            // Setup error listener
            parser.RemoveErrorListeners();
            SyntaxCheckErrorListener syntaxCheckErrorListener = new SyntaxCheckErrorListener();
            parser.AddErrorListener(syntaxCheckErrorListener);

            // Parse
            parser.BuildParseTree = true;
            tree = parser.passage();
            parsed = true;

            // Check for errors
            if (syntaxCheckErrorListener.errorReports.Count > 0) {
                throw new VineParseException(syntaxCheckErrorListener.errorReports);
            }
        }

        private List<SyntaxErrorReport> OptimizedParse(string vinecode)
        {
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
            catch (ParseCanceledException e) // thrown by BailErrorStrategy
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

        public void Parse(string vinecode)
        {
            // Setup error listener
            parser.RemoveErrorListeners(); // remove ConsoleErrorListener
            ParserErrorListener underlineErrorListener = new ParserErrorListener();
            parser.AddErrorListener(underlineErrorListener);

            // Parse
            parser.BuildParseTree = true;
            tree = parser.passage();
            parsed = true;
        }

        public PassageResult Compile(string vinecode, VineStory story, bool checkSyntax=true)
        {
            Setup(vinecode);

            ParseWithSyntaxCheck(vinecode);

#if GRAMMAR_TREE || GRAMMAR_VERBOSE
            Console.WriteLine(Util.PrettyGrammarTree(tree.ToStringTree(parser)));
#endif

            var eval = new VineVisitor(story);
            eval.Visit(tree);

#if GRAMMAR_VERBOSE
            eval.printOutput();
#endif
            return eval.passageResult;
        }

        public string Eval(string expr, VineStory story)
        {
            // TODO need a specific Error Listener for this mode?
            Setup(expr);

            // Go to "code" mode directly, we don't want to evaluate text
            lexer.PushMode(VineLexer.VineCode);

            // Use the 'Evaluate Expression' mode of the parser
            parser.ParseMode = VineParser.EVineParseMode.EVAL_EXPR;

            var tree = parser.passage();

#if GRAMMAR_TREE || GRAMMAR_VERBOSE
            Console.WriteLine(Util.PrettyGrammarTree(tree.ToStringTree(parser)));
#endif

            var eval = new VineVisitor(story);
            eval.Visit(tree);

#if GRAMMAR_VERBOSE
            eval.printOutput();
#endif

            return eval.passageResult.text;
        }
    }
}

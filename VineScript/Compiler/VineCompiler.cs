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
        private VineStory story;
        private AntlrInputStream inputStream;
        private VineLexer lexer;
        private CommonTokenStream tokens;
        private VineParser parser;
        private ParserRuleContext tree;
        private bool parsed = false;
        private bool inited = false;

        public VineCompiler(VineStory story)
        {
            this.story = story;
        }

        private void Init(string vinecode, string sourceName)
        {
            if (!inited)
            {
                inputStream = new AntlrInputStream(vinecode);
                inputStream.name = sourceName;
                lexer = new VineLexer(inputStream);
                tokens = new CommonTokenStream(lexer);
                parser = new VineParser(tokens);
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

        public List<SyntaxErrorReport> CheckSyntax(string vinecode, string sourceName)
        {
            Init(vinecode, sourceName);

            // Parse
            parser.BuildParseTree = false;
            var errorReports = Parse(vinecode);
            //compiler.parser.AddParseListener();

            return errorReports;
        }

        private List<SyntaxErrorReport> Parse(string vinecode)
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


        public PassageResult Compile(string vinecode, string sourceName, bool checkSyntax=true)
        {
            Init(vinecode, sourceName);
            
            parser.BuildParseTree = true;
            var errorReports = Parse(vinecode);

            // Check for errors
            //parser.NumberOfSyntaxErrors
            if (errorReports?.Count > 0) {
                throw new VineParseException(errorReports);
            }

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

        public string Eval(string expr, string sourceName)
        {
            // TODO need a specific Error Listener for this mode?
            Init(expr, sourceName);

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

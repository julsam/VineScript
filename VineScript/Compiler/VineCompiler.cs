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

        public void Init(string vinecode, string sourceName)
        {
#if TIME_STATS
            var initTimer = System.Diagnostics.Stopwatch.StartNew();
#endif

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
            
#if TIME_STATS
            initTimer.Stop();
            Console.WriteLine(string.Format(
                "Init in: {0} ms", initTimer.ElapsedMilliseconds.ToString("0.00")
            ));
#endif
        }

        public void ClearCache()
        {
            lexer?.Interpreter.ClearDFA();
            parser?.Interpreter.ClearDFA();
        }

        private List<SyntaxErrorReport> Parse()
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
            catch (ParseCanceledException) // thrown by BailErrorStrategy
            {
#if GRAMMAR_VERBOSE
                Console.WriteLine("*** VineCompiler: SSL(*) failed, trying with LL(*) ***");
#endif
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

        public List<SyntaxErrorReport> CheckSyntax(string vinecode, string sourceName)
        {
            Init(vinecode, sourceName);

            // Parse
            parser.BuildParseTree = false;
            var errorReports = Parse();
            //compiler.parser.AddParseListener();

            return errorReports;
        }

        public ParserRuleContext BuildTree()
        {
            // Parsing
#if TIME_STATS
            var parseTimer = System.Diagnostics.Stopwatch.StartNew();
#endif
            parser.BuildParseTree = true;
            var errorReports = Parse();
#if TIME_STATS
            parseTimer.Stop();
            Console.WriteLine(string.Format(
                "Parsed in: {0} ms", parseTimer.ElapsedMilliseconds.ToString("0.00")
            ));
#endif
            
            // Check for errors
            //parser.NumberOfSyntaxErrors
            if (errorReports?.Count > 0) {
                throw new VineParseException(errorReports);
            }

            return tree;
        }

        public PassageResult CompileTree(ParserRuleContext tree)
        {

#if GRAMMAR_TREE || GRAMMAR_VERBOSE
            Console.WriteLine(Util.PrettyGrammarTree(tree.ToStringTree(parser)));
#endif

#if TIME_STATS
            var evalTimer = System.Diagnostics.Stopwatch.StartNew();
#endif
            var eval = new VineVisitor(story);
            eval.Visit(tree);
#if TIME_STATS
            evalTimer.Stop();
            Console.WriteLine(string.Format(
                "Evaluated in: {0} ms", evalTimer.ElapsedMilliseconds.ToString("0.00")
            ));
#endif

#if GRAMMAR_VERBOSE
            eval.printOutput();
#endif
            return eval.passageResult;
        }

        public PassageResult Compile(string vinecode, string sourceName)
        {
            // Setup
            Init(vinecode, sourceName);

            // Tree
            BuildTree();

            return CompileTree(tree);
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

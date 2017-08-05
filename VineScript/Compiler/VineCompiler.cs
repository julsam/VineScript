using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScript.Core;

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

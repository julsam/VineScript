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
        private ParserErrorListener errorListener;
        private UnderlineErrorListener underlineListener;

        private void Setup(string text)
        {
            inputStream = new AntlrInputStream(text);
            lexer = new VineLexer(inputStream);
            tokens = new CommonTokenStream(lexer);
            parser = new VineParser(tokens);
            
            //errorListener = new ParserErrorListener();
            underlineListener = new UnderlineErrorListener();
            parser.RemoveErrorListeners(); // remove ConsoleErrorListener
            //parser.AddErrorListener(errorListener);
            parser.AddErrorListener(underlineListener);
        }

        public PassageResult Parse(string text, VineStory story)
        {
            Setup(text);

            var tree = parser.passage();

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

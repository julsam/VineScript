using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScriptLib.Core;
using VineScriptLib.Compilers;
using VineScriptLib.Compilers.Vine;

namespace VineScriptLib.Compilers.Vine
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

        public string Parse(string text, VineStory story)
        {
            Setup(text);

            var tree = parser.passage();
            
            Console.WriteLine(Utils.PrettyPrint(tree.ToStringTree(parser)));
            
            var eval = new VineVisitor(story);
            eval.Visit(tree);
            eval.printOutput();

            return eval.output;
        }

        public string Eval(string expr, VineStory story)
        {
            // TODO need a specific Error Listener for this mode?
            Setup(expr);

            // Go to "code" mode directly, we don't want to evaluate text
            lexer.PushMode(VineLexer.VineCode);

            // Use the 'Evaluate Expression' mode of the parser
            parser.ParseMode = VineParser.EVineParseMode.EXPR;

            var tree = parser.passage();
            
            Console.WriteLine(Utils.PrettyPrint(tree.ToStringTree(parser)));
            
            var eval = new VineVisitor(story);
            eval.Visit(tree);
            eval.printOutput();

            return eval.output;
        }
    }
}

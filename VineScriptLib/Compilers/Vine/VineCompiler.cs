﻿using System.IO;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using VineScriptLib.Core;
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

        private void Setup(string text)
        {
            inputStream = new AntlrInputStream(text);
            lexer = new VineLexer(inputStream);
            tokens = new CommonTokenStream(lexer);
            parser = new VineParser(tokens);

            errorListener = new ParserErrorListener();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);
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
    }
}
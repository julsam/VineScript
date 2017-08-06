using System;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    internal class BailErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int column, string errmsg, RecognitionException e)
        {
            throw new Antlr4.Runtime.Misc.ParseCanceledException(errmsg);
        }
    }
}

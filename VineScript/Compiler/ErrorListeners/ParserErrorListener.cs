using System;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    class ParserErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string errmsg = string.Format(
                "[Parser] Invalid Expression at {0}:{1}, {2}", 
                line, charPositionInLine, msg
            );
            throw new Exception(errmsg, e);
        }
    }
}

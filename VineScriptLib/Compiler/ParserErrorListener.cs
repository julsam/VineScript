using System;
using Antlr4.Runtime;

namespace VineScriptLib
{
    class ParserErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string m = string.Format("Invalid Expression at {0}:{1}, {2}", line, charPositionInLine, msg);
            throw new ArgumentException(m, msg, e);
        }
    }
}

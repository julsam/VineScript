using System;
using Antlr4.Runtime;

namespace VineScriptLib
{
    public class ParserErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string desc, RecognitionException e)
        {
            string msg = string.Format("[Parser] Invalid Expression at {0}:{1}, {2}", line, charPositionInLine, desc);
            throw new ArgumentException(msg, desc, e);
        }
    }
}

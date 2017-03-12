using System;
using Antlr4.Runtime;

namespace VineScriptLib
{
    class LexerErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        //IAntlrErrorListener<int> implementation
        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string m = string.Format("Invalid Expression at {0}:{1}, {2}", line, charPositionInLine, msg);
            throw new ArgumentException(m, msg, e);
        }
        
        //BaseErrorListener implementation
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string m = string.Format("Invalid Expression at {0}:{1}, {2}", line, charPositionInLine, msg);
            throw new ArgumentException(m, msg, e);
        }
    }
}

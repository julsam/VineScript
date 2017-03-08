using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace VineScriptLib
{
    class InterpreterErrorHandler : BaseErrorListener, IAntlrErrorListener<int>
    {
        //BaseErrorListener implementation
        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string aaa = string.Format("Invalid Expression at {0}:{1}, {2}", line, charPositionInLine, msg);
            throw new ArgumentException(aaa, msg, e);
        }
        
        //IAntlrErrorListener<int> implementation
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string aaa = string.Format("Invalid Expression at {0}:{1}, {2}", line, charPositionInLine, msg);
            throw new ArgumentException(aaa, msg, e);
        }
    }
}

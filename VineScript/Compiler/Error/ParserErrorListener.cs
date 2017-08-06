using System;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    internal class ParserErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int column, string errmsg, RecognitionException e)
        {
            var report = new SyntaxErrorReport(
                "<stdin>", recognizer, offendingSymbol, line, column, errmsg
            );
            throw new VineParseException(report);
        }
    }
}

using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    internal class SyntaxCheckErrorListener : BaseErrorListener
    {
        public List<SyntaxErrorReport> errorReports { get; } = new List<SyntaxErrorReport>();

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int column, string errmsg, RecognitionException e)
        {
            errorReports.Add(new SyntaxErrorReport(
                "<stdin>", recognizer, offendingSymbol, line, column, errmsg
            ));
        }
    }
}

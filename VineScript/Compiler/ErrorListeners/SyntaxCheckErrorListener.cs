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
            var underline = UnderlineErrorFormatter.Underline(
                recognizer, offendingSymbol, line, column
            );
            errmsg = Utils.UppercaseFirst(errmsg);
            var fullmsg = string.Format(
                "{0} in {1} at line {2}:{3}: {4}", 
                "Syntax error", "<stdin>", line, column, errmsg
            );
            errorReports.Add(new SyntaxErrorReport(
                "<stdin>", line, column, offendingSymbol, fullmsg, errmsg,
                underline
            ));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    public class VineParseException : Exception
    {
        public VineParseException(SyntaxErrorReport report)
            : base(ParserErrorFormatter.Format(report)) {}

        public VineParseException(List<SyntaxErrorReport> reports)
            : base(ParserErrorFormatter.Format(reports)) {}

        public VineParseException(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int column, string msg, RecognitionException e)
            : base(ParserErrorFormatter.Format(recognizer, offendingSymbol, line, column, msg), e)
        {}

    }
}

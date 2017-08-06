using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    public class VineParseException : Exception
    {
        public SyntaxErrorReport errorReport { get; }

        public VineParseException(SyntaxErrorReport report)
            : base(ParserErrorFormatter.Format(report))
        {
            errorReport = report;
        }

        public VineParseException(List<SyntaxErrorReport> reports)
            : base(ParserErrorFormatter.Format(reports))
        {
            if (reports.Count > 0) {
                errorReport = reports[0];
            }
        }
    }
}

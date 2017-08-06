using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    public class VineParseException : Exception
    {
        public SyntaxErrorReport errorReport {
            get {
                if (errorReports?.Count > 0) {
                    return errorReports[0];
                } else {
                    return null;
                }
            }
        }

        public List<SyntaxErrorReport> errorReports { get; } = new List<SyntaxErrorReport>();

        public VineParseException(SyntaxErrorReport report)
            : base(ParserErrorFormatter.Format(report))
        {
            errorReports.Add(report);
        }

        public VineParseException(List<SyntaxErrorReport> reports)
            : base(ParserErrorFormatter.Format(reports))
        {
            if (reports.Count > 0) {
                errorReports = reports.ConvertAll(val => val);
            }
        }
    }
}

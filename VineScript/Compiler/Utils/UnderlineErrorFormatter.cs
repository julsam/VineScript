using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    internal class UnderlineErrorFormatter
    {
        public static string Underline(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int column)
        {
            CommonTokenStream tokens = (CommonTokenStream)recognizer.InputStream;
            string input = tokens.TokenSource.InputStream.ToString();
            int start = offendingSymbol.StartIndex;
            int stop = offendingSymbol.StopIndex;
            
            return Underline(input, line, column, start, stop);
        }

        public static string Underline(string input, int line, 
            int column, int offendingSymbolStart, int offendingSymbolStop)
        {
            string[] lines = input.Split('\n');
            string errorLine = lines[line - 1];
            string underline = errorLine + "\n";
            for (int i = 0; i < column; i++) {
                if (errorLine[i] == '\t') {
                    underline += "\t";
                } else {
                    underline += " ";
                }
            }

            // underline the error
            if (offendingSymbolStart >= 0 && offendingSymbolStop >= 0) {
                for (int i = offendingSymbolStart; i <= offendingSymbolStop; i++) {
                    underline += "^";
                }
            }

            return underline;
        }
    }

    internal class RuntimeErrorFormatter : UnderlineErrorFormatter
    {
        public static string Format(Type cls, string msg,
            ParserRuleContext ctx)
        {
            int line = ctx.Start.Line;
            int column = ctx.Start.Column;
            string input = ctx.Start.InputStream.ToString();
            int start = ctx.Start.StartIndex;
            int stop = ctx.Stop.StopIndex;

            string underline = Underline(
                input, line, column, start, stop
            );
            
            // TODO if the input is a file, show the filename in the error msg.
            // instead of <stdin>. Could use the Passage's name too, if they
            // ever get one
            return string.Format(
                "{0}: {1}\n  File \"{2}\", in line {3}:{4} at '{5}':\n{6}", 
                cls.Name, msg, "<stdin>", line, column, ctx.GetText(), underline
            );
        }
    }

    internal class ParserErrorFormatter : UnderlineErrorFormatter
    {
        public static string Format(IRecognizer recognizer, IToken offendingSymbol,
            int line, int column, string errmsg)
        {
            CommonTokenStream tokens = (CommonTokenStream)recognizer.InputStream;
            string input = tokens.TokenSource.InputStream.ToString();
            int start = offendingSymbol.StartIndex;
            int stop = offendingSymbol.StopIndex;

            string underline = Underline(
                input, line, column, start, stop
            );
            return Format(line, column, offendingSymbol, underline, errmsg);
        }

        public static string Format(List<SyntaxErrorReport> reports)
        {
            if (reports.Count <= 0) {
                return "";
            }

            StringBuilder errorListing = new StringBuilder();
            if (reports.Count > 1) {
                for (int i = 1; i < reports.Count; i++) {
                    errorListing.Append(" * " + reports[i].FullMessage);
                    if (i < reports.Count - 1) {
                        errorListing.Append("\n");
                    }
                }
            }
            return Format(reports[0].Line, reports[0].Column,
                reports[0].OffendingSymbol, reports[0].Underline,
                reports[0].ErrorMessage + "\n\n" + errorListing.ToString()
            );

        }

        public static string Format(SyntaxErrorReport report)
        {
            return Format(report.Line, report.Column,
                report.OffendingSymbol, report.Underline, report.ErrorMessage
            );
        }

        public static string Format(int line, int column,
            IToken offendingSymbol, string underline, string errmsg)
        {
            return string.Format(
                "[Parser] Invalid Expression '{0}' at line {1}:{2}:\n{3}\n{4}",
                Compiler.Util.EscapeWhiteSpace(offendingSymbol.Text),
                line, column, underline, errmsg
            );
        }
    }
}

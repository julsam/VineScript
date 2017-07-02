using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace VineScriptLib.Compilers
{
    class ExceptionUtil
    {
        public static string UnderlineError(Type cls, string msg, ParserRuleContext ctx)
        {
            int line = ctx.Start.Line;
            int column = ctx.Start.Column;
            string input = ctx.Start.InputStream.ToString();
            string[] lines = input.Split('\n');
            string errorLine = lines[line - 1];
            string underline = errorLine + "\n";
            for (int i = 0; i < column; i++) {
                underline += " ";
            }
            int start = ctx.Start.StartIndex;
            int stop = ctx.Stop.StopIndex;

            if (start >= 0 && stop >= 0) {
                for (int i = start; i <= stop; i++) {
                    underline += "^";
                }
            }
            // TODO if the input is a file, show the filename in the error msg.
            // instead of <stdin>. Could use the Passage's name too, if they
            // ever get one
            return string.Format(
                "{0}: {1}\n  File \"{2}\", in line {3}:{4} at '{5}':\n{6}", 
                cls.Name, msg, "<stdin>", line, column, ctx.GetText(), underline
            );
        }
    }
}

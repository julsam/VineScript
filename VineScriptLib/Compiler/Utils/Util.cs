using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScriptLib.Compiler
{
    public class Util
    {
        public static string PrettyPrint(string stringTree)
        {
            string output = "";
            string str = stringTree.Replace(" (", "\n(");
            str = str.Replace(")", ")\n");
            string[] lines = str.Split(new char[] { '\n'}, StringSplitOptions.RemoveEmptyEntries);
            int tab = -1;
            foreach (string line in lines) {
                if (line[0] == '(') {
                    tab++;
                }
                for (int i = 0; i < tab; i++) {
                    output += "  ";
                }
                output += line.Trim() + "\n";
                if (line[line.Length - 1] == ')') {
                    tab--;
                }
            }
            return output;
        }

        public static string RemoveWhiteSpace(string text)
        {
            Console.WriteLine("--- WS removed: ---");
            string wsRemoved = "";
            string tmp = text.Replace("\r", "");
            string[] lines = tmp.Split(new char[] { '\n'});
            for (int i = 0; i < lines.Length; i++) {
                // removes starting & ending whitespaces
                wsRemoved += lines[i].Trim(new char[] { '\t', ' ' });
                if (i < lines.Length - 1) {
                    // add new line if it's not the last one
                    wsRemoved += Environment.NewLine;
                }
            }
            Console.WriteLine(wsRemoved);
            Console.WriteLine("--- ---");
            return wsRemoved;
        }

        public static string ToLiteral(string input)
        {
            StringBuilder literal = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append(@"\"""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default: literal.Append(c); break;
                }
            }
            return literal.ToString();
        }
    }
}

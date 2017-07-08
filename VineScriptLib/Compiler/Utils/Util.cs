using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string Escape(string input)
        {
            StringBuilder literal = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append(@"\"""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break; // Null (0x00)
                    case '\a': literal.Append(@"\a"); break; // Bell (0x07)
                    case '\b': literal.Append(@"\b"); break; // Backspace (0x08)
                    case '\f': literal.Append(@"\f"); break; // From Feed (0x0C)
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break; // Vertical Tab, used internally (0x0B)
                    default: literal.Append(c); break;
                }
            }
            return literal.ToString();
        }

        public static string UnescapeAuthorizedChars(string input)
        {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            var unescaped = Regex.Replace(input, @"\\.", m => {
                switch (m.Value)
                {
                    case @"\'": return "\'";
                    case @"\""": return "\"";
                    case @"\\": return "\\";
                    case @"\n": return "\n";
                    case @"\t": return "\t";
                    default:
                        throw new Exception("Unrecognized escape sequence '" + m.Value + "'");
                }
            });
            return unescaped;
        }
    }
}

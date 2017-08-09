using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VineScript.Compiler
{
    public class Util
    {
#if GRAMMAR_TREE
        internal static string PrettyGrammarTree(string stringTree)
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
#endif
    }
}
